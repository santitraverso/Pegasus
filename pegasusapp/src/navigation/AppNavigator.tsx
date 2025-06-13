"use client"

import type React from "react"
import { useEffect, useState } from "react"
import { NavigationContainer } from "@react-navigation/native"
import { createNativeStackNavigator } from "@react-navigation/native-stack"
import type { FirebaseAuthTypes } from "@react-native-firebase/auth"

// Pantallas
import LoginScreen from "../screens/LoginScreen"
import HomeScreen from "../screens/HomeScreen"
import SplashScreen from "../screens/SplashScreen"
import ListaCursosScreen from "../screens/ListaCursosScreen"
import ListaMateriasScreen from "../screens/ListaMateriasScreen"
import CalificacionesScreen from "../screens/CalificacionesScreen"
import CreateCalificacionScreen from "../screens/CreateCalificacionScreen"
import ReporteCalificacionesScreen from "../screens/ReporteCalificacionesScreen"
import CuadernoScreen from "../screens/CuadernoScreen"
import ListaComunicadosScreen from "../screens/ListaComunicadosScreen"
import CreateComunicadoScreen from "../screens/CreateComunicadoScreen"
import DesempenioScreen from "../screens/DesempenioScreen"
import CreateDesempenioScreen from "../screens/CreateDesempenioScreen"
import AsistenciaScreen from "../screens/AsistenciaScreen"
import ReporteAsistenciaScreen from "../screens/ReporteAsistenciaScreen"
import CursosScreen from "../screens/CursosScreen"
import CreateCursoScreen from "../screens/CreateCursoScreen"
import IntegrantesCursosScreen from "../screens/IntegrantesCursosScreen"
import MateriasCursoScreen from "../screens/MateriasCursoScreen"
import MateriasScreen from "../screens/MateriasScreen"
import CreateMateriaScreen from "../screens/CreateMateriaScreen"
import ListaContenidosScreen from "../screens/ListaContenidosScreen"
import CreateContenidoMateriaScreen from "../screens/CreateContenidoMateriaScreen"
import UsuariosScreen from "../screens/UsuariosScreen"
import CreateUsuarioScreen from "../screens/CreateUsuarioScreen"
import ListaContactosScreen from "../screens/ListaContactosScreen"
import ContactosScreen from "../screens/ContactosScreen"
import CreateContactoScreen from "../screens/CreateContactoScreen"
import EventosScreen from "../screens/EventosScreen"
import CreateEventoScreen from "../screens/CreateEventoScreen"
import ConfirmarAsistenciaScreen from "../screens/ConfirmarAsistenciaScreen"
import ConfirmacionesEventoScreen from "../screens/ConfirmacionesEventoScreen"

import { AuthProvider } from "../context/AuthContext"
import { UserProvider } from "../context/UserContext"
import { subscribeToAuthChanges, getLastAuthError } from "../services/authService"

// Tipos para las rutas con parámetros
export type RootStackParamList = {
  Login: undefined
  Home: undefined
  Usuario: undefined
  CreateUsuario: { usuarioId?: number }
  ListaCursos: { parametro?: string }
  ListaMaterias: { cursoId: number; cursoNombre: string; modulo: string }
  Calificacion: { cursoId: number; cursoNombre: string; materiaId: number; materiaNombre: string }
  CreateCalificacion: {
    alumnoId: number
    alumnoNombre: string
    alumnoApellido: string
    cursoId: number
    cursoNombre: string
    materiaId: number
    materiaNombre: string
    esNuevo: boolean
  }
  ReporteCalificaciones: {
    cursoId: number
    cursoNombre: string
    materiaId: number
    materiaNombre: string
  }
  Cuaderno: {
    cursoId: number
    cursoNombre: string
  }
  ListaComunicados: {
    cursoId: number
    cursoNombre: string
    alumnosIds: number[]
  }
  CreateComunicado: {
    cursoId: number
    cursoNombre: string
    alumnosIds: number[]
  }
  Desempenio: { cursoId: number; cursoNombre: string }
  CreateDesempenio: {
    desempenioId: number
    alumnoId: number
    alumnoNombre: string
    alumnoApellido: string
    cursoId: number
    cursoNombre: string
    esVer: boolean
  }
  Asistencia: { cursoId: number; cursoNombre: string; materiaId: number; materiaNombre: string }
  ReporteAsistencia: { cursoId: number; cursoNombre: string; materiaId: number; materiaNombre: string; fecha: string }
  Cursos: undefined
  Curso: undefined
  CreateCurso: { cursoId?: number }
  IntegrantesCursos: { cursoId: number }
  MateriasCurso: { cursoId: number }
  Materias: undefined
  Materia: undefined
  CreateMateria: { materiaId?: number; viewOnly?: boolean }
  ListaContenidos: { materiaId: number; viewOnly?: boolean }
  CreateContenidoMateria: { materiaId: number; contenidoId?: number; viewOnly?: boolean }
  ListaContacto: undefined
  Contactos: { tipoContacto: number }
  CreateContacto: { tipoContacto: number; contactoId?: number }
  Evento: undefined
  CreateEvento: { eventoId?: number }
  ConfirmarAsistencia: { eventoId?: number }
  ConfirmacionesEvento: { eventoId?: number }
}

const Stack = createNativeStackNavigator<RootStackParamList>()

// Componente para las rutas autenticadas
const AuthenticatedRoutes = () => {
  return (
    <Stack.Navigator screenOptions={{ headerShown: false }}>
      <Stack.Screen name="Home" component={HomeScreen} />
      <Stack.Screen name="Usuario" component={UsuariosScreen} />
      <Stack.Screen name="CreateUsuario" component={CreateUsuarioScreen} />
      <Stack.Screen name="ListaCursos" component={ListaCursosScreen} />
      <Stack.Screen name="ListaMaterias" component={ListaMateriasScreen} />
      <Stack.Screen name="Calificacion" component={CalificacionesScreen} />
      <Stack.Screen name="CreateCalificacion" component={CreateCalificacionScreen} />
      <Stack.Screen name="ReporteCalificaciones" component={ReporteCalificacionesScreen} />
      <Stack.Screen name="Cuaderno" component={CuadernoScreen} />
      <Stack.Screen name="ListaComunicados" component={ListaComunicadosScreen} />
      <Stack.Screen name="CreateComunicado" component={CreateComunicadoScreen} />
      <Stack.Screen name="Desempenio" component={DesempenioScreen} />
      <Stack.Screen name="CreateDesempenio" component={CreateDesempenioScreen} />
      <Stack.Screen name="Asistencia" component={AsistenciaScreen} />
      <Stack.Screen name="ReporteAsistencia" component={ReporteAsistenciaScreen} />
      <Stack.Screen name="Cursos" component={CursosScreen} />
      <Stack.Screen name="CreateCurso" component={CreateCursoScreen} />
      <Stack.Screen name="IntegrantesCursos" component={IntegrantesCursosScreen} />
      <Stack.Screen name="MateriasCurso" component={MateriasCursoScreen} />
      <Stack.Screen name="Materias" component={MateriasScreen} />
      <Stack.Screen name="CreateMateria" component={CreateMateriaScreen} />
      <Stack.Screen name="ListaContenidos" component={ListaContenidosScreen} />
      <Stack.Screen name="CreateContenidoMateria" component={CreateContenidoMateriaScreen} />
      <Stack.Screen name="ListaContacto" component={ListaContactosScreen} />
      <Stack.Screen name="Contactos" component={ContactosScreen} />
      <Stack.Screen name="CreateContacto" component={CreateContactoScreen} />
      <Stack.Screen name="Evento" component={EventosScreen} />
      <Stack.Screen name="CreateEvento" component={CreateEventoScreen} />
      <Stack.Screen name="ConfirmarAsistencia" component={ConfirmarAsistenciaScreen} />
      <Stack.Screen name="ConfirmacionesEvento" component={ConfirmacionesEventoScreen} />
    </Stack.Navigator>
  )
}

const AppNavigator: React.FC = () => {
  const [initializing, setInitializing] = useState(true)
  const [user, setUser] = useState<FirebaseAuthTypes.User | null>(null)
  const [isLoginProcessing, setIsLoginProcessing] = useState(false)

  useEffect(() => {
    // Manejar cambios de estado de autenticación
    const handleAuthStateChanged = (authUser: FirebaseAuthTypes.User | null) => {


      if (authUser) {
        // Usuario autenticado en Firebase
        // Verificar inmediatamente si hay errores
        const immediateError = getLastAuthError()
        if (immediateError) {
          setUser(null)
          setIsLoginProcessing(false)
          if (initializing) setInitializing(false)
          return
        }

        setIsLoginProcessing(true)

        // Dar tiempo para que el proceso de login complete, pero verificar errores
        const checkInterval = setInterval(() => {
          const currentError = getLastAuthError()
          if (currentError) {
            clearInterval(checkInterval)
            setUser(null)
            setIsLoginProcessing(false)
            return
          }
        }, 500)
        
        // Timeout final después de 3 segundos
        setTimeout(() => {
          clearInterval(checkInterval)

          // Verificación final de errores
          const finalError = getLastAuthError()
          if (finalError) {
            setUser(null)
            setIsLoginProcessing(false)
          } else {
            setUser(authUser)
            setIsLoginProcessing(false)
          }
        }, 3000)
      } else {
        // Usuario no autenticado
        setUser(null)
        setIsLoginProcessing(false)
      }

      if (initializing) setInitializing(false)
    }

    // Suscribirse a cambios de autenticación
    const unsubscribe = subscribeToAuthChanges(handleAuthStateChanged)

    // Desuscribirse al desmontar
    return unsubscribe
  }, [initializing])

  if (initializing || isLoginProcessing) {
    return <SplashScreen message={isLoginProcessing ? "Validando usuario..." : "Cargando..."} />
  }

  return (
    <AuthProvider>
      <NavigationContainer>
        {!user ? (
          <Stack.Navigator screenOptions={{ headerShown: false }}>
            <Stack.Screen name="Login" component={LoginScreen} />
          </Stack.Navigator>
        ) : (
          <UserProvider>
            <AuthenticatedRoutes />
          </UserProvider>
        )}
      </NavigationContainer>
    </AuthProvider>
  )
}

export default AppNavigator
