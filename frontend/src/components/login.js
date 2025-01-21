import React, { useState } from "react";
import logo from "../Images/food.png";
import admin from "../Images/Admin.png";
import axios from "axios";
import { toast } from "react-toastify";
import { Link, useNavigate } from "react-router-dom";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";

const Login = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [branchId, setBranchId] = useState(""); // เพิ่มการจัดการ branchId
    const [isLoading, setIsLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const navigate = useNavigate();

    const handleLogin = async (e) => {
        e.preventDefault();
        setIsLoading(true);

        if (!email || !password ) { // ตรวจสอบให้แน่ใจว่าทุกฟิลด์ถูกกรอก
            toast.error("Email and Password are required.");
            setIsLoading(false);
            return;
        }

        try {
            // ส่งคำขอ Login ไปยัง Backend
            const response = await axios.post(
                "/api/Auth/login",
                { email, password, branchId }, // รวม branchId ไป
                {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    },
                    withCredentials: true 
                }
            );

            const { role } = response.data; 
            const token = response.data.token;
            Cookies.set("authToken", token, { expires: 1, secure: true, sameSite: "Strict" });
            Cookies.set("branchId", response.data.branchId, { expires: 1, secure: true, sameSite: "Strict" });

            // นำทางตาม Role
            if (role === "Admin") {
                navigate("/select-branch");

            } else if (role === "Manager" || role === "Employee") {
                const decodedToken = jwtDecode(token);
                const emailFromToken = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || 'No email found';
                const roleFromToken = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || 'No role found';
                navigate(`/${branchId}/sale`); 

            } else {
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
                        type="text"
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
                    <input
                        type="text" 
                        placeholder="Branch ID" 
                        value={branchId}
                        onChange={(e) => setBranchId(e.target.value)}
                        className="form-input"
                    />
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