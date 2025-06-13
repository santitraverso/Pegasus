import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator, Alert } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { ComunicadoViewModel } from "../models/comunicadoViewModel"
import type { ComunicadoAlumnos } from "../models/comunicadoAlumnos"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

// Extender el tipo de navegación para incluir la nueva ruta
type ExtendedRootStackParamList = RootStackParamList & {
  ListaComunicados: {
    cursoId: number
    cursoNombre: string
    alumnosIds: number[]
  }
  Cuaderno: {
    cursoId: number
    cursoNombre: string
  }
  CreateComunicado: {
    cursoId: number
    cursoNombre: string
    alumnosIds: number[]
    comunicadoId?: number
  }
}

type ListaComunicadosScreenRouteProp = RouteProp<ExtendedRootStackParamList, "ListaComunicados">
type ListaComunicadosScreenNavigationProp = NativeStackNavigationProp<ExtendedRootStackParamList, "ListaComunicados">

const ListaComunicadosScreen: React.FC = () => {
  const route = useRoute<ListaComunicadosScreenRouteProp>()
  const navigation = useNavigation<ListaComunicadosScreenNavigationProp>()
  const { userData, hijoSeleccionado } = useUser()
  const { cursoId, cursoNombre, alumnosIds } = route.params

  const [comunicados, setComunicados] = useState<ComunicadoViewModel[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchComunicados()
  }, [cursoId, alumnosIds])

  const fetchComunicados = async () => {
    try {
      setLoading(true)
      setError(null)

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      // Validar que hay hijo seleccionado para padres
      if (userData.id_perfil === 4 && !hijoSeleccionado) {
        throw new Error("Debe seleccionar un hijo antes de ver los comunicados")
      }

      const comunicadosDict = new Map<number, ComunicadoAlumnos[]>()

      // Para cada alumno seleccionado
      for (const alumnoId of alumnosIds) {

        // Obtener comunicados del alumno
        const comunicadosAlumno = await getComunicadosAlumnosAsync(alumnoId)

        const comunicadosAlumnos: ComunicadoAlumnos[] = []

        // Para cada comunicado del alumno, obtener todos los alumnos de ese comunicado
        for (const comunicado of comunicadosAlumno) {
          const comuAlumnos = await getAlumnosComunicadoAsync(comunicado.id_Comunicado)
          comunicadosAlumnos.push(...comuAlumnos)
        }

        // Agrupar por ID de comunicado
        for (const comunicadoAlumno of comunicadosAlumnos) {
          if (comunicadosDict.has(comunicadoAlumno.id_Comunicado)) {
            const existing = comunicadosDict.get(comunicadoAlumno.id_Comunicado)!
            // Verificar si el alumno ya está en la lista para evitar duplicados
            if (!existing.find((ca) => ca.id === comunicadoAlumno.id)) {
              existing.push(comunicadoAlumno)
            }
          } else {
            comunicadosDict.set(comunicadoAlumno.id_Comunicado, [comunicadoAlumno])
          }
        }
       }

      // Filtrar por perfil si es alumno o padre
      if (userData.id_perfil === 2 || userData.id_perfil === 4) {
        // Determinar el ID a usar para filtrar
        let idParaFiltrar: number| null | undefined

        if (userData.id_perfil === 4) {
          // Para padres, usar el ID del hijo seleccionado
          if (!hijoSeleccionado) {
            throw new Error("No hay hijo seleccionado para el padre")
          }
          idParaFiltrar = hijoSeleccionado.id_Hijo
        } else {
          // Para alumnos, usar su propio ID
          idParaFiltrar = userData.id
        }

        for (const [comunicadoId, alumnos] of comunicadosDict.entries()) {
          const filteredAlumnos = alumnos.filter((c) => c.id_Alumno === idParaFiltrar)

          if (filteredAlumnos.length === 0) {
            comunicadosDict.delete(comunicadoId)
          } else {
            comunicadosDict.set(comunicadoId, filteredAlumnos)
          }
        }
       }

      // Convertir a ComunicadoViewModel
      const comunicadosResult: ComunicadoViewModel[] = Array.from(comunicadosDict.entries())
        .filter(([_, alumnos]) => alumnos[0]?.comunicado?.id_Curso === cursoId)
        .map(([comunicadoId, alumnos]) => {
          // Obtener nombres únicos de alumnos
          const nombresUnicos = Array.from(new Set(alumnos.map((c) => `${c.alumno?.apellido} ${c.alumno?.nombre}`)))

          return {
            ids: alumnos.map((c) => c.id).join(", "),
            id_Comunicado: comunicadoId,
            descripcion: alumnos[0]?.comunicado?.descripcion || "",
            alumnosConcatenados: nombresUnicos.join(", "),
            fecha: alumnos[0]?.comunicado?.fecha || null,
          }
        })
        .sort((a, b) => {
          const dateA = new Date(a.fecha || 0).getTime()
          const dateB = new Date(b.fecha || 0).getTime()
          return dateB - dateA // Ordenar por fecha descendente
        })

      setComunicados(comunicadosResult)

    } catch (error: any) {
      setError(error.message || "Error al cargar los comunicados")
    } finally {
      setLoading(false)
    }
  }

  // Función para obtener comunicados de un alumno
  const getComunicadosAlumnosAsync = async (alumnoId: number): Promise<ComunicadoAlumnos[]> => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_alumno==${alumnoId}`)
      const url = `${CONFIG.API_BASE_URL}/ComunicadoAlumnos/GetComunicadoAlumnossForCombo?query=${queryParam}`

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: ComunicadoAlumnos[] = await response.json()
      return data || []
    } catch (error) {
      throw error
    }
  }

  // Función para obtener alumnos de un comunicado
  const getAlumnosComunicadoAsync = async (comunicadoId: number): Promise<ComunicadoAlumnos[]> => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_comunicado == ${comunicadoId}`)
      const url = `${CONFIG.API_BASE_URL}/ComunicadoAlumnos/GetComunicadoAlumnossForCombo?query=${queryParam}`

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: ComunicadoAlumnos[] = await response.json()
      return data || []
    } catch (error) {
      throw error
    }
  }

  const formatDate = (dateString?: string | null): string => {
    if (!dateString) return "Fecha no disponible"

    try {
      const date = new Date(dateString)
      return date.toLocaleDateString("es-ES", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      })
    } catch (error) {
      return "Fecha inválida"
    }
  }

  const handleEditarComunicado = (comunicado: ComunicadoViewModel) => {
    navigation.navigate("CreateComunicado", {
      cursoId: cursoId,
      cursoNombre: cursoNombre,
      alumnosIds: alumnosIds,
      comunicadoId: comunicado.id_Comunicado,
    })
  }

  const handleEliminarComunicado = (comunicado: ComunicadoViewModel) => {
    Alert.alert(
      "¿Estás seguro?",
      "¿Deseas eliminar este comunicado?",
      [
        {
          text: "Cancelar",
          style: "cancel",
        },
        {
          text: "Sí, eliminar",
          style: "destructive",
          onPress: () => eliminarComunicado(comunicado),
        },
      ],
      { cancelable: true },
    )
  }

  const eliminarComunicado = async (comunicado: ComunicadoViewModel) => {
    try {

      // Eliminar relaciones ComunicadoAlumnos primero
      const idsArray = comunicado.ids.split(", ").map((id) => Number.parseInt(id.trim()))

      if (idsArray.length > 0) {
        const comunicadoAlumnosEliminar = idsArray.map(id => ({ Id: id }))
        
        const deleteRelacionesResponse = await fetch(`${CONFIG.API_BASE_URL}/ComunicadoAlumnos/DeleteAllComunicadoAlumnos`, {
          method: "DELETE",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(comunicadoAlumnosEliminar),
        })

        if (!deleteRelacionesResponse.ok) {
          const errorResponse = await deleteRelacionesResponse.text()
          throw new Error(`Error al eliminar relaciones comunicado-alumnos: ${errorResponse}`)
        }
      }

      // Eliminar el comunicado principal
      await eliminarComunicadoAsync(comunicado.id_Comunicado)

      // Actualizar la lista
      await fetchComunicados()
      Alert.alert("Éxito", "Comunicado eliminado correctamente")
    } catch (error: any) {
      Alert.alert("Error", "No se pudo eliminar el comunicado")
    }
  }

  const eliminarComunicadoAsync = async (comunicadoId: number) => {
    const response = await fetch(
      `${CONFIG.API_BASE_URL}/CuadernoComunicados/DeleteCuadernoComunicados/${comunicadoId}`,
      {
        method: "DELETE",
      },
    )

    if (!response.ok) {
      throw new Error("Error al eliminar el comunicado")
    }
  }

  const handleVolver = () => {
    navigation.navigate("Cuaderno", {
      cursoId: cursoId,
      cursoNombre: cursoNombre,
    })
  }

  const renderComunicadoItem = ({ item }: { item: ComunicadoViewModel }) => (
    <View style={styles.comunicadoCard}>
      <View style={styles.cardHeader}>
        <Text style={styles.alumnosText}>
          <Text style={styles.alumnosLabel}>Alumnos: </Text>
          {item.alumnosConcatenados}
        </Text>
      </View>

      <View style={styles.cardBody}>
        <Text style={styles.comunicadoDescripcion}>{item.descripcion}</Text>
      </View>

      <View style={styles.cardFooter}>
        <Text style={styles.fechaText}>
          <Text style={styles.fechaLabel}>Fecha: </Text>
          {formatDate(item.fecha)}
        </Text>

        {userData && userData.id_perfil !== 2 && userData.id_perfil !== 4 && (
          <View style={styles.actionsContainer}>
            <TouchableOpacity style={styles.editButton} onPress={() => handleEditarComunicado(item)}>
              <Icon name="edit" size={20} color="#4285F4" />
            </TouchableOpacity>
            <TouchableOpacity style={styles.deleteButton} onPress={() => handleEliminarComunicado(item)}>
              <Icon name="delete" size={20} color="#F44336" />
            </TouchableOpacity>
          </View>
        )}
      </View>
    </View>
  )

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando comunicados...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar comunicados</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchComunicados}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (comunicados.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="message" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>No hay comunicados para los alumnos seleccionados</Text>
        </View>
      )
    }

    return (
      <View style={styles.mainContent}>
        <FlatList
          data={comunicados}
          renderItem={renderComunicadoItem}
          keyExtractor={(item) => item.id_Comunicado.toString()}
          contentContainerStyle={styles.listContainer}
          showsVerticalScrollIndicator={false}
        />
      </View>
    )
  }

  return (
    <AppLayout title={`Comunicados - ${cursoNombre}`} showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Comunicados</Text>
          <Text style={styles.headerSubtitle}>{cursoNombre}</Text>
        </View>

        {renderContent()}

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
  mainContent: {
    flex: 1,
  },
  listContainer: {
    padding: 16,
  },
  comunicadoCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    marginBottom: 16,
    elevation: 3,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.15,
    shadowRadius: 4,
    overflow: "hidden",
  },
  cardHeader: {
    backgroundColor: "#3183C4",
    padding: 12,
  },
  alumnosText: {
    color: "#FFFFFF",
    fontSize: 14,
  },
  alumnosLabel: {
    fontWeight: "bold",
  },
  cardBody: {
    padding: 16,
  },
  comunicadoDescripcion: {
    fontSize: 15,
    color: "#333",
    lineHeight: 22,
  },
  cardFooter: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: 12,
    backgroundColor: "#F8F9FA",
    borderTopWidth: 1,
    borderTopColor: "#E9ECEF",
  },
  fechaText: {
    fontSize: 14,
    color: "#666",
  },
  fechaLabel: {
    fontWeight: "bold",
  },
  actionsContainer: {
    flexDirection: "row",
    alignItems: "center",
  },
  editButton: {
    padding: 8,
    marginRight: 8,
    backgroundColor: "#E3F2FD",
    borderRadius: 20,
  },
  deleteButton: {
    padding: 8,
    backgroundColor: "#FFEBEE",
    borderRadius: 20,
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
    paddingHorizontal: 32,
  },
})

export default ListaComunicadosScreen
