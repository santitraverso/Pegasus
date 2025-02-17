using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class CuadernoComunicados
    {
        public int Id { get; set; }        

        public Usuario? Usuario { get; set; }
        public int? Id_Usuario { get; set; }
        public Curso? Curso { get; set; }
        public int? Id_Curso { get; set; }

        public string Descripcion  { get; set; }

        public DateTime? Fecha  { get; set; }
    }
}
