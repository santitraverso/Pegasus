import type { Usuario } from "./usuario"
import type { Calificaciones } from "./calificaciones"

export interface AlumnoConCalificaciones {
  id_Usuario: number
  id_Materia: number
  usuario: Usuario
  calificaciones: Calificaciones[]
}
