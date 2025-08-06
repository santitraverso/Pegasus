import { getApp } from "@react-native-firebase/app"
import {
  getAuth,
  signInWithCredential,
  signOut as firebaseSignOut,
  onAuthStateChanged,
  GoogleAuthProvider,
} from "@react-native-firebase/auth"
import { GoogleSignin, type SignInResponse } from "@react-native-google-signin/google-signin"
import { getUserData, clearUserDataCache } from "./userService"
import AsyncStorage from "@react-native-async-storage/async-storage"

// Configurar Google Sign-In
GoogleSignin.configure({
  webClientId: "173127890311-qt9g2he90bvducq13psij627e9lidg6j.apps.googleusercontent.com",
  offlineAccess: false,
})

// Variable global para almacenar el último error de autenticación
let lastAuthError: string | null = null

// Variable global para controlar si hay un login en progreso
let isLoginInProgress = false

export const getLastAuthError = () => {
  return lastAuthError
}

export const clearLastAuthError = () => {
  lastAuthError = null
}

export const setLoginInProgress = (inProgress: boolean) => {
  isLoginInProgress = inProgress
  console.log(`🔄 Login en progreso: ${inProgress}`)
}

export const getLoginInProgress = () => {
  return isLoginInProgress
}

// Función para detectar si es la primera vez que se instala la app
const isFirstTimeEver = async (): Promise<boolean> => {
  try {
    const hasEverLoggedIn = await AsyncStorage.getItem("hasEverLoggedIn")
    return hasEverLoggedIn === null
  } catch (error) {
    return true
  }
}

// Función para marcar que ya se hizo login por primera vez
const markFirstLoginComplete = async (): Promise<void> => {
  try {
    await AsyncStorage.setItem("hasEverLoggedIn", "true")
  } catch (error) {
  }
}

// Función helper para cerrar sesión completa
const forceSignOut = async () => {
  try {
    console.log("🚪 Iniciando cierre de sesión completo...")
    
    const app = getApp()
    const auth = getAuth(app)
    const currentUser = auth.currentUser

    if (currentUser) {
      console.log("🔥 Cerrando sesión en Firebase...")
      await firebaseSignOut(auth)
    }

    try {
      console.log("🔄 Revocando acceso de Google...")
      await GoogleSignin.revokeAccess()
      await GoogleSignin.signOut()
    } catch (googleError) {
      console.log("⚠️ Error cerrando Google (puede ser normal):", googleError)
    }

    // IMPORTANTE: Limpiar cache de AsyncStorage
    console.log("🗑️ Limpiando cache de AsyncStorage...")
    await clearUserDataCache()
    
    console.log("✅ Cierre de sesión completo terminado")
  } catch (signOutError) {
    console.error("❌ Error en forceSignOut:", signOutError)
    // Aún así intentar limpiar cache
    try {
      await clearUserDataCache()
    } catch (cacheError) {
      console.error("❌ Error limpiando cache:", cacheError)
    }
  }
}

// Función mejorada para obtener tokens con reintentos
const getTokenWithRetry = async (isAddingNewAccount: boolean, maxRetries = 3): Promise<string> => {
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      // Delay progresivo para cuentas nuevas
      if (isAddingNewAccount && attempt > 1) {
        const delay = attempt * 3000 // 3s, 6s, 9s
        console.log(`⏳ Esperando ${delay}ms antes del intento ${attempt} (cuenta nueva)...`)
        await new Promise((resolve) => setTimeout(resolve, delay))
      } else if (!isAddingNewAccount && attempt > 1) {
        const delay = attempt * 1000 // 1s, 2s, 3s
        console.log(`⏳ Esperando ${delay}ms antes del intento ${attempt} (cuenta existente)...`)
        await new Promise((resolve) => setTimeout(resolve, delay))
      }

      const tokens = await GoogleSignin.getTokens()

      if (tokens.idToken) {
        console.log(`✅ Token obtenido exitosamente en intento ${attempt}`)
        return tokens.idToken
      } else {
        throw new Error("Token vacío recibido")
      }
    } catch (error: any) {
      console.log(`❌ Error en intento ${attempt}:`, error.message)
      if (attempt === maxRetries) {
        const errorMessage = error?.message || "Error desconocido obteniendo token"
        throw new Error(`No se pudo obtener el ID token después de ${maxRetries} intentos: ${errorMessage}`)
      }

      // Para cuentas nuevas, esperar más tiempo entre intentos
      if (isAddingNewAccount) {
        console.log("⏳ Esperando 2 segundos adicionales para cuenta nueva...")
        await new Promise((resolve) => setTimeout(resolve, 2000))
      }
    }
  }

  throw new Error("Error inesperado obteniendo token")
}

// Función helper para hacer retry del backend con delay ajustado
const callBackendWithRetry = async (
  userEmail: string,
  idToken: string,
  retryCount = 0,
  isAddingNewAccount = false,
): Promise<any> => {
  const maxRetries = 3 
  const baseDelay = isAddingNewAccount ? 5000 : 2000 // Aumentado el delay base para cuentas nuevas

  try {
    console.log(`🌐 Llamando backend (intento ${retryCount + 1}/${maxRetries + 1}) - ${isAddingNewAccount ? 'CUENTA NUEVA' : 'CUENTA EXISTENTE'}...`)
    const userData = await getUserData(userEmail, idToken)
    console.log("✅ Backend respondió exitosamente")
    return userData
  } catch (backendError: any) {
    console.log(`❌ Error en backend (intento ${retryCount + 1}):`, backendError.message)
    console.log(`❌ Código de error:`, backendError.errorCode)
    
    // Si es INTERNAL_ERROR y no agotamos los reintentos
    if (
      retryCount < maxRetries &&
      (backendError.errorCode === "INTERNAL_ERROR" ||
        backendError.status >= 500 ||
        backendError.message?.includes("network") ||
        backendError.message?.includes("timeout") ||
        backendError.message?.includes("USER_NOT_FOUND"))
    ) {
      const delay = baseDelay * (retryCount + 1)
      console.log(`⏳ Esperando ${delay}ms antes del siguiente intento (${isAddingNewAccount ? 'cuenta nueva' : 'cuenta existente'})...`)
      await new Promise((resolve) => setTimeout(resolve, delay))
      return callBackendWithRetry(userEmail, idToken, retryCount + 1, isAddingNewAccount)
    }

    throw backendError
  }
}

// Función para refrescar token automáticamente
export const refreshUserToken = async (): Promise<string | null> => {
  try {
    console.log("🔄 Intentando refrescar token de Google automáticamente...")

    // Verificar si hay usuario logueado intentando obtener tokens
    try {
      const tokens = await GoogleSignin.getTokens()
      if (tokens.idToken) {
        console.log("✅ Token refrescado exitosamente")
        return tokens.idToken
      }
    } catch (error: any) {
      console.log("❌ No hay usuario logueado en Google o error obteniendo tokens")
      return null
    }

    return null
  } catch (error: any) {
    console.error("❌ Error refrescando token:", error.message)
    return null
  }
}

export const signInWithGoogle = async () => {
  try {
    console.log("🚀 Iniciando proceso de login con Google...")
    
    // MARCAR LOGIN EN PROGRESO
    setLoginInProgress(true)
    
    // Limpiar error anterior
    clearLastAuthError()

    // Detectar si es la primera vez EVER (después de instalar la app)
    const isFirstEver = await isFirstTimeEver()
    console.log("📱 Primera vez instalando app:", isFirstEver)

    await GoogleSignin.hasPlayServices({ showPlayServicesUpdateDialog: true })

    // Configuración para detectar acción del usuario
    const USER_ACTION_THRESHOLD = 9000 // 9 segundos
    const signInTimeout = 60000 // 60 segundos timeout general

    const signInStartTime = Date.now()
    let signInResult: SignInResponse
    let isAddingNewAccount = false

    try {
      console.log("👆 Esperando selección del usuario...")
      // Hacer el signIn con timeout
      const signInPromise = GoogleSignin.signIn()
      const timeoutPromise = new Promise<never>((_, reject) =>
        setTimeout(() => reject(new Error("SIGNIN_TIMEOUT")), signInTimeout),
      )

      signInResult = await Promise.race([signInPromise, timeoutPromise])

      const signInTime = Date.now() - signInStartTime
      console.log(`⏱️ Tiempo de selección: ${signInTime}ms`)

      // Detectar acción del usuario por tiempo de respuesta
      if (signInTime <= USER_ACTION_THRESHOLD) {
        // Selección rápida = Usuario tocó una cuenta del listado
        isAddingNewAccount = false
        console.log("⚡ Cuenta existente seleccionada")
      } else {
        // Selección lenta = Usuario tocó "Agregar otra cuenta"
        isAddingNewAccount = true
        console.log("🆕 Nueva cuenta agregada - APLICANDO DELAYS EXTENDIDOS")
      }
    } catch (error: any) {
      if (error.message === "SIGNIN_TIMEOUT") {
        throw new Error("SIGNIN_TIMEOUT_EXTENDED")
      }
      throw error
    }

    // DELAYS AJUSTADOS SEGÚN LA ACCIÓN DEL USUARIO
    if (isAddingNewAccount) {
      // Agregar otra cuenta - tiempo extendido
      console.log("⏳ Esperando 10 segundos para nueva cuenta (delay inicial)...")
      await new Promise((resolve) => setTimeout(resolve, 10000)) // Aumentado a 10 segundos
    } else {
      // Cuenta del listado - delay mínimo
      console.log("⏳ Esperando 800ms para cuenta existente...")
      await new Promise((resolve) => setTimeout(resolve, 800)) // 0.8 segundos
    }

    // Obtener idToken con manejo mejorado
    let idToken = signInResult.data?.idToken

    if (!idToken) {
      console.log("🔑 Token no encontrado en respuesta, obteniendo con reintentos...")
      try {
        // Usar la función mejorada con reintentos
        idToken = await getTokenWithRetry(isAddingNewAccount)
      } catch (tokenError: any) {
        const errorMessage = tokenError?.message || "Error desconocido obteniendo token"
        throw new Error(`Error obteniendo token de Google: ${errorMessage}`)
      }
    }

    if (!idToken) {
      throw new Error("No se pudo obtener el ID token de Google después de múltiples intentos")
    }

    console.log("🔥 Autenticando con Firebase...")
    // Autenticar con Firebase
    const app = getApp()
    const auth = getAuth(app)
    const googleCredential = GoogleAuthProvider.credential(idToken)
    const userCredential = await signInWithCredential(auth, googleCredential)

    const userEmail = userCredential.user.email

    if (!userEmail) {
      throw new Error("No se pudo obtener el email del usuario de Firebase")
    }

    console.log("👤 Usuario autenticado:", userEmail)

    try {
      // Delay antes de llamar al backend - MÁS TIEMPO PARA CUENTAS NUEVAS
      if (isAddingNewAccount) {
        console.log("⏳ Esperando 8 segundos antes de llamar backend (nueva cuenta)...")
        await new Promise((resolve) => setTimeout(resolve, 8000)) // Aumentado a 8 segundos
      } else {
        console.log("⏳ Esperando 500ms antes de llamar backend (cuenta existente)...")
        await new Promise((resolve) => setTimeout(resolve, 500)) // Aumentado ligeramente
      }

      // Llamar al backend con retry
      const userData = await callBackendWithRetry(userEmail, idToken, 0, isAddingNewAccount)

      // Asegurar que los datos se guarden antes de marcar como completado
      console.log("💾 Asegurando que los datos estén guardados...")
      
      // Verificar que los datos realmente se guardaron
      let dataVerified = false
      let verificationAttempts = 0
      const maxVerificationAttempts = 5
      
      while (!dataVerified && verificationAttempts < maxVerificationAttempts) {
        try {
          const { hasValidCachedData } = await import('./userService')
          const hasData = await hasValidCachedData(userEmail)
          if (hasData) {
            dataVerified = true
            console.log("✅ Datos verificados en AsyncStorage")
          } else {
            verificationAttempts++
            console.log(`⏳ Intento ${verificationAttempts}: Datos aún no disponibles, esperando...`)
            await new Promise((resolve) => setTimeout(resolve, 500))
          }
        } catch (error) {
          verificationAttempts++
          console.log(`❌ Error verificando datos (intento ${verificationAttempts}):`, error)
          await new Promise((resolve) => setTimeout(resolve, 500))
        }
      }
      
      if (!dataVerified) {
        console.log("⚠️ No se pudo verificar que los datos se guardaron, pero continuando...")
      }

      // Pausa final
      if (isAddingNewAccount) {
        console.log("⏳ Pausa final de 2 segundos (cuenta nueva)...")
        await new Promise((resolve) => setTimeout(resolve, 2000))
      } else {
        console.log("⏳ Pausa final de 500ms (cuenta existente)...")
        await new Promise((resolve) => setTimeout(resolve, 500))
      }

      // Marcar primera vez solo si aplica
      if (isFirstEver) {
        await markFirstLoginComplete()
      }

      console.log("✅ Proceso de login completado exitosamente")
      
      // MARCAR LOGIN COMPLETADO
      setLoginInProgress(false)

      return {
        firebaseUser: userCredential.user,
        userData: userData,
      }
    } catch (backendError: any) {
      console.error("❌ Error en validación con backend:", backendError)
      
      let errorMessage = "Error desconocido al validar usuario"

      if (backendError.errorCode === "USER_NOT_FOUND") {
        errorMessage = backendError.serverMessage || "Usuario no encontrado. Póngase en contacto con la institución."
      } else if (backendError.errorCode === "USER_INACTIVE") {
        errorMessage = backendError.serverMessage || "Usuario inactivo. Contacte al administrador."
      } else if (backendError.errorCode === "NO_PROFILE_ASSIGNED") {
        errorMessage = backendError.serverMessage || "Usuario sin perfil asignado. Contacte al administrador."
      } else if (backendError.errorCode === "INTERNAL_ERROR") {
        errorMessage =
          "Error interno del servidor. Si el problema persiste después de varios intentos, contacte al soporte técnico."
      } else if (backendError.message) {
        errorMessage = backendError.message
      }

      lastAuthError = errorMessage
      
      // MARCAR LOGIN COMPLETADO (incluso en error)
      setLoginInProgress(false)
      
      await forceSignOut()
      throw backendError
    }
  } catch (error: any) {
    console.error("❌ Error general en signInWithGoogle:", error)
    
    // MARCAR LOGIN COMPLETADO (incluso en error)
    setLoginInProgress(false)
    
    if (!lastAuthError) {
      let errorMessage = "Error desconocido al iniciar sesión"

      if (error.code === "auth/network-request-failed") {
        errorMessage = "Error de conexión. Verifica tu conexión a internet."
      } else if (error.code === "SIGN_IN_CANCELLED") {
        errorMessage = "Login cancelado por el usuario."
      } else if (error.code === "IN_PROGRESS") {
        errorMessage = "Ya hay un proceso de login en curso. Espera un momento."
      } else if (error.code === "PLAY_SERVICES_NOT_AVAILABLE") {
        errorMessage = "Google Play Services no disponible."
      } else if (error.message === "SIGNIN_TIMEOUT_EXTENDED") {
        errorMessage =
          "El proceso de login tardó demasiado tiempo. Si estás agregando una cuenta nueva, esto es normal. Intenta nuevamente."
      } else if (error.message?.includes("getTokens requires a user to be signed in")) {
        errorMessage = "Error de autenticación con Google. Intenta cerrar la app y volver a abrirla."
      } else if (error.message) {
        errorMessage = error.message
      }

      lastAuthError = errorMessage
      await forceSignOut()
    }

    throw error
  }
}

export const signOut = async () => {
  try {
    console.log("🚪 Iniciando signOut...")
    clearLastAuthError()
    setLoginInProgress(false) // Limpiar estado de login
    await forceSignOut()
    console.log("✅ SignOut completado")
  } catch (error) {
    console.error("❌ Error en signOut:", error)
    throw error
  }
}

export const getCurrentUser = () => {
  const app = getApp()
  const auth = getAuth(app)
  return auth.currentUser
}

export const subscribeToAuthChanges = (callback: (user: any) => void) => {
  const app = getApp()
  const auth = getAuth(app)
  return onAuthStateChanged(auth, callback)
}