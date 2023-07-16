using CardboardBox.PushNotifications;
using CardboardBox.PushNotifications.Database;
using CardboardBox.PushNotifications.Web.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder
    .Services
    .AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

builder
    .Services
    .AddAuthentication(opts => opts.DefaultScheme = AuthMiddleware.SCHEMA)
    .AddScheme<AuthMiddlewareOptions, AuthMiddleware>(AuthMiddleware.SCHEMA, c => { });

await builder
    .Services
    .AddServices(
        builder.Configuration, 
        bob => bob
            .AddDatabase()
            .AddNotifications()
    );

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(c =>
{
    c.AllowAnyHeader()
     .AllowAnyMethod()
     .AllowAnyOrigin()
     .WithExposedHeaders("Content-Disposition");
});

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
