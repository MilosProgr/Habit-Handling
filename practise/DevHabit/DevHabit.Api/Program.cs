using DevHabit.Api;
using DevHabit.Api.Configurations;
using DevHabit.Api.Extensions;
using DevHabit.Api.Middleware;
//using DevHabit.Api.Settings;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddBackgroundJobs()
    //.AddControllers()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices()
    .AddAuthenticationServices()
    .AddCorsPolicy()
    .AddHealthChecks()
    .AddRateLimiting();
    

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    //app.UseSwaggerUI(options =>
    //{
    //    //options.SwaggerEndpoint("/openapi/v1.json", "v1");
    //    Console.WriteLine("Swagger je upaljen");
    //});
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DevHabit API vs1");
        //options.RoutePrefix = "swagger";
        // Integracija sa Keycloak loginom
        options.OAuthClientId("swagger-ui"); // client_id koji si registrovao u Keycloak-u
        options.OAuthUsePkce();              // Preporuka za sigurnost
        options.OAuthScopeSeparator(" ");
        options.OAuthAppName("DevHabit Swagger UI");

        options.OAuth2RedirectUrl("http://localhost:5000/swagger/oauth2-redirect.html");

    });

    app.MapHealthChecks("/health"); // <-- This exposes the health check endpoint
    app.MapHealthChecksUI();
    //app.UseSwaggerUI();

    await app.ApplyMigrationsAsync();

    //await app.SendInitialDataAsync();

}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors(CorsOptions.PolicyName);

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.UseEtagCaching();

//app.UseMiddleware<ETagMiddleware>();

app.MapControllers();

await app.RunAsync();
