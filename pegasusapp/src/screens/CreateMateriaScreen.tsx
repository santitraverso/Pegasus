import { useState, useEffect } from "react"
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Alert, ActivityIndicator, ScrollView } from "react-native"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RouteProp } from "@react-navigation/native"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { Materia } from "../models/materia"
import type { ContenidoMaterias } from "../models/contenidoMaterias"
import { CONFIG } from "../services/config"
import AppLayout from "../components/AppLayout"
import Icon from "react-native-vector-icons/MaterialIcons"
import { useUser } from "../context/UserContext"

type CreateMateriaScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, "CreateMateria">
type CreateMateriaScreenRouteProp = RouteProp<RootStackParamList, "CreateMateria">

const CreateMateriaScreen = () => {
  const navigation = useNavigation<CreateMateriaScreenNavigationProp>()
  const route = useRoute<CreateMateriaScreenRouteProp>()
  const { materiaId, viewOnly } = route.params || {}
  const { userData } = useUser()

  const [materia, setMateria] = useState<Materia>({ id: 0, nombre: "" })
  const [contenidos, setContenidos] = useState<ContenidoMaterias[]>([])
  const [loading, setLoading] = useState(false)
  const [expandedContent, setExpandedContent] = useState<{ [key: number]: boolean }>({})

  // Determinar permisos según el perfil del usuario
  const userProfile = userData?.id_perfil || 0
  const isViewOnly = viewOnly || userProfile === 2 || userProfile === 3 || userProfile === 4
  const isEditing = Boolean(materiaId && materiaId > 0)
  const screenTitle = isEditing ? (isViewOnly ? "Datos de la materia" : "Editar Materia") : "Nueva Materia"

  useEffect(() => {
    if (isEditing) {
      fetchMateria()
      fetchContenidos()
    }
  }, [materiaId])

  const fetchMateria = async () => {
    if (!materiaId) return

    try {
      setLoading(true)
      const response = await fetch(`${CONFIG.API_BASE_URL}/Materia/GetById?id=${materiaId}`)

      if (response.ok) {
        const data = await response.json()
        setMateria(data || { id: 0, nombre: "" })
      } else {
        Alert.alert("Error", "No se pudo cargar la materia")
        navigation.navigate("Materias")
      }
    } catch (error) {
      Alert.alert("Error", "Error de conexión al cargar la materia")
      navigation.navigate("Materias")
    } finally {
      setLoading(false)
    }
  }

  const fetchContenidos = async () => {
    if (!materiaId) return

    try {
      const query = encodeURIComponent(`x=>x.id_materia==${materiaId}`)
      const response = await fetch(
        `${CONFIG.API_BASE_URL}/ContenidoMaterias/GetContenidoMateriasForCombo?query=${query}`,
      )

      if (response.ok) {
        const data = await response.json()
        setContenidos(Array.isArray(data) ? data : [])
      }
    } catch (error) {
      console.error("Error al obtener contenidos:", error)
    }
  }

  const handleSave = async () => {
    const nombreMateria = materia.nombre ? materia.nombre.trim() : ""

    if (!nombreMateria) {
      Alert.alert("Error", "El nombre de la materia es requerido")
      return
    }

    try {
      setLoading(true)

      const materiaData = {
        Nombre: nombreMateria,
        ...(isEditing && { Id: materia.id }),
      }

      const response = await fetch(`${CONFIG.API_BASE_URL}/Materia/${isEditing ? "UpdateMateria" : "CreateMateria"}`, {
        method: isEditing ? "PUT" : "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(materiaData),
      })

      if (response.ok) {
        const successMessage = isEditing ? "actualizada" : "creada"
        Alert.alert("Éxito", `Materia ${successMessage} correctamente`, [
          { text: "OK", onPress: () => navigation.navigate("Materias") },
        ])
      } else {
        const errorText = await response.text()
        const actionText = isEditing ? "actualizar" : "crear"
        Alert.alert("Error", `No se pudo ${actionText} la materia: ${errorText}`)
      }
    } catch (error) {
      console.error("Error al guardar materia:", error)
      Alert.alert("Error", "Error de conexión al guardar la materia")
    } finally {
      setLoading(false)
    }
  }

  const handleModificarContenidos = () => {
    if (materia.id && materia.id > 0) {
      navigation.navigate("ListaContenidos", {
        materiaId: materia.id,
        viewOnly: isViewOnly,
      })
    } else {
      Alert.alert("Información", "Primero debes guardar la materia para gestionar sus contenidos")
    }
  }

  const toggleContentExpansion = (contenidoId: number) => {
    setExpandedContent((prev) => ({
      ...prev,
      [contenidoId]: !prev[contenidoId],
    }))
  }

  const handleVolver = () => {
    navigation.navigate("Materias")
  }

  const renderContenidos = () => {
    if (!contenidos || contenidos.length === 0) {
      return (
        <View style={styles.emptyContent}>
          <Text style={styles.emptyContentText}>No hay contenidos disponibles</Text>
          <Text style={styles.emptyContentSubtext}>
            {isViewOnly
              ? "Esta materia no tiene contenidos asociados"
              : 'Toca "Gestionar Contenidos" para agregar contenidos a esta materia'}
          </Text>
        </View>
      )
    }

    return (
      <View>
        {contenidos.map((contenido, index) => {
          const contenidoId = contenido.id || 0
          const titulo = contenido.titulo || "Sin título"
          const descripcion = contenido.descripcion || "Sin descripción"
          const numeroUnidad = index + 1
          const tituloCompleto = "Unidad " + numeroUnidad.toString() + ": " + titulo

          return (
            <View key={contenidoId} style={styles.contentItem}>
              <TouchableOpacity style={styles.contentToggle} onPress={() => toggleContentExpansion(contenidoId)}>
                <Text style={styles.contentItemTitle}>{tituloCompleto}</Text>
                <Icon name={expandedContent[contenidoId] ? "expand-less" : "expand-more"} size={20} color="#fff" />
              </TouchableOpacity>

              {expandedContent[contenidoId] ? (
                <View style={styles.contentDescription}>
                  <Text style={styles.contentDescriptionText}>{descripcion}</Text>
                </View>
              ) : null}
            </View>
          )
        })}
      </View>
    )
  }

  if (loading) {
    return (
      <AppLayout title={screenTitle}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4A90E2" />
          <Text style={styles.loadingText}>{isEditing ? "Cargando materia..." : "Guardando materia..."}</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title={screenTitle}>
      <ScrollView style={styles.container} contentContainerStyle={styles.contentContainer}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>{screenTitle}</Text>
        </View>
        <View style={styles.form}>
          <View style={styles.inputGroup}>
            <Text style={styles.label}>Nombre de la materia {!isViewOnly && "*"}</Text>
            <TextInput
              style={[styles.input, isViewOnly && styles.inputDisabled]}
              value={materia.nombre || ""}
              onChangeText={(text) => !isViewOnly && setMateria((prev) => ({ ...prev, nombre: text }))}
              placeholder="Ingresa el nombre de la materia"
              maxLength={100}
              editable={!isViewOnly}
            />
          </View>

          {!isViewOnly && (
            <View style={styles.buttonContainer}>
              <TouchableOpacity style={[styles.button, styles.saveButton]} onPress={handleSave} disabled={loading}>
                <Icon name="save" size={18} color="#FFFFFF" />
                <Text style={styles.buttonText}>Guardar</Text>
              </TouchableOpacity>

              <TouchableOpacity style={[styles.button, styles.cancelButton]} onPress={handleVolver}>
                <Icon name="cancel" size={18} color="#FFFFFF" />
                <Text style={[styles.buttonText, { color: "#FFFFFF" }]}>Cancelar</Text>
              </TouchableOpacity>
            </View>
          )}
        </View>

        {isEditing && materia.id && materia.id > 0 ? (
          <View style={styles.contentSection}>
            <View style={styles.contentHeader}>
              <Text style={styles.contentTitle}>Contenidos</Text>
              {!isViewOnly && (
                <TouchableOpacity style={styles.modifyContentButton} onPress={handleModificarContenidos}>
                  <Icon name="add" size={20} color="#4A90E2" />
                  <Text style={styles.modifyContentText}>Gestionar Contenidos</Text>
                </TouchableOpacity>
              )}
            </View>
            {renderContenidos()}
          </View>
        ) : null}
      </ScrollView>

      <View style={styles.buttonsContainer}>
        <TouchableOpacity style={styles.backButton} onPress={handleVolver}>
          <Icon name="arrow-back" size={16} color="#FFFFFF" />
          <Text style={styles.backButtonText}>Atrás</Text>
        </TouchableOpacity>
      </View>
    </AppLayout>
  )
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#f5f5f5",
  },
  contentContainer: {
    paddingBottom: 20,
  },
  form: {
    backgroundColor: "#fff",
    margin: 16,
    padding: 20,
    borderRadius: 12,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  inputGroup: {
    marginBottom: 20,
  },
  label: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    marginBottom: 8,
  },
  input: {
    borderWidth: 1,
    borderColor: "#ddd",
    borderRadius: 8,
    padding: 12,
    fontSize: 16,
    backgroundColor: "#fff",
  },
  inputDisabled: {
    backgroundColor: "#f5f5f5",
    color: "#666",
  },
  buttonContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginTop: 20,
  },
  button: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 20,
    paddingVertical: 12,
    borderRadius: 8,
    flex: 1,
    marginHorizontal: 8,
    justifyContent: "center",
  },
  saveButton: {
    backgroundColor: "#4A90E2",
  },
  cancelButton: {
    backgroundColor: "#F44336",
    borderWidth: 1,
    borderColor: "#ddd",
  },
  buttonText: {
    color: "#fff",
    fontWeight: "600",
  },
  contentSection: {
    backgroundColor: "#fff",
    margin: 16,
    marginTop: 0,
    padding: 20,
    borderRadius: 12,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  contentHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 16,
  },
  contentTitle: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#333",
  },
  modifyContentButton: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 6,
    backgroundColor: "#f0f8ff",
  },
  modifyContentText: {
    color: "#4A90E2",
    fontWeight: "600",
  },
  contentItem: {
    borderWidth: 1,
    borderColor: "#e0e0e0",
    borderRadius: 8,
    marginBottom: 12,
    overflow: "hidden",
  },
  contentToggle: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    padding: 16,
    backgroundColor: "#00a9ff",
  },
  contentItemTitle: {
    fontSize: 16,
    fontWeight: "600",
    color: "#fff",
    flex: 1,
  },
  contentDescription: {
    padding: 16,
    backgroundColor: "#fff",
    borderTopWidth: 1,
    borderTopColor: "#e0e0e0",
  },
  contentDescriptionText: {
    fontSize: 14,
    color: "#666",
    lineHeight: 20,
  },
  emptyContent: {
    alignItems: "center",
    paddingVertical: 40,
  },
  emptyContentText: {
    fontSize: 16,
    fontWeight: "600",
    color: "#666",
    marginTop: 16,
  },
  emptyContentSubtext: {
    fontSize: 14,
    color: "#999",
    marginTop: 8,
    textAlign: "center",
  },
  loadingContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "#f5f5f5",
  },
  loadingText: {
    marginTop: 16,
    fontSize: 16,
    color: "#666",
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingHorizontal: 20,
    paddingVertical: 15,
    backgroundColor: "#fff",
    borderTopWidth: 1,
    borderTopColor: "#e0e0e0",
  },
  backButton: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#6c757d",
    paddingHorizontal: 20,
    paddingVertical: 10,
    borderRadius: 5,
    flex: 1,
    justifyContent: "center",
  },
  backButtonText: {
    color: "#FFFFFF",
    fontWeight: "bold",
    marginLeft: 5,
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

export default CreateMateriaScreen
