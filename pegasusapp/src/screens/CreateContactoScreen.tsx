import type React from "react"
import { useState, useEffect } from "react"
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Alert, ScrollView, ActivityIndicator } from "react-native"
import { Picker } from "@react-native-picker/picker"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RouteProp } from "@react-navigation/native"
import type { RootStackParamList } from "../navigation/AppNavigator"
import AppLayout from "../components/AppLayout"
import { CONFIG } from "../services/config"
import type { Contactos } from "../models/contactos"
import Icon from "react-native-vector-icons/MaterialIcons"


type NavigationProp = NativeStackNavigationProp<RootStackParamList, "CreateContacto">
type RouteProps = RouteProp<RootStackParamList, "CreateContacto">

const CreateContactoScreen: React.FC = () => {
  const navigation = useNavigation<NavigationProp>()
  const route = useRoute<RouteProps>()
  const { tipoContacto, contactoId } = route.params

  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [formData, setFormData] = useState({
    nombre: "",
    apellido: "",
    mail: "",
    telefono: "",
    tipoContactoSeleccionado: tipoContacto,
  })

  const isEditing = !!contactoId
  const title = isEditing ? "Editar Contacto" : "Nuevo Contacto"
  const showApellido = formData.tipoContactoSeleccionado === 2

  useEffect(() => {
    if (isEditing) {
      loadContacto()
    }
  }, [contactoId])

  const loadContacto = async () => {
    try {
      setLoading(true)
      const response = await fetch(`${CONFIG.API_BASE_URL}/Contactos/GetById?id=${contactoId}`)

      if (response.ok) {
        const contacto = await response.json()

        if (contacto) {
          // Dividir el nombre completo en nombre y apellido
          const nombreCompleto = contacto.nombre || ""
          const partes = nombreCompleto.split(" ")
          const nombre = partes[0] || ""
          const apellido = partes.length > 1 ? partes.slice(1).join(" ") : ""

          setFormData({
            nombre,
            apellido,
            mail: contacto.mail || "",
            telefono: contacto.telefono || "",
            tipoContactoSeleccionado: contacto.tipo_Contacto || tipoContacto,
          })
        }
      } else {
        Alert.alert("Error", "No se pudo cargar el contacto")
      }
    } catch (error) {
      Alert.alert("Error", "No se pudo cargar el contacto")
    } finally {
      setLoading(false)
    }
  }

  const validateForm = (): boolean => {
    if (!formData.nombre.trim()) {
      Alert.alert("Error", "El campo Nombre es requerido")
      return false
    }

    if (showApellido && !formData.apellido.trim()) {
      Alert.alert("Error", "El campo Apellido es requerido")
      return false
    }

    if (!formData.mail.trim()) {
      Alert.alert("Error", "El campo Mail es requerido")
      return false
    }

    if (!formData.telefono.trim()) {
      Alert.alert("Error", "El campo Teléfono es requerido")
      return false
    }

    if (!formData.tipoContactoSeleccionado) {
      Alert.alert("Error", "El campo Tipo de Contacto es requerido")
      return false
    }

    // Validar formato de email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(formData.mail)) {
      Alert.alert("Error", "Por favor ingresa un email válido")
      return false
    }

    return true
  }

  const handleSave = async () => {
    if (!validateForm()) return

    try {
      setSaving(true)

      const contactoData: Partial<Contactos> = {
        nombre: `${formData.nombre} ${formData.apellido}`.trim(),
        mail: formData.mail,
        telefono: formData.telefono,
        tipo_Contacto: formData.tipoContactoSeleccionado,
      }

      if (isEditing) {
        contactoData.id = contactoId
      }

      const url = isEditing
        ? `${CONFIG.API_BASE_URL}/Contactos/UpdateContacto`
        : `${CONFIG.API_BASE_URL}/Contactos/CreateContacto`

      const method = isEditing ? "PUT" : "POST"

      const response = await fetch(url, {
        method,
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(contactoData),
      })

      if (response.ok) {
        Alert.alert("Éxito", `Contacto ${isEditing ? "actualizado" : "creado"} correctamente`, [
          {
            text: "OK",
            onPress: () => navigation.navigate("Contactos", { tipoContacto: formData.tipoContactoSeleccionado }),
          },
        ])
      } else {
        const errorText = await response.text()
        Alert.alert("Error", `No se pudo ${isEditing ? "actualizar" : "crear"} el contacto: ${errorText}`)
      }
    } catch (error) {
      Alert.alert("Error", `Hubo un error al ${isEditing ? "actualizar" : "crear"} el contacto`)
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <AppLayout title={title} showLoadingOverlay={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#007AFF" />
          <Text style={styles.loadingText}>Cargando contacto...</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title={title}>
        <View style={styles.mainContainer}>
                <ScrollView style={styles.container} contentContainerStyle={styles.scrollContent}>
                  <View style={styles.header}>
                    <Text style={styles.headerTitle}>{title}</Text>
                  </View>
        
                  <View style={styles.formContainer}>
                    <View style={styles.formCard}>
                      <View style={styles.inputGroup}>
                        <Text style={styles.label}>Nombre *</Text>
                        <TextInput
                          style={styles.input}
                          value={formData.nombre}
                          onChangeText={(text) => setFormData({ ...formData, nombre: text })}
                          placeholder="Ingresa el nombre"
                          autoCapitalize="words"
                        />
                      </View>

                      {showApellido && (
                        <View style={styles.inputGroup}>
                        <Text style={styles.label}>Apellido *</Text>
                        <TextInput
                          style={styles.input}
                          value={formData.apellido}
                          onChangeText={(text) => setFormData({ ...formData, apellido: text })}
                          placeholder="Ingresa el apellido"
                          autoCapitalize="words"
                        />
                      </View>
                        )}

                      <View style={styles.inputGroup}>
                        <Text style={styles.label}>Email *</Text>
                        <TextInput
                          style={styles.input}
                          value={formData.mail}
                          onChangeText={(text) => setFormData({ ...formData, mail: text })}
                          placeholder="Ingresa el email"
                          keyboardType="email-address"
                          autoCapitalize="none"
                        />
                      </View>

                      <View style={styles.inputGroup}>
                        <Text style={styles.label}>Teléfono *</Text>
                        <TextInput
                          style={styles.input}
                          value={formData.telefono}
                          onChangeText={(text) => setFormData({ ...formData, telefono: text })}
                          placeholder="Ingresa el teléfono"
                          keyboardType="phone-pad"
                        />
                      </View>
        
                      <View style={styles.inputGroup}>
                        <Text style={styles.label}>Tipo de Contacto *</Text>
                        <View style={styles.pickerContainer}>
                          <Picker
                            selectedValue={formData.tipoContactoSeleccionado}
                            onValueChange={(value) => setFormData({ ...formData, tipoContactoSeleccionado: value })}
                            style={styles.picker}
                          >
                            <Picker.Item label="-- Seleccionar un Tipo de Contacto --" value={0} />
                            <Picker.Item label="Institucional" value={1} />
                            <Picker.Item label="Docente" value={2} />
                          </Picker>
                        </View>
                       </View>
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
    flex: 1,
    padding: 15,
    backgroundColor: "#f8f9fa",
  },
  formGroup: {
    marginBottom: 15,
  },
  label: {
    fontSize: 16,
    fontWeight: "500",
    marginBottom: 5,
    color: "#333",
  },
  input: {
    backgroundColor: "#fff",
    borderWidth: 1,
    borderColor: "#ced4da",
    borderRadius: 4,
    padding: 10,
    fontSize: 16,
  },
  pickerContainer: {
    backgroundColor: "#fff",
    borderWidth: 1,
    borderColor: "#ced4da",
    borderRadius: 4,
  },
  picker: {
    height: 50,
  },
  actionButtonsContainer: {
    flexDirection: "row",
    justifyContent: "space-around",
    padding: 15,
    backgroundColor: "#fff",
    borderTopWidth: 1,
    borderTopColor: "#e9ecef",
  },
  actionButton: {
    paddingVertical: 10,
    paddingHorizontal: 20,
    backgroundColor: "#007bff",
    borderRadius: 5,
    minWidth: 100,
    alignItems: "center",
  },
  actionButtonText: {
    color: "#fff",
    fontWeight: "bold",
  },
  disabledButton: {
    opacity: 0.6,
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
  mainContainer: {
    flex: 1,
  },
  container: {
    flex: 1,
    backgroundColor: "#F5F5F5",
  },
  scrollContent: {
    paddingBottom: 80,
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
  saveButton: {
    flex: 1,
    backgroundColor: "#4285F4",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 12,
    borderRadius: 8,
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
})

export default CreateContactoScreen
