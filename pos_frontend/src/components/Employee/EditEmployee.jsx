import { useEffect, useState } from "react";
import "../../styles/employee.css";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import Cookies from "js-cookie";
import { toast } from "react-toastify";

const EditEmployee = () => {
    const navigate = useNavigate();
    const { employeeId, branchId } = useParams(); // ดึง Employee ID จาก URL
    // const branchId = new URLSearchParams(window.location.search).get("branch"); // ดึง Branch ID จาก URL

    // ตรวจสอบการกำหนดค่าคงที่ใหม่
    const existingSalt = "";  // ต้องมีการกำหนดค่า
    const existingPasswordHash = ""; // ต้องมีการกำหนดค่า

    const [formData, setFormData] = useState({
        firstName: "",
        lastName: "",
        email: "",
    });

    const [roles, setRoles] = useState([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (!employeeId || !branchId) {
            toast.error("Employee ID or Branch ID is missing.");

            navigate(`/${branchId}/EmployeeList`);
            return;
        }

        const token = Cookies.get("authToken");

        const fetchEmployee = async () => {
            setIsLoading(true);
            try {
                const response = await axios.get(`/api/Employee//branches/${branchId}/employees/${employeeId}`, {
                    headers: {
                        Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                    },
                    withCredentials: true,
                });
        
                // console.log(response); // เพิ่มบรรทัดนี้เพื่อตรวจสอบข้อมูล
        
                // ตรวจสอบว่า response.data มีข้อมูลตามที่เราต้องการหรือไม่
                if (response.status === 200 && response.data) {
                    setFormData({
                        firstName: response.data.firstName,
                        lastName: response.data.lastName,
                        email: response.data.email,
                        id: response.data.id, // เก็บ ID ที่ถูกต้อง
                    });
                    setRoles(response.data.roles); // เก็บ roles ไว้
                }
            } catch (error) {
                toast.error("Failed to fetch employee details.");
                // console.error('Error fetching employee details:', error);
                navigate(`/${branchId}/EmployeeList`);
            } finally {
                setIsLoading(false);
            }
        };

        fetchEmployee();
    }, [employeeId, branchId, navigate]);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
    
        // ตรวจสอบให้แน่ใจว่าฟิลด์ที่สำคัญครบถ้วน
        if (!formData.firstName || !formData.lastName || !formData.email) {
            toast.error("Please fill out all fields!");
            return;
        }
    
        setIsLoading(true);
        const token = Cookies.get("authToken");
    
        // เตรียม object ที่จะส่งไป
        const employeeData = {
            id: formData.id,  // ใช้ id ที่ถูกต้อง
            firstName: formData.firstName,
            lastName: formData.lastName,
            email: formData.email,
            roles: roles, // ใช้ roles ที่ดึงมาจาก API
            branchId: branchId,
            // ควรรวม salt และ passwordHash หากจำเป็น
            salt: existingSalt,  // ดึงค่าจากที่เก็บข้อมูล
            passwordHash: existingPasswordHash // ดึงค่าจากที่เก็บข้อมูล
        };
    
        // console.log('Employee Data:', employeeData); // ดูข้อมูลก่อนส่ง
    
        try {
            const response = await axios.put(`/api/Employee/branches/${branchId}/employees/${employeeId}`, employeeData, {
                headers: {
                    Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                },
                withCredentials: true,
            });
    
            if (response.status === 200) {
                toast.success("Employee updated successfully!");

                navigate(`/${branchId}/EmployeeList`);
            }
        } catch (error) {
            toast.error("Failed to update employee.");
            // console.error("Error updating employee:", error.response ? error.response.data : error.message);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="edit-employee-container">
            <h2>Edit Employee</h2>
            {isLoading ? (
                <p>Loading...</p>
            ) : (
                <form onSubmit={handleSubmit} className="edit-employee-form">
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
                        <button type="button" onClick={() => navigate(`/${branchId}/EmployeeList`)} className="cancel-button">
                            Cancel
                        </button>
                        <button type="submit" className="save-button">
                            {isLoading ? "Saving..." : "Save"}
                        </button>
                    </div>
                </form>
            )}
        </div>
    );
};

export default EditEmployee;