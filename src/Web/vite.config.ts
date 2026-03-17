import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const gatewayUrl =
  process.env['services__gateway__https__0'] ??
  process.env['services__gateway__http__0'] ??
  'http://localhost:5000'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: gatewayUrl,
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
