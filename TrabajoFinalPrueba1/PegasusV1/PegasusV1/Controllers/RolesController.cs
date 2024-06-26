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
        private readonly IService<Roles> RolesService;

        public RolesController(ILogger<RolesController> logger,
            IService<Roles> rolesService)
        {
            _logger = logger;
            RolesService = rolesService;
        }

        [HttpGet]
        [Route("GetRolesForCombo")]
        public async Task<List<Roles>> GetRolessForCombo(string? query = null)
        {
            Expression<Func<Roles, bool>>? ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Roles), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Roles, bool>>)e;
            }

            List<Roles> Roless = await RolesService.GetRolesForCombo(ex);

            return Roless;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Roles?> GetById(int id)
        {
            Roles Roles = await RolesService.GetById(id);

            return Roles;
        }

        [HttpPost]
        [Route("CreateRoles")]
        public async Task<Roles> CreateRoles(Roles Roles)
        {
            return await RolesService.Create(Roles);
        }

        [HttpPut]
        [Route("UpdateRoles")]
        public async Task<Roles> UpdateRoles(Roles Roles)
        {
            return await RolesService.Update(Roles);
        }

        [HttpGet]
        [Route("DeleteRoles")]
        public async Task DeleteRoles(int id)
        {
            Roles? Roles = await RolesService.GetById(id);
            if (Roles != null)
                await RolesService.Delete(Roles);
        }

        [HttpPut]
        [Route("CreateAllRoles")]
        public async Task<List<Roles>> CreateAllRoles(List<Roles> Roless)
        {
            return await RolesService.CreateAll(Roless);
        }

        [HttpPut]
        [Route("UpdateAllRoles")]
        public async Task<List<Roles>> UpdateAllRoles(List<Roles> Roless)
        {
            return await RolesService.UpdateAll(Roless);
        }

        [HttpGet]
        [Route("DeleteAllRoles")]
        public async Task DeleteAllRoles(List<Roles> Roless)
        {
            await RolesService.DeleteAll(Roless);
        }

    }
}
