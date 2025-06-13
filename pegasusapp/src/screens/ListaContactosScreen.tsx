import type React from "react"
import { View, Text, StyleSheet, TouchableOpacity } from "react-native"
import { useNavigation } from "@react-navigation/native"
import type { NativeStackNavigationProp } from "@react-navigation/native-stack"
import type { RootStackParamList } from "../navigation/AppNavigator"
import AppLayout from "../components/AppLayout"
import { useUser } from "../context/UserContext"

type NavigationProp = NativeStackNavigationProp<RootStackParamList, "ListaContacto">

const ListaContactosScreen: React.FC = () => {
  const navigation = useNavigation<NavigationProp>()
  const { userData } = useUser()

  const handleTipoContactoPress = (tipoContacto: number) => {
    navigation.navigate("Contactos", { tipoContacto })
  }

  return (
    <AppLayout title={`Contactos`} showHomeButton={true}>
      <View style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.headerTitle}>Contactos</Text>
        </View>

        <View style={styles.cardsContainer}>
          {/* Tarjeta Institución */}
          <TouchableOpacity style={styles.card} onPress={() => handleTipoContactoPress(1)} activeOpacity={0.8}>
            <Text style={styles.cardTitle}>Institución</Text>
            <View style={styles.iconContainer}>
              <Text style={styles.iconText}>🏫</Text>
            </View>
          </TouchableOpacity>
        {/* Tarjeta Docentes - Solo para perfiles 1 y 5 */}
        {userData && (userData.id_perfil === 1 || userData.id_perfil === 5) && (
          <TouchableOpacity style={styles.card} onPress={() => handleTipoContactoPress(2)} activeOpacity={0.8}>
            <Text style={styles.cardTitle}>Docentes</Text>
            <View style={styles.iconContainer}>
              <Text style={styles.iconText}>👥</Text>
            </View>
          </TouchableOpacity>
        )}
        </View>
      </View>
    </AppLayout>
  )
}

const styles = StyleSheet.create({
  content: {
    flex: 1,
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: "bold",
    color: "#333",
    textAlign: "center",
    marginBottom: 10,
  },
  separator: {
    height: 1,
    backgroundColor: "#ddd",
    marginBottom: 30,
  },
  cardsContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    gap: 40,
  },
  card: {
    backgroundColor: "#fff",
    borderRadius: 12,
    padding: 30,
    alignItems: "center",
    justifyContent: "center",
    width: "80%",
    maxWidth: 300,
    shadowColor: "#000",
    shadowOffset: {
      width: 0,
      height: 2,
    },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
    borderWidth: 1,
    borderColor: "#e0e0e0",
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
    marginBottom: 15,
    textAlign: "center",
  },
  iconContainer: {
    justifyContent: "center",
    alignItems: "center",
  },
  iconText: {
    fontSize: 60,
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
  container: {
    flex: 1,
    backgroundColor: "#F5F5F5",
  },
})

export default ListaContactosScreen
