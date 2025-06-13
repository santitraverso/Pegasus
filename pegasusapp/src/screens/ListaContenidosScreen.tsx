import type React from "react"
import { useState, useCallback } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, Alert, ActivityIndicator } from "react-native"
import { useFocusEffect, useNavigation, useRoute, type RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { ContenidoMaterias } from "../models/contenidoMaterias"
import { CONFIG } from "../services/config"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"

type ListaContenidosScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "ListaContenidos">
type ListaContenidosScreenRouteProp = RouteProp<RootStackParamList, "ListaContenidos">

const ListaContenidosScreen: React.FC = () => {
  const navigation = useNavigation<ListaContenidosScreenNavigationProp>()
  const route = useRoute<ListaContenidosScreenRouteProp>()
  const { materiaId } = route.params

  const [contenidos, setContenidos] = useState<ContenidoMaterias[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchContenidos = async () => {
    try {
      setLoading(true)
      setError(null)
      const query = encodeURIComponent(`x=>x.id_materia==${materiaId}`)

      const response = await fetch(
        `${CONFIG.API_BASE_URL}/ContenidoMaterias/GetContenidoMateriasForCombo?query=${query}`,
      )

      if (response.ok) {
        const data = await response.json()
        setContenidos(data || [])
      } else {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }
    } catch (error: any) {
      setError(error.message || "Error al cargar los contenidos")
    } finally {
      setLoading(false)
    }
  }

  const handleDeleteContenido = async (contenidoId: number, contenidoTitulo: string) => {
    Alert.alert("¿Estás seguro?", `¿Deseas eliminar el contenido "${contenidoTitulo}"?`, [
      { text: "Cancelar", style: "cancel" },
      {
        text: "Eliminar",
        style: "destructive",
        onPress: async () => {
          try {
            setLoading(true)

            const response = await fetch(
              `${CONFIG.API_BASE_URL}/ContenidoMaterias/DeleteContenidoMaterias/${contenidoId}`,
              { method: "DELETE" }, 
            )

            if (response.ok) {
              Alert.alert("Éxito", "Contenido eliminado correctamente")
              fetchContenidos()
            } else {
              Alert.alert("Error", "No se pudo eliminar el contenido")
              setLoading(false)
            }
          } catch (error) {
            Alert.alert("Error", "Error de conexión al eliminar el contenido")
            setLoading(false)
          }
        },
      },
    ])
  }

  const handleEditContenido = (contenidoId: number) => {
    navigation.navigate("CreateContenidoMateria", { materiaId, contenidoId })
  }

  const handleCreateContenido = () => {
    navigation.navigate("CreateContenidoMateria", { materiaId })
  }

  const handleVolver = () => {
    navigation.navigate("CreateMateria", { materiaId })
  }

  useFocusEffect(
    useCallback(() => {
      fetchContenidos()
    }, [materiaId]),
  )

  const renderContenidoItem = ({ item, index }: { item: ContenidoMaterias; index: number }) => (
    <View style={styles.contenidoCard}>
      <View style={styles.contenidoHeader}>
        <Text style={styles.contenidoTitle}>
          Unidad {index + 1}: {item.titulo}
        </Text>
      </View>

      <View style={styles.contenidoContent}>
        <Text style={styles.contenidoDescription}>{item.descripcion}</Text>
      </View>

      <View style={styles.contenidoActions}>
        <TouchableOpacity
          style={[styles.actionButton, styles.editButton]}
          onPress={() => handleEditContenido(item.id || 0)}
        >
          <Icon name="edit" size={18} color="#4285F4" />
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.actionButton, styles.deleteButton]}
          onPress={() => handleDeleteContenido(item.id || 0, item.titulo || "")}
        >
          <Icon name="delete-outline" size={18} color="#F44336" />
        </TouchableOpacity>
      </View>
    </View>
  )

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando contenidos...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar contenidos</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchContenidos}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (contenidos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="description" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>No hay contenidos disponibles</Text>
        </View>
      )
    }

    return (
      <FlatList
        data={contenidos}
        renderItem={renderContenidoItem}
        keyExtractor={(item) => item.id?.toString() || "0"}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
      />
    )
  }

  return (
    <AppLayout title="Contenidos" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Contenido</Text>
        </View>

        {/* Botón Crear Contenido */}
        <View style={styles.createButtonContainer}>
          <TouchableOpacity style={styles.createContenidoButton} onPress={handleCreateContenido}>
            <Icon name="add" size={20} color="#FFFFFF" />
            <Text style={styles.createContenidoButtonText}>Crear Contenido</Text>
          </TouchableOpacity>
        </View>

        {/* Contenido principal */}
        <View style={styles.contentContainer}>{renderContent()}</View>
      </View>
      <View style={styles.buttonsContainer}>
        <TouchableOpacity style={styles.backButton} onPress={handleVolver}>
          <Icon name="arrow-back" size={16} color="#FFFFFF" />
          <Text style={styles.backButtonText}>Atrás</Text>
        </TouchableOpacity>
      </View>
    </AppLayout>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#F5F5F5",
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
  createButtonContainer: {
    paddingHorizontal: 16,
    paddingVertical: 16,
  },
  createContenidoButton: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#4285F4",
    paddingVertical: 12,
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  createContenidoButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
    marginLeft: 8,
  },
  contentContainer: {
    flex: 1,
    paddingHorizontal: 16,
  },
  listContainer: {
    paddingBottom: 16,
  },
  contenidoCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
    marginBottom: 12,
    elevation: 3,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  contenidoHeader: {
    padding: 16,
    borderBottomWidth: 1,
    borderTopEndRadius: 12,
    borderTopStartRadius: 12,
    borderBottomColor: "#E0E0E0",
    backgroundColor: "#00a9ff",
  },
  contenidoTitle: {
    fontSize: 16,
    fontWeight: "600",
    color: "#fff",
  },
  contenidoContent: {
    padding: 16,
  },
  contenidoDescription: {
    fontSize: 14,
    color: "#666",
    lineHeight: 20,
  },
  contenidoActions: {
    flexDirection: "row",
    justifyContent: "center",
    alignItems: "center",
    padding: 12,
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  actionButton: {
    padding: 8,
    borderRadius: 20,
    marginHorizontal: 8,
  },
  editButton: {
    backgroundColor: "#E3F2FD",
  },
  deleteButton: {
    backgroundColor: "#FFEBEE",
  },
  loadingContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  loadingText: {
    marginTop: 16,
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
  emptyContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  emptyText: {
    fontSize: 16,
    color: "#666",
    marginTop: 16,
    textAlign: "center",
  },
  createButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#4285F4",
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
    marginTop: 16,
  },
  createButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
    marginLeft: 8,
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingHorizontal: 20,
    paddingVertical: 15,
    backgroundColor: "#fff",
    borderTopWidth: 1,
    borderTopColor: "#e0e0e0",
  },
  backButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#6c757d",
    paddingHorizontal: 20,
    paddingVertical: 10,
    borderRadius: 5,
    flex: 1,
    justifyContent: "center",
  },
  backButtonText: {
    color: "#FFFFFF",
    fontWeight: "bold",
    marginLeft: 5,
  },
})

export default ListaContenidosScreen
