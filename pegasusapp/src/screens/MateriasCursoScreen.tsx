import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator, Alert, TextInput } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useRoute, useNavigation } from "@react-navigation/native"
import type { RouteProp } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { Materia } from "../models/materia"
import type { CursoMateria } from "../models/cursoMateria"
import { useUser } from "../context/UserContext"
import { CONFIG } from "../services/config"

// Definir los tipos para las rutas
type MateriasCursoScreenRouteProp = RouteProp<
  {
    params: {
      cursoId: number
    }
  },
  "params"
>

type MateriasCursoScreenNavigationProp = NativeStackNavigationProp<any>

const MateriasCursoScreen: React.FC = () => {
  const route = useRoute<MateriasCursoScreenRouteProp>()
  const navigation = useNavigation<MateriasCursoScreenNavigationProp>()
  const { cursoId } = route.params

  const [materias, setMaterias] = useState<Materia[]>([])
  const [materiasSeleccionadas, setMateriasSeleccionadas] = useState<Set<number>>(new Set())
  const [materiasCurso, setMateriasCurso] = useState<CursoMateria[]>([])
  const [filteredMaterias, setFilteredMaterias] = useState<Materia[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searchText, setSearchText] = useState("")

  useEffect(() => {
    fetchData()
  }, [cursoId])

  useEffect(() => {
    // Filtrar materias cuando cambia el texto de búsqueda
    if (searchText.trim() === "") {
      setFilteredMaterias(materias)
    } else {
      const filtered = materias.filter((materia) => {
        const nombre = materia.nombre?.toLowerCase() || ""
        return nombre.includes(searchText.toLowerCase())
      })
      setFilteredMaterias(filtered)
    }
  }, [searchText, materias])

  const fetchData = async () => {
    try {
      setLoading(true)
      setError(null)

      //primero cargar todas las materias
      const todasLasMaterias = await getMateriasAsync()
      setMaterias(todasLasMaterias)
      setFilteredMaterias(todasLasMaterias)

      // Traer las materias actuales del curso para marcar en la lista
      const materiasDelCurso = await getMateriasCursoAsync(cursoId)
      setMateriasCurso(materiasDelCurso)

      // Marcar las materias que ya están en el curso
      const idsSeleccionados = new Set<number>()
      for (const materia of todasLasMaterias) {
        if (materiasDelCurso.some((cursoMateria) => cursoMateria.id_Materia === materia.id)) {
          if (materia.id) {
            idsSeleccionados.add(materia.id)
          }
        }
      }
      setMateriasSeleccionadas(idsSeleccionados)
    } catch (error: any) {
      setError(error.message || "Error al cargar los datos")
    } finally {
      setLoading(false)
    }
  }

  const getMateriasCursoAsync = async (curso: number): Promise<CursoMateria[]> => {

    const queryParam = encodeURIComponent(`x=>x.id_curso==${curso}`)
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

  const getMateriasAsync = async (): Promise<Materia[]> => {

    const response = await fetch(`${CONFIG.API_BASE_URL}/Materia/GetMateriasForCombo`, {
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

  const toggleMateriaSeleccion = (materiaId: number) => {
    if (!materiaId) return

    const nuevaSeleccion = new Set(materiasSeleccionadas)

    if (nuevaSeleccion.has(materiaId)) {
      nuevaSeleccion.delete(materiaId)
    } else {
      nuevaSeleccion.add(materiaId)
    }

    setMateriasSeleccionadas(nuevaSeleccion)
  }

  const handleGuardar = async () => {
  try {
    setSaving(true)
    setError(null)

    //Eliminar todas las materias actuales
    const borradoExitoso = await borrarMateriasAsync()
    
    if (borradoExitoso) {
      //Agregar todas las materias seleccionadas
      await guardarMateriasMasivoAsync(cursoId, Array.from(materiasSeleccionadas))
      
      Alert.alert("Éxito", "Las materias se asignaron correctamente al curso", [
        {
          text: "OK",
          onPress: () => navigation.navigate("CreateCurso", { cursoId }),
        },
      ])
    } else {
      throw new Error("Error al eliminar las materias actuales del curso")
    }
  } catch (error: any) {
    setError(error.message || "Error al guardar las materias del curso")
    Alert.alert("Error", error.message || "Error al guardar las materias del curso")
  } finally {
    setSaving(false)
  }
}

// Borrar todas las materias actuales
const borrarMateriasAsync = async (): Promise<boolean> => {
  try {
    if (materiasCurso.length === 0) {
      return true
    }
    
    const materiasEliminar = materiasCurso.map(materia => ({ Id: materia.id }))
    
    const response = await fetch(`${CONFIG.API_BASE_URL}/CursoMateria/DeleteAllCursoMateria`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(materiasEliminar),
    })
    
    return response.ok
  } catch (error) {
    console.error("Error al borrar materias:", error)
    return false
  }
}

// Crear todas las materias
const guardarMateriasMasivoAsync = async (curso: number, materiasIds: number[]): Promise<void> => {
  if (materiasIds.length === 0) {
    return 
  }
  
  const cursoMateriasData = materiasIds.map(materiaId => ({
    ID_CURSO: curso.toString(),
    ID_MATERIA: materiaId.toString(),
  }))
  
  const response = await fetch(`${CONFIG.API_BASE_URL}/CursoMateria/CreateAllCursoMateria`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(cursoMateriasData),
  })
  
  if (!response.ok) {
    const errorResponse = await response.text()
    throw new Error(`Error al crear materias del curso: ${errorResponse}`)
  }
}

  const handleVolver = () => {
    navigation.navigate("CreateCurso", { cursoId })
  }

  const renderMateriaItem = ({ item }: { item: Materia }) => {
    if (!item.id) return null

    return (
      <TouchableOpacity
        style={[styles.materiaItem, materiasSeleccionadas.has(item.id) && styles.materiaItemSelected]}
        onPress={() => toggleMateriaSeleccion(item.id!)}
      >
        <View style={styles.materiaInfo}>
          <Icon name="book" size={24} color="#4285F4" style={styles.materiaIcon} />
          <Text style={styles.materiaNombre}>{item.nombre}</Text>
        </View>
        <View style={[styles.checkbox, materiasSeleccionadas.has(item.id) && styles.checkboxSelected]}>
          {materiasSeleccionadas.has(item.id) && <Icon name="check" size={16} color="#FFFFFF" />}
        </View>
      </TouchableOpacity>
    )
  }

  const renderContent = () => {
    if (loading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando materias...</Text>
        </View>
      )
    }

    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar materias</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={fetchData}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      )
    }

    if (filteredMaterias.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <Icon name="book" size={64} color="#CCCCCC" />
          <Text style={styles.emptyText}>
            {searchText ? "No se encontraron materias" : "No hay materias disponibles"}
          </Text>
        </View>
      )
    }

    return (
      <FlatList
        data={filteredMaterias}
        renderItem={renderMateriaItem}
        keyExtractor={(item) => item.id?.toString() || ""}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
      />
    )
  }

  return (
    <AppLayout title="Asignar Materias" showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Asignar Materias al Curso</Text>
          <Text style={styles.headerSubtitle}>Seleccione las materias que desea asignar a este curso</Text>
        </View>

        {/* Barra de búsqueda */}
        <View style={styles.searchContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="Buscar materias..."
            value={searchText}
            onChangeText={setSearchText}
          />
        </View>

        {/* Contador de seleccionadas */}
        <View style={styles.selectionInfo}>
          <Text style={styles.selectionText}>
            {materiasSeleccionadas.size} materia{materiasSeleccionadas.size !== 1 ? "s" : ""} seleccionada
            {materiasSeleccionadas.size !== 1 ? "s" : ""}
          </Text>
        </View>

        {/* Lista de materias */}
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
  materiaItem: {
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
  materiaItemSelected: {
    backgroundColor: "#E3F2FD",
    borderColor: "#4285F4",
    borderWidth: 1,
  },
  materiaInfo: {
    flexDirection: "row",
    alignItems: "center",
    flex: 1,
  },
  materiaIcon: {
    marginRight: 12,
  },
  materiaNombre: {
    fontSize: 16,
    fontWeight: "500",
    color: "#333",
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

export default MateriasCursoScreen
