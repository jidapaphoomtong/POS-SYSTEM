import React, { useState } from "react";
import logo from "../Images/food.png";
import admin from "../Images/Admin.png";
import axios from "axios";
import { toast } from "react-toastify"; // Import toast
import { Link } from "react-router-dom";

const Login = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const handleLogin = async (e) => {
        e.preventDefault();
        setIsLoading(true);

        try {
        const response = await axios.post(
            "http://localhost:5293/api/Auth/login",
            { email, password },
            {
            headers: {
                "Content-Type": "application/json",
            },
            }
        );
        console.log("Success:", response.data);
        toast.success("Login successful! 🎉"); // แจ้งเตือนสำเร็จ
        } catch (error) {
        console.error("Error:", error.response?.data || error.message);
        toast.error(
            error.response?.data?.Message || "Unable to login. Please try again."
        ); // แจ้งเตือนข้อผิดพลาด
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
                type="email"
                placeholder="Email..."
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
            />
            <input
                type="password"
                placeholder="Password..."
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
            />
            <button type="submit" className="btn" disabled={isLoading}>
                {isLoading ? "Processing..." : "Login"}
            </button>
            </form>
            <p>Don't have an account? <Link to="/register">Register here</Link></p>
        </div>
        </div>
    );
};

export default Login;