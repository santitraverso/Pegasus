import type React from "react"
import { View, StyleSheet, ActivityIndicator, Text, SafeAreaView } from "react-native"
import Navbar from "./Navbar"
import { useUser } from "../context/UserContext"
import { useNavigation } from "@react-navigation/native"
import Icon from "react-native-vector-icons/MaterialIcons"

interface AppLayoutProps {
  children: React.ReactNode
  showHomeButton?: boolean
  title?: string
  showLoadingOverlay?: boolean
}

const AppLayout: React.FC<AppLayoutProps> = ({
  children,
  showHomeButton = true,
  title,
  showLoadingOverlay = false,
}) => {
  const { userData, loading, error } = useUser()
  const navigation = useNavigation()

  const handleHomePress = () => {
    // @ts-ignore
    navigation.navigate("Home")
  }

  // Si hay un error crítico, mostrar pantalla de error
  if (error && !userData) {
    return (
      <View style={styles.container}>
        <Navbar userData={null} showHomeButton={false} title="Error" />
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={64} color="#F44336" />
          <Text style={styles.errorTitle}>Error de autenticación</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <Text style={styles.errorSubtext}>La aplicación se cerrará automáticamente para resolver el problema.</Text>
        </View>
      </View>
    )
  }

  // Si está cargando y no hay datos, mostrar pantalla de carga
  if (loading && !userData && showLoadingOverlay) {
    return (
      <View style={styles.container}>
        <Navbar userData={null} showHomeButton={false} title="Cargando..." />
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando datos del usuario...</Text>
        </View>
      </View>
    )
  }

  return (
    <SafeAreaView style={styles.container}>
      <Navbar userData={userData} onHomePress={handleHomePress} showHomeButton={showHomeButton} title={title} />
      <View style={styles.content}>{children}</View>
    </SafeAreaView>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#F5F5F5",
  },
  content: {
    flex: 1,
    paddingBottom: 50,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 20,
  },
  loadingText: {
    marginTop: 16,
    fontSize: 16,
    color: "#666",
    textAlign: "center",
  },
  errorContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 20,
  },
  errorTitle: {
    fontSize: 20,
    fontWeight: "bold",
    color: "#F44336",
    marginTop: 16,
    textAlign: "center",
  },
  errorMessage: {
    fontSize: 16,
    color: "#666",
    textAlign: "center",
    marginTop: 12,
    lineHeight: 22,
  },
  errorSubtext: {
    fontSize: 14,
    color: "#999",
    textAlign: "center",
    marginTop: 16,
    fontStyle: "italic",
  },
  safeButtonContainer: {
    paddingHorizontal: 20,
    paddingVertical: 15,
    paddingBottom: 40, // Espacio extra para botones del sistema
    backgroundColor: "#fff",
    borderTopWidth: 1,
    borderTopColor: "#e0e0e0",
  },
})

export default AppLayout
