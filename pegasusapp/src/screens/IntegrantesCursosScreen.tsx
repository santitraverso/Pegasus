import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator, Alert, TextInput } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { Usuario } from "../models/usuario"
import type { IntegrantesCursos } from "../models/integrantesCursos"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

// Definir los tipos para las rutas
type IntegrantesCursosScreenRouteProp = RouteProp<
  {
    params: {
      cursoId: number
    }
  },
  "params"
>

type IntegrantesCursosScreenNavigationProp = NativeStackNavigationProp<any>

const IntegrantesCursosScreen: React.FC = () => {
  const route = useRoute<IntegrantesCursosScreenRouteProp>()
  const navigation = useNavigation<IntegrantesCursosScreenNavigationProp>()
  const { cursoId } = route.params

  const [alumnos, setAlumnos] = useState<IntegrantesCursos[]>([]) // Cambio: ahora es IntegrantesCursos[]
  const [alumnosSeleccionados, setAlumnosSeleccionados] = useState<Set<number>>(new Set())
  const [integrantesCurso, setIntegrantesCurso] = useState<IntegrantesCursos[]>([]) // Cambio: renombrado
  const [filteredAlumnos, setFilteredAlumnos] = useState<IntegrantesCursos[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searchText, setSearchText] = useState("")

  useEffect(() => {
    fetchData()
  }, [cursoId])

  useEffect(() => {
    // Filtrar alumnos cuando cambia el texto de búsqueda
    if (searchText.trim() === "") {
      setFilteredAlumnos(alumnos)
    } else {
      const filtered = alumnos.filter((alumno) => {
        const nombreCompleto = `${alumno.usuario?.nombre || ""} ${alumno.usuario?.apellido || ""}`.toLowerCase()
        return nombreCompleto.includes(searchText.toLowerCase())
      })
      setFilteredAlumnos(filtered)
    }
  }, [searchText, alumnos])

  const fetchData = async () => {
    try {
      setLoading(true)
      setError(null)

      //traer todos los usuarios alumnos
      const todosLosUsuarios = await getUsuariosAlumnosAsync()

      // Crear lista de IntegrantesCursos a partir de los usuarios
      const alumnosFormateados: IntegrantesCursos[] = todosLosUsuarios.map((usuario) => ({
        id: 0,
        id_Usuario: usuario.id || 0, 
        id_Curso: cursoId,
        usuario: usuario,
      }))

      setAlumnos(alumnosFormateados)
      setFilteredAlumnos(alumnosFormateados)

      // Traer los integrantes actuales del curso para marcar en la lista
      const integrantesActuales = await getIntegrantesCursosAsync(cursoId)
      setIntegrantesCurso(integrantesActuales)

      // Marcar los alumnos que ya están en el curso
      const idsSeleccionados = new Set<number>()
      for (const alumno of alumnosFormateados) {
        if (integrantesActuales.some((integrante) => integrante.id_Usuario === alumno.id_Usuario)) {
          if (alumno.id_Usuario) {
            idsSeleccionados.add(alumno.id_Usuario)
          }
        }
      }
      setAlumnosSeleccionados(idsSeleccionados)
    } catch (error: any) {
      setError(error.message || "Error al cargar los datos")
    } finally {
      setLoading(false)
    }
  }

  // obtener usuarios que son alumnos activos
  const getUsuariosAlumnosAsync = async (): Promise<Usuario[]> => {

    // Obtener alumnos activos (perfil 2 y activo true)
    const queryParam = encodeURIComponent(`x=>x.id_perfil == 2 && x.activo == true`)
    const response = await fetch(`${CONFIG.API_BASE_URL}/Usuario/GetUsuariosForCombo?query=${queryParam}`, {
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

  const getIntegrantesCursosAsync = async (curso: number): Promise<IntegrantesCursos[]> => {

    const queryParam = encodeURIComponent(`x=>x.id_curso == ${curso}`)
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

  const toggleAlumnoSeleccion = (alumnoId: number) => {
    if (!alumnoId) return 

    const nuevaSeleccion = new Set(alumnosSeleccionados)

    if (nuevaSeleccion.has(alumnoId)) {
      nuevaSeleccion.delete(alumnoId)
    } else {
      nuevaSeleccion.add(alumnoId)
    }

    setAlumnosSeleccionados(nuevaSeleccion)
  }

  const handleGuardar = async () => {
  try {
    setSaving(true)
    setError(null)

    //Eliminar todos los integrantes actuales
    const borradoExitoso = await borrarIntegrantesAsync()
    
    if (borradoExitoso) {
      //Agregar todos los integrantes seleccionados
      await guardarIntegrantesMasivoAsync(cursoId, Array.from(alumnosSeleccionados))
      
      Alert.alert("Éxito", "Los alumnos se asignaron correctamente al curso", [
        {
          text: "OK",
          onPress: () => navigation.navigate("CreateCurso", { cursoId }),
        },
      ])
    } else {
      throw new Error("Error al eliminar los integrantes actuales del curso")
    }
  } catch (error: any) {
    setError(error.message || "Error al guardar los integrantes del curso")
    Alert.alert("Error", error.message || "Error al guardar los integrantes del curso")
  } finally {
    setSaving(false)
  }
}

// Borrar todos los integrantes actuales
const borrarIntegrantesAsync = async (): Promise<boolean> => {
  try {
    if (integrantesCurso.length === 0) {
      return true
    }
    
    const integrantesEliminar = integrantesCurso.map(alumno => ({ Id: alumno.id }))
    
    const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesCursos/DeleteAllIntegrantesCursos`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(integrantesEliminar),
    })
    
    return response.ok
  } catch (error) {
    console.error("Error al borrar integrantes:", error)
    return false
  }
}

// Crear todos los integrantes
const guardarIntegrantesMasivoAsync = async (curso: number, alumnosIds: number[]): Promise<void> => {
  if (alumnosIds.length === 0) {
    return
  }
  
  const integrantesData = alumnosIds.map(alumnoId => ({
    ID_CURSO: curso.toString(),
    ID_USUARIO: alumnoId.toString(),
  }))
  
  const response = await fetch(`${CONFIG.API_BASE_URL}/IntegrantesCursos/CreateAllIntegrantesCursos`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(integrantesData),
  })
  
  if (!response.ok) {
    const errorResponse = await response.text()
    throw new Error(`Error al crear integrantes: ${errorResponse}`)
  }
}

  const handleVolver = () => {
    navigation.navigate("CreateCurso", { cursoId })
  }

  const renderAlumnoItem = ({ item }: { item: IntegrantesCursos }) => {
    if (!item.id_Usuario) return null

    return (
      <TouchableOpacity
        style={[styles.alumnoItem, alumnosSeleccionados.has(item.id_Usuario) && styles.alumnoItemSelected]}
        onPress={() => toggleAlumnoSeleccion(item.id_Usuario!)}
      >
        <View style={styles.alumnoInfo}>
          <Text style={styles.alumnoApellido}>{item.usuario?.apellido}</Text>
          <Text style={styles.alumnoNombre}>{item.usuario?.nombre}</Text>
        </View>
        <View style={[styles.checkbox, alumnosSeleccionados.has(item.id_Usuario) && styles.checkboxSelected]}>
          {alumnosSeleccionados.has(item.id_Usuario) && <Icon name="check" size={16} color="#FFFFFF" />}
        </View>
      </TouchableOpacity>
    )
  }

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando alumnos...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar alumnos</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchData}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (filteredAlumnos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="person" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>
            {searchText ? "No se encontraron alumnos" : "No hay alumnos disponibles"}
          </Text>
        </View>
      )
    }

    return (
      <FlatList
        data={filteredAlumnos}
        renderItem={renderAlumnoItem}
        keyExtractor={(item) => item.id_Usuario?.toString() || ""}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
      />
    )
  }

  return (
    <AppLayout title="Asignar Alumnos" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Asignar Alumnos al Curso</Text>
          <Text style={styles.headerSubtitle}>Seleccione los alumnos que desea asignar a este curso</Text>
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

        {/* Contador de seleccionados */}
        <View style={styles.selectionInfo}>
          <Text style={styles.selectionText}>
            {alumnosSeleccionados.size} alumno{alumnosSeleccionados.size !== 1 ? "s" : ""} seleccionado
            {alumnosSeleccionados.size !== 1 ? "s" : ""}
          </Text>
        </View>

        {/* Lista de alumnos */}
        <View style={styles.contentContainer}>{renderContent()}</View>

        {/* Botones de acción */}
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
  selectionInfo: {
    backgroundColor: "#E3F2FD",
    paddingVertical: 8,
    paddingHorizontal: 16,
    marginHorizontal: 16,
    marginBottom: 16,
    borderRadius: 8,
  },
  selectionText: {
    color: "#4285F4",
    fontSize: 14,
    fontWeight: "500",
  },
  contentContainer: {
    flex: 1,
    marginHorizontal: 16,
  },
  listContainer: {
    paddingBottom: 16,
  },
  alumnoItem: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    backgroundColor: "#FFFFFF",
    padding: 16,
    borderRadius: 8,
    marginBottom: 8,
    elevation: 1,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
  },
  alumnoItemSelected: {
    backgroundColor: "#E3F2FD",
    borderColor: "#4285F4",
    borderWidth: 1,
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
  checkbox: {
    width: 20,
    height: 20,
    borderWidth: 2,
    borderColor: "#4285F4",
    borderRadius: 4,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "#FFFFFF",
  },
  checkboxSelected: {
    backgroundColor: "#4285F4",
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

export default IntegrantesCursosScreen
