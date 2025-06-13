import { Materia } from './materia';
import { Usuario } from './usuario';

export interface IntegrantesMaterias {
  id: number;

  materia?: Materia | null;
  id_Materia?: number | null;

  usuario?: Usuario | null;
  id_Usuario?: number | null;
}
