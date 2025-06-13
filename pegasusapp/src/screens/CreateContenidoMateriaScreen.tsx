import { useState, useEffect } from "react"
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Alert, ActivityIndicator, ScrollView } from "react-native"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RouteProp } from "@react-navigation/native"
import type { RootStackParamList } from "../navigation/AppNavigator"
import type { ContenidoMaterias } from "../models/contenidoMaterias"
import { CONFIG } from "../services/config"
import AppLayout from "../components/AppLayout"
import Icon from "react-native-vector-icons/MaterialIcons"
import { useUser } from "../context/UserContext"

type CreateContenidoMateriaScreenNavigationProp = NativeStackNavigationProp<
  RootStackParamList,
  "CreateContenidoMateria"
>
type CreateContenidoMateriaScreenRouteProp = RouteProp<RootStackParamList, "CreateContenidoMateria">

const CreateContenidoMateriaScreen = () => {
  const navigation = useNavigation<CreateContenidoMateriaScreenNavigationProp>()
  const route = useRoute<CreateContenidoMateriaScreenRouteProp>()
  const { materiaId, contenidoId, viewOnly } = route.params
  const { userData } = useUser()

  const [contenido, setContenido] = useState<ContenidoMaterias>({
    id: 0,
    titulo: "",
    descripcion: "",
    id_Materia: materiaId,
  })
  const [loading, setLoading] = useState(false)

  // Determinar permisos según el perfil del usuario
  const userProfile = userData?.id_perfil || 0
  const isViewOnly = viewOnly || userProfile === 2 || userProfile === 3 || userProfile === 4
  const isEditing = Boolean(contenidoId && contenidoId > 0)
  const screenTitle = isEditing ? (isViewOnly ? "Ver Contenido" : "Editar Contenido") : "Nuevo Contenido"

  useEffect(() => {
    if (isEditing) {
      fetchContenido()
    }
  }, [contenidoId])

  const fetchContenido = async () => {
    if (!contenidoId) return

    try {
      setLoading(true)
      const response = await fetch(`${CONFIG.API_BASE_URL}/ContenidoMaterias/GetById?id=${contenidoId}`)

      if (response.ok) {
        const data = await response.json()
        setContenido(data || { id: 0, titulo: "", descripcion: "", id_materia: materiaId })
      } else {
        Alert.alert("Error", "No se pudo cargar el contenido")
        navigation.navigate("ListaContenidos", { materiaId, viewOnly: isViewOnly })
      }
    } catch (error) {
      Alert.alert("Error", "Error de conexión al cargar el contenido")
      navigation.navigate("ListaContenidos", { materiaId, viewOnly: isViewOnly })
    } finally {
      setLoading(false)
    }
  }

  const handleSave = async () => {
    const titulo = contenido.titulo ? contenido.titulo.trim() : ""
    const descripcion = contenido.descripcion ? contenido.descripcion.trim() : ""

    if (!titulo) {
      Alert.alert("Error", "El título del contenido es requerido")
      return
    }

    if (!descripcion) {
      Alert.alert("Error", "La descripción del contenido es requerida")
      return
    }

    try {
      setLoading(true)

      const contenidoData = {
        Titulo: titulo,
        Descripcion: descripcion,
        Id_materia: materiaId,
        ...(isEditing && { Id: contenido.id }),
      }

      const response = await fetch(
        `${CONFIG.API_BASE_URL}/ContenidoMaterias/${isEditing ? "UpdateContenidoMaterias" : "CreateContenidoMaterias"}`,
        {
          method: isEditing ? "PUT" : "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(contenidoData),
        },
      )

      if (response.ok) {
        const successMessage = isEditing ? "actualizado" : "creado"
        Alert.alert("Éxito", `Contenido ${successMessage} correctamente`, [
          { text: "OK", onPress: () => navigation.navigate("ListaContenidos", { materiaId, viewOnly: isViewOnly }) },
        ])
      } else {
        const errorText = await response.text()
        const actionText = isEditing ? "actualizar" : "crear"
        Alert.alert("Error", `No se pudo ${actionText} el contenido: ${errorText}`)
      }
    } catch (error) {
      Alert.alert("Error", "Error de conexión al guardar el contenido")
    } finally {
      setLoading(false)
    }
  }

  const handleVolver = () => {
    navigation.navigate("ListaContenidos", { materiaId, viewOnly: isViewOnly })
  }

  if (loading) {
    return (
      <AppLayout title={screenTitle} showHomeButton={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4A90E2" />
          <Text style={styles.loadingText}>{isEditing ? "Cargando contenido..." : "Guardando contenido..."}</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title={screenTitle} showHomeButton={true}>
      <ScrollView style={styles.container} contentContainerStyle={styles.contentContainer}>
        <View style={styles.header}>
            <Text style={styles.headerTitle}>{screenTitle}</Text>
        </View>
        <View style={styles.form}>
          <View style={styles.inputGroup}>
            <Text style={styles.label}>Título del contenido {!isViewOnly && "*"}</Text>
            <TextInput
              style={[styles.input, isViewOnly && styles.inputDisabled]}
              value={contenido.titulo || ""}
              onChangeText={(text) => !isViewOnly && setContenido((prev) => ({ ...prev, titulo: text }))}
              placeholder="Ingresa el título del contenido"
              maxLength={200}
              editable={!isViewOnly}
            />
          </View>

          <View style={styles.inputGroup}>
            <Text style={styles.label}>Descripción {!isViewOnly && "*"}</Text>
            <TextInput
              style={[styles.input, styles.textArea, isViewOnly && styles.inputDisabled]}
              value={contenido.descripcion || ""}
              onChangeText={(text) => !isViewOnly && setContenido((prev) => ({ ...prev, descripcion: text }))}
              placeholder="Ingresa la descripción del contenido"
              multiline={true}
              numberOfLines={6}
              maxLength={1000}
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
    fontSize: 14,
    backgroundColor: "#fff",
  },
  inputDisabled: {
    backgroundColor: "#f5f5f5",
    color: "#666",
  },
  textArea: {
    height: 100,
    textAlignVertical: "top",
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

export default CreateContenidoMateriaScreen
