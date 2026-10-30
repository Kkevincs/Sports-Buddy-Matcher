
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SportsBuddyMatcher.Security;
using SportsBuddyMatcher.Repository.Models;
using SportsBuddyMatcher.BusinessLogic.Interfaces;
using SportsBuddyMatcher.BusinessLogic.Mappings;
using SportsBuddyMatcher.BusinessLogic.Services;
using SportsBuddyMatcher.Repository;
using SportsBuddyMatcher.Repository.Interfaces;
using SportsBuddyMatcher.Repository.Repositories;
using SportsBuddyMatcher.Infrastructure;

namespace SportsBuddyMatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            DotNetEnv.Env.NoClobber().Load(Path.Combine(builder.Environment.ContentRootPath, ".env"));
            builder.Configuration.AddEnvironmentVariables();

            var jwtSecret = builder.Configuration["JWT_SECRET"]
                ?? throw new InvalidOperationException("JWT_SECRET must be set in .env.");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                        ClockSkew = TimeSpan.Zero
                    };
                });
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => int.TryParse(context.User.FindFirst("sub")?.Value, out var id) && id > 0)
                    .Build();
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddDbContext<SportsBuddyMatcherDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddAutoMapper(config =>
            {
                config.AddProfile<MappingProfile>();
                config.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
            });
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ILocationService, LocationService>();
            builder.Services.AddHttpClient<IGeocodingService, GeoapifyGeocodingService>(client =>
            {
                client.BaseAddress = new Uri("https://api.geoapify.com/v1/geocode/");
                client.Timeout = TimeSpan.FromSeconds(10);
            }).RemoveAllLoggers();
            builder.Services.AddScoped<ISkillService, SkillService>();
            builder.Services.AddScoped<IPlayerSkillService, PlayerSkillService>();
            builder.Services.AddScoped<IPlayerSkillRepository, PlayerSkillRepository>();
            builder.Services.AddScoped<IMatchingService, MatchingService>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IUserEventService, UserEventService>();
            builder.Services.AddScoped<IUserEventRepository, UserEventRepository>();
            builder.Services.AddScoped<IConnectionRepository, ConnectionRepository>();
            builder.Services.AddScoped<IConnectionService, ConnectionService>();
            builder.Services.AddScoped<IPhotoStorage, LocalPhotoStorage>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    }] = Array.Empty<string>()
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                await next();
            });
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
