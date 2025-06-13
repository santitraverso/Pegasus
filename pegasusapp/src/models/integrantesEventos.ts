import { Usuario } from './usuario';
import { Evento } from './evento';

export interface IntegrantesEventos {
  id: number
  evento?: Evento | null
  id_Evento?: number | null
  usuario?: Usuario | null
  id_Usuario?: number | null
  leido?: boolean
  confirmado?: boolean
}
