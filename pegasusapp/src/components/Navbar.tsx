import type React from "react"
import { View, Text, StyleSheet, TouchableOpacity, Alert } from "react-native"
import Icon from "react-native-vector-icons/MaterialIcons"
import { signOut } from "../services/authService"
import type { AppUserData } from "../models/appUserData"

interface NavbarProps {
  userData?: AppUserData | null
  onHomePress?: () => void
  showHomeButton?: boolean
  title?: string
}

const Navbar: React.FC<NavbarProps> = ({ userData, onHomePress, showHomeButton = true, title }) => {
  const handleSignOut = async () => {
    Alert.alert("Cerrar Sesión", "¿Estás seguro que deseas cerrar sesión?", [
      {
        text: "Cancelar",
        style: "cancel",
      },
      {
        text: "Cerrar Sesión",
        style: "destructive",
        onPress: async () => {
          try {
            await signOut()
          } catch (error) {
            Alert.alert("Error", "No se pudo cerrar la sesión. Intenta nuevamente.")
          }
        },
      },
    ])
  }

  const handleHomePress = () => {
    if (onHomePress) {
      onHomePress()
    }
  }

  return (
    <View style={styles.navbar}>
      <View style={styles.leftSection}>
        {showHomeButton && (
          <TouchableOpacity
            style={[styles.iconButton, styles.homeButton]}
            onPress={handleHomePress}
            activeOpacity={0.7}
          >
            <Icon name="home" size={24} color="#4285F4" />
          </TouchableOpacity>
        )}
      </View>

      <View style={styles.centerSection}>
        {userData ? (
          <View style={styles.userInfo}>
            <Text style={styles.userName} numberOfLines={1}>
              {userData.name || "Usuario"}
            </Text>
            <Text style={styles.userRole} numberOfLines={1}>
              {userData.role}
            </Text>
          </View>
        ) : (
          <View style={styles.userInfo}>
            <Text style={styles.userName}>Cargando...</Text>
            <Text style={styles.userRole}>Usuario</Text>
          </View>
        )}
      </View>

      <View style={styles.rightSection}>
        <TouchableOpacity style={[styles.iconButton, styles.logoutButton]} onPress={handleSignOut} activeOpacity={0.7}>
          <Icon name="logout" size={24} color="#DB4437" />
        </TouchableOpacity>
      </View>
    </View>
  )
}

const styles = StyleSheet.create({
  navbar: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    backgroundColor: "#FFFFFF",
    paddingHorizontal: 16,
    paddingVertical: 12,
    paddingTop: 50, // Espacio para el status bar
    elevation: 4,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    borderBottomWidth: 1,
    borderBottomColor: "#E0E0E0",
  },
  leftSection: {
    flexDirection: "row",
    alignItems: "center",
    flex: 1,
  },
  centerSection: {
    flex: 2,
    alignItems: "center",
  },
  rightSection: {
    flex: 1,
    alignItems: "flex-end",
  },
  iconButton: {
    padding: 8,
    borderRadius: 20,
    backgroundColor: "#F5F5F5",
    justifyContent: "center",
    alignItems: "center",
    width: 40,
    height: 40,
  },
  homeButton: {
    backgroundColor: "#FFFFFF",
    borderWidth: 1,
    borderColor: "#DADCE0",
    elevation: 2,
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.15,
    shadowRadius: 2,
  },
  logoutButton: {
    backgroundColor: "#FFF0F0",
  },
  title: {
    fontSize: 18,
    fontWeight: "600",
    color: "#333",
    marginLeft: 12,
  },
  userInfo: {
    alignItems: "center",
  },
  userName: {
    fontSize: 16,
    fontWeight: "600",
    color: "#333",
    textAlign: "center",
  },
  userRole: {
    fontSize: 12,
    color: "#4285F4",
    fontWeight: "500",
    textAlign: "center",
    marginTop: 2,
  },
})

export default Navbar
