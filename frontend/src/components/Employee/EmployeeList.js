import axios from "axios";
import { useEffect, useState } from "react";
import EditEmployee from "./EditEmployee";
import ConfirmationModal from "./ConfirmationModal";
import { useNavigate } from "react-router-dom";
import "../../styles/employee.css";
import Cookies from "js-cookie";
import Navbar from "../bar/Navbar";
import Sidebar from "../bar/Sidebar";

const EmployeeList = () => {
    const [employees, setEmployees] = useState([]);
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [editEmployee, setEditEmployee] = useState(null);
    const [deleteEmployeeId, setDeleteEmployeeId] = useState(null);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);

    useEffect(() => {
        const fetchEmployees = async () => {
            const token = Cookies.get("authToken");
            const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");

            if (!branchId) {
                alert("Branch ID is missing!");
                return;
            }

            setIsLoading(true);
            try {
                const response = await axios.get(`/api/Employee/branches/${branchId}/employees`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                // console.log(response);

                if (response.data.success) {
                    // ปรับโครงสร้างเพื่อให้ดึงข้อมูลในรูปแบบที่เหมาะสม
                    const employeeData = response.data.data.map(emp => ({
                        id: emp.id,
                        ...emp.data // เข้าถึงข้อมูลใน "data"
                    }));
                    setEmployees(employeeData);
                } else {
                    alert(response.data.message);
                }
            } catch (error) {
                console.error("Failed to fetch employees:", error);
                alert("Failed to fetch employees!");
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
        
        const handleOpenDeleteModal = (id) => {
            setDeleteEmployeeId(id);
            setIsConfirmModalOpen(true);
        };
        
        const handleCloseDeleteModal = () => {
            setDeleteEmployeeId(null);
            setIsConfirmModalOpen(false);
        };
        
        const handleConfirmDelete = async () => {
            const token = Cookies.get("authToken");
            
            try {
                const response = await axios.delete(`/api/Employee/branches/${branchId}/employees/${deleteEmployeeId}`, { // ตรวจสอบ URL นี้
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });
                
                if (response.status === 200) {
                    alert("Employee deleted successfully!");
                    setEmployees((prevEmployees) => prevEmployees.filter((employee) => employee.id !== deleteEmployeeId));
                    handleCloseDeleteModal();
                } else {
                    alert("Failed to delete employee.");
                }
            } catch (error) {
                console.error("Failed to delete employee:", error);
                alert("Failed to delete employee: " + (error.response?.data?.message || "Unknown error"));
            }
        };
        
        // Extract branchId for navigation
        const branchId = new URLSearchParams(window.location.search).get("branch");

    return (
        <div className="employee-container">
            <Navbar />
            <div className="content">
                <Sidebar />
                <div className="main-content">
                    <div className="header-section">
                        <div className="header-title">
                            <h2>STAFF ({employees.length})</h2>
                            <button
                                className="add-button"
                                onClick={() => navigate(`/add-employee?branch=${branchId}`)}
                                disabled={isLoading}
                                style={{ marginLeft: 'auto' }}
                            >
                                Add Staff
                            </button>
                        </div>
                    </div>

                    {isLoading ? (
                        <p>Loading employees...</p>
                    ) : (
                        <table className="employee-table">
                            <thead>
                                <tr>
                                    {/* <th>Detail</th> */}
                                    <th>Employee ID</th>
                                    <th>First Name</th>
                                    <th>Last Name</th>
                                    <th>Email</th>
                                    <th>Role</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {employees.map(({ id, firstName, lastName, email, roles }) => (
                                    <tr key={id}>
                                        <td>{id}</td>
                                        <td>{firstName}</td>
                                        <td>{lastName}</td>
                                        <td>{email}</td>
                                        <td>{roles.map(role => role.Name).join(', ')}</td>
                                        <td className="action-buttons">
                                            <button
                                                className="edit-button"
                                                onClick={() => {
                                                    // ส่งไปที่หน้า EditEmployee พร้อมกับ employeeId และ branchId
                                                    navigate(`/edit-employee/${id}?branch=${branchId}`);
                                                }}
                                            >
                                                ✏️
                                            </button>
                                            <button
                                                className="delete-button"
                                                onClick={() => handleOpenDeleteModal(id)}
                                            >
                                                🗑️
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}

                    {isConfirmModalOpen && (
                        <ConfirmationModal
                            isOpen={isConfirmModalOpen}
                            onClose={handleCloseDeleteModal}
                            onConfirm={handleConfirmDelete}
                            message="Are you sure you want to delete this employee?"
                        />
                    )}

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