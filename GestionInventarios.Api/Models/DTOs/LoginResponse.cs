namespace GestionInventarios.Api.Models.DTOs
{
    public class LoginResponse
    {
        public string   Token     { get; set; } = string.Empty;
        public string   Email     { get; set; } = string.Empty;
        public string   Rol       { get; set; } = string.Empty;
        public DateTime ExpiraEn  { get; set; }
    }
}
