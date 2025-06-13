import { Materia } from './materia';

export interface ContenidoMaterias {
  id: number;
  id_Materia?: number | null;

  titulo: string;
  descripcion: string;

  materia?: Materia | null;
}
