import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import { fileURLToPath, URL } from "node:url";

// O backend (.NET) roda em http://localhost:5131 (perfil "http").
// Em dev fazemos proxy de /v1 e /openapi para evitar CORS.
const API_TARGET = process.env.VITE_API_PROXY_TARGET ?? "http://localhost:5131";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    proxy: {
      "/v1": { target: API_TARGET, changeOrigin: true },
      "/openapi": { target: API_TARGET, changeOrigin: true },
    },
  },
});
