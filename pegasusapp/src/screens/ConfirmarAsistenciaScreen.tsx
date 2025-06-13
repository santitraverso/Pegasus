import type React from "react"
import { View, Text, StyleSheet, TouchableOpacity, ActivityIndicator, Alert } from "react-native"
import { useState, useEffect } from "react"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp, NativeStackScreenProps } from "@react-navigation/native-stack"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { eventoService } from "../services/eventoService"
import { useUser } from "../context/UserContext"
import type { Evento } from "../models/evento"
import type { IntegrantesEventos } from "../models/integrantesEventos"
import type { RootStackParamList } from "../navigation/AppNavigator"

type ConfirmarAsistenciaScreenProps = NativeStackScreenProps<RootStackParamList, "ConfirmarAsistencia">

const ConfirmarAsistenciaScreen: React.FC = () => {
  const [evento, setEvento] = useState<Evento | null>(null)
  const [integrante, setIntegrante] = useState<IntegrantesEventos | null>(null)
  const [loading, setLoading] = useState(true)
  const [updating, setUpdating] = useState(false)

  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>()
  const route = useRoute<ConfirmarAsistenciaScreenProps["route"]>()
  const { userData } = useUser()

  const eventoId = route.params?.eventoId

  useEffect(() => {
    if (eventoId && userData?.id) {
      loadData()
    }
  }, [eventoId, userData?.id])

  const loadData = async () => {
    try {
      setLoading(true)

      // Cargar evento
      const eventoData = await eventoService.getEventoById(eventoId!)
      setEvento(eventoData)

      // Cargar integrante del evento
      const integranteData = await eventoService.getIntegranteEvento(eventoId!, userData!.id!)
      setIntegrante(integranteData)

      // Marcar como leído si existe el integrante
      if (integranteData && !integranteData.leido) {
        await eventoService.marcarComoLeido(integranteData.id, eventoId!, userData!.id!)
        // Actualizar el estado local
        setIntegrante((prev) => (prev ? { ...prev, leido: true } : null))
      }
    } catch (error) {
      Alert.alert("Error", "No se pudieron cargar los datos del evento")
      navigation.goBack()
    } finally {
      setLoading(false)
    }
  }

  const handleConfirmarAsistencia = async (confirmar: boolean) => {
    if (!integrante || !evento || !userData?.id) return

    try {
      setUpdating(true)

      await eventoService.actualizarConfirmacion(integrante.id, evento.id!, userData.id, confirmar)

      const mensaje = confirmar ? "Asistencia confirmada correctamente" : "Se ha registrado que no asistirás"

      Alert.alert("Éxito", mensaje, [{ text: "OK", onPress: () => navigation.goBack() }])
    } catch (error) {
      Alert.alert("Error", "No se pudo actualizar la confirmación")
    } finally {
      setUpdating(false)
    }
  }

  const formatDate = (dateString: string) => {
    const date = new Date(dateString)
    return date.toLocaleDateString("es-ES", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    })
  }

  if (loading) {
    return (
      <AppLayout title="Confirmar Asistencia" showLoadingOverlay={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando evento...</Text>
        </View>
      </AppLayout>
    )
  }

  if (!evento || !integrante) {
    return (
      <AppLayout title="Confirmar Asistencia">
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>No se pudo cargar el evento</Text>
        </View>
        <View style={styles.footer}>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <Text style={styles.backButtonText}>Volver</Text>
          </TouchableOpacity>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title="Confirmar Asistencia">
      <View style={styles.container}>
         <View style={styles.header}>
            <Text style={styles.headerTitle}>Asistencia</Text>
        </View>
        <View style={styles.content}>
          <View style={styles.eventoCard}>
            <View style={styles.eventoInfo}>
              <Text style={styles.eventoFecha}>{formatDate(evento.fecha)}</Text>
              <Text style={styles.eventoNombre}>{evento.nombre}</Text>
              <Text style={styles.eventoDescripcion}>{evento.descripcion}</Text>

              <View style={styles.statusContainer}>
                <Text
                  style={[styles.statusText, integrante.confirmado ? styles.statusConfirmado : styles.statusPendiente]}
                >
                  {integrante.confirmado ? "Ya has confirmado tu asistencia" : "No has confirmado tu asistencia"}
                </Text>
              </View>
            </View>

            <View style={styles.actionsContainer}>
              <TouchableOpacity
                style={[styles.actionButton, styles.confirmButton]}
                onPress={() => handleConfirmarAsistencia(true)}
                disabled={updating}
              >
                {updating ? (
                  <ActivityIndicator size="small" color="#FFFFFF" />
                ) : (
                  <>
                    <Icon name="check-circle" size={24} color="#FFFFFF" />
                    <Text style={styles.actionButtonText}>Confirmar Asistencia</Text>
                  </>
                )}
              </TouchableOpacity>

              <TouchableOpacity
                style={[styles.actionButton, styles.declineButton]}
                onPress={() => handleConfirmarAsistencia(false)}
                disabled={updating}
              >
                {updating ? (
                  <ActivityIndicator size="small" color="#FFFFFF" />
                ) : (
                  <>
                    <Icon name="cancel" size={24} color="#FFFFFF" />
                    <Text style={styles.actionButtonText}>No Asistir</Text>
                  </>
                )}
              </TouchableOpacity>
            </View>
          </View>
        </View>

        <View style={styles.footer}>
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
    textAlign: "center",
  },
  content: {
    flex: 1,
    padding: 16,
    justifyContent: "center",
  },
  eventoCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 12,
    padding: 24,
    elevation: 4,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 8,
    maxWidth: 400,
    alignSelf: "center",
    width: "100%",
  },
  eventoInfo: {
    alignItems: "center",
    marginBottom: 32,
  },
  eventoFecha: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#4285F4",
    marginBottom: 8,
  },
  eventoNombre: {
    fontSize: 20,
    fontWeight: "600",
    color: "#333",
    textAlign: "center",
    marginBottom: 12,
  },
  eventoDescripcion: {
    fontSize: 16,
    color: "#666",
    textAlign: "center",
    lineHeight: 22,
    marginBottom: 16,
  },
  statusContainer: {
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 20,
    backgroundColor: "#F5F5F5",
  },
  statusText: {
    fontSize: 14,
    fontWeight: "600",
    textAlign: "center",
  },
  statusConfirmado: {
    color: "#4CAF50",
  },
  statusPendiente: {
    color: "#FF9800",
  },
  actionsContainer: {
    gap: 12,
  },
  actionButton: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    padding: 16,
    borderRadius: 8,
    gap: 8,
  },
  confirmButton: {
    backgroundColor: "#4CAF50",
  },
  declineButton: {
    backgroundColor: "#F44336",
  },
  actionButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
  footer: {
    backgroundColor: "#FFFFFF",
    padding: 16,
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  backButton: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#6C757D",
    paddingVertical: 12,
    paddingHorizontal: 24,
    borderRadius: 8,
    gap: 8,
  },
  backButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
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

export default ConfirmarAsistenciaScreen
