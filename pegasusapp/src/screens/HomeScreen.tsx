import type React from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useUser } from "../context/UserContext"
import type { AppModule } from "../services/userService"
import { useNavigation } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"

const HomeScreen: React.FC = () => {
  const { userData, loading, error, refreshUserData, clearError } = useUser()
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>()

  // Función para renderizar el icono del módulo
  const renderModuleIcon = (iconName: string) => {
    return <Icon name={iconName} size={32} color="#4285F4" />
  }

  const handleModulePress = (module: AppModule) => {

    // Navegación del atributo page del módulo
    try {
      const cleanPage = module.page.startsWith("/") ? module.page.substring(1) : module.page
      // Convertir el page a una ruta válida del navegador
      const routeName = cleanPage as keyof RootStackParamList

      // Verificar si la ruta existe en nuestro navegador
      if (routeName === "ListaCursos") {
        navigation.navigate("ListaCursos", { parametro: module.parametro })
      } else if (routeName === "Curso") {
        navigation.navigate("Cursos")
      } else if (routeName === "Materia") {
        navigation.navigate("Materias")
      } else if (routeName === "Usuario") {
        navigation.navigate("Usuario")
      } else if (routeName === "Evento") {
        navigation.navigate("Evento")
      } else if (routeName === "ListaContacto") {
        navigation.navigate("ListaContacto")
      } else {
        // Para otras rutas que puedan existir
        console.log(`Navegando a: ${routeName}`)
      }
    } catch (error) {
      console.error("Error al navegar:", error)
      console.log("Ruta no encontrada:", module.page)
    }
  }

  const handleRetry = async () => {
    clearError()
    await refreshUserData()
  }

  const renderModuleItem = ({ item, index }: { item: AppModule; index: number }) => {
    // Si es el elemento fantasma, no renderizar nada
    if (item.id === -1) {
      return <View style={{ width: "46%" }} /> // Espacio invisible para mantener la cuadrícula
    }

    return (
      <TouchableOpacity
        style={[
          styles.moduleCard,
          // Forzar que cada tarjeta ocupe exactamente la mitad del ancho
          { width: "46%" }, // 46% para dejar espacio para el margen
        ]}
        onPress={() => handleModulePress(item)}
      >
        {renderModuleIcon(item.icon)}
        <Text style={styles.moduleName}>{item.name}</Text>
      </TouchableOpacity>
    )
  }

  // Contenido principal
  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando datos...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar datos</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={handleRetry}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    // Añadir un elemento "fantasma" si hay un número impar de módulos
    const modulesData = userData?.modules ? [...userData.modules] : []

    if (modulesData.length % 2 !== 0) {
      modulesData.push({ id: -1, name: "", icon: "", page: "" } as AppModule)
    }

    return (
      <View style={styles.contentContainer}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Secciones</Text>
        </View>

        {modulesData.length > 0 ? (
          <FlatList
            data={modulesData}
            renderItem={renderModuleItem}
            keyExtractor={(item) => item.id.toString()}
            numColumns={2}
            columnWrapperStyle={styles.row}
            contentContainerStyle={styles.modulesList}
            showsVerticalScrollIndicator={false}
          />
        ) : (
          <View style={styles.emptyState}>
            <Icon name="info" size={48} color="#CCCCCC" />
            <Text style={styles.emptyStateText}>No tienes módulos asignados</Text>
          </View>
        )}
      </View>
    )
  }

  return (
    <AppLayout showHomeButton={false}>
      <View style={styles.container}>{renderContent()}</View>
    </AppLayout>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#F5F5F5",
  },
  contentContainer: {
    flex: 1,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
    color: "#666",
  },
  errorContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 20,
  },
  errorText: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#F44336",
    marginTop: 12,
  },
  errorMessage: {
    fontSize: 14,
    color: "#666",
    textAlign: "center",
    marginTop: 8,
    lineHeight: 20,
  },
  retryButton: {
    backgroundColor: "#4285F4",
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
    marginTop: 16,
  },
  retryButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
  welcomeSection: {
    marginBottom: 24,
  },
  welcomeText: {
    fontSize: 16,
    color: "#666",
  },
  userName: {
    fontSize: 24,
    fontWeight: "bold",
  },
  header: {
    padding: 16,
    backgroundColor: "#FFFFFF",
    borderBottomWidth: 1,
    borderBottomColor: "#E0E0E0",
  },
  headerTitle: {
    fontSize: 20,
    fontWeight: "bold",
    color: "#333",
  },
  modulesList: {
    padding: 16,
    paddingBottom: 20,
  },
  row: {
    justifyContent: "space-between",
  },
  moduleCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    padding: 16,
    marginBottom: 16,
    alignItems: "center",
    justifyContent: "center",
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    minHeight: 120,
  },
  moduleName: {
    marginTop: 12,
    fontSize: 16,
    fontWeight: "500",
    textAlign: "center",
  },
  emptyState: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  emptyStateText: {
    marginTop: 12,
    fontSize: 16,
    color: "#666",
  },
})

export default HomeScreen
