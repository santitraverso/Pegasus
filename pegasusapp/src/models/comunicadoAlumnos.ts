import { CuadernoComunicados } from './cuadernoComunicados';
import { Usuario } from './usuario';

export interface ComunicadoAlumnos {
  id: number;
  id_Comunicado: number;
  id_Alumno?: number | null;

  comunicado?: CuadernoComunicados | null;
  alumno?: Usuario | null;
}
