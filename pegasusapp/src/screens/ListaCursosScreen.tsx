import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useUser } from "../context/UserContext"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RouteProp } from "@react-navigation/native"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { Curso } from "../models/curso"
import { CONFIG } from "../services/config"
import type { DocenteMateria } from "../models/docenteMateria"
import type { IntegrantesCursos } from "../models/integrantesCursos"

type ListaCursosScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "ListaCursos">
type ListaCursosScreenRouteProp = RouteProp<RootStackParamList, "ListaCursos">

const ListaCursosScreen: React.FC = () => {
  const { userData, hijoSeleccionado } = useUser()
  const navigation = useNavigation<ListaCursosScreenNavigationProp>()
  const route = useRoute<ListaCursosScreenRouteProp>()

  const [cursos, setCursos] = useState<Curso[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  

  // Obtener el parámetro pasado desde el módulo
  const parametroDestino = route.params?.parametro

  useEffect(() => {
    fetchCursos()
  }, [])

  const fetchCursos = async () => {
    try {
      setLoading(true)
      setError(null)

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      let cursosResult: Curso[] = []

      // Lógica según el perfil
      switch (userData.id_perfil) {
        case 2: // Alumno
          const cursosAlumno = await getCursosAsync(userData.id)
          cursosResult = integrantesToCurso(cursosAlumno)
          break

        case 3: // Docente
          const cursosDocente = await getCursosDocenteAsync(userData.id)
          cursosResult = docenteMateriaToCurso(cursosDocente)
          break

        case 4: // Padre
          if (hijoSeleccionado?.hijoUsuario?.id) {
            const cursosHijo = await getCursosAsync(hijoSeleccionado.hijoUsuario?.id)
            cursosResult = integrantesToCurso(cursosHijo)
          }
          break

        default: // Administrador u otros
          const todosLosCursos = await getCursosAsync() 
          // Agrupar por Id_Curso y ordenar
          const cursosUnicos = todosLosCursos
            .reduce((acc, current) => {
              const cursoId = current.curso?.id
              if (cursoId && !acc.find((item) => item.curso?.id === cursoId)) {
                acc.push(current)
              }
              return acc
            }, [] as IntegrantesCursos[])
            .sort((a, b) => (a.curso?.id || 0) - (b.curso?.id || 0))

          cursosResult = integrantesToCurso(cursosUnicos)
          break
        }
      

      setCursos(cursosResult)
    } catch (error: any) {
      setError(error.message || "Error al cargar los cursos")
    } finally {
      setLoading(false)
    }
  }

  // Función para obtener cursos de alumnos/padres
  const getCursosAsync = async (usuario = 0): Promise<IntegrantesCursos[]> => {
    try {
      let url: string

      if (usuario > 0) {
        const queryParam = encodeURIComponent(`x=>x.id_usuario==${usuario}`)
        url = `${CONFIG.API_BASE_URL}/IntegrantesCursos/GetIntegrantesCursosForCombo?query=${queryParam}`
      } else {
        url = `${CONFIG.API_BASE_URL}/IntegrantesCursos/GetIntegrantesCursosForCombo`
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

      const cursosData: IntegrantesCursos[] = await response.json()
      return cursosData || []
    } catch (error) {
      throw error
    }
  }

  // Función para obtener cursos de docentes
  const getCursosDocenteAsync = async (docente: number): Promise<DocenteMateria[]> => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_docente==${docente}`)
      const url = `${CONFIG.API_BASE_URL}/DocenteMateria/GetDocenteMateriaForCombo?query=${queryParam}`

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const cursosData: DocenteMateria[] = await response.json()
      return cursosData || []
    } catch (error) {
      throw error
    }
  }

  // Función para convertir IntegrantesCursos a Curso
  const integrantesToCurso = (integrantes: IntegrantesCursos[]): Curso[] => {
    return integrantes
      .filter((item) => item.curso)
      .map((item) => ({
        id: item.curso!.id,
        nombre_Curso: item.curso!.nombre_Curso,
        grado: item.curso!.grado,
        division: item.curso!.division,
        turno: item.curso!.turno,
      }))
  }

  // Función para convertir DocenteMateria a Curso (agrupando por curso)
  const docenteMateriaToCurso = (docenteMaterias: DocenteMateria[]): Curso[] => {
    // Agrupar por Id_Curso y tomar el primer elemento de cada grupo
    const cursosUnicos = docenteMaterias
      .filter((item) => item.curso)
      .reduce((acc, current) => {
        const cursoId = current.curso!.id
        if (cursoId && !acc.find((item) => item.curso!.id === cursoId)) {
          acc.push(current)
        }
        return acc
      }, [] as DocenteMateria[])

    return cursosUnicos.map((item) => ({
      id: item.curso!.id,
      nombre_Curso: item.curso!.nombre_Curso,
      grado: item.curso!.grado,
      division: item.curso!.division,
      turno: item.curso!.turno,
    }))
  }

  const handleCursoPress = (curso: Curso) => {

    if (!parametroDestino || !curso.id) {
      return
    }

    // Determinar si necesita ir a ListaMaterias o directamente al destino
    const modulosQueNecesitanMaterias = ["Calificacion", "Asistencia"]

    if (modulosQueNecesitanMaterias.includes(parametroDestino)) {
      // Navegar a ListaMaterias primero
      navigation.navigate("ListaMaterias", {
        cursoId: curso.id,
        cursoNombre: curso.nombre_Curso || `${curso.grado}° ${curso.division}`,
        modulo: parametroDestino,
      })
      return
    }

    // Para otras pantallas, navegar directamente
    try {
      const routeName = parametroDestino as keyof RootStackParamList

      switch (routeName) {
        case "Cuaderno":
          navigation.navigate("Cuaderno", {
            cursoId: curso.id,
            cursoNombre: curso.nombre_Curso || `${curso.grado}° ${curso.division}`,
          })
          break
        case "Desempenio":
          navigation.navigate("Desempenio", {
            cursoId: curso.id,
            cursoNombre: curso.nombre_Curso || `${curso.grado}° ${curso.division}`,
          })
          break
        default:
          console.error("Ruta de destino no reconocida:", parametroDestino)
      }
    } catch (error) {
      console.error("Error al navegar:", error)
    }
  }

  const renderCursoItem = ({ item }: { item: Curso }) => (
    <TouchableOpacity style={styles.cursoCard} onPress={() => handleCursoPress(item)}>
      <View style={styles.cursoHeader}>
        <Icon name="school" size={24} color="#4285F4" />
        <Text style={styles.cursoNombre}>{item.nombre_Curso || `${item.grado}° ${item.division}`}</Text>
      </View>
      <View style={styles.cursoDetails}>
        <Text style={styles.cursoDetail}>Grado: {item.grado}</Text>
        <Text style={styles.cursoDetail}>División: {item.division}</Text>
        <Text style={styles.cursoDetail}>Turno: {item.turno}</Text>
      </View>
      <Icon name="chevron-right" size={24} color="#CCCCCC" />
    </TouchableOpacity>
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

    if (cursos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="school" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>No tienes cursos asignados</Text>
        </View>
      )
    }

    return (
      <FlatList
        data={cursos}
        renderItem={renderCursoItem}
        keyExtractor={(item) => item.id?.toString() || ""}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
      />
    )
  }

  return (
    <AppLayout title="Seleccionar Curso" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Cursos</Text>
        </View>
        {renderContent()}
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
  listContainer: {
    padding: 16,
  },
  cursoCard: {
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
  cursoHeader: {
    flexDirection: "row",
    alignItems: "center",
    flex: 1,
  },
  cursoNombre: {
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
    marginLeft: 12,
  },
  cursoDetails: {
    flex: 2,
    marginLeft: 16,
  },
  cursoDetail: {
    fontSize: 12,
    color: "#666",
    marginBottom: 2,
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
  },
})

export default ListaCursosScreen
