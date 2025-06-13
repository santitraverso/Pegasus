import { Perfiles } from './perfiles';
import { Modulos } from './modulos';

export interface ModulosPerfiles {
  id: number;

  modulo?: Modulos | null;
  id_Modulo?: number | null;

  perfil?: Perfiles | null;
  id_Perfil?: number | null;
}
