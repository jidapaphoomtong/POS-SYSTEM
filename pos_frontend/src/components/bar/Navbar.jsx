import { useState, useEffect } from "react";
import "../../styles/Navbar.css";
import { FaBell, FaUser, FaBars } from "react-icons/fa";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";

const Navbar = () => {
    const [isDropdownOpen, setDropdownOpen] = useState(false);
    const [userData, setUserData] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);

    useEffect(() => {
        const token = Cookies.get("authToken");
        
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                setUserData(decodedToken);
            } catch (error) {
                console.error("Invalid token:", error);
                toast.error("Have Something wrong");
            }
        }
    }, []);

    const handleToggleDropdown = () => {
        setDropdownOpen(!isDropdownOpen);
    };

    const showUserInfoModal = () => {
        setIsModalOpen(true);
    };

    const closeUserInfoModal = () => {
        setIsModalOpen(false);
    };

    return (
        <nav className="navbar">
            <div className="navbar-container">
                <div className="navbar-icons">
                    <div className="notification-icon">
                        <FaBell className="icon bell-icon" />
                    </div>
                    <div className="user-info">
                        <FaUser className="icon user-icon" />
                        <span>{userData ? userData.firstName : 'Guest'}</span>
                    </div>
                    <div className="menu-icon" onClick={handleToggleDropdown}>
                        <FaBars />
                        {isDropdownOpen && (
                            <div className="dropdown">
                                <ul>
                                    <li onClick={showUserInfoModal}>User Info</li>
                                </ul>
                            </div>
                        )}
                    </div>
                </div>
            </div>

            {/* Modal สำหรับแสดงรายละเอียดผู้ใช้ */}
            {isModalOpen && (
                <div className="modal">
                    <div className="modal-content">
                        <span className="close" onClick={closeUserInfoModal}>&times;</span>
                        <h2>User Detail</h2>
                        {userData ? (
                            <div>
                                <p><strong>First Name:</strong> {userData.firstName || 'N/A'}</p>
                                <p><strong>Email:</strong> {userData['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || 'N/A'}</p>
                                <p><strong>Role:</strong> {userData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'N/A'}</p>
                            </div>
                        ) : (
                            <p>No user data found. Please log in.</p>
                        )}
                    </div>
                </div>
            )}
        </nav>
    );
};

export default Navbar;