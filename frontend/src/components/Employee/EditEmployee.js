// import { useEffect, useState } from "react";
// import "../../styles/employee.css";
// import { useParams } from "react-router-dom";
// import axios from "axios";
// import Cookies from "js-cookie";
// import { useNavigate } from "react-router-dom";

// const EditEmployee = () => {
//     const navigate = useNavigate();
//     const { employeeId } = useParams(); // ดึง Employee ID จาก URL

//     const [formData, setFormData] = useState({
//         firstName: "",
//         lastName: "",
//         email: "",
//         position: "",
//     });
//     const [isLoading, setIsLoading] = useState(false);

//     // ฟังก์ชันโหลดรายละเอียดของ Employee
//     useEffect(() => {
//         if (!employeeId) {
//             alert("Employee ID is missing.");
//             navigate("/EmployeeList");
//             return;
//         }

//         const token = Cookies.get("authToken"); // ดึง Token จาก Cookie

//         const fetchEmployee = async () => {
//             try {
//                 setIsLoading(true);
//                 const response = await axios.get(`http://localhost:5293/api/Employee/employees/${employeeId}`, {
//                     headers: {
//                         "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
//                         Authorization: `Bearer ${token}`,
//                     },
//                     withCredentials: true,
//                 });

//                 if (response.data.success) {
//                     setFormData({
//                         firstName: response.data.data.firstName,
//                         lastName: response.data.data.lastName,
//                         email: response.data.data.email,
//                         position: response.data.data.position,
//                     });
//                 } else {
//                     alert(response.data.message || "Failed to fetch employee details.");
//                     navigate("/EmployeeList");
//                 }
//             } catch (error) {
//                 console.error("Failed to fetch employee details:", error);
//                 alert(error.response ? error.response.data.message : "Failed to load employee details.");
//                 navigate("/EmployeeList");
//             } finally {
//                 setIsLoading(false);
//             }
//         };

//         fetchEmployee();
//     }, [employeeId, navigate]);

//     // ฟังก์ชันจัดการการเปลี่ยนค่าช่อง Input
//     const handleChange = (e) => {
//         setFormData({ ...formData, [e.target.name]: e.target.value });
//     };

//     // ฟังก์ชันจัดการการบันทึกข้อมูล
//     const handleSubmit = async (e) => {
//         e.preventDefault();
//         if (!formData.firstName || !formData.lastName || !formData.email || !formData.position) {
//             alert("Please fill out all fields!");
//             return;
//         }

//         try {
//             setIsLoading(true);
//             const token = Cookies.get("authToken"); // ใช้ Token จาก Cookies
//             const response = await axios.put(`http://localhost:5293/api/Employee/employees/${employeeId}`, formData, {
//                 headers: {
//                     "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
//                     Authorization: `Bearer ${token}`,
//                 },
//                 withCredentials: true,
//             });

//             alert("Employee updated successfully!");
//             navigate("/EmployeeList"); // นำทางกลับไปที่ Employee List
//         } catch (error) {
//             console.error("Failed to update employee:", error);
//             alert(error.response ? error.response.data.message : "Failed to update employee.");
//         } finally {
//             setIsLoading(false);
//         }
//     };

//     return (
//         <div className="edit-employee-container">
//             <h2>Edit Employee</h2>
//             {isLoading ? (
//                 <p>Loading...</p>
//             ) : (
//                 <form onSubmit={handleSubmit}>
//                     <input
//                         type="text"
//                         name="firstName"
//                         placeholder="First Name"
//                         value={formData.firstName}
//                         onChange={handleChange}
//                     />
//                     <input
//                         type="text"
//                         name="lastName"
//                         placeholder="Last Name"
//                         value={formData.lastName}
//                         onChange={handleChange}
//                     />
//                     <input
//                         type="email"
//                         name="email"
//                         placeholder="Email"
//                         value={formData.email}
//                         onChange={handleChange}
//                     />
//                     <input
//                         type="text"
//                         name="position"
//                         placeholder="Position"
//                         value={formData.position}
//                         onChange={handleChange}
//                     />
//                     <div className="form-buttons">
//                         <button type="button" onClick={() => navigate('/EmployeeList')} disabled={isLoading}>
//                             Cancel
//                         </button>
//                         <button type="submit" disabled={isLoading}>
//                             {isLoading ? "Saving..." : "Save"}
//                         </button>
//                     </div>
//                 </form>
//             )}
//         </div>
//     );
// };

// export default EditEmployee;