using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("HIJOS")]
    public class Hijo
    {
        public int Id { get; set; }

        [NotMapped]
        [ForeignKey("Id_Padre")]
        public Usuario Padre { get; set; }
        public int? Id_Padre { get; set; }

        [NotMapped]
        [ForeignKey("Id_Hijo")]
        public Usuario HijoUsuario { get; set; }
        public int? Id_Hijo { get; set; }


    }
}
