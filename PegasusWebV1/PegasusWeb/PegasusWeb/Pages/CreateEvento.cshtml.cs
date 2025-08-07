using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PegasusWeb.Entities;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PegasusWeb.Pages
{
    public class CreateEventoModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl;

        public CreateEventoModel(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _client = httpClient;
            _apiBaseUrl = apiSettings.Value.BaseUrl;
        }

        [BindProperty]
        public Evento? Evento { get; set; }

        [TempData]
        public int IdEvento { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar sesión válida
            string token = HttpContext.Session.GetString("JwtToken") ?? "";
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idPerfil = HttpContext.Session.GetInt32("IdPerfil") ?? 0;

            if (string.IsNullOrEmpty(token) || idUsuario <= 0 || idPerfil <= 0)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Index");
            }

            if (IdEvento > 0)
            {
                // Es una edición, se carga el evento existente
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Evento/GetById?id={IdEvento}");

                // Añadir el token JWT al encabezado
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string eventoJson = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(eventoJson))
                    {
                        Evento = JsonConvert.DeserializeObject<Evento>(eventoJson);
                    }
                }

                if (Evento == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // Es una carga nueva
                Evento = new Evento
                {
                    Id = 0,
                    TipoDestinatario = TipoDestinatario.Ambos
                };
                Evento.Fecha = DateTime.Now;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string nombre, string descripcion, DateTime fecha, int id, bool requiereConfirmacion = false, TipoDestinatario tipoDestinatario = TipoDestinatario.Ambos)
        {
            IdEvento = id;
            // Validaciones de entrada
            if (string.IsNullOrEmpty(nombre))
                ModelState.AddModelError("nombre", "El campo Nombre es requerido");

            if (string.IsNullOrEmpty(descripcion))
                ModelState.AddModelError("descripcion", "El campo Descripcion es requerido");

            if (fecha == DateTime.MinValue)
                ModelState.AddModelError("fecha", "El campo Fecha es requerido");

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            // Si es una edición, obtener el estado anterior del evento
            Evento eventoAnterior = null;
            if (id > 0)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Evento/GetById?id={id}");

                // Añadir el token JWT al encabezado
                string token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage getResponse = await _client.SendAsync(request);

                if (getResponse.IsSuccessStatusCode)
                {
                    string eventoJson = await getResponse.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(eventoJson))
                    {
                        eventoAnterior = JsonConvert.DeserializeObject<Evento>(eventoJson);
                    }
                }
            }

            dynamic eventoData = new ExpandoObject();
            eventoData.Nombre = nombre;
            eventoData.Descripcion = descripcion;
            eventoData.Fecha = fecha;
            eventoData.RequiereConfirmacion = requiereConfirmacion;
            eventoData.TipoDestinatario = (int)tipoDestinatario;

            if (id > 0)
            {
                eventoData.Id = id;
            }

            // Convertir el objeto dinámico a JSON
            var jsonContent = JsonConvert.SerializeObject(eventoData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Hacer la llamada HTTP (PUT si actualiza, POST si crea)
            HttpResponseMessage response;
            if (id > 0)
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/Evento/UpdateEvento");

                request.Content = content;

                // Añadir el token JWT al encabezado
                string token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                response = await _client.SendAsync(request);

                // Si la actualización fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    var eventoActualizado = JsonConvert.DeserializeObject<Evento>(await response.Content.ReadAsStringAsync());

                    // Verificar si existen registros de IntegrantesEventos para este evento
                    bool existenRegistros = await VerificarExistenciaIntegrantesAsync(id);

                    // Caso 1: Antes requería confirmación y ahora no
                    if (eventoAnterior != null && eventoAnterior.RequiereConfirmacion && !requiereConfirmacion && existenRegistros)
                    {
                        // Eliminar todos los registros de IntegrantesEventos
                        await EliminarIntegrantesEventoAsync(id);
                    }
                    // Caso 2: Antes no requería confirmación y ahora sí
                    else if ((eventoAnterior == null || !eventoAnterior.RequiereConfirmacion) && requiereConfirmacion && !existenRegistros)
                    {
                        // Crear registros de IntegrantesEventos
                        await GuardarIntegrantesAsync(id, tipoDestinatario);
                    }
                    // Caso 3: Sigue requiriendo confirmación pero cambió el tipo de destinatario
                    else if (requiereConfirmacion && existenRegistros &&
                            eventoAnterior != null && eventoAnterior.TipoDestinatario != tipoDestinatario)
                    {
                        // Actualizar los registros según el nuevo tipo de destinatario
                        await ActualizarIntegrantesPorTipoDestinatarioAsync(id, tipoDestinatario);
                    }

                    // Enviar correos informativos sobre la actualización
                    await EnviarCorreosInformativosAsync(eventoActualizado);
                }

            }
            else
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/Evento/CreateEvento");

                request.Content = content;

                // Añadir el token JWT al encabezado
                string token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                response = await _client.SendAsync(request);

                // Si es un evento nuevo, enviar correos y crear registros de integrantes
                if (response.IsSuccessStatusCode)
                {
                    var eventoCreado = JsonConvert.DeserializeObject<Evento>(await response.Content.ReadAsStringAsync());

                    // Enviar correos informativos a usuarios con perfil alumno y padre
                    await EnviarCorreosInformativosAsync(eventoCreado);

                    // Si requiere confirmación, crear registros en INTEGRANTES_EVENTOS
                    if (requiereConfirmacion)
                    {
                        await GuardarIntegrantesAsync(eventoCreado.Id, tipoDestinatario);
                    }
                }
            }

            // Manejar errores de la respuesta HTTP
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("evento", id > 0
                    ? "Hubo un error inesperado al actualizar el Evento: " + errorResponse
                    : "Hubo un error inesperado al crear el Evento: " + errorResponse);

                await OnGetAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "El evento se guardó correctamente.";
            return RedirectToPage("Evento");
        }

        private async Task EnviarCorreosInformativosAsync(Evento evento)
        {
            try
            {
                var emailData = new
                {
                    EventoId = evento.Id,
                    Nombre = evento.Nombre,
                    Descripcion = evento.Descripcion,
                    Fecha = evento.Fecha,
                    RequiereConfirmacion = evento.RequiereConfirmacion,
                    TipoDestinatario = (int)evento.TipoDestinatario

                };

                var jsonContent = JsonConvert.SerializeObject(emailData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/Email/EnviarCorreoInformativo");

                request.Content = content;

                // Añadir el token JWT al encabezado
                string token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                await _client.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correos: {ex.Message}");
            }
        }

        private async Task GuardarIntegrantesAsync(int eventoId, TipoDestinatario tipoDestinatario)
        {
            try
            {
                // Elegir el tipo de destinatario seleccionado
                string queryParam = tipoDestinatario switch
                {
                    TipoDestinatario.Alumnos => Uri.EscapeDataString($"x=>x.id_perfil==2"),
                    TipoDestinatario.Padres => Uri.EscapeDataString($"x=>x.id_perfil==4"),
                    TipoDestinatario.Ambos => Uri.EscapeDataString($"x=>x.id_perfil==2 || x.id_perfil==4"),
                    _ => Uri.EscapeDataString($"x=>x.id_perfil==2 || x.id_perfil==4")
                };

                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                // Obtener la lista de usuarios
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/Usuario/GetUsuariosForCombo?query={queryParam}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage usuariosResponse = await _client.SendAsync(request);

                if (usuariosResponse.IsSuccessStatusCode)
                {
                    var usuariosJson = await usuariosResponse.Content.ReadAsStringAsync();
                    var usuarios = JsonConvert.DeserializeObject<List<Usuario>>(usuariosJson);

                    // Llista de IntegrantesEventos para todos los usuarios
                    var integrantesEventosList = new List<IntegrantesEventos>();

                    foreach (var usuario in usuarios)
                    {
                        integrantesEventosList.Add(new IntegrantesEventos
                        {
                            Id_Evento = eventoId,
                            Id_Usuario = usuario.Id,
                            Leido = false,
                            Confirmado = false
                        });
                    }

                    if (integrantesEventosList.Any())
                    {
                        var jsonContent = JsonConvert.SerializeObject(integrantesEventosList);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var createAllRequest = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/IntegrantesEventos/CreateAllIntegrantesEventos");
                        createAllRequest.Content = content;

                        if (!string.IsNullOrEmpty(token))
                        {
                            createAllRequest.Headers.Add("Authorization", $"Bearer {token}");
                        }

                        var createResponse = await _client.SendAsync(createAllRequest);

                        if (!createResponse.IsSuccessStatusCode)
                        {
                            var errorResponse = await createResponse.Content.ReadAsStringAsync();
                            Console.WriteLine($"Error al crear integrantes de eventos: {errorResponse}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando registros de integrantes: {ex.Message}");
            }
        }

        private async Task<bool> VerificarExistenciaIntegrantesAsync(int eventoId)
        {
            try
            {
                string queryParam = Uri.EscapeDataString($"x=>x.id_evento=={eventoId}");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesEventos/GetIntegrantesEventossForCombo?query={queryParam}");

                // Añadir el token JWT al encabezado
                string token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var integrantesJson = await response.Content.ReadAsStringAsync();
                    var integrantes = JsonConvert.DeserializeObject<List<IntegrantesEventos>>(integrantesJson);
                    return integrantes != null && integrantes.Count > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verificando integrantes: {ex.Message}");
                return false;
            }
        }

        private async Task ActualizarIntegrantesPorTipoDestinatarioAsync(int eventoId, TipoDestinatario tipoDestinatario)
        {
            try
            {
                // Primero eliminar todos los registros existentes
                await EliminarIntegrantesEventoAsync(eventoId);

                // Luego crear los nuevos registros según el tipo de destinatario
                await GuardarIntegrantesAsync(eventoId, tipoDestinatario);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando integrantes por tipo: {ex.Message}");
            }
        }

        private async Task EliminarIntegrantesEventoAsync(int eventoId)
        {
            try
            {
                // Obtener el token JWT
                string token = HttpContext.Session.GetString("JwtToken");

                // Obtener todos los integrantes del evento
                string queryParam = Uri.EscapeDataString($"x=>x.id_evento=={eventoId}");
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/IntegrantesEventos/GetIntegrantesEventossForCombo?query={queryParam}");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }

                HttpResponseMessage integrantesResponse = await _client.SendAsync(request);

                if (integrantesResponse.IsSuccessStatusCode)
                {
                    var integrantesJson = await integrantesResponse.Content.ReadAsStringAsync();
                    var integrantes = JsonConvert.DeserializeObject<List<IntegrantesEventos>>(integrantesJson);

                    if (integrantes != null && integrantes.Any())
                    {
                        var jsonContent = JsonConvert.SerializeObject(integrantes);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var deleteAllRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/IntegrantesEventos/DeleteAllIntegrantesEventos");
                        deleteAllRequest.Content = content;

                        if (!string.IsNullOrEmpty(token))
                        {
                            deleteAllRequest.Headers.Add("Authorization", $"Bearer {token}");
                        }

                        var deleteResponse = await _client.SendAsync(deleteAllRequest);

                        if (!deleteResponse.IsSuccessStatusCode)
                        {
                            var errorResponse = await deleteResponse.Content.ReadAsStringAsync();
                            Console.WriteLine($"Error al eliminar integrantes de eventos: {errorResponse}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error eliminando integrantes: {ex.Message}");
            }
        }
    }
}
