import { Materia } from './materia';

export interface Contenido {
  id: number;
  id_Materia?: number | null;
  nombre: string;
  descripcion: string;
  tipo_Contenido: number;

  materia?: Materia | null;
}
