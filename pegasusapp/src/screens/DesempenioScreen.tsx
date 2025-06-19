import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, TextInput, ActivityIndicator } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { IntegrantesCursos } from "../models/integrantesCursos"
import type { DesempenioAlumnos } from "../models/desempenioAlumnos"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

type DesempenioScreenRouteProp = RouteProp<RootStackParamList, "Desempenio">
type DesempenioScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "Desempenio">

interface AlumnoDesempenio {
  id_Alumno: number
  alumno: {
    id?: number | null
    nombre?: string | null
    apellido?: string | null
    mail?: string | null
    activo: boolean
  }
  desempenioId: number
  tieneDesempenio: boolean
}

const DesempenioScreen: React.FC = () => {
  const route = useRoute<DesempenioScreenRouteProp>()
  const navigation = useNavigation<DesempenioScreenNavigationProp>()
  const { userData, hijoSeleccionado } = useUser()
  const { cursoId, cursoNombre } = route.params

  const [alumnos, setAlumnos] = useState<AlumnoDesempenio[]>([])
  const [filteredAlumnos, setFilteredAlumnos] = useState<AlumnoDesempenio[]>([])
  const [searchText, setSearchText] = useState("")
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchAlumnosDesempenio()
  }, [cursoId])

  useEffect(() => {
    // Filtrar alumnos cuando cambia el texto de búsqueda
    if (searchText.trim() === "") {
      setFilteredAlumnos(alumnos)
    } else {
      const filtered = alumnos.filter((alumno) => {
        const nombreCompleto = `${alumno.alumno.apellido || ""} ${alumno.alumno.nombre || ""}`.toLowerCase()
        return nombreCompleto.includes(searchText.toLowerCase())
      })
      setFilteredAlumnos(filtered)
    }
  }, [searchText, alumnos])

  const fetchAlumnosDesempenio = async () => {
    try {
      setLoading(true)
      setError(null)

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      let integrantesCursos: IntegrantesCursos[] = []
      // Obtener alumnos según el perfil
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

      // Obtener desempeños existentes
      const desempeniosExistentes = await getDesempenioAlumnosAsync(cursoIdToUse)
      // Mapear alumnos con información de desempeño
      const alumnosConDesempenio: AlumnoDesempenio[] = integrantesCursos.map((integrante) => {
        const desempenio = desempeniosExistentes.find((d) => d.id_Alumno === integrante.id_Usuario)
        return {
          id_Alumno: integrante.id_Usuario || 0,
          alumno: {
            id: integrante.usuario?.id,
            nombre: integrante.usuario?.nombre,
            apellido: integrante.usuario?.apellido,
            mail: integrante.usuario?.mail,
            activo: integrante.usuario?.activo || false,
          },
          desempenioId: desempenio?.id || 0,
          tieneDesempenio: !!desempenio,
        }
      })
      setAlumnos(alumnosConDesempenio)
      
    } catch (error: any) {
      setError(error.message || "Error al cargar los datos")
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

  // Función para obtener desempeños de alumnos
  const getDesempenioAlumnosAsync = async (curso: number): Promise<DesempenioAlumnos[]> => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_curso == ${curso}`)
      const url = `${CONFIG.API_BASE_URL}/DesempenioAlumnos/GetDesempenioAlumnossForCombo?query=${queryParam}`

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: DesempenioAlumnos[] = await response.json()
      return data || []
    } catch (error) {
      throw error
    }
  }

  const handleVerDesempenio = (alumno: AlumnoDesempenio) => {
    navigation.navigate("CreateDesempenio", {
      desempenioId: alumno.desempenioId,
      alumnoId: alumno.id_Alumno,
      alumnoNombre: alumno.alumno.nombre || "",
      alumnoApellido: alumno.alumno.apellido || "",
      cursoId: cursoId,
      cursoNombre: cursoNombre,
      esVer: true,
    })
  }

  const handleEditarDesempenio = (alumno: AlumnoDesempenio) => {
    navigation.navigate("CreateDesempenio", {
      desempenioId: alumno.desempenioId,
      alumnoId: alumno.id_Alumno,
      alumnoNombre: alumno.alumno.nombre || "",
      alumnoApellido: alumno.alumno.apellido || "",
      cursoId: cursoId,
      cursoNombre: cursoNombre,
      esVer: false,
    })
  }

  const handleVolver = () => {
    if(userData?.id_perfil == 2 || userData?.id_perfil == 4)
      navigation.navigate("Home")
    else
      navigation.navigate("ListaCursos", { parametro: "Desempenio" })
  }

  const renderAlumnoItem = ({ item }: { item: AlumnoDesempenio }) => (
    <View style={styles.alumnoRow}>
      <View style={styles.iconContainer}>
        <Icon name="person" size={28} color="#4285F4" />
      </View>

      <View style={styles.alumnoInfo}>
        <Text style={styles.alumnoNombre}>
          {item.alumno.apellido}, {item.alumno.nombre}
        </Text>
      </View>

      <View style={styles.actionsContainer}>
        {userData && userData.id_perfil !== 2 && userData.id_perfil !== 4 ? (
          // Administrador/Docente
          item.tieneDesempenio ? (
            <>
              <TouchableOpacity style={styles.actionButton} onPress={() => handleEditarDesempenio(item)}>
                <Icon name="edit" size={20} color="#4285F4" />
              </TouchableOpacity>
              <TouchableOpacity style={styles.actionButton} onPress={() => handleVerDesempenio(item)}>
                <Icon name="visibility" size={20} color="#FF9800" />
              </TouchableOpacity>
            </>
          ) : (
            <TouchableOpacity style={styles.actionButton} onPress={() => handleEditarDesempenio(item)}>
              <Icon name="add" size={20} color="#4CAF50" />
            </TouchableOpacity>
          )
        ) : (
          // Alumno/Padre - solo ver si tiene desempeño
          item.tieneDesempenio && (
            <TouchableOpacity style={styles.actionButton} onPress={() => handleVerDesempenio(item)}>
              <Icon name="visibility" size={20} color="#FF9800" />
            </TouchableOpacity>
          )
        )}
      </View>
    </View>
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
          <TouchableOpacity style={styles.retryButton} onPress={fetchAlumnosDesempenio}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (filteredAlumnos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="analytics" size={64} color="#CCCCCC" />
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
          keyExtractor={(item) => item.id_Alumno.toString()}
          style={styles.listContainer}
          showsVerticalScrollIndicator={false}
        />
      </View>
    )
  }

  return (
    <AppLayout title={`Desempeño - ${cursoNombre}`} showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Desempeño</Text>
          <Text style={styles.headerSubtitle}>{cursoNombre}</Text>
        </View>

        {/* Barra de búsqueda */}
        <View style={styles.searchContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="Buscar alumnos..."
            value={searchText}
            onChangeText={setSearchText}
          />
        </View>

        {/* Contenedor principal */}
        <View style={styles.mainContent}>{renderContent()}</View>

        {/* Botón de volver */}
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
  actionsContainer: {
    flexDirection: "row",
    alignItems: "center",
  },
  actionButton: {
    padding: 8,
    marginLeft: 8,
    borderRadius: 20,
    backgroundColor: "#F5F5F5",
    justifyContent: "center",
    alignItems: "center",
    width: 36,
    height: 36,
  },
  buttonsContainer: {
    padding: 16,
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
    alignItems: "center",
  },
  backButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#6C7B7F",
    paddingHorizontal: 24,
    paddingVertical: 10,
    borderRadius: 8,
    justifyContent: "center",
    minWidth: 120,
    maxWidth: 200,
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

export default DesempenioScreen
