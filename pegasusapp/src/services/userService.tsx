import AsyncStorage from '@react-native-async-storage/async-storage';
import type { AppModule } from "../models/appModule"
import type { AppUserData } from "../models/appUserData"
import type { CustomError } from "../models/customError"
import type { LoginAppResponse } from "../models/loginResponse"
import { CONFIG } from "./config"


// ==================== MAPEO DE ICONOS ====================
const getModuleIcon = (moduleName: string): string => {
  const iconMap: { [key: string]: string } = {
    dashboard: "dashboard",
    usuarios: "people",
    contactos: "contact-page",
    configuracion: "settings",
    reportes: "bar-chart",
    auditoria: "security",
    calificaciones: "grade",
    estudiantes: "school",
    profesores: "person",
    cursos: "school",
    materias: "subject",
    notas: "assignment",
    asistencia: "event-available",
    horarios: "schedule",
    comunicaciones: "message",
    biblioteca: "local-library",
    finanzas: "account-balance",
    inventario: "inventory",
    mantenimiento: "build",
    seguridad: "security",
    backup: "backup",
    eventos: "event",
    desempeño: "analytics",
    cuaderno: "message",
  }

  const normalizedName = moduleName.toLowerCase().replace(/\s+/g, "")
  return iconMap[normalizedName] || "apps"
}

const mapBackendToAppData = (backendData: LoginAppResponse): AppUserData => {
  // Mapear módulos del backend al formato de la app
  const appModules: AppModule[] = backendData.modulos.map((modulo) => ({
    id: modulo.id,
    name: modulo.modulo,
    icon: getModuleIcon(modulo.modulo),
    page: modulo.page,
    parametro: modulo.parametro || undefined,
  }))

  return {
    id: backendData.usuario.id || 0,
    email: backendData.usuario.mail || "",
    name: `${backendData.usuario.nombre || ""} ${backendData.usuario.apellido || ""}`.trim(),
    role: backendData.perfil.nombre,
    modules: appModules,
    id_perfil: backendData.usuario.id_Perfil || 0,
  }
}

const loginUserWithModules = async (email: string, googleToken: string): Promise<AppUserData> => {
  try {

    const response = await fetch(`${CONFIG.API_BASE_URL}/account/loginApp`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        email: email,
        googleToken: googleToken,
      }),
    })

    if (!response.ok) {
      const errorData = await response.json()
      const error: CustomError = new Error(errorData.message || "Error desconocido")
      error.name = errorData.errorCode || "UNKNOWN_ERROR"
      error.status = response.status
      error.errorCode = errorData.errorCode
      error.serverMessage = errorData.message

      throw error
    }

    const backendData: LoginAppResponse = await response.json()

    if (backendData.token) {
      await AsyncStorage.setItem('jwtToken', backendData.token);
    }

    // Mapear datos del backend al formato de la app
    const appData = mapBackendToAppData(backendData)

    return appData
  } catch (error) {
    if (
      process.env.NODE_ENV === "development" &&
      error instanceof TypeError &&
      error.message.includes("Network request failed")
    ) {
      console.error("1. Verifica que tu API esté corriendo en:", CONFIG.API_BASE_URL)
      console.error("2. Verifica que el endpoint /account/loginApp exista")
      console.error("3. Verifica la configuración de CORS")
    }

    throw error
  }
}

// Variable global para cachear los datos del usuario con timestamp
let cachedUserData: { data: AppUserData; timestamp: number; email: string } | null = null
const CACHE_DURATION = 5 * 60 * 1000 // 5 minutos en milisegundos

// Función para verificar si el cache es válido
const isCacheValid = (email: string): boolean => {
  if (!cachedUserData) return false
  if (cachedUserData.email !== email) return false

  const now = Date.now()
  const isValid = now - cachedUserData.timestamp < CACHE_DURATION

  if (!isValid) {
    cachedUserData = null
  }

  return isValid
}

// Función principal para obtener datos del usuario
const getUserDataReal = async (email: string, googleToken?: string): Promise<AppUserData> => {
  try {

    if (googleToken) {
      // Caso 1: Login inicial con token - obtener todo en una llamada
      const userData = await loginUserWithModules(email, googleToken)

      // Cachear los datos para uso posterior con timestamp
      cachedUserData = {
        data: userData,
        timestamp: Date.now(),
        email: email,
      }

      return userData
    } else {
      // Caso 2: Usuario ya autenticado, verificar cache

      if (isCacheValid(email)) {
        return cachedUserData!.data
      } else {

        // Crear un error específico para indicar que no hay cache
        const error = new Error("No hay datos de usuario cacheados válidos. El usuario debe hacer login nuevamente.")
        error.name = "NO_CACHED_DATA"
        throw error
      }
    }
  } catch (error) {
    throw error
  }
}


// limpiar el cache
export const clearUserDataCache = () => {
  cachedUserData = null
}

// refresh del cache
export const forceRefreshUserData = async (email: string): Promise<AppUserData | null> => {
  try {

    clearUserDataCache()

    // Intentar obtener datos frescos (esto requerirá un nuevo login)
    return null
  } catch (error) {
    return null
  }
}

// Función principal exportada
export const getUserData = async (email: string, googleToken?: string): Promise<AppUserData> => {
  return getUserDataReal(email, googleToken)
}

// Exportar tipos y funciones auxiliares
export type { AppUserData, AppModule }
export { mapBackendToAppData, getModuleIcon }
