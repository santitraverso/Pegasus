using Microsoft.AspNetCore.Mvc;
using PegasusV1.Entities;
using PegasusV1.Interfaces;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactosController : ControllerBase
    {
        private readonly ILogger<ContactosController> _logger;
        private readonly IService<Contactos> ContactoService;

        public ContactosController(ILogger<ContactosController> logger, IService<Contactos> contactoService)
        {
            _logger = logger;
            ContactoService = contactoService;
        }

        [HttpGet]
        [Route("GetContactosForCombo")]
        public async Task<List<Contactos>> GetContactosForCombo(string? query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Contactos), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                var ex = (Expression<Func<Contactos, bool>>)e;
                return await ContactoService.GetForCombo(ex);
            }
            else
            {
                return await ContactoService.GetForCombo();
            }
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Contactos> GetById(int id)
        {
            return await ContactoService.GetById(id);
        }

        [HttpPost]
        [Route("CreateContacto")]
        public async Task<Contactos> CreateContacto(string contactos)
        {
            Contactos Contacto = JsonConvert.DeserializeObject<Contactos>(contactos);
            return await ContactoService.Create(Contacto);
        }

        [HttpPost]
        [Route("UpdateContacto")]
        public async Task<Contactos> UpdateContacto(string contactos)
        {
            Contactos Contacto = JsonConvert.DeserializeObject<Contactos>(contactos);
            return await ContactoService.Update(Contacto);
        }

        [HttpPost]
        [Route("DeleteContacto")]
        public async void DeleteContacto(string contactos)
        {
            Contactos Contacto = JsonConvert.DeserializeObject<Contactos>(contactos);
            ContactoService.Delete(Contacto);
        }
    }
}
