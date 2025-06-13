import type React from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator } from "react-native"
import { useState, useEffect } from "react"
import { useNavigation, useRoute } from "@react-navigation/native"
import type { NativeStackNavigationProp, NativeStackScreenProps } from "@react-navigation/native-stack"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { eventoService } from "../services/eventoService"
import type { Evento } from "../models/evento"
import type { IntegrantesEventos } from "../models/integrantesEventos"
import type { RootStackParamList } from "../navigation/AppNavigator"

type ConfirmacionesEventoScreenProps = NativeStackScreenProps<RootStackParamList, "ConfirmacionesEvento">

const ConfirmacionesEventoScreen: React.FC = () => {
  const [evento, setEvento] = useState<Evento | null>(null)
  const [integrantes, setIntegrantes] = useState<IntegrantesEventos[]>([])
  const [loading, setLoading] = useState(true)

  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>()
  const route = useRoute<ConfirmacionesEventoScreenProps["route"]>()

  const eventoId = route.params?.eventoId

  useEffect(() => {
    if (eventoId) {
      loadData()
    }
  }, [eventoId])

  const loadData = async () => {
    try {
      setLoading(true)

      // Cargar evento
      const eventoData = await eventoService.getEventoById(eventoId!)
      setEvento(eventoData)

      // Cargar integrantes del evento
      const integrantesData = await eventoService.getIntegrantesEvento(eventoId!)
      setIntegrantes(integrantesData)
    } catch (error) {
    } finally {
      setLoading(false)
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

  const confirmados = integrantes.filter((i) => i.confirmado)
  const sinConfirmar = integrantes.filter((i) => !i.confirmado)

  const renderIntegrante = ({ item }: { item: IntegrantesEventos }) => (
    <View style={styles.integranteItem}>
      <View style={styles.integranteInfo}>
        <Text style={styles.integranteNombre}>
          {item.usuario?.nombre} {item.usuario?.apellido}
        </Text>
        <Text style={styles.integranteMail}>{item.usuario?.mail}</Text>
      </View>
      <View style={[styles.statusBadge, item.leido ? styles.statusLeido : styles.statusNoLeido]}>
        <Text style={[styles.statusBadgeText, item.leido ? styles.statusLeidoText : styles.statusNoLeidoText]}>
          {item.leido ? "Leído" : "No leído"}
        </Text>
      </View>
    </View>
  )

  if (loading) {
    return (
      <AppLayout title="Confirmaciones" showLoadingOverlay={true}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando confirmaciones...</Text>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout title="Confirmaciones">
      <View style={styles.container}>
        <View style={styles.content}>
          {evento && (
            <View style={styles.eventoInfo}>
              <Text style={styles.eventoNombre}>{evento.nombre}</Text>
              <Text style={styles.eventoFecha}>
                <Text style={styles.label}>Fecha: </Text>
                {formatDate(evento.fecha)}
              </Text>
            </View>
          )}

          {integrantes.length > 0 ? (
            <View style={styles.confirmacionesContainer}>
              {/* Confirmados */}
              <View style={[styles.section, { flex: confirmados.length > 0 ? confirmados.length : 0.5 }]}>
                <View style={[styles.sectionHeader, styles.confirmadosHeader]}>
                  <Icon name="check-circle" size={20} color="#FFFFFF" />
                  <Text style={styles.sectionHeaderText}>Confirmados ({confirmados.length})</Text>
                </View>
                {confirmados.length > 0 ? (
                  <FlatList
                    data={confirmados}
                    renderItem={renderIntegrante}
                    keyExtractor={(item) => item.id.toString()}
                    style={styles.list}
                    nestedScrollEnabled={true}
                  />
                ) : (
                  <View style={styles.emptySection}>
                    <Text style={styles.emptySectionText}>Aún no hay confirmaciones</Text>
                  </View>
                )}
              </View>

              {/* Sin confirmar */}
              <View style={[styles.section, { flex: sinConfirmar.length > 0 ? sinConfirmar.length : 0.5 }]}>
                <View style={[styles.sectionHeader, styles.sinConfirmarHeader]}>
                  <Icon name="schedule" size={20} color="#FFFFFF" />
                  <Text style={styles.sectionHeaderText}>Sin Confirmar ({sinConfirmar.length})</Text>
                </View>
                {sinConfirmar.length > 0 ? (
                  <FlatList
                    data={sinConfirmar}
                    renderItem={renderIntegrante}
                    keyExtractor={(item) => item.id.toString()}
                    style={styles.list}
                    nestedScrollEnabled={true}
                  />
                ) : (
                  <View style={styles.emptySection}>
                    <Text style={styles.emptySectionText}>Todos los usuarios han confirmado</Text>
                  </View>
                )}
              </View>
            </View>
          ) : (
            <View style={styles.emptyState}>
              <Icon name="people-outline" size={64} color="#CCCCCC" />
              <Text style={styles.emptyStateText}>Sin confirmaciones</Text>
            </View>
          )}
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
  content: {
    flex: 1,
  },
  eventoInfo: {
    backgroundColor: "#FFFFFF",
    padding: 16,
    borderBottomWidth: 1,
    borderBottomColor: "#E0E0E0",
  },
  eventoNombre: {
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
    marginBottom: 4,
  },
  eventoFecha: {
    fontSize: 14,
    color: "#666",
  },
  label: {
    fontWeight: "600",
  },
  confirmacionesContainer: {
    flex: 1,
    padding: 16,
    gap: 16,
  },
  section: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    overflow: "hidden",
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    minHeight: 100,
  },
  sectionHeader: {
    flexDirection: "row",
    alignItems: "center",
    padding: 12,
    gap: 8,
  },
  confirmadosHeader: {
    backgroundColor: "#4CAF50",
  },
  sinConfirmarHeader: {
    backgroundColor: "#FF9800",
  },
  sectionHeaderText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
  list: {
    flex: 1,
  },
  integranteItem: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    padding: 12,
    borderBottomWidth: 1,
    borderBottomColor: "#F0F0F0",
  },
  integranteInfo: {
    flex: 1,
  },
  integranteNombre: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
  },
  integranteMail: {
    fontSize: 14,
    color: "#666",
    marginTop: 2,
  },
  statusBadge: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  statusLeido: {
    backgroundColor: "#E8F5E8",
  },
  statusNoLeido: {
    backgroundColor: "#FFF3E0",
  },
  statusBadgeText: {
    fontSize: 12,
    fontWeight: "600",
  },
  statusLeidoText: {
    color: "#4CAF50",
  },
  statusNoLeidoText: {
    color: "#FF9800",
  },
  emptySection: {
    padding: 20,
    alignItems: "center",
  },
  emptySectionText: {
    fontSize: 14,
    color: "#666",
    fontStyle: "italic",
  },
  emptyState: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  emptyStateText: {
    marginTop: 16,
    fontSize: 16,
    color: "#666",
    textAlign: "center",
  },
  footer: {
    backgroundColor: "#FFFFFF",
    padding: 16,
    borderTopWidth: 1,
    borderTopColor: "#E0E0E0",
  },
  backButton: {
    backgroundColor: "#6C757D",
    paddingVertical: 12,
    paddingHorizontal: 24,
    borderRadius: 8,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
  },
  backButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
})

export default ConfirmacionesEventoScreen
