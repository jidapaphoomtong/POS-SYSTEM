import React, { useState } from "react";
import axios from "axios";
import logo from "../Images/food.png";
import admin from "../Images/Admin.png";
import { toast } from "react-toastify";
import { Link } from "react-router-dom";

const Register = () => {
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    

    const handleRegister = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        try {
        const response = await axios.post(
            "https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/Auth/register",
            {
            firstName, // แยก Firstname
            lastName, // แยก Lastname หรือกำหนด Default เป็นค่าว่าง
            email,
            password,
            },
            {
            headers: {
                "Content-Type": "application/json",
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
    };

    return (
        <div className="login-register-container">
        <div className="logo">
            <img src={logo} alt="Logo" />
        </div>
        <div className="form-container">
            <img src={admin} className="avatar" alt="Admin Avatar" />
            <form onSubmit={handleRegister}>
            <input
                type="text-form"
                placeholder="Full Name... (e.g. John Doe)"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                required
            />
            <input
                type="text-form"
                placeholder="Full Name... (e.g. John Doe)"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                required
            />
            <input
                type="text-form"
                placeholder="Email Address..."
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
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
                {isLoading ? "Processing..." : "Register"}
            </button>
            </form>
            <p>
            Already have an account? <Link to="/">Login here</Link>
            </p>
        </div>
        </div>
    );
};

export default Register;