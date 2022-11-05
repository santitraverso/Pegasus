using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("CONTENIDO")]
    public class Contenido
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Id_Materia")]
        public virtual Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }
        
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
    }
}
