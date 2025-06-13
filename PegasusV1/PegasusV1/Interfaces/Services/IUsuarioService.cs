using PegasusV1.Entities;

namespace PegasusV1.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> GetUsuarios(string nombre = null, string apellido = null, string perfil = null, string mail = null, bool? activo = null);
    }
}
