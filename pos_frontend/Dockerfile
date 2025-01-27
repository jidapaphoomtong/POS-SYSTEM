# -----------------------------
# Stage 1: Build (Vite)
# -----------------------------
    FROM node:20-alpine AS build
    WORKDIR /app
    
    COPY package.json package-lock.json ./
    RUN npm install
    
    COPY . . 
    RUN npm run build  # จะสร้างไฟล์ลงใน /app/dist
    
    # -----------------------------
    # Stage 2: Production
    # -----------------------------
    FROM node:20-alpine
    WORKDIR /app
    
    # Copy เฉพาะโฟลเดอร์ dist
    COPY --from=build /app/dist ./dist
    
    # Copy โค้ด server.js (ESM) + package.json สำหรับ Production
    COPY --from=build /app/server.js .
    COPY --from=build /app/package.json .
    COPY --from=build /app/package-lock.json .
    
    # ติดตั้งเฉพาะ dependencies ที่จำเป็น (prod)
    RUN npm install --omit=dev
    
    # เปิด port 3000
    EXPOSE 3000
    
    # รัน server.js
    CMD ["node", "server.js"]