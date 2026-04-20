namespace WebApplication1.Models
{
    /// <summary>
    /// Token de refresco (PDF §6 "Seguridad").
    ///
    /// Permite mantener sesiones largas sin necesidad de pedir credenciales
    /// de nuevo: el access token JWT vive pocos minutos; el refresh token
    /// vive días y se intercambia por un nuevo access token.
    ///
    /// Rotación: cuando se usa para emitir un access token nuevo, se marca
    /// como revocado y se genera uno distinto. Así, si un atacante reusa
    /// un refresh token previamente consumido, detectamos el compromiso.
    /// </summary>
    public class RefreshToken
    {
        public int RefreshTokenID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario? Usuario { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public DateTime? FechaRevocacion { get; set; }
        public string? RevocadoPorIP { get; set; }
        public string? ReemplazadoPorToken { get; set; }

        public string? CreadoPorIP { get; set; }

        public bool Activo =>
            FechaRevocacion == null && FechaExpiracion > DateTime.UtcNow;
    }
}
