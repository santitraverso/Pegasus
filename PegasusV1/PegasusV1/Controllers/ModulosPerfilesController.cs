using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModulosPerfilesController : BaseSecureController<ModulosPerfiles>
    {
        private readonly IService<Perfiles> _rolesService;
        private readonly IService<Modulos> _modulosService;

        public ModulosPerfilesController(
            IService<ModulosPerfiles> modulosPerfilesService,
            IService<Modulos> modulosService,
            IService<Perfiles> rolesService)
            : base(modulosPerfilesService)
        {
            _modulosService = modulosService;
            _rolesService = rolesService;
        }

        [HttpGet]
        [Route("GetModulosPerfilesForCombo")]
        public async Task<ActionResult<List<ModulosPerfiles>>> GetModulosPerfilessForCombo(string? query = null)
        {
            return await GetForComboSecure(query);
        }

        protected override async Task<List<ModulosPerfiles>> GetForComboInternal(Expression<Func<ModulosPerfiles, bool>>? predicate)
        {
            var modulosPerfiles = await _service.GetModulosPerfilesForCombo(predicate);

            foreach (var moduloPerfil in modulosPerfiles)
            {
                if (moduloPerfil.Id_Modulo.HasValue)
                {
                    moduloPerfil.Modulo = await _modulosService.GetById(moduloPerfil.Id_Modulo.Value);
                }

                if (moduloPerfil.Id_Perfil.HasValue)
                {
                    moduloPerfil.Perfil = await _rolesService.GetById(moduloPerfil.Id_Perfil.Value);
                }
            }

            return modulosPerfiles;
        }

        protected override async Task<ModulosPerfiles?> GetByIdInternal(int id)
        {
            var modulosPerfiles = await _service.GetById(id);

            if (modulosPerfiles != null)
            {
                if (modulosPerfiles.Id_Perfil.HasValue)
                {
                    modulosPerfiles.Perfil = await _rolesService.GetById(modulosPerfiles.Id_Perfil.Value);
                }

                if (modulosPerfiles.Id_Modulo.HasValue)
                {
                    modulosPerfiles.Modulo = await _modulosService.GetById(modulosPerfiles.Id_Modulo.Value);
                }
            }

            return modulosPerfiles;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ActionResult<ModulosPerfiles?>> GetById(int id)
        {
            return await GetByIdSecure(id);
        }

        [HttpPost]
        [Route("CreateModulosPerfiles")]
        public async Task<ActionResult<ModulosPerfiles>> CreateModulos([FromBody] ModulosPerfiles modulosPerfiles)
        {
            try
            {
                var result = await _service.Create(modulosPerfiles);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateModulosPerfiles")]
        public async Task<ActionResult<ModulosPerfiles>> UpdateModulos([FromBody] ModulosPerfiles modulosPerfiles)
        {
            try
            {
                var result = await _service.Update(modulosPerfiles);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteModulosPerfiles/{id}")]
        public async Task<ActionResult> DeleteModulosPerfiles(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var modulosPerfiles = await _service.GetById(id);
                if (modulosPerfiles != null)
                {
                    await _service.Delete(modulosPerfiles);
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
        [Route("CreateAllModulosPerfiles")]
        public async Task<ActionResult<List<ModulosPerfiles>>> CreateAllModulosPerfiles([FromBody] List<ModulosPerfiles> modulosPerfiles)
        {
            try
            {
                var result = await _service.CreateAll(modulosPerfiles);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut]
        [Route("UpdateAllModulosPerfiles")]
        public async Task<ActionResult<List<ModulosPerfiles>>> UpdateAllModulosPerfiles([FromBody] List<ModulosPerfiles> modulosPerfiles)
        {
            try
            {
                var result = await _service.UpdateAll(modulosPerfiles);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete]
        [Route("DeleteAllModulosPerfiles")]
        public async Task<ActionResult> DeleteAllModulosPerfiles([FromBody] List<ModulosPerfiles> modulosPerfiles)
        {
            try
            {
                await _service.DeleteAll(modulosPerfiles);
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
