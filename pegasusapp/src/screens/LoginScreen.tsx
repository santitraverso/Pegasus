import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, TouchableOpacity, Image, ActivityIndicator } from "react-native"
import { signInWithGoogle, getLastAuthError, clearLastAuthError } from "../services/authService"

const LoginScreen: React.FC = () => {
  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  // Verificar si hay un error guardado al montar el componente
  useEffect(() => {
    const savedError = getLastAuthError()
    if (savedError) {
      setErrorMessage(savedError)
      clearLastAuthError() // Limpiar después de usar
    }
  }, [])

  useEffect(() => {
    const checkForErrors = () => {
      const savedError = getLastAuthError()
      if (savedError && savedError !== errorMessage) {
        setErrorMessage(savedError)
        clearLastAuthError()
      }
    }

    // Verificar inmediatamente
    checkForErrors()

    // Verificar periódicamente por si hay errores nuevos
    const interval = setInterval(checkForErrors, 1000)

    return () => clearInterval(interval)
  }, [errorMessage])

  const handleGoogleSignIn = async () => {
    try {
      setLoading(true)
      setErrorMessage(null) 
      clearLastAuthError()

      await signInWithGoogle()
    } catch (error: any) {

      // Mostrar mensaje específico según el tipo de error
      let displayMessage = "No se pudo iniciar sesión. Inténtalo de nuevo."

      if (error.errorCode === "USER_NOT_FOUND") {
        displayMessage = error.message || "Usuario no encontrado. Póngase en contacto con la institución."
      } else if (error.errorCode === "USER_INACTIVE") {
        displayMessage = error.message || "Usuario inactivo. Contacte al administrador."
      } else if (error.errorCode === "NO_PROFILE_ASSIGNED") {
        displayMessage = error.message || "Usuario sin perfil asignado. Contacte al administrador."
      } else if (error.errorCode === "INVALID_GOOGLE_TOKEN") {
        displayMessage = error.message || "Error de autenticación. Intenta nuevamente."
      } else if (error.errorCode === "EMAIL_MISMATCH") {
        displayMessage = error.message || "Error de validación. Intenta nuevamente."
      } else if (error.message) {
        displayMessage = error.message
      }

      setErrorMessage(displayMessage)
    } finally {
      setLoading(false)
    }
  }

  return (
    <View style={styles.container}>
      <View style={styles.logoContainer}>
        <Text style={styles.appName}>Pegasus App</Text>
        <Image 
          source={require('../images/pegasus.png')} 
          style={styles.logoImage}
          resizeMode="contain"
        />
        <Text style={styles.subtitle}>Inicia sesión para continuar</Text>
      </View>

      {errorMessage && (
        <View style={styles.errorContainer}>
          <Text style={styles.errorText}>{errorMessage}</Text>
        </View>
      )}

      <View style={styles.buttonContainer}>
        <TouchableOpacity onPress={handleGoogleSignIn} disabled={loading} style={styles.imageButtonContainer}>
          {loading ? (
            <View style={styles.loadingContainer}>
              <ActivityIndicator size="small" color="#4285F4" />
              <Text style={styles.loadingText}>Iniciando sesión...</Text>
            </View>
          ) : (
            <Image
              source={require("../images/botonCompleto.png")}
              style={styles.googleButtonImage}
              resizeMode="contain"
            />
          )}
        </TouchableOpacity>
      </View>
    </View>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "#FFFFFF",
    padding: 20,
  },
  logoContainer: {
    alignItems: "center",
    marginBottom: 50,
  },
  appName: {
    fontSize: 28,
    fontWeight: "bold",
    marginBottom: 10,
  },
  subtitle: {
    fontSize: 16,
    color: "#666",
  },
  errorContainer: {
    backgroundColor: "#FFEBEE",
    borderColor: "#F44336",
    borderWidth: 1,
    borderRadius: 8,
    padding: 16,
    marginBottom: 20,
    width: "100%",
  },
  errorText: {
    color: "#C62828",
    fontSize: 14,
    textAlign: "center",
    lineHeight: 20,
    marginBottom: 8,
  },
  clearErrorButton: {
    alignSelf: "center",
    paddingHorizontal: 12,
    paddingVertical: 6,
    backgroundColor: "#F44336",
    borderRadius: 4,
  },
  clearErrorText: {
    color: "#FFFFFF",
    fontSize: 12,
    fontWeight: "600",
  },
  buttonContainer: {
    width: "100%",
  },
  googleButton: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#4285F4",
    borderRadius: 4,
    paddingVertical: 12,
    paddingHorizontal: 24,
    elevation: 3,
  },
  googleIcon: {
    width: 24,
    height: 24,
    marginRight: 12,
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
  },
  buttonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
  imageButtonContainer: {
    borderRadius: 25,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 3 },
    shadowOpacity: 0.4,
    shadowRadius: 6,
    elevation: 8,
    alignSelf: "center",
  },

  googleButtonImage: {
    width: 250,
    height: 50,
    borderRadius: 25,
  },

  loadingContainer: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#f1f3f4",
    paddingHorizontal: 20,
    paddingVertical: 12,
    borderRadius: 25,
    width: 250,
    height: 50,
    borderWidth: 1,
    borderColor: "#dadce0",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.25,
    shadowRadius: 3.84,
    elevation: 5,
  },

  loadingText: {
    color: "#5f6368",
    marginLeft: 10,
    fontSize: 16,
    fontWeight: "500",
  },
  logoImage: {
    width: 120,
    height: 120,
    marginBottom: 20,
  },
})

export default LoginScreen
