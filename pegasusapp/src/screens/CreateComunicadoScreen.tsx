import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, TextInput, TouchableOpacity, ActivityIndicator, Alert, ScrollView } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { CuadernoComunicados } from "../models/cuadernoComunicados"
import type { ComunicadoAlumnos } from "../models/comunicadoAlumnos"
import type { Usuario } from "../models/usuario"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

// Extender el tipo de navegación para incluir la nueva ruta
type ExtendedRootStackParamList = RootStackParamList & {
  CreateComunicado: {
    cursoId: number
    cursoNombre: string
    alumnosIds: number[]
    comunicadoId?: number
  }
  Cuaderno: {
    cursoId: number
    cursoNombre: string
  }
}

type CreateComunicadoScreenRouteProp = RouteProp<ExtendedRootStackParamList, "CreateComunicado">
type CreateComunicadoScreenNavigationProp = NativeStackNavigationProp<ExtendedRootStackParamList, "CreateComunicado">

const CreateComunicadoScreen: React.FC = () => {
  const route = useRoute<CreateComunicadoScreenRouteProp>()
  const navigation = useNavigation<CreateComunicadoScreenNavigationProp>()
  const { userData } = useUser()
  const { cursoId, cursoNombre, alumnosIds, comunicadoId } = route.params

  const [descripcion, setDescripcion] = useState("")
  const [nombresConcatenados, setNombresConcatenados] = useState("")
  const [saving, setSaving] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const esEdicion = comunicadoId && comunicadoId > 0
  const titulo = esEdicion ? "Editar Comunicado" : "Cargar Nuevo Comunicado"

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setLoading(true)
      setError(null)

      if (esEdicion && comunicadoId) {
        // Cargar datos del comunicado existente
        
        const comunicado = await getComunicadoCursoAsync(comunicadoId)
        const comunicadoAlumnos = await getAlumnosComunicadoAsync(comunicadoId)
        setDescripcion(comunicado.descripcion || "")
        setNombresConcatenados(comunicadoAlumnos.map((a) => `${a.alumno?.apellido} ${a.alumno?.nombre}`).join(", "))
        
      } else {
        // Nuevo comunicado - cargar nombres de alumnos seleccionados

        let nombres = ""
        for (const alumnoId of alumnosIds) {
          const usuario = await getUsuarioAsync(alumnoId)
          nombres += `${usuario.apellido} ${usuario.nombre}, `
        }
        // Remover la última coma y espacio
        if (nombres.endsWith(", ")) {
          nombres = nombres.substring(0, nombres.length - 2)
        }
        setNombresConcatenados(nombres)
        
      }
    } catch (error: any) {
      setError(error.message || "Error al cargar los datos")
    } finally {
      setLoading(false)
    }
  }

  // Función para obtener comunicado por ID
  const getComunicadoCursoAsync = async (comunicadoId: number): Promise<CuadernoComunicados> => {
    const response = await fetch(`${CONFIG.API_BASE_URL}/CuadernoComunicados/GetById?id=${comunicadoId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    })

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`)
    }

    const data: CuadernoComunicados = await response.json()
    return data
  }

  // Función para obtener alumnos de un comunicado
  const getAlumnosComunicadoAsync = async (comunicadoId: number): Promise<ComunicadoAlumnos[]> => {
    const queryParam = encodeURIComponent(`x=>x.id_comunicado == ${comunicadoId}`)
    const response = await fetch(
      `${CONFIG.API_BASE_URL}/ComunicadoAlumnos/GetComunicadoAlumnossForCombo?query=${queryParam}`,
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

    const data: ComunicadoAlumnos[] = await response.json()
    return data || []
  }

  // Función para obtener usuario por ID
  const getUsuarioAsync = async (usuarioId: number): Promise<Usuario> => {
    const response = await fetch(`${CONFIG.API_BASE_URL}/Usuario/GetById?id=${usuarioId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    })

    if (!response.ok) {
      throw new Error(`Error ${response.status}: ${response.statusText}`)
    }

    const data: Usuario = await response.json()
    return data
  }

  const validateForm = (): boolean => {
    if (!descripcion.trim()) {
      Alert.alert("Error", "Por favor, completa todos los campos.")
      return false
    }
    return true
  }

  const handleGuardar = async () => {
    try {
      if (!validateForm()) {
        return
      }

      if (!userData) {
        throw new Error("No hay datos de usuario disponibles")
      }

      setSaving(true)
      setError(null)

      // Preparar datos del comunicado
      const comunicadoData = {
        Id_Usuario: userData.id,
        Id_Curso: cursoId,
        Descripcion: descripcion,
        Fecha: new Date().toISOString(),
        ...(esEdicion && comunicadoId && { Id: comunicadoId }),
      }
      // Crear o actualizar el comunicado
      const response = await fetch(
        esEdicion
          ? `${CONFIG.API_BASE_URL}/CuadernoComunicados/UpdateCuadernoComunicados`
          : `${CONFIG.API_BASE_URL}/CuadernoComunicados/CreateCuadernoComunicados`,
        {
          method: esEdicion ? "PUT" : "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(comunicadoData),
        },
      )
      if (!response.ok) {
        const errorResponse = await response.text()
        throw new Error(
          esEdicion
            ? `Error al actualizar el comunicado: ${errorResponse}`
            : `Error al crear el comunicado: ${errorResponse}`,
        )
      }
      const responseContent = await response.json()
      const comunicadoIdFinal = esEdicion ? comunicadoId : responseContent.id

      // Si es nuevo comunicado, crear las relaciones con los alumnos
      if (!esEdicion && alumnosIds.length > 0) {

        const comunicadoAlumnosData = alumnosIds.map(alumnoId => ({
          Id_Comunicado: comunicadoIdFinal,
          Id_Alumno: alumnoId
        }))

        const alumnosResponse = await fetch(`${CONFIG.API_BASE_URL}/ComunicadoAlumnos/CreateAllComunicadoAlumnos`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(comunicadoAlumnosData),
        })

        if (!alumnosResponse.ok) {
          const errorResponse = await alumnosResponse.text()
          throw new Error(`Error al asociar alumnos: ${errorResponse}`)
        }
      }
      
      Alert.alert("Éxito", "El comunicado se guardó correctamente", [
        {
          text: "OK",
          onPress: () => {
            navigation.navigate("Cuaderno", {
              cursoId: cursoId,
              cursoNombre: cursoNombre,
            })
          },
        },
      ])
    } catch (error: any) {
      setError(error.message || "Error al guardar el comunicado")
      Alert.alert("Error", error.message || "Error al guardar el comunicado")
    } finally {
      setSaving(false)
    }
  }

  const handleCancelar = () => {
    navigation.navigate("Cuaderno", {
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
            <View style={styles.formGroup}>
              <Text style={styles.label}>Alumnos</Text>
              <TextInput style={styles.readOnlyInput} value={nombresConcatenados} editable={false} multiline />
            </View>

            <View style={styles.formGroup}>
              <Text style={styles.label}>Descripción</Text>
              <TextInput
                style={styles.textArea}
                placeholder="Máximo 300 caracteres"
                value={descripcion}
                onChangeText={setDescripcion}
                multiline
                numberOfLines={6}
                textAlignVertical="top"
                maxLength={300}
              />
              <Text style={styles.characterCount}>{descripcion.length}/300</Text>
            </View>

            {error && (
              <View style={styles.errorContainer}>
                <Text style={styles.errorText}>{error}</Text>
              </View>
            )}
          </View>
        </ScrollView>

        {/* Botones parte inferior */}
        <View style={styles.buttonsContainer}>
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

          <TouchableOpacity style={styles.cancelButton} onPress={handleCancelar} disabled={saving}>
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
  formGroup: {
    marginBottom: 20,
  },
  label: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    marginBottom: 8,
    textAlign: "center",
  },
  readOnlyInput: {
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 6,
    padding: 12,
    fontSize: 16,
    color: "#333",
    backgroundColor: "#F8F9FA",
    minHeight: 50,
  },
  textArea: {
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 6,
    padding: 12,
    fontSize: 16,
    color: "#333",
    backgroundColor: "#FFFFFF",
    minHeight: 120,
  },
  characterCount: {
    fontSize: 12,
    color: "#666",
    textAlign: "right",
    marginTop: 4,
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
    position: "relative",
    bottom: 0,
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
  cancelButton: {
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

export default CreateComunicadoScreen
