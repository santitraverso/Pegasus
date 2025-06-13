"use client"

import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, TouchableOpacity, ActivityIndicator, Alert, ScrollView } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { Asistencia } from "../models/asistencia"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"
import { generateAsistenciaReport } from "../services/reportService"

// Extender el tipo de navegación para incluir la nueva ruta
type ExtendedRootStackParamList = RootStackParamList & {
  ReporteAsistencia: {
    cursoId: number
    cursoNombre: string
    materiaId: number
    materiaNombre: string
    fecha: string
  }
}

type ReporteAsistenciaScreenRouteProp = RouteProp<ExtendedRootStackParamList, "ReporteAsistencia">
type ReporteAsistenciaScreenNavigationProp = NativeStackNavigationProp<ExtendedRootStackParamList, "ReporteAsistencia">

const ReporteAsistenciaScreen: React.FC = () => {
  const route = useRoute<ReporteAsistenciaScreenRouteProp>()
  const navigation = useNavigation<ReporteAsistenciaScreenNavigationProp>()
  const { userData, hijoSeleccionado } = useUser()
  const { cursoId, cursoNombre, materiaId, materiaNombre, fecha } = route.params

  const [alumnos, setAlumnos] = useState<Asistencia[]>([])
  const [loading, setLoading] = useState(true)
  const [generating, setGenerating] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    cargarAsistencia()
  }, [])

  const cargarAsistencia = async () => {
    try {
      setLoading(true)
      setError(null)

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      // Obtener asistencia para la fecha seleccionada
      const asistencias = await getAsistenciaAsync(materiaId, new Date(fecha), cursoId)
      // Filtrar según el perfil
      if (userData?.id_perfil === 2) {
        // Alumno - solo ver su propia asistencia
        setAlumnos(asistencias.filter((a) => a.id_Alumno === userData.id))
      } else if (userData?.id_perfil === 4) {
        // Padre - solo ver asistencia de su hijo
        if (hijoSeleccionado?.hijoUsuario?.id) {
          setAlumnos(asistencias.filter((a) => a.id_Alumno === hijoSeleccionado?.hijoUsuario?.id))
        }
      } else {
        // Administrador/Docente - ver todas las asistencias
        setAlumnos(asistencias)
      }
    } catch (error: any) {
      setError(error.message || "Error al cargar los datos de asistencia")
    } finally {
      setLoading(false)
    }
  }

  // Función para obtener asistencia
  const getAsistenciaAsync = async (materia: number, fecha: Date, curso: number): Promise<Asistencia[]> => {
    try {
      // Formatear fecha para la consulta
      const fechaString = fecha.toISOString().split("T")[0]

      const queryParam = encodeURIComponent(`x=>x.id_Materia==${materia} && x.id_Curso==${curso}`)
      const url = `${CONFIG.API_BASE_URL}/Asistencia/GetAsistenciasForCombo?query=${queryParam}`

      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: Asistencia[] = await response.json()

      // Filtrar por la fecha específica
      return (data || []).filter((a) => {
        if (!a.fecha) return false
        const asistenciaFecha = new Date(a.fecha).toISOString().split("T")[0]
        return asistenciaFecha === fechaString
      })
    } catch (error) {
      throw error
    }
  }

  const generarReporteHTML = async () => {
    try {
      setGenerating(true)
      setError(null)

      // Preparar datos para el reporte
      const reportData = {
        cursoNombre,
        materiaNombre,
        fecha: formatDate(new Date(fecha)),
        alumnos: alumnos.map((alumno) => ({
          alumno: {
            apellido: alumno.alumno?.apellido || "",
            nombre: alumno.alumno?.nombre || "",
          },
          presente: alumno.presente,
        })),
        estadisticas: {
          total: alumnos.length,
          presentes: alumnos.filter((a) => a.presente).length,
          ausentes: alumnos.filter((a) => !a.presente).length,
          porcentajePresentes:
            alumnos.length > 0 ? Math.round((alumnos.filter((a) => a.presente).length / alumnos.length) * 100) : 0,
        },
      }

      // Generar el reporte
      const filePath = await generateAsistenciaReport(reportData, true)
    } catch (error: any) {
      setError(error.message || "Error al generar el reporte")
      Alert.alert("Error", error.message || "Error al generar el reporte")
    } finally {
      setGenerating(false)
    }
  }

  const handleVolver = () => {
    navigation.goBack()
  }

  const formatDate = (date: Date): string => {
    return date.toLocaleDateString("es-ES", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    })
  }

  const formatDateWithDay = (date: Date): string => {
    return date.toLocaleDateString("es-ES", {
      weekday: "long",
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    })
  }

  const renderAlumnoItem = (alumno: Asistencia, index: number) => (
    <View key={index} style={styles.alumnoRow}>
      <View style={styles.alumnoInfo}>
        <Text style={styles.alumnoApellido}>{alumno.alumno?.apellido}</Text>
        <Text style={styles.alumnoNombre}>{alumno.alumno?.nombre}</Text>
      </View>
      <View style={styles.estadoContainer}>
        <View style={[styles.estadoBadge, alumno.presente ? styles.presenteBadge : styles.ausenteBadge]}>
          <Text style={[styles.estadoText, alumno.presente ? styles.presenteText : styles.ausenteText]}>
            {alumno.presente ? "Presente" : "Ausente"}
          </Text>
        </View>
      </View>
    </View>
  )

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando reporte...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar reporte</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={cargarAsistencia}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (alumnos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="event-busy" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>No hay datos de asistencia para esta fecha</Text>
        </View>
      )
    }

    return (
      <View style={styles.contentContainer}>
        {/* Lista de alumnos */}
        <View style={styles.listContainer}>
          <View style={styles.listHeader}>
            <Text style={styles.listTitle}>Detalle de Asistencia</Text>
            <Text style={styles.listSubtitle}>{formatDateWithDay(new Date(fecha))}</Text>
          </View>

          <ScrollView style={styles.scrollContainer} contentContainerStyle={styles.scrollContentContainer}>
            {alumnos.map((alumno, index) => renderAlumnoItem(alumno, index))}
            <View style={styles.spacer} />
          </ScrollView>
        </View>
      </View>
    )
  }

  return (
    <AppLayout title="Reporte de Asistencia" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Reporte de Asistencia</Text>
          <Text style={styles.headerSubtitle}>
            {cursoNombre} - {materiaNombre}
          </Text>
        </View>

        {/* Contenido principal */}
        <View style={styles.mainContent}>{renderContent()}</View>

        {/* Botones de acción */}
        <View style={styles.buttonsContainer}>
          <TouchableOpacity
            style={[styles.generateButton, generating && styles.disabledButton]}
            onPress={generarReporteHTML}
            disabled={generating || loading || alumnos.length === 0}
          >
            {generating ? (
              <ActivityIndicator size="small" color="#FFFFFF" />
            ) : (
              <Icon name="download" size={20} color="#FFFFFF" />
            )}
            <Text style={styles.buttonText}>{generating ? "Generando..." : "Descargar"}</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.backButton} onPress={handleVolver} disabled={generating || loading}>
            <Icon name="arrow-back" size={20} color="#FFFFFF" />
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
  contentContainer: {
    flex: 1,
    padding: 16,
  },
  statsContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 16,
  },
  statCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    padding: 16,
    alignItems: "center",
    flex: 1,
    marginHorizontal: 4,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  presentesCard: {
    borderLeftWidth: 4,
    borderLeftColor: "#4CAF50",
  },
  ausentesCard: {
    borderLeftWidth: 4,
    borderLeftColor: "#F44336",
  },
  statNumber: {
    fontSize: 24,
    fontWeight: "bold",
    color: "#333",
  },
  presentesNumber: {
    color: "#4CAF50",
  },
  ausentesNumber: {
    color: "#F44336",
  },
  statLabel: {
    fontSize: 12,
    color: "#666",
    marginTop: 4,
  },
  listContainer: {
    flex: 1,
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  listHeader: {
    padding: 16,
    borderBottomWidth: 1,
    borderBottomColor: "#E0E0E0",
  },
  listTitle: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
  },
  listSubtitle: {
    fontSize: 14,
    color: "#666",
    marginTop: 4,
    textTransform: "capitalize",
  },
  scrollContainer: {
    flex: 1,
  },
  scrollContentContainer: {
    paddingVertical: 8,
  },
  alumnoRow: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  alumnoInfo: {
    flex: 1,
  },
  alumnoApellido: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
  },
  alumnoNombre: {
    fontSize: 14,
    color: "#666",
    marginTop: 2,
  },
  estadoContainer: {
    minWidth: 100,
    alignItems: "flex-end",
  },
  estadoBadge: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 16,
  },
  presenteBadge: {
    backgroundColor: "#E8F5E9",
  },
  ausenteBadge: {
    backgroundColor: "#FFEBEE",
  },
  estadoText: {
    fontSize: 12,
    fontWeight: "600",
  },
  presenteText: {
    color: "#4CAF50",
  },
  ausenteText: {
    color: "#F44336",
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 16,
    paddingBottom: 32,
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  generateButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#4285F4",
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderRadius: 8,
    flex: 1,
    marginRight: 8,
    justifyContent: "center",
  },
  backButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#6C7B7F",
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderRadius: 8,
    flex: 1,
    marginLeft: 8,
    justifyContent: "center",
  },
  disabledButton: {
    opacity: 0.6,
  },
  buttonText: {
    color: "#FFFFFF",
    fontSize: 14,
    fontWeight: "600",
    marginLeft: 8,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    padding: 20,
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
    padding: 20,
  },
  emptyText: {
    fontSize: 16,
    color: "#666",
    marginTop: 16,
    textAlign: "center",
  },
  spacer: {
    height: 80,
  },
})

export default ReporteAsistenciaScreen
