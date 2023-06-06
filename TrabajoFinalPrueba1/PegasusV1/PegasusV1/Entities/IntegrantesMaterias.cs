using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("INTEGRANTES_MATERIAS")]
    public class IntegrantesMaterias
    {
        public int Id { get; set; }

        [ForeignKey("Id_Materia")]
        public Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }

        [ForeignKey("Id_Usuario")]
        public Usuario? Usuario { get; set; }
        public int? Id_Usuario { get; set; }


    }
}
