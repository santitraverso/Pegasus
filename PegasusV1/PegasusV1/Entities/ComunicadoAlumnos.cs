using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("COMUNICADO_ALUMNOS")]
    public class ComunicadoAlumnos
    {
        public int Id { get; set; }
        public int? Id_Comunicado { get; set; }
        [ForeignKey("Id_Comunicado")]
        public CuadernoComunicados? Comunicado { get; set; }
        public int? Id_Alumno { get; set; }
        [ForeignKey("Id_Alumno")]
        public Usuario? Alumno {  get; set; }
    }
}
