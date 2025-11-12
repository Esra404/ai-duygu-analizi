import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    host: true, // dış bağlantılara izin verir
    port: 3000,
    open: true,
    allowedHosts: 'all', // tüm dış hostlara izin verir (ngrok dahil)
    cors: true // istersen CORS'u da açık bırak
  }
})
