using Microsoft.EntityFrameworkCore;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using PegasusV1.DbDataContext;

namespace PegasusV1.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        protected readonly DataContext _dbContext;

        public UsuarioRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Usuario>> GetUsuarios(string nombre = null, string apellido = null, string perfil = null, string mail = null, bool? activo = null)
        {
            var query = _dbContext.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(x => x.Nombre.ToLower().Contains(nombre.ToLower()));
            }

            query = query.OrderBy(x => x.Nombre);

            return await query.ToListAsync();
        }
    }
}
