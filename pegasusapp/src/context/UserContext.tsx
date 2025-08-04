"use client"

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
        console.log("❌ UserContext: No hay usuario autenticado")
        setUserData(null)
        setLoading(false)
        return
      }

      // Verificar si hay un error de autenticación pendiente
      const pendingError = getLastAuthError()
      if (pendingError) {
        console.log("❌ UserContext: Error de autenticación pendiente:", pendingError)
        setError(pendingError)
        setUserData(null)
        setLoading(false)
        return
      }

      console.log("📱 UserContext: Cargando datos desde cache...")

      try {
        // Intentar cargar datos desde AsyncStorage (ya validados por AppNavigator)
        const data = await getUserData(currentUser.email)
        console.log("✅ UserContext: Datos cargados exitosamente")
        setUserData(data)
      } catch (cacheError: any) {
        console.error("❌ UserContext: Error cargando datos:", cacheError.message)

        // Manejar diferentes tipos de errores
        let errorMessage = "Error al cargar datos del usuario"

        if (cacheError.errorCode === "USER_NOT_FOUND") {
          errorMessage = "Usuario no encontrado. Póngase en contacto con la institución."
        } else if (cacheError.errorCode === "USER_INACTIVE") {
          errorMessage = "Usuario inactivo. Contacte al administrador."
        } else if (cacheError.errorCode === "NO_PROFILE_ASSIGNED") {
          errorMessage = "Usuario sin perfil asignado. Contacte al administrador."
        } else if (cacheError.errorCode === "INVALID_GOOGLE_TOKEN") {
          errorMessage = "Error de autenticación con Google. Intenta nuevamente."
        } else if (cacheError.errorCode === "EMAIL_MISMATCH") {
          errorMessage = "El email de Google no coincide. Verifica tu cuenta."
        } else if (cacheError.errorCode === "INVALID_EMAIL") {
          errorMessage = "El formato del email no es válido."
        } else if (cacheError.errorCode === "MISSING_TOKEN") {
          errorMessage = "Error de autenticación. Intenta nuevamente."
        } else if (cacheError.errorCode === "INTERNAL_ERROR") {
          errorMessage = "Error interno del servidor. Intenta más tarde."
        } else if (cacheError.name === "NO_CACHED_DATA") {
          errorMessage = "No hay datos de usuario cacheados válidos. El usuario debe hacer login nuevamente."
        } else if (cacheError.message) {
          errorMessage = cacheError.message
        }

        setError(errorMessage)
        setUserData(null)
      }
    } catch (error: any) {
      console.error("❌ UserContext: Error inesperado:", error)
      setError("Error inesperado al cargar datos del usuario")
      setUserData(null)
    } finally {
      setLoading(false)
    }
  }

  // Cargar datos del usuario al montar el componente
  useEffect(() => {
    console.log("🚀 UserContext: Iniciando carga de datos del usuario")
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