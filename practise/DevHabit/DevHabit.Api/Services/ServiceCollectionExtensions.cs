using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace DevHabit.Api.Services;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSwaggerGen(o =>
        {
            // Unikatni ID-jevi za modele (izbegava kolizije kad ima nested tipova)
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            // Dodaj osnovni JWT Bearer scheme
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });

            // Dodaj OAuth2 (Keycloak)
            o.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(configuration["Keycloak:AuthorizationUrl"]!),
                        TokenUrl = new Uri(configuration["Keycloak:TokenUrl"]!),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect" },
                            { "profile", "User profile" }
                        }
                    }
                },
                Description = "OAuth2 integration with Keycloak"
            });

            // Security zahtevi
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Keycloak",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string> { "openid", "profile" }
                }
            });

            // Dodaj opis API-ja (verzionisanje)
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DevHabit API",
                Version = "v1",
                Description = "API za praćenje i upravljanje navikama"
            });

            // Uključi XML komentare (iz .csproj)
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                o.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}
