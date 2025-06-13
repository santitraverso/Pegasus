import { type Evento, TipoDestinatario } from "../models/evento"
import type { IntegrantesEventos } from "../models/integrantesEventos"
import type { Usuario } from "../models/usuario"
import { CONFIG } from "./config"


export const eventoService = {
  // Obtener todos los eventos filtrados por perfil
  async getEventos(idPerfil: number): Promise<Evento[]> {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/Evento/GetEventosForCombo`)

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const eventos: Evento[] = await response.json()

      // Filtrar eventos según el perfil del usuario
      return filtrarEventosPorPerfil(eventos, idPerfil)
    } catch (error) {
      throw error
    }
  },

  // Obtener evento por ID
  async getEventoById(id: number): Promise<Evento> {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/Evento/GetById?id=${id}`)

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      throw error
    }
  },

  // Crear evento
  async createEvento(evento: Partial<Evento>): Promise<Evento> {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/Evento/CreateEvento`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          nombre: evento.nombre,
          descripcion: evento.descripcion,
          fecha: evento.fecha,
          requiereConfirmacion: evento.requiereConfirmacion || false,
          tipoDestinatario: evento.tipoDestinatario || TipoDestinatario.Ambos,
        }),
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`Error ${response.status}: ${errorText}`)
      }

      const eventoCreado = await response.json()

      // Enviar correos informativos
      await this.enviarCorreosInformativos(eventoCreado)

      // Si requiere confirmación, crear registros de integrantes
      if (evento.requiereConfirmacion) {
        await this.guardarIntegrantes(eventoCreado.id, evento.tipoDestinatario || TipoDestinatario.Ambos)
      }

      return eventoCreado
    } catch (error) {
      throw error
    }
  },

  // Actualizar evento (versión unificada)
  async updateEvento(evento: Partial<Evento>): Promise<Evento> {
    try {
      // Obtener el evento anterior para comparar
      const eventoAnterior = await this.getEventoById(evento.id!)

      const response = await fetch(`${CONFIG.API_BASE_URL}/Evento/UpdateEvento`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          id: evento.id,
          nombre: evento.nombre,
          descripcion: evento.descripcion,
          fecha: evento.fecha,
          requiereConfirmacion: evento.requiereConfirmacion || false,
          tipoDestinatario: evento.tipoDestinatario || TipoDestinatario.Ambos,
        }),
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`Error ${response.status}: ${errorText}`)
      }

      const eventoActualizado = await response.json()

      // Verificar si existen registros de IntegrantesEventos para este evento
      const existenRegistros = await this.verificarExistenciaIntegrantes(evento.id!)

      // Caso 1: Antes requería confirmación y ahora no
      if (eventoAnterior.requiereConfirmacion && !evento.requiereConfirmacion && existenRegistros) {
        await this.eliminarIntegrantesEvento(evento.id!)
      }
      // Caso 2: Antes no requería confirmación y ahora sí
      else if (!eventoAnterior.requiereConfirmacion && evento.requiereConfirmacion && !existenRegistros) {
        await this.guardarIntegrantes(evento.id!, evento.tipoDestinatario || TipoDestinatario.Ambos)
      }
      // Caso 3: Sigue requiriendo confirmación pero cambió el tipo de destinatario
      else if (evento.requiereConfirmacion && existenRegistros &&
              eventoAnterior.tipoDestinatario !== evento.tipoDestinatario) {
        await this.actualizarIntegrantesPorTipoDestinatario(evento.id!, evento.tipoDestinatario || TipoDestinatario.Ambos)
      }

      // Enviar correos informativos sobre la actualización
      await this.enviarCorreosInformativos(eventoActualizado)

      return eventoActualizado
    } catch (error) {
      throw error
    }
  },

  // Verificar si existen integrantes para un evento
  async verificarExistenciaIntegrantes(eventoId: number): Promise<boolean> {
    try {
      const integrantes = await this.getIntegrantesEvento(eventoId)
      return integrantes.length > 0
    } catch (error) {
      console.error("Error verificando existencia de integrantes:", error)
      return false
    }
  },

  // Actualizar integrantes por tipo de destinatario
  async actualizarIntegrantesPorTipoDestinatario(eventoId: number, tipoDestinatario: TipoDestinatario): Promise<void> {
    try {
      // Primero eliminar todos los integrantes existentes
      await this.eliminarIntegrantesEvento(eventoId)

      // Luego crear los nuevos según el tipo de destinatario
      await this.guardarIntegrantes(eventoId, tipoDestinatario)
    } catch (error) {
      throw error
    }
  },

  // Eliminar evento
  async deleteEvento(id: number): Promise<void> {
    try {
      // Primero eliminar integrantes del evento
      await this.eliminarIntegrantesEvento(id)

      // Luego eliminar el evento
       const response = await fetch(`${CONFIG.API_BASE_URL}/Evento/DeleteEvento/${id}`, {
      method: 'DELETE',
    });

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`Error ${response.status}: ${errorText}`)
      }
    } catch (error) {
      throw error
    }
  },

  // Obtener integrantes de un evento
  async getIntegrantesEvento(eventoId: number): Promise<IntegrantesEventos[]> {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_evento==${eventoId}`)
      const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesEventos/GetIntegrantesEventossForCombo?query=${queryParam}`)

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      return await response.json()
    } catch (error) {
      throw error
    }
  },

  // Obtener integrante específico de un evento
  async getIntegranteEvento(eventoId: number, usuarioId: number): Promise<IntegrantesEventos | null> {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_evento==${eventoId} && x.id_usuario==${usuarioId}`)
      const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesEventos/GetIntegrantesEventossForCombo?query=${queryParam}`)

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const integrantes: IntegrantesEventos[] = await response.json()
      return integrantes.length > 0 ? integrantes[0] : null
    } catch (error) {
      throw error
    }
  },

  // Marcar como leído
  async marcarComoLeido(integranteId: number, eventoId: number, usuarioId: number): Promise<void> {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesEventos/UpdateIntegrantesEventos`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          id: integranteId,
          id_Evento: eventoId,
          id_Usuario: usuarioId,
          leido: true,
          confirmado: false,
        }),
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`Error ${response.status}: ${errorText}`)
      }
    } catch (error) {
      throw error
    }
  },

  // Actualizar confirmación de asistencia
  async actualizarConfirmacion(
    integranteId: number,
    eventoId: number,
    usuarioId: number,
    confirmado: boolean,
  ): Promise<void> {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesEventos/UpdateIntegrantesEventos`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          id: integranteId,
          id_Evento: eventoId,
          id_Usuario: usuarioId,
          leido: true,
          confirmado: confirmado,
        }),
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`Error ${response.status}: ${errorText}`)
      }
    } catch (error) {
      throw error
    }
  },

  // Enviar correos informativos
  async enviarCorreosInformativos(evento: Evento): Promise<void> {
    try {
      await fetch(`${CONFIG.API_BASE_URL}/Email/EnviarCorreoInformativo`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          eventoId: evento.id,
          nombre: evento.nombre,
          descripcion: evento.descripcion,
          fecha: evento.fecha,
          requiereConfirmacion: evento.requiereConfirmacion,
          tipoDestinatario: evento.tipoDestinatario,
        }),
      })
    } catch (error) {
      console.error("Error enviando correos:", error)
    }
  },

  // Guardar integrantes del evento
  async guardarIntegrantes(eventoId: number, tipoDestinatario: TipoDestinatario): Promise<void> {
    try {
      // Obtener usuarios según el tipo de destinatario
      let queryParam = ""
      switch (tipoDestinatario) {
        case TipoDestinatario.SoloAlumnos:
          queryParam = encodeURIComponent("x=>x.id_perfil==2")
          break
        case TipoDestinatario.SoloPadres:
          queryParam = encodeURIComponent("x=>x.id_perfil==4")
          break
        case TipoDestinatario.Ambos:
        default:
          queryParam = encodeURIComponent("x=>x.id_perfil==2 || x.id_perfil==4")
          break
      }

      const usuariosResponse = await fetch(`${CONFIG.API_BASE_URL}/Usuario/GetUsuariosForCombo?query=${queryParam}`)

      if (!usuariosResponse.ok) {
        throw new Error("Error obteniendo usuarios")
      }

      const usuarios: Usuario[] = await usuariosResponse.json()

      if (usuarios.length === 0) {
        return 
      }

      const integrantesData = usuarios.map(usuario => ({
        id_Evento: eventoId,
        id_Usuario: usuario.id,
        leido: false,
        confirmado: false,
      }))

      const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesEventos/CreateAllIntegrantesEventos`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(integrantesData),
      })

      if (!response.ok) {
        const errorResponse = await response.text()
        throw new Error(`Error al crear integrantes del evento: ${errorResponse}`)
      }
    } catch (error) {
      throw error
    }
  },

// Eliminar integrantes del evento
  async eliminarIntegrantesEvento(eventoId: number): Promise<void> {
    try {
      const integrantes = await this.getIntegrantesEvento(eventoId)

      if (integrantes.length === 0) {
        return
      }

      const integrantesEliminar = integrantes.map(integrante => ({ Id: integrante.id }))

      const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesEventos/DeleteAllIntegrantesEventos`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(integrantesEliminar),
      })

      if (!response.ok) {
        const errorResponse = await response.text()
        throw new Error(`Error al eliminar integrantes del evento: ${errorResponse}`)
      }
    } catch (error) {
      throw error
    }
  },
}

// Función auxiliar para filtrar eventos por perfil
function filtrarEventosPorPerfil(eventos: Evento[], idPerfil: number): Evento[] {
  switch (idPerfil) {
    case 2: // Alumnos
      return eventos.filter(
        (e) => e.tipoDestinatario === TipoDestinatario.SoloAlumnos || e.tipoDestinatario === TipoDestinatario.Ambos,
      )
    case 4: // Padres
      return eventos.filter(
        (e) => e.tipoDestinatario === TipoDestinatario.SoloPadres || e.tipoDestinatario === TipoDestinatario.Ambos,
      )
    default: // Otros perfiles (administradores, docentes)
      return eventos
  }
}
