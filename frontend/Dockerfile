# ใช้ Node.js เวอร์ชัน 16
FROM node:16

# ตั้งค่า working directory
WORKDIR /app

# คัดลอก package.json และ package-lock.json เข้าไปใน container
COPY package*.json ./

# ติดตั้ง dependencies
RUN npm install

# ติดตั้ง serve (ตัวช่วยสำหรับเสิร์ฟไฟล์ build)
RUN npm install -g serve

# คัดลอกไฟล์ทั้งหมดจากโปรเจกต์เข้าไปใน container
COPY . .

# สร้าง production build
RUN npm run build

# เปิดพอร์ต 3000 สำหรับ container
EXPOSE 3000

# ตั้งค่า entrypoint ให้ใช้ serve เสิร์ฟไฟล์ในโฟลเดอร์ build
CMD ["serve", "-s", "build", "-l", "3000"]
