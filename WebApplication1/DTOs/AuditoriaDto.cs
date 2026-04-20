namespace WebApplication1.DTOs
{
    /// <summary>
    /// Evento de auditoría unificado (PDF §2.3, §6).
    /// No existe una tabla dedicada: se consolida desde IntentoLogin,
    /// QRGenerado, Asistencia y Calificacion para dar una bitácora navegable.
    /// </summary>
    public class EventoAuditoriaDto
    {
        public string Tipo { get; set; } = string.Empty;      // Login | QR | Asistencia | Calificacion
        public DateTime Fecha { get; set; }
        public string Resumen { get; set; } = string.Empty;   // Descripción corta
        public string? Meta { get; set; }                     // Detalle (IP, actor, grupo, …)
    }
}
