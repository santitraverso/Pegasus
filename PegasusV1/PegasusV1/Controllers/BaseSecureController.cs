using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PegasusV1.Interfaces;
using PegasusV1.Security;
using System.Linq.Expressions;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseSecureController<T> : ControllerBase where T : class
    {
        protected readonly IService<T> _service;

        protected BaseSecureController(IService<T> service)
        {
            _service = service;
        }

        protected async Task<ActionResult<List<T>>> GetForComboSecure(string? query = null)
        {
            try
            {
                Expression<Func<T, bool>>? predicate = null;

                if (!string.IsNullOrEmpty(query))
                {
                    predicate = SecureQueryParser.ParseSafeQuery<T>(query);
                }

                var results = await GetForComboInternal(predicate);
                return Ok(results);
            }
            catch (SecurityException)
            {
                return BadRequest("Consulta no permitida por razones de seguridad");
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        protected async Task<ActionResult<T?>> GetByIdSecure(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido");
            }

            try
            {
                var result = await GetByIdInternal(id);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        protected virtual async Task<List<T>> GetForComboInternal(Expression<Func<T, bool>>? predicate)
        {
            return await _service.GetForCombo(predicate);
        }

        protected virtual async Task<T?> GetByIdInternal(int id)
        {
            return await _service.GetById(id);
        }
    }
}
