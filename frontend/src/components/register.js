import React, { useState } from 'react';
import axios from 'axios';
import logo from '../Images/food.png';
import admin from '../Images/Admin.png';
import { toast } from 'react-toastify';
import { Link } from 'react-router-dom';

const Register = () => {
    const [fullName, setFullName] = useState("");
    const [userRole, setUserRole] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [responseMessage, setResponseMessage] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const handleRegister = async (e) => {
        e.preventDefault();

        try {
            const response = await axios.post(
            "http://localhost:5293/api/Auth/register",
            { fullName, userRole, email, password },
            {
                headers: {
                "Content-Type": "application/json", // ระบุ Header สำหรับ JSON
                },
            }
            );
            console.log("Success:", response.data);
            toast.success("Registration Successful! 🎉");
        } catch (error) {
            console.error("Error:", error.response?.data || error.message);
            toast.error(
                error.response?.data?.Message || "Unable to register. Please try again."
            );
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <div className="login-register-container">
        <div className="logo">
            <img src={logo} alt="Logo" />
        </div>
        <div className="form-container">
            <img src={admin} className="avatar" alt="Admin Avatar" />
            <form onSubmit={handleRegister}>
            <input
                type="fullName"
                placeholder="Full Name..."
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                required
            />
            <input
                type="userRole"
                placeholder="Role..."
                value={userRole}
                onChange={(e) => setUserRole(e.target.value)}
                required
            />
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
                {isLoading ? "Processing..." : "Register"}
            </button>
            </form>
            <p>Already have an account? <Link to="/">Login here</Link></p>
        </div>
        </div>
    );
};

export default Register;