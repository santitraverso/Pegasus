import { Modulos } from "./modulos"
import { Perfiles } from "./perfiles"
import { Usuario } from "./usuario"

export interface LoginAppResponse {
  usuario: Usuario
  perfil: Perfiles
  modulos: Modulos[]
  google_id?: string
  verified_email?: boolean
  token?: string
}