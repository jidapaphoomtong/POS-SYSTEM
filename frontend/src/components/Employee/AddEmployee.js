import React, { useState } from "react";
import axios from "axios";
import "../../styles/employee.css";
import Cookies from "js-cookie";
import { useNavigate } from "react-router-dom";

const AddEmployee = () => {
    const navigate = useNavigate();
    const branchId = new URLSearchParams(window.location.search).get("branch");

    const [formData, setFormData] = useState({
        firstName: "",
        lastName: "",
        email: "",
        password: "",
    });

    const [isLoading, setIsLoading] = useState(false);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
    
        // ตรวจสอบว่าทุกฟิลด์ถูกกรอกครบถ้วน
        if (!formData.firstName || !formData.lastName || !formData.email || !formData.password) {
            alert("Please fill out all fields!");
            return;
        }
    
        setIsLoading(true);
    
        const token = Cookies.get("authToken");
        if (!token) {
            alert("No token found. Please log in again.");
            setIsLoading(false);
            return;
        }
    
        const employeeData = {
            firstName: formData.firstName,
            lastName: formData.lastName,
            email: formData.email,
            passwordHash: formData.password,
            branchId: branchId,
        };

        console.log(employeeData)
    
        try {
            const response = await axios.post(
                `/api/Employee/add-employee/${branchId}`,
                employeeData, // ส่งข้อมูลพนักงาน
                {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                        "Content-Type": "application/json",
                    },
                    withCredentials: true,
                }
            );
    
            if (response.status === 200) {
                alert("Employee added successfully!");
                navigate(`/EmployeeList?branch=${branchId}`);
            }
        } catch (error) {
            console.error("Error adding employee:", error);
            alert("Failed to add employee: " + (error.response?.data?.message || "Unknown error"));
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="add-employee-container">
            <h2>Add Employee</h2>
            <form onSubmit={handleSubmit} className="add-employee-form">
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
                    name="password" 
                    placeholder="Password" 
                    value={formData.password} 
                    onChange={handleChange} 
                    required 
                />
                <div className="form-buttons">
                    <button 
                        type="button" 
                        onClick={() => navigate(`/EmployeeList?branch=${branchId}`)} 
                        className="cancel-button"
                    >
                        Cancel
                    </button>
                    <button 
                        type="submit" 
                        className="save-button" 
                        disabled={isLoading}
                    >
                        {isLoading ? "Saving..." : "Save"}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default AddEmployee;