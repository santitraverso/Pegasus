export enum TipoDestinatario {
  SoloAlumnos = 1,
  SoloPadres = 2,
  Ambos = 3,
}

export interface Evento {
  id?: number | null
  nombre?: string | null
  descripcion?: string | null
  fecha: string
  requiereConfirmacion?: boolean
  tipoDestinatario?: TipoDestinatario
}
