using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Usuario
    {
        public int? Id { get; set; }

        public string? Nombre { get; set; }

        public string? Apellido { get; set; }

        public int? Perfil { get; set; }  

        public string? Mail { get; set; }

        public bool Activo { get; set; }

        [NotMapped]
        public List<Calificaciones>? Calificaciones { get; set; }

        [NotMapped]
        public List<Roles>? Roles { get; set; }
    }
}
