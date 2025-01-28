import axios from "axios";
import { useEffect, useState } from "react";
import EditEmployee from "./EditEmployee";
import ConfirmationModal from "./ConfirmationModal";
import { useNavigate, useParams } from "react-router-dom";
import "../../styles/employee.css";
import Cookies from "js-cookie";
import Navbar from "../bar/Navbar";
import Sidebar from "../bar/Sidebar";
import { FaEye, FaEyeSlash,FaEdit } from "react-icons/fa";
import { FaTrash } from "react-icons/fa6";
import { toast } from "react-toastify";

const EmployeeList = () => {
    const [employees, setEmployees] = useState([]);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 6; // จำนวนสินค้าที่จะแสดงต่อหน้า
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [editEmployee, setEditEmployee] = useState(null);
    const [deleteEmployeeId, setDeleteEmployeeId] = useState(null);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
    const { branchId } = useParams();
    // console.log(branchId);

    useEffect(() => {
        const fetchEmployees = async () => {
            const token = Cookies.get("authToken");
            if (!token) {
                toast.error("Your session has expired. Please login again.");
                navigate("/");
                return;
            }
        
            if (!branchId) {
                toast.error("Branch ID is missing!");
                return;
            }
        
            setIsLoading(true);
            try {
                const response = await axios.get(`/api/Employee/branches/${branchId}/employees`, {
                    headers: {
                        Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                    },
                    withCredentials: true,
                });
        
                if (response.data.success) {
                    const employeeData = response.data.data.map(emp => ({
                        id: emp.id,
                        ...emp.data,
                        employeeStatus: emp.data.status || "active"
                    }));
                    setEmployees(employeeData);
                } else {
                    toast.error(response.data.message);
                }
            } catch (error) {
                if (error.response?.status === 401) {
                    toast.error("Unauthorized. Please login.");
                    navigate("/");
                } else {
                    toast.error("Failed to fetch employees!");
                }
            } finally {
                setIsLoading(false);
            }
        };

        fetchEmployees();
    }, [navigate]);

        const handleEditEmployee = (updatedEmployee) => {
            setEmployees((prevEmployees) =>
                prevEmployees.map((employee) =>
                employee.id === updatedEmployee.id ? { ...employee, ...updatedEmployee } : employee
                )
            );
        };

        // ฟังก์ชันสำหรับการจัดการ pagination
        const handlePageChange = (pageNumber) => {
            setCurrentPage(pageNumber);
        };

        // คำนวณสินค้าที่จะแสดง
        const indexOfLastEmployee = currentPage * itemsPerPage;
        const indexOfFirstEmployee = indexOfLastEmployee - itemsPerPage;
        const currentEmployees = employees.slice(indexOfFirstEmployee, indexOfLastEmployee);

        const totalPages = Math.ceil(employees.length / itemsPerPage);

        const toggleEmployeeStatus = async (id, currentStatus) => {
            const token = Cookies.get("authToken");
            if (!token) {
                toast.error("Your session has expired. Please login again.");
                navigate("/");
                return;
            }
        
            try {
                setIsLoading(true);
                const newStatus = currentStatus === "active" ? "inactive" : "active"; // เปลี่ยนสถานะ
                const response = await axios.put(`/api/Employee/branches/${branchId}/employees/${id}/status`, {
                    status: newStatus
                }, {
                    headers: {
                        Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                    },
                    withCredentials: true,
                });
        
                if (response.status === 200) {
                    toast.success(`Employee status updated to ${newStatus}!`);
                    setEmployees(employees.map(employee => 
                        employee.id === id ? { ...employee, employeeStatus: newStatus } : employee
                    ));
                } else {
                    toast.error("Failed to update employee status.");
                }
            } catch (error) {
                console.error("Failed to update employee status:", error);
                toast.error("Failed to update employee status: " + (error.response?.data?.message || "Unknown error"));
            } finally {
                setIsLoading(false);
            }
        };

    return (
        <div className="employee-container">
            <Navbar />
            <div className="content">
                <Sidebar />
                <div className="main-content">
                <div className="header">
                <h2>STAFF ({employees.length})</h2>
                <button
                    className="add-button"
                    onClick={() => navigate(`/${branchId}/add-employee`)}
                    disabled={isLoading}
                >
                    Add Staff
                </button>
            </div>

                    {isLoading ? (
                        <p>Loading employees...</p>
                    ) : (
                        <table className="employee-table">
                            <thead>
                                <tr>
                                    <th>Employee ID</th>
                                    <th>First Name</th>
                                    <th>Last Name</th>
                                    <th>Email</th>
                                    <th>Position</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                            {currentEmployees.map(({ id, firstName, lastName, email, roles, employeeStatus }) => {
                                return (
                                    <tr key={id}>
                                        <td style={{ textAlign: 'center' }}>
                                            <a href={`/${branchId}/employee/${id}`} className="detail-link">{id}</a>
                                        </td>
                                        <td>{firstName}</td>
                                        <td>{lastName}</td>
                                        <td>{email}</td>
                                        <td>{roles.map(role => role.Name).join(', ')}</td>
                                        <td>
                                            <div className="row-employee">
                                                <button
                                                    className="icon-button"
                                                    onClick={() => navigate(`/${branchId}/edit-employee/${id}`)}
                                                >
                                                    <FaEdit className="icon icon-blue" />
                                                </button>
                                                {employeeStatus === "active" ? (
                                                    <FaEye className="icon-green" onClick={() => toggleEmployeeStatus(id, employeeStatus)} title="Deactivate" />
                                                ) : (
                                                    <FaEyeSlash className="icon-red" onClick={() => toggleEmployeeStatus(id, employeeStatus)} title="Activate" />
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                );
                            })}
                            </tbody>
                        </table>
                    )}

                    <div className="pagination">
                        {Array.from({ length: totalPages }, (_, index) => (
                            <button
                                key={index + 1}
                                onClick={() => handlePageChange(index + 1)}
                                className={currentPage === index + 1 ? 'active' : ''}
                            >
                                {index + 1}
                            </button>
                        ))}
                    </div>

                    {editEmployee && (
                        <EditEmployee
                            employeeId={editEmployee.id}
                            onEdit={handleEditEmployee}
                        />
                    )}
                </div>
            </div>
        </div>
    );
};

export default EmployeeList;