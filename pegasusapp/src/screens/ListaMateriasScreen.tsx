import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { CursoMateria } from "../models/cursoMateria"
import type { DocenteMateria } from "../models/docenteMateria"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

type ListaMateriasScreenRouteProp = RouteProp<RootStackParamList, "ListaMaterias">
type ListaMateriasScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "ListaMaterias">

const ListaMateriasScreen: React.FC = () => {
  const route = useRoute<ListaMateriasScreenRouteProp>()
  const navigation = useNavigation<ListaMateriasScreenNavigationProp>()
  const { userData } = useUser()
  const { cursoId, cursoNombre, modulo } = route.params

  const [materias, setMaterias] = useState<CursoMateria[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchMaterias()
  }, [cursoId])

  const fetchMaterias = async () => {
    try {
      setLoading(true)
      setError(null)

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      let materiasResult: CursoMateria[] = []
      if (userData.id_perfil === 3) {
        // Docente - obtener solo las materias que enseña en este curso
        const materiasDocente = await getMateriasDocenteAsync(userData.id, cursoId)
        // Agrupar por Id_Materia y convertir a CursoMateria
        const materiasUnicas = materiasDocente.reduce((acc, current) => {
          const materiaId = current.id_Materia
          if (materiaId && !acc.find((item) => item.id_Materia === materiaId)) {
            acc.push(current)
          }
          return acc
        }, [] as DocenteMateria[])
        materiasResult = materiasUnicas.map((docenteMateria) => ({
          id: docenteMateria.id,
          id_Curso: docenteMateria.id_Curso,
          id_Materia: docenteMateria.id_Materia,
          materia: docenteMateria.materia,
          curso: docenteMateria.curso,
        }))
      } else {
        // Otros perfiles - obtener todas las materias del curso
        materiasResult = await getMateriasAsync(cursoId)
      }
      setMaterias(materiasResult)
      

    } catch (error: any) {
      setError(error.message || "Error al cargar las materias")
    } finally {
      setLoading(false)
    }
  }

  // Función para obtener todas las materias de un curso
  const getMateriasAsync = async (curso: number): Promise<CursoMateria[]> => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_curso == ${curso}`)
      const url = `${CONFIG.API_BASE_URL}/CursoMateria/GetCursoMateriaForCombo?query=${queryParam}`

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: CursoMateria[] = await response.json()
      return data || []
    } catch (error) {
      throw error
    }
  }

  // Función para obtener materias de un docente en un curso específico
  const getMateriasDocenteAsync = async (docente: number, curso: number): Promise<DocenteMateria[]> => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_docente==${docente} && x.id_curso==${curso}`)
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

      const data: DocenteMateria[] = await response.json()
      return data || []
    } catch (error) {
      throw error
    }
  }

  const handleMateriaPress = (materia: CursoMateria) => {

    if (!materia.id_Materia || !materia.materia) {
      return
    }

    // Navegar según el módulo especificado
    switch (modulo) {
      case "Calificacion":
        navigation.navigate("Calificacion", {
          cursoId: cursoId,
          cursoNombre: cursoNombre,
          materiaId: materia.id_Materia,
          materiaNombre: materia.materia.nombre || "Materia",
        })
        break
      case "Asistencia":
        navigation.navigate("Asistencia", {
          cursoId: cursoId,
          cursoNombre: cursoNombre,
          materiaId: materia.id_Materia,
          materiaNombre: materia.materia.nombre || "Materia",
        })
        break
      default:
        navigation.goBack()
        break
    }
  }

  const handleVolver = () => {
    navigation.goBack()
  }

  const renderMateriaItem = ({ item }: { item: CursoMateria }) => (
    <TouchableOpacity style={styles.materiaCard} onPress={() => handleMateriaPress(item)}>
      <View style={styles.materiaContent}>
        <Icon name="book" size={32} color="#4285F4" />
        <Text style={styles.materiaNombre}>{item.materia?.nombre || "Materia"}</Text>
      </View>
    </TouchableOpacity>
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

    if (materias.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="book" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>No hay materias disponibles para este curso</Text>
        </View>
      )
    }

    return (
      <FlatList
        data={materias}
        renderItem={renderMateriaItem}
        keyExtractor={(item) => item.id?.toString() || item.id_Materia?.toString() || ""}
        numColumns={2}
        columnWrapperStyle={styles.row}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
      />
    )
  }

  return (
    <AppLayout title="Seleccionar Materia" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Materias</Text>
          <Text style={styles.headerSubtitle}>{cursoNombre}</Text>
        </View>

        {renderContent()}

        {/* Botón volver */}
        <View style={styles.buttonContainer}>
          <TouchableOpacity style={styles.backButton} onPress={handleVolver}>
            <Icon name="arrow-back" size={20} color="#FFFFFF" />
            <Text style={styles.buttonText}>Volver</Text>
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
  headerModule: {
    fontSize: 12,
    color: "#4285F4",
    marginTop: 2,
    fontWeight: "500",
  },
  listContainer: {
    padding: 16,
  },
  row: {
    justifyContent: "space-between",
  },
  materiaCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    padding: 16,
    marginBottom: 16,
    width: "46%",
    elevation: 3,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.15,
    shadowRadius: 4,
    minHeight: 120,
    justifyContent: "center",
    alignItems: "center",
  },
  materiaContent: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  materiaNombre: {
    fontSize: 14,
    fontWeight: "600",
    color: "#333",
    textAlign: "center",
    marginTop: 8,
    lineHeight: 18,
  },
  buttonContainer: {
    padding: 16,
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
    alignItems: "center",
  },
  backButton: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#666",
    paddingVertical: 12,
    borderRadius: 8,
    paddingHorizontal: 24,
    minWidth: 120,
    maxWidth: 200,
  },
  buttonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
    marginLeft: 8,
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
})

export default ListaMateriasScreen
