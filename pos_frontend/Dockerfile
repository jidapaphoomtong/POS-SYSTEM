# -----------------------------
# Stage 1: Build React App
# -----------------------------
FROM node:18-alpine AS builder

WORKDIR /app

# คัดลอก package.json และ package-lock.json (ถ้ามี)
COPY package*.json ./

# ติดตั้ง dependencies สำหรับ build
RUN npm install

# คัดลอก source code ทั้งหมด
COPY . .

# สร้าง production build ของ React App
RUN npm run build

# -----------------------------
# Stage 2: Deploy ด้วย Express Server
# -----------------------------
FROM node:18-alpine

WORKDIR /app

# คัดลอกไฟล์ package.json เพื่อทำการติดตั้ง production dependencies
COPY package*.json ./

# ติดตั้ง dependencies แบบ production
RUN npm install --only=production

# คัดลอกไฟล์ build ที่ได้จาก Stage 1
COPY --from=builder /app/dist ./dist

# คัดลอกไฟล์ server.js เพื่อใช้งาน Express server
COPY server.cjs ./

# เปิด port 3000
EXPOSE 3000

# รัน Express server
CMD [ "npm", "start" ]