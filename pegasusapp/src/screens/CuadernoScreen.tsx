import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, TextInput, ActivityIndicator, Alert } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { IntegrantesCursos } from "../models/integrantesCursos"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

// Extender el tipo de navegación para incluir la nueva ruta
type ExtendedRootStackParamList = RootStackParamList & {
  Cuaderno: {
    cursoId: number
    cursoNombre: string
  }
  ListaComunicados: {
    cursoId: number
    cursoNombre: string
    alumnosIds: number[]
  }
  CreateComunicado: {
    cursoId: number
    cursoNombre: string
    alumnosIds: number[]
  }
  ListaCursos: { parametro?: string }
}

type CuadernoScreenRouteProp = RouteProp<ExtendedRootStackParamList, "Cuaderno">
type CuadernoScreenNavigationProp = NativeStackNavigationProp<ExtendedRootStackParamList, "Cuaderno">

interface AlumnoSeleccionable extends IntegrantesCursos {
  selected: boolean
}

const CuadernoScreen: React.FC = () => {
  const route = useRoute<CuadernoScreenRouteProp>()
  const navigation = useNavigation<CuadernoScreenNavigationProp>()
  const { userData, hijoSeleccionado } = useUser()
  const { cursoId, cursoNombre } = route.params

  const [alumnos, setAlumnos] = useState<AlumnoSeleccionable[]>([])
  const [filteredAlumnos, setFilteredAlumnos] = useState<AlumnoSeleccionable[]>([])
  const [searchText, setSearchText] = useState("")
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchAlumnos()
  }, [cursoId])

  useEffect(() => {
    // Filtrar alumnos cuando cambia el texto de búsqueda
    if (searchText.trim() === "") {
      setFilteredAlumnos(alumnos)
    } else {
      const filtered = alumnos.filter((alumno) => {
        const nombreCompleto = `${alumno.usuario?.apellido || ""} ${alumno.usuario?.nombre || ""}`.toLowerCase()
        return nombreCompleto.includes(searchText.toLowerCase())
      })
      setFilteredAlumnos(filtered)
    }
  }, [searchText, alumnos])

  const fetchAlumnos = async () => {
    try {
      setLoading(true)
      setError(null)

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      let integrantesCursos: IntegrantesCursos[] = []
      let cursoIdToUse = cursoId

      // Para alumnos y padres, obtener automáticamente el curso
      if (userData.id_perfil === 2) {
        // Alumno
        const cursosAlumno = await getCursosAsync(userData.id)
        if (cursosAlumno.length > 0) {
          cursoIdToUse = cursosAlumno[0].id_Curso || 0
        }
        integrantesCursos = await getIntegrantesCursosAsync(cursoIdToUse, userData.id)
      } else if (userData.id_perfil === 4) {
        // Padre
        if (hijoSeleccionado?.hijoUsuario?.id) {
          const cursosPadre = await getCursosAsync(hijoSeleccionado.hijoUsuario.id)
          if (cursosPadre.length > 0) {
            cursoIdToUse = cursosPadre[0].id_Curso || 0
          }
          integrantesCursos = await getIntegrantesCursosAsync(cursoIdToUse, hijoSeleccionado.hijoUsuario.id)
        }
      } else {
        // Administrador/Docente
        integrantesCursos = await getIntegrantesCursosAsync(cursoIdToUse)
      }
      

      // Convertir a AlumnoSeleccionable
      const alumnosSeleccionables = integrantesCursos.map((integrante) => ({
        ...integrante,
        selected: false,
      }))

      setAlumnos(alumnosSeleccionables)
      setFilteredAlumnos(alumnosSeleccionables)
    } catch (error: any) {
      setError(error.message || "Error al cargar los alumnos")
    } finally {
      setLoading(false)
    }
  }

   const getCursosAsync = async (usuario: number): Promise<IntegrantesCursos[]> => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_usuario==${usuario}`)
      const url = `${CONFIG.API_BASE_URL}/IntegrantesCursos/GetIntegrantesCursosForCombo?query=${queryParam}`

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: IntegrantesCursos[] = await response.json()
      return data || []
    } catch (error) {
      throw error
    }
  }

  // Función para obtener integrantes del curso
  const getIntegrantesCursosAsync = async (curso: number, usuario = 0): Promise<IntegrantesCursos[]> => {
    try {
      let url: string

      if (usuario > 0) {
        const queryParam = encodeURIComponent(`x=>x.id_curso==${curso} && x.id_usuario==${usuario}`)
        url = `${CONFIG.API_BASE_URL}/IntegrantesCursos/GetIntegrantesCursosForCombo?query=${queryParam}`
      } else {
        const queryParam = encodeURIComponent(`x=>x.id_curso==${curso}`)
        url = `${CONFIG.API_BASE_URL}/IntegrantesCursos/GetIntegrantesCursosForCombo?query=${queryParam}`
      }

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: IntegrantesCursos[] = await response.json()
      return data || []
    } catch (error) {
      throw error
    }
  }

  const toggleAlumnoSelection = (id: number) => {
    const updatedAlumnos = alumnos.map((alumno) => {
      if (alumno.id_Usuario === id) {
        return { ...alumno, selected: !alumno.selected }
      }
      return alumno
    })
    setAlumnos(updatedAlumnos)

    // También actualizar la lista filtrada
    const updatedFiltered = filteredAlumnos.map((alumno) => {
      if (alumno.id_Usuario === id) {
        return { ...alumno, selected: !alumno.selected }
      }
      return alumno
    })
    setFilteredAlumnos(updatedFiltered)
  }

  const getSelectedAlumnosIds = (): number[] => {
    return alumnos.filter((alumno) => alumno.selected).map((alumno) => alumno.id_Usuario || 0)
  }

  const handleComunicar = () => {
    const selectedIds = getSelectedAlumnosIds()
    if (selectedIds.length === 0) {
      Alert.alert("Error", "Debe seleccionar alumnos")
      return
    }

    navigation.navigate("CreateComunicado", {
      cursoId: cursoId,
      cursoNombre: cursoNombre,
      alumnosIds: selectedIds,
    })
  }

  const handleVerComunicados = () => {
    const selectedIds = getSelectedAlumnosIds()
    if (selectedIds.length === 0) {
      Alert.alert("Error", "Debe seleccionar alumnos")
      return
    }

    navigation.navigate("ListaComunicados", {
      cursoId: cursoId,
      cursoNombre: cursoNombre,
      alumnosIds: selectedIds,
    })
  }

  const handleVolver = () => {
    if(userData?.id_perfil == 2 || userData?.id_perfil == 4)
      navigation.navigate("Home")
    else
      navigation.navigate("ListaCursos", { parametro: "Cuaderno" })
  }

  const renderAlumnoItem = ({ item }: { item: AlumnoSeleccionable }) => (
    <TouchableOpacity style={styles.alumnoRow} onPress={() => toggleAlumnoSelection(item.id_Usuario || 0)}>
      <View style={styles.iconContainer}>
        <Icon name="person" size={28} color="#4285F4" />
      </View>

      <View style={styles.alumnoInfo}>
        <Text style={styles.alumnoNombre}>
          {item.usuario?.apellido}, {item.usuario?.nombre}
        </Text>
      </View>

      <View style={styles.checkboxContainer}>
        <View style={[styles.checkbox, item.selected && styles.checkboxSelected]}>
          {item.selected && <Icon name="check" size={16} color="#FFFFFF" />}
        </View>
      </View>
    </TouchableOpacity>
  )

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando alumnos...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar alumnos</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchAlumnos}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (filteredAlumnos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="person" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>
            {searchText ? "No se encontraron alumnos" : "No hay alumnos en este curso"}
          </Text>
        </View>
      )
    }

    return (
      <View style={styles.contentContainer}>
        <FlatList
          data={filteredAlumnos}
          renderItem={renderAlumnoItem}
          keyExtractor={(item) => item.id?.toString() || ""}
          style={styles.listContainer}
          showsVerticalScrollIndicator={false}
          scrollEnabled={filteredAlumnos.length > 8}
        />
      </View>
    )
  }

  return (
    <AppLayout title={`Alumnos - ${cursoNombre}`} showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Alumnos</Text>
          <Text style={styles.headerSubtitle}>{cursoNombre}</Text>
        </View>

        {/* Barra de búsqueda */}
        <View style={styles.searchContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="Buscar usuarios..."
            value={searchText}
            onChangeText={setSearchText}
          />
        </View>

        <View style={styles.mainContent}>
          {/* Lista de alumnos */}
          {renderContent()}
        </View>

        {/* Botones de acción */}
        <View style={styles.buttonsContainer}>
          <TouchableOpacity style={styles.verButton} onPress={handleVerComunicados}>
            <Icon name="visibility" size={16} color="#FFFFFF" />
            <Text style={styles.buttonText}>Ver</Text>
          </TouchableOpacity>

          {userData && userData.id_perfil !== 2 && userData.id_perfil !== 4 && (
            <TouchableOpacity style={styles.comunicarButton} onPress={handleComunicar}>
              <Icon name="message" size={16} color="#FFFFFF" />
              <Text style={styles.buttonText}>Comunicar</Text>
            </TouchableOpacity>
          )}

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
  headerSubtitle: {
    fontSize: 14,
    color: "#666",
    marginTop: 4,
  },
  searchContainer: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FFFFFF",
    margin: 16,
    paddingHorizontal: 12,
    borderRadius: 25,
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
  mainContent: {
    flex: 1,
  },
  contentContainer: {
    marginHorizontal: 16,
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  listContainer: {
    maxHeight: 400,
  },
  alumnoRow: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  iconContainer: {
    marginRight: 12,
  },
  alumnoInfo: {
    flex: 1,
  },
  alumnoNombre: {
    fontSize: 16,
    color: "#333",
    fontWeight: "400",
  },
  checkboxContainer: {
    marginLeft: 12,
  },
  checkbox: {
    width: 20,
    height: 20,
    borderWidth: 2,
    borderColor: "#4285F4",
    borderRadius: 4,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "#FFFFFF",
  },
  checkboxSelected: {
    backgroundColor: "#4285F4",
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 16,
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  verButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FF9800",
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 8,
    minWidth: 80,
    justifyContent: "center",
  },
  comunicarButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#4285F4",
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 8,
    minWidth: 100,
    justifyContent: "center",
  },
  backButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#6C7B7F",
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 8,
    minWidth: 80,
    justifyContent: "center",
  },
  buttonText: {
    color: "#FFFFFF",
    fontSize: 14,
    fontWeight: "600",
    marginLeft: 4,
  },
  loadingContainer: {
    justifyContent: "center",
    alignItems: "center",
    padding: 40,
    marginHorizontal: 16,
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  loadingText: {
    marginTop: 16,
    fontSize: 16,
    color: "#666",
  },
  errorContainer: {
    justifyContent: "center",
    alignItems: "center",
    padding: 40,
    marginHorizontal: 16,
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
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
    justifyContent: "center",
    alignItems: "center",
    padding: 40,
    marginHorizontal: 16,
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  emptyText: {
    fontSize: 16,
    color: "#666",
    marginTop: 16,
    textAlign: "center",
  },
})

export default CuadernoScreen
