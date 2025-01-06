// import React, { useState } from "react";
// import logo from "../Images/food.png";
// import admin from "../Images/Admin.png";
// import axios from "axios";
// import { toast } from "react-toastify";
// import { Link, useNavigate } from "react-router-dom";
// import Cookies from "js-cookie";

// const Login = () => {
//     const [email, setEmail] = useState("");
//     const [password, setPassword] = useState("");
//     const [isLoading, setIsLoading] = useState(false);
//     const [showPassword, setShowPassword] = useState(false);
//     const navigate = useNavigate();

//     const handleLogin = async (e) => {
//         e.preventDefault();
//         setIsLoading(true);

//         if (!email || !password) {
//             toast.error("Email and Password are required.");
//             setIsLoading(false);
//             return;
//         }

//         try {
//             const response = await axios.post("http://localhost:5293/api/Auth/login", {
//                 email: email,
//                 password: password,

//             });
    
//             const getCookie = (name) => {
//                 const value = `; ${document.cookie}`; // โหลด Cookie ทั้งหมด
//                 const parts = value.split(`; ${name}=`); // แยก Cookie ด้วย ;
//                 if (parts.length === 2) return parts.pop().split(";").shift(); // ดึงค่าที่ต้องการ
//             };

//             const token = getCookie("authToken");
//             console.log("JWT Token from Cookie:", token);
    
//             // เก็บ JWT Token ลงใน Cookie
//             Cookies.set("PosAppCookie", token, { expires: 1, secure: true, sameSite: "Strict" });

//             // แจ้งเตือนสำเร็จ
//             toast.success("Login successful! 🎉");

//             // นำทางไปหน้าอื่น
//             navigate("/select-branch");
//         } catch (error) {
//             console.error(error);
//             const status = error.response?.status || 500;
//             if (status === 400) toast.error("Invalid request. Please check your input.");
//             else if (status === 401) toast.error("Invalid login credentials.");
//             else toast.error("Unable to login. Please try again.");
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
//                 {/* ช่อง Email */}
//                     <input
//                         type="text-form"
//                         placeholder="Email..."
//                         value={email}
//                         onChange={(e) => setEmail(e.target.value)}
//                         required
//                         className="form-input"
//                     />

//                     {/* ช่อง Password */}
//                     <div className="input-group">
//                         <input
//                             type={showPassword ? "text" : "password"} // ซ่อนหรือแสดงรหัสผ่าน
//                             placeholder="Password..."
//                             value={password}
//                             onChange={(e) => setPassword(e.target.value)}
//                             required
//                             className="form-input"
//                         />
//                         <button
//                             type="button"
//                             onClick={() => setShowPassword(!showPassword)} // สลับสถานะ Show/Hide
//                             className="show-password-btn"
//                         >
//                             {showPassword ? "Hide" : "Show"}
//                         </button>
//                     </div>
//                     <button type="submit" className="btn" disabled={isLoading}>
//                         {isLoading ? <div className="spinner"></div> : "Login"}
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
                { withCredentials: true }
            );

            const { role, message } = response.data; // ดึง Role จาก Response
            
            // แจ้งเตือนสำเร็จ
            toast.success(message);

            // นำทางผู้ใช้ตาม Role
            if (role === "Admin") {
                navigate("/select-branch"); // Admin ไปหน้า Select Branch
            } else if (role === "Manager" || role === "Employee") {
                navigate("/sale"); // Manager และ Employee ไปหน้า Sale
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