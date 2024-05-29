using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class IntegrantesMaterias
    {
        public int Id { get; set; }

        public Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }

        public Usuario? Usuario { get; set; }
        public int? Id_Usuario { get; set; }
    }
}
