import type React from "react"
import { useState, useEffect, useContext } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, Alert, ActivityIndicator, TextInput } from "react-native"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RouteProp } from "@react-navigation/native"
import type { RootStackParamList } from "../navigation/AppNavigator"
import AppLayout from "../components/AppLayout"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"
import type { Contactos } from "../models/contactos"
import Icon from "react-native-vector-icons/MaterialIcons"


type NavigationProp = NativeStackNavigationProp<RootStackParamList, "Contactos">
type RouteProps = RouteProp<RootStackParamList, "Contactos">

const ContactosScreen: React.FC = () => {
  const navigation = useNavigation<NavigationProp>()
  const route = useRoute<RouteProps>()
  const { tipoContacto } = route.params
  const { userData } = useUser()

  const [contactos, setContactos] = useState<Contactos[]>([])
  const [loading, setLoading] = useState(true)

  const puedeEditar = userData?.id_perfil !== 2 && userData?.id_perfil !== 4
  const [searchText, setSearchText] = useState("")
  const [filteredCursos, setFilteredContactos] = useState<Contactos[]>([])
  
  

  useEffect(() => {
    loadContactos()
  }, [tipoContacto])

  useEffect(() => {
      // Filtrar contactos cuando cambia el texto de búsqueda
      if (searchText.trim() === "") {
        setFilteredContactos(contactos)
      } else {
        const filtered = contactos.filter((contacto) => {
          const nombreContacto = contacto.nombre?.toLowerCase() || ""
          const apellidoContacto = contacto.apellido?.toLowerCase() || ""
          const searchLower = searchText.toLowerCase()
  
          return nombreContacto.includes(searchLower) || apellidoContacto.includes(searchLower)
        })
        setFilteredContactos(filtered)
      }
    }, [searchText, contactos])


  const loadContactos = async () => {
    try {
      setLoading(true)
      const queryParam = encodeURIComponent(`x=>x.tipo_contacto==${tipoContacto}`)
      const response = await fetch(`${CONFIG.API_BASE_URL}/Contactos/GetContactosForCombo?query=${queryParam}`)

      if (response.ok) {
        const data = await response.json()
        setContactos(data)
      } else {
        Alert.alert("Error", "No se pudieron cargar los contactos")
      }
    } catch (error) {
      Alert.alert("Error", "No se pudieron cargar los contactos")
    } finally {
      setLoading(false)
    }
  }

  const handleVolver = () => {
    // Navegar específicamente a ListaContactos
    navigation.navigate("ListaContacto")
  }

  const handleEditContacto = (contactoId: number) => {
    navigation.navigate("CreateContacto", { tipoContacto, contactoId })
  }

  const handleDeleteContacto = (contacto: Contactos) => {
    Alert.alert("¿Estás seguro?", `¿Deseas eliminar el contacto ${contacto.nombre}?`, [
      {
        text: "Cancelar",
        style: "cancel",
      },
      {
        text: "Eliminar",
        style: "destructive",
        onPress: () => deleteContacto(contacto.id!),
      },
    ])
  }

  const deleteContacto = async (contactoId: number) => {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/Contactos/DeleteContacto?id=${contactoId}`)

      if (response.ok) {
        Alert.alert("Éxito", "Contacto eliminado correctamente")
        loadContactos()
      } else {
        Alert.alert("Error", "No se pudo eliminar el contacto")
      }
    } catch (error) {
      Alert.alert("Error", "Hubo un error al eliminar el contacto")
    }
  }

  const renderContactoItem = ({ item }: { item: Contactos }) => (
    <View style={styles.contactoItem}>
      <View style={styles.contactoInfo}>
        <Text style={styles.contactoNombre}>{item.nombre}</Text>
        <Text style={styles.contactoDetalle}>Email: {item.mail}</Text>
        <Text style={styles.contactoDetalle}>Teléfono: {item.telefono}</Text>
        <Text style={styles.contactoDetalle}>Tipo: {item.tipo_Contacto === 1 ? "Institucional" : "Docente"}</Text>
      </View>

      {puedeEditar && (
        <View style={styles.actionsContainer}>
          <TouchableOpacity style={styles.editButton} onPress={() => handleEditContacto(item.id!)}>
            <Icon name="edit" size={18} color="#4285F4" />
          </TouchableOpacity>

          <TouchableOpacity style={styles.deleteButton} onPress={() => handleDeleteContacto(item)}>
            <Icon name="delete-outline" size={18} color="#F44336" />
          </TouchableOpacity>
        </View>
      )}
    </View>
  )

  if (loading) {
    return (
      <AppLayout title="Contactos" showLoadingOverlay={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#007AFF" />
          <Text style={styles.loadingText}>Cargando contactos...</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title="Contactos" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Contactos</Text>
        </View>

        {/* Barra de búsqueda */}
        <View style={styles.searchContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="Buscar contactos..."
            value={searchText}
            onChangeText={setSearchText}
          />
        </View>

        {puedeEditar && (
          <View style={styles.createButtonContainer}>
            <TouchableOpacity style={styles.createCourseButton} onPress={() => navigation.navigate("CreateContacto", { tipoContacto })}>
              <Icon name="add" size={20} color="#FFFFFF" />
              <Text style={styles.createCourseButtonText}>Crear Contacto</Text>
            </TouchableOpacity>
          </View>
        )}

        {/* Contenido principal */}
        {filteredCursos.length === 0 ? (
        <View style={styles.emptyContainer}>
          <Text style={styles.emptyText}>No hay contactos registrados</Text>
        </View>
      ) : (
        <FlatList
          data={filteredCursos}
          renderItem={renderContactoItem}
          keyExtractor={(item) => item.id?.toString() || ""}
          style={styles.list}
          showsVerticalScrollIndicator={false}
        />
      )}
        <View style={styles.buttonsContainer}>
            <TouchableOpacity style={styles.backButton} onPress={handleVolver}>
              <Icon name="arrow-back" size={16} color="#FFFFFF" />
              <Text style={styles.buttonText}>Atrás</Text>
            </TouchableOpacity>
        </View>
      </View>
    </AppLayout>
  )
}

const styles = StyleSheet.create({
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
  actionButtonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 10,
    backgroundColor: "#fff",
    borderBottomWidth: 1,
    borderBottomColor: "#e9ecef",
  },
  actionButton: {
    paddingVertical: 10,
    paddingHorizontal: 20,
    backgroundColor: "#007bff",
    borderRadius: 5,
  },
  actionButtonText: {
    color: "#fff",
    fontWeight: "bold",
  },
  list: {
    flex: 1,
    backgroundColor: "#f8f9fa",
  },
  contactoItem: {
    backgroundColor: "#fff",
    borderRadius: 8,
    padding: 15,
    marginHorizontal: 10,
    marginVertical: 5,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    shadowColor: "#000",
    shadowOffset: {
      width: 0,
      height: 1,
    },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  contactoInfo: {
    flex: 1,
  },
  contactoNombre: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    marginBottom: 5,
  },
  contactoDetalle: {
    fontSize: 14,
    color: "#666",
    marginBottom: 2,
  },
  actionsContainer: {
    flexDirection: "row",
    gap: 10,
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
  emptyContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  emptyText: {
    fontSize: 16,
    color: "#666",
    textAlign: "center",
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 16,
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  reportButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FF9800",
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
    flex: 1,
    marginRight: 8,
    justifyContent: "center",
  },
  backButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#666",
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
    flex: 1,
    marginLeft: 8,
    justifyContent: "center",
  },
  buttonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
    marginLeft: 8,
  },
  container: {
    flex: 1,
    backgroundColor: "#F5F5F5",
  },
  searchContainer: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FFFFFF",
    margin: 16,
    paddingHorizontal: 12,
    borderRadius: 20,
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
    color: "#333",
  },
  createButtonContainer: {
    paddingHorizontal: 16,
    marginBottom: 16,
  },
  createCourseButton: {
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
  createCourseButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
    marginLeft: 8,
  },
})

export default ContactosScreen
