import AsyncStorage from "@react-native-async-storage/async-storage"
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
  const controller = new AbortController()
  const timeoutId = setTimeout(() => controller.abort(), 30000)

  try {
    console.log("🌐 Llamando al backend (intento 1/3)...")

    const response = await fetch(`${CONFIG.API_BASE_URL}/account/loginApp`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify({
        email: email,
        googleToken: googleToken,
      }),
      signal: controller.signal,
    })

    clearTimeout(timeoutId)

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
    console.log("✅ Backend respondió exitosamente")

    if (backendData.token) {
      await AsyncStorage.setItem("jwtToken", backendData.token)
    }

    const appData = mapBackendToAppData(backendData)
    return appData
  } catch (error: any) {
    clearTimeout(timeoutId)

    if (error.name === "AbortError") {
      console.error("❌ Timeout: El servidor tardó demasiado en responder")
      const timeoutError: CustomError = new Error("Tiempo de espera agotado. El servidor tardó demasiado en responder.")
      timeoutError.name = "TIMEOUT_ERROR"
      timeoutError.errorCode = "TIMEOUT_ERROR"
      throw timeoutError
    }

    throw error
  }
}

// Constantes para AsyncStorage
const USER_DATA_KEY = "userData"
const CACHE_DURATION = 3 * 60 * 60 * 1000 // 3 horas en milisegundos

interface CachedUserData {
  data: AppUserData
  timestamp: number
  email: string
}

let cachedUserData: CachedUserData | null = null

// Función para guardar datos en AsyncStorage
const saveUserDataToStorage = async (userData: AppUserData, email: string): Promise<void> => {
  try {
    const cacheData: CachedUserData = {
      data: userData,
      timestamp: Date.now(),
      email: email,
    }

    await AsyncStorage.setItem(USER_DATA_KEY, JSON.stringify(cacheData))
    cachedUserData = cacheData
    console.log("✅ Datos del usuario guardados en AsyncStorage")
  } catch (error) {
    console.error("❌ Error guardando datos en AsyncStorage:", error)
  }
}

// Función para obtener datos desde AsyncStorage
const getUserDataFromStorage = async (email: string): Promise<AppUserData | null> => {
  try {
    const storedData = await AsyncStorage.getItem(USER_DATA_KEY)
    if (!storedData) {
      console.log("📱 No hay datos en AsyncStorage")
      return null
    }

    const cacheData: CachedUserData = JSON.parse(storedData)

    if (cacheData.email !== email) {
      console.log("📱 Email no coincide en AsyncStorage, limpiando cache")
      await AsyncStorage.removeItem(USER_DATA_KEY)
      return null
    }

    const now = Date.now()
    if (now - cacheData.timestamp > CACHE_DURATION) {
      console.log("📱 Cache expirado en AsyncStorage, limpiando")
      await AsyncStorage.removeItem(USER_DATA_KEY)
      return null
    }

    cachedUserData = cacheData
    console.log("✅ Datos válidos recuperados desde AsyncStorage")
    return cacheData.data
  } catch (error) {
    console.error("❌ Error leyendo datos desde AsyncStorage:", error)
    return null
  }
}

export const hasValidCachedData = async (email: string): Promise<boolean> => {
  try {
    const userData = await getUserDataFromStorage(email)
    return userData !== null
  } catch (error) {
    return false
  }
}

const getUserDataReal = async (email: string, googleToken?: string): Promise<AppUserData> => {
  try {
    if (googleToken) {
      console.log("🔐 Login inicial con token de Google")
      const userData = await loginUserWithModules(email, googleToken)
      await saveUserDataToStorage(userData, email)
      console.log("✅ Proceso de login completado exitosamente")
      return userData
    } else {
      console.log("📱 Verificando cache para usuario autenticado")

      if (cachedUserData && cachedUserData.email === email) {
        const now = Date.now()
        if (now - cachedUserData.timestamp < CACHE_DURATION) {
          console.log("✅ Datos válidos encontrados en cache de memoria")
          return cachedUserData.data
        }
      }

      const userData = await getUserDataFromStorage(email)
      if (userData) {
        return userData
      }

      const error = new Error("No hay datos de usuario cacheados válidos. El usuario debe hacer login nuevamente.")
      error.name = "NO_CACHED_DATA"
      throw error
    }
  } catch (error) {
    throw error
  }
}

export const clearUserDataCache = async (): Promise<void> => {
  try {
    cachedUserData = null
    await AsyncStorage.removeItem(USER_DATA_KEY)
    console.log("🗑️ Cache de usuario limpiado completamente")
  } catch (error) {
    console.error("❌ Error limpiando cache:", error)
  }
}

export const forceRefreshUserData = async (email: string): Promise<AppUserData | null> => {
  try {
    await clearUserDataCache()
    return null
  } catch (error) {
    return null
  }
}

export const getUserData = async (email: string, googleToken?: string): Promise<AppUserData> => {
  return getUserDataReal(email, googleToken)
}

export type { AppUserData, AppModule }
export { mapBackendToAppData, getModuleIcon }