import type { Materia } from "./materia"
import type { Usuario } from "./usuario"

export interface Calificaciones {
  id: number
  id_Alumno?: number | null
  id_Materia?: number | null
  calificacion: number // Byte en C# se traduce a number en TS (0-255)

  materia?: Materia | null
  usuario?: Usuario | null
}
