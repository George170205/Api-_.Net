namespace WebApplication1.DTOs
{
    /// <summary>
    /// Kárdex académico del estudiante (PDF §2.1 "Alumno").
    /// Por cada materia inscrita devuelve calificación final y porcentaje de
    /// asistencia computado contra las sesiones del grupo.
    /// </summary>
    public class KardexItemDto
    {
        public int InscripcionID { get; set; }
        public int MateriaID { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Ciclo { get; set; } = string.Empty;
        public decimal? CalificacionFinal { get; set; }
        public int SesionesTotales { get; set; }
        public int SesionesAsistidas { get; set; }
        public int PorcentajeAsistencia { get; set; }
        public string Estado { get; set; } = "Cursando";
    }
}
