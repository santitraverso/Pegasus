import { Curso } from './curso';
import { Usuario } from './usuario';

export interface CuadernoComunicados {
  id: number;
  id_Usuario?: number | null;
  id_Curso?: number | null;

  descripcion: string;
  fecha?: string | null;

  usuario?: Usuario | null;
  curso?: Curso | null;
}
