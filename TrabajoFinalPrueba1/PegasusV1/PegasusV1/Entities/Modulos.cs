using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("MODULOS")]
    public class Modulos
    {
        public int Id { get; set; }
        public string Modulo { get; set; }
        public int? Id_Perfil { get; set; }
        [ForeignKey("Id_Perfil")]
        public Perfiles Perfil { get; set; }
    }
}
