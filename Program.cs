using CheckCarsAPI.Data;
using CheckCarsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Extensions.FileProviders;
using CheckCarsAPI.Migrations.ApplicationDb;
using CheckCarsAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configura Kestrel para escuchar en todas las interfaces
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Add services to the container.

#region  Identity

// Add Identity Service
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
       .AddEntityFrameworkStores<ApplicationDbContext>()
       .AddDefaultTokenProviders();

#endregion 

#region  Data Bases
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<ReportsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ReportsConnection")));
builder.Services.AddTransient<EmailService>();
#endregion

#region  CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AnyOrigin",
        builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        
        );
});

#endregion

#region  Swagger
// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    // Configurar esquema de seguridad para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese  su token JWT. \nEjemplo: eyJhbGciOiJIUzI1NiIs..."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    // Para que Swagger muestre el soporte para archivos y datos JSON
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API with File Upload",
        Version = "v1"
    });

});
// Authentication And JWT
builder.Services
    .AddAuthentication(options =>
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region  Reminders

builder.Services.AddScoped<ReminderService>(); // Servicio para verificar recordatorios
builder.Services.AddHostedService<ReminderBackgroundService>(); // Servicio en segundo plano

#endregion

builder.Services.AddScoped<IFileService, FileService>();


#region  Hubs

builder.Services.AddSignalR();

#endregion

var app = builder.Build();

#region Create Databases 
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
        dbContext.Database.Migrate();  // Aplica las migraciones pendientes autom�ticamente
    }
    catch (Exception r)
    {
        Console.WriteLine(r.Message);
    }
}
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();  // Aplica las migraciones pendientes autom�ticamente
    }
    catch (Exception r)
    {
        Console.WriteLine(r.Message);
    }
}
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var path = Path.Combine(Directory.GetCurrentDirectory(), "images");
if (!Directory.Exists(path))
{
    Directory.CreateDirectory(path);
}


string imagesPath = builder.Configuration["StaticFiles:ImagesPath"];
/*
// Detectar el sistema operativo y ajustar la ruta si es necesario
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Asegúrate de que las rutas estén en el formato correcto para Windows
    imagesPath = imagesPath.Replace('/', '\\');
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{*/
imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images");
// Asegúrate de que las rutas estén en el formato correcto para Linux/Mac
imagesPath = imagesPath.Replace('\\', '/');


Directory.CreateDirectory(imagesPath);

#region  Static Files

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagesPath),
        RequestPath = "/images"
    }
);

#endregion

app.UseCors("AnyOrigin");

app.UseAuthentication();  // Aseg�rate de llamar a UseAuthentication
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationhub");

app.Run();
