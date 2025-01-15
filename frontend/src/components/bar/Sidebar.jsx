import React from "react";
import "../../styles/Sidebar.css";
import { FaUsers, FaTh, FaBox, FaCalendarAlt, FaBuilding, FaSignOutAlt } from "react-icons/fa";
import { AiFillProduct } from "react-icons/ai";
import { useNavigate } from "react-router-dom";

const Sidebar = () => {
    const navigate = useNavigate();

    // Extract branchId from the URL
    const branchId = new URLSearchParams(window.location.search).get("branch");

    const handleLogout = () => {
        navigate('/'); // Return to Login page
    };

    const handleSale = () => {
        navigate(`/sale?branch=${branchId}`);
    };

    const handleStaff = () => {
        navigate(`/EmployeeList?branch=${branchId}`);
    };

    const handleDashboard = () => {
        navigate(`/?branch=${branchId}`);
    };

    const handleStock = () => {
        navigate(`/?branch=${branchId}`);
    };

    const handleHistory = () => {
        navigate(`/?branch=${branchId}`);
    };

    const handleDepartment = () => {
        navigate(`/BranchList?branch=${branchId}`);
    };

    return (
        <div className="sidebar">
            <div className="sidebar-container">
                {/* Menu */}
                <div className="sidebar-item">
                    <AiFillProduct onClick={handleSale} className="sidebar-icon" />
                    <span>Sale</span>
                </div>

                {/* Staff */}
                <div className="sidebar-item">
                    <FaUsers onClick={handleStaff} className="sidebar-icon" />
                    <span>Staff</span>
                </div>

                {/* Dashboard */}
                <div className="sidebar-item">
                    <FaTh onClick={handleDashboard} className="sidebar-icon" />
                    <span>Dashboard</span>
                </div>

                {/* Stock */}
                <div className="sidebar-item">
                    <FaBox onClick={handleStock} className="sidebar-icon" />
                    <span>Stock</span>
                </div>

                {/* History */}
                <div className="sidebar-item">
                    <FaCalendarAlt onClick={handleHistory} className="sidebar-icon" />
                    <span>History</span>
                </div>

                {/* Department */}
                <div className="sidebar-item">
                    <FaBuilding onClick={handleDepartment} className="sidebar-icon" />
                    <span>Department</span>
                </div>

                {/* Logout */}
                <div className="sidebar-item logout">
                    <FaSignOutAlt onClick={handleLogout} className="sidebar-icon" />
                    <span className="sidebar-logout">Logout</span>
                </div>
            </div>
        </div>
    );
};

export default Sidebar;

// import React from "react";
// import "../../styles/Sidebar.css";
// import { FaBars, FaUsers, FaTh, FaBox, FaCalendarAlt, FaBuilding, FaSignOutAlt } from "react-icons/fa";
// import { AiFillProduct } from "react-icons/ai";
// import { useNavigate } from "react-router-dom";
// import Cookies from "js-cookie";

// const Sidebar = () => {
//     const navigate = useNavigate();

//     // Extract branchId from the URL
//     const branchId = new URLSearchParams(window.location.search).get("branch");

//     // Get user role from the token in cookies
//     const token = Cookies.get("authToken");
//     let userRole = "employee"; // Default role
//     if (token) {
//         const decodedToken = JSON.parse(atob(token.split('.')[1])); // Decode JWT to get the payload
//         userRole = decodedToken.role; // Get the role from the decoded token
//     }

//     const handleLogout = () => {
//         navigate('/'); // Return to Login page
//     };

//     const handleSale = () => {
//         navigate(`/sale?branch=${branchId}`);
//     };

//     const handleStaff = () => {
//         navigate(`/EmployeeList?branch=${branchId}`);
//     };

//     const handleDashboard = () => {
//         navigate(`/?branch=${branchId}`);
//     };

//     const handleStock = () => {
//         navigate(`/?branch=${branchId}`);
//     };

//     const handleHistory = () => {
//         navigate(`/?branch=${branchId}`);
//     };

//     const handleDepartment = () => {
//         navigate(`/BranchList?branch=${branchId}`);
//     };

//     return (
//         <div className="sidebar">
//             <div className="sidebar-container">
//                 {/* Sale Menu Item */}
//                 {userRole === "admin" || userRole === "manager" || userRole === "employee" ? (
//                     <div className="sidebar-item">
//                         <AiFillProduct onClick={handleSale} className="sidebar-icon" />
//                         <span>Sale</span>
//                     </div>
//                 ) : null}

//                 {/* Staff Menu Item */}
//                 {userRole === "admin" ? (
//                     <div className="sidebar-item">
//                         <FaUsers onClick={handleStaff} className="sidebar-icon" />
//                         <span>Staff</span>
//                     </div>
//                 ) : null}

//                 {/* Dashboard Menu Item */}
//                 {userRole === "admin" || userRole === "manager" || userRole === "employee" ? (
//                     <div className="sidebar-item">
//                         <FaTh onClick={handleDashboard} className="sidebar-icon" />
//                         <span>Dashboard</span>
//                     </div>
//                 ) : null}

//                 {/* Stock Menu Item */}
//                 {userRole === "admin" || userRole === "manager" || userRole === "employee" ? (
//                     <div className="sidebar-item">
//                         <FaBox onClick={handleStock} className="sidebar-icon" />
//                         <span>Stock</span>
//                     </div>
//                 ) : null}

//                 {/* History Menu Item */}
//                 {userRole === "admin" || userRole === "manager" || userRole === "employee" ? (
//                     <div className="sidebar-item">
//                         <FaCalendarAlt onClick={handleHistory} className="sidebar-icon" />
//                         <span>History</span>
//                     </div>
//                 ) : null}

//                 {/* Department Menu Item */}
//                 {userRole === "admin" ? (
//                     <div className="sidebar-item">
//                         <FaBuilding onClick={handleDepartment} className="sidebar-icon" />
//                         <span>Department</span>
//                     </div>
//                 ) : null}

//                 {/* Logout Menu Item */}
//                 <div className="sidebar-item logout">
//                     <FaSignOutAlt onClick={handleLogout} className="sidebar-icon" />
//                     <span className="sidebar-logout">Logout</span>
//                 </div>
//             </div>
//         </div>
//     );
// };

// export default Sidebar;