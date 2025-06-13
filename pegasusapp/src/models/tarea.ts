import { Usuario } from './usuario';
import { Materia } from './materia';

export interface Tarea {
  id: number;
  titulo: string;
  descripcion: string;
  fecha_Entrega?: string | null;
  entregado?: boolean | null;
  calificacion?: number | null;

  materia?: Materia | null;
  id_Materia?: number | null;

  alumno?: Usuario | null;
  id_Alumno?: number | null;
}
