using API.Template.Infrastructure.Configuration.Adapters;
using API.Template.Infrastructure.Configuration.Extensions;
using API.Template.Infrastructure.Configuration.Options;
using API.Template.Infrastructure.DI;
using API.Template.Application;
using Azure.Identity;
using Infrastructure.Persistence.Configuration;

var builder = WebApplication.CreateBuilder(args);

// User Secrets — only meaningful for "Local", called here (not inside
// AddAppKeyVault) because it needs API's own Program type to find this
// project's UserSecretsId.
if (builder.Environment.IsEnvironment("Local"))
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

// Config SOURCES — order-sensitive, must run before options binding.
builder.Configuration.AddAppKeyVault(builder.Environment);
builder.Configuration.AddDatabaseSettings();

builder.Services.AddAllApplicationServices();                          // Application: MediatR
builder.Services.AddAllConfigurationServices(builder.Configuration);   // Infra: keys/settings
//builder.Services.AddInfrastructureServices(builder.Configuration);     // Infra: db/email/blob — depends on line above


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
