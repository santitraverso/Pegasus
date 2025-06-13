import React from "react"
import { useState, useEffect } from "react"
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  TextInput,
  Dimensions,
} from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useNavigation, useFocusEffect } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { Materia } from "../models/materia"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"
import { Asistencia } from "../models/asistencia"
import { CursoMateria } from "../models/cursoMateria"

type MateriasScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "Materias">

const { width: screenWidth } = Dimensions.get("window")
const cardWidth = (screenWidth - 48) / 2

const MateriasScreen: React.FC = () => {
  const navigation = useNavigation<MateriasScreenNavigationProp>()
  const { userData } = useUser()

  const [materias, setMaterias] = useState<Materia[]>([])
  const [filteredMaterias, setFilteredMaterias] = useState<Materia[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [searchText, setSearchText] = useState("")
  const [deleteLoading, setDeleteLoading] = useState(false)

  // Determinar permisos según el perfil del usuario
  const userProfile = userData?.id_perfil || 0
  const canCreate = userProfile !== 2 && userProfile !== 3 && userProfile !== 4
  const canEditDelete = userProfile !== 2 && userProfile !== 3 && userProfile !== 4
  const isViewOnly = userProfile === 2 || userProfile === 3 || userProfile === 4

  // Recargar datos cuando la pantalla recibe foco
  useFocusEffect(
    React.useCallback(() => {
      fetchMaterias()
    }, []),
  )

  useEffect(() => {
    // Filtrar materias cuando cambia el texto de búsqueda
    if (searchText.trim() === "") {
      setFilteredMaterias(materias)
    } else {
      const filtered = materias.filter((materia) => {
        const nombreMateria = materia.nombre?.toLowerCase() || ""
        const searchLower = searchText.toLowerCase()

        return nombreMateria.includes(searchLower)
      })
      setFilteredMaterias(filtered)
    }
  }, [searchText, materias])

  const fetchMaterias = async () => {
    try {
      setLoading(true)
      setError(null)
      const materiasData = await getMateriasAsync()
      setMaterias(materiasData)
      setFilteredMaterias(materiasData)
    } catch (error: any) {
      setError(error.message || "Error al cargar las materias")
    } finally {
      setLoading(false)
    }
  }

  const getMateriasAsync = async (): Promise<Materia[]> => {

    const response = await fetch(`${CONFIG.API_BASE_URL}/Materia/GetMateriasForCombo`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    })

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`)
    }

    return await response.json()
  }

  const handleCrearMateria = () => {
    navigation.navigate("CreateMateria", {})
  }

  const handleVerEditarMateria = (materia: Materia) => {
    navigation.navigate("CreateMateria", {
      materiaId: materia.id || undefined,
      viewOnly: isViewOnly,
    })
  }

  const handleEliminarMateria = async (materia: Materia) => {
    if (!materia.id) {
      Alert.alert("Error", "ID de materia no válido")
      return
    }

    try {
      // Verificar si tiene asistencias
      const tieneAsistencias = await tieneAsistenciasMateria(materia.id)
    
      // Verificar si tiene cursos asociados
      const tieneCursos = await tieneCursosMateria(materia.id)
      

      // Confirmar eliminación
      Alert.alert("¿Estás seguro?", `¿Deseas eliminar la materia "${materia.nombre}"?`, [
        {
          text: "Cancelar",
          style: "cancel",
        },
        {
          text: "Eliminar",
          style: "destructive",
          onPress: async () => {
            try {
              // Mostrar indicador de carga específico para eliminación
              setDeleteLoading(true)
              await eliminarMateriaConRelaciones(materia.id!, tieneAsistencias, tieneCursos)
              Alert.alert("Éxito", "La materia se eliminó correctamente")
              fetchMaterias() // Recargar la lista
            } catch (error: any) {
              Alert.alert("Error", error.message || "Error al eliminar la materia")
            } finally {
              setDeleteLoading(false)
            }
          },
        },
      ])
    } catch (error: any) {
      Alert.alert("Error", error.message || "Error al verificar las dependencias de la materia")
    }
  }

  const tieneAsistenciasMateria = async (materiaId: number): Promise<boolean> => {

    const queryParam = encodeURIComponent(`x=>x.id_materia == ${materiaId}`)
    const response = await fetch(`${CONFIG.API_BASE_URL}/Asistencia/GetAsistenciasForCombo?query=${queryParam}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    })

    if (!response.ok) {
      throw new Error(`Error al verificar asistencias: ${response.statusText}`)
    }

    const asistencias = await response.json()
    return asistencias.length > 0
  }

  const tieneCursosMateria = async (materiaId: number): Promise<boolean> => {

    const queryParam = encodeURIComponent(`x=>x.id_materia == ${materiaId}`)
    const response = await fetch(`${CONFIG.API_BASE_URL}/CursoMateria/GetCursoMateriaForCombo?query=${queryParam}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    })

    if (!response.ok) {
      throw new Error(`Error al verificar cursos: ${response.statusText}`)
    }

    const cursos = await response.json()
    return cursos.length > 0
  }

 
  const eliminarMateriaConRelaciones = async (
    materiaId: number, 
    tieneAsistencias: boolean, 
    tieneCursos: boolean
  ): Promise<void> => {
    try {
      //Eliminar asistencias si existen
      if (tieneAsistencias) {
        const asistencias = await obtenerAsistenciasMateria(materiaId)
        if (asistencias.length > 0) {
          await eliminarAsistenciasMateria(asistencias)
        }
      }

      //Eliminar asociaciones con cursos si existen
      if (tieneCursos) {
        const cursosMaterias = await obtenerCursosMaterias(materiaId)
        if (cursosMaterias.length > 0) {
          await eliminarCursosMaterias(cursosMaterias)
        }
      }

      //Eliminar la materia
      await eliminarMateriaAsync(materiaId)

    } catch (error: any) {
      throw new Error(`Error al eliminar materia y relaciones: ${error.message}`)
    }
  }


  const obtenerAsistenciasMateria = async (materiaId: number): Promise<Asistencia[]> => {
    const queryParam = encodeURIComponent(`x=>x.id_materia == ${materiaId}`)
    const response = await fetch(
      `${CONFIG.API_BASE_URL}/Asistencia/GetAsistenciasForCombo?query=${queryParam}`
    )

    if (!response.ok) {
      throw new Error(`Error al obtener asistencias: ${response.statusText}`)
    }

    return await response.json()
  }


  const eliminarAsistenciasMateria = async (asistencias: Asistencia[]): Promise<void> => {
    const asistenciasSimplificadas = asistencias.map(a => ({ Id: a.id }))
    
    const response = await fetch(`${CONFIG.API_BASE_URL}/Asistencia/DeleteAllAsistencia`, {
      method: "DELETE",
      body: JSON.stringify(asistenciasSimplificadas)
    })
  
    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Error al eliminar asistencia: ${errorText}`)
    }
  }


  const obtenerCursosMaterias = async (materiaId: number): Promise<CursoMateria[]> => {
    const queryParam = encodeURIComponent(`x=>x.id_materia == ${materiaId}`)
    const response = await fetch(
      `${CONFIG.API_BASE_URL}/CursoMateria/GetCursoMateriaForCombo?query=${queryParam}`
    )
  
    if (!response.ok) {
      throw new Error(`Error al obtener relaciones curso-materia: ${response.statusText}`)
    }
  
    return await response.json()
  }


  const eliminarCursosMaterias = async (cursosMaterias: CursoMateria[]): Promise<void> => {
    const cursoMateriasSimplificadas = cursosMaterias.map(cm => ({ Id: cm.id }))

    const response = await fetch(`${CONFIG.API_BASE_URL}/CursoMateria/DeleteAllCursoMateria`, {
      method: "DELETE",
      body: JSON.stringify(cursoMateriasSimplificadas)
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Error al eliminar la relacion curso materia: ${errorText}`)
    }
  }

  const eliminarMateriaAsync = async (materiaId: number): Promise<void> => {

    const response = await fetch(`${CONFIG.API_BASE_URL}/Materia/DeleteMateria/${materiaId}`, {
      method: "DELETE",
    })

    if (!response.ok) {
      throw new Error(`Error al eliminar materia: ${response.statusText}`)
    }
  }

  const renderMateriaItem = ({ item }: { item: Materia }) => (
    <View style={styles.materiaCard}>
      <View style={styles.materiaIconContainer}>
        <Icon name="book" size={32} color="#4285F4" />
      </View>

      {/* Información de la materia */}
      <View style={styles.materiaInfo}>
        <Text style={styles.materiaNombre}>{item.nombre}</Text>
      </View>

      {/* Botones de acción */}
      <View style={styles.materiaActions}>
        {isViewOnly ? (
          // Solo ver para perfiles 2, 3, 4
          <TouchableOpacity style={styles.viewButton} onPress={() => handleVerEditarMateria(item)}>
            <Icon name="visibility" size={18} color="#FF9800" />
          </TouchableOpacity>
        ) : (
          // Editar y eliminar para otros perfiles (admin)
          <>
            <TouchableOpacity style={styles.editButton} onPress={() => handleVerEditarMateria(item)}>
              <Icon name="edit" size={18} color="#4285F4" />
            </TouchableOpacity>
            <TouchableOpacity style={styles.deleteButton} onPress={() => handleEliminarMateria(item)}>
              <Icon name="delete-outline" size={18} color="#F44336" />
            </TouchableOpacity>
          </>
        )}
      </View>
    </View>
  )

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando materias...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar materias</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchMaterias}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (filteredMaterias.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="book" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>
            {searchText ? "No se encontraron materias" : "No hay materias disponibles"}
          </Text>
          {canCreate && (
            <TouchableOpacity style={styles.createButton} onPress={handleCrearMateria}>
              <Icon name="add" size={20} color="#FFFFFF" />
              <Text style={styles.createButtonText}>Crear Primera Materia</Text>
            </TouchableOpacity>
          )}
        </View>
      )
    }

    return (
      <FlatList
        data={filteredMaterias}
        renderItem={renderMateriaItem}
        keyExtractor={(item) => item.id?.toString() || ""}
        numColumns={2}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
        columnWrapperStyle={styles.row}
      />
    )
  }

  return (
    <AppLayout title="Materias" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Materias</Text>
        </View>

        {/* Barra de búsqueda */}
        <View style={styles.searchContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="Buscar materias..."
            value={searchText}
            onChangeText={setSearchText}
          />
        </View>

        {/* Botón Crear Materia */}
        {canCreate && (
          <View style={styles.createButtonContainer}>
            <TouchableOpacity style={styles.createMateriaButton} onPress={handleCrearMateria}>
              <Icon name="add" size={20} color="#FFFFFF" />
              <Text style={styles.createMateriaButtonText}>Crear Materia</Text>
            </TouchableOpacity>
          </View>
        )}

        {/* Contenido principal */}
        <View style={styles.contentContainer}>{renderContent()}</View>

        {/* Overlay de carga para la eliminación*/}
        {deleteLoading && (
          <View style={styles.loadingOverlay}>
            <ActivityIndicator size="large" color="#4285F4" />
            <Text style={styles.loadingText}>Eliminando...</Text>
          </View>
        )}
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
  createMateriaButton: {
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
  createMateriaButtonText: {
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
  row: {
    justifyContent: "space-between",
  },
  materiaCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
    width: cardWidth,
    height: cardWidth,
    elevation: 3,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    alignItems: "center",
    justifyContent: "space-between",
  },
  materiaIconContainer: {
    marginBottom: 8,
    padding: 6,
    backgroundColor: "#F8F9FA",
    borderRadius: 40,
  },
  materiaInfo: {
    alignItems: "center",
    flex: 1,
    justifyContent: "center",
  },
  materiaNombre: {
    fontSize: 14,
    fontWeight: "bold",
    color: "#333",
    textAlign: "center",
    marginBottom: 4,
  },
  materiaActions: {
    flexDirection: "row",
    justifyContent: "center",
    alignItems: "center",
    marginTop: 8,
  },
  editButton: {
    padding: 8,
    backgroundColor: "#E3F2FD",
    borderRadius: 20,
    marginRight: 8,
  },
  deleteButton: {
    padding: 8,
    backgroundColor: "#FFEBEE",
    borderRadius: 20,
    marginLeft: 8,
  },
  viewButton: {
    padding: 8,
    backgroundColor: "#F5F5F5",
    borderRadius: 20,
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
  loadingOverlay: {
    position: 'absolute',
    left: 0,
    right: 0,
    top: 0,
    bottom: 0,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    zIndex: 1000,
  },
})

export default MateriasScreen
