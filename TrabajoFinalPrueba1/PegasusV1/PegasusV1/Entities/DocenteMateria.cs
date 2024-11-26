using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("DOCENTE_MATERIA")]
    public class DocenteMateria
    {
        public int Id { get; set; }

        [ForeignKey("Id_Docente")]
        public Usuario? Docente { get; set; }
        public int? Id_Docente { get; set; }

        [ForeignKey("Id_Materia")]
        public Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }

        [ForeignKey("Id_Curso")]
        public Curso? Curso { get; set; }
        public int? Id_Curso { get; set; }
    }
}
