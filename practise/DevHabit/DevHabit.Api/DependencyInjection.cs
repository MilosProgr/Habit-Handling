using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Asp.Versioning;
using DevHabit.Api.Common.Hateoas;
using DevHabit.Api.Common.Sorting;
using DevHabit.Api.Common.Telemetry;
using DevHabit.Api.Configurations;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Generics;
using DevHabit.Api.Jobs.EntryImport;
using DevHabit.Api.Jobs.GitHub;
using DevHabit.Api.Middleware;
using DevHabit.Api.Services;
using DevHabit.Api.Services.GitHub;
using DevHabit.Api.Services.Habit;

//using DevHabit.Api.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IO;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Refit;


namespace DevHabit.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;
        })
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver())
            .AddXmlSerializerFormatters();

        builder.Services.Configure<MvcOptions>(options =>
        {
            NewtonsoftJsonOutputFormatter formatter = options.OutputFormatters
                .OfType<NewtonsoftJsonOutputFormatter>()
                .First();

            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
        });

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector = new DefaultApiVersionSelector(options);

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new MediaTypeApiVersionReader(),
                    new MediaTypeApiVersionReaderBuilder()
                        .Template("application/vnd.dev-habit.hateoas.{version}+json")
                        .Build());
            })
            .AddMvc();
      

        builder.Services.AddOpenApi();

        builder.Services.AddResponseCaching();

        return builder;
    }

    //public static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    //{
    //    builder.Services.AddControllers(options =>
    //    {
    //        options.ReturnHttpNotAcceptable = true;
    //    })
    //        .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver =
    //            new CamelCasePropertyNamesContractResolver())
    //        .AddXmlSerializerFormatters();
    //    builder.Services.Configure<MvcOptions>(options =>
    //    {
    //        NewtonsoftJsonOutputFormatter formatter = options.OutputFormatters
    //        .OfType<NewtonsoftJsonOutputFormatter>()
    //        .First();
    //        formatter.SupportedMediaTypes.Add(Application.HateoasJson);
    //    });
    //    builder.Services.AddOpenApi();

    //    return builder;
    //}

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
                .UseSnakeCaseNamingConvention());
        builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
            options
                .UseNpgsql(
                    builder.Configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                .UseSnakeCaseNamingConvention());

        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddSource("DevHabit.Tracing")
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddNpgsql())
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation())
            .UseOtlpExporter();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
        });

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddValidatorsFromAssemblyContaining<IApiMarker>();

        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<LinkService>();

        builder.Services.AddScoped<TokenProvider>();

        builder.Services.AddScoped<UserContext>();

        builder.Services.AddScoped<GitHubService>();

        builder.Services.AddSingleton<DevHabitMetrics>();

        //builder.Services.AddScoped<DevHabitMetrics>();

        builder.Services.AddScoped<RefitGitHubService>();

        builder.Services.AddScoped<GitHubAccessTokenService>();

        builder.Services.AddHttpClient()
            .ConfigureHttpClientDefaults(options => options.AddStandardResilienceHandler());

        builder.Services.AddHttpClient("github", client =>
        {
            client.BaseAddress = new(builder.Configuration.GetValue<string>("GitHub:BaseUrl")!);

            client.DefaultRequestHeaders
                .UserAgent.Add(new("DevHabit", "1.0"));

            client.DefaultRequestHeaders
                .Accept.Add(new("application/vnd.github+json"));
        });

        builder.Services.AddRefitClient<IGitHubApi>(new()
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer(new()
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy(),
                },
            }),
        })
        .ConfigureHttpClient(client =>
        {
            client.BaseAddress = new(builder.Configuration.GetValue<string>("GitHub:BaseUrl")!);
        });

        builder.Services.Configure<Configurations.EncryptionOptions>(
            builder.Configuration.GetSection("Encryption"));

        builder.Services.AddSingleton<EncryptionService>();

        builder.Services.AddSingleton<RecyclableMemoryStreamManager>();

        //builder.Services.AddSingleton<InMemoryETagStore>();

        builder.Services.AddScoped<IHabitService,HabitService>();

        builder.Services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var connStr = builder.Configuration.GetConnectionString("Database");
            return NpgsqlDataSource.Create(connStr!);
        });

        builder.Services.AddSingleton<ICacheService, PostgresCacheService>();


        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        //builder.Services
        //    .AddIdentity<IdentityUser, IdentityRole>()
        //    .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        //builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));

        //Configurations.JwtAuthOptions jwtAuthOptions =  builder.Configuration.GetSection("Jwt").Get<Configurations.JwtAuthOptions>()!;

        builder.Services
            .AddAuthentication(options => 
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                //options.TokenValidationParameters = new TokenValidationParameters
                //{
                //    ValidIssuer = jwtAuthOptions.Issuer,
                //    ValidAudience = jwtAuthOptions.Audience,
                //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
                //};
                options.RequireHttpsMetadata = false; // true u produkciji!
                options.Authority = builder.Configuration["Authentication:Authority"];
                options.Audience = builder.Configuration["Authentication:Audience"];
                options.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"]!;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Authentication:Audience"],
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                    //ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
                    ////RoleClaimType = ClaimTypes.Role,
                    //NameClaimType = "sub",
                    //RoleClaimType = "roles",// 👈 kažemo da gledamo Role claim
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal!.Identity as ClaimsIdentity;

                        // Dodajemo role iz resource_access
                        var resourceAccess = context.Principal.FindFirst("resource_access");
                        if (resourceAccess != null)
                        {
                            var json = JsonDocument.Parse(resourceAccess.Value);
                            if (json.RootElement.TryGetProperty("public-client", out var client) &&
                                client.TryGetProperty("roles", out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    identity!.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                                }
                            }
                        }

                        // Dodajemo role iz realm_access
                        var realmAccess = context.Principal.FindFirst("realm_access");
                        if (realmAccess != null)
                        {
                            var json = JsonDocument.Parse(realmAccess.Value);
                            if (json.RootElement.TryGetProperty("roles", out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    identity!.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                                }
                            }
                        }
                        if(identity != null && !identity.HasClaim(c => c.Type == "sub"))
                        {
                            var subClaim = context.Principal.FindFirst("sub");
                            if (subClaim != null)
                            {
                                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        builder.Services.AddAuthorization();
        builder.Services
           .AddSwaggerGenWithAuth(builder.Configuration);

        return builder;
    }

    public static WebApplicationBuilder AddBackgroundJobs(this WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(configurator =>
        {
            // GitHub automation scheduler
            configurator.AddJob<GitHubAutomationSchedulerJob>(options => options.WithIdentity("github-automation-scheduler"));

            configurator.AddTrigger(options =>
            {
                options.ForJob("github-automation-scheduler")
                    .WithIdentity("github-automation-scheduler-trigger")
                    .WithSimpleSchedule(scheduleBuilder =>
                    {
                        GitHubAutomationOptions settings = builder.Configuration
                            .GetSection(GitHubAutomationOptions.SectionName)
                            .Get<GitHubAutomationOptions>()!;

                        scheduleBuilder.WithIntervalInMinutes(settings.ScanIntervalInMinutes)
                            .RepeatForever();
                    });
            });

            // Entry import clean up - runs daily at 3 AM UTC
            configurator.AddJob<CleanUpEntryImportJob>(options => options.WithIdentity("cleanup-entry-imports"));

            configurator.AddTrigger(options =>
            {
                options.ForJob("cleanup-entry-imports")
                    .WithIdentity("cleanup-entry-imports-trigger")
                    .WithCronSchedule("0 0 3 * * ?", x => x.InTimeZone(TimeZoneInfo.Utc));
            });
        });

        builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return builder;
    }

    public static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        //CorsOptions corsOptions = builder.Configuration
        //    .GetSection(CorsOptions.SectionName)
        //    .Get<CorsOptions>()!;
        //var corsOptions = builder.Configuration.GetSection("ReverseProxy:AllowedOrigins").Get<string[]>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsOptions.PolicyName, policy =>
            {
                policy
                    .WithOrigins(
                    "http://localhost:3000", 
                    "http://localhost:5173", 
                    "http://localhost:5000", 
                    "https://localhost:5001", 
                    "https://localhost:8081", 
                    "http://localhost:8085")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // potrebno za JWT u browseru
            });
        });

        return builder;
    }

    public static WebApplicationBuilder AddRateLimiting(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                    ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices
                    .GetRequiredService<ProblemDetailsFactory>();
                    Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = problemDetailsFactory
                    .CreateProblemDetails(
                        context.HttpContext,
                        StatusCodes.Status429TooManyRequests,
                        "Too many Requests",
                        detail: $"Too many requests. Please try again affter {retryAfter.TotalSeconds} seconds"
                        );
                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
                }
            };

            options.AddPolicy("default", httpContext =>
            {
                string userName = httpContext.User.Identity?.Name ?? string.Empty;
                if (!string.IsNullOrEmpty(userName))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(
                        userName,
                        _ => 
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 100,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 25
                        }
                    );
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    "anonymous",
                    _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
        });

        return builder;
    }

    

}
