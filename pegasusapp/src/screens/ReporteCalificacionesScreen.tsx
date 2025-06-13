"use client"

import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, FlatList, ActivityIndicator, TouchableOpacity, Alert } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { IntegrantesCursos } from "../models/integrantesCursos"
import type { Calificaciones } from "../models/calificaciones"
import type { AlumnoConCalificaciones } from "../models/alumnoConCalificaciones"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

// Extender el tipo de navegación para incluir la nueva ruta
type ExtendedRootStackParamList = RootStackParamList & {
  ReporteCalificaciones: {
    cursoId: number
    cursoNombre: string
    materiaId: number
    materiaNombre: string
  }
}

type ReporteCalificacionesScreenRouteProp = RouteProp<ExtendedRootStackParamList, "ReporteCalificaciones">
type ReporteCalificacionesScreenNavigationProp = NativeStackNavigationProp<
  ExtendedRootStackParamList,
  "ReporteCalificaciones"
>

const ReporteCalificacionesScreen: React.FC = () => {
  const route = useRoute<ReporteCalificacionesScreenRouteProp>()
  const navigation = useNavigation<ReporteCalificacionesScreenNavigationProp>()
  const { userData, hijoSeleccionado } = useUser()
  const { cursoId, cursoNombre, materiaId, materiaNombre } = route.params

  const [alumnos, setAlumnos] = useState<AlumnoConCalificaciones[]>([])
  const [loading, setLoading] = useState(true)
  const [generatingReport, setGeneratingReport] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchAlumnosConCalificaciones()
  }, [cursoId, materiaId])

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
            integrantesCursos = await getIntegrantesCursosAsync(cursoId, hijoSeleccionado?.hijoUsuario?.id)
          }
          break
        default: // Administrador/Docente
          integrantesCursos = await getIntegrantesCursosAsync(cursoId)
          break
      }
      // Para cada alumno, obtener sus calificaciones (incluso si no tiene)
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
      setError(error.message || "Error al cargar los datos del reporte")
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

  const calcularPromedio = (calificaciones: Calificaciones[]): number => {
    if (calificaciones.length === 0) return 0
    const suma = calificaciones.reduce((acc, cal) => acc + cal.calificacion, 0)
    return Math.round((suma / calificaciones.length) * 100) / 100
  }

  const getColorCalificacion = (calificacion: number): string => {
    if (calificacion === 0) return "#999999" // Gris para sin calificaciones
    return calificacion >= 6 ? "#4CAF50" : "#F44336" // Verde para aprobado, rojo para desaprobado
  }

  const generateReport = async () => {
    try {
      setGeneratingReport(true)

      // Preparar datos para el reporte
      const reportData = {
        cursoNombre: cursoNombre,
        materiaNombre: materiaNombre,
        alumnos: alumnos,
        subtitulo: `Generado por: ${userData?.name || "Usuario"}`,
      }

      try {
        // Usar el servicio de reportes simplificado
        const { generateCalificacionesReport } = await import("../services/reportService")
        const filePath = await generateCalificacionesReport(reportData, true)
      } catch (error: any) {
        Alert.alert("Error", `No se pudo generar el reporte: ${error.message}`)
      }
    } catch (error: any) {
      Alert.alert("❌ Error", "No se pudo generar el reporte. Intenta nuevamente.")
    } finally {
      setGeneratingReport(false)
    }
  }

  const handleVolver = () => {
    navigation.goBack()
  }

  const renderCalificaciones = (calificaciones: Calificaciones[]) => {
    if (calificaciones.length === 0) {
      return (
        <View style={styles.calificacionesContainer}>
          <Text style={styles.sinCalificaciones}>Sin calificaciones</Text>
          <View style={styles.promedioContainer}>
            <Text style={styles.promedioLabel}>Promedio:</Text>
            <Text style={[styles.promedioValor, { color: getColorCalificacion(0) }]}>N/A</Text>
          </View>
        </View>
      )
    }

    return (
      <View style={styles.calificacionesContainer}>
        {calificaciones.map((calificacion, index) => (
          <View key={calificacion.id} style={styles.calificacionItem}>
            <Text style={styles.notaLabel}>Nota {index + 1}:</Text>
            <Text style={[styles.notaValor, { color: getColorCalificacion(calificacion.calificacion) }]}>
              {calificacion.calificacion}
            </Text>
          </View>
        ))}
        <View style={styles.promedioContainer}>
          <Text style={styles.promedioLabel}>Promedio:</Text>
          <Text style={[styles.promedioValor, { color: getColorCalificacion(calcularPromedio(calificaciones)) }]}>
            {calcularPromedio(calificaciones)}
          </Text>
        </View>
      </View>
    )
  }

  const renderAlumnoItem = ({ item }: { item: AlumnoConCalificaciones }) => (
    <View style={[styles.alumnoCard, item.calificaciones.length === 0 && styles.alumnoSinCalificaciones]}>
      <View style={styles.alumnoHeader}>
        <Icon name="person" size={20} color={item.calificaciones.length === 0 ? "#999999" : "#4285F4"} />
        <Text style={[styles.alumnoNombre, item.calificaciones.length === 0 && styles.nombreSinCalificaciones]}>
          <Text style={styles.apellido}>{item.usuario.apellido}</Text>
          {", "}
          <Text style={styles.nombre}>{item.usuario.nombre}</Text>
        </Text>
        {item.calificaciones.length === 0 && (
          <View style={styles.badgeSinCalificaciones}>
            <Text style={styles.badgeText}>Sin notas</Text>
          </View>
        )}
      </View>
      <View style={styles.calificacionesSection}>{renderCalificaciones(item.calificaciones)}</View>
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
          <TouchableOpacity style={styles.retryButton} onPress={fetchAlumnosConCalificaciones}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (alumnos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="assignment" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>No hay alumnos en este curso</Text>
        </View>
      )
    }

    // Separar alumnos con y sin calificaciones para estadísticas
    const alumnosConCalificaciones = alumnos.filter((alumno) => alumno.calificaciones.length > 0)
    const alumnosSinCalificaciones = alumnos.filter((alumno) => alumno.calificaciones.length === 0)

    return (
      <>
        {/* Estadísticas del reporte */}
        <View style={styles.statsContainer}>
          <View style={styles.statItem}>
            <Text style={styles.statNumber}>{alumnos.length}</Text>
            <Text style={styles.statLabel}>Total alumnos</Text>
          </View>
          <View style={styles.statItem}>
            <Text style={[styles.statNumber, { color: "#4CAF50" }]}>{alumnosConCalificaciones.length}</Text>
            <Text style={styles.statLabel}>Con calificaciones</Text>
          </View>
          <View style={styles.statItem}>
            <Text style={[styles.statNumber, { color: "#FF9800" }]}>{alumnosSinCalificaciones.length}</Text>
            <Text style={styles.statLabel}>Sin calificaciones</Text>
          </View>
        </View>

        <FlatList
          data={alumnos}
          renderItem={renderAlumnoItem}
          keyExtractor={(item) => item.id_Usuario.toString()}
          contentContainerStyle={styles.listContainer}
          showsVerticalScrollIndicator={false}
        />
      </>
    )
  }

  return (
    <AppLayout title="Reporte de Calificaciones" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Reporte de Calificaciones</Text>
          <Text style={styles.headerSubtitle}>
            {cursoNombre} - {materiaNombre}
          </Text>
        </View>

        {renderContent()}

        {/* Botones de acción */}
        <View style={styles.buttonsContainer}>
          <TouchableOpacity
            style={[styles.downloadButton, generatingReport && styles.disabledButton]}
            onPress={generateReport}
            disabled={generatingReport || alumnos.length === 0}
          >
            {generatingReport ? (
              <ActivityIndicator size="small" color="#FFFFFF" />
            ) : (
              <Icon name="download" size={20} color="#FFFFFF" />
            )}
            <Text style={styles.buttonText}>{generatingReport ? "Generando..." : "Descargar"}</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.backButton} onPress={handleVolver} disabled={generatingReport}>
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
    textAlign: "center",
  },
  headerSubtitle: {
    fontSize: 14,
    color: "#666",
    marginTop: 4,
    textAlign: "center",
  },
  statsContainer: {
    flexDirection: "row",
    backgroundColor: "#FFFFFF",
    margin: 16,
    borderRadius: 8,
    padding: 16,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  statItem: {
    flex: 1,
    alignItems: "center",
  },
  statNumber: {
    fontSize: 24,
    fontWeight: "bold",
    color: "#4285F4",
  },
  statLabel: {
    fontSize: 12,
    color: "#666",
    marginTop: 4,
    textAlign: "center",
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
  alumnoSinCalificaciones: {
    backgroundColor: "#FAFAFA",
    borderLeftWidth: 4,
    borderLeftColor: "#FF9800",
  },
  alumnoHeader: {
    flexDirection: "row",
    alignItems: "center",
    paddingBottom: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  alumnoNombre: {
    marginLeft: 8,
    fontSize: 16,
    color: "#333",
    flex: 1,
  },
  nombreSinCalificaciones: {
    color: "#666",
  },
  apellido: {
    fontWeight: "600",
  },
  nombre: {
    fontWeight: "400",
  },
  badgeSinCalificaciones: {
    backgroundColor: "#FF9800",
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  badgeText: {
    color: "#FFFFFF",
    fontSize: 10,
    fontWeight: "600",
  },
  calificacionesSection: {
    marginTop: 12,
  },
  calificacionesContainer: {
    backgroundColor: "#FAFBFC",
    borderRadius: 6,
    padding: 12,
  },
  calificacionItem: {
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
  },
  promedioContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingVertical: 8,
    paddingHorizontal: 8,
    marginTop: 8,
    backgroundColor: "#F8F9FA",
    borderRadius: 4,
    borderWidth: 1,
    borderColor: "#E9ECEF",
  },
  promedioLabel: {
    fontSize: 14,
    color: "#333",
    fontWeight: "600",
  },
  promedioValor: {
    fontSize: 16,
    fontWeight: "bold",
  },
  sinCalificaciones: {
    fontSize: 14,
    color: "#999",
    fontStyle: "italic",
    textAlign: "center",
    paddingVertical: 12,
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 16,
    paddingBottom: 32, // Aumentar padding bottom
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  downloadButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#4285F4",
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
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderRadius: 8,
    flex: 1,
    marginLeft: 4,
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

export default ReporteCalificacionesScreen
