
// import React, { useState } from "react";
// import axios from "axios";
// import "../../styles/employee.css";
// import Cookies from "js-cookie";
// import { useNavigate } from "react-router-dom";

// const AddEmployee = () => {
//     const [formData, setFormData] = useState({
//         firstName: "",
//         lastName: "",
//         email: "",
//         position: "",
//     });
    
//     const [isLoading, setIsLoading] = useState(false);
//     const navigate = useNavigate();

//     const handleChange = (e) => {
//         setFormData({ ...formData, [e.target.name]: e.target.value });
//     };

//     const handleSubmit = async (e) => {
//         e.preventDefault();

//         // Validation
//         if (!formData.firstName || !formData.lastName || !formData.email || !formData.position) {
//             alert("Please fill out all fields!");
//             return;
//         }

//         setIsLoading(true);

//         try {
//             const token = Cookies.get("authToken");
//             if (!token) {
//                 alert("No token found. Please log in again.");
//                 return;
//             }
        
//             const response = await axios.post(`http://localhost:5293/api/Employee/add-employee`, formData, {
//                 headers: {
//                     "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
//                     Authorization: `Bearer ${token}`,
//                     "Content-Type": "application/json",
//                 },
//                 withCredentials: true,
//             });
        
//             if (response.status === 200) {
//                 alert("Employee added successfully!");
//                 navigate("/EmployeeList");
//             } else {
//                 alert(`Request failed with status: ${response.status}`);
//             }
//         } catch (error) {
//             console.error("Error adding employee:", error);
//             if (error.response) {
//                 alert(error.response.data?.Message || "Failed to add employee.");
//             } else {
//                 alert("Error: " + error.message);
//             }
//         } finally {
//             setIsLoading(false);
//         }
//     };

//     return (
//         <div className="add-employee-container">
//             <h2>Add Employee</h2>
//             <form onSubmit={handleSubmit}>
//                 <input
//                     type="text"
//                     name="firstName"
//                     placeholder="First Name"
//                     value={formData.firstName}
//                     onChange={handleChange}
//                     required
//                 />
//                 <input
//                     type="text"
//                     name="lastName"
//                     placeholder="Last Name"
//                     value={formData.lastName}
//                     onChange={handleChange}
//                     required
//                 />
//                 <input
//                     type="email"
//                     name="email"
//                     placeholder="Email"
//                     value={formData.email}
//                     onChange={handleChange}
//                     required
//                 />
//                 <input
//                     type="text"
//                     name="position"
//                     placeholder="Position"
//                     value={formData.position}
//                     onChange={handleChange}
//                     required
//                 />
//                 <div className="form-buttons">
//                     <button type="button" onClick={() => navigate('/EmployeeList')} disabled={isLoading}>
//                         Cancel
//                     </button>
//                     <button type="submit" disabled={isLoading}>
//                         {isLoading ? "Saving..." : "Save"}
//                     </button>
//                 </div>
//             </form>
//         </div>
//     );
// };

// export default AddEmployee;