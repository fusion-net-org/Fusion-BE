using FluentValidation;
using Fusion.API;
using Fusion.API.Auth;
using Fusion.Repository;
using Fusion.Service;
using System.Net;
using Travelogue.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
/*builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Member.AssignRole", p => p.RequireAssertion(ctx =>
    {
        // Ở Presentation bạn có thể đọc CompanyContext từ middleware
        // ví dụ minh hoạ: cho phép nếu có claim "perm:Member.AssignRole"
        return ctx.User.HasClaim("perm", "Member.AssignRole") || ctx.User.IsInRole("SystemAdmin");
    }));
});*/

#region Custom application service configuration

System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

/*builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();*/

builder.Services.ConfigureRepositoryLayerService(builder.Configuration);
builder.Services.ConfigureServiceLayerService(builder.Configuration);
builder.Services.ConfigureApiLayerServices(builder.Configuration);

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});


#endregion End of custom application service configuration
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle



var app = builder.Build();
app.UseMiddleware<CompanyContextMiddleware>();
app.UseMiddleware<CustomExceptionHandlerMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
