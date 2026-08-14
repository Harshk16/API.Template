using API.Template.Application;
using API.Template.Identity;
using API.Template.Infrastructure.Configuration.Adapters;
using API.Template.Infrastructure.Configuration.Extensions;
using API.Template.Infrastructure.Configuration.Options;
using API.Template.Infrastructure.DI;
using API.Template.WebApi.Objects;
using Azure.Identity;
using Infrastructure.Persistence.Configuration;
using Microsoft.AspNetCore.Mvc.Authorization;

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
builder.Services.AddInfrastructureServices(builder.Configuration);     // Infra: db/email/blob — depends on line above

builder.Services.AddIdentityServices(builder.Configuration, builder.Environment); // ← added builder.Environment

// builder.Services.AddHttpContextAccessor(); // confirmed redundant — AddIdentity already registers this internally; left commented, not deleted, for clarity
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();


builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});
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
app.UseHsts();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
