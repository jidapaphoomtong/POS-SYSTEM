import React, { useState } from "react";
import axios from "axios";
import "../../styles/employee.css";
import Cookies from "js-cookie";
import { useParams, useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

const AddEmployee = () => {
    const token = Cookies.get("authToken");
    const { branchId } = useParams(); // useParams() จาก react-router-dom
    // console.log("Branch ID:", branchId); // เพิ่มการล็อกเพื่อตรวจสอบค่า

    const [formData, setFormData] = useState({
        firstName: "",
        lastName: "",
        email: "",
        passwordHash: "",
        salt : "",
    });

    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
    
        // ตรวจสอบข้อมูลก่อนส่ง
        if (!formData.firstName || !formData.lastName || !formData.email || !formData.passwordHash) {
            toast.error("Please fill out all fields!");
            return;
        }
    
        setIsLoading(true);
        console.log("Sending Form Data:", { ...formData, branchId }); // สำหรับตรวจสอบการส่ง
    
        try {
            if (!token) {
                toast.error("No token found. Please log in again.");
                return;
            }
            
            // แก้ไข URL ให้ถูกต้อง
            const response = await axios.post(`/api/Employee/add-employee/${branchId}`, {
                ...formData,
                branchId,
                salt: formData.salt,
            }, {
                headers: {
                    Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                },
                withCredentials: true,
            });
    
            if (response.status === 200) {
                toast.success("Employee added successfully!");

                navigate(`/${branchId}/EmployeeList`);
            } else {
                toast.error(`Request failed with status: ${response.status}`);
            }
        } catch (error) {
            // console.error("Error adding employee:", error);
            if (error.response) {
                // console.error("Response data:", error.response.data);
                toast.error(error.response.data?.Message || "Failed to add employee.");
            } else if (error.request) {
                toast.error("No response from server. Please try again later.");
            } else {
                toast.error("Error: " + error.message);
            }
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="add-employee-container">
            <h2>Add Employee</h2>
            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    name="firstName"
                    placeholder="First Name"
                    value={formData.firstName}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="lastName"
                    placeholder="Last Name"
                    value={formData.lastName}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="email"
                    placeholder="Email"
                    value={formData.email}
                    onChange={handleChange}
                    required
                />
                <input
                    type="password"
                    name="passwordHash"
                    placeholder="Password"
                    value={formData.passwordHash}
                    onChange={handleChange}
                    required
                />
                <div className="form-buttons">
                    <button type="button" onClick={() => navigate(`/${branchId}/EmployeeList`)} className="cancel-button">
                        Cancel
                    </button>
                    <button type="submit" disabled={isLoading}>
                        {isLoading ? "Saving..." : "Save"}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default AddEmployee;