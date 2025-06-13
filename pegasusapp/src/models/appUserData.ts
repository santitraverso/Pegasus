import { AppModule } from "./appModule"

export interface AppUserData {
  id: number
  email: string
  name: string
  role: string
  modules: AppModule[]
  id_perfil: number
}