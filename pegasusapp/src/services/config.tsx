const ENVIRONMENT = 'ANDROID_EMULATOR';

const API_URLS = {
  // Para emulador Android con HTTPS
  ANDROID_EMULATOR: 'http://10.0.2.2:5130',
  
  // Para dispositivo físico (reemplaza con tu IP real)
  PHYSICAL_DEVICE: 'https://192.168.1.100:5130', // Tu IP local
  
  // Para iOS simulator
  IOS_SIMULATOR: 'https://localhost:7130/api',
  
  // Producción
  PRODUCTION: 'https://pegasus-api-v2.azurewebsites.net'
};

export const CONFIG = {
  API_BASE_URL: false ? API_URLS[ENVIRONMENT] : API_URLS.PRODUCTION,
  
  // Configuración adicional para desarrollo con HTTPS
  ALLOW_SELF_SIGNED_CERTS: __DEV__, // Solo en desarrollo
};