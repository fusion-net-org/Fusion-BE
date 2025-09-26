using Fusion.API;
using Fusion.API.Middlewares;
using Fusion.Repository;
using Fusion.Service;
using System.Net;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Custom application service configuration

System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

builder.Services.ConfigureRepositoryLayerService(builder.Configuration);
builder.Services.ConfigureServiceLayerService(builder.Configuration);
builder.Services.ConfigureApiLayerServices(builder.Configuration);


//builder.Services.Configure<RouteOptions>(options =>
//{
//    options.LowercaseUrls = true;
//});

#endregion End of custom application service configuration
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var app = builder.Build();

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
