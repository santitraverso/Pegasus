import { Usuario } from './usuario';
import { Curso } from './curso';
import { Desempenio } from './desempenio';

export interface DesempenioAlumnos {
  id: number;

  alumno?: Usuario | null;
  id_Alumno?: number | null;

  id_Curso?: number | null;
  curso?: Curso | null;

  asistencia: number;
  participacion: number;
  tareas: number;
  calificaciones: number;
  promedio: number;

  desempenio?: Desempenio | null;
}
