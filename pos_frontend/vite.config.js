import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api':{
        target: 'http://localhost:5293/api',
        // target: 'https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api', // เปลี่ยน URL backend ที่คุณต้องการ
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      }
    }
  }
})
