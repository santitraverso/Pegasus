using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Contenido
    {
        [Key]
        public int Id { get; set; }

        public virtual Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }
        
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
    }
}
