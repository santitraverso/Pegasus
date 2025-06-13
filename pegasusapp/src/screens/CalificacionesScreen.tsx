import type React from "react"
import { useState, useEffect, useCallback } from "react"
import { View, Text, StyleSheet, FlatList, ActivityIndicator, TextInput, TouchableOpacity } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation, useFocusEffect } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { IntegrantesCursos } from "../models/integrantesCursos"
import type { Calificaciones } from "../models/calificaciones"
import type { AlumnoConCalificaciones } from "../models/alumnoConCalificaciones"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

type CalificacionesScreenRouteProp = RouteProp<RootStackParamList, "Calificacion">
type CalificacionesScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "Calificacion">

const CalificacionesScreen: React.FC = () => {
  const route = useRoute<CalificacionesScreenRouteProp>()
  const navigation = useNavigation<CalificacionesScreenNavigationProp>()
  const { userData, hijoSeleccionado } = useUser()
  const { cursoId, cursoNombre, materiaId, materiaNombre } = route.params

  const [alumnos, setAlumnos] = useState<AlumnoConCalificaciones[]>([])
  const [filteredAlumnos, setFilteredAlumnos] = useState<AlumnoConCalificaciones[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [searchText, setSearchText] = useState("")

  // Usar useFocusEffect para refrescar cuando la pantalla recibe foco
  useFocusEffect(
    useCallback(() => {
      fetchAlumnosConCalificaciones()
    }, [cursoId, materiaId]),
  )

  useEffect(() => {
    // Filtrar alumnos cuando cambia el texto de búsqueda
    if (searchText.trim() === "") {
      setFilteredAlumnos(alumnos)
    } else {
      const filtered = alumnos.filter((alumno) => {
        const nombreCompleto = `${alumno.usuario.nombre} ${alumno.usuario.apellido}`.toLowerCase()
        return nombreCompleto.includes(searchText.toLowerCase())
      })
      setFilteredAlumnos(filtered)
    }
  }, [searchText, alumnos])

  const fetchAlumnosConCalificaciones = async () => {
    try {
      setLoading(true)
      setError(null)

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      let integrantesCursos: IntegrantesCursos[] = []
      // Obtener alumnos según el perfil
      switch (userData.id_perfil) {
        case 2: // Alumno
          integrantesCursos = await getIntegrantesCursosAsync(cursoId, userData.id)
          break
        case 4: // Padre
          if (hijoSeleccionado?.hijoUsuario?.id) {
            integrantesCursos = await getIntegrantesCursosAsync(cursoId, hijoSeleccionado.hijoUsuario.id)
          } else {
            throw new Error("No hay hijo seleccionado")
          }
          break
        default: // Administrador/Docente
          integrantesCursos = await getIntegrantesCursosAsync(cursoId)
          break
      }
      // Para cada alumno, obtener sus calificaciones
      const alumnosConCalificaciones: AlumnoConCalificaciones[] = await Promise.all(
        integrantesCursos.map(async (integrante) => {
          const calificaciones = await getCalificacionesAsync(materiaId, cursoId, integrante.id_Usuario!)
          return {
            id_Usuario: integrante.id_Usuario!,
            id_Materia: materiaId,
            usuario: integrante.usuario!,
            calificaciones: calificaciones,
          }
        }),
      )
      setAlumnos(alumnosConCalificaciones)
      
    } catch (error: any) {
      setError(error.message || "Error al cargar las calificaciones")
    } finally {
      setLoading(false)
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

  // Función para obtener calificaciones
  const getCalificacionesAsync = async (materia: number, curso: number, usuario: number): Promise<Calificaciones[]> => {
    try {
      const queryParam = encodeURIComponent(
        `x=>x.id_materia==${materia} && x.id_curso==${curso} && x.id_alumno==${usuario}`,
      )
      const url = `${CONFIG.API_BASE_URL}/Calificaciones/GetCalificacionesForCombo?query=${queryParam}`

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: Calificaciones[] = await response.json()
      return data || []
    } catch (error) {
      throw error
    }
  }

  const handleEditarCalificaciones = (alumno: AlumnoConCalificaciones) => {
    navigation.navigate("CreateCalificacion", {
      alumnoId: alumno.id_Usuario,
      alumnoNombre: alumno.usuario.nombre || "",
      alumnoApellido: alumno.usuario.apellido || "",
      cursoId: cursoId,
      cursoNombre: cursoNombre,
      materiaId: materiaId,
      materiaNombre: materiaNombre,
      esNuevo: false,
    })
  }

  const handleReporte = () => {
    navigation.navigate("ReporteCalificaciones", {
      cursoId: cursoId,
      cursoNombre: cursoNombre,
      materiaId: materiaId,
      materiaNombre: materiaNombre,
    })
  }

  const handleVolver = () => {
    navigation.goBack()
  }

  const renderCalificaciones = (calificaciones: Calificaciones[]) => {
    if (calificaciones.length === 0) {
      return (
        <View style={styles.calificacionesTable}>
          <Text style={styles.noCalificaciones}>Sin calificaciones</Text>
        </View>
      )
    }

    return (
      <View style={styles.calificacionesTable}>
        {calificaciones.map((calificacion, index) => (
          <View key={calificacion.id} style={styles.calificacionRow}>
            <Text style={styles.notaLabel}>Nota {index + 1}</Text>
            <Text style={styles.notaValor}>{calificacion.calificacion}</Text>
          </View>
        ))}
      </View>
    )
  }

  const renderAlumnoItem = ({ item }: { item: AlumnoConCalificaciones }) => (
    <View style={styles.alumnoCard}>
      {/* Cabecera con nombre y apellido */}
      <View style={styles.alumnoHeader}>
        <Icon name="person" size={24} color="#4285F4" />
        <Text style={styles.alumnoNombreCompleto}>
          <Text style={styles.apellido}>{item.usuario.apellido}</Text>
          {", "}
          <Text style={styles.nombre}>{item.usuario.nombre}</Text>
        </Text>
      </View>

      {/* Contenido principal con calificaciones y botones */}
      <View style={styles.cardContent}>
        {/* Calificaciones en grilla */}
        <View style={styles.calificacionesContainer}>
          <Text style={styles.calificacionesTitle}>Calificaciones:</Text>
          <View style={styles.tableAndActionsRow}>
            {renderCalificaciones(item.calificaciones)}
            {/* Solo mostrar botón de editar (solo para administradores y docentes) */}
            {userData && userData.id_perfil !== 2 && userData.id_perfil !== 4 && (
              <View style={styles.actionsContainer}>
                <TouchableOpacity style={styles.actionButton} onPress={() => handleEditarCalificaciones(item)}>
                  <Icon name="edit" size={18} color="#4285F4" />
                </TouchableOpacity>
              </View>
            )}
          </View>
        </View>
      </View>
    </View>
  )

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando calificaciones...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar calificaciones</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchAlumnosConCalificaciones}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (filteredAlumnos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="grade" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>
            {searchText ? "No se encontraron alumnos" : "No hay alumnos en este curso"}
          </Text>
        </View>
      )
    }

    return (
      <FlatList
        data={filteredAlumnos}
        renderItem={renderAlumnoItem}
        keyExtractor={(item) => item.id_Usuario.toString()}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
      />
    )
  }

  return (
    <AppLayout title={`Calificaciones - ${materiaNombre}`} showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Calificaciones</Text>
          <Text style={styles.headerSubtitle}>
            {cursoNombre} - {materiaNombre}
          </Text>
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

        {/* Lista de alumnos */}
        {renderContent()}

        {/* Botones de acción */}
        <View style={styles.buttonsContainer}>
          <TouchableOpacity style={styles.reportButton} onPress={handleReporte}>
            <Icon name="assessment" size={20} color="#FFFFFF" />
            <Text style={styles.buttonText}>Reporte</Text>
          </TouchableOpacity>

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
  listContainer: {
    padding: 16,
    paddingTop: 0,
  },
  alumnoCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    padding: 16,
    marginBottom: 12,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  alumnoHeader: {
    flexDirection: "row",
    alignItems: "center",
    paddingBottom: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  alumnoNombreCompleto: {
    marginLeft: 12,
    fontSize: 16,
    color: "#333",
  },
  apellido: {
    fontWeight: "600",
  },
  nombre: {
    fontWeight: "400",
  },
  cardContent: {
    marginTop: 12,
  },
  calificacionesContainer: {
    flex: 1,
  },
  calificacionesTitle: {
    fontSize: 14,
    fontWeight: "600",
    color: "#333",
    marginBottom: 8,
  },
  tableAndActionsRow: {
    flexDirection: "row",
    alignItems: "center",
  },
  calificacionesTable: {
    backgroundColor: "#FAFBFC",
    borderRadius: 6,
    padding: 8,
    flex: 1,
  },
  calificacionRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingVertical: 4,
    paddingHorizontal: 8,
    borderBottomWidth: 1,
    borderBottomColor: "#E9ECEF",
  },
  notaLabel: {
    fontSize: 14,
    color: "#666",
    fontWeight: "500",
  },
  notaValor: {
    fontSize: 14,
    fontWeight: "600",
    color: "#4285F4",
  },
  noCalificaciones: {
    fontSize: 14,
    color: "#999",
    fontStyle: "italic",
    textAlign: "center",
    paddingVertical: 8,
  },
  actionsContainer: {
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    marginLeft: 12,
  },
  actionButton: {
    padding: 6,
    marginBottom: 6,
    justifyContent: "center",
    backgroundColor: "#E3F2FD",
    borderRadius: 20,
    alignItems: "center",
    width: 32,
    height: 32,
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

export default CalificacionesScreen
