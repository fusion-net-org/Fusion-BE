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



var isCi = builder.Configuration.GetValue<bool>("CI"); // GitHub Actions tự set CI=true

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

if (!isCi)
{
    builder.Services.AddStackExchangeRedisCache(o =>
    {
        o.Configuration = "localhost:6379";
        o.InstanceName = "Fusion_";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddHealthChecks();

#region Custom application service configuration


builder.Services.ConfigureRepositoryLayerService(builder.Configuration);
builder.Services.ConfigureServiceLayerService(builder.Configuration);
builder.Services.ConfigureApiLayerServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));


#endregion End of custom application service configuration
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var app = builder.Build();
app.MapHealthChecks("/health");
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
if (!isCi)
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseMiddleware<CompanyContextMiddleware>();
app.UseAuthorization();




app.MapControllers();

app.Run();
