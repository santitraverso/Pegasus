using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController : BaseSecureController<Usuario>
    {
        private readonly IService<Perfiles> _perfilesService;
        private readonly IService<DocenteMateria> _docenteMateriaService;
        private readonly IService<Hijo> _hijoService;

        public UsuarioController(
            IService<Usuario> usuarioService,
            IService<Perfiles> perfilesService,
            IService<DocenteMateria> docenteMateriaService,
            IService<Hijo> hijoService)
            : base(usuarioService)
        {
            _perfilesService = perfilesService;
            _docenteMateriaService = docenteMateriaService;
            _hijoService = hijoService;
        }

        [HttpGet]
        [Route("GetUsuariosForCombo")]
        public async Task<ActionResult<List<Usuario>>> GetUsuariosForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<Usuario>> GetForComboInternal(Expression<Func<Usuario, bool>>? predicate)
        {
            var users = await _service.GetForCombo(predicate);

            foreach (var user in users)
            {
                if (user.Id_Perfil.HasValue)
                {
                    user.Perfil = await _perfilesService.GetById(user.Id_Perfil.Value);
                }
            }

            return users;
        }

        protected override async Task<Usuario?> GetByIdInternal(int id)
        {
            var user = await _service.GetById(id);

            if (user != null)
            {
                if (user.Id_Perfil.HasValue)
                {
                    user.Perfil = await _perfilesService.GetById(user.Id_Perfil.Value);
                }
            }

            return user;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<Usuario?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateUsuario")]
        public async Task<ActionResult<Usuario>> CreateUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var result = await _service.Create(usuario);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateUsuario")]
        public async Task<ActionResult<Usuario>> UpdateUsuario([FromBody] Usuario usuario)
        {
            try
            {
                var result = await _service.Update(usuario);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteUsuario/{id}")]
        public async Task<ActionResult> DeleteUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var usuario = await _service.GetById(id);
                if (usuario != null)
                {
                    if (usuario.Id_Perfil == 1)
                    {
                        return BadRequest("No se pueden eliminar usuarios administradores");
                    }

                    // Verificar si es un docente
                    if (usuario.Id_Perfil == 3)
                    {
                        var relaciones = await _docenteMateriaService.GetDocenteMateriaForCombo(dm => dm.Id_Docente == id);

                        // Eliminar cada relación
                        foreach (var relacion in relaciones)
                        {
                            await _docenteMateriaService.Delete(relacion);
                        }
                    }

                    //Verificar si es un padre para borrar relacion con hijos
                    if (usuario.Id_Perfil == 4)
                    {
                        var relaciones = await _hijoService.GetHijoForCombo(h => h.Id_Padre == id);

                        // Eliminar cada relación
                        foreach (var relacion in relaciones)
                        {
                            await _hijoService.Delete(relacion);
                        }
                    }

                    await _service.Delete(usuario);
                    return Ok();
                }
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        [Route("CreateAllUsuario")]
        public async Task<ActionResult<List<Usuario>>> CreateAllUsuario([FromBody] List<Usuario> usuarios)
        {
            try
            {
                var result = await _service.CreateAll(usuarios);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllUsuario")]
        public async Task<ActionResult<List<Usuario>>> UpdateAllUsuario([FromBody] List<Usuario> usuarios)
        {
            try
            {
                var result = await _service.UpdateAll(usuarios);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
