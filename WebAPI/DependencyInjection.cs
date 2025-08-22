using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebAPI.Data;
using WebAPI.Mapping;
using WebAPI.Repositories.Implements;
using WebAPI.Repositories.Interfaces;
using WebAPI.Services.Implements;
using WebAPI.Services.Interfaces;
using WebAPI.Utils.Query;

namespace WebAPI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<ISpecialtyService, SpecialtyService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IMedicineService, MedicineService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();

            return services;
        }

        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
            });

            return services;
        }

        public static IServiceCollection AddAutoMapperService(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var jwtIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var jwtAudience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddControllersWithFilters(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<QueryOptionsFilter>();
            });

            services.AddEndpointsApiExplorer();

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Skibidi Hopita API",
                    Version = "v1",
                    Description = "Hospital Management System API with SOLID principles and clean architecture"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            return services;
        }

        public static IServiceCollection AddAllServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration)
                   .AddRepositories()
                   .AddBusinessServices()
                   .AddCaching(configuration)
                   .AddAutoMapperService()
                   .AddJwtAuthentication(configuration)
                   .AddControllersWithFilters()
                   .AddSwaggerDocumentation()
                   .AddCorsPolicy();

            return services;
        }
    }
}
