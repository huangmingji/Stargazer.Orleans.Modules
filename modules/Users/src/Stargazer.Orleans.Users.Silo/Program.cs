using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Stargazer.Common.Extend;
using Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations;
using Stargazer.Orleans.Users.Silo;
using Stargazer.Orleans.Users.Silo.Authorization;
using Stargazer.Orleans.Users.Silo.Middleware;
using Stargazer.Orleans.Users.Grains.Abstractions.Security;
using Stargazer.Orleans.Users.Silo.Security;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Host.UseSerilog();

builder.Services.UseEntityFramworkCore().MigrateDatabase();
builder.ConfigureOrleansServer();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings not configured");
var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
var apiPrefix = configuration.GetSection("Api:Prefix").Get<string>() ?? "api";
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPermissionPolicies();
});

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.ShouldInclude = (_) => true;
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Stargazer Users API",
            Version = "v1",
            Description = "Stargazer Users API"
        };
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
builder.Services.AddControllers()
    .AddNewtonsoftJson(
    op =>
    {
        op.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        op.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        op.SerializerSettings.Converters.Add(new Ext.DateTimeJsonConverter());
        op.SerializerSettings.Converters.Add(new Ext.LongJsonConverter());
    })
    .AddMvcOptions(options =>
    {
        options.Conventions.Insert(0, new CentralizedPrefixConvention(apiPrefix));
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
        options.WithTitle("Stargazer Users API")
            .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
            .AddHttpAuthentication(JwtBearerDefaults.AuthenticationScheme, auth => { auth.Token = ""; })
    );
}

app.UseGlobalExceptionMiddleware();
app.UseHttpsRedirection();
if (allowedOrigins.Length > 0)
{
    app.UseCors("AllowFrontend");
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.Run();


internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
        }
    }
}

internal sealed class CentralizedPrefixConvention(string prefix) : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        new AttributeRouteModel(new RouteAttribute(prefix)),
                        selector.AttributeRouteModel);
                }
            }
        }
    }
}


