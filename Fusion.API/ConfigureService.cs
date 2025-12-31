using FluentValidation;
using FluentValidation.AspNetCore;
using Fusion.API.Auth;
using Fusion.API.Context;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.AITaskGenerate;
using Fusion.Service.ViewModels.Users.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
            services.AddScoped<IPermissionCache, NoopPermissionCache>();
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddHttpContextAccessor();
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
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
                 })
                 .AddJsonOptions(options =>
                 {
                     options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                 }); ;
            // --- FluentValidation (vẫn enable) ---
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.Configure<AiTaskGenerationOptions>(
            configuration.GetSection("AiTaskGeneration"));

            // SignalR
            services.AddSignalR();
            return services;
        }  
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

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            // chỉ áp dụng cho hub chat
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        //Cấu hình Swagger với JWT Bearer
        public static void AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fusion API",
                    Version = "v1",
                    Description = "API documentation with JWT"
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer' [space] and then your valid JWT token.",
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
        public static void ConfigCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy.WithOrigins(
                                       "http://localhost:5173",
                                       "https://localhost:5173",
                                       "http://localhost:19006",
                                       "https://www.fusion.info.vn",
                                       "https://fusion.info.vn"
                                          )
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
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

