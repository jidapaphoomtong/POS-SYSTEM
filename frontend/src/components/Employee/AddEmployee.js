import React, { useState } from "react";
import axios from "axios";
import "../../styles/employee.css";
import Cookies from "js-cookie";
import { useNavigate } from "react-router-dom";

const AddEmployee = () => {
    const navigate = useNavigate();
    const branchId = new URLSearchParams(window.location.search).get("branch"); // ดึง branchId จาก URL

    const [formData, setFormData] = useState({
        firstName: "",
        lastName: "",
        email: ""
    });

    const [isLoading, setIsLoading] = useState(false);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        const branchId = new URLSearchParams(window.location.search).get("branch"); // Ensure branchId is set

        if (!formData.firstName || !formData.lastName || !formData.email) {
            alert("Please fill out all fields!");
            return;
        }

        setIsLoading(true);

        try {
            const token = Cookies.get("authToken");
            if (!token) {
                alert("No token found. Please log in again.");
                return;
            }

            const response = await axios.post(`https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/Employee/add-employee`, {
                ...formData,
                branchId: branchId, // ส่ง branchId
            }, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json",
                },
                withCredentials: true,
            });

            if (response.status === 200) {
                alert("Employee added successfully!");
                navigate(`/EmployeeList?branch=${branchId}`); // นำทางกลับไปที่ EmployeeList
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
                <div className="form-buttons">
                    <button type="button" onClick={() => navigate(`/EmployeeList?branch=${branchId}`)} className="cancel-button">
                        Cancel
                    </button>
                    <button type="submit" className="save-button">
                        {isLoading ? "Saving..." : "Save"}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default AddEmployee;