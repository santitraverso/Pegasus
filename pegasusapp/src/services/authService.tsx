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

// Función helper para cerrar sesión completa
const forceSignOut = async () => {
  try {
    const app = getApp()
    const auth = getAuth(app)
    await firebaseSignOut(auth)
    await GoogleSignin.revokeAccess()
    await GoogleSignin.signOut()
    // Limpiar cache de datos de usuario
    clearUserDataCache()
  } catch (signOutError) {
    console.error("❌ AuthService: Error cerrando sesión tras fallo:", signOutError)
  }
}

export const signInWithGoogle = async () => {
  try {
    // Limpiar error anterior
    clearLastAuthError()

    // 1. Verificar Google Play Services
    await GoogleSignin.hasPlayServices({ showPlayServicesUpdateDialog: true })

    // 2. Hacer sign in con Google
    const signInResult = await GoogleSignin.signIn()

    // 3. Obtener tokens
    const tokens = await GoogleSignin.getTokens()
    const { idToken } = tokens

    if (!idToken) {
      throw new Error("No se pudo obtener el ID token de Google")
    }

    // 4. Autenticación con Firebase 
    const app = getApp()
    const auth = getAuth(app)
    const googleCredential = GoogleAuthProvider.credential(idToken)
    const userCredential = await signInWithCredential(auth, googleCredential)

    const userEmail = userCredential.user.email

    if (!userEmail) {
      throw new Error("No se pudo obtener el email del usuario de Firebase")
    }

    // 5. Validar usuario en el backend CON el token de Google

    try {
      const userData = await getUserData(userEmail, idToken)

      return {
        firebaseUser: userCredential.user,
        userData: userData,
      }
    } catch (backendError: any) {
      // Si hay error del backend, guardar el error y cerrar sesión

      let errorMessage = "Error desconocido al validar usuario"

      if (backendError.errorCode === "USER_NOT_FOUND") {
        errorMessage = backendError.serverMessage || "Usuario no encontrado. Póngase en contacto con la institución."
      } else if (backendError.errorCode === "USER_INACTIVE") {
        errorMessage = backendError.serverMessage || "Usuario inactivo. Contacte al administrador."
      } else if (backendError.errorCode === "NO_PROFILE_ASSIGNED") {
        errorMessage = backendError.serverMessage || "Usuario sin perfil asignado. Contacte al administrador."
      } else if (backendError.serverMessage) {
        errorMessage = backendError.serverMessage
      } else if (backendError.message) {
        errorMessage = backendError.message
      }

      lastAuthError = errorMessage

      await forceSignOut()
      throw backendError
    }
  } catch (error: any) {

    // Si no es un error del backend, manejar otros tipos de errores
    if (!lastAuthError) {
      let errorMessage = "Error desconocido al iniciar sesión"

      if (error.code === "auth/network-request-failed") {
        errorMessage = "Error de conexión. Verifica tu conexión a internet."
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

    // Limpiar error guardado
    clearLastAuthError()

    // Revocar acceso de Google
    await GoogleSignin.revokeAccess()

    // Cerrar sesión en Firebase usando la nueva API modular
    const app = getApp()
    const auth = getAuth(app)
    await firebaseSignOut(auth)

    // Limpiar cache de datos de usuario
    clearUserDataCache()

  } catch (error) {
    throw error
  }
}

export const getCurrentUser = () => {
  const app = getApp()
  const auth = getAuth(app)
  return auth.currentUser
}

// Exportar función para suscribirse a cambios de autenticación
export const subscribeToAuthChanges = (callback: (user: any) => void) => {
  const app = getApp()
  const auth = getAuth(app)
  return onAuthStateChanged(auth, callback)
}
