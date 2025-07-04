import type React from "react"
import { createContext, useState, useContext, useEffect, type ReactNode } from "react"
import { getCurrentUser, getLastAuthError } from "../services/authService"
import { getUserData, type AppUserData } from "../services/userService"
import type { Hijo } from "../models/hijo"
import { CONFIG } from "../services/config"
import AsyncStorage from "@react-native-async-storage/async-storage"

// Tipos
interface UserContextType {
  userData: AppUserData | null
  loading: boolean
  error: string | null
  refreshUserData: () => Promise<void>
  clearError: () => void
  hijos: Hijo[] | null
  hijoSeleccionado: Hijo | null
  seleccionarHijo: (hijo: Hijo) => void
  cargarHijos: () => Promise<void>
}

// Crear el contexto
const UserContext = createContext<UserContextType | undefined>(undefined)

// Proveedor del contexto
export const UserProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [userData, setUserData] = useState<AppUserData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [hijos, setHijos] = useState<Hijo[] | null>(null)
  const [hijoSeleccionado, setHijoSeleccionado] = useState<Hijo | null>(null)

  // Función para detectar si es la primera vez que se instala la app
  const isFirstTimeEver = async (): Promise<boolean> => {
    try {
      const hasEverLoggedIn = await AsyncStorage.getItem("hasEverLoggedIn")
      return hasEverLoggedIn === null
    } catch (error) {
      return true
    }
  }

  const fetchUserData = async () => {
    try {
      setLoading(true)
      setError(null)

      const currentUser = getCurrentUser()
      if (!currentUser || !currentUser.email) {
        setUserData(null)
        setLoading(false)
        return
      }

      // Verificar si hay un error de autenticación pendiente
      const pendingError = getLastAuthError()
      if (pendingError) {
        setError(pendingError)
        setUserData(null)
        setLoading(false)
        return
      }

      // Detectar si es primera vez después de instalar para ajustar delays
      const isFirstEver = await isFirstTimeEver()
      console.log(`⏳ UserContext: ${isFirstEver ? "Primera vez después de instalar" : "Login normal"} detectado`)

      // Delay inicial basado en si es primera vez después de instalar
      if (isFirstEver) {
        console.log("⏳ UserContext: Esperando 8 segundos para establecimiento del cache (primera vez)...")
        await new Promise((resolve) => setTimeout(resolve, 8000))
      } else {
        console.log("⏳ UserContext: Esperando 1 segundo para cache (login normal)...")
        await new Promise((resolve) => setTimeout(resolve, 1000))
      }

      // Configurar reintentos basado en si es primera vez después de instalar
      const maxRetries = isFirstEver ? 6 : 3
      const retryDelay = isFirstEver ? 4000 : 2000

      let retryCount = 0

      while (retryCount < maxRetries) {
        try {
          // Llamar getUserData sin googleToken para usar cache
          const data = await getUserData(currentUser.email)
          setUserData(data)
          return // Salir si fue exitoso
        } catch (cacheError: any) {
          console.log(`❌ Error obteniendo datos (intento ${retryCount + 1}):`, cacheError.message)

          retryCount++

          if (retryCount < maxRetries) {
            console.log(`⏳ Esperando ${retryDelay}ms antes del siguiente intento...`)
            await new Promise((resolve) => setTimeout(resolve, retryDelay))
          } else {
            // Si es el último intento, lanzar el error
            throw cacheError
          }
        }
      }
    } catch (error: any) {
      console.error("❌ Error final en fetchUserData:", error)
      // Manejar diferentes tipos de errores
      let errorMessage = "Error al cargar datos del usuario"

      if (error.errorCode === "USER_NOT_FOUND") {
        errorMessage = "Usuario no encontrado. Póngase en contacto con la institución."
      } else if (error.errorCode === "USER_INACTIVE") {
        errorMessage = "Usuario inactivo. Contacte al administrador."
      } else if (error.errorCode === "NO_PROFILE_ASSIGNED") {
        errorMessage = "Usuario sin perfil asignado. Contacte al administrador."
      } else if (error.errorCode === "INVALID_GOOGLE_TOKEN") {
        errorMessage = "Error de autenticación con Google. Intenta nuevamente."
      } else if (error.errorCode === "EMAIL_MISMATCH") {
        errorMessage = "El email de Google no coincide. Verifica tu cuenta."
      } else if (error.errorCode === "INVALID_EMAIL") {
        errorMessage = "El formato del email no es válido."
      } else if (error.errorCode === "MISSING_TOKEN") {
        errorMessage = "Error de autenticación. Intenta nuevamente."
      } else if (error.errorCode === "INTERNAL_ERROR") {
        errorMessage = "Error interno del servidor. Intenta más tarde."
      } else if (error.name === "NO_CACHED_DATA") {
        errorMessage = "No hay datos de usuario cacheados válidos. El usuario debe hacer login nuevamente."
      } else if (error.message) {
        errorMessage = error.message
      }

      setError(errorMessage)
      setUserData(null)
    } finally {
      setLoading(false)
    }
  }

  // Cargar datos del usuario al montar el componente con delay ajustado
  useEffect(() => {
    const loadUserData = async () => {
      const isFirstEver = await isFirstTimeEver()
      // Delay inicial basado en si es primera vez después de instalar
      const initialDelay = isFirstEver ? 3000 : 500

      const timer = setTimeout(() => {
        fetchUserData()
      }, initialDelay)

      return () => clearTimeout(timer)
    }

    loadUserData()
  }, [])

  const refreshUserData = async () => {
    await fetchUserData()
  }

  const clearError = () => {
    setError(null)
  }

  const cargarHijos = async () => {
    try {
      if (!userData || userData.id_perfil !== 4) {
        setHijos(null)
        return
      }

      const queryParam = encodeURIComponent(`x=>x.id_padre==${userData.id}`)
      const url = `${CONFIG.API_BASE_URL}/Hijo/GetHijosForCombo?query=${queryParam}`
      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })
      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }
      const hijosData: Hijo[] = await response.json()
      setHijos(hijosData || [])

      // Seleccionar automáticamente el primer hijo
      if (hijosData && hijosData.length > 0) {
        setHijoSeleccionado(hijosData[0])
      }
    } catch (error: any) {
      setError(error.message || "Error al cargar los hijos")
    }
  }

  const seleccionarHijo = (hijo: Hijo) => {
    setHijoSeleccionado(hijo)
  }

  // Cargar hijos cuando userData cambie y sea un padre
  useEffect(() => {
    if (userData && userData.id_perfil === 4) {
      cargarHijos()
    } else {
      setHijos(null)
      setHijoSeleccionado(null)
    }
  }, [userData])

  return (
    <UserContext.Provider
      value={{
        userData,
        loading,
        error,
        refreshUserData,
        clearError,
        hijos,
        hijoSeleccionado,
        seleccionarHijo,
        cargarHijos,
      }}
    >
      {children}
    </UserContext.Provider>
  )
}

export const useUser = (): UserContextType => {
  const context = useContext(UserContext)
  if (context === undefined) {
    throw new Error("useUser debe ser usado dentro de un UserProvider")
  }
  return context
}
