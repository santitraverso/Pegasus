import AsyncStorage from '@react-native-async-storage/async-storage';

export const setupFetchInterceptor = () => {
  const originalFetch = global.fetch;
  
  global.fetch = async (input: RequestInfo | URL, init?: RequestInit) => {
    try {
      // Obtener el token
      const token = await AsyncStorage.getItem('jwtToken');
      
      // Crear nuevos headers con el token
      const headers = new Headers(init?.headers || {});
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
      if (!headers.has('Content-Type')) {
        headers.set('Content-Type', 'application/json');
      }
      
      // Crear nuevas opciones con los headers actualizados
      const modifiedInit = {
        ...init,
        headers,
      };
      
      // Llamar al fetch original con las opciones modificadas
      return originalFetch(input, modifiedInit);
    } catch (error) {
      console.error('Error en fetch interceptor:', error);
      return originalFetch(input, init);
    }
  };
};