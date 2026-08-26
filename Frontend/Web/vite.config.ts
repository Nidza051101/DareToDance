import { resolve } from 'node:path'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      input: {
        main: resolve(import.meta.dirname, 'index.html'),
        verify: resolve(import.meta.dirname, 'verify.html'),
        home: resolve(import.meta.dirname, 'home.html'),
        users: resolve(import.meta.dirname, 'users.html'),
        completeRegistration: resolve(import.meta.dirname, 'complete-registration.html'),
      },
    },
  },
})
