using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PegasusV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiagnosticoController : ControllerBase
    {
        private readonly ILogger<DiagnosticoController> _logger;
        private readonly IConfiguration _configuration;

        public DiagnosticoController(ILogger<DiagnosticoController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("Health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "API funcionando correctamente",
                timestamp = DateTime.Now,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }

        [HttpGet]
        [Route("TestDatabase")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("WebApiDatabase");

                if (string.IsNullOrEmpty(connectionString))
                {
                    return BadRequest(new
                    {
                        error = "La cadena de conexión 'WebApiDatabase' no está configurada",
                        allConnectionStrings = _configuration.GetSection("ConnectionStrings").GetChildren().Select(x => x.Key).ToArray()
                    });
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("SELECT 1", connection);
                    var result = await command.ExecuteScalarAsync();

                    return Ok(new
                    {
                        status = "Conexión a la base de datos exitosa",
                        databaseName = connection.Database,
                        serverVersion = connection.ServerVersion,
                        testQuery = result
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "Error al conectar a la base de datos",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet]
        [Route("GetConfig")]
        public IActionResult GetConfig()
        {
            return Ok(new
            {
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                hasConnectionString = !string.IsNullOrEmpty(_configuration.GetConnectionString("WebApiDatabase")),
                connectionStringLength = _configuration.GetConnectionString("WebApiDatabase")?.Length ?? 0,
                hasGoogleClientId = !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientId"]),
                hasGoogleClientSecret = !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientSecret"]),
                allowedHosts = _configuration["AllowedHosts"]
            });
        }
    }
}