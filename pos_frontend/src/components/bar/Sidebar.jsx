import React from "react";
import "../../styles/Sidebar.css";
import { FaUsers, FaTh, FaBox, FaCalendarAlt, FaBuilding, FaSignOutAlt } from "react-icons/fa";
import { AiFillProduct } from "react-icons/ai";
import { useNavigate, useParams } from "react-router-dom";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";

const Sidebar = () => {
    const navigate = useNavigate();
    const { branchId } = useParams(); // สำหรับการดึงค่า branchId
    
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
            toast.error("Have Something wrong");
        }
    }
    
    if (!branchId) {
        alert("Branch ID is missing!");
        return null; // Return null to avoid rendering if Branch ID is missing
    }
    
    const handleLogout = () => {
        navigate('/'); // Return to Login page
    };

    const handleSale = () => {
        navigate(`/${branchId}/sale`);
    };

    const handleStaff = () => {
        navigate(`/${branchId}/EmployeeList`);
    };

    const handleDashboard = () => {
        navigate(`/${branchId}/dashboard`);
    };

    const handleStock = () => {
        navigate(`/${branchId}/ProductList`);
    };

    const handleHistory = () => {
        navigate(`/${branchId}/history`);
    };

    const handleDepartment = () => {
        navigate(`/select-branch`);
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
                {userRole === "Admin" || userRole === "Manager"  ? (
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