using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("CURSO_MATERIA")]
    public class CursoMateria
    {
        public int Id { get; set; }

        [ForeignKey("Id_Materia")]
        public Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }

        [ForeignKey("Id_Curso")]
        public Curso? Curso { get; set; }
        public int? Id_Curso { get; set; }
    }
}
