using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("INTEGRANTES_CURSOS")]
    public class IntegrantesCursos
    {
        public int Id { get; set; }

        [ForeignKey("Id_Curso")]
        public Curso? Curso { get; set; }
        public int? Id_Curso { get; set; }

        [ForeignKey("Id_Usuario")]
        public Usuario? Usuario { get; set; }
        public int? Id_Usuario { get; set; }
    }
}
