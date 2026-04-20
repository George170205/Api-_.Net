namespace WebApplication1.DTOs
{
    /// <summary>
    /// Fila de horario semanal (PDF §2.1, §2.2).
    /// Se arma a partir de <c>Horario</c> (definición recurrente por día de
    /// semana) uniendo Grupo → Materia → Docente.
    /// </summary>
    public class HorarioDto
    {
        public int     HorarioID      { get; set; }
        public int     GrupoID        { get; set; }
        public int     MateriaID      { get; set; }
        public int     DocenteID      { get; set; }

        public int     DiaSemana      { get; set; } // 1=Lunes … 7=Domingo
        public string  DiaNombre      { get; set; } = string.Empty;
        public string  HoraInicio     { get; set; } = string.Empty; // "HH:mm"
        public string  HoraFin        { get; set; } = string.Empty;

        public string  MateriaNombre  { get; set; } = string.Empty;
        public string? MateriaCodigo  { get; set; }
        public string? GrupoCodigo    { get; set; }
        public string? Docente        { get; set; }
        public string? Aula           { get; set; }
    }
}
