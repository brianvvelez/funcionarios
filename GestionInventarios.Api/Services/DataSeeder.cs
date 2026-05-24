using GestionInventarios.Api.DAO.Interfaces;
using GestionInventarios.Api.Models.Entities;

namespace GestionInventarios.Api.Services
{
    /// <summary>
    /// Seeder de datos de arranque. Se ejecuta una sola vez al iniciar
    /// la aplicación; si la tabla 'usuarios' está vacía, inserta los
    /// dos usuarios por defecto (Administrador y Docente) usando las
    /// credenciales declaradas en appsettings.json (sección "Seed") y
    /// hasheando las contraseñas con BCrypt. De esta forma no se
    /// versionan hashes en el script SQL.
    /// </summary>
    public class DataSeeder
    {
        private readonly IUsuarioDAO     _usuarioDAO;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration  _configuration;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            IUsuarioDAO     usuarioDAO,
            IPasswordHasher passwordHasher,
            IConfiguration  configuration,
            ILogger<DataSeeder> logger)
        {
            _usuarioDAO     = usuarioDAO;
            _passwordHasher = passwordHasher;
            _configuration  = configuration;
            _logger         = logger;
        }

        public void Run()
        {
            if (!_usuarioDAO.EstaVacia())
            {
                _logger.LogInformation(
                    "Tabla 'usuarios' no está vacía. Se omite el seed de usuarios.");
                return;
            }

            var adminEmail    = _configuration["Seed:AdminEmail"]    ?? "admin@empresa.com";
            var adminPassword = _configuration["Seed:AdminPassword"] ?? "Admin123*";
            var docEmail      = _configuration["Seed:DocenteEmail"]    ?? "docente@empresa.com";
            var docPassword   = _configuration["Seed:DocentePassword"] ?? "Docente123*";

            _usuarioDAO.Crear(new Usuario
            {
                Nombres      = "Administrador",
                Apellidos    = "del Sistema",
                Email        = adminEmail,
                PasswordHash = _passwordHasher.Hash(adminPassword),
                Rol          = "Administrador",
                Activo       = true
            });

            _usuarioDAO.Crear(new Usuario
            {
                Nombres      = "Docente",
                Apellidos    = "de Prueba",
                Email        = docEmail,
                PasswordHash = _passwordHasher.Hash(docPassword),
                Rol          = "Docente",
                Activo       = true
            });

            _logger.LogInformation(
                "Seed de usuarios completado. Admin: {AdminEmail}, Docente: {DocenteEmail}",
                adminEmail, docEmail);
        }
    }
}
