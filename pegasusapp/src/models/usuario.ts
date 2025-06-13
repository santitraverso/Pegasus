import type { Calificaciones } from "./calificaciones"
import type { Perfiles } from "./perfiles"

export interface Usuario {
  id?: number | null
  nombre?: string | null
  apellido?: string | null
  id_Perfil?: number | null
  mail?: string | null
  activo: boolean

  calificaciones?: Calificaciones[] | null
  perfil?: Perfiles | null
}
