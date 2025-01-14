import React, { useEffect, useState } from "react";
import axios from "axios";
import Cookies from "js-cookie";
import { useParams } from "react-router-dom";
import "../../styles/employee.css"; // ปรับให้ตรงกับสไตล์ที่คุณต้องการ
import { useNavigate } from "react-router-dom";

const EmployeeDetail = () => {
    const { employeeId } = useParams(); // ดึง Employee ID จาก URL
    const [employee, setEmployee] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const navigate = useNavigate();

    // ฟังก์ชันดึงข้อมูล Employee
    useEffect(() => {
        const fetchEmployeeDetails = async () => {
            const token = Cookies.get("authToken"); // ดึง Token จาก Cookie

            try {
                const response = await axios.get(`https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/Employee/branches/${branchId}/employees/${employeeId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                if (response.data.success) {
                    setEmployee(response.data.data);
                } else {
                    alert(response.data.message || "Failed to fetch employee details.");
                }
            } catch (error) {
                console.error("Failed to fetch employee details:", error);
                alert(error.response ? error.response.data.message : "Failed to load employee details.");
            } finally {
                setIsLoading(false);
            }
        };

        fetchEmployeeDetails();
    }, [employeeId]);

    if (isLoading) {
        return <p>Loading...</p>;
    }

    if (!employee) {
        return <p>Employee not found.</p>;
    }

    return (
        <div className="employee-detail-container">
            <h2>{employee.firstName} {employee.lastName}</h2>
            <p>Email: {employee.email}</p>
            <p>Position: {employee.roles}</p>
            {/* เพิ่มข้อมูลเพิ่มเติมที่ต้องการแสดง */}
            <button onClick={() => navigate(-1)}>Back</button>
        </div>
    );
};

export default EmployeeDetail;