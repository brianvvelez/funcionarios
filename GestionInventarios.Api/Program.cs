using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using GestionInventarios.Api.DAO.Implementations;
using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Middleware;
using GestionInventarios.Api.Services;
using GestionInventarios.Api.Util;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------
// 1. Servicios MVC + endpoints
// ----------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ----------------------------------------------------------------
// 2. Swagger con soporte de Authorization: Bearer
// ----------------------------------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Gestión de Inventarios API",
        Version     = "v1",
        Description = "API REST con autenticación JWT y autorización por rol " +
                      "(Administrador / Docente) para la gestión de inventarios de equipos."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Description = "Introduce el token JWT obtenido del endpoint /api/auth/login. " +
                      "Swagger lo enviará automáticamente como 'Authorization: Bearer {token}'."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentarios XML para enriquecer Swagger.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// ----------------------------------------------------------------
// 3. Autenticación JWT
// ----------------------------------------------------------------
var jwtKey      = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta la clave 'Jwt:Key' en appsettings.json.");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "GestionInventarios.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GestionInventarios.Clients";

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "La clave Jwt:Key debe tener al menos 32 caracteres / 256 bits.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // En producción debería ser true.
        options.SaveToken            = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// ----------------------------------------------------------------
// 4. Inyección de dependencias (DAO + Services)
//    Se registran como Scoped porque cada DAO mantiene una referencia
//    a ConexionDB y se utilizan dentro del scope de una petición HTTP.
// ----------------------------------------------------------------
builder.Services.AddSingleton<ConexionDB>();

builder.Services.AddScoped<IUsuarioDAO,       UsuarioDAO>();
builder.Services.AddScoped<IInventarioDAO,    InventarioDAO>();
builder.Services.AddScoped<IEstadoEquipoDAO,  EstadoEquipoDAO>();
builder.Services.AddScoped<IMarcaDAO,         MarcaDAO>();
builder.Services.AddScoped<ITipoEquipoDAO,    TipoEquipoDAO>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService,       AuthService>();
builder.Services.AddScoped<DataSeeder>();

// ----------------------------------------------------------------
// 5. Construcción y pipeline
// ----------------------------------------------------------------
var app = builder.Build();

// Middleware global de excepciones DEBE ir primero para envolver todo.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ----------------------------------------------------------------
// 6. Seeder de usuarios al arranque (idempotente)
// ----------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var conexion = scope.ServiceProvider.GetRequiredService<ConexionDB>();
        if (conexion.ProbarConexion(out var mensajeConn))
        {
            logger.LogInformation("Conexión a SQL Server: {Msg}", mensajeConn);
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            seeder.Run();
        }
        else
        {
            logger.LogWarning(
                "No se pudo conectar a SQL Server al arranque: {Msg}. " +
                "El seeder se omitió; la API arrancará igualmente, pero los endpoints " +
                "fallarán hasta que la BD esté disponible.", mensajeConn);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error inesperado durante la inicialización del seeder.");
    }
}

app.Run();
