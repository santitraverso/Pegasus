import type React from "react"
import { useState, useEffect } from "react"
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  Switch,
  Modal,
  ScrollView,
} from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { IntegrantesCursos } from "../models/integrantesCursos"
import type { Asistencia } from "../models/asistencia"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

// Extender el tipo de navegación para incluir la nueva ruta
type ExtendedRootStackParamList = RootStackParamList & {
  Asistencia: {
    cursoId: number
    cursoNombre: string
    materiaId: number
    materiaNombre: string
  }
  ReporteAsistencia: {
    cursoId: number
    cursoNombre: string
    materiaId: number
    materiaNombre: string
    fecha: string
  }
}

type AsistenciaScreenRouteProp = RouteProp<ExtendedRootStackParamList, "Asistencia">
type AsistenciaScreenNavigationProp = NativeStackNavigationProp<ExtendedRootStackParamList, "Asistencia">

// Componente personalizado para seleccionar fecha
const CustomDatePicker: React.FC<{
  date: Date
  onDateChange: (date: Date) => void
  onClose: () => void
  visible: boolean
}> = ({ date, onDateChange, onClose, visible }) => {
  const [selectedDate, setSelectedDate] = useState(date)
  const [currentMonth, setCurrentMonth] = useState(new Date(date.getFullYear(), date.getMonth(), 1))

  useEffect(() => {
    setSelectedDate(date)
    setCurrentMonth(new Date(date.getFullYear(), date.getMonth(), 1))
  }, [date, visible])

  const handleDateSelect = (day: number) => {
    const newDate = new Date(currentMonth.getFullYear(), currentMonth.getMonth(), day)
    setSelectedDate(newDate)
  }

  const handleConfirm = () => {
    onDateChange(selectedDate)
    onClose()
  }

  const changeMonth = (increment: number) => {
    const newMonth = new Date(currentMonth)
    newMonth.setMonth(newMonth.getMonth() + increment)
    setCurrentMonth(newMonth)
  }

  const formatMonthYear = (date: Date): string => {
    return date.toLocaleDateString("es-ES", {
      month: "long",
      year: "numeric",
    })
  }

  const formatFullDate = (date: Date): string => {
    return date.toLocaleDateString("es-ES", {
      weekday: "long",
      day: "numeric",
      month: "long",
      year: "numeric",
    })
  }

  const getDaysInMonth = (year: number, month: number): number => {
    return new Date(year, month + 1, 0).getDate()
  }

  const getFirstDayOfMonth = (year: number, month: number): number => {
    return new Date(year, month, 1).getDay()
  }

  const renderCalendar = () => {
    const year = currentMonth.getFullYear()
    const month = currentMonth.getMonth()
    const daysInMonth = getDaysInMonth(year, month)
    const firstDayOfMonth = getFirstDayOfMonth(year, month)

    // Ajustar para que la semana comience en lunes (0 = lunes, 6 = domingo)
    const adjustedFirstDay = firstDayOfMonth === 0 ? 6 : firstDayOfMonth - 1

    const days = []
    const weekDays = ["L", "M", "X", "J", "V", "S", "D"]

    // Renderizar los días de la semana
    const dayLabels = weekDays.map((day, index) => (
      <View key={`day-label-${index}`} style={styles.dayLabelContainer}>
        <Text style={styles.dayLabel}>{day}</Text>
      </View>
    ))

    // Añadir espacios vacíos para los días anteriores al primer día del mes
    for (let i = 0; i < adjustedFirstDay; i++) {
      days.push(<View key={`empty-${i}`} style={styles.dayContainer} />)
    }

    // Añadir los días del mes
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(year, month, day)
      const isSelected =
        selectedDate.getDate() === day && selectedDate.getMonth() === month && selectedDate.getFullYear() === year

      const isToday =
        new Date().getDate() === day && new Date().getMonth() === month && new Date().getFullYear() === year

      days.push(
        <TouchableOpacity
          key={`day-${day}`}
          style={[styles.dayContainer, isSelected && styles.selectedDayContainer, isToday && styles.todayContainer]}
          onPress={() => handleDateSelect(day)}
        >
          <Text style={[styles.dayText, isSelected && styles.selectedDayText, isToday && styles.todayText]}>{day}</Text>
        </TouchableOpacity>,
      )
    }

    // Calcular cuántas filas necesitamos
    const totalCells = adjustedFirstDay + daysInMonth
    const rows = Math.ceil(totalCells / 7)

    // Crear la matriz del calendario
    const calendar = []

    // Añadir la fila de etiquetas de días
    calendar.push(
      <View key="weekdays" style={styles.weekRow}>
        {dayLabels}
      </View>,
    )

    // Añadir las filas de días
    for (let row = 0; row < rows; row++) {
      const rowCells = []
      for (let col = 0; col < 7; col++) {
        const index = row * 7 + col
        if (index < days.length) {
          rowCells.push(days[index])
        } else {
          rowCells.push(<View key={`empty-end-${index}`} style={styles.dayContainer} />)
        }
      }
      calendar.push(
        <View key={`row-${row}`} style={styles.weekRow}>
          {rowCells}
        </View>,
      )
    }

    return calendar
  }

  if (!visible) return null

  return (
    <Modal visible={visible} transparent animationType="fade">
      <View style={styles.modalOverlay}>
        <View style={styles.calendarModal}>
          <View style={styles.calendarHeader}>
            <Text style={styles.calendarTitle}>Seleccionar fecha</Text>
            <Text style={styles.selectedDateText}>{formatFullDate(selectedDate)}</Text>
          </View>

          <View style={styles.monthSelector}>
            <TouchableOpacity onPress={() => changeMonth(-1)} style={styles.monthButton}>
              <Icon name="chevron-left" size={24} color="#4285F4" />
            </TouchableOpacity>
            <Text style={styles.monthYearText}>{formatMonthYear(currentMonth)}</Text>
            <TouchableOpacity onPress={() => changeMonth(1)} style={styles.monthButton}>
              <Icon name="chevron-right" size={24} color="#4285F4" />
            </TouchableOpacity>
          </View>

          <View style={styles.calendar}>{renderCalendar()}</View>

          <View style={styles.calendarActions}>
            <TouchableOpacity style={styles.cancelButton} onPress={onClose}>
              <Text style={styles.cancelButtonText}>Cancelar</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.confirmButton} onPress={handleConfirm}>
              <Text style={styles.confirmButtonText}>Confirmar</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  )
}

const AsistenciaScreen: React.FC = () => {
  const route = useRoute<AsistenciaScreenRouteProp>()
  const navigation = useNavigation<AsistenciaScreenNavigationProp>()
  const { userData, hijoSeleccionado } = useUser()
  const { cursoId, cursoNombre, materiaId, materiaNombre } = route.params

  const [alumnos, setAlumnos] = useState<Asistencia[]>([])
  const [fecha, setFecha] = useState(new Date())
  const [showDatePicker, setShowDatePicker] = useState(false)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [existeAsistencia, setExisteAsistencia] = useState(false)

  useEffect(() => {
    cargarAsistencia()
  }, [fecha])

  const cargarAsistencia = async () => {
    try {
      setLoading(true)
      setError(null)

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      // Verificar si existe asistencia para la fecha seleccionada
      const existeAsistenciaParaFecha = await verificarAsistenciaExistente()
      setExisteAsistencia(existeAsistenciaParaFecha)

      if (existeAsistenciaParaFecha) {
        // Si existe asistencia, cargar los datos existentes
        await cargarAsistenciaExistente()
      } else {
        // Si no existe asistencia, cargar la lista de alumnos sin asistencia
        await cargarAlumnosSinAsistencia()
      }

    } catch (error: any) {
      setError(error.message || "Error al cargar los datos de asistencia")
    } finally {
      setLoading(false)
    }
  }

  const verificarAsistenciaExistente = async (): Promise<boolean> => {
    try {
      // Obtener asistencia para la fecha seleccionada
      const asistencias = await getAsistenciaAsync(materiaId, fecha, cursoId)

      // Verificar si hay asistencia según el perfil
      if (userData?.id_perfil === 2) {
        // Alumno - verificar si existe su propia asistencia
        return asistencias.some((a) => a.id_Alumno === userData.id)
      } else if (userData?.id_perfil === 4) {
        // Padre - verificar si existe asistencia para su hijo seleccionado
        if (hijoSeleccionado?.hijoUsuario?.id) {
          return asistencias.some((a) => a.id_Alumno === hijoSeleccionado.hijoUsuario?.id)
        } else {
          return false
        }
      } else {
        // Administrador/Docente - verificar si existe cualquier asistencia
        return asistencias.length > 0
      }
    } catch (error) {
      return false
    }
  }

  const cargarAsistenciaExistente = async () => {
    try {
       
        // Obtener asistencia para la fecha seleccionada
        const asistencias = await getAsistenciaAsync(materiaId, fecha, cursoId)

        // Filtrar según el perfil
        if (userData?.id_perfil === 2) {
          // Alumno - solo ver su propia asistencia
          setAlumnos(asistencias.filter((a) => a.id_Alumno === userData.id))
        } else if (userData?.id_perfil === 4) {
          // Padre - solo ver asistencia de su hijo
          if (hijoSeleccionado?.hijoUsuario?.id) {
            setAlumnos(asistencias.filter((a) => a.id_Alumno === hijoSeleccionado?.hijoUsuario?.id))
          } else {
            setAlumnos(asistencias.filter((a) => a.id_Alumno === userData.id))
          }
        } else {
          // Administrador/Docente - ver todas las asistencias
          setAlumnos(asistencias)
        }
      
    } catch (error) {
      throw error
    }
  }

  const cargarAlumnosSinAsistencia = async () => {
    try {

        // Obtener integrantes del curso según el perfil
        let integrantesCursos: IntegrantesCursos[] = []

        if (userData?.id_perfil === 2) {
          // Alumno - solo él mismo
          integrantesCursos = await getIntegrantesCursosAsync(cursoId, userData.id)
        } else if (userData?.id_perfil === 4) {
          // Padre - solo su hijo seleccionado
          if (hijoSeleccionado?.hijoUsuario?.id) {
            integrantesCursos = await getIntegrantesCursosAsync(cursoId, hijoSeleccionado.hijoUsuario.id)
          } else {
            throw new Error("No hay hijo seleccionado")
          }
        } else {
          // Administrador/Docente - todos los alumnos
          integrantesCursos = await getIntegrantesCursosAsync(cursoId)
        }

        // Convertir integrantes a formato de asistencia
        const nuevasAsistencias: Asistencia[] = integrantesCursos.map((integrante) => ({
          id: 0,
          id_Alumno: integrante.id_Usuario || 0,
          id_Materia: materiaId,
          id_Curso: cursoId,
          fecha: fecha.toISOString(),
          presente: false, 
          alumno: integrante.usuario || { id: 0, nombre: "", apellido: "", activo: true },
        }))

        setAlumnos(nuevasAsistencias)
      
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

      console.log("🔗 URL para integrantes:", url)

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

  // Función para obtener asistencia
  const getAsistenciaAsync = async (materia: number, fecha: Date, curso: number): Promise<Asistencia[]> => {
    try {
      // Formatear fecha para la consulta
      const fechaString = fecha.toISOString().split("T")[0]

      // Construir la consulta
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

  const handleDateChange = (selectedDate: Date) => {
    setFecha(selectedDate)
  }

  const togglePresente = (index: number) => {
    const nuevosAlumnos = [...alumnos]
    nuevosAlumnos[index].presente = !nuevosAlumnos[index].presente
    setAlumnos(nuevosAlumnos)
  }

  const guardarAsistencia = async () => {
  try {
    setSaving(true)
    setError(null)

    const asistenciasNuevas = alumnos.filter(alumno => alumno.id <= 0)
    const asistenciasExistentes = alumnos.filter(alumno => alumno.id > 0)

    const nuevasData = asistenciasNuevas.map(alumno => ({
      Id_Alumno: alumno.id_Alumno,
      Id_Materia: alumno.id_Materia,
      Id_Curso: alumno.id_Curso,
      Fecha: alumno.fecha,
      Presente: alumno.presente
    }))

    const existentesData = asistenciasExistentes.map(alumno => ({
      Id: alumno.id,
      Id_Alumno: alumno.id_Alumno,
      Id_Materia: alumno.id_Materia,
      Id_Curso: alumno.id_Curso,
      Fecha: alumno.fecha,
      Presente: alumno.presente
    }))

    // Crear nuevas asistencias
    if (nuevasData.length > 0) {
      const createResponse = await fetch(`${CONFIG.API_BASE_URL}/Asistencia/CreateAllAsistencia`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(nuevasData),
      })

      if (!createResponse.ok) {
        const errorResponse = await createResponse.text()
        throw new Error(`Error al crear asistencias: ${errorResponse}`)
      }
    }

    // Actualizar asistencias existentes
    if (existentesData.length > 0) {
      const updateResponse = await fetch(`${CONFIG.API_BASE_URL}/Asistencia/UpdateAllAsistencia`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(existentesData),
      })

      if (!updateResponse.ok) {
        const errorResponse = await updateResponse.text()
        throw new Error(`Error al actualizar asistencias: ${errorResponse}`)
      }
    }

    // Actualizar estado para mostrar que ahora existe asistencia
    setExisteAsistencia(true)

    Alert.alert("Éxito", "La asistencia se guardó correctamente")
  } catch (error: any) {
    setError(error.message || "Error al guardar la asistencia")
    Alert.alert("Error", error.message || "Error al guardar la asistencia")
  } finally {
    setSaving(false)
  }
}

  const generarReporte = () => {
    navigation.navigate("ReporteAsistencia", {
      cursoId: cursoId,
      cursoNombre: cursoNombre,
      materiaId: materiaId,
      materiaNombre: materiaNombre,
      fecha: fecha.toISOString(),
    })
  }

  const handleVolver = () => {
    navigation.goBack()
  }

  const formatDateWithDay = (date: Date): string => {
    return date.toLocaleDateString("es-ES", {
      weekday: "long",
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    })
  }

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando asistencia...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar asistencia</Text>
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
          <Text style={styles.emptyText}>No hay alumnos para esta fecha</Text>
        </View>
      )
    }

    return (
      <View style={styles.contentContainer}>
        <View style={styles.asistenciaHeader}>
          <Text style={styles.asistenciaTitle}>Asistencia para {formatDateWithDay(fecha)}</Text>
        </View>

        <ScrollView style={styles.scrollContainer} contentContainerStyle={styles.scrollContentContainer}>
          {alumnos.map((item, index) => (
            <View key={`alumno-${item.id_Alumno}-${index}`} style={styles.alumnoRow}>
              <View style={styles.alumnoInfo}>
                <Text style={styles.alumnoApellido}>{item.alumno?.apellido}</Text>
                <Text style={styles.alumnoNombre}>{item.alumno?.nombre}</Text>
              </View>
              <View style={styles.presenteContainer}>
                <Switch
                  value={item.presente}
                  onValueChange={() => togglePresente(index)}
                  disabled={userData?.id_perfil === 2 || userData?.id_perfil === 4}
                  trackColor={{ false: "#E0E0E0", true: "#C8E6C9" }}
                  thumbColor={item.presente ? "#4CAF50" : "#F5F5F5"}
                  ios_backgroundColor="#E0E0E0"
                />
                <Text style={[styles.presenteText, { color: item.presente ? "#4CAF50" : "#999" }]}>
                  {item.presente ? "Presente" : "Ausente"}
                </Text>
              </View>
            </View>
          ))}
          <View style={styles.spacer} />
        </ScrollView>
      </View>
    )
  }

  return (
    <AppLayout title="Asistencia" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Asistencia</Text>
          <Text style={styles.headerSubtitle}>
            {cursoNombre} - {materiaNombre}
          </Text>
        </View>

        {/* Selector de fecha */}
        <View style={styles.datePickerSection}>
          <Text style={styles.datePickerLabel}>Seleccionar fecha:</Text>
          <TouchableOpacity style={styles.datePickerButton} onPress={() => setShowDatePicker(true)}>
            <Icon name="calendar-today" size={20} color="#4285F4" style={styles.datePickerIcon} />
            <Text style={styles.datePickerText}>{formatDateWithDay(fecha)}</Text>
          </TouchableOpacity>
        </View>

        {/* Selector de fecha personalizado */}
        <CustomDatePicker
          date={fecha}
          onDateChange={handleDateChange}
          onClose={() => setShowDatePicker(false)}
          visible={showDatePicker}
        />

        {/* Contenido principal */}
        <View style={styles.mainContent}>{renderContent()}</View>

        {/* Botones de acción */}
        <View style={styles.buttonsContainer}>
          {/* Solo mostrar botón de guardar para administradores y docentes */}
          {userData?.id_perfil !== 2 && userData?.id_perfil !== 4 && (
            <TouchableOpacity
              style={[styles.saveButton, saving && styles.disabledButton]}
              onPress={guardarAsistencia}
              disabled={saving || loading}
            >
              {saving ? (
                <ActivityIndicator size="small" color="#FFFFFF" />
              ) : (
                <Icon name="save" size={20} color="#FFFFFF" />
              )}
              <Text style={styles.buttonText}>{saving ? "Guardando..." : "Guardar"}</Text>
            </TouchableOpacity>
          )}

          <TouchableOpacity style={styles.reportButton} onPress={generarReporte} disabled={saving || loading}>
            <Icon name="assessment" size={20} color="#FFFFFF" />
            <Text style={styles.buttonText}>Reporte</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.backButton} onPress={handleVolver} disabled={saving || loading}>
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
  datePickerSection: {
    backgroundColor: "#FFFFFF",
    margin: 16,
    padding: 16,
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  datePickerLabel: {
    fontSize: 16,
    fontWeight: "500",
    color: "#333",
    marginBottom: 12,
  },
  datePickerButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#F5F5F5",
    paddingHorizontal: 12,
    paddingVertical: 12,
    borderRadius: 6,
  },
  datePickerIcon: {
    marginRight: 8,
  },
  datePickerText: {
    fontSize: 16,
    color: "#333",
    textTransform: "capitalize",
  },
  mainContent: {
    flex: 1,
  },
  contentContainer: {
    flex: 1,
    marginHorizontal: 16,
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    marginBottom: 16,
  },
  asistenciaHeader: {
    padding: 16,
    borderBottomWidth: 1,
    borderBottomColor: "#E0E0E0",
  },
  asistenciaTitle: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    textAlign: "center",
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
  presenteContainer: {
    flexDirection: "row",
    alignItems: "center",
    minWidth: 120,
  },
  presenteText: {
    marginLeft: 8,
    fontSize: 14,
    fontWeight: "500",
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 16,
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  saveButton: {
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
  reportButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FF9800",
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderRadius: 8,
    flex: 1,
    marginHorizontal: 4,
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
  modalOverlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.5)",
    justifyContent: "center",
    alignItems: "center",
  },
  calendarModal: {
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
    padding: 20,
    width: "90%",
    maxWidth: 400,
    elevation: 5,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.25,
    shadowRadius: 3.84,
  },
  calendarHeader: {
    alignItems: "center",
    marginBottom: 16,
  },
  calendarTitle: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#333",
  },
  selectedDateText: {
    fontSize: 14,
    color: "#666",
    marginTop: 4,
    textTransform: "capitalize",
  },
  monthSelector: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 16,
    paddingHorizontal: 8,
  },
  monthButton: {
    padding: 8,
  },
  monthYearText: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    textTransform: "capitalize",
  },
  calendar: {
    marginBottom: 16,
  },
  weekRow: {
    flexDirection: "row",
    justifyContent: "space-around",
    marginBottom: 8,
  },
  dayLabelContainer: {
    width: 36,
    height: 36,
    justifyContent: "center",
    alignItems: "center",
  },
  dayLabel: {
    fontSize: 14,
    fontWeight: "bold",
    color: "#666",
  },
  dayContainer: {
    width: 36,
    height: 36,
    justifyContent: "center",
    alignItems: "center",
    borderRadius: 18,
  },
  dayText: {
    fontSize: 14,
    color: "#333",
  },
  selectedDayContainer: {
    backgroundColor: "#4285F4",
  },
  selectedDayText: {
    color: "#FFFFFF",
    fontWeight: "bold",
  },
  todayContainer: {
    borderWidth: 1,
    borderColor: "#4285F4",
  },
  todayText: {
    color: "#4285F4",
    fontWeight: "500",
  },
  calendarActions: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginTop: 10,
  },
  cancelButton: {
    backgroundColor: "#F0F0F0",
    paddingVertical: 10,
    paddingHorizontal: 20,
    borderRadius: 6,
    flex: 1,
    marginRight: 10,
    alignItems: "center",
  },
  cancelButtonText: {
    color: "#666",
    fontSize: 16,
    fontWeight: "500",
  },
  confirmButton: {
    backgroundColor: "#4285F4",
    paddingVertical: 10,
    paddingHorizontal: 20,
    borderRadius: 6,
    flex: 1,
    marginLeft: 10,
    alignItems: "center",
  },
  confirmButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "500",
  },
  spacer: {
    height: 80,
  },
})

export default AsistenciaScreen
