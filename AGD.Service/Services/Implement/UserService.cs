using AGD.Repositories.ConfigurationModels;
using AGD.Repositories.Repositories;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using AGD.Repositories.Models;
using BCrypt.Net;

namespace AGD.Service.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GoogleIdTokenOptions _options;
        public UserService(IUnitOfWork unitOfWork, IOptions<GoogleIdTokenOptions> options)
        {
            _unitOfWork = unitOfWork;
            _options = options.Value;
        }

        public async Task<GoogleIdLoginResponse> LoginByGoogleWithIdTokenAsync(GoogleIdLoginRequest request, CancellationToken ct = default)
        {
            if (_options.ClientIds == null || !_options.ClientIds.Any())
            {
                throw new InvalidOperationException("Google Client IDs are not configured.");
            }

            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = _options.ClientIds
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);

            if (!string.Equals(payload.Issuer, "https://accounts.google.com", StringComparison.Ordinal) &&
                !string.Equals(payload.Issuer, "accounts.google.com", StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("Invalid token issuer.");
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
                throw new UnauthorizedAccessException("Google account has no email.");

            if (_options.RequireEmailVerified && !payload.EmailVerified)
                throw new UnauthorizedAccessException("Email not verified by Google.");

            // trong trường hợo nghiệp vụ có limit domain email thì bật đống này lên
            //if (_options.AllowedHostedDomains?.Count > 0)
            //{
            //    var hd = payload.HostedDomain;
            //    var ok = !string.IsNullOrWhiteSpace(hd) &&
            //             _options.AllowedHostedDomains.Contains(hd, StringComparer.OrdinalIgnoreCase);
            //    if (!ok) throw new UnauthorizedAccessException("Email domain is not allowed.");
            //}

            var googleId = payload.Subject.Trim();
            var email = payload.Email.Trim().ToLowerInvariant();

            var user = await _unitOfWork.UserRepository.GetByGoogleIdAsync(googleId, ct);
            var isNew = false;

            if (user is null)
            {
                var byEmail = await _unitOfWork.UserRepository.GetByEmailAsync(email, ct);
                if (byEmail != null)
                {
                    user = byEmail;
                    if (payload.EmailVerified)
                    {
                        user.GoogleId = googleId;
                        if (!user.IsEmailVerified)
                        {
                            user.IsEmailVerified = true;
                            user.EmailVerifiedAt = DateTime.Now;
                        }
                        await _unitOfWork.UserRepository.UpdateAsync(user, ct);
                        await _unitOfWork.SaveChangesAsync(ct);
                    }
                }
                if (!_options.AutoCreateUser)
                {
                    throw new UnauthorizedAccessException("No user associated with this Google account.");
                }
                //trong db là user name có thể trùng nhưng mà mấy đứa new user thì sẽ là NewUser1 2 3 để phân biệt nhau chứ mà NewUser hết
                // thì như 1 rừng ẩn danh đứa nào cũng New User thì biết ai là ai, giống facebook nó Ẩn danh 1 2 3 thì mình y chang :))
                var username = await GenerateUniqueUsernameAsync(email, ct);
                
                var now = DateTime.Now;

                //password không đăng nhập được, để thỏa not null thôi
                var randomPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"));

                user = new User
                {
                    Username = username,
                    Email = email,
                    IsEmailVerified = payload.EmailVerified,
                    EmailVerifiedAt = payload.EmailVerified ? now : null,

                    PhoneNumber = string.Empty,
                    IsPhoneVerified = false,
                    PhoneVerifiedAt = null,

                    PasswordHash = randomPassword,
                    RoleId = _options.DefaultRoleId,

                    FullName = !string.IsNullOrWhiteSpace(payload.Name) ? payload.Name : username,
                    AvatarUrl = payload.Picture,
                    Gender = _options.DefaultGender,
                    DateOfBirth = null,

                    CreatedAt = now,
                    IsDeleted = false,

                    GoogleId = googleId,
                    Status = UserStatus.active
                };

                var pictureUrl = string.IsNullOrWhiteSpace(payload.Picture) ? null : payload.Picture;
                if (!string.IsNullOrEmpty(pictureUrl) && pictureUrl.Contains("s96-c"))
                {
                    pictureUrl = pictureUrl.Replace("s96-c", "s256-c");
                }
                user.AvatarUrl = pictureUrl;

                await _unitOfWork.UserRepository.CreateAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                isNew = true;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(user.GoogleId))
                {
                    if (payload.EmailVerified)
                    {
                        user.GoogleId = googleId;

                        if (!user.IsEmailVerified)
                        {
                            user.Email = email;
                            user.IsEmailVerified = true;
                            user.EmailVerifiedAt = DateTime.Now;
                        }

                        await _unitOfWork.UserRepository.UpdateAsync(user, ct);
                        await _unitOfWork.SaveChangesAsync(ct);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Please verify your Google email, or login with password then link Google.");
                    }
                }
                else if (!string.Equals(user.GoogleId, googleId, StringComparison.Ordinal))
                {
                    // trường hợp này hiếm khi xảy ra, trừ khi có lỗi hệ thống
                    throw new UnauthorizedAccessException("Google ID does not match the linked account.");
                }
            }

            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = email;
                await _unitOfWork.UserRepository.UpdateAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            if (user.Status != UserStatus.active)
            {
                throw new UnauthorizedAccessException("User account is not active or banned.");
            }

            var jwt = _unitOfWork.JwtHelper.GenerateToken(user);
            return new GoogleIdLoginResponse
            {
                AccessToken = jwt,
                TokenType = "Bearer",
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsNewUser = isNew
            };
        }

        public async Task<LoginUserNameResponse> LoginWithUsernameAsync(LoginUserNameRequest request, CancellationToken ct = default)
        {
            var user = await _unitOfWork.UserRepository.GetByUsernameAsync(request.username, ct);

            if (user == null) throw new UnauthorizedAccessException("User account is not existed.");

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.password, user.PasswordHash);

            if (!isValidPassword)
            {
                throw new UnauthorizedAccessException("Invalid Password");
            }

            if (user.Status != UserStatus.active) throw new UnauthorizedAccessException("User account is not active or banned.");

            var jwt = _unitOfWork.JwtHelper.GenerateToken(user);

            return new LoginUserNameResponse 
            {
                AccessToken = jwt,
                TokenType = "Bearer",
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                RoleId = user.RoleId,
                Email = user.Email,
            };
        }

        public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken ct = default)
        {
            var existedUsername = await _unitOfWork.UserRepository.GetByUsernameAsync(request.Username, ct);

            if (existedUsername != null) throw new InvalidOperationException("Username already exists.");

            var existedEmail = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email, ct);

            if(existedEmail != null) throw new InvalidOperationException("Email already exists.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                //PhoneNumber = request.PhoneNumber,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                AvatarUrl = request.AvatarUrl,
                RoleId = 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            };

            await _unitOfWork.UserRepository.CreateAsync(newUser, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new RegisterUserResponse
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                IsEmailVerified = newUser.IsEmailVerified,
                FullName = newUser.FullName,
                //PhoneNumber = newUser.PhoneNumber,
                Gender = newUser.Gender,
                DateOfBirth = newUser.DateOfBirth,
                AvatarUrl = newUser.AvatarUrl,
                CreatedAt = newUser.CreatedAt ?? DateTime.Now
            };
        }

        private async Task<string> GenerateUniqueUsernameAsync(string email, CancellationToken ct)
        {
            var local = email.Split('@')[0].ToLowerInvariant();

            var cleaned = System.Text.RegularExpressions.Regex.Replace(local, "[^a-z0-9_\\-\\.]", "");
            if (string.IsNullOrWhiteSpace(cleaned)) cleaned = "user";

            var candidate = cleaned;
            var i = 0;

            while (await _unitOfWork.UserRepository.GetByUsernameAsync(candidate, ct) != null)
            {
                i++;
                candidate = $"{cleaned}{i}";
            }
            return candidate;
        }
    }
}
