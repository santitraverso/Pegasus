using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class Contactos
    {
        public int? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Mail { get; set; }
        public string? Telefono { get; set; }
        public int? Tipo_Contacto { get; set; }

        [NotMapped]
        public string? NombreDividido { get; set; } //Campo para mostrar cuando es un docente nombre y apellido
        [NotMapped]
        public string? Apellido { get; set; }
    }
}
