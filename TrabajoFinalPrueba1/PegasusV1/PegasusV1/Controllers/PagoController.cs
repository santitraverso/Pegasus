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
    public class PagoController : ControllerBase
    {
        private readonly ILogger<PagoController> _logger;
        private readonly IService<Pago> PagoService;
        private readonly IService<Usuario> UsuarioService;

        public PagoController(ILogger<PagoController> logger,
            IService<Pago> pagoService,
            IService<Usuario> usuarioService)
        {
            _logger = logger;
            PagoService = pagoService;
            UsuarioService = usuarioService;
        }

        [HttpGet]
        [Route("GetPagosForCombo")]
        public async Task<List<Pago>> GetPagosForCombo(string? query = null)
        {
            Expression<Func<Pago, bool>> ex = null;
            if (!string.IsNullOrEmpty(query))
            {
                var p = Expression.Parameter(typeof(Pago), query);
                var e = (Expression)DynamicExpressionParser.ParseLambda(new[] { p }, null, query);
                ex = (Expression<Func<Pago, bool>>)e;
            }

            List<Pago> Pagos = await PagoService.GetForCombo(ex);

            foreach (Pago Pago in Pagos)
            {
                if (Pago.Id_Alumno.HasValue)
                {
                    Pago.Alumno = await UsuarioService.GetById(Pago.Id_Alumno.Value);
                }
            }

            return Pagos;
        }

        [HttpGet]
        [Route("GetById")]
        public async Task<Pago?> GetById(int id)
        {
            Pago Pago = await PagoService.GetById(id);

            if (Pago != null)
            {
                if (Pago.Id_Alumno.HasValue)
                {
                    Pago.Alumno = await UsuarioService.GetById(Pago.Id_Alumno.Value);
                }
            }

            return Pago;
        }

        [HttpPost]
        [Route("CreatePago")]
        public async Task<Pago> CreatePago(Pago pago)
        {
            return await PagoService.Create(pago);
        }

        [HttpPut]
        [Route("UpdatePago")]
        public async Task<Pago> UpdatePago(Pago pago)
        {
            return await PagoService.Update(pago);
        }

        [HttpGet]
        [Route("DeletePago")]
        public async Task DeletePago(int id)
        {
            Pago? pago = await PagoService.GetById(id);
            if(pago != null)
                await PagoService.Delete(pago);
        }
    }
}
