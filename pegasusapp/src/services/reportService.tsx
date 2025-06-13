import RNFS from "react-native-fs"
import { PermissionsAndroid, Platform, Alert, Linking } from "react-native"
import type { AlumnoConCalificaciones } from "../models/alumnoConCalificaciones"
import type { Calificaciones } from "../models/calificaciones"

export interface ReportData {
  cursoNombre: string
  materiaNombre: string
  alumnos: AlumnoConCalificaciones[]
  fechaGeneracion: string
  subtitulo?: string
}

export const formatDate = (date: Date): string => {
  try {
    return date.toLocaleDateString("es-ES", {
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    })
  } catch (error) {
    console.error("Error formateando fecha:", error)
    const day = date.getDate().toString().padStart(2, "0")
    const month = (date.getMonth() + 1).toString().padStart(2, "0")
    const year = date.getFullYear()
    const hours = date.getHours().toString().padStart(2, "0")
    const minutes = date.getMinutes().toString().padStart(2, "0")
    return `${day}/${month}/${year} ${hours}:${minutes}`
  }
}

const calcularPromedio = (calificaciones: Calificaciones[]): number => {
  if (calificaciones.length === 0) return 0
  const suma = calificaciones.reduce((acc, cal) => acc + cal.calificacion, 0)
  return Math.round((suma / calificaciones.length) * 100) / 100
}

const requestStoragePermission = async (): Promise<boolean> => {
  if (Platform.OS !== "android") {
    return true
  }

  try {
    const androidVersion = Platform.Version
    console.log("🔍 Android Version:", androidVersion)

    if (androidVersion >= 30) {
      // Para Android 11+, intentar solicitar permisos de gestión de archivos
      console.log("📱 Android 11+ detectado, usando permisos modernos")
      return true
    } else {
      // Para versiones anteriores de Android
      console.log("📱 Android anterior a 11, solicitando permisos tradicionales")
      const granted = await PermissionsAndroid.requestMultiple([
        PermissionsAndroid.PERMISSIONS.WRITE_EXTERNAL_STORAGE,
        PermissionsAndroid.PERMISSIONS.READ_EXTERNAL_STORAGE,
      ])

      const writeGranted =
        granted[PermissionsAndroid.PERMISSIONS.WRITE_EXTERNAL_STORAGE] === PermissionsAndroid.RESULTS.GRANTED
      const readGranted =
        granted[PermissionsAndroid.PERMISSIONS.READ_EXTERNAL_STORAGE] === PermissionsAndroid.RESULTS.GRANTED

      console.log("✅ Permisos - Write:", writeGranted, "Read:", readGranted)
      return writeGranted && readGranted
    }
  } catch (err) {
    console.error("❌ Error requesting permissions:", err)
    return true // Continuar de todos modos
  }
}

const getDownloadPath = async (): Promise<{ path: string; location: string }> => {
  try {
    if (Platform.OS === "android") {
      const androidVersion = Platform.Version
      console.log("📁 Determinando ruta de descarga para Android", androidVersion)

      // Intentar diferentes rutas en orden de preferencia
      const possiblePaths = [
        { path: RNFS.DownloadDirectoryPath, location: "Descargas" },
        { path: RNFS.ExternalDirectoryPath, location: "Almacenamiento externo" },
        { path: `${RNFS.ExternalStorageDirectoryPath}/Download`, location: "Carpeta Download" },
        { path: RNFS.DocumentDirectoryPath, location: "Documentos de la app" },
      ]

      for (const option of possiblePaths) {
        if (option.path) {
          try {
            // Verificar si la ruta existe y es escribible
            const exists = await RNFS.exists(option.path)
            console.log(`📂 Verificando ${option.location}: ${option.path} - Existe: ${exists}`)

            if (exists) {
              return option
            }
          } catch (error) {
            console.log(`⚠️ Error verificando ${option.location}:`, error)
            continue
          }
        }
      }

      // Si ninguna funciona, usar DocumentDirectoryPath como fallback
      console.log("📁 Usando directorio de documentos como fallback")
      return { path: RNFS.DocumentDirectoryPath, location: "Documentos de la app" }
    } else {
      return { path: RNFS.DocumentDirectoryPath, location: "Documentos" }
    }
  } catch (error) {
    console.error("❌ Error obteniendo ruta de descarga:", error)
    return { path: RNFS.DocumentDirectoryPath, location: "Documentos de la app" }
  }
}

// ==================== GENERADOR DE HTML ====================

const generateCalificacionesHTML = (data: ReportData): string => {
  return `
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Reporte de Calificaciones</title>
    <style>
        body {
            font-family: 'Arial', sans-serif;
            margin: 20px;
            color: #333;
            line-height: 1.4;
            background-color: white;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
            background-color: white;
            padding: 20px;
        }
        .header {
            text-align: center;
            border-bottom: 3px solid #4285F4;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }
        .header h1 {
            color: #4285F4;
            margin: 0;
            font-size: 24px;
            font-weight: bold;
        }
        .header h2 {
            color: #666;
            margin: 5px 0;
            font-size: 18px;
            font-weight: normal;
        }
        .header p {
            color: #888;
            margin: 5px 0;
            font-size: 12px;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
            background-color: white;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        th {
            background-color: #4285F4;
            color: white;
            padding: 12px 8px;
            text-align: left;
            font-weight: 600;
            border-bottom: 2px solid #3367d6;
        }
        td {
            padding: 12px 8px;
            border-bottom: 1px solid #e0e0e0;
            vertical-align: top;
        }
        tr:nth-child(even) {
            background-color: #f8f9fa;
        }
        tr:hover {
            background-color: #e3f2fd;
        }
        .nombre-col {
            width: 25%;
        }
        .calificaciones-col {
            width: 50%;
        }
        .promedio-col {
            width: 25%;
            text-align: center;
        }
        .apellido {
            font-weight: 600;
            color: #333;
            font-size: 14px;
            display: block;
        }
        .nombre {
            color: #555;
            font-size: 13px;
            margin-top: 2px;
        }
        .calificacion-item {
            margin: 2px 0;
            font-size: 14px;
            display: inline-block;
            margin-right: 12px;
        }
        .calificacion-aprobado {
            color: #4CAF50;
            font-weight: 600;
        }
        .calificacion-desaprobado {
            color: #F44336;
            font-weight: 600;
        }
        .sin-calificaciones {
            color: #999;
            font-style: italic;
            font-size: 14px;
        }
        .promedio-valor {
            font-weight: bold;
            font-size: 16px;
        }
        .promedio-aprobado {
            color: #4CAF50;
        }
        .promedio-desaprobado {
            color: #F44336;
        }
        .promedio-na {
            color: #999;
        }
        .footer {
            margin-top: 30px;
            text-align: center;
            font-size: 10px;
            color: #888;
            border-top: 1px solid #e0e0e0;
            padding-top: 15px;
        }
        @media print {
            body { margin: 0; }
            .container { max-width: 100%; }
            tr { page-break-inside: avoid; }
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>REPORTE DE CALIFICACIONES</h1>
            <h2>${data.cursoNombre} - ${data.materiaNombre}</h2>
            ${data.subtitulo ? `<p>${data.subtitulo}</p>` : ""}
            <p>Fecha de generación: ${data.fechaGeneracion}</p>
        </div>

        <table>
            <tr>
                <th class="nombre-col">Nombre</th>
                <th class="calificaciones-col">Calificaciones</th>
                <th class="promedio-col">Promedio</th>
            </tr>
            ${data.alumnos
              .map((alumno) => {
                const promedio = calcularPromedio(alumno.calificaciones)
                const tieneCalificaciones = alumno.calificaciones.length > 0

                let promedioClass = "promedio-na"
                if (tieneCalificaciones) {
                  promedioClass = promedio >= 6 ? "promedio-aprobado" : "promedio-desaprobado"
                }

                return `
            <tr>
                <td>
                    <span class="apellido">${alumno.usuario.apellido}</span>
                    <span class="nombre">${alumno.usuario.nombre}</span>
                </td>
                <td>
                    ${
                      tieneCalificaciones
                        ? alumno.calificaciones
                            .map(
                              (cal, index) => `
                                <span class="calificacion-item">
                                    Nota ${index + 1}: 
                                    <span class="${
                                      cal.calificacion >= 6 ? "calificacion-aprobado" : "calificacion-desaprobado"
                                    }">
                                        ${cal.calificacion}
                                    </span>
                                </span>
                            `,
                            )
                            .join("")
                        : '<span class="sin-calificaciones">Sin calificaciones</span>'
                    }
                </td>
                <td class="promedio-col">
                    <span class="promedio-valor ${promedioClass}">
                        ${tieneCalificaciones ? promedio : "N/A"}
                    </span>
                </td>
            </tr>
        `
              })
              .join("")}
        </table>

        <div class="footer">
            <p>Reporte generado por Pegasus App - ${data.fechaGeneracion}</p>
            <p>Este documento contiene información confidencial del sistema educativo</p>
        </div>
    </div>
</body>
</html>
  `
}

// ==================== FUNCIÓN PRINCIPAL ====================

export const generateReport = async (data: ReportData): Promise<string> => {
  try {
    console.log("📄 Iniciando generación de reporte HTML...")

    // Solicitar permisos
    const hasPermission = await requestStoragePermission()
    if (!hasPermission) {
      console.log("⚠️ Sin permisos, pero continuando...")
    }

    // Generar HTML
    const htmlContent = generateCalificacionesHTML(data)

    // Generar nombre del archivo con timestamp
    const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, "-")
    const fileName = `Reporte_${data.materiaNombre.replace(/\s+/g, "_")}_${data.cursoNombre.replace(
      /\s+/g,
      "_",
    )}_${timestamp}.html`

    // Obtener ruta de descarga
    const { path: downloadPath, location } = await getDownloadPath()
    const filePath = `${downloadPath}/${fileName}`

    console.log("📁 Guardando HTML en:", filePath)
    console.log("📍 Ubicación:", location)

    // Escribir el archivo HTML
    await RNFS.writeFile(filePath, htmlContent, "utf8")

    console.log("✅ Reporte HTML generado exitosamente")

    // Verificar que el archivo se creó correctamente
    const fileExists = await RNFS.exists(filePath)
    if (!fileExists) {
      throw new Error("El archivo no se pudo crear correctamente")
    }

    // Obtener información del archivo
    const fileInfo = await RNFS.stat(filePath)
    console.log("📊 Tamaño del archivo:", Math.round(fileInfo.size / 1024), "KB")

    return filePath
  } catch (error) {
    console.error("❌ Error generando reporte HTML:", error)
    throw error
  }
}

// ==================== FUNCIÓN PARA MOSTRAR RESULTADO (VERSIÓN ORIGINAL MEJORADA) ====================

export const showReportResult = async (filePath: string): Promise<void> => {
  try {
    console.log("📖 Mostrando resultado del reporte HTML:", filePath)

    // Verificar que el archivo existe
    const fileExists = await RNFS.exists(filePath)
    if (!fileExists) {
      throw new Error("El archivo HTML no existe")
    }

    // Obtener información del archivo
    const fileInfo = await RNFS.stat(filePath)
    const fileSizeKB = Math.round(fileInfo.size / 1024)
    const fileName = filePath.split("/").pop()

    // Determinar ubicación amigable basada en la ruta
    let ubicacion = "Documentos de la aplicación"
    let instrucciones = "Busca en tu explorador de archivos en la carpeta de la aplicación."

    if (filePath.includes("Download")) {
      ubicacion = "Carpeta de Descargas"
      instrucciones = "Ve a tu explorador de archivos > Descargas para encontrar el archivo."
    } else if (filePath.includes("External")) {
      ubicacion = "Almacenamiento externo"
      instrucciones = "Busca en tu explorador de archivos en el almacenamiento del dispositivo."
    }

    console.log("📍 Ubicación determinada:", ubicacion)
    console.log("📄 Nombre del archivo:", fileName)

    Alert.alert(
      "✅ Reporte Generado",
      `El reporte se guardó correctamente:\n\n📄 ${fileName}\n📊 Tamaño: ${fileSizeKB} KB\n📁 Ubicación: ${ubicacion}\n\n💡 ${instrucciones}\n\n🔍 Busca archivos que empiecen con "Reporte_"`,
      [
        {
          text: "Abrir Explorador",
          onPress: () => {
            // Volver a la versión simple que funcionaba
            const fileManagers = [
              "content://com.android.externalstorage.documents/root/primary",
              "content://com.android.providers.downloads.documents/root/downloads",
            ]

            const tryOpenFileManager = async (index = 0) => {
              if (index >= fileManagers.length) {
                Alert.alert("Info", "Abre tu explorador de archivos manualmente para encontrar el reporte.")
                return
              }

              try {
                await Linking.openURL(fileManagers[index])
              } catch (error) {
                tryOpenFileManager(index + 1)
              }
            }

            tryOpenFileManager()
          },
        },
        { text: "Entendido" },
      ],
    )

    console.log("✅ Resultado del reporte HTML mostrado correctamente")
  } catch (error: any) {
    console.error("❌ Error mostrando resultado del reporte HTML:", error)
    Alert.alert("Error", "Hubo un problema con el archivo generado.")
  }
}

// ==================== FUNCIONES DE APERTURA ====================

export const openReport = async (filePath: string): Promise<void> => {
  try {
    console.log("📖 Abriendo reporte HTML:", filePath)

    // Verificar que el archivo existe
    const fileExists = await RNFS.exists(filePath)
    if (!fileExists) {
      throw new Error("El archivo HTML no existe")
    }

    // Crear URL para abrir
    const fileUrl = `file://${filePath}`

    console.log("🔗 URL del archivo HTML:", fileUrl)

    // Intentar abrir con el navegador predeterminado
    const canOpen = await Linking.canOpenURL(fileUrl)

    if (canOpen) {
      await Linking.openURL(fileUrl)
      console.log("✅ Reporte HTML abierto en el navegador")
    } else {
      throw new Error("No se puede abrir automáticamente")
    }
  } catch (error: any) {
    console.error("❌ Error abriendo reporte HTML:", error)

    const fileName = filePath.split("/").pop()
    Alert.alert(
      "Archivo Guardado",
      `El reporte se guardó exitosamente como:\n\n📄 ${fileName}\n\n📁 Ubicación: Carpeta de Descargas\n\n💡 Puedes abrirlo desde tu explorador de archivos.`,
      [{ text: "Entendido" }],
    )
  }
}

// ==================== FUNCIÓN DE CONVENIENCIA ====================

export const generateCalificacionesReport = async (
  data: Omit<ReportData, "fechaGeneracion">,
  autoShow = true,
): Promise<string> => {
  const reportData: ReportData = {
    ...data,
    fechaGeneracion: formatDate(new Date()),
  }

  const filePath = await generateReport(reportData)

  if (autoShow) {
    await showReportResult(filePath)
  }

  return filePath
}

// ==================== FUNCIÓN PARA GENERAR REPORTES DE ASISTENCIA ====================

export const generateAsistenciaReport = async (
  data: {
    cursoNombre: string
    materiaNombre: string
    fecha: string
    alumnos: any[]
    estadisticas?: {
      total: number
      presentes: number
      ausentes: number
      porcentajePresentes: number
    }
    subtitulo?: string
  },
  autoShow = true,
): Promise<string> => {
  try {
    console.log("📄 Iniciando generación de reporte de asistencia...")

    // Solicitar permisos
    const hasPermission = await requestStoragePermission()
    if (!hasPermission) {
      console.log("⚠️ Sin permisos, pero continuando...")
    }

    const reportData = {
      ...data,
      fechaGeneracion: formatDate(new Date()),
    }

    const htmlContent = generateAsistenciaHTML(reportData)

    // Generar nombre del archivo con timestamp
    const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, "-")
    const fileName = `Reporte_Asistencia_${data.materiaNombre.replace(/\s+/g, "_")}_${data.cursoNombre.replace(
      /\s+/g,
      "_",
    )}_${timestamp}.html`

    // Obtener ruta de descarga
    const { path: downloadPath, location } = await getDownloadPath()
    const filePath = `${downloadPath}/${fileName}`

    console.log("📁 Guardando HTML en:", filePath)
    console.log("📍 Ubicación:", location)

    // Escribir el archivo HTML
    await RNFS.writeFile(filePath, htmlContent, "utf8")

    console.log("✅ Reporte de asistencia generado exitosamente")

    if (autoShow) {
      await showReportResult(filePath)
    }

    return filePath
  } catch (error) {
    console.error("❌ Error generando reporte de asistencia:", error)
    throw error
  }
}

// ==================== GENERADOR DE HTML PARA ASISTENCIA ====================

const generateAsistenciaHTML = (data: any): string => {
  return `
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Reporte de Asistencia</title>
    <style>
        body {
            font-family: 'Arial', sans-serif;
            margin: 20px;
            color: #333;
            line-height: 1.4;
            background-color: white;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
            background-color: white;
            padding: 20px;
        }
        .header {
            text-align: center;
            border-bottom: 3px solid #4285F4;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }
        .header h1 {
            color: #4285F4;
            margin: 0;
            font-size: 24px;
            font-weight: bold;
        }
        .header h2 {
            color: #666;
            margin: 5px 0;
            font-size: 18px;
            font-weight: normal;
        }
        .header p {
            color: #888;
            margin: 5px 0;
            font-size: 12px;
        }
        .fecha-asistencia {
            color: #4285F4;
            font-size: 16px;
            font-weight: 600;
            margin: 20px 0 10px 0;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
            background-color: white;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        th {
            background-color: #4285F4;
            color: white;
            padding: 12px 8px;
            text-align: left;
            font-weight: 600;
            border-bottom: 2px solid #3367d6;
        }
        td {
            padding: 12px 8px;
            border-bottom: 1px solid #e0e0e0;
            vertical-align: top;
        }
        tr:nth-child(even) {
            background-color: #f8f9fa;
        }
        tr:hover {
            background-color: #e3f2fd;
        }
        .apellido-col {
            width: 35%;
        }
        .nombre-col {
            width: 35%;
        }
        .presente-col {
            width: 30%;
            text-align: center;
        }
        .presente-si {
            color: #4CAF50;
            font-weight: 600;
        }
        .presente-no {
            color: #F44336;
            font-weight: 600;
        }
        .footer {
            margin-top: 30px;
            text-align: center;
            font-size: 10px;
            color: #888;
            border-top: 1px solid #e0e0e0;
            padding-top: 15px;
        }
        @media print {
            body { margin: 0; }
            .container { max-width: 100%; }
            tr { page-break-inside: avoid; }
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>REPORTE DE ASISTENCIA</h1>
            <h2>${data.cursoNombre} - ${data.materiaNombre}</h2>
            ${data.subtitulo ? `<p>${data.subtitulo}</p>` : ""}
            <p>Fecha de generación: ${data.fechaGeneracion}</p>
        </div>
        
        ${
          data.alumnos && data.alumnos.length > 0
            ? `
            <h5 class="fecha-asistencia">Asistencia para ${data.fecha}</h5>
            <table>
                <thead>
                    <tr>
                        <th class="apellido-col">Apellido</th>
                        <th class="nombre-col">Nombre</th>
                        <th class="presente-col">Presente</th>
                    </tr>
                </thead>
                <tbody>
                    ${data.alumnos
                      .map(
                        (alumno: any) => `
                        <tr>
                            <td>${alumno.alumno?.apellido || alumno.apellido || ""}</td>
                            <td>${alumno.alumno?.nombre || alumno.nombre || ""}</td>
                            <td class="presente-col">
                                <span class="${alumno.presente ? "presente-si" : "presente-no"}">
                                    ${alumno.presente ? "Si" : "No"}
                                </span>
                            </td>
                        </tr>
                    `,
                      )
                      .join("")}
                </tbody>
            </table>
            `
            : `
            <p>No hay datos de asistencia para mostrar.</p>
            `
        }

        <div class="footer">
            <p>Reporte generado por Pegasus App - ${data.fechaGeneracion}</p>
            <p>Este documento contiene información confidencial del sistema educativo</p>
        </div>
    </div>
</body>
</html>
`
}
