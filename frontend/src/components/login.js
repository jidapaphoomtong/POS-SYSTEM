// import React, { useState } from "react";
// import logo from "../Images/food.png";
// import admin from "../Images/Admin.png";
// import axios from "axios";
// import { toast } from "react-toastify";
// import { Link, useNavigate } from "react-router-dom";

// const Login = () => {
//     const [email, setEmail] = useState(""); // เพิ่ม missing state สำหรับ email
//     const [password, setPassword] = useState("");
//     const [isLoading, setIsLoading] = useState(false);
//     const navigate = useNavigate();

//     const handleLogin = async (e) => {
//         e.preventDefault();
//         setIsLoading(true);

//         try {
//             const response = await axios.post("http://localhost:5293/api/Auth/login", { email, password });
//             // const response = await axios.post(
//             //     `${process.env.REACT_APP_BASE_API_URL}/api/Auth/login`,
//             //     { email, password },
//             //     {
//             //         headers: {
//             //             "Content-Type": "application/json",
//             //         },
//             //     }
//             // );

//             // แสดง Token ใน Console เฉพาะโหมด Development
//             if (process.env.NODE_ENV === "development") {
//                 console.log("JWT Token:", response.data.Token);
//             }

//             // เก็บ Token ใน localStorage
//             localStorage.setItem("authToken", response.data.Token);

//             // Decode JWT เพื่อเช็ค Role
//             const decodedToken = JSON.parse(atob(response.data.Token.split(".")[1])); // Decode Payload
//             const userRole = decodedToken.role;

//             // นำทางตาม Role
//             if (userRole === "admin") {
//                 navigate("/select-branch");
//             } else if (userRole === "manager" || userRole === "employee") {
//                 navigate("/sale");
//             } else {
//                 toast.error("Invalid role! Unable to identify user.");
//             }

//             toast.success("Login successful! 🎉");
//         } catch (error) {
//             toast.error(error.response?.data?.Message || "Unable to login. Please try again.");
//         } finally {
//             setIsLoading(false);
//         }
//     };

//     return (
//         <div className="login-register-container">
//             <div className="logo">
//                 <img src={logo} alt="App Logo" />
//             </div>
//             <div className="form-container">
//                 <img src={admin} className="avatar" alt="Admin Avatar" />
//                 <form onSubmit={handleLogin}>
//                     <input
//                         type="text-form"
//                         placeholder="Email..."
//                         value={email}
//                         onChange={(e) => setEmail(e.target.value)}
//                         required
//                     />
//                     <input
//                         type="text-form"
//                         placeholder="Password..."
//                         value={password}
//                         onChange={(e) => setPassword(e.target.value)}
//                         required
//                     />
//                     <button type="submit" className="btn" disabled={isLoading}>
//                         {isLoading ? "Processing..." : "Login"}
//                     </button>
//                 </form>
//                 <p>
//                     Don't have an account? <Link to="/register">Register here</Link>
//                 </p>
//             </div>
//         </div>
//     );
// };

// export default Login;

import React, { useState } from "react";
import logo from "../Images/food.png";
import admin from "../Images/Admin.png";
import axios from "axios";
import { toast } from "react-toastify";
import { Link, useNavigate } from "react-router-dom";

const Login = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleLogin = async (e) => {
        e.preventDefault();
        setIsLoading(true);

        try {
        // เรียก API เพื่อเข้าสู่ระบบ
        const response = await axios.post("http://localhost:5293/api/Auth/login", {
            email,
            password,
        });

        // แสดง Token ใน Console (เฉพาะโหมดพัฒนา)
        if (process.env.NODE_ENV === "development") {
            console.log("JWT Token:", response.data.token);
        }

        // เก็บ JWT Token ใน LocalStorage
        localStorage.setItem("authToken", response.data.token);

        // แจ้งเตือนสำเร็จ
        toast.success("Login successful! 🎉");

        // นำทางไปหน้า Dashboard หรือ Sale
        navigate("/select-branch");
        } catch (error) {
        console.error(error.response?.data || error.message);
        toast.error(
            error.response?.data?.message || "Unable to login. Please try again."
        );
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
                placeholder="Email..."
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
            />
            <input
                type="text-form"
                placeholder="Password..."
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
            />
            <button type="submit" className="btn" disabled={isLoading}>
                {isLoading ? "Processing..." : "Login"}
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