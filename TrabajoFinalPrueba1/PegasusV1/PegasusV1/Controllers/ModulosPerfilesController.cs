using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web;
using Microsoft.AspNetCore.Cors.Infrastructure;
using PegasusV1.Services;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ModulosPerfilesController : ControllerBase
    {
        private readonly ILogger<ModulosPerfilesController> _logger;
        private readonly IService<ModulosPerfiles> ModulosPerfilesService;
        private readonly IService<Perfiles> RolesService;
        private readonly IService<Modulos> ModulosService;

        public ModulosPerfilesController(ILogger<ModulosPerfilesController> logger,
            IService<ModulosPerfiles> modulosPerfilesService,
            IService<Modulos> modulosService,
            IService<Perfiles> rolesService)
        {
            _logger = logger;
            ModulosPerfilesService = modulosPerfilesService;
            ModulosService = modulosService;
            RolesService = rolesService;
        }

        [HttpGet]
        [Route("GetModulosPerfilesForCombo")]
        public async Task<List<ModulosPerfiles>> GetModulosPerfilessForCombo(string? query = null)
        {
            Expression<Func<ModulosPerfiles, bool>>? ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(ModulosPerfiles), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<ModulosPerfiles, bool>>)e;
            }

            List<ModulosPerfiles> modulosPerfiless = await ModulosPerfilesService.GetModulosPerfilesForCombo(ex);

            foreach (ModulosPerfiles moduloPerfil in modulosPerfiless)
            {
                if (moduloPerfil.Id_Modulo.HasValue)
                {
                    moduloPerfil.Modulo = await ModulosService.GetById(moduloPerfil.Id_Modulo.Value);
                }

                if (moduloPerfil.Id_Perfil.HasValue)
                {
                    moduloPerfil.Perfil = await RolesService.GetById(moduloPerfil.Id_Perfil.Value);
                }
            }

            return modulosPerfiless;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<ModulosPerfiles?> GetById(int id)
        {
            ModulosPerfiles ModulosPerfiles = await ModulosPerfilesService.GetById(id);


            if (ModulosPerfiles != null)
            {
                if (ModulosPerfiles.Id_Perfil.HasValue)
                {
                    ModulosPerfiles.Perfil = await RolesService.GetById(ModulosPerfiles.Id_Perfil.Value);
                }

                if (ModulosPerfiles.Id_Modulo.HasValue)
                {
                    ModulosPerfiles.Modulo = await ModulosService.GetById(ModulosPerfiles.Id_Modulo.Value);
                }

            }

            return ModulosPerfiles;
        }

        [HttpPost]
        [Route("CreateModulosPerfiles")]
        public async Task<ModulosPerfiles> CreateModulos(ModulosPerfiles ModulosPerfiles)
        {
            return await ModulosPerfilesService.Create(ModulosPerfiles);
        }

        [HttpPut]
        [Route("UpdateModulosPerfiles")]
        public async Task<ModulosPerfiles> UpdateModulos(ModulosPerfiles ModulosPerfiles)
        {
            return await ModulosPerfilesService.Update(ModulosPerfiles);
        }

        [HttpGet]
        [Route("DeleteModulosPerfiles")]
        public async Task DeleteModulosPerfiles(int id)
        {
            ModulosPerfiles? ModulosPerfiles = await ModulosPerfilesService.GetById(id);
            if (ModulosPerfiles != null)
                await ModulosPerfilesService.Delete(ModulosPerfiles);
        }

        [HttpPut]
        [Route("CreateAllModulosPerfiles")]
        public async Task<List<ModulosPerfiles>> CreateAllModulosPerfiles(List<ModulosPerfiles> ModulosPerfiless)
        {
            return await ModulosPerfilesService.CreateAll(ModulosPerfiless);
        }

        [HttpPut]
        [Route("UpdateAllModulosPerfiles")]
        public async Task<List<ModulosPerfiles>> UpdateAllModulosPerfiles(List<ModulosPerfiles> ModulosPerfiless)
        {
            return await ModulosPerfilesService.UpdateAll(ModulosPerfiless);
        }

        [HttpGet]
        [Route("DeleteAllModulosPerfiles")]
        public async Task DeleteAllModulosPerfiles(List<ModulosPerfiles> ModulosPerfiless)
        {
            await ModulosPerfilesService.DeleteAll(ModulosPerfiless);
        }

    }
}
