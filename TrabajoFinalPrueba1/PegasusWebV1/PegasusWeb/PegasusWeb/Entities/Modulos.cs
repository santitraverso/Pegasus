using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Modulos
    {
        public int Id { get; set; }
        public string Modulo { get; set; }
        public int? Id_Perfil { get; set; }
        public Perfiles Perfil { get; set; }
    }
}
