using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class ModulosPerfiles
    {
        public int Id { get; set; }

        public Modulos? Modulo { get; set; }
        public int? Id_Modulo { get; set; }
        public Perfiles? Perfil { get; set; }
        public int? Id_Perfil { get; set; }
    }
}
