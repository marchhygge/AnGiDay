using AGD.API.Extensions;
using AGD.API.Middlewares;
using AGD.Repositories.ConfigurationModels;
using AGD.Repositories.DBContext;
using AGD.Repositories.Enums;
using AGD.Repositories.Helpers;
using AGD.Repositories.Models;
using AGD.Repositories.Repositories;
using AGD.Service.Integrations;
using AGD.Service.Integrations.Implements;
using AGD.Service.Integrations.Interfaces;
using AGD.Service.Mapping;
using AGD.Service.Services.BackgroundServices;
using AGD.Service.Services.Implement;
using AGD.Service.Services.Interfaces;
using AGD.Service.Services.Retrieval;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
{
    var missing = new List<string>();
    if (string.IsNullOrEmpty(jwtSection["Key"])) missing.Add("JwtSettings:Key (env JwtSettings__Key)");
    if (string.IsNullOrEmpty(jwtSection["Issuer"])) missing.Add("JwtSettings:Issuer (env JwtSettings__Issuer)");
    if (string.IsNullOrEmpty(jwtSection["Audience"])) missing.Add("JwtSettings:Audience (env JwtSettings__Audience)");
    Console.WriteLine("ERROR: Missing JWT settings: " + string.Join(", ", missing));
    throw new Exception("Invalid JWT settings in configuration. Missing: " + string.Join(", ", missing));
}
builder.Services.AddSingleton<JwtSettings>(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<GoogleIdTokenOptions>(builder.Configuration.GetSection("GoogleIdToken"));
builder.Services.Configure<R2Options>(builder.Configuration.GetSection("R2"));

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
var vectorCs = builder.Configuration.GetConnectionString("EmbeddingConnection");
var dsb = new NpgsqlDataSourceBuilder(cs);
var vectorDsb = new NpgsqlDataSourceBuilder(vectorCs);
dsb.MapEnum<UserStatus>("user_status"); // hoặc "public.user_status"
dsb.MapEnum<NotificationType>("notification_type");
dsb.MapEnum<PaymentProvider>("payment_provider");
dsb.MapEnum<PaymentStatus>("payment_status");
dsb.MapEnum<LedgerEntryType>("ledger_entry_type");
var dataSource = dsb.Build();
var vectorDs = vectorDsb.Build();

static IEdmModel GetEdmModel()
{
    var odataBuilder = new ODataConventionModelBuilder();
    var restaurants = odataBuilder.EntitySet<Restaurant>("Restaurants");
    odataBuilder.EntitySet<SignatureFood>("SignatureFoods");
    odataBuilder.EntitySet<Post>("Posts");
    //khai báo navigation
    restaurants.EntityType.HasMany(r => r.SignatureFoods);
    return odataBuilder.GetEdmModel();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<IWeatherProvider, OpenMeteoWeatherProvider>();
builder.Services.AddSingleton<IObjectStorageService, R2StorageService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IServicesProvider, ServicesProvider>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRestaurantRetrieval, RestaurantRetrieval>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IUserTagService, UserTagService>();
builder.Services.AddScoped<IPostService, PostService>();

builder.Services.AddScoped<VectorRetrievalService>();
builder.Services.AddScoped<ContextBuilder>();

builder.Services.AddHostedService<EmbeddingIngestWorker>();

builder.Services.AddScoped<IObjectStorageService, R2StorageService>();
//Connect DB
var enableSensitiveDataLogging = builder.Configuration.GetValue<bool>("Db:EnableSensitiveDataLogging", false);
builder.Services.AddDbContext<AnGiDayContext>(options =>
{
    options.UseNpgsql(dataSource);
    options.EnableDetailedErrors();
    if (enableSensitiveDataLogging)
    {
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddDbContext<AnGiDayVectorContext>(options =>
{
    options.UseNpgsql(vectorDs);
    options.EnableDetailedErrors();
    if (enableSensitiveDataLogging)
    {
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddHttpClient<OllamaClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"]!);
});
builder.Services.AddHttpClient<OllamaEmbeddingClient>(c => c.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"]!));
builder.Services.AddHttpClient<OpenMeteoWeatherProvider>();

builder.Services.AddControllers().AddOData(options =>
{
    options.Select().Filter().OrderBy().Expand().SetMaxTop(100).Count();
    options.AddRouteComponents("odata", GetEdmModel());
});

var redisConn = builder.Configuration.GetSection("Redis")?.GetValue<string>("Configuration");
if (!string.IsNullOrWhiteSpace(redisConn))
{
    Console.WriteLine($"Using Redis: {redisConn.Substring(0, Math.Min(50, redisConn.Length))}...");
    if (redisConn.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase))
    {
        var cfgOpts = StackExchange.Redis.ConfigurationOptions.Parse(redisConn);
        cfgOpts.Ssl = true;
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = cfgOpts;
        });
    }
    else
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConn;
        });
    }
}
else
{
    Console.WriteLine("Redis not configured. Using in-memory cache fallback.");
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
    options.SwaggerDoc(
         "v1",
         new OpenApiInfo
         {
             Title = "AnGiDay API - V1",
             Version = "v1"
         }
    );
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
});

//Mapper
//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidAudience = jwtSettings!.Audience,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings!.Key)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = "sub"
                };
            });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("4"));
    options.AddPolicy("RequireEmployeeRole", policy => policy.RequireRole("3"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("2"));
    options.AddPolicy("RequireOwnerRole", policy => policy.RequireRole("1"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedisAndServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AnGiDay v1");
    });
}

app.UseRouting();
app.UseHttpsRedirection();

app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");
app.MapControllers();

app.Run();

