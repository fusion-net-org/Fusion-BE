using FluentValidation;
using Fusion.API;
using Fusion.API.Auth;
using Fusion.API.Middlewares;
using Fusion.Repository;
using Fusion.Service;
using Fusion.Service.Commons.BaseResponses;
using Microsoft.Extensions.Configuration;
using System.Net;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
/*   builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Member.AssignRole", p => p.RequireAssertion(ctx =>
    {
        // Ở Presentation bạn có thể đọc CompanyContext từ middleware
        // ví dụ minh hoạ: cho phép nếu có claim "perm:Member.AssignRole"
        return ctx.User.HasClaim("perm", "Member.AssignRole") || ctx.User.IsInRole("SystemAdmin");
    }));
});*/
builder.Services.AddMemoryCache();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "Fusion_";
});

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

#region Custom application service configuration

System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

builder.Services.ConfigureRepositoryLayerService(builder.Configuration);
builder.Services.ConfigureServiceLayerService(builder.Configuration);
builder.Services.ConfigureApiLayerServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));


#endregion End of custom application service configuration
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var app = builder.Build();
app.UseMiddleware<CustomExceptionHandlerMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fusion API v1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<CompanyContextMiddleware>();
app.UseAuthorization();

app.UseHttpsRedirection();



app.MapControllers();

app.Run();
