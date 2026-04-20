namespace WebApplication1.DTOs
{
    /// <summary>
    /// Próxima sesión de clase del alumno (PDF §3.3). Se devuelve una lista
    /// corta ordenada por fecha+hora ascendente, incluyendo si el alumno ya
    /// registró asistencia para esa sesión (flag <see cref="YaRegistrada"/>).
    /// </summary>
    public class ProximaAsistenciaDto
    {
        public int      SesionClaseID { get; set; }
        public int      GrupoID       { get; set; }
        public int      MateriaID     { get; set; }
        public string   MateriaNombre { get; set; } = string.Empty;
        public string?  Profesor      { get; set; }
        public DateTime Fecha         { get; set; }
        public string   HoraInicio    { get; set; } = string.Empty; // "HH:mm"
        public string   HoraFin       { get; set; } = string.Empty;
        public string?  Aula          { get; set; }
        public string?  Tema          { get; set; }
        public bool     YaRegistrada  { get; set; }
        public string?  EstadoAsistencia { get; set; } // Presente | Retardo | Falta | null
    }
}
