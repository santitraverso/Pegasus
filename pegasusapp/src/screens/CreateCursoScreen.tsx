import React from "react"
import { useState, useEffect } from "react"
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  ScrollView,
  FlatList,
} from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation, useFocusEffect } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { Curso } from "../models/curso"
import type { IntegrantesCursos } from "../models/integrantesCursos"
import type { CursoMateria } from "../models/cursoMateria"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

type CreateCursoScreenRouteProp = RouteProp<RootStackParamList, "CreateCurso">
type CreateCursoScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "CreateCurso">

const CreateCursoScreen: React.FC = () => {
  const route = useRoute<CreateCursoScreenRouteProp>()
  const navigation = useNavigation<CreateCursoScreenNavigationProp>()
  const { userData } = useUser()
  const { cursoId } = route.params

  const [curso, setCurso] = useState<Curso>({
    id: 0,
    nombre_Curso: "",
    grado: null,
    division: "",
    turno: "",
  })
  const [alumnos, setAlumnos] = useState<IntegrantesCursos[]>([])
  const [materias, setMaterias] = useState<CursoMateria[]>([])
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const esEdicion = cursoId && cursoId > 0
  const esVisualizacion = userData?.id_perfil === 3
  const titulo = esVisualizacion ? "Datos del Curso" : esEdicion ? "Editar Curso" : "Crear Nuevo Curso"

  // Recargar datos cuando la pantalla recibe foco
  useFocusEffect(
    React.useCallback(() => {
      if (esEdicion) {
        fetchCursoData()
      }
    }, [cursoId]),
  )

  useEffect(() => {
    if (esEdicion) {
      fetchCursoData()
    }
  }, [cursoId])

  const fetchCursoData = async () => {
    if (!cursoId) return

    try {
      setLoading(true)
      setError(null)

      // Cargar datos del curso
      const cursoData = await getCursoAsync(cursoId)
      setCurso(cursoData)

      // Cargar alumnos del curso
      const alumnosData = await getIntegrantesCursosAsync(cursoId)
      setAlumnos(alumnosData)

      // Cargar materias del curso
      const materiasData = await getMateriasCursoAsync(cursoId)
      setMaterias(materiasData)
    } catch (error: any) {
      console.error("Error al cargar datos del curso:", error)
      setError(error.message || "Error al cargar los datos del curso")
    } finally {
      setLoading(false)
    }
  }

  const getCursoAsync = async (id: number): Promise<Curso> => {

    const response = await fetch(`${CONFIG.API_BASE_URL}/Curso/GetById?id=${id}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    })

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`)
    }

    return await response.json()
  }

  const getIntegrantesCursosAsync = async (cursoId: number): Promise<IntegrantesCursos[]> => {

    const queryParam = encodeURIComponent(`x=>x.id_curso==${cursoId}`)
    const response = await fetch(
      `${CONFIG.API_BASE_URL}/IntegrantesCursos/GetIntegrantesCursosForCombo?query=${queryParam}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      },
    )

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`)
    }

    return await response.json()
  }

  const getMateriasCursoAsync = async (cursoId: number): Promise<CursoMateria[]> => {

    const queryParam = encodeURIComponent(`x=>x.id_curso==${cursoId}`)
    const response = await fetch(`${CONFIG.API_BASE_URL}/CursoMateria/GetCursoMateriaForCombo?query=${queryParam}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    })

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`)
    }

    return await response.json()
  }

  const handleInputChange = (field: keyof Curso, value: string | number) => {
    setCurso((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const validateForm = (): boolean => {
    if (!curso.nombre_Curso?.trim()) {
      Alert.alert("Error", "El nombre del curso es requerido")
      return false
    }

    if (!curso.grado || curso.grado < 1) {
      Alert.alert("Error", "El grado debe ser mayor a 0")
      return false
    }

    if (!curso.division?.trim()) {
      Alert.alert("Error", "La división es requerida")
      return false
    }

    if (!curso.turno?.trim()) {
      Alert.alert("Error", "El turno es requerido")
      return false
    }

    return true
  }

  const handleGuardar = async () => {
    if (!validateForm()) return

    try {
      setSaving(true)
      setError(null)

      const cursoData = {
        ...(esEdicion && { Id: curso.id }),
        Nombre_Curso: curso.nombre_Curso,
        Grado: curso.grado,
        Division: curso.division,
        Turno: curso.turno,
      }
      const response = await fetch(`${CONFIG.API_BASE_URL}/Curso/${esEdicion ? "UpdateCurso" : "CreateCurso"}`, {
        method: esEdicion ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(cursoData),
      })
      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`Error al ${esEdicion ? "actualizar" : "crear"} el curso: ${errorText}`)
      }
      

      Alert.alert("Éxito", `El curso se ${esEdicion ? "actualizó" : "creó"} correctamente`, [
        {
          text: "OK",
          onPress: () => navigation.navigate("Cursos"),
        },
      ])
    } catch (error: any) {
      setError(error.message || `Error al ${esEdicion ? "actualizar" : "crear"} el curso`)
      Alert.alert("Error", error.message || `Error al ${esEdicion ? "actualizar" : "crear"} el curso`)
    } finally {
      setSaving(false)
    }
  }

  const handleVolver = () => {
    navigation.navigate("Cursos")
  }

  const handleAgregarAlumnos = () => {
    if (curso.id) {
      navigation.navigate("IntegrantesCursos", { cursoId: curso.id })
    }
  }

  const handleAgregarMaterias = () => {
    if (curso.id) {
      navigation.navigate("MateriasCurso", { cursoId: curso.id })
    }
  }

  const renderAlumnoItem = ({ item }: { item: IntegrantesCursos }) => (
    <View style={styles.listItem}>
      <Icon name="person" size={16} color="#4285F4" style={styles.listItemIcon} />
      <Text style={styles.listItemText}>
        {item.usuario?.apellido}, {item.usuario?.nombre}
      </Text>
    </View>
  )

  const renderMateriaItem = ({ item }: { item: CursoMateria }) => (
    <View style={styles.listItem}>
      <Icon name="book" size={16} color="#4285F4" style={styles.listItemIcon} />
      <Text style={styles.listItemText}>{item.materia?.nombre}</Text>
    </View>
  )

  if (loading) {
    return (
      <AppLayout title={titulo} showHomeButton={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando datos del curso...</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title={titulo} showHomeButton={true}>
      <View style={styles.container}>
        <ScrollView style={styles.scrollContainer} showsVerticalScrollIndicator={false}>
            <View style={styles.header}>
                <Text style={styles.headerTitle}>{titulo}</Text>
            </View>
          {error && (
            <View style={styles.errorContainer}>
              <Icon name="error-outline" size={24} color="#F44336" />
              <Text style={styles.errorText}>{error}</Text>
            </View>
          )}

          {/* Formulario del curso */}
          <View style={styles.formContainer}>
            <Text style={styles.sectionTitle}>Información del Curso</Text>

            <View style={styles.inputContainer}>
              <Text style={styles.inputLabel}>Nombre del curso</Text>
              <TextInput
                style={[styles.textInput, esVisualizacion && styles.disabledInput]}
                value={curso.nombre_Curso || ""}
                onChangeText={(text) => handleInputChange("nombre_Curso", text)}
                placeholder="Ingrese el nombre del curso..."
                editable={!esVisualizacion}
              />
            </View>

            <View style={styles.inputContainer}>
              <Text style={styles.inputLabel}>Grado</Text>
              <TextInput
                style={[styles.textInput, esVisualizacion && styles.disabledInput]}
                value={curso.grado?.toString() || ""}
                onChangeText={(text) => handleInputChange("grado", Number.parseInt(text) || 1)}
                placeholder="1"
                keyboardType="numeric"
                editable={!esVisualizacion}
              />
            </View>

            <View style={styles.inputContainer}>
              <Text style={styles.inputLabel}>División</Text>
              <TextInput
                style={[styles.textInput, esVisualizacion && styles.disabledInput]}
                value={curso.division || ""}
                onChangeText={(text) => handleInputChange("division", text.toUpperCase())}
                placeholder="A"
                maxLength={1}
                editable={!esVisualizacion}
              />
            </View>

            <View style={styles.inputContainer}>
              <Text style={styles.inputLabel}>Turno</Text>
              <TextInput
                style={[styles.textInput, esVisualizacion && styles.disabledInput]}
                value={curso.turno || ""}
                onChangeText={(text) => handleInputChange("turno", text)}
                placeholder="Mañana, Tarde, Noche..."
                editable={!esVisualizacion}
              />
            </View>
          </View>

          {/* Secciones de alumnos y materias (solo si el curso ya existe) */}
          {esEdicion && (
            <View style={styles.sectionsContainer}>
              {/* Sección de Alumnos */}
              <View style={styles.sectionContainer}>
                <View style={styles.sectionHeader}>
                  <Text style={styles.sectionTitle}>Alumnos</Text>
                  {!esVisualizacion && (
                    <TouchableOpacity style={styles.sectionButton} onPress={handleAgregarAlumnos}>
                      <Icon name="person-add" size={16} color="#FFFFFF" />
                      <Text style={styles.sectionButtonText}>{alumnos.length > 0 ? "Modificar" : "Agregar"}</Text>
                    </TouchableOpacity>
                  )}
                </View>

                {alumnos.length > 0 ? (
                  <View style={styles.listWrapper}>
                    <FlatList
                      data={alumnos}
                      renderItem={renderAlumnoItem}
                      keyExtractor={(item) => item.id?.toString() || ""}
                      scrollEnabled={false}
                      nestedScrollEnabled={true}
                    />
                  </View>
                ) : (
                  <View style={styles.emptySection}>
                    <Icon name="person" size={32} color="#CCCCCC" />
                    <Text style={styles.emptySectionText}>No hay alumnos asignados</Text>
                  </View>
                )}
              </View>

              {/* Sección de Materias (solo para administradores) */}
              {!esVisualizacion && (
                <View style={styles.sectionContainer}>
                  <View style={styles.sectionHeader}>
                    <Text style={styles.sectionTitle}>Materias</Text>
                    <TouchableOpacity style={styles.sectionButton} onPress={handleAgregarMaterias}>
                      <Icon name="book" size={16} color="#FFFFFF" />
                      <Text style={styles.sectionButtonText}>{materias.length > 0 ? "Modificar" : "Agregar"}</Text>
                    </TouchableOpacity>
                  </View>

                  {materias.length > 0 ? (
                    <View style={styles.listWrapper}>
                      <FlatList
                        data={materias}
                        renderItem={renderMateriaItem}
                        keyExtractor={(item) => item.id?.toString() || ""}
                        scrollEnabled={false}
                        nestedScrollEnabled={true}
                      />
                    </View>
                  ) : (
                    <View style={styles.emptySection}>
                      <Icon name="book" size={32} color="#CCCCCC" />
                      <Text style={styles.emptySectionText}>No hay materias asignadas</Text>
                    </View>
                  )}
                </View>
              )}
            </View>
          )}
        </ScrollView>

        {/* Botones de acción */}
        <View style={styles.footerContainer}>
          <View style={styles.buttonsContainer}>
            {!esVisualizacion && (
              <TouchableOpacity
                style={[styles.saveButton, saving && styles.disabledButton]}
                onPress={handleGuardar}
                disabled={saving}
              >
                {saving ? (
                  <ActivityIndicator size="small" color="#FFFFFF" />
                ) : (
                  <Icon name="save" size={20} color="#FFFFFF" />
                )}
                <Text style={styles.buttonText}>{saving ? "Guardando..." : "Guardar"}</Text>
              </TouchableOpacity>
            )}

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
  scrollContainer: {
    flex: 1,
  },
  footerContainer: {
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
    paddingBottom: 20,
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
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FFEBEE",
    padding: 12,
    margin: 16,
    borderRadius: 8,
    borderLeftWidth: 4,
    borderLeftColor: "#F44336",
  },
  errorText: {
    color: "#F44336",
    fontSize: 14,
    marginLeft: 8,
    flex: 1,
  },
  formContainer: {
    backgroundColor: "#FFFFFF",
    margin: 16,
    padding: 16,
    borderRadius: 12,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#333",
    marginBottom: 16,
  },
  inputContainer: {
    marginBottom: 16,
  },
  inputLabel: {
    fontSize: 14,
    fontWeight: "600",
    color: "#333",
    marginBottom: 8,
  },
  textInput: {
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 12,
    fontSize: 16,
    color: "#333",
    backgroundColor: "#FFFFFF",
  },
  disabledInput: {
    backgroundColor: "#F5F5F5",
    color: "#666",
  },
  sectionsContainer: {
    marginHorizontal: 16,
    marginBottom: 16,
  },
  sectionContainer: {
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  sectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 12,
  },
  sectionButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#4285F4",
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 6,
  },
  sectionButtonText: {
    color: "#FFFFFF",
    fontSize: 12,
    fontWeight: "600",
    marginLeft: 4,
  },
  listWrapper: {
  },
  listItem: {
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 8,
    paddingHorizontal: 12,
    backgroundColor: "#F8F9FA",
    borderRadius: 6,
    marginBottom: 4,
  },
  listItemIcon: {
    marginRight: 8,
  },
  listItemText: {
    fontSize: 14,
    color: "#333",
    flex: 1,
  },
  emptySection: {
    alignItems: "center",
    paddingVertical: 24,
  },
  emptySectionText: {
    fontSize: 14,
    color: "#666",
    marginTop: 8,
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 16,
    backgroundColor: "#FFFFFF",
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
})

export default CreateCursoScreen
