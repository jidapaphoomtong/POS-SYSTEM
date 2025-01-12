// import axios from "axios";
// import { useEffect, useState } from "react";
// import EditEmployee from "./EditEmployee";
// import ConfirmationModal from "./ConfirmationModal";
// import { useNavigate } from "react-router-dom";
// import "../../styles/employee.css";
// import Cookies from "js-cookie";

// const EmployeeList = () => {
//     const [employees, setEmployees] = useState([]); 
//     const navigate = useNavigate();
//     const [isLoading, setIsLoading] = useState(false);
//     const [editEmployee, setEditEmployee] = useState(null);
//     const [deleteEmployeeId, setDeleteEmployeeId] = useState(null);
//     const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);

//     // Fetch Employee List
//     useEffect(() => {
//         const fetchEmployees = async () => {
//             const token = Cookies.get("authToken");
//             if (!token) {
//                 alert("Your session has expired. Please login again.");
//                 navigate("/");
//                 return;
//             }

//             try {
//                 setIsLoading(true);
//                 const response = await axios.get(`http://localhost:5293/api/Employee/employees`, {
//                     headers: {
//                         "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
//                         Authorization: `Bearer ${token}`,
//                     },
//                     withCredentials: true,
//                 });

//                 setEmployees(response.data.data || []);
//             } catch (error) {
//                 console.error("Failed to fetch employees:", error);
//                 if (error.response?.status === 401) {
//                     alert("Unauthorized. Please login.");
//                     navigate("/");
//                 } else {
//                     alert("Failed to fetch employees.");
//                 }
//             } finally {
//                 setIsLoading(false);
//             }
//         };

//         fetchEmployees();
//     }, [navigate]);

//     const handleEditEmployee = (updatedEmployee) => {
//         setEmployees(
//             employees.map((employee) =>
//                 employee.id === updatedEmployee.id ? { ...employee, ...updatedEmployee } : employee
//             )
//         );
//     };

//     const handleOpenDeleteModal = (id) => {
//         setDeleteEmployeeId(id);
//         setIsConfirmModalOpen(true);
//     };

//     const handleCloseDeleteModal = () => {
//         setDeleteEmployeeId(null);
//         setIsConfirmModalOpen(false);
//     };

//     const handleConfirmDelete = async () => {
//         const token = Cookies.get("authToken");

//         try {
//             const response = await axios.delete(`http://localhost:5293/api/Employee/employees/${deleteEmployeeId}`, {
//                 headers: {
//                     "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
//                     Authorization: `Bearer ${token}`,
//                 },
//                 withCredentials: true,
//             });

//             if (response.status === 200) {
//                 alert("Employee deleted successfully!");
//                 setEmployees(employees.filter((employee) => employee.id !== deleteEmployeeId));
//                 handleCloseDeleteModal();
//             } else {
//                 alert("Failed to delete employee.");
//             }
//         } catch (error) {
//             console.error("Failed to delete employee:", error);
//             alert("Failed to delete employee: " + (error.response?.data?.message || "Unknown error"));
//         }
//     };

//     return (
//         <div className="employee-container">
//             <div className="header">
//                 <h1>Employee Management</h1>
//                 <button
//                     className="add-button"
//                     onClick={() => navigate('/add-employee')} // นำทางไปยัง AddEmployee
//                     disabled={isLoading}
//                 >
//                     Add Employee
//                 </button>
//             </div>

//             {isLoading ? (
//                 <p>Loading employees...</p>
//             ) : (
//                 <table className="employee-table">
//                     <thead>
//                         <tr>
//                             <th>Detail</th>
//                             <th>Employee ID</th>
//                             <th>First Name</th>
//                             <th>Last Name</th>
//                             <th>Email</th>
//                             <th>Actions</th>
//                         </tr>
//                     </thead>
//                     <tbody>
//                         {employees.map(({ id, firstName, lastName, email }) => (
//                             <tr key={id}>
//                                 <td>
//                                     <a href={`/employee/${id}`} className="detail-link">Detail</a>
//                                 </td>
//                                 <td>{id}</td>
//                                 <td>{firstName}</td>
//                                 <td>{lastName}</td>
//                                 <td>{email}</td>
//                                 <td className="action-buttons">
//                                     <button
//                                         className="edit-button"
//                                         onClick={() => {
//                                             setEditEmployee({ id, firstName, lastName, email });
//                                             navigate(`/edit-employee/${id}`);
//                                         }}
//                                     >
//                                         ✏️
//                                     </button>
//                                     <button
//                                         className="delete-button"
//                                         onClick={() => handleOpenDeleteModal(id)}
//                                     >
//                                         🗑️
//                                     </button>
//                                 </td>
//                             </tr>
//                         ))}
//                     </tbody>
//                 </table>
//             )}

//             <button
//                 className="back-button"
//                 onClick={() => navigate("/select-branch")}
//             >
//                 Back
//             </button>

//             {isConfirmModalOpen && (
//                 <ConfirmationModal
//                     isOpen={isConfirmModalOpen}
//                     onClose={handleCloseDeleteModal}
//                     onConfirm={handleConfirmDelete}
//                     message="Are you sure you want to delete this employee?"
//                 />
//             )}

//             {editEmployee && (
//                 <EditEmployee
//                     employeeId={editEmployee.id}
//                     onEdit={handleEditEmployee} // ส่งฟังก์ชันแก้ไขไปยัง EditEmployee
//                 />
//             )}
//         </div>
//     );
// };

// export default EmployeeList;