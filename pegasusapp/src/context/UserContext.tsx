import type React from "react"
import { createContext, useState, useContext, useEffect, type ReactNode } from "react"
import { getCurrentUser, getLastAuthError } from "../services/authService"
import { getUserData, type AppUserData } from "../services/userService"
import type { Hijo } from "../models/hijo"
import { CONFIG } from "../services/config"

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

      // Para el primer login, intentar obtener datos con más tiempo de espera
      let retryCount = 0
      const maxRetries = 3
      const retryDelay = 2000 // 2 segundos entre reintentos

      while (retryCount < maxRetries) {
        try {
          const data = await getUserData(currentUser.email)
          setUserData(data)
          return // Salir si fue exitoso
        } catch (cacheError: any) {

          // Si es el primer intento y no hay datos cacheados, esperar más tiempo
          if (retryCount === 0 && cacheError.message.includes("datos de usuario cacheados")) {
            await new Promise((resolve) => setTimeout(resolve, 4000))
          } else if (retryCount < maxRetries - 1) {
            await new Promise((resolve) => setTimeout(resolve, retryDelay))
          }

          retryCount++

          // Si es el último intento, lanzar el error
          if (retryCount === maxRetries) {
            throw cacheError
          }
        }
      }
    } catch (error: any) {
      // Manejar diferentes tipos de errores
      let errorMessage = "Error al cargar datos del usuario"

      if (error.errorCode === "USER_NOT_FOUND") {
        errorMessage = "Usuario no encontrado. Contacte al administrador."
      } else if (error.errorCode === "USER_INACTIVE") {
        errorMessage = "Usuario inactivo. Contacte al administrador."
      } else if (error.errorCode === "NO_PROFILE_ASSIGNED") {
        errorMessage = "Usuario sin perfil asignado. Contacte al administrador."
      } else if (error.message) {
        errorMessage = error.message
      }

      setError(errorMessage)
      setUserData(null)
    } finally {
      setLoading(false)
    }
  }

  // Cargar datos del usuario al montar el componente
  useEffect(() => {
    fetchUserData()
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
