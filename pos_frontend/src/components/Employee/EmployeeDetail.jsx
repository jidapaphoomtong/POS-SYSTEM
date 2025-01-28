import React, { useEffect, useState } from "react";
import axios from "axios";
import Cookies from "js-cookie";
import { useParams } from "react-router-dom";
import { useNavigate } from "react-router-dom";
import "../../styles/employee.css"; 
import { toast } from "react-toastify";

const EmployeeDetail = () => {
    // const branchId = new URLSearchParams(window.location.search).get("branch"); // ดึง Branch ID จาก URL
    const { employeeId, branchId } = useParams(); // ดึง Employee ID และ Branch ID จาก URL
    const [employee, setEmployee] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const navigate = useNavigate();
    // console.log(branchId);

    const [formData, setFormData] = useState({
        firstName: "",
        lastName: "",
        email: "",
        roles:"",
    });

    useEffect(() => {
        const fetchEmployeeDetails = async () => {
            const token = Cookies.get("authToken"); // ดึง Token จาก Cookie

            try {
                const response = await axios.get(`/api/Employee/branches/${branchId}/employees/${employeeId}`, {
                    headers: {
                        Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                    },
                    withCredentials: true,
                });

                // console.log(response)

                if (response.status === 200 && response.data){
                    setFormData({
                        id: response.data.id, // เก็บ ID ที่ถูกต้อง
                        firstName: response.data.firstName,
                        lastName: response.data.lastName,
                        email: response.data.email,
                        roles: response.data.roles.map(role => role.name).join(', ')
                    });
                    // console.log('Roles:', response.data.roles); // ดูเนื้อหาที่ดึงมา
                } else {
                    toast.error(response.data.message || "Failed to fetch employee details.");
                    navigate(`/${branchId}/EmployeeList`);
                }
            } catch (error) {
                // console.error("Failed to fetch employee details:", error);
                toast.error(error.response ? error.response.data.message : "Failed to load employee details.");
            } finally {
                setIsLoading(false);
            }
        };

        fetchEmployeeDetails();
    }, [employeeId, branchId]); // เพิ่ม branchId ใน dependencies

    if (isLoading) {
        return <p>Loading...</p>;
    }

    if (!formData.firstName) { // เช็คค่าที่ได้จาก formData
        return <p>Employee not found.</p>;
    }

    return (
        <div className="employee-detail-container">
            <h2>{formData.id}</h2>
            <p>{formData.firstName} {formData.lastName}</p>
            <p>Email: {formData.email}</p>
            <p>Position: {formData.roles}</p> {/* แสดงตำแหน่งในรูปแบบของสตริง */}
            <button onClick={() => navigate(-1)}>Back</button>
        </div>
    );
};

export default EmployeeDetail;