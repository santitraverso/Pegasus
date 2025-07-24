"use client"

import type React from "react"
import { useState, useEffect } from "react"
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TextInput,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  Switch,
} from "react-native"
import { Picker } from "@react-native-picker/picker"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RouteProp } from "@react-navigation/native"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { Usuario } from "../models/usuario"
import type { Perfiles } from "../models/perfiles"
import type { Curso } from "../models/curso"
import type { Materia } from "../models/materia"
import type { DocenteMateria } from "../models/docenteMateria"
import type { CursoMateriaPair } from "../models/cursoMateriaPair"
import type { Hijo } from "../models/hijo"
import { CONFIG } from "../services/config"
import { useUser } from "../context/UserContext"

type NavigationProp = NativeStackNavigationProp<RootStackParamList, "CreateUsuario">
type CreateUsuarioScreenRouteProp = RouteProp<RootStackParamList, "CreateUsuario">

const CreateUsuarioScreen: React.FC = () => {
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [usuario, setUsuario] = useState<Usuario>({
    id: null,
    nombre: "",
    apellido: "",
    mail: "",
    activo: true,
    id_Perfil: null,
  })
  const [perfiles, setPerfiles] = useState<Perfiles[]>([])
  const [cursos, setCursos] = useState<Curso[]>([])
  const [materias, setMaterias] = useState<Materia[]>([])
  const [cursoMateriaPairs, setCursoMateriaPairs] = useState<CursoMateriaPair[]>([])

  // Estados para manejo de hijos
  const [hijosDisponibles, setHijosDisponibles] = useState<Usuario[]>([])
  const [hijosSeleccionados, setHijosSeleccionados] = useState<number[]>([])
  const [searchHijos, setSearchHijos] = useState("")
  const [filteredHijos, setFilteredHijos] = useState<Usuario[]>([])

  const { userData } = useUser()
  const isAdmin = userData?.id_perfil === 1

  const navigation = useNavigation<NavigationProp>()
  const route = useRoute<CreateUsuarioScreenRouteProp>()
  const { usuarioId } = route.params || {}

  const isEditing = !!usuarioId
  const isDocente = usuario.id_Perfil === 3
  const isPadre = usuario.id_Perfil === 4 // Asumiendo que 4 es el ID del perfil Padre

  // Cargar datos iniciales
  useEffect(() => {
    loadInitialData()
  }, [usuarioId])

  // Filtrar hijos cuando cambia la búsqueda
  useEffect(() => {
    if (searchHijos.trim() === "") {
      setFilteredHijos(hijosDisponibles)
    } else {
      const filtered = hijosDisponibles.filter((hijo) => {
        const nombreCompleto = `${hijo.apellido} ${hijo.nombre} ${hijo.mail}`.toLowerCase()
        return nombreCompleto.includes(searchHijos.toLowerCase())
      })
      setFilteredHijos(filtered)
    }
  }, [searchHijos, hijosDisponibles])

  const loadInitialData = async () => {
    try {
      setLoading(true)
      await Promise.all([loadPerfiles(), loadCursos(), loadMaterias(), loadHijosDisponibles()])

      if (isEditing) {
        await loadUsuario()
      }
    } catch (error) {
      Alert.alert("Error", "Error al cargar los datos iniciales")
    } finally {
      setLoading(false)
    }
  }

  // Cargar perfiles
  const loadPerfiles = async () => {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/Perfiles/GetPerfilesForCombo`)
      if (response.ok) {
        const data = await response.json()

        console.log("ES ADMIN ", isAdmin)
        console.log("Perfil Usuario ", userData?.id_perfil)
        // Si no es admin, filtrar para excluir el perfil de administrador
        const filteredPerfiles = isAdmin ? data : data.filter((perfil: Perfiles) => perfil.id !== 1)

        setPerfiles(filteredPerfiles)
      }
    } catch (error) {
      console.error("Error loading perfiles:", error)
    }
  }

  // Cargar cursos
  const loadCursos = async () => {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/Curso/GetCursosForCombo`)
      if (response.ok) {
        const data = await response.json()
        setCursos(data)
      }
    } catch (error) {
      console.error("Error loading cursos:", error)
    }
  }

  // Cargar materias
  const loadMaterias = async () => {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/Materia/GetMateriasForCombo`)
      if (response.ok) {
        const data = await response.json()
        setMaterias(data)
      }
    } catch (error) {
      console.error("Error loading materias:", error)
    }
  }

  // Cargar hijos disponibles (usuarios con perfil Alumno)
  const loadHijosDisponibles = async () => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_perfil==2`) // Asumiendo que 2 es el ID del perfil Alumno
      const response = await fetch(`${CONFIG.API_BASE_URL}/Usuario/GetUsuariosForCombo?query=${queryParam}`)
      if (response.ok) {
        const data = await response.json()
        setHijosDisponibles(data)
        setFilteredHijos(data)
      }
    } catch (error) {
      console.error("Error loading hijos disponibles:", error)
    }
  }

  // Cargar usuario para edición
  const loadUsuario = async () => {
    try {
      const response = await fetch(`${CONFIG.API_BASE_URL}/Usuario/GetById?id=${usuarioId}`)
      if (response.ok) {
        const data = await response.json()
        setUsuario(data)

        // Si es docente, cargar sus cursos y materias
        if (data.id_Perfil === 3) {
          await loadDocenteMaterias(usuarioId!)
        }

        // Si es padre, cargar sus hijos
        if (data.id_Perfil === 4) {
          await loadHijosPadre(usuarioId!)
        }
      }
    } catch (error) {
      console.error("Error loading usuario:", error)
    }
  }

  // Cargar relaciones docente-materia
  const loadDocenteMaterias = async (docenteId: number) => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_docente==${docenteId}`)
      const response = await fetch(
        `${CONFIG.API_BASE_URL}/DocenteMateria/GetDocenteMateriaForCombo?query=${queryParam}`,
      )
      if (response.ok) {
        const data: DocenteMateria[] = await response.json()
        const pairs = data.map((item) => ({
          cursoId: item.id_Curso,
          materiaId: item.id_Materia,
        }))
        setCursoMateriaPairs(pairs)
      }
    } catch (error) {
      console.error("Error loading docente materias:", error)
    }
  }

  // Cargar hijos del padre
  const loadHijosPadre = async (padreId: number) => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_padre==${padreId}`)
      const response = await fetch(`${CONFIG.API_BASE_URL}/Hijo/GetHijosForCombo?query=${queryParam}`)
      if (response.ok) {
        const data: Hijo[] = await response.json()
        const hijosIds = data.filter((hijo) => hijo.id_Hijo).map((hijo) => hijo.id_Hijo!)
        setHijosSeleccionados(hijosIds)
      }
    } catch (error) {
      console.error("Error loading hijos padre:", error)
    }
  }

  // Manejar selección/deselección de hijos
  const toggleHijoSelection = (hijoId: number) => {
    setHijosSeleccionados((prev) => {
      if (prev.includes(hijoId)) {
        return prev.filter((id) => id !== hijoId)
      } else {
        return [...prev, hijoId]
      }
    })
  }

  // Validar formulario
  const validateForm = (): boolean => {
    if (!usuario.nombre?.trim()) {
      Alert.alert("Error", "El nombre es requerido")
      return false
    }
    if (!usuario.apellido?.trim()) {
      Alert.alert("Error", "El apellido es requerido")
      return false
    }
    if (!usuario.mail?.trim()) {
      Alert.alert("Error", "El email es requerido")
      return false
    }
    if (!usuario.id_Perfil) {
      Alert.alert("Error", "El perfil es requerido")
      return false
    }
    if (isDocente && cursoMateriaPairs.length === 0) {
      Alert.alert("Error", "Debe seleccionar al menos un curso y materia para el docente")
      return false
    }
    if (isDocente) {
      const hasInvalidPair = cursoMateriaPairs.some((pair) => !pair.cursoId || !pair.materiaId)
      if (hasInvalidPair) {
        Alert.alert("Error", "Todos los pares curso-materia deben estar completos")
        return false
      }
    }
    // Validación para padres
    if (isPadre && hijosSeleccionados.length === 0) {
      Alert.alert("Error", "Debe seleccionar al menos un hijo")
      return false
    }

    return true
  }

  // Guardar usuario
  const handleSave = async () => {
    if (!validateForm()) return

    try {
      setSaving(true)

      // Limpiar el email de espacios en blanco
      const emailLimpio = usuario.mail?.trim() || ""

      const usuarioData = {
        Id: isEditing ? usuario.id : null,
        Nombre: usuario.nombre,
        Apellido: usuario.apellido,
        Mail: emailLimpio,
        Activo: usuario.activo,
        Id_Perfil: usuario.id_Perfil,
      }

      const response = await fetch(`${CONFIG.API_BASE_URL}/Usuario/${isEditing ? "UpdateUsuario" : "CreateUsuario"}`, {
        method: isEditing ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(usuarioData),
      })

      if (!response.ok) {
        throw new Error("Error al guardar el usuario")
      }

      let usuarioId = usuario.id
      if (!isEditing) {
        const responseData = await response.json()
        usuarioId = responseData.id
      }

      // Si es docente, manejar relaciones curso-materia
      if (isDocente && usuarioId) {
        await handleDocenteMaterias(usuarioId)
      }

      // Si es padre, manejar relaciones padre-hijo
      if (isPadre && usuarioId) {
        await handlePadreHijos(usuarioId)
      }

      Alert.alert("Éxito", "Usuario guardado correctamente", [
        {
          text: "OK",
          onPress: () => navigation.goBack(),
        },
      ])
    } catch (error) {
      console.error("Error saving usuario:", error)
      Alert.alert("Error", "Hubo un error al guardar el usuario")
    } finally {
      setSaving(false)
    }
  }

  // Manejar relaciones docente-materia
  const handleDocenteMaterias = async (docenteId: number) => {
    try {
      // Primero eliminar relaciones existentes si es edición
      if (isEditing) {
        await deleteExistingDocenteMaterias(docenteId)
      }

      if (cursoMateriaPairs.length > 0) {
        const docenteMateriasData = cursoMateriaPairs.map((pair) => ({
          Id_Docente: docenteId,
          Id_Materia: pair.materiaId,
          Id_Curso: pair.cursoId,
        }))

        const response = await fetch(`${CONFIG.API_BASE_URL}/DocenteMateria/CreateAllDocenteMateria`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(docenteMateriasData),
        })

        if (!response.ok) {
          const errorResponse = await response.text()
          throw new Error(`Error al crear relaciones docente-materia: ${errorResponse}`)
        }
      }
    } catch (error) {
      throw error
    }
  }

  // Manejar relaciones padre-hijo
  const handlePadreHijos = async (padreId: number) => {
    try {
      // Primero eliminar relaciones existentes si es edición
      if (isEditing) {
        await deleteExistingPadreHijos(padreId)
      }

      if (hijosSeleccionados.length > 0) {
        const padreHijosData = hijosSeleccionados.map((hijoId) => ({
          Id_Padre: padreId,
          Id_Hijo: hijoId,
        }))

        const response = await fetch(`${CONFIG.API_BASE_URL}/Hijo/CreateAllHijos`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(padreHijosData),
        })

        if (!response.ok) {
          const errorResponse = await response.text()
          throw new Error(`Error al crear relaciones padre-hijo: ${errorResponse}`)
        }
      }
    } catch (error) {
      console.error("Error saving padre hijos:", error)
      throw error
    }
  }

  // Eliminar relaciones docente-materia existentes
  const deleteExistingDocenteMaterias = async (docenteId: number) => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_docente==${docenteId}`)
      const response = await fetch(
        `${CONFIG.API_BASE_URL}/DocenteMateria/GetDocenteMateriaForCombo?query=${queryParam}`,
      )

      if (response.ok) {
        const existingRelations: DocenteMateria[] = await response.json()

        if (existingRelations.length > 0) {
          const relacionesEliminar = existingRelations.map((relation) => ({ Id: relation.id }))

          const deleteResponse = await fetch(`${CONFIG.API_BASE_URL}/DocenteMateria/DeleteAllDocenteMateria`, {
            method: "DELETE",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify(relacionesEliminar),
          })

          if (!deleteResponse.ok) {
            const errorResponse = await deleteResponse.text()
            throw new Error(`Error al eliminar relaciones docente-materia existentes: ${errorResponse}`)
          }
        }
      }
    } catch (error) {
      console.error("Error deleting existing docente materias:", error)
      throw error
    }
  }

  // Eliminar relaciones padre-hijo existentes
  const deleteExistingPadreHijos = async (padreId: number) => {
    try {
      const queryParam = encodeURIComponent(`x=>x.id_padre==${padreId}`)
      const response = await fetch(`${CONFIG.API_BASE_URL}/Hijo/GetHijosForCombo?query=${queryParam}`)

      if (response.ok) {
        const existingRelations: Hijo[] = await response.json()

        if (existingRelations.length > 0) {
          const relacionesEliminar = existingRelations.map((relation) => ({ Id: relation.id }))

          const deleteResponse = await fetch(`${CONFIG.API_BASE_URL}/Hijo/DeleteAllHijos`, {
            method: "DELETE",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify(relacionesEliminar),
          })

          if (!deleteResponse.ok) {
            const errorResponse = await deleteResponse.text()
            throw new Error(`Error al eliminar relaciones padre-hijo existentes: ${errorResponse}`)
          }
        }
      }
    } catch (error) {
      console.error("Error deleting existing padre hijos:", error)
      throw error
    }
  }

  // Agregar par curso-materia
  const addCursoMateriaPair = () => {
    setCursoMateriaPairs([...cursoMateriaPairs, { cursoId: null, materiaId: null }])
  }

  // Eliminar par curso-materia
  const removeCursoMateriaPair = (index: number) => {
    const newPairs = cursoMateriaPairs.filter((_, i) => i !== index)
    setCursoMateriaPairs(newPairs)
  }

  // Actualizar par curso-materia
  const updateCursoMateriaPair = (index: number, field: "cursoId" | "materiaId", value: number | null) => {
    const newPairs = [...cursoMateriaPairs]
    newPairs[index] = { ...newPairs[index], [field]: value }
    setCursoMateriaPairs(newPairs)
  }

  // Renderizar item de hijo - CAMBIADO: Ya no es para FlatList
  const renderHijoItem = (item: Usuario, index: number) => (
    <TouchableOpacity
      key={item.id?.toString() || index.toString()}
      style={styles.hijoItem}
      onPress={() => toggleHijoSelection(item.id!)}
    >
      <View style={styles.hijoCheckbox}>
        <Icon
          name={hijosSeleccionados.includes(item.id!) ? "check-box" : "check-box-outline-blank"}
          size={24}
          color={hijosSeleccionados.includes(item.id!) ? "#4285F4" : "#666"}
        />
      </View>
      <View style={styles.hijoInfo}>
        <Text style={styles.hijoNombre}>
          {item.apellido}, {item.nombre}
        </Text>
        <Text style={styles.hijoEmail}>{item.mail}</Text>
      </View>
    </TouchableOpacity>
  )

  if (loading) {
    return (
      <AppLayout>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando...</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <View style={styles.mainContainer}>
        <ScrollView style={styles.container} contentContainerStyle={styles.scrollContent}>
          <View style={styles.header}>
            <Text style={styles.headerTitle}>{isEditing ? "Editar Usuario" : "Crear Usuario"}</Text>
          </View>

          <View style={styles.formContainer}>
            <View style={styles.formCard}>
              <View style={styles.inputGroup}>
                <Text style={styles.label}>Apellido *</Text>
                <TextInput
                  style={styles.input}
                  value={usuario.apellido || ""}
                  onChangeText={(text) => setUsuario({ ...usuario, apellido: text })}
                  placeholder="Ingrese el apellido"
                />
              </View>

              <View style={styles.inputGroup}>
                <Text style={styles.label}>Nombre *</Text>
                <TextInput
                  style={styles.input}
                  value={usuario.nombre || ""}
                  onChangeText={(text) => setUsuario({ ...usuario, nombre: text })}
                  placeholder="Ingrese el nombre"
                />
              </View>

              <View style={styles.inputGroup}>
                <Text style={styles.label}>Email *</Text>
                <TextInput
                  style={styles.input}
                  value={usuario.mail || ""}
                  onChangeText={(text) => setUsuario({ ...usuario, mail: text.trim() })}
                  placeholder="Ingrese el email"
                  keyboardType="email-address"
                  autoCapitalize="none"
                />
              </View>

              <View style={styles.inputGroup}>
                <Text style={styles.label}>Perfil *</Text>
                <View style={styles.pickerContainer}>
                  <Picker
                    selectedValue={usuario.id_Perfil}
                    onValueChange={(value) => {
                      setUsuario({ ...usuario, id_Perfil: value })
                      if (value !== 3) {
                        setCursoMateriaPairs([])
                      }
                      if (value !== 4) {
                        setHijosSeleccionados([])
                      }
                    }}
                    style={styles.picker}
                    mode="dropdown"
                    dropdownIconColor="#333"
                    itemStyle={styles.pickerItem}
                  >
                    <Picker.Item
                      label="-- Seleccionar perfil --"
                      value={null}
                      color="#666"
                      style={styles.pickerItemStyle}
                    />
                    {perfiles.map((perfil) => (
                      <Picker.Item
                        key={perfil.id}
                        label={perfil.nombre}
                        value={perfil.id}
                        color="#333"
                        style={styles.pickerItemStyle}
                      />
                    ))}
                  </Picker>
                </View>
              </View>

              <View style={styles.switchGroup}>
                <Text style={styles.label}>Activo</Text>
                <Switch
                  value={usuario.activo}
                  onValueChange={(value) => setUsuario({ ...usuario, activo: value })}
                  trackColor={{ false: "#767577", true: "#4285F4" }}
                  thumbColor={usuario.activo ? "#FFFFFF" : "#f4f3f4"}
                />
              </View>

              {isDocente && (
                <View style={styles.docenteSection}>
                  <View style={styles.sectionHeader}>
                    <Text style={styles.sectionTitle}>Cursos y Materias</Text>
                    <TouchableOpacity style={styles.addButton} onPress={addCursoMateriaPair}>
                      <Icon name="add" size={20} color="#4285F4" />
                      <Text style={styles.addButtonText}>Agregar</Text>
                    </TouchableOpacity>
                  </View>

                  {cursoMateriaPairs.map((pair, index) => (
                    <View key={index} style={styles.pairContainer}>
                      <View style={styles.pairRow}>
                        <View style={styles.pairInput}>
                          <Text style={styles.label}>Curso</Text>
                          <View style={styles.pickerContainer}>
                            <Picker
                              selectedValue={pair.cursoId}
                              onValueChange={(value) => updateCursoMateriaPair(index, "cursoId", value)}
                              style={styles.picker}
                              mode="dropdown"
                              dropdownIconColor="#333"
                              itemStyle={styles.pickerItem}
                            >
                              <Picker.Item
                                label="-- Seleccionar curso --"
                                value={null}
                                color="#666"
                                style={styles.pickerItemStyle}
                              />
                              {cursos.map((curso) => (
                                <Picker.Item
                                  key={curso.id}
                                  label={curso.nombre_Curso || ""}
                                  value={curso.id}
                                  color="#333"
                                  style={styles.pickerItemStyle}
                                />
                              ))}
                            </Picker>
                          </View>
                        </View>

                        <View style={styles.pairInput}>
                          <Text style={styles.label}>Materia</Text>
                          <View style={styles.pickerContainer}>
                            <Picker
                              selectedValue={pair.materiaId}
                              onValueChange={(value) => updateCursoMateriaPair(index, "materiaId", value)}
                              style={styles.picker}
                              mode="dropdown"
                              dropdownIconColor="#333"
                              itemStyle={styles.pickerItem}
                            >
                              <Picker.Item
                                label="-- Seleccionar materia --"
                                value={null}
                                color="#666"
                                style={styles.pickerItemStyle}
                              />
                              {materias.map((materia) => (
                                <Picker.Item
                                  key={materia.id}
                                  label={materia.nombre || ""}
                                  value={materia.id}
                                  color="#333"
                                  style={styles.pickerItemStyle}
                                />
                              ))}
                            </Picker>
                          </View>
                        </View>

                        <TouchableOpacity style={styles.removeButton} onPress={() => removeCursoMateriaPair(index)}>
                          <Icon name="delete-outline" size={20} color="#F44336" />
                        </TouchableOpacity>
                      </View>
                    </View>
                  ))}
                </View>
              )}

              {/* Sección para padres - ARREGLADA: Sin FlatList */}
              {isPadre && (
                <View style={styles.padreSection}>
                  <View style={styles.sectionHeader}>
                    <Text style={styles.sectionTitle}>Seleccionar Hijos</Text>
                    <Text style={styles.selectedCount}>
                      {hijosSeleccionados.length} seleccionado{hijosSeleccionados.length !== 1 ? "s" : ""}
                    </Text>
                  </View>

                  <View style={styles.searchContainer}>
                    <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
                    <TextInput
                      style={styles.searchInput}
                      placeholder="Buscar hijos..."
                      value={searchHijos}
                      onChangeText={setSearchHijos}
                    />
                  </View>

                  <View style={styles.hijosListContainer}>
                    <ScrollView
                      style={styles.hijosScrollView}
                      showsVerticalScrollIndicator={true}
                      nestedScrollEnabled={true}
                    >
                      {filteredHijos.length > 0 ? (
                        filteredHijos.map((item, index) => renderHijoItem(item, index))
                      ) : (
                        <View style={styles.emptyContainer}>
                          <Text style={styles.emptyText}>
                            {searchHijos ? "No se encontraron hijos" : "No hay hijos disponibles"}
                          </Text>
                        </View>
                      )}
                    </ScrollView>
                  </View>
                </View>
              )}
            </View>
          </View>
        </ScrollView>

        <View style={styles.floatingFooter}>
          <TouchableOpacity
            style={[styles.saveButton, saving && styles.disabledButton]}
            onPress={handleSave}
            disabled={saving}
          >
            {saving ? (
              <ActivityIndicator size="small" color="#FFFFFF" />
            ) : (
              <>
                <Icon name="save" size={20} color="#FFFFFF" />
                <Text style={styles.saveButtonText}>Guardar</Text>
              </>
            )}
          </TouchableOpacity>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <Icon name="arrow-back" size={20} color="#FFFFFF" />
            <Text style={styles.backButtonText}>Atrás</Text>
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
  loadingContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
    color: "#666",
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
  formContainer: {
    padding: 16,
  },
  formCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
    padding: 20,
    shadowColor: "#000",
    shadowOffset: {
      width: 0,
      height: 2,
    },
    shadowOpacity: 0.1,
    shadowRadius: 3.84,
    elevation: 5,
  },
  inputGroup: {
    marginBottom: 16,
  },
  label: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    marginBottom: 8,
  },
  input: {
    backgroundColor: "#FFFFFF",
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 12,
    fontSize: 16,
    color: "#333",
  },
  pickerContainer: {
    backgroundColor: "#FFFFFF",
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 8,
    minHeight: 50,
    overflow: "hidden",
  },
  picker: {
    height: 50,
    color: "#333",
    fontSize: 16,
    backgroundColor: "#FFFFFF",
  },
  pickerItem: {
    backgroundColor: "#FFFFFF",
    color: "#333",
    fontSize: 16,
    height: 50,
  },
  pickerItemStyle: {
    backgroundColor: "#FFFFFF",
    fontSize: 16,
    paddingVertical: 12,
    paddingHorizontal: 16,
  },
  switchGroup: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 16,
    backgroundColor: "#FFFFFF",
    padding: 16,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#E0E0E0",
  },
  docenteSection: {
    marginTop: 16,
  },
  // Estilos para la sección de padres
  padreSection: {
    marginTop: 16,
  },
  sectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 16,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#333",
  },
  selectedCount: {
    fontSize: 14,
    color: "#4285F4",
    fontWeight: "600",
  },
  searchContainer: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#F8F9FA",
    borderRadius: 8,
    paddingHorizontal: 12,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: "#E0E0E0",
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
  hijosListContainer: {
    height: 250, // Fixed height instead of maxHeight
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 8,
    backgroundColor: "#FFFFFF",
    overflow: "hidden", // Ensure content doesn't overflow
  },
  hijoItem: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  hijoCheckbox: {
    marginRight: 12,
  },
  hijoInfo: {
    flex: 1,
  },
  hijoNombre: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    marginBottom: 2,
  },
  hijoEmail: {
    fontSize: 14,
    color: "#666",
  },
  emptyContainer: {
    padding: 20,
    alignItems: "center",
  },
  emptyText: {
    fontSize: 16,
    color: "#666",
    textAlign: "center",
  },
  addButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#E3F2FD",
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 6,
  },
  addButtonText: {
    color: "#4285F4",
    fontWeight: "600",
    marginLeft: 4,
  },
  pairContainer: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: "#E0E0E0",
  },
  pairRow: {
    flexDirection: "row",
    alignItems: "flex-end",
  },
  pairInput: {
    flex: 1,
    marginRight: 8,
  },
  removeButton: {
    padding: 8,
    alignSelf: "flex-end",
    backgroundColor: "#FFEBEE",
    borderRadius: 20,
    marginLeft: 8,
    marginBottom: 10,
  },
  saveButton: {
    flex: 1,
    backgroundColor: "#4285F4",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 12,
    borderRadius: 8,
  },
  disabledButton: {
    backgroundColor: "#CCCCCC",
  },
  saveButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
    marginLeft: 8,
  },
  backButton: {
    flex: 1,
    backgroundColor: "#6c757d",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 12,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: "#E0E0E0",
  },
  backButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "bold",
    marginLeft: 8,
  },
  mainContainer: {
    flex: 1,
  },
  scrollContent: {
    paddingBottom: 80,
  },
  hijosScrollView: {
    flex: 1,
  },
  floatingFooter: {
    position: "absolute",
    bottom: 0,
    left: 0,
    right: 0,
    flexDirection: "row",
    padding: 16,
    gap: 12,
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
    shadowColor: "#000",
    shadowOffset: {
      width: 0,
      height: -2,
    },
    shadowOpacity: 0.1,
    shadowRadius: 3.84,
    elevation: 5,
  },
})

export default CreateUsuarioScreen
