using AGD.Repositories.ConfigurationModels;
using AGD.Repositories.Helpers;
using AGD.Service.Services.Implement;
using AGD.Service.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace AGD.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtSettings!.Key);
            var securityKey = new SymmetricSecurityKey(keyBytes);

            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            return services;
        }

        public static IServiceCollection AddRedisAndServices(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConfiguration = configuration.GetSection("Redis")["Configuration"];
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfiguration!));
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITokenBlacklistService, RedisTokenBlacklistService>();
            return services;
        }
    }
}
