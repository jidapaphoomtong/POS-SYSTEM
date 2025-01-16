import React from "react";
import "../../styles/Sidebar.css";
import { FaUsers, FaTh, FaBox, FaCalendarAlt, FaBuilding, FaSignOutAlt } from "react-icons/fa";
import { AiFillProduct } from "react-icons/ai";
import { useNavigate } from "react-router-dom";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";

const Sidebar = () => {
    const navigate = useNavigate();
    
    // Extract token and decode it to get user role
    const token = Cookies.get("authToken");
    // console.log(token)
    let userRole = "";

    if (token) {
        try {
            const decodedToken = jwtDecode(token);
            const roleFromToken = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || 'No role found';
            userRole = roleFromToken; // Adjust according to your JWT structure
            // console.log(userRole);
        } catch (error) {
            console.error("Invalid token:", error);
            alert("Have Something wrong")
        }
    }

    const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");
    
    if (!branchId) {
        alert("Branch ID is missing!");
        return null; // Return null to avoid rendering if Branch ID is missing
    }
    
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
        navigate(`/`);
    };

    const handleStock = () => {
        navigate(`/ProductList?branch=${branchId}`);
    };

    const handleHistory = () => {
        navigate(`/`);
    };

    const handleDepartment = () => {
        navigate(`/BranchList?branch=${branchId}`);
    };

    return (
        <div className="sidebar">
            <div className="sidebar-container">
                {/* Sale Menu Item */}
                {userRole === "Admin" || userRole === "Manager" || userRole === "Employee" ? (
                    <div className="sidebar-item">
                        <AiFillProduct onClick={handleSale} className="sidebar-icon" />
                        <span>Sale</span>
                    </div>
                ) : null}

                {/* Staff Menu Item */}
                {userRole === "Admin" ? (
                    <div className="sidebar-item">
                        <FaUsers onClick={handleStaff} className="sidebar-icon" />
                        <span>Staff</span>
                    </div>
                ) : null}

                {/* Dashboard Menu Item */}
                {userRole === "Admin" || userRole === "Manager" || userRole === "Employee" ? (
                    <div className="sidebar-item">
                        <FaTh onClick={handleDashboard} className="sidebar-icon" />
                        <span>Dashboard</span>
                    </div>
                ) : null}

                {/* Stock Menu Item */}
                {userRole === "Admin" || userRole === "Manager" || userRole === "Employee" ? (
                    <div className="sidebar-item">
                        <FaBox onClick={handleStock} className="sidebar-icon" />
                        <span>Stock</span>
                    </div>
                ) : null}

                {/* History Menu Item */}
                {userRole === "Admin" || userRole === "Manager" || userRole === "Employee" ? (
                    <div className="sidebar-item">
                        <FaCalendarAlt onClick={handleHistory} className="sidebar-icon" />
                        <span>History</span>
                    </div>
                ) : null}

                {/* Department Menu Item */}
                {userRole === "Admin" ? (
                    <div className="sidebar-item">
                        <FaBuilding onClick={handleDepartment} className="sidebar-icon" />
                        <span>Department</span>
                    </div>
                ) : null}

                {/* Logout Menu Item */}
                <div className="sidebar-item logout">
                    <FaSignOutAlt onClick={handleLogout} className="sidebar-icon" />
                    <span className="sidebar-logout">Logout</span>
                </div>
            </div>
        </div>
    );
};

export default Sidebar;