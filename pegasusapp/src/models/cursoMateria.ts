import { Materia } from './materia';
import { Curso } from './curso';

export interface CursoMateria {
  id: number;
  id_Materia?: number | null;
  id_Curso?: number | null;

  materia?: Materia | null;
  curso?: Curso | null;
}
