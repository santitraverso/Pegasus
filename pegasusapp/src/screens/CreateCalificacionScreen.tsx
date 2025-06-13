import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Alert, ScrollView, ActivityIndicator } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { Calificaciones } from "../models/calificaciones"
import { CONFIG } from "../services/config"

// Extender el tipo de navegación para incluir la nueva ruta
type ExtendedRootStackParamList = RootStackParamList & {
  CreateCalificacion: {
    alumnoId: number
    alumnoNombre: string
    alumnoApellido: string
    cursoId: number
    cursoNombre: string
    materiaId: number
    materiaNombre: string
    esNuevo: boolean
  }
}

type CreateCalificacionScreenRouteProp = RouteProp<ExtendedRootStackParamList, "CreateCalificacion">
type CreateCalificacionScreenNavigationProp = NativeStackNavigationProp<
  ExtendedRootStackParamList,
  "CreateCalificacion"
>

interface CalificacionForm {
  id: number
  calificacion: string
  esNueva: boolean
}

const CreateCalificacionScreen: React.FC = () => {
  const route = useRoute<CreateCalificacionScreenRouteProp>()
  const navigation = useNavigation<CreateCalificacionScreenNavigationProp>()

  const { alumnoId, alumnoNombre, alumnoApellido, cursoId, cursoNombre, materiaId, materiaNombre, esNuevo } =
    route.params

  const [calificaciones, setCalificaciones] = useState<CalificacionForm[]>([])
  const [calificacionesEliminadas, setCalificacionesEliminadas] = useState<number[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const esEdicion = !esNuevo

  useEffect(() => {
    fetchCalificaciones()
  }, [])

  const fetchCalificaciones = async () => {
    try {
      setLoading(true)
      setError(null)

      if (esNuevo) {
        // Para nueva calificación, empezar con una fila vacía
        setCalificaciones([{ id: 0, calificacion: "", esNueva: true }])
      } else {
        // Para edición, cargar calificaciones existentes
        const calificacionesExistentes = await getCalificacionesAsync(materiaId, cursoId, alumnoId)
        const calificacionesForm = calificacionesExistentes.map((cal) => ({
          id: cal.id,
          calificacion: cal.calificacion.toString(),
          esNueva: false,
        }))
        setCalificaciones(
          calificacionesForm.length > 0 ? calificacionesForm : [{ id: 0, calificacion: "", esNueva: true }],
        )
      }
      
    } catch (error: any) {
      setError(error.message || "Error al cargar las calificaciones")
    } finally {
      setLoading(false)
    }
  }

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

  const agregarCalificacion = () => {
    const nuevaCalificacion: CalificacionForm = {
      id: 0,
      calificacion: "",
      esNueva: true,
    }
    setCalificaciones([...calificaciones, nuevaCalificacion])
  }

  const eliminarCalificacion = (index: number) => {
    const calificacion = calificaciones[index]

    Alert.alert("¿Estás seguro?", "¿Deseas eliminar esta calificación? Recuerda Guardar para confirmar", [
      {
        text: "Cancelar",
        style: "cancel",
      },
      {
        text: "Sí, eliminar",
        style: "destructive",
        onPress: () => {
          // Si la calificación tiene ID (no es nueva), agregarla a la lista de eliminadas
          if (calificacion.id > 0) {
            setCalificacionesEliminadas([...calificacionesEliminadas, calificacion.id])
          }

          // Remover de la lista actual
          const nuevasCalificaciones = calificaciones.filter((_, i) => i !== index)
          setCalificaciones(nuevasCalificaciones)
        },
      },
    ])
  }

  const actualizarCalificacion = (index: number, valor: string) => {
    const nuevasCalificaciones = [...calificaciones]
    nuevasCalificaciones[index].calificacion = valor
    setCalificaciones(nuevasCalificaciones)
  }

  const validarCalificaciones = (): boolean => {
    for (const cal of calificaciones) {
      const valor = Number.parseFloat(cal.calificacion)
      if (isNaN(valor) || valor < 1 || valor > 10) {
        Alert.alert("Error de validación", "Todas las calificaciones deben estar entre 1 y 10")
        return false
      }
    }
    return true
  }

  const guardarCalificaciones = async () => {
  if (!validarCalificaciones()) {
    return
  }

  try {
    setSaving(true)

    //Eliminar las calificaciones marcadas para eliminación
    if (calificacionesEliminadas.length > 0) {
      const calificacionesEliminar = calificacionesEliminadas.map(id => ({ Id: id }))
      
      const deleteResponse = await fetch(`${CONFIG.API_BASE_URL}/Calificaciones/DeleteAllCalificaciones`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(calificacionesEliminar),
      })

      if (!deleteResponse.ok) {
        const errorResponse = await deleteResponse.text()
        throw new Error(`Error al eliminar calificaciones: ${errorResponse}`)
      }
    }

    const calificacionesNuevas = calificaciones.filter(cal => cal.id <= 0)
    const calificacionesExistentes = calificaciones.filter(cal => cal.id > 0)

    const nuevasData = calificacionesNuevas.map(cal => ({
      Calificacion: Number.parseFloat(cal.calificacion),
      Id_Materia: materiaId,
      Id_Curso: cursoId,
      Id_Alumno: alumnoId
    }))

    const existentesData = calificacionesExistentes.map(cal => ({
      Id: cal.id,
      Calificacion: Number.parseFloat(cal.calificacion),
      Id_Materia: materiaId,
      Id_Curso: cursoId,
      Id_Alumno: alumnoId
    }))

    //rear nuevas calificaciones
    if (nuevasData.length > 0) {
      const createResponse = await fetch(`${CONFIG.API_BASE_URL}/Calificaciones/CreateAllCalificaciones`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(nuevasData),
      })

      if (!createResponse.ok) {
        const errorResponse = await createResponse.text()
        throw new Error(`Error al crear calificaciones: ${errorResponse}`)
      }
    }

    //Actualizar calificaciones existentes
    if (existentesData.length > 0) {
      const updateResponse = await fetch(`${CONFIG.API_BASE_URL}/Calificaciones/UpdateAllCalificaciones`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(existentesData),
      })

      if (!updateResponse.ok) {
        const errorResponse = await updateResponse.text()
        throw new Error(`Error al actualizar calificaciones: ${errorResponse}`)
      }
    }

    Alert.alert("Éxito", "Las calificaciones se guardaron correctamente.", [
      {
        text: "OK",
        onPress: () => {
          navigation.goBack()
        },
      },
    ])
  } catch (error: any) {
    Alert.alert("Error", error.message || "Error al guardar las calificaciones")
  } finally {
    setSaving(false)
  }
}


  const handleVolver = () => {
    navigation.goBack()
  }

  if (loading) {
    return (
      <AppLayout title="Cargando..." showHomeButton={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando calificaciones...</Text>
        </View>
      </AppLayout>
    )
  }

  if (error) {
    return (
      <AppLayout title="Error" showHomeButton={true}>
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar datos</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchCalificaciones}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title="Gestionar Calificaciones" showHomeButton={true}>
      <View style={styles.container}>
        <ScrollView style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Calificaciones</Text>
          <Text style={styles.headerSubtitle}>
            {cursoNombre} - {materiaNombre}
          </Text>
        </View>

        <View style={styles.studentCard}>
          <View style={styles.studentHeader}>
            <Icon name="person" size={24} color="#4285F4" />
            <Text style={styles.studentName}>
              {alumnoApellido}, {alumnoNombre}
            </Text>
          </View>

          <View style={styles.gradesSection}>
            <View style={styles.gradesSectionHeader}>
              <Text style={styles.gradesSectionTitle}>Calificaciones</Text>
              <TouchableOpacity style={styles.addButton} onPress={agregarCalificacion}>
                <Icon name="add" size={20} color="#4CAF50" />
                <Text style={styles.addButtonText}>Agregar</Text>
              </TouchableOpacity>
            </View>

            {calificaciones.map((calificacion, index) => (
              <View key={index} style={styles.gradeRow}>
                <Text style={styles.gradeLabel}>Nota {index + 1}</Text>
                <View style={styles.gradeInputContainer}>
                  <TextInput
                    style={styles.gradeInput}
                    value={calificacion.calificacion}
                    onChangeText={(text) => actualizarCalificacion(index, text)}
                    keyboardType="numeric"
                    placeholder="1-10"
                    maxLength={4}
                  />
                  {/* Mostrar botón eliminar siempre que haya más de una calificación O si es la única pero no está vacía */}
                  {(calificaciones.length > 1 ||
                    (calificaciones.length === 1 && calificacion.calificacion.trim() !== "")) && (
                    <TouchableOpacity style={styles.deleteButton} onPress={() => eliminarCalificacion(index)}>
                      <Icon name="delete-outline" size={20} color="#F44336" />
                    </TouchableOpacity>
                  )}
                </View>
              </View>
            ))}
          </View>
        </View>
      </ScrollView>
      <View style={styles.footerContainer}>
          <View style={styles.buttonsContainer}>
            <TouchableOpacity
              style={[styles.saveButton, saving && styles.disabledButton]}
              onPress={guardarCalificaciones}
              disabled={saving}
            >
              {saving ? (
                <ActivityIndicator size="small" color="#FFFFFF" />
              ) : (
                <Icon name="save" size={20} color="#FFFFFF" />
              )}
              <Text style={styles.buttonText}>{saving ? "Guardando..." : "Guardar"}</Text>
            </TouchableOpacity>    
            <TouchableOpacity style={styles.backButton} onPress={handleVolver} disabled={saving}>
              <Icon name="arrow-back" size={20} color="#FFFFFF" />
              <Text style={styles.buttonText}>Atrás</Text>
            </TouchableOpacity>
          </View>
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
  studentCard: {
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
  studentHeader: {
    flexDirection: "row",
    alignItems: "center",
    paddingBottom: 16,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  studentName: {
    marginLeft: 12,
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
  },
  gradesSection: {
    marginTop: 16,
  },
  gradesSectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 16,
  },
  gradesSectionTitle: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
  },
  addButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#E8F5E8",
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 6,
  },
  addButtonText: {
    marginLeft: 4,
    fontSize: 14,
    color: "#4CAF50",
    fontWeight: "500",
  },
  gradeRow: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  gradeLabel: {
    fontSize: 16,
    color: "#666",
    fontWeight: "500",
    flex: 1,
  },
  gradeInputContainer: {
    flexDirection: "row",
    alignItems: "center",
    flex: 2,
  },
  gradeInput: {
    flex: 1,
    borderBottomWidth: 2,
    borderBottomColor: "#E0E0E0",
    paddingVertical: 8,
    paddingHorizontal: 4,
    fontSize: 16,
    color: "#333",
    textAlign: "center",
  },
  deleteButton: {
    marginLeft: 12,
    padding: 8,
    borderRadius: 20,
    backgroundColor: "#FFEBEE",
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 16,
    backgroundColor: "#FFFFFF",
    marginTop: 16,
  },
  saveButton: {
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
    paddingHorizontal: 24,
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
  footerContainer: {
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
    paddingBottom: 20,
  },
})

export default CreateCalificacionScreen
