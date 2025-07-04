import { getApp } from "@react-native-firebase/app"
import {
  getAuth,
  signInWithCredential,
  signOut as firebaseSignOut,
  onAuthStateChanged,
  GoogleAuthProvider,
} from "@react-native-firebase/auth"
import { GoogleSignin } from "@react-native-google-signin/google-signin"
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
    console.log("Error checking first time ever:", error)
    return true
  }
}

// Función para marcar que ya se hizo login por primera vez
const markFirstLoginComplete = async (): Promise<void> => {
  try {
    await AsyncStorage.setItem("hasEverLoggedIn", "true")
  } catch (error) {
    console.log("Error marking first login complete:", error)
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
    // NO borramos "hasEverLoggedIn" porque eso es solo para la primera instalación
  } catch (signOutError) {
    console.error("❌ AuthService: Error cerrando sesión tras fallo:", signOutError)
  }
}

// Función helper para hacer retry del backend con delay
const callBackendWithRetry = async (userEmail: string, idToken: string, retryCount = 0): Promise<any> => {
  const maxRetries = 2
  const baseDelay = 2000 // 2 segundos base

  try {
    console.log(`🌐 Llamando al backend (intento ${retryCount + 1}/${maxRetries + 1})...`)
    const userData = await getUserData(userEmail, idToken)
    console.log("✅ Backend respondió exitosamente")
    return userData
  } catch (backendError: any) {
    console.error(`❌ Error en backend (intento ${retryCount + 1}):`, {
      errorCode: backendError.errorCode,
      message: backendError.message,
      serverMessage: backendError.serverMessage,
      status: backendError.status,
    })

    // Si es INTERNAL_ERROR y no hemos agotado los reintentos
    if (
      retryCount < maxRetries &&
      (backendError.errorCode === "INTERNAL_ERROR" ||
        backendError.status >= 500 ||
        backendError.message?.includes("network") ||
        backendError.message?.includes("timeout"))
    ) {
      const delay = baseDelay * (retryCount + 1) // Incrementar delay
      console.log(`🔄 Reintentando en ${delay}ms...`)
      await new Promise((resolve) => setTimeout(resolve, delay))
      return callBackendWithRetry(userEmail, idToken, retryCount + 1)
    }

    // Si no es un error que amerite retry, o ya agotamos los intentos
    throw backendError
  }
}

export const signInWithGoogle = async () => {
  try {
    // Limpiar error anterior
    clearLastAuthError()

    // Detectar si es la primera vez EVER (después de instalar la app)
    const isFirstEver = await isFirstTimeEver()
    console.log(`🔍 Tipo de login: ${isFirstEver ? "PRIMERA VEZ DESPUÉS DE INSTALAR" : "LOGIN NORMAL"}`)

    console.log("🔍 Verificando Google Play Services...")
    await GoogleSignin.hasPlayServices({ showPlayServicesUpdateDialog: true })

    console.log("🔑 Iniciando sesión con Google...")
    const signInResult = await GoogleSignin.signIn()

    console.log("✅ Google Sign-In exitoso:", {
      email: signInResult.data?.user?.email,
      name: signInResult.data?.user?.name,
      hasIdToken: !!signInResult.data?.idToken,
      hasUser: !!signInResult.data?.user,
    })

    // DELAY SOLO PARA LA PRIMERA VEZ DESPUÉS DE INSTALAR
    if (isFirstEver) {
      console.log("⏳ PRIMERA VEZ DESPUÉS DE INSTALAR - Esperando 5 segundos para Google Play Services...")
      await new Promise((resolve) => setTimeout(resolve, 5000))
    } else {
      console.log("⚡ LOGIN NORMAL - Sin delay adicional")
    }

    // Obtener idToken directamente del resultado del signIn
    let idToken = signInResult.data?.idToken

    // Si no está disponible en el resultado, intentar obtenerlo con getTokens
    if (!idToken) {
      console.log("🎫 ID Token no disponible en signIn result, obteniendo con getTokens...")
      try {
        // Pausa solo para primera vez después de instalar
        if (isFirstEver) {
          console.log("⏳ Primera vez - Esperando 3 segundos antes de obtener tokens...")
          await new Promise((resolve) => setTimeout(resolve, 3000))
        } else {
          console.log("⏳ Esperando 500ms antes de obtener tokens...")
          await new Promise((resolve) => setTimeout(resolve, 500))
        }

        const tokens = await GoogleSignin.getTokens()
        idToken = tokens.idToken
        console.log("✅ Token obtenido via getTokens")
      } catch (tokenError) {
        console.error("❌ Error obteniendo tokens:", tokenError)

        // Para primera vez después de instalar, intentar una vez más con más delay
        if (isFirstEver) {
          console.log("🔄 Primera vez - Reintentando obtener tokens con delay adicional...")
          await new Promise((resolve) => setTimeout(resolve, 3000))
          try {
            const tokens = await GoogleSignin.getTokens()
            idToken = tokens.idToken
            console.log("✅ Token obtenido en segundo intento")
          } catch (secondTokenError) {
            console.error("❌ Error en segundo intento de tokens:", secondTokenError)
            throw new Error(
              "No se pudo obtener el ID token de Google después de múltiples intentos. Intenta nuevamente.",
            )
          }
        } else {
          throw new Error("No se pudo obtener el ID token de Google. Intenta nuevamente.")
        }
      }
    }

    if (!idToken) {
      throw new Error("No se pudo obtener el ID token de Google")
    }

    console.log("🔥 Autenticando con Firebase...")
    const app = getApp()
    const auth = getAuth(app)
    const googleCredential = GoogleAuthProvider.credential(idToken)
    const userCredential = await signInWithCredential(auth, googleCredential)

    const userEmail = userCredential.user.email

    if (!userEmail) {
      throw new Error("No se pudo obtener el email del usuario de Firebase")
    }

    console.log("✅ Firebase autenticación exitosa para:", userEmail)

    try {
      console.log("🌐 Validando usuario con el backend...")

      // Delay antes de llamar al backend solo para primera vez después de instalar
      if (isFirstEver) {
        console.log("⏳ Primera vez - Esperando 3 segundos antes de validar con backend...")
        await new Promise((resolve) => setTimeout(resolve, 3000))
      }

      // Usar la función con retry
      const userData = await callBackendWithRetry(userEmail, idToken)

      console.log("✅ Proceso de login completado exitosamente")

      // Pausa final solo para primera vez después de instalar
      if (isFirstEver) {
        console.log("⏳ Primera vez - Esperando 2 segundos para establecer cache...")
        await new Promise((resolve) => setTimeout(resolve, 2000))
        await markFirstLoginComplete()
        console.log("✅ Primera vez después de instalar completada")
      } else {
        await new Promise((resolve) => setTimeout(resolve, 200))
      }

      return {
        firebaseUser: userCredential.user,
        userData: userData,
      }
    } catch (backendError: any) {
      console.error("❌ Error final del backend después de reintentos:", backendError)

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
    console.error("❌ Error en signInWithGoogle:", {
      code: error.code,
      message: error.message,
      errorCode: error.errorCode,
    })

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
