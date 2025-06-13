using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusWeb.Entities
{
    public class CursoMateria
    {
        public int Id { get; set; }
        public Materia? Materia { get; set; }
        public int? Id_Materia { get; set; }
        public Curso? Curso { get; set; }
        public int? Id_Curso { get; set; }
    }
}
