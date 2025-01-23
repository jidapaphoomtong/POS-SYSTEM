// server.js
const express = require('express');
const path = require('path');
const {createProxyMiddleware} = require('http-proxy-middleware');

const app = express();

// ตั้ง reverse proxy สำหรับ API endpoints
// เปลี่ยน target ให้ตรงกับ URL ของ backend ที่ต้องการ proxy ไป
app.use(
    '/api',
    createProxyMiddleware({
        target: 'http://localhost:5293/api',
        // target: 'https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api', // เปลี่ยน URL backend ที่คุณต้องการ
        changeOrigin: true,
    })
);

// เสิร์ฟไฟล์ static ของ React build
app.use(express.static(path.join(__dirname, 'dist')));

// สำหรับคำขอ (requests) อื่นๆ ให้ส่งกลับ index.html เพื่อรองรับ React Router
app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, 'dist', 'index.html'));
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});