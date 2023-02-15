using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class IntegrantesEventos
    {
        public int Id { get; set; }

        public Evento? Evento { get; set; }
        public int? Id_Evento { get; set; }

        public Usuario? Usuario { get; set; }
        public int? Id_Usuario { get; set; }


    }
}
