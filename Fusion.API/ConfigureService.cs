using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Fusion.API.Auth;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.ViewModels.Users.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Fusion.API
{
    public static class ConfigureService
    {
        public static IServiceCollection ConfigureApiLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // --- JWT / Swagger / CORS ---
            services.AddAuthenJwt(configuration);
            services.AddSwaggerWithJwt();
            services.ConfigCors();
            services.AddScoped<IPermissionQuery, PermissionQuery>();

            // --- Controllers 一 Validation Response --
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var errors = context.ModelState
                            .Where(kvp => kvp.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            );

                        var response = ResponseModel<object>.Error(
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Validation failed",
                            additionalData: errors
                        );

                        return new BadRequestObjectResult(response);
                    };
                });
            // --- FluentValidation ---
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
            services.AddFluentValidationAutoValidation();
            // 3. Client-side adapters if needed
            services.AddFluentValidationClientsideAdapters();
            services.AddFluentValidationClientsideAdapters();

            return services;
        }
        public static void ConfigCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();   
                });
            });
        }

        // set up JWT authentication
        public static void AddAuthenJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JWT");

            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("JWT: key is missing in configuration");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                        ClockSkew = TimeSpan.Zero
                    };
                });
        }

        // Set up Swagger to use JWT authentication
        public static void AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                // Thông tin API
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fusion API",
                    Version = "v1",
                    Description = "API documentation with JWT authentication"
                });

                // Cấu hình JWT Bearer
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer' [space] and then your valid JWT token.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    securityScheme,
                    Array.Empty<string>()
                }
            };

                c.AddSecurityRequirement(securityRequirement);
            });
    }
        public class CamelCaseParameterTransformer : IOutboundParameterTransformer
        {
            public string? TransformOutbound(object? value)
            {
                if (value == null) return null;

                // Convert to camelCase
                string input = value.ToString()!;
                return Regex.Replace(input, "(?<!^)([A-Z])", "-$1").ToLowerInvariant();
            }
        }
    }
}
