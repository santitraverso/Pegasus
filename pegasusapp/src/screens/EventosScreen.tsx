import type React from "react"
import { View, Text, StyleSheet, FlatList, TouchableOpacity, ActivityIndicator, Alert, TextInput } from "react-native"
import { useState, useCallback } from "react"
import { useFocusEffect, useNavigation } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import Icon from "react-native-vector-icons/MaterialIcons"
import AppLayout from "../components/AppLayout"
import { eventoService } from "../services/eventoService"
import { useUser } from "../context/UserContext"
import type { Evento } from "../models/evento"
import type { RootStackParamList } from "../navigation/AppNavigator"

const EventosScreen: React.FC = () => {
  const [eventos, setEventos] = useState<Evento[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const { userData } = useUser()
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>()
  const [searchText, setSearchText] = useState("")
  const hoy = new Date();
  hoy.setHours(0, 0, 0, 0);

  const loadEventos = async () => {
    try {
      setLoading(true)
      setError(null)

      if (userData?.id_perfil) {
        const eventosData = await eventoService.getEventos(userData.id_perfil)
        // Ordenar por fecha descendente
        const eventosOrdenados = eventosData.filter(evento => {
                                  const fechaEvento = new Date(evento.fecha);
                                  fechaEvento.setHours(0, 0, 0, 0);
                                  return fechaEvento >= hoy;
                                }).sort((a, b) => new Date(b.fecha).getTime() - new Date(a.fecha).getTime())
        setEventos(eventosOrdenados)
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error desconocido")
    } finally {
      setLoading(false)
    }
  }

  useFocusEffect(
    useCallback(() => {
      loadEventos()
    }, [userData?.id_perfil]),
  )

  const handleCreateEvento = () => {
    navigation.navigate("CreateEvento", {})
  }

  const handleEditEvento = (evento: Evento) => {
    if (evento.id) {
      navigation.navigate("CreateEvento", { eventoId: evento.id })
    }
  }

  const handleDeleteEvento = (evento: Evento) => {
    Alert.alert("¿Estás seguro?", "¿Deseas eliminar este evento?", [
      { text: "Cancelar", style: "cancel" },
      {
        text: "Eliminar",
        style: "destructive",
        onPress: async () => {
          try {
            if (evento.id) {
              await eventoService.deleteEvento(evento.id)
              await loadEventos()
              Alert.alert("Éxito", "El evento se eliminó correctamente")
            }
          } catch (err) {
            Alert.alert("Error", "No se pudo eliminar el evento")
          }
        },
      },
    ])
  }

  const handleConfirmarAsistencia = (evento: Evento) => {
    if (evento.id) {
      navigation.navigate("ConfirmarAsistencia", { eventoId: evento.id })
    }
  }

  const handleVerConfirmaciones = (evento: Evento) => {
    if (evento.id) {
      navigation.navigate("ConfirmacionesEvento", { eventoId: evento.id })
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

  const puedeEditarEventos = () => {
    return userData?.id_perfil !== 2 && userData?.id_perfil !== 3 && userData?.id_perfil !== 4
  }

  const puedeConfirmAsistencia = () => {
    return userData?.id_perfil === 2 || userData?.id_perfil === 4
  }

  const filteredEventos = eventos.filter((evento) => {
    if (!searchText) return true

    const searchLower = searchText.toLowerCase()
    return evento.nombre?.toLowerCase().includes(searchLower) || evento.descripcion?.toLowerCase().includes(searchLower)
  })

  const renderEventoItem = ({ item }: { item: Evento }) => (
    <View style={styles.eventoCard}>
      <View style={styles.eventoHeader}>
        <Text style={styles.eventoFecha}>{formatDate(item.fecha)}</Text>
        <Text style={styles.eventoNombre}>{item.nombre}</Text>
      </View>

      <Text style={styles.eventoDescripcion}>{item.descripcion}</Text>

      {item.requiereConfirmacion && <Text style={styles.requiereConfirmacion}>Requiere Confirmación</Text>}

      <View style={styles.eventoActions}>
        {puedeEditarEventos() && (
          <>
            <TouchableOpacity style={[styles.actionButton, styles.editButton]} onPress={() => handleEditEvento(item)}>
              <Icon name="edit" size={20} color="#4285F4" />
            </TouchableOpacity>

            <TouchableOpacity
              style={[styles.actionButton, styles.deleteButton]}
              onPress={() => handleDeleteEvento(item)}
            >
              <Icon name="delete-outline" size={20} color="#F44336" />
            </TouchableOpacity>

            {item.requiereConfirmacion && (
              <TouchableOpacity
                style={[styles.actionButton, styles.confirmationsButton]}
                onPress={() => handleVerConfirmaciones(item)}
              >
                <Icon name="check-circle" size={20} color="#4CAF50" />
              </TouchableOpacity>
            )}
          </>
        )}

        {puedeConfirmAsistencia() && item.requiereConfirmacion && (
          <TouchableOpacity
            style={[styles.actionButton, styles.attendanceButton]}
            onPress={() => handleConfirmarAsistencia(item)}
          >
            <Icon name="event-available" size={20} color="#FF9800" />
          </TouchableOpacity>
        )}
      </View>
    </View>
  )

  if (loading) {
    return (
      <AppLayout>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#4285F4" />
          <Text style={styles.loadingText}>Cargando eventos...</Text>
        </View>
      </AppLayout>
    )
  }

  if (error) {
    return (
      <AppLayout>
        <View style={styles.errorContainer}>
          <Icon name="error-outline" size={48} color="#F44336" />
          <Text style={styles.errorText}>Error al cargar eventos</Text>
          <Text style={styles.errorMessage}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={loadEventos}>
            <Text style={styles.retryButtonText}>Reintentar</Text>
          </TouchableOpacity>
        </View>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Eventos</Text>
        </View>

        {/* Buscador */}
        <View style={styles.searchContainer}>
          <Icon name="search" size={20} color="#666" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="Buscar eventos..."
            value={searchText}
            onChangeText={setSearchText}
          />
        </View>

        {puedeEditarEventos() && (
          <View style={styles.createButtonContainer}>
            <TouchableOpacity style={styles.createEventButton} onPress={handleCreateEvento}>
              <Text style={styles.createEventButtonText}>Crear Evento</Text>
            </TouchableOpacity>
          </View>
        )}

        {filteredEventos.length > 0 ? (
          <FlatList
            data={filteredEventos}
            renderItem={renderEventoItem}
            keyExtractor={(item) => item.id?.toString() || "0"}
            contentContainerStyle={styles.eventosList}
            showsVerticalScrollIndicator={false}
          />
        ) : searchText ? (
          <View style={styles.emptyState}>
            <Icon name="search-off" size={64} color="#CCCCCC" />
            <Text style={styles.emptyStateText}>No se encontraron eventos que coincidan con "{searchText}"</Text>
          </View>
        ) : (
          <View style={styles.emptyState}>
            <Icon name="event" size={64} color="#CCCCCC" />
            <Text style={styles.emptyStateText}>No hay eventos disponibles</Text>
          </View>
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
    lineHeight: 20,
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
  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
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
  eventosList: {
    padding: 16,
  },
  eventoCard: {
    backgroundColor: "#FFFFFF",
    borderRadius: 8,
    padding: 16,
    marginBottom: 12,
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  eventoHeader: {
    marginBottom: 8,
  },
  eventoFecha: {
    fontSize: 16,
    fontWeight: "bold",
    color: "#4285F4",
    marginBottom: 4,
  },
  eventoNombre: {
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
  },
  eventoDescripcion: {
    fontSize: 14,
    color: "#666",
    lineHeight: 20,
    marginBottom: 8,
  },
  requiereConfirmacion: {
    fontSize: 14,
    fontWeight: "600",
    color: "#FF9800",
    marginBottom: 12,
  },
  eventoActions: {
    flexDirection: "row",
    justifyContent: "flex-end",
    gap: 12,
  },
  actionButton: {
    padding: 8,
    borderRadius: 4,
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
  createButtonContainer: {
    paddingHorizontal: 16,
    marginBottom: 16,
  },
  createEventButton: {
    backgroundColor: "#4285F4",
    paddingVertical: 12,
    paddingHorizontal: 24,
    borderRadius: 8,
    alignItems: "center",
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
  },
  createEventButtonText: {
    color: "#FFFFFF",
    fontSize: 16,
    fontWeight: "600",
  },
  editButton: {
    backgroundColor: "#E3F2FD",
    borderRadius: 20
  },
  deleteButton: {
    backgroundColor: "#FFEBEE",
    borderRadius: 20
  },
  confirmationsButton: {
    backgroundColor: "#E8F5E8",
    borderRadius: 20
  },
  attendanceButton: {
    backgroundColor: "#FFF3E0",
    borderRadius: 20
  },
})

export default EventosScreen
