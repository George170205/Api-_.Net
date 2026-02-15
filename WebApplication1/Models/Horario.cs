namespace WebApplication1.Models
{
    public class Horario
    {
        public int HorarioID { get; set; }

        public int GrupoID { get; set; }
        public Grupo Grupo { get; set; }

        public int DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public string? Aula { get; set; }
    }
}
