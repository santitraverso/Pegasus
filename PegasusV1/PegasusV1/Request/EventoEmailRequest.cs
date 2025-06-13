using PegasusV1.Entities;
using System.ComponentModel.DataAnnotations;

namespace PegasusV1.Request
{
    public class EventoEmailRequest
    {
        public int EventoId { get; set; }

        [Required(ErrorMessage = "El nombre del evento es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }
        public bool RequiereConfirmacion { get; set; }        

        [Required(ErrorMessage = "El tipo de destinatario es requerido")]
        public TipoDestinatario TipoDestinatario { get; set; }

    }
}
