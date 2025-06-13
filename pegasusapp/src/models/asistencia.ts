import { Curso } from './curso';
import { Materia } from './materia';
import { Usuario } from './usuario';

export interface Asistencia {
  id: number;
  id_Alumno?: number | null;
  id_Materia?: number | null;
  id_Curso?: number | null;
  fecha?: string | null; // ISO 8601 string format, e.g., "2025-05-19T00:00:00Z"
  presente: boolean;

  alumno?: Usuario | null;
  materia?: Materia | null;
  curso?: Curso | null;
}
