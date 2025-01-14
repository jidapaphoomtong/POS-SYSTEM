import React from "react";
import "../../styles/Sidebar.css";
import { FaBars, FaUsers, FaTh, FaBox, FaCalendarAlt, FaBuilding, FaSignOutAlt } from "react-icons/fa";
import { AiFillProduct } from "react-icons/ai";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { useNavigate } from "react-router-dom";

const Sidebar = () => {
    const navigate = useNavigate(); // ใช้สำหรับเปลี่ยนไปยังหน้าใหม่

    const handleLogout = () => {
        navigate('/'); // กลับไปที่หน้า Login
    };

    const handleSale = () => {
        navigate('/sale');
    };

    const handleStaff = () => {
        navigate('/EmployeeList');
    };

    const handleDashboard = () => {
        navigate('/');
    };

    const handelStock = () => {
        navigate('/');
    };

    const handelHistory = () => {
        navigate('/');
    };

    const handleDepartment = () => {
        navigate('/BranchList');
    }



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
                <FaBox onClick={handelStock} className="sidebar-icon" />
                <span>Stock</span>
                </div>

                {/* History */}
                <div className="sidebar-item">
                <FaCalendarAlt onClick={handelHistory} className="sidebar-icon" />
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
                    <span className="sidebar-logout">
                        Logout
                    </span>
                </div>
            </div>
        </div>
    );
};

export default Sidebar;