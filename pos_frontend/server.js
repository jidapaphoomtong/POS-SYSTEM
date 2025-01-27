import express from 'express';
import path from 'path';
import { fileURLToPath } from 'url';
import { createProxyMiddleware } from 'http-proxy-middleware';

// เนื่องจาก ESM ไม่สามารถใช้ __dirname ได้ตรง ๆ
// ต้องใช้เทคนิคสร้าง __dirname จาก import.meta.url
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();

// (1) Reverse Proxy สำหรับ /api => เปลี่ยน target ให้ตรงกับ URL ของ backend
app.use(
    '/api',
    createProxyMiddleware({
        target: 'https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api', // หรือ URL อื่นที่คุณต้องการ
        changeOrigin: true,
        logger: console
    })
);

// (2) เสิร์ฟไฟล์ static จากโฟลเดอร์ dist (สร้างด้วย Vite)
app.use(express.static(path.join(__dirname, 'dist')));

// (3) รองรับ React Router หรือ SPA: ถ้าไม่มีไฟล์ตรงกัน ให้ส่ง index.html กลับ
app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, 'dist', 'index.html'));
});

// (4) กำหนด PORT
const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server started on port ${PORT}`);
});