using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("CONTENIDO_MATERIAS")]
    public class ContenidoMaterias
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Id_Materia")]
        public virtual Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;
    }
}
