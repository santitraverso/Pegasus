import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, ScrollView, TextInput, TouchableOpacity, ActivityIndicator, Alert } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { DesempenioAlumnos } from "../models/desempenioAlumnos"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

type CreateDesempenioScreenRouteProp = RouteProp<RootStackParamList, "CreateDesempenio">
type CreateDesempenioScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "CreateDesempenio">

const CreateDesempenioScreen: React.FC = () => {
  const route = useRoute<CreateDesempenioScreenRouteProp>()
  const navigation = useNavigation<CreateDesempenioScreenNavigationProp>()
  const { userData } = useUser()
  const { desempenioId, alumnoId, alumnoNombre, alumnoApellido, cursoId, cursoNombre, esVer } = route.params

  const [asistencia, setAsistencia] = useState("")
  const [participacion, setParticipacion] = useState("")
  const [tareas, setTareas] = useState("")
  const [calificaciones, setCalificaciones] = useState("")
  const [promedio, setPromedio] = useState("")
  const [descripcionDesempenio, setDescripcionDesempenio] = useState("")
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const esNuevo = desempenioId === 0
  const titulo = esVer ? "Ver Desempeño" : esNuevo ? "Cargar Nuevo Desempeño" : "Editar Desempeño"

  useEffect(() => {
    if (!esNuevo) {
      fetchDesempenioData()
    } else {
      setLoading(false)
    }
  }, [desempenioId])

  useEffect(() => {
    // Calcular promedio automáticamente cuando cambian los valores
    calcularPromedio()
  }, [asistencia, participacion, tareas, calificaciones])

  const fetchDesempenioData = async () => {
    try {
      setLoading(true)
      setError(null)

      const desempenioData = await getDesempenioByIdAsync(desempenioId)
      setAsistencia(desempenioData.asistencia?.toString() || "")
      setParticipacion(desempenioData.participacion?.toString() || "")
      setTareas(desempenioData.tareas?.toString() || "")
      setCalificaciones(desempenioData.calificaciones?.toString() || "")
      setPromedio(desempenioData.promedio?.toString() || "")

      // Obtener descripción del desempeño si existe
      if (desempenioData.desempenio?.descripcion) {
        setDescripcionDesempenio(desempenioData.desempenio.descripcion)
      }
      
    } catch (error: any) {
      setError(error.message || "Error al cargar los datos")
    } finally {
      setLoading(false)
    }
  }

  const getDesempenioByIdAsync = async (id: number): Promise<DesempenioAlumnos> => {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/DesempenioAlumnos/GetById?id=${id}`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      })

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data: DesempenioAlumnos = await response.json()
      return data
    } catch (error) {
      throw error
    }
  }

  const calcularPromedio = () => {
    const valores = [asistencia, participacion, tareas, calificaciones]
      .map((val) => Number.parseFloat(val) || 0)
      .filter((val) => val > 0)

    if (valores.length === 4) {
      // Solo calcular si todos los campos tienen valores válidos
      const promedioCalculado = valores.reduce((sum, val) => sum + val, 0) / 4
      setPromedio((Math.round(promedioCalculado * 100) / 100).toString()) // Redondear a 2 decimales y convertir a string
    } else {
      setPromedio("")
    }
  }

  const validateForm = (): boolean => {
    const asistenciaNum = Number.parseFloat(asistencia)
    const participacionNum = Number.parseFloat(participacion)
    const tareasNum = Number.parseFloat(tareas)
    const calificacionesNum = Number.parseFloat(calificaciones)

    // Verificar que todos los campos tengan valores
    if (!asistencia || !participacion || !tareas || !calificaciones) {
      Alert.alert("Error", "Todos los campos son obligatorios")
      return false
    }

    // Verificar que todos los valores sean mayores a 0
    if (asistenciaNum <= 0 || participacionNum <= 0 || tareasNum <= 0 || calificacionesNum <= 0) {
      Alert.alert("Error", "Todos los campos deben tener un valor mayor a 0")
      return false
    }

    // Verificar que todos los valores sean menores o iguales a 10
    if (asistenciaNum > 10 || participacionNum > 10 || tareasNum > 10 || calificacionesNum > 10) {
      Alert.alert("Error", "Los campos no pueden tener un valor mayor a 10")
      return false
    }

    return true
  }

  const handleGuardar = async () => {
    if (!validateForm()) {
      return
    }

    try {
      setSaving(true)
      setError(null)

      const promedioCalculado =
        Math.round(
          ((Number.parseFloat(asistencia) +
            Number.parseFloat(participacion) +
            Number.parseFloat(tareas) +
            Number.parseFloat(calificaciones)) /
            4) *
            100,
        ) / 100

      const desempenioData = {
        Id_Alumno: alumnoId,
        Id_Curso: cursoId,
        Asistencia: Number.parseFloat(asistencia),
        Participacion: Number.parseFloat(participacion),
        Tareas: Number.parseFloat(tareas),
        Calificaciones: Number.parseFloat(calificaciones),
        Promedio: promedioCalculado,
        ...(desempenioId > 0 && { Id: desempenioId }),
      }

      const response = await fetch(
        esNuevo
          ? `${CONFIG.API_BASE_URL}/DesempenioAlumnos/CreateDesempenioAlumnos`
          : `${CONFIG.API_BASE_URL}/DesempenioAlumnos/UpdateDesempenioAlumnos`,
        {
          method: esNuevo ? "POST" : "PUT",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(desempenioData),
        },
      )
      if (!response.ok) {
        const errorResponse = await response.text()
        throw new Error(
          esNuevo
            ? `Error al crear el desempeño: ${errorResponse}`
            : `Error al actualizar el desempeño: ${errorResponse}`,
        )
      }

      Alert.alert("Éxito", "El desempeño se guardó correctamente", [
        {
          text: "OK",
          onPress: () => {
            navigation.navigate("Desempenio", {
              cursoId: cursoId,
              cursoNombre: cursoNombre,
            })
          },
        },
      ])
    } catch (error: any) {
      setError(error.message || "Error al guardar el desempeño")
      Alert.alert("Error", error.message || "Error al guardar el desempeño")
    } finally {
      setSaving(false)
    }
  }

  const handleEliminar = async () => {
    Alert.alert(
      "¿Estás seguro?",
      "¿Deseas eliminar este desempeño?",
      [
        {
          text: "Cancelar",
          style: "cancel",
        },
        {
          text: "Sí, eliminar",
          style: "destructive",
          onPress: async () => {
            try {
              setSaving(true)

              const response = await fetch(
                `${CONFIG.API_BASE_URL}/DesempenioAlumnos/DeleteDesempenioAlumnos/${desempenioId}`,
                {
                  method: "DELETE",
                },
              )

              if (!response.ok) {
                throw new Error("Error al eliminar el desempeño")
              }              

              Alert.alert("Éxito", "El desempeño se eliminó correctamente", [
                {
                  text: "OK",
                  onPress: () => {
                    navigation.navigate("Desempenio", {
                      cursoId: cursoId,
                      cursoNombre: cursoNombre,
                    })
                  },
                },
              ])
            } catch (error: any) {
              Alert.alert("Error", "No se pudo eliminar el desempeño")
            } finally {
              setSaving(false)
            }
          },
        },
      ],
      { cancelable: true },
    )
  }

  const handleVolver = () => {
    navigation.navigate("Desempenio", {
      cursoId: cursoId,
      cursoNombre: cursoNombre,
    })
  }

  if (loading) {
    return (
      <AppLayout title={titulo} showHomeButton={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando datos...</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title={titulo} showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>{titulo}</Text>
          <Text style={styles.headerSubtitle}>{cursoNombre}</Text>
        </View>

        <ScrollView style={styles.scrollContainer} contentContainerStyle={styles.scrollContent}>
          <View style={styles.formContainer}>
            {/* Información del alumno */}
            <View style={styles.studentCard}>
              <View style={styles.studentHeader}>
                <Icon name="person" size={24} color="#4285F4" />
                <Text style={styles.studentName}>
                  {alumnoNombre} {alumnoApellido}
                </Text>
              </View>
            </View>

            {/* Formulario de desempeño */}
            <View style={styles.formGroup}>
              <Text style={styles.label}>Asistencia</Text>
              <TextInput
                style={[styles.input, esVer && styles.readOnlyInput]}
                value={asistencia}
                onChangeText={setAsistencia}
                keyboardType="numeric"
                placeholder="1-10"
                editable={!esVer}
                maxLength={5}
              />
            </View>

            <View style={styles.formGroup}>
              <Text style={styles.label}>Participación</Text>
              <TextInput
                style={[styles.input, esVer && styles.readOnlyInput]}
                value={participacion}
                onChangeText={setParticipacion}
                keyboardType="numeric"
                placeholder="1-10"
                editable={!esVer}
                maxLength={5}
              />
            </View>

            <View style={styles.formGroup}>
              <Text style={styles.label}>Calificaciones</Text>
              <TextInput
                style={[styles.input, esVer && styles.readOnlyInput]}
                value={calificaciones}
                onChangeText={setCalificaciones}
                keyboardType="numeric"
                placeholder="1-10"
                editable={!esVer}
                maxLength={5}
              />
            </View>

            <View style={styles.formGroup}>
              <Text style={styles.label}>Tareas</Text>
              <TextInput
                style={[styles.input, esVer && styles.readOnlyInput]}
                value={tareas}
                onChangeText={setTareas}
                keyboardType="numeric"
                placeholder="1-10"
                editable={!esVer}
                maxLength={5}
              />
            </View>

            {promedio && (
              <View style={styles.promedioContainer}>
                <Text style={styles.promedioLabel}>Promedio:</Text>
                <Text style={styles.promedioValor}>{promedio}</Text>
              </View>
            )}

            {/* Mostrar descripción solo en modo ver */}
            {esVer && descripcionDesempenio && (
              <View style={styles.formGroup}>
                <Text style={styles.label}>Descripción</Text>
                <View style={styles.descripcionContainer}>
                  <Text style={styles.descripcionText}>{descripcionDesempenio}</Text>
                </View>
              </View>
            )}

            {error && (
              <View style={styles.errorContainer}>
                <Text style={styles.errorText}>{error}</Text>
              </View>
            )}
          </View>
        </ScrollView>

        {/* Botones */}
        <View style={styles.buttonsContainer}>
          {/* Solo mostrar botones de acción para administradores y docentes */}
          {userData && userData.id_perfil !== 2 && userData.id_perfil !== 4 && (
            <TouchableOpacity
              style={[esVer ? styles.deleteButton : styles.saveButton, saving && styles.disabledButton]}
              onPress={esVer ? handleEliminar : handleGuardar}
              disabled={saving}
            >
              {saving ? (
                <ActivityIndicator size="small" color="#FFFFFF" />
              ) : (
                <Icon name={esVer ? "delete" : "save"} size={20} color="#FFFFFF" />
              )}
              <Text style={styles.buttonText}>
                {saving ? (esVer ? "Eliminando..." : "Guardando...") : esVer ? "Eliminar" : "Guardar"}
              </Text>
            </TouchableOpacity>
          )}

          <TouchableOpacity style={styles.backButton} onPress={handleVolver} disabled={saving}>
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
  scrollContainer: {
    flex: 1,
  },
  scrollContent: {
    paddingBottom: 20,
  },
  formContainer: {
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
  studentCard: {
    marginBottom: 20,
    paddingBottom: 16,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  studentHeader: {
    flexDirection: "row",
    alignItems: "center",
  },
  studentName: {
    marginLeft: 12,
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
  },
  formGroup: {
    marginBottom: 16,
    paddingTop: 16,
  },
  label: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    marginBottom: 8,
    textAlign: "center",
  },
  input: {
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 6,
    padding: 12,
    fontSize: 16,
    color: "#333",
    backgroundColor: "#FFFFFF",
    textAlign: "center",
  },
  textArea: {
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 6,
    padding: 12,
    fontSize: 16,
    color: "#333",
    backgroundColor: "#FFFFFF",
    minHeight: 80,
  },
  readOnlyInput: {
    backgroundColor: "#F8F9FA",
    color: "#666",
  },
  promedioContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    backgroundColor: "#E3F2FD",
    padding: 16,
    borderRadius: 8,
    marginTop: 16,
    borderLeftWidth: 4,
    borderLeftColor: "#4285F4",
  },
  promedioLabel: {
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
  },
  promedioValor: {
    fontSize: 20,
    fontWeight: "bold",
    color: "#4285F4",
  },
  descripcionContainer: {
    backgroundColor: "#F8F9FA",
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 6,
    padding: 16,
    minHeight: 80,
  },
  descripcionText: {
    fontSize: 16,
    color: "#333",
    lineHeight: 22,
  },
  errorContainer: {
    backgroundColor: "#FFEBEE",
    padding: 12,
    borderRadius: 6,
    marginTop: 16,
  },
  errorText: {
    color: "#F44336",
    fontSize: 14,
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
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
    flex: 1,
    marginRight: 8,
    justifyContent: "center",
  },
  deleteButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#F44336",
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
    backgroundColor: "#6C7B7F",
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
})

export default CreateDesempenioScreen
