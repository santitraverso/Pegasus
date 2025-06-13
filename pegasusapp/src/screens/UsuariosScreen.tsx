import type React from "react"
import { useState, useEffect, useCallback } from "react"
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  ActivityIndicator,
  TextInput,
  Alert,
  RefreshControl,
} from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useUser } from "../context/UserContext"
import { useNavigation, useFocusEffect } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { Usuario } from "../models/usuario"
import { CONFIG } from "../services/config"

type NavigationProp = NativeStackNavigationProp<RootStackParamList, "Usuario">

const UsuariosScreen: React.FC = () => {
  const [usuarios, setUsuarios] = useState<Usuario[]>([])
  const [loading, setLoading] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [searchText, setSearchText] = useState("")
  const [filteredUsuarios, setFilteredUsuarios] = useState<Usuario[]>([])
  const { userData, hijos, hijoSeleccionado, seleccionarHijo, cargarHijos } = useUser()
  const [error, setError] = useState<string | null>(null)

  const navigation = useNavigation<NavigationProp>()

  const isParentProfile = userData?.id_perfil === 4

  // Función para obtener usuarios
  const fetchUsuarios = async () => {
    try {
      setError(null)
      let fetchedUsuarios: Usuario[] = []

      if (isParentProfile) {
        // Para padres, usar los hijos del contexto
        if (hijos && hijos.length > 0) {
          fetchedUsuarios = hijos.map((hijo) => hijo.hijoUsuario).filter(Boolean) as Usuario[]
        } else {
          // Si no hay hijos cargados, intentar cargarlos
          await cargarHijos()
          return // La función se ejecutará nuevamente cuando se actualice el contexto
        }
      } else {
        // Para otros perfiles, obtener todos los usuarios

        let url = `${CONFIG.API_BASE_URL}/Usuario/GetUsuariosForCombo`

        // Si el usuario actual no es admin, filtrar en la consulta
        const isCurrentUserAdmin = userData?.id_perfil === 1
        if (!isCurrentUserAdmin) {
          const query = encodeURIComponent("x=>x.id_perfil!=1")
          url += `?query=${query}`
        }

        const response = await fetch(url)

        if (response.ok) {
          fetchedUsuarios = await response.json()
        } else {
          throw new Error("Error al obtener usuarios")
        }
      }

      setUsuarios(fetchedUsuarios)
      setFilteredUsuarios(fetchedUsuarios)
    } catch (error) {
      setError(error instanceof Error ? error.message : "Error desconocido")
    } finally {
      setLoading(false)
      setRefreshing(false)
    }
  }

  // Cargar datos al montar el componente y al volver a la pantalla
  useFocusEffect(
    useCallback(() => {
      setLoading(true)
      fetchUsuarios()
    }, [isParentProfile, userData?.id, hijos, cargarHijos]),
  )

  // Función para refrescar
  const onRefresh = () => {
    setRefreshing(true)
    fetchUsuarios()
  }

  // Función para filtrar usuarios
  useEffect(() => {
    if (searchText.trim() === "") {
      setFilteredUsuarios(usuarios)
    } else {
      const filtered = usuarios.filter(
        (usuario) =>
          usuario.nombre?.toLowerCase().includes(searchText.toLowerCase()) ||
          usuario.apellido?.toLowerCase().includes(searchText.toLowerCase()),
      )
      setFilteredUsuarios(filtered)
    }
  }, [searchText, usuarios])

  // Función para eliminar usuario
  const handleDeleteUsuario = (usuarioId: number) => {
    Alert.alert("¿Estás seguro?", "¿Deseas eliminar este usuario?", [
      {
        text: "Cancelar",
        style: "cancel",
      },
      {
        text: "Eliminar",
        style: "destructive",
        onPress: async () => {
          try {
            const response = await fetch(
              `${CONFIG.API_BASE_URL}/Usuario/DeleteUsuario/${usuarioId}`,
              {
                method: "DELETE",
              },
            )

            if (response.ok) {
              Alert.alert("Éxito", "Usuario eliminado correctamente")
              fetchUsuarios() // Recargar la lista
            } else {
              Alert.alert("Error", "Hubo un error al eliminar el usuario")
            }
          } catch (error) {
            Alert.alert("Error", "Hubo un error inesperado al eliminar el usuario")
          }
        },
      },
    ])
  }

  // Función para editar usuario
  const handleEditUsuario = (usuarioId: number) => {
    navigation.navigate("CreateUsuario", { usuarioId })
  }

  // Función para crear nuevo usuario
  const handleCreateUsuario = () => {
    navigation.navigate("CreateUsuario", {})
  }

  // Función para seleccionar hijo (solo para padres)
  const handleSelectHijo = async (hijoId: number) => {
    try {
      const hijoEncontrado = hijos?.find((h) => h.hijoUsuario?.id === hijoId)
      if (hijoEncontrado) {
        seleccionarHijo(hijoEncontrado)
        Alert.alert("Éxito", "Hijo seleccionado correctamente", [
          {
            text: "OK",
            onPress: () => navigation.navigate("Home"),
          },
        ])
      }
    } catch (error) {
      Alert.alert("Error", "Hubo un error al seleccionar el hijo")
    }
  }

  const selectedHijoId = hijoSeleccionado?.hijoUsuario?.id || null

  // Renderizar item de usuario para padres
  const renderHijoItem = ({ item }: { item: Usuario }) => (
    <TouchableOpacity
      style={[styles.hijoCard, selectedHijoId === item.id && styles.hijoCardSelected]}
      onPress={() => handleSelectHijo(item.id || 0)}
    >
      <View style={styles.hijoCardContent}>
        <View style={styles.radioContainer}>
          <View style={[styles.radioButton, selectedHijoId === item.id && styles.radioButtonSelected]}>
            {selectedHijoId === item.id && <View style={styles.radioButtonInner} />}
          </View>
        </View>
        <View style={styles.hijoInfo}>
          <Text style={styles.hijoName}>{`${item.apellido || ""} ${item.nombre || ""}`}</Text>
          <Text style={styles.hijoDetail}>Estudiante</Text>
        </View>
        <View style={styles.hijoIconContainer}>
          <Icon name="person" size={24} color="#4285F4" />
        </View>
      </View>
    </TouchableOpacity>
  )

  // Renderizar item de usuario normal
  const renderUsuarioItem = ({ item }: { item: Usuario }) => (
    <View style={styles.userCard}>
      <View style={styles.userInfo}>
        <Text style={styles.userName}>{`${item.apellido || ""} ${item.nombre || ""}`}</Text>
        <Text style={styles.userDetail}>Perfil: {item.perfil?.nombre || "N/A"}</Text>
        <Text style={styles.userDetail}>Estado: {item.activo ? "Activo" : "Inactivo"}</Text>
        <Text style={styles.userDetail}>Email: {item.mail || "N/A"}</Text>
      </View>
      <View style={styles.actionsContainer}>
        <TouchableOpacity style={styles.editButton} onPress={() => handleEditUsuario(item.id || 0)}>
          <Icon name="edit" size={20} color="#4285F4" />
        </TouchableOpacity>
        <TouchableOpacity style={styles.deleteButton} onPress={() => handleDeleteUsuario(item.id || 0)}>
          <Icon name="delete-outline" size={20} color="#F44336" />
        </TouchableOpacity>
      </View>
    </View>
  )

  if (loading) {
    return (
      <AppLayout>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando usuarios...</Text>
        </View>
      </AppLayout>
    )
  }

  if (error) {
    return (
      <AppLayout>
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar usuarios</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchUsuarios}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>{isParentProfile ? "Seleccionar Hijo" : "Usuarios"}</Text>
        </View>

        {isParentProfile && (
          <View style={styles.instructionContainer}>
            <Icon name="info-outline" size={20} color="#4285F4" />
            <Text style={styles.instructionText}>Selecciona el hijo para ver su información académica</Text>
          </View>
        )}

        {!isParentProfile && (
          <View style={styles.searchContainer}>
            <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
            <TextInput
              style={styles.searchInput}
              placeholder="Buscar usuarios..."
              value={searchText}
              onChangeText={setSearchText}
            />
          </View>
        )}

        {!isParentProfile && (
          <View style={styles.createButtonContainer}>
            <TouchableOpacity style={styles.createButton} onPress={handleCreateUsuario}>
              <Icon name="add" size={20} color="#FFFFFF" />
              <Text style={styles.createButtonText}>Crear Usuario</Text>
            </TouchableOpacity>
          </View>
        )}

        <FlatList
          data={filteredUsuarios}
          renderItem={isParentProfile ? renderHijoItem : renderUsuarioItem}
          keyExtractor={(item) => item.id?.toString() || "0"}
          contentContainerStyle={styles.listContainer}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
          ListEmptyComponent={
            <View style={styles.emptyContainer}>
              <Icon name="people-outline" size={48} color="#CCCCCC" />
              <Text style={styles.emptyText}>No se encontraron usuarios</Text>
            </View>
          }
        />
      </View>
    </AppLayout>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#F5F5F5",
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
  searchContainer: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FFFFFF",
    margin: 16,
    paddingHorizontal: 12,
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  searchIcon: {
    marginRight: 8,
  },
  searchInput: {
    flex: 1,
    paddingVertical: 12,
    fontSize: 16,
  },
  createButtonContainer: {
    paddingHorizontal: 16,
    marginBottom: 8,
  },
  createButton: {
    backgroundColor: "#4285F4",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 12,
    borderRadius: 8,
  },
  createButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
    marginLeft: 8,
  },
  listContainer: {
    padding: 16,
  },
  userCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    padding: 16,
    marginBottom: 12,
    flexDirection: "row",
    alignItems: "center",
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  radioContainer: {
    marginRight: 12,
  },
  radioButton: {
    width: 20,
    height: 20,
    borderRadius: 10,
    borderWidth: 2,
    borderColor: "#4285F4",
    alignItems: "center",
    justifyContent: "center",
  },
  radioButtonSelected: {
    backgroundColor: "#4285F4",
  },
  radioButtonInner: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: "#FFFFFF",
  },
  userInfo: {
    flex: 1,
  },
  userName: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    marginBottom: 4,
  },
  userDetail: {
    fontSize: 14,
    color: "#666",
    marginBottom: 2,
  },
  actionsContainer: {
    flexDirection: "row",
    gap: 7,
  },
  editButton: {
    backgroundColor: "#E3F2FD",
    width: 40,
    height: 40,
    borderRadius: 20,
    justifyContent: "center",
    alignItems: "center",
  },
  deleteButton: {
    backgroundColor: "#FFEBEE",
    width: 40,
    height: 40,
    borderRadius: 20,
    justifyContent: "center",
    alignItems: "center",
  },
  emptyContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    paddingTop: 50,
  },
  emptyText: {
    marginTop: 12,
    fontSize: 16,
    color: "#666",
  },
  saveButtonContainer: {
    padding: 16,
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  saveButton: {
    backgroundColor: "#4285F4",
    paddingVertical: 12,
    borderRadius: 8,
    alignItems: "center",
  },
  saveButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
  hijoCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
    marginBottom: 12,
    elevation: 3,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 6,
    borderWidth: 2,
    borderColor: "transparent",
  },
  hijoCardSelected: {
    borderColor: "#4285F4",
    backgroundColor: "#F8F9FF",
  },
  hijoCardContent: {
    flexDirection: "row",
    alignItems: "center",
    padding: 16,
  },
  hijoInfo: {
    flex: 1,
    marginLeft: 12,
  },
  hijoName: {
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
    marginBottom: 4,
  },
  hijoDetail: {
    fontSize: 14,
    color: "#666",
  },
  hijoIconContainer: {
    backgroundColor: "#E3F2FD",
    width: 48,
    height: 48,
    borderRadius: 24,
    justifyContent: "center",
    alignItems: "center",
  },
  instructionContainer: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#E3F2FD",
    margin: 16,
    padding: 12,
    borderRadius: 8,
    marginBottom: 8,
  },
  instructionText: {
    fontSize: 14,
    color: "#1976D2",
    marginLeft: 8,
    flex: 1,
  },
})

export default UsuariosScreen
