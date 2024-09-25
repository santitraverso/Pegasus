using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Web;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly ILogger<RolesController> _logger;
        private readonly IService<Perfiles> RolesService;

        public RolesController(ILogger<RolesController> logger,
            IService<Perfiles> rolesService)
        {
            _logger = logger;
            RolesService = rolesService;
        }

        [HttpGet]
        [Route("GetRolesForCombo")]
        public async Task<List<Perfiles>> GetRolessForCombo(string? query = null)
        {
            Expression<Func<Perfiles, bool>>? ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Perfiles), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Perfiles, bool>>)e;
            }

            List<Perfiles> Roless = await RolesService.GetRolesForCombo(ex);

            return Roless;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Perfiles?> GetById(int id)
        {
            Perfiles Roles = await RolesService.GetById(id);

            return Roles;
        }

        [HttpPost]
        [Route("CreateRoles")]
        public async Task<Perfiles> CreateRoles(Perfiles Roles)
        {
            return await RolesService.Create(Roles);
        }

        [HttpPut]
        [Route("UpdateRoles")]
        public async Task<Perfiles> UpdateRoles(Perfiles Roles)
        {
            return await RolesService.Update(Roles);
        }

        [HttpGet]
        [Route("DeleteRoles")]
        public async Task DeleteRoles(int id)
        {
            Perfiles? Roles = await RolesService.GetById(id);
            if (Roles != null)
                await RolesService.Delete(Roles);
        }

        [HttpPut]
        [Route("CreateAllRoles")]
        public async Task<List<Perfiles>> CreateAllRoles(List<Perfiles> Roless)
        {
            return await RolesService.CreateAll(Roless);
        }

        [HttpPut]
        [Route("UpdateAllRoles")]
        public async Task<List<Perfiles>> UpdateAllRoles(List<Perfiles> Roless)
        {
            return await RolesService.UpdateAll(Roless);
        }

        [HttpGet]
        [Route("DeleteAllRoles")]
        public async Task DeleteAllRoles(List<Perfiles> Roless)
        {
            await RolesService.DeleteAll(Roless);
        }

    }
}
