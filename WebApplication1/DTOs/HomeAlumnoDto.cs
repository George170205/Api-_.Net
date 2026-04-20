namespace WebApplication1.DTOs
{
    /// <summary>
    /// DTO agregador para la pantalla "Home" del alumno (PDF §2.1).
    ///
    /// Consolida identidad + estadísticas globales en una sola respuesta para
    /// evitar que la vista dispare 3-4 requests al montar. Alimenta
    /// HomeAlumnoPage y PerfilAlumnoPage del cliente MAUI.
    /// </summary>
    public class HomeAlumnoDto
    {
        public int    EstudianteID      { get; set; }
        public string NombreCompleto    { get; set; } = string.Empty;
        public string Iniciales         { get; set; } = string.Empty;
        public string Matricula         { get; set; } = string.Empty;
        public string? Carrera          { get; set; }
        public int?    Semestre         { get; set; }
        public string? Email            { get; set; }

        // Estadísticas globales
        public int     MateriasActivas  { get; set; }
        public decimal Promedio         { get; set; }
        public int     PorcentajeAsistencia { get; set; }
        public int     FaltasTotales    { get; set; }

        // Contexto "próxima clase del día" (si existe)
        public ProximaClaseDto? ProximaClase { get; set; }

        public List<MateriaActivaDto> MateriasList { get; set; } = new();
    }

    public class ProximaClaseDto
    {
        public int      SesionClaseID { get; set; }
        public int      GrupoID       { get; set; }
        public string   Nombre        { get; set; } = string.Empty;
        public string?  Salon         { get; set; }
        public DateTime Fecha         { get; set; }
        public string   HoraInicio    { get; set; } = string.Empty; // "HH:mm"
        public string   HoraFin       { get; set; } = string.Empty;
        public string?  Profesor      { get; set; }
    }

    /// <summary>Tarjeta resumen de cada materia en la que el alumno está inscrito.</summary>
    public class MateriaActivaDto
    {
        public int      MateriaID   { get; set; }
        public int      GrupoID     { get; set; }
        public string   Nombre      { get; set; } = string.Empty;
        public string?  Codigo      { get; set; }
        public string?  Profesor    { get; set; }
        public string?  Horario     { get; set; }
        public string?  Salon       { get; set; }
        public int      Porcentaje  { get; set; } // asistencia %
    }
}
