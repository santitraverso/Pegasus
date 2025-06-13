using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class IntegrantesCursos
    {
        public int Id { get; set; }

        public Curso? Curso { get; set; }
        public int? Id_Curso { get; set; }
        public Usuario? Usuario { get; set; }
        public int? Id_Usuario { get; set; }
    }
}
