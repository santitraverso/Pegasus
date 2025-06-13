import { Usuario } from './usuario';

export interface Hijo {
  id: number;

  padre?: Usuario | null;
  id_Padre?: number | null;

  hijoUsuario?: Usuario | null;
  id_Hijo?: number | null;
}
