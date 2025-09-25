using AGD.Repositories.ConfigurationModels;
using AGD.Repositories.Models;
using AGD.Repositories.Repositories;
using AGD.Service.DTOs.Request;
using AGD.Service.DTOs.Response;
using AGD.Service.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AGD.Service.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GoogleIdTokenOptions _options;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;
        private readonly IObjectStorageService _objectStorageService;
        public UserService(IUnitOfWork unitOfWork, 
                           IOptions<GoogleIdTokenOptions> options, 
                           IEmailService emailService, 
                           IOptions<JwtSettings> jwtSettings, 
                           IObjectStorageService objectStorageService)
        {
            _unitOfWork = unitOfWork;
            _options = options.Value;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
            _objectStorageService = objectStorageService;
        }

        private static DateTime UtcNowUnspecified() =>
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        public async Task<UserProfileResponse?> GetUserProfileAsync(int userId, CancellationToken ct = default)
        {
            var user = await _unitOfWork.UserRepository.GetUserAsync(userId, ct);
            if (user is null)
            {
                return null;
            }

            var (postCount, restaurantBookmarkCount, postBookmarkCount, ownedRestaurantCount) = await _unitOfWork
                                                                .UserRepository.GetUserAggregatesAsync(userId, ct);
            var userProfile = new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Gender = user.Gender,
                Email = user.Email,
                IsEmailVerified = user.IsEmailVerified,
                AvatarUrl = user.AvatarUrl,
                PhoneNumber = user.PhoneNumber ?? "",
                IsPhoneVerified = user.IsPhoneVerified,
                RoleId = user.RoleId,
                DateOfBirth = user.DateOfBirth,
                TotalPosts = postCount,
                TotalPostBookmarks = postBookmarkCount,
                TotalRestaurantBookmarks = restaurantBookmarkCount,
                TotalRestaurantsOwned = ownedRestaurantCount,
                CreatedAt = user.CreatedAt,
                Status = user.Status.ToString()
            };

            userProfile.IsProfileComplete = !string.IsNullOrWhiteSpace(userProfile.Email)
                                        && !string.IsNullOrWhiteSpace(userProfile.PhoneNumber)
                                        && !string.IsNullOrWhiteSpace(userProfile.AvatarUrl);

            return userProfile;
        }

        public async Task<GoogleIdLoginResponse> LoginByGoogleWithIdTokenAsync(GoogleIdLoginRequest request, CancellationToken ct = default)
        {
            if (_options.ClientIds == null || !_options.ClientIds.Any())
            {
                throw new InvalidOperationException("Google Client IDs are not configured.");
            }

            var rawAud = TryReadAudienceUnverified(request.IdToken);
            if (rawAud != null && !_options.ClientIds.Contains(rawAud, StringComparer.Ordinal))
            {
                // give explicit info (you may hide details in Production)
                var expected = string.Join(", ", _options.ClientIds);
                throw new Google.Apis.Auth.InvalidJwtException($"Untrusted audience '{rawAud}'. Expected one of: {expected}");
            }

            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = _options.ClientIds
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                // line 88 (wrapped with try-catch to enrich diagnostics)
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, validationSettings);
            }
            catch (Google.Apis.Auth.InvalidJwtException ex)
            {
                // Append aud if we extracted it
                if (rawAud != null && !ex.Message.Contains(rawAud, StringComparison.Ordinal))
                {
                    throw new Google.Apis.Auth.InvalidJwtException($"{ex.Message} (aud='{rawAud}')");
                }
                throw;
            }

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
                            user.EmailVerifiedAt = UtcNowUnspecified();
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
                
                var now = UtcNowUnspecified();

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
                user.AvatarUrl = pictureUrl ?? "uploads/anonymous.jpg";

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
                            user.EmailVerifiedAt = UtcNowUnspecified();
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

        private static string? TryReadAudienceUnverified(string jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt)) return null;
            var parts = jwt.Split('.');
            if (parts.Length != 3) return null;
            try
            {
                static byte[] Base64UrlDecode(string input)
                {
                    input = input.Replace('-', '+').Replace('_', '/');
                    switch (input.Length % 4)
                    {
                        case 2: input += "=="; break;
                        case 3: input += "="; break;
                    }
                    return Convert.FromBase64String(input);
                }
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                using var doc = JsonDocument.Parse(payloadJson);
                if (doc.RootElement.TryGetProperty("aud", out var audProp))
                {
                    return audProp.ValueKind switch
                    {
                        JsonValueKind.String => audProp.GetString(),
                        JsonValueKind.Array when audProp.GetArrayLength() > 0 => audProp[0].GetString(),
                        _ => null
                    };
                }
            }
            catch
            {
                // ignore decoding errors
            }
            return null;
        }

        private ClaimsPrincipal? ValidateEmailVerificationToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        private async Task SendEmailVerificationAsync(User user, CancellationToken ct)
        {
            var token = GenerateEmailVerificationToken(user);
            var verifyLink = $"{_jwtSettings.Audience?.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(token)}";

            var subject = "Xác minh địa chỉ email của bạn";
            var body = $@"
<p>Xin chào {System.Net.WebUtility.HtmlEncode(user.FullName ?? user.Username)},</p>
<p>Bạn đã yêu cầu cập nhật hoặc xác minh email cho tài khoản AnGiDay.</p>
<p>Nhấn vào liên kết sau để xác minh: <a href=""{verifyLink}"">Xác minh ở đây</a></p>
<p>Liên kết sẽ hết hạn sau 24 giờ.</p>
<p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
<hr/>
<p>AnGiDay Team</p>";

            await _emailService.SendMailAsync(user.Email, subject, body);
        }

        private string GenerateEmailVerificationToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new("purpose", "verify_email")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public async Task<UserProfileResponse?> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request, CancellationToken ct = default)
        {
            var user = await _unitOfWork.UserRepository.GetUserAsync(userId, ct);
            if (user is null) return null;

            bool changed = false;
            bool emailChanged = false;

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var u = request.Username.Trim();
                if (u.Length is < 3 or > 50)
                    throw new InvalidOperationException("Username length must be between 3 and 50 characters.");
                if (!Regex.IsMatch(u, "^[a-zA-Z0-9_.-]+$"))
                    throw new InvalidOperationException("Username contains invalid characters.");
                if (!string.Equals(u, user.Username, StringComparison.Ordinal))
                {
                    var exists = await _unitOfWork.UserRepository.GetByUsernameAsync(u, ct);
                    if (exists != null && exists.Id != user.Id)
                        throw new InvalidOperationException("Username already in use.");
                    user.Username = u;
                    changed = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.FullName) &&
                !string.Equals(request.FullName, user.FullName, StringComparison.Ordinal))
            {
                user.FullName = request.FullName.Trim();
                changed = true;
            }

            if (request.PhoneNumber != null)
            {
                var raw = request.PhoneNumber.Trim();
                if (raw.Length == 0)
                {
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        user.PhoneNumber = string.Empty;
                        user.IsPhoneVerified = false;
                        user.PhoneVerifiedAt = null;
                        changed = true;
                    }
                }
                else
                {
                    var phone = Regex.Replace(raw, "\\s+", "");
                    if (!string.Equals(phone, user.PhoneNumber, StringComparison.Ordinal))
                    {
                        user.PhoneNumber = phone;
                        user.IsPhoneVerified = false;
                        user.PhoneVerifiedAt = null;
                        changed = true;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var normalized = request.Email.Trim().ToLowerInvariant();
                if (!string.Equals(normalized, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    if (!Regex.IsMatch(normalized, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        throw new InvalidOperationException("Email format invalid.");
                    var emailOwner = await _unitOfWork.UserRepository.GetByEmailAsync(normalized, ct);
                    if (emailOwner != null && emailOwner.Id != user.Id)
                        throw new InvalidOperationException("Email already in use.");
                    user.Email = normalized;
                    if (user.IsEmailVerified)
                    {
                        user.IsEmailVerified = false;
                        user.EmailVerifiedAt = null;
                    }
                    emailChanged = true;
                    changed = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Gender))
            {
                var gender = request.Gender.Trim();
                if (!string.Equals(gender, user.Gender, StringComparison.OrdinalIgnoreCase))
                {
                    user.Gender = gender;
                    changed = true;
                }
            }

            if (request.DateOfBirth.HasValue)
            {
                var dob = request.DateOfBirth.Value;
                if (dob > DateOnly.FromDateTime(DateTime.UtcNow))
                    throw new InvalidOperationException("Date of birth cannot be in the future.");
                if (user.DateOfBirth != dob)
                {
                    user.DateOfBirth = dob;
                    changed = true;
                }
            }

            if (changed)
            {
                await _unitOfWork.UserRepository.UpdateAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            if (emailChanged)
            {
                await SendEmailVerificationAsync(user, ct);
            }

            return await GetUserProfileAsync(userId, ct);
        }

        public async Task<UserProfileResponse?> UpdateUserAvatarAsync(int userId, IFormFile avatarFile, CancellationToken ct = default)
        {
            if (avatarFile == null || avatarFile.Length == 0)
                throw new InvalidOperationException("Avatar file is empty.");

            const long maxBytes = 5 * 1024 * 1024; // 5MB
            if (avatarFile.Length > maxBytes)
                throw new InvalidOperationException("Avatar file too large (max 5MB).");

            var allowed = new[] { "image/png", "image/jpeg", "image/jpg", "image/webp" };
            if (!allowed.Contains(avatarFile.ContentType, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Unsupported image format.");

            var user = await _unitOfWork.UserRepository.GetUserAsync(userId, ct);
            if (user is null) return null;

            var ext = Path.GetExtension(avatarFile.FileName);
            if (string.IsNullOrWhiteSpace(ext))
            {
                ext = avatarFile.ContentType switch
                {
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => ".jpg"
                };
            }
            var key = $"uploads/avatars/{userId}_{Guid.NewGuid():N}{ext}";

            await using (var stream = avatarFile.OpenReadStream())
            {
                await _objectStorageService.UploadAsync(key, stream, avatarFile.ContentType, ct);
            }

            var old = user.AvatarUrl;

            try
            {
                await using (var stream = avatarFile.OpenReadStream())
                {
                    await _objectStorageService.UploadAsync(key, stream, avatarFile.ContentType, ct);
                }
            }
            catch
            {
                throw;
            }

            user.AvatarUrl = key;
            await _unitOfWork.UserRepository.UpdateAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            if (!string.IsNullOrWhiteSpace(old) &&
                old.StartsWith($"uploads/avatars/", StringComparison.OrdinalIgnoreCase) &&
                !old.EndsWith("anonymous.jpg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(old, key, StringComparison.OrdinalIgnoreCase))
            {
                try { await _objectStorageService.DeleteAsync(old, ct); } catch { /* ignore */ }
            }

            return await GetUserProfileAsync(userId, ct);
        }

        public async Task TriggerEmailVerificationAsync(int userId, CancellationToken ct = default)
        {
            var user = await _unitOfWork.UserRepository.GetUserAsync(userId, ct);
            if (user == null) throw new InvalidOperationException("User not found.");
            if (user.IsEmailVerified) return;
            await SendEmailVerificationAsync(user, ct);
        }

        public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default)
        {
            var principal = ValidateEmailVerificationToken(token);
            if (principal == null) return false;

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var purpose = principal.FindFirst("purpose")?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (purpose != "verify_email" ||
                string.IsNullOrWhiteSpace(userIdStr) ||
                string.IsNullOrWhiteSpace(email) ||
                !int.TryParse(userIdStr, out var userId))
            {
                return false;
            }

            var user = await _unitOfWork.UserRepository.GetUserAsync(userId, ct);
            if (user == null) return false;
            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)) return false;

            if (!user.IsEmailVerified)
            {
                user.IsEmailVerified = true;
                user.EmailVerifiedAt = UtcNowUnspecified();
                await _unitOfWork.UserRepository.UpdateAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            return true;
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
