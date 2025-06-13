import { Share, Platform } from "react-native"
import RNFS from "react-native-fs"

/**
 * Comparte un archivo con otras aplicaciones
 * @param filePath Ruta completa al archivo que se desea compartir
 * @returns Promise que se resuelve cuando se completa la acción de compartir
 */
export const shareFile = async (filePath: string): Promise<void> => {
  try {

    // Verificar que el archivo existe
    const fileExists = await RNFS.exists(filePath)
    if (!fileExists) {
      throw new Error("El archivo no existe")
    }

    // Preparar la URL del archivo según la plataforma
    const fileUrl = Platform.OS === "android" ? `file://${filePath}` : filePath

    // Determinar el tipo MIME basado en la extensión del archivo
    const extension = filePath.split(".").pop()?.toLowerCase() || ""
    let mimeType = "application/octet-stream" // Tipo por defecto

    // Asignar tipo MIME según extensión
    switch (extension) {
      case "pdf":
        mimeType = "application/pdf"
        break
      case "txt":
        mimeType = "text/plain"
        break
      case "csv":
        mimeType = "text/csv"
        break
      case "jpg":
      case "jpeg":
        mimeType = "image/jpeg"
        break
      case "png":
        mimeType = "image/png"
        break
    }

    // Opciones para compartir
    const shareOptions = {
      title: "Compartir Archivo",
      message: "Compartir reporte generado por Pegasus App", // Solo para Android
      url: fileUrl,
      type: mimeType,
    }

    // Compartir el archivo
    const result = await Share.share(shareOptions)

    if (result.action === Share.sharedAction) {
      if (result.activityType) {
      }
    } else if (result.action === Share.dismissedAction) {
    }
  } catch (error) {
    throw error
  }
}

/**
 * Comparte texto plano con otras aplicaciones
 * @param text Texto a compartir
 * @param title Título opcional para el diálogo de compartir
 * @returns Promise que se resuelve cuando se completa la acción de compartir
 */
export const shareText = async (text: string, title?: string): Promise<void> => {
  try {
    // Opciones para compartir
    const shareOptions = {
      title: title || "Compartir",
      message: text,
    }

    // Compartir el texto
    const result = await Share.share(shareOptions)

    if (result.action === Share.sharedAction) {
      console.log("✅ Texto compartido exitosamente")
    } else if (result.action === Share.dismissedAction) {
      console.log("❌ Compartir cancelado por el usuario")
    }
  } catch (error) {
    throw error
  }
}
