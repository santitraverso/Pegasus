import type React from "react"
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  ScrollView,
  ActivityIndicator,
  Alert,
  Switch,
} from "react-native"
import { useState, useEffect } from "react"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp, NativeStackScreenProps } from "@react-navigation/native-stack"
import Icon from "react-native-vector-icons/MaterialIcons"
import { Picker } from "@react-native-picker/picker"
import DateTimePicker from "@react-native-community/datetimepicker"
import AppLayout from "../components/AppLayout"
import { eventoService } from "../services/eventoService"
import type { Evento, TipoDestinatario } from "../models/evento"
import type { RootStackParamList } from "../navigation/AppNavigator"

type CreateEventoScreenProps = NativeStackScreenProps<RootStackParamList, "CreateEvento">

const CreateEventoScreen: React.FC = () => {
  const [nombre, setNombre] = useState("")
  const [descripcion, setDescripcion] = useState("")
  const [fecha, setFecha] = useState(new Date())
  const [requiereConfirmacion, setRequiereConfirmacion] = useState(false)
  const [tipoDestinatario, setTipoDestinatario] = useState<TipoDestinatario>(3) // Ambos por defecto
  const [showDatePicker, setShowDatePicker] = useState(false)
  const [loading, setLoading] = useState(false)
  const [loadingData, setLoadingData] = useState(false)

  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>()
  const route = useRoute<CreateEventoScreenProps["route"]>()

  const eventoId = route.params?.eventoId
  const isEditing = !!eventoId

  useEffect(() => {
    if (isEditing && eventoId) {
      loadEvento()
    }
  }, [eventoId])

  const loadEvento = async () => {
    try {
      setLoadingData(true)
      const evento = await eventoService.getEventoById(eventoId!)

      setNombre(evento.nombre || "")
      setDescripcion(evento.descripcion || "")
      setFecha(evento.fecha ? new Date(evento.fecha) : new Date())
      setRequiereConfirmacion(evento.requiereConfirmacion || false)
      setTipoDestinatario(evento.tipoDestinatario || 3)
    } catch (error) {
      Alert.alert("Error", "No se pudo cargar el evento")
      navigation.goBack()
    } finally {
      setLoadingData(false)
    }
  }

  const validateForm = () => {
    if (!nombre.trim()) {
      Alert.alert("Error", "El nombre del evento es requerido")
      return false
    }
    if (!descripcion.trim()) {
      Alert.alert("Error", "La descripción del evento es requerida")
      return false
    }
    if (descripcion.length > 200) {
      Alert.alert("Error", "La descripción no puede exceder los 200 caracteres")
      return false
    }
    return true
  }

  const handleSave = async () => {
    if (!validateForm()) return

    try {
      setLoading(true)

      const eventoData: Partial<Evento> = {
        nombre: nombre.trim(),
        descripcion: descripcion.trim(),
        fecha: fecha.toISOString(),
        requiereConfirmacion,
        tipoDestinatario,
      }

      if (isEditing) {
        eventoData.id = eventoId
        await eventoService.updateEvento(eventoData as Evento)
      } else {
        await eventoService.createEvento(eventoData as Omit<Evento, "id">)
      }

      Alert.alert("Éxito", "El evento se guardó correctamente", [{ text: "OK", onPress: () => navigation.goBack() }])
    } catch (error) {
      Alert.alert("Error", "No se pudo guardar el evento")
    } finally {
      setLoading(false)
    }
  }

  const onDateChange = (event: any, selectedDate?: Date) => {
    setShowDatePicker(false)
    if (selectedDate) {
      setFecha(selectedDate)
    }
  }

  const formatDate = (date: Date) => {
    return date.toLocaleDateString("es-ES", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    })
  }

  const tipoDestinatarioOptions = [
    { label: "Solo Alumnos", value: 1 },
    { label: "Solo Padres", value: 2 },
    { label: "Alumnos y Padres", value: 3 },
  ]

  if (loadingData) {
    return (
      <AppLayout title={isEditing ? "Editar Evento" : "Crear Evento"} showLoadingOverlay={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando evento...</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title={isEditing ? "Editar Evento" : "Crear Evento"}>
      <View style={styles.container}>
        {/* Buscador */}

        <ScrollView style={styles.content} showsVerticalScrollIndicator={false}>
          <View style={styles.form}>
            {/* Nombre del Evento */}
            <View style={styles.inputGroup}>
              <Text style={styles.label}>Nombre del Evento</Text>
              <TextInput
                style={styles.input}
                placeholder="Ingrese el nombre del evento"
                value={nombre}
                onChangeText={setNombre}
                maxLength={100}
              />
            </View>

            {/* Fecha del Evento */}
            <View style={styles.inputGroup}>
              <Text style={styles.label}>Fecha del Evento</Text>
              <TouchableOpacity style={styles.dateInput} onPress={() => setShowDatePicker(true)}>
                <Text style={styles.dateText}>{formatDate(fecha)}</Text>
                <Icon name="calendar-today" size={20} color="#666" />
              </TouchableOpacity>
            </View>

            {/* Requiere Confirmación */}
            <View style={styles.inputGroup}>
              <View style={styles.switchContainer}>
                <Text style={styles.label}>Requiere Confirmación</Text>
                <Switch
                  value={requiereConfirmacion}
                  onValueChange={setRequiereConfirmacion}
                  trackColor={{ false: "#E0E0E0", true: "#4285F4" }}
                  thumbColor={requiereConfirmacion ? "#FFFFFF" : "#FFFFFF"}
                />
              </View>
            </View>

            {/* Destinatarios */}
            <View style={styles.inputGroup}>
              <Text style={styles.label}>Destinatarios</Text>
              <View style={styles.pickerContainer}>
                <Picker
                  selectedValue={tipoDestinatario}
                  onValueChange={(itemValue) => setTipoDestinatario(itemValue)}
                  style={styles.picker}
                >
                  {tipoDestinatarioOptions.map((option) => (
                    <Picker.Item key={option.value} label={option.label} value={option.value} />
                  ))}
                </Picker>
              </View>
            </View>

            {/* Descripción */}
            <View style={styles.inputGroup}>
              <Text style={styles.label}>Descripción</Text>
              <TextInput
                style={[styles.input, styles.textArea]}
                placeholder="Ingrese la descripción del evento (máximo 200 caracteres)"
                value={descripcion}
                onChangeText={setDescripcion}
                multiline
                numberOfLines={4}
                maxLength={200}
                textAlignVertical="top"
              />
              <Text style={styles.characterCount}>{descripcion.length}/200</Text>
            </View>
          </View>
        </ScrollView>

        {/* Footer con botones */}
        <View style={styles.footerContainer}>
            <View style={styles.buttonsContainer}>
                <TouchableOpacity
                 style={[styles.saveButton, loading && styles.saveButtonDisabled]}
                 onPress={handleSave}
                 disabled={loading}
                >
                {loading ? (
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

        {/* DateTimePicker */}
        {showDatePicker && (
          <DateTimePicker
            value={fecha}
            mode="date"
            display="default"
            onChange={onDateChange}
            minimumDate={new Date()}
          />
        )}
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
  searchContainer: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FFFFFF",
    margin: 16,
    paddingHorizontal: 12,
    borderRadius: 8,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
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
  content: {
    flex: 1,
  },
  form: {
    padding: 16,
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
    backgroundColor: "#FFFFFF",
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 12,
    fontSize: 16,
    color: "#333",
  },
  textArea: {
    height: 100,
    textAlignVertical: "top",
  },
  characterCount: {
    fontSize: 12,
    color: "#666",
    textAlign: "right",
    marginTop: 4,
  },
  dateInput: {
    backgroundColor: "#FFFFFF",
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 12,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  dateText: {
    fontSize: 16,
    color: "#333",
  },
  switchContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  pickerContainer: {
    backgroundColor: "#FFFFFF",
    borderWidth: 1,
    borderColor: "#E0E0E0",
    borderRadius: 8,
    overflow: "hidden",
  },
  picker: {
    height: 50,
  },
  footerContainer: {
    backgroundColor: "#FFFFFF",
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
    paddingBottom: 20,
  },
  buttonsContainer: {
    flexDirection: "row",
    justifyContent: "space-between",
    padding: 16,
    gap: 12,
  },
  backButton: {
    flex: 1,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#6C757D",
    paddingVertical: 12,
    borderRadius: 8,
    gap: 8,
  },
  backButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
  saveButton: {
    flex: 1,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#4285F4",
    paddingVertical: 12,
    borderRadius: 8,
    gap: 8,
  },
  saveButtonDisabled: {
    backgroundColor: "#CCCCCC",
  },
  saveButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
})

export default CreateEventoScreen