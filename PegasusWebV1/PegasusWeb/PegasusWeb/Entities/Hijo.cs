using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Hijo
    {
        public int Id { get; set; }

        public Usuario? Padre { get; set; }
        public int? Id_Padre { get; set; }

        public Usuario? HijoUsuario { get; set; }
        public int? Id_Hijo { get; set; }


    }
}
