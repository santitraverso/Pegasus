using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("CUADERNO_COMUNICADOS")]
    public class CuadernoComunicados
    {
        public int Id { get; set; }        

        [ForeignKey("Id_Usuario")]
        public Usuario? Usuario { get; set; }
        public int? Id_Usuario { get; set; }

        [ForeignKey("Id_Curso")]
        public Curso? Curso { get; set; }
        public int? Id_Curso { get; set; }

        public string Descripcion  { get; set; }

        public DateTime? Fecha  { get; set; }
    }
}
