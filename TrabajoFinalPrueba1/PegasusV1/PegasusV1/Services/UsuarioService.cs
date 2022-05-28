using PegasusV1.Entities;
using PegasusV1.Interfaces;
using PegasusV1.Interfaces;

namespace PegasusV1.Services
{
    public class UsuarioService : IUsuarioService
    {
        protected readonly IUsuarioRepository UsuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            UsuarioRepository = usuarioRepository;
        }

        public async Task<List<Usuario>> GetUsuarios(string nombre = null, string apellido = null, string perfil = null, string mail = null, bool? activo = null)
        {
            return await UsuarioRepository.GetUsuarios(nombre, apellido, perfil, mail, activo);
        }

    }
}
