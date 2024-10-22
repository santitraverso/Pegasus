using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusV1.Entities
{
    [Table("MODULOS_PERFILES")]
    public class ModulosPerfiles
    {
        public int Id { get; set; }

        [ForeignKey("Id_Modulo")]
        public Modulos? Modulo { get; set; }
        public int? Id_Modulo { get; set; }

        [ForeignKey("Id_Perfil")]
        public Perfiles? Perfil { get; set; }
        public int? Id_Perfil { get; set; }
    }
}
