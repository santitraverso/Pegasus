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
    console.error("❌ AuthService: Error cerrando sesión tras fallo:", signOutError)
  }
}

export const signInWithGoogle = async () => {
  try {
    // Limpiar error anterior
    clearLastAuthError()

    console.log("🔍 Verificando Google Play Services...")
    await GoogleSignin.hasPlayServices({ showPlayServicesUpdateDialog: true })

    console.log("🔑 Iniciando sesión con Google...")
    const signInResult = await GoogleSignin.signIn()
    console.log("✅ Google Sign-In exitoso para:", signInResult.data?.user?.email)

    // Obtener idToken directamente del resultado del signIn
    let idToken = signInResult.data?.idToken

    // Si no está disponible en el resultado, intentar obtenerlo con getTokens
    if (!idToken) {
      console.log("🎫 ID Token no disponible en signIn result, obteniendo con getTokens...")
      try {
        const tokens = await GoogleSignin.getTokens()
        idToken = tokens.idToken
      } catch (tokenError) {
        console.error("❌ Error obteniendo tokens:", tokenError)
        throw new Error("No se pudo obtener el ID token de Google. Intenta nuevamente.")
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
      const userData = await getUserData(userEmail, idToken)

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
    console.error("❌ Error en signInWithGoogle:", error)

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
