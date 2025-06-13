import type React from "react"
import { View, Text, StyleSheet, ActivityIndicator } from "react-native"
import type { SplashScreenProps } from "../models/splashScreenProps"

const SplashScreen: React.FC<SplashScreenProps> = ({ message = "Cargando..." }) => {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Pegasus App</Text>
      <ActivityIndicator size="large" color="#4285F4" style={styles.loader} />
      <Text style={styles.message}>{message}</Text>
      <Text style={styles.submessage}>
        {message.includes("Validando") ? "Verificando credenciales y cargando perfil..." : ""}
      </Text>
    </View>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "#FFFFFF",
  },
  title: {
    fontSize: 24,
    fontWeight: "bold",
    marginBottom: 20,
    color: "#4285F4",
  },
  loader: {
    marginTop: 20,
  },
  message: {
    fontSize: 16,
    color: "#666",
    marginTop: 20,
    textAlign: "center",
  },
  submessage: {
    fontSize: 14,
    color: "#999",
    marginTop: 10,
    textAlign: "center",
    fontStyle: "italic",
  },
})

export default SplashScreen
