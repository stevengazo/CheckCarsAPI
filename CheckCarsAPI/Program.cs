using CheckCarsAPI.Data;
using CheckCarsAPI.Hubs;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using static Serilog.Sinks.MSSqlServer.ColumnOptions;


Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("******** Starting CheckCars API ********");
Console.ResetColor();



var builder = WebApplication.CreateBuilder(args);



#region Logger DB table
try
{
    Console.WriteLine("[INFO] Configuring Logger");
    var connectionString = builder.Configuration.GetConnectionString("ReportsConnection");

    Log.Logger = new LoggerConfiguration()
     .MinimumLevel.Debug() // 👈 Nivel general (lo más bajo que quieras ver en consola)
     .Enrich.FromLogContext()
     .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information) // 👈 consola muestra desde Info
     .WriteTo.MSSqlServer(
         connectionString: connectionString,
         sinkOptions: new MSSqlServerSinkOptions
         {
             TableName = "Logs",
             AutoCreateSqlTable = true
         },
         restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error // 👈 solo Error o peor en DB
     )
     .CreateLogger();


    builder.Host.UseSerilog();
}
catch (Exception rf)
{
    Console.WriteLine(rf.Message);

    
}

#endregion

Console.WriteLine("[INFO] Configuring Kestrel and URLs...");
builder.WebHost.UseUrls("http://0.0.0.0:8080");

#region Identity Configuration

Console.WriteLine("[INFO] Adding Identity services...");
builder.Services.AddIdentity<CheckCarsAPI.Models.UserApp, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

#endregion

#region Database Configuration

Console.WriteLine("[INFO] Registering database contexts...");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityUsers")));
builder.Services.AddDbContext<ReportsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ReportsConnection")));

Console.WriteLine("[INFO] Registering email service...");
builder.Services.AddTransient<EmailService>();

#endregion

#region CORS Policy

Console.WriteLine("[INFO] Setting up CORS policy...");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AnyOrigin", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

#endregion

#region Swagger Configuration

Console.WriteLine("[INFO] Configuring Swagger...");
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token.\nExample: eyJhbGciOiJIUzI1NiIs..."
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

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API with File Upload",
        Version = "v1"
    });
});

#endregion

#region JWT Authentication

Console.WriteLine("[INFO] Configuring JWT authentication...");
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

#endregion

#region Controllers & JSON

Console.WriteLine("[INFO] Configuring controllers and JSON...");
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddEndpointsApiExplorer();

#endregion

#region Custom Services

Console.WriteLine("[INFO] Registering custom services...");
builder.Services.AddScoped<ReminderService>();
builder.Services.AddHostedService<ReminderBackgroundService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSignalR();

#endregion

var app = builder.Build();

#region Database Migration

Console.BackgroundColor = ConsoleColor.DarkGreen;
Console.WriteLine("=======================================");
Console.WriteLine(" STARTING DATABASE MIGRATION PROCESS");
Console.WriteLine("=======================================\n");

void MigrateDatabase<T>(string dbName) where T : DbContext
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<T>();

    Console.WriteLine($"-- Migrating database: {dbName}...");

    try
    {
        context.Database.Migrate();
        Console.WriteLine($"-- {dbName} migrated successfully.\n");
    }
    catch (Exception ex)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine($"-- Error migrating {dbName}: {ex.Message}\n");
        Console.BackgroundColor = ConsoleColor.DarkGreen;
    }
}

MigrateDatabase<ReportsDbContext>("ReportsDbContext");
MigrateDatabase<ApplicationDbContext>("ApplicationDbContext");

Console.WriteLine("\n=======================================");
Console.WriteLine(" Database migration completed.");
Console.WriteLine("=======================================\n");
Console.ResetColor();

#endregion

#region Static Files

Console.WriteLine("[INFO] Configuring static files...");
var configuredPath = builder.Configuration["StaticFiles:ImagesPath"];
var imagesPath = string.IsNullOrWhiteSpace(configuredPath)
    ? Path.Combine(Directory.GetCurrentDirectory(), "images")
    : Path.GetFullPath(configuredPath);

imagesPath = imagesPath.Replace('\\', Path.DirectorySeparatorChar)
                       .Replace('/', Path.DirectorySeparatorChar);

if (!Directory.Exists(imagesPath))
{
    Console.WriteLine($"[INFO] Creating directory for images at: {imagesPath}");
    Directory.CreateDirectory(imagesPath);
}
else 
{
    Console.WriteLine($"[INFO] exits folder for images at: {imagesPath}");

}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});

#endregion

Console.WriteLine("[INFO] Starting middleware pipeline...");

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AnyOrigin");
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\n✅ CheckCars API is running at http://0.0.0.0:8080");
Console.ResetColor();

app.Run();
