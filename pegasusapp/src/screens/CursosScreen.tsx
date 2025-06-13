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
import type { Curso } from "../models/curso"
import type { IntegrantesCursos } from "../models/integrantesCursos"
import type { CursoMateria } from "../models/cursoMateria"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

type CursosScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "Cursos">

const { width: screenWidth } = Dimensions.get("window")
const cardWidth = (screenWidth - 48) / 2 // 48 = margins and padding

const CursosScreen: React.FC = () => {
  const navigation = useNavigation<CursosScreenNavigationProp>()
  const { userData } = useUser()

  const [cursos, setCursos] = useState<Curso[]>([])
  const [filteredCursos, setFilteredCursos] = useState<Curso[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [searchText, setSearchText] = useState("")

  // Recargar datos cuando la pantalla recibe foco
  useFocusEffect(
    React.useCallback(() => {
      fetchCursos()
    }, []),
  )

  useEffect(() => {
    // Filtrar cursos cuando cambia el texto de búsqueda
    if (searchText.trim() === "") {
      setFilteredCursos(cursos)
    } else {
      const filtered = cursos.filter((curso) => {
        const nombreCurso = curso.nombre_Curso?.toLowerCase() || ""
        const gradoDivision = `${curso.grado} ${curso.division}`.toLowerCase()
        const turno = curso.turno?.toLowerCase() || ""
        const searchLower = searchText.toLowerCase()

        return nombreCurso.includes(searchLower) || gradoDivision.includes(searchLower) || turno.includes(searchLower)
      })
      setFilteredCursos(filtered)
    }
  }, [searchText, cursos])

  const fetchCursos = async () => {
    try {
      setLoading(true)
      setError(null)
      const cursosData = await getCursosAsync()
      setCursos(cursosData)
      setFilteredCursos(cursosData)
    } catch (error: any) {
      setError(error.message || "Error al cargar los cursos")
    } finally {
      setLoading(false)
    }
  }

  const getCursosAsync = async (): Promise<Curso[]> => {

    const response = await fetch(`${CONFIG.API_BASE_URL}/Curso/GetCursosForCombo`, {
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

  const handleCrearCurso = () => {
    navigation.navigate("CreateCurso", {})
  }

  const handleEditarCurso = (curso: Curso) => {
    navigation.navigate("CreateCurso", { cursoId: curso.id || undefined })
  }

  const handleEliminarCurso = async (curso: Curso) => {
    if (!curso.id) {
      Alert.alert("Error", "ID de curso no válido")
      return
    }

    try {
      // Verificar si tiene integrantes
      const tieneIntegrantes = await tieneIntegrantesCurso(curso.id)
      // Verificar si tiene materias
      const tieneMaterias = await tieneMateriasCurso(curso.id)
      

      // Confirmar eliminación
      Alert.alert("¿Estás seguro?", `¿Deseas eliminar el curso "${curso.nombre_Curso}"?`, [
        {
          text: "Cancelar",
          style: "cancel",
        },
        {
          text: "Eliminar",
          style: "destructive",
          onPress: async () => {
            try {
              await eliminarCursoConRelaciones(curso.id!, tieneIntegrantes, tieneMaterias)
              Alert.alert("Éxito", "El curso se eliminó correctamente")
              fetchCursos() // Recargar la lista
            } catch (error: any) {
              Alert.alert("Error", error.message || "Error al eliminar el curso")
            }
          },
        },
      ])
    } catch (error: any) {
      Alert.alert("Error", error.message || "Error al verificar las dependencias del curso")
    }
  }

  const tieneIntegrantesCurso = async (cursoId: number): Promise<boolean> => {

    const queryParam = encodeURIComponent(`x=>x.id_curso == ${cursoId}`)
    const response = await fetch(
      `${CONFIG.API_BASE_URL}/IntegrantesCursos/GetIntegrantesCursosForCombo?query=${queryParam}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      },
    )

    if (!response.ok) {
      throw new Error(`Error al verificar integrantes: ${response.statusText}`)
    }

    const integrantes: IntegrantesCursos[] = await response.json()
    return integrantes.length > 0
  }

  const tieneMateriasCurso = async (cursoId: number): Promise<boolean> => {
  
    const queryParam = encodeURIComponent(`x=>x.id_curso == ${cursoId}`)
    const response = await fetch(`${CONFIG.API_BASE_URL}/CursoMateria/GetCursoMateriaForCombo?query=${queryParam}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    })

    if (!response.ok) {
      throw new Error(`Error al verificar materias: ${response.statusText}`)
    }

    const materias: CursoMateria[] = await response.json()
    return materias.length > 0
  }

  const eliminarCursoConRelaciones = async (
    cursoId: number, 
    tieneIntegrantes: boolean, 
    tieneMaterias: boolean
    ): Promise<void> => {
    try {
      //Eliminar integrantes si existen
      if (tieneIntegrantes) {
        const integrantes = await obtenerIntegrantesCurso(cursoId)
        if (integrantes.length > 0) {
          await eliminarIntegrantesCurso(integrantes)
        }
      }

      //Eliminar materias si existen
      if (tieneMaterias) {
        const materias = await obtenerMateriasCurso(cursoId)
        if (materias.length > 0) {
          await eliminarMateriasCurso(materias)
        }
      }

      //Eliminar el curso
      await eliminarCursoAsync(cursoId)

    } catch (error: any) {
      throw new Error(`Error al eliminar curso y relaciones: ${error.message}`)
    }
  }

  const obtenerIntegrantesCurso = async (cursoId: number): Promise<IntegrantesCursos[]> => {
    const queryParam = encodeURIComponent(`x=>x.id_curso == ${cursoId}`)
    const response = await fetch(
    `${CONFIG.API_BASE_URL}/IntegrantesCursos/GetIntegrantesCursosForCombo?query=${queryParam}`,
      {
        method: "GET",
      }
    )

    if (!response.ok) {
      throw new Error(`Error al obtener integrantes: ${response.statusText}`)
    }

    return await response.json()
  }


const eliminarIntegrantesCurso = async (integrantes: IntegrantesCursos[]): Promise<void> => {
  
  const integrantesSimplificados = integrantes.map(i => ({ Id: i.id }))

  const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesCursos/DeleteAllIntegrantesCursos`, {
    method: "DELETE",
    body: JSON.stringify(integrantesSimplificados)
  })

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(`Error al eliminar integrantes: ${errorText}`)
  }
}


const obtenerMateriasCurso = async (cursoId: number): Promise<CursoMateria[]> => {
    const queryParam = encodeURIComponent(`x=>x.id_curso == ${cursoId}`)
    const response = await fetch(
      `${CONFIG.API_BASE_URL}/CursoMateria/GetCursoMateriaForCombo?query=${queryParam}`,
      {
        method: "GET",
      }
    )

    if (!response.ok) {
      throw new Error(`Error al obtener materias: ${response.statusText}`)
    }

    return await response.json()
}


  const eliminarMateriasCurso = async (materias: CursoMateria[]): Promise<void> => {
    const materiasSimplificadas = materias.map(m => ({ Id: m.id }))
    
    const response = await fetch(`${CONFIG.API_BASE_URL}/CursoMateria/DeleteAllCursoMateria`, {
      method: "DELETE",
      body: JSON.stringify(materiasSimplificadas)
    })
  
    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Error al eliminar materias: ${errorText}`)
    }
  }

  const eliminarCursoAsync = async (cursoId: number): Promise<void> => {

    const response = await fetch(`${CONFIG.API_BASE_URL}/Curso/DeleteCurso/${cursoId}`, {
      method: "DELETE",
    })

    if (!response.ok) {
      throw new Error(`Error al eliminar curso: ${response.statusText}`)
    }
  }

  const renderCursoItem = ({ item }: { item: Curso }) => (
    <View style={styles.cursoCard}>
      <View style={styles.cursoIconContainer}>
        <Icon name="school" size={32} color="#4285F4" />
      </View>

      {/* Información del curso */}
      <View style={styles.cursoInfo}>
        <Text style={styles.cursoNombre}>{item.nombre_Curso}</Text>
        <Text style={styles.cursoDetalle}>
          {item.grado}° {item.division}
        </Text>
        <Text style={styles.cursoTurno}>{item.turno}</Text>
      </View>

      {/* Botones de acción */}
      <View style={styles.cursoActions}>
        {userData?.id_perfil === 3 ? (
          // Solo ver para docentes
          <TouchableOpacity style={styles.viewButton} onPress={() => handleEditarCurso(item)}>
            <Icon name="visibility" size={18} color="#FF9800" />
          </TouchableOpacity>
        ) : (
          // Editar y eliminar para administradores y preceptores
          <>
            <TouchableOpacity style={styles.editButton} onPress={() => handleEditarCurso(item)}>
              <Icon name="edit" size={18} color="#4285F4" />
            </TouchableOpacity>
            <TouchableOpacity style={styles.deleteButton} onPress={() => handleEliminarCurso(item)}>
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
          <Text style={styles.loadingText}>Cargando cursos...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar cursos</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchCursos}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (filteredCursos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="school" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>{searchText ? "No se encontraron cursos" : "No hay cursos disponibles"}</Text>
          {userData?.id_perfil !== 3 && (
            <TouchableOpacity style={styles.createButton} onPress={handleCrearCurso}>
              <Icon name="add" size={20} color="#FFFFFF" />
              <Text style={styles.createButtonText}>Crear Primer Curso</Text>
            </TouchableOpacity>
          )}
        </View>
      )
    }

    return (
      <FlatList
        data={filteredCursos}
        renderItem={renderCursoItem}
        keyExtractor={(item) => item.id?.toString() || ""}
        numColumns={2}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
        columnWrapperStyle={styles.row}
      />
    )
  }

  return (
    <AppLayout title="Cursos" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Cursos</Text>
        </View>

        {/* Barra de búsqueda */}
        <View style={styles.searchContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="Buscar cursos..."
            value={searchText}
            onChangeText={setSearchText}
          />
        </View>

        {/* Botón Crear Curso */}
        {userData?.id_perfil !== 3 && (
          <View style={styles.createButtonContainer}>
            <TouchableOpacity style={styles.createCourseButton} onPress={handleCrearCurso}>
              <Icon name="add" size={20} color="#FFFFFF" />
              <Text style={styles.createCourseButtonText}>Crear Curso</Text>
            </TouchableOpacity>
          </View>
        )}

        {/* Contenido principal */}
        <View style={styles.contentContainer}>{renderContent()}</View>
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
  cursoCard: {
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
  cursoIconContainer: {
    marginBottom: 8,
    padding: 6,
    borderRadius: 40,
  },
  cursoInfo: {
    alignItems: "center",
    flex: 1,
    justifyContent: "center",
  },
  cursoNombre: {
    fontSize: 14,
    fontWeight: "bold",
    color: "#333",
    textAlign: "center",
    marginBottom: 4,
  },
  cursoDetalle: {
    fontSize: 13,
    color: "#666",
    textAlign: "center",
    marginBottom: 2,
  },
  cursoTurno: {
    fontSize: 11,
    color: "#888",
    textAlign: "center",
  },
  cursoActions: {
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
})

export default CursosScreen
