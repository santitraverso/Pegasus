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

export const getLastAuthError = () => {
  return lastAuthError
}

export const clearLastAuthError = () => {
  lastAuthError = null
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
    // Ignorar error
  }
}

// Función helper para cerrar sesión completa
const forceSignOut = async () => {
  try {
    const app = getApp()
    const auth = getAuth(app)
    const currentUser = auth.currentUser

    if (currentUser) {
      await firebaseSignOut(auth)
    }

    try {
      await GoogleSignin.revokeAccess()
      await GoogleSignin.signOut()
    } catch (googleError) {
      // Ignorar errores de Google si no hay usuario
    }

    clearUserDataCache()
  } catch (signOutError) {
    // Ignorar errores
  }
}

// Función mejorada para obtener tokens con reintentos
const getTokenWithRetry = async (isAddingNewAccount: boolean, maxRetries = 3): Promise<string> => {
  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      // Delay progresivo para cuentas nuevas
      if (isAddingNewAccount && attempt > 1) {
        const delay = attempt * 3000 // 3s, 6s, 9s
        await new Promise((resolve) => setTimeout(resolve, delay))
      } else if (!isAddingNewAccount && attempt > 1) {
        const delay = attempt * 1000 // 1s, 2s, 3s
        await new Promise((resolve) => setTimeout(resolve, delay))
      }

      const tokens = await GoogleSignin.getTokens()

      if (tokens.idToken) {
        return tokens.idToken
      } else {
        throw new Error("Token vacío recibido")
      }
    } catch (error: any) {
      if (attempt === maxRetries) {
        const errorMessage = error?.message || "Error desconocido obteniendo token"
        throw new Error(`No se pudo obtener el ID token después de ${maxRetries} intentos: ${errorMessage}`)
      }

      // Para cuentas nuevas, esperar más tiempo entre intentos
      if (isAddingNewAccount) {
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
  const maxRetries = 2
  const baseDelay = isAddingNewAccount ? 4000 : 2000
  const timeout = isAddingNewAccount ? 50000 : 30000

  try {
    const userData = await getUserData(userEmail, idToken)
    return userData
  } catch (backendError: any) {
    // Si es INTERNAL_ERROR y no hemos agotado los reintentos
    if (
      retryCount < maxRetries &&
      (backendError.errorCode === "INTERNAL_ERROR" ||
        backendError.status >= 500 ||
        backendError.message?.includes("network") ||
        backendError.message?.includes("timeout"))
    ) {
      const delay = baseDelay * (retryCount + 1)
      await new Promise((resolve) => setTimeout(resolve, delay))
      return callBackendWithRetry(userEmail, idToken, retryCount + 1, isAddingNewAccount)
    }

    throw backendError
  }
}

export const signInWithGoogle = async () => {
  try {
    // Limpiar error anterior
    clearLastAuthError()

    // Detectar si es la primera vez EVER (después de instalar la app)
    const isFirstEver = await isFirstTimeEver()

    await GoogleSignin.hasPlayServices({ showPlayServicesUpdateDialog: true })

    // Configuración para detectar acción del usuario
    const USER_ACTION_THRESHOLD = 8000 // 6 segundos
    const signInTimeout = 60000 // 60 segundos timeout general

    const signInStartTime = Date.now()
    let signInResult: SignInResponse
    let isAddingNewAccount = false

    try {
      // Hacer el signIn con timeout
      const signInPromise = GoogleSignin.signIn()
      const timeoutPromise = new Promise<never>((_, reject) =>
        setTimeout(() => reject(new Error("SIGNIN_TIMEOUT")), signInTimeout),
      )

      signInResult = await Promise.race([signInPromise, timeoutPromise])

      const signInTime = Date.now() - signInStartTime

      // Detectar acción del usuario por tiempo de respuesta
      if (signInTime <= USER_ACTION_THRESHOLD) {
        // Selección rápida = Usuario tocó una cuenta del listado
        isAddingNewAccount = false
      } else {
        // Selección lenta = Usuario tocó "Agregar otra cuenta"
        isAddingNewAccount = true
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
      await new Promise((resolve) => setTimeout(resolve, 10000)) // 10 segundos
    } else {
      // Cuenta del listado - delay mínimo
      await new Promise((resolve) => setTimeout(resolve, 800)) // 0.8 segundos
    }

    // Obtener idToken con manejo mejorado
    let idToken = signInResult.data?.idToken

    if (!idToken) {
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

    // Autenticar con Firebase
    const app = getApp()
    const auth = getAuth(app)
    const googleCredential = GoogleAuthProvider.credential(idToken)
    const userCredential = await signInWithCredential(auth, googleCredential)

    const userEmail = userCredential.user.email

    if (!userEmail) {
      throw new Error("No se pudo obtener el email del usuario de Firebase")
    }

    try {
      // Delay antes de llamar al backend
      if (isAddingNewAccount) {
        await new Promise((resolve) => setTimeout(resolve, 4000)) // 4 segundos para cuentas nuevas
      } else {
        await new Promise((resolve) => setTimeout(resolve, 300)) // 0.3 segundos para cuentas existentes
      }

      // Llamar al backend con retry
      const userData = await callBackendWithRetry(userEmail, idToken, 0, isAddingNewAccount)

      // Pausa final
      if (isAddingNewAccount) {
        await new Promise((resolve) => setTimeout(resolve, 2000))
      } else {
        await new Promise((resolve) => setTimeout(resolve, 200))
      }

      // Marcar primera vez solo si aplica
      if (isFirstEver) {
        await markFirstLoginComplete()
      }

      return {
        firebaseUser: userCredential.user,
        userData: userData,
      }
    } catch (backendError: any) {
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
      await forceSignOut()
      throw backendError
    }
  } catch (error: any) {
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
    clearLastAuthError()
    await forceSignOut()
  } catch (error) {
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
