import { Usuario } from './usuario';
import { Curso } from './curso';

export interface IntegrantesCursos {
  id: number;

  curso?: Curso | null;
  id_Curso?: number | null;

  usuario?: Usuario | null;
  id_Usuario?: number | null;
}
