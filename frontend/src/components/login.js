import React, { useState } from "react";
import logo from "../Images/food.png";
import admin from "../Images/Admin.png";
import axios from "axios";
import { toast } from "react-toastify";
import { Link, useNavigate } from "react-router-dom";
import Cookies from "js-cookie";

const Login = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const navigate = useNavigate();

    const handleLogin = async (e) => {
        e.preventDefault();
        setIsLoading(true);

        if (!email || !password) {
            toast.error("Email and Password are required.");
            setIsLoading(false);
            return;
        }

        try {
            // ส่งคำขอ Login ไปยัง Backend
            const response = await axios.post(
                "http://localhost:5293/api/Auth/login",
                { email, password },
                {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        
                    },
                    withCredentials: true 
                }
            );
            // console.log("Login Response:", response.data); // ใช้เพื่อดู Response

            const { role } = response.data; // ดึง Role จาก Response
            console.log("User Role:", role); // Debug Role เพื่อดูว่ามีค่าหรือไม่

            const token = response.data.token;
            // console.log("Login successful, JWT Token received:", token);

            // เก็บ JWT Token ลงใน Cookie
            Cookies.set("authToken", token, { expires: 1, secure: true, sameSite: "Strict" });

            // นำทางตาม Role
            if (role === "Admin") {
                console.log("Redirecting to: /select-branch");
                navigate("/select-branch");
            } else if (role === "Manager" || role === "Employee") {
                console.log("Redirecting to: /sale");
                navigate("/sale");
            } else {
                console.log("Redirecting to: undefined");
                toast.error("Unknown role. Please contact support.");
            }
        } catch (error) {
            console.error("Login failed:", error.response?.data || error.message);

            if (error.response?.status === 401) {
                toast.error("Invalid login credentials.");
            } else {
                toast.error("Unable to login. Please try again.");
            }
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="login-register-container">
            <div className="logo">
                <img src={logo} alt="App Logo" />
            </div>
            <div className="form-container">
                <img src={admin} className="avatar" alt="Admin Avatar" />
                <form onSubmit={handleLogin}>
                    <input
                        type="text-form"
                        placeholder="Email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                        className="form-input"
                    />
                    <div className="input-group">
                        <input
                            type={showPassword ? "text" : "password"}
                            placeholder="Password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            className="form-input"
                        />
                        <button
                            type="button"
                            onClick={() => setShowPassword(!showPassword)}
                            className="show-password-btn"
                        >
                            {showPassword ? "Hide" : "Show"}
                        </button>
                    </div>
                    <button type="submit" className="btn" disabled={isLoading}>
                        {isLoading ? "Loading..." : "Login"}
                    </button>
                </form>
                <p>
                    Don't have an account? <Link to="/register">Register here</Link>
                </p>
            </div>
        </div>
    );
};

export default Login;