using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class DocenteMateria
    {
        public int Id { get; set; }
        public Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }
        public Usuario? Docente { get; set; }
        public int? Id_Docente { get; set; }
        public Curso? Curso { get; set; }
        public int? Id_Curso { get; set; }
    }
}
