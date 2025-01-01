import React from "react";
import "../../styles/Sidebar.css";
import { FaBars, FaUsers, FaTh, FaBox, FaCalendarAlt, FaBuilding, FaSignOutAlt } from "react-icons/fa";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { useNavigate } from "react-router-dom";

const Sidebar = () => {
    const navigate = useNavigate(); // ใช้สำหรับเปลี่ยนไปยังหน้าใหม่

    const handleLogout = () => {
        navigate('/'); // กลับไปที่หน้า Login
    };

    return (
        <div className="sidebar">
            <div className="sidebar-container">
                {/* Menu */}
                <div className="sidebar-item">
                <FaBars className="sidebar-icon" />
                <span>Menu</span>
                </div>

                {/* Staff */}
                <div className="sidebar-item">
                <FaUsers className="sidebar-icon" />
                <span>Staff</span>
                </div>

                {/* Dashboard */}
                <div className="sidebar-item">
                <FaTh className="sidebar-icon" />
                <span>Dashboard</span>
                </div>

                {/* Stock */}
                <div className="sidebar-item">
                <FaBox className="sidebar-icon" />
                <span>Stock</span>
                </div>

                {/* History */}
                <div className="sidebar-item">
                <FaCalendarAlt className="sidebar-icon" />
                <span>History</span>
                </div>

                {/* Department */}
                <div className="sidebar-item">
                <FaBuilding className="sidebar-icon" />
                <span>Department</span>
                </div>

                {/* Logout */}
                <div className="sidebar-item logout">
                    <FaSignOutAlt className="sidebar-icon" />
                    <span onClick={handleLogout} className="sidebar-logout">
                        Logout
                    </span>
                </div>
            </div>
        </div>
    );
};

export default Sidebar;