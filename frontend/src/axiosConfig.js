import axios from "axios";

// กำหนด Base URL
const axiosInstance = axios.create({
    baseURL: "http://localhost:5293",
    headers: {
        "Content-Type": "application/json",
    },
});

// เพิ่ม Interceptor สำหรับแนบ Token
axiosInstance.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("authToken");
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

export default axiosInstance;