import React from 'react';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import AppNavigator from './src/navigation/AppNavigator';
import { setupFetchInterceptor } from './src/services/setupInterceptors';

// Configurar el interceptor al inicio
setupFetchInterceptor();

const App = () => {
  return (
    <SafeAreaProvider>
      <AppNavigator />
    </SafeAreaProvider>
  );
};

export default App;