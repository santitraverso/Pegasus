import type React from "react"
import { createContext, useState, useContext, type ReactNode } from "react"

// Tipos
interface AuthContextType {
  authError: string | null
  setAuthError: (error: string | null) => void
  clearAuthError: () => void
}

// Crear el contexto
const AuthContext = createContext<AuthContextType | undefined>(undefined)

// Proveedor del contexto
export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [authError, setAuthError] = useState<string | null>(null)

  const clearAuthError = () => {
    setAuthError(null)
  }

  return <AuthContext.Provider value={{ authError, setAuthError, clearAuthError }}>{children}</AuthContext.Provider>
}

// Hook personalizado para usar el contexto
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error("useAuth debe ser usado dentro de un AuthProvider")
  }
  return context
}
