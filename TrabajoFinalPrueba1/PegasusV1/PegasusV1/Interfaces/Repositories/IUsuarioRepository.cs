using PegasusV1.DbDataContext;
using PegasusV1.Entities;

namespace PegasusV1.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetUsuarios(string nombre = null, string apellido = null, string perfil = null, string mail = null, bool? activo = null);
    }
}
