import { Materia } from './materia';
import { Usuario } from './usuario';
import { Curso } from './curso';

export interface DocenteMateria {
  id: number;
  id_Materia?: number | null;
  id_Docente?: number | null;
  id_Curso?: number | null;

  materia?: Materia | null;
  docente?: Usuario | null;
  curso?: Curso | null;
}
