using FirebaseAdmin;
using FluentValidation;
using Fusion.API;
using Fusion.API.Auth;
using Fusion.API.Background.Jobs;
using Fusion.API.Middlewares;
using Fusion.Repository;
using Fusion.Service;
using Fusion.Service.Commons.Background;
using Fusion.Service.Commons.BaseResponses;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Ai code mobile thi mo cai nay de xai
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(5191); // HTTP

//});

var isCi = builder.Configuration.GetValue<bool>("CI"); // GitHub Actions tự set CI=true
if (isCi)
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
}
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

builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = "localhost:6379";
    o.InstanceName = "Fusion_";
});


builder.Services.AddHealthChecks();
if (!isCi)
{
    var firebaseConfig = builder.Configuration.GetSection("FireBase").Get<Dictionary<string, object>>();
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(firebaseConfig)),
    });
}

#region Custom application service configuration


builder.Services.ConfigureRepositoryLayerService(builder.Configuration);
builder.Services.ConfigureServiceLayerService(builder.Configuration);
builder.Services.ConfigureApiLayerServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Background service
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<BackgroundJobRunner>();
    builder.Services.AddSingleton<IBackgroundJob, AutoMonthlyEntitlementResetJob>();
    builder.Services.AddSingleton<IBackgroundJob, SubscriptionStatusMonitorJob>();
}

#endregion End of custom application service configuration
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var app = builder.Build();
app.MapHealthChecks("/health");
app.UseMiddleware<CustomExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();
if (!isCi)
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseMiddleware<CompanyContextMiddleware>();
app.UseAuthorization();




app.MapControllers();

app.Run();
