import { useState, useEffect } from "react";
import "../../styles/Navbar.css";
import { FaBell, FaUser, FaBars } from "react-icons/fa";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";
import axios from "axios";
import { useRef } from "react";

const Navbar = () => {
    const [isDropdownOpen, setDropdownOpen] = useState(false);
    const [userData, setUserData] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [notifications, setNotifications] = useState([]);
    const [isNotificationOpen, setNotificationOpen] = useState(false);
    const notificationRef = useRef(null);
    const userRef = useRef(null); // สำหรับ User Info เมนูถ้าต้องการ

    useEffect(() => {
        const token = Cookies.get("authToken");
        
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                setUserData(decodedToken);
                fetchNotifications(); // ดึงการแจ้งเตือนตอนโหลด
            } catch (error) {
                console.error("Invalid token:", error);
                toast.error("มีบางอย่างผิดพลาด");
            }
        }

        const handleClickOutside = (event) => {
            if (
                notificationRef.current && 
                !notificationRef.current.contains(event.target)
            ) {
                setNotificationOpen(false);
            }
            if (userRef.current && !userRef.current.contains(event.target)) {
                setDropdownOpen(false);
            }
        };
    
        document.addEventListener("mousedown", handleClickOutside);
        
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };

    }, []);

    // ฟังก์ชันดึงการแจ้งเตือนจาก API
    const fetchNotifications = async () => {
        const token = Cookies.get("authToken");
        const branchId = userData ? userData.branchId : ""; // สมมุติว่า branchId อยู่ใน token
        if (!token || !branchId) {
            return;
        }

        try {
            const response = await axios.get(`/api/notification/${branchId}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            if (response.data.success) {
                setNotifications(response.data.data);
            } else {
                toast.error("ไม่สามารถดึงการแจ้งเตือนได้.");
            }
        } catch (error) {
            console.error("Error fetching notifications:", error);
            toast.error("เกิดข้อผิดพลาดในการดึงการแจ้งเตือน.");
        }
    };

    const handleToggleNotifications = () => {
        setNotificationOpen(!isNotificationOpen);
    };

    const handleToggleDropdown = () => {
        setDropdownOpen(!isDropdownOpen);
    };

    const showUserInfoModal = () => {
        setIsModalOpen(true);
    };

    const closeUserInfoModal = () => {
        setIsModalOpen(false);
    };

    const deleteNotification = async (notificationId) => {
        const token = Cookies.get("authToken");
        const branchId = userData ? userData.branchId : ""; // สมมุติว่า branchId อยู่ใน token
    
        if (!token || !branchId) return;
    
        try {
            const response = await axios.delete(`/api/notification/delete-notification/${branchId}/${notificationId}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });
    
            if (response.data.success) {
                // ลบการแจ้งเตือนจาก state
                setNotifications(notifications.filter(notification => notification.productId !== notificationId));
                toast.success("Notification deleted successfully.");
            } else {
                toast.error("Failed to delete notification.");
            }
        } catch (error) {
            console.error("Error deleting notification:", error);
            toast.error("Failed to delete notification.");
        }
    };

    return (
        <nav className="navbar">
            <div className="navbar-container">
                <div className="navbar-icons">
                    <div className="notification-icon" onClick={handleToggleNotifications} ref={notificationRef}>
                        <FaBell className="icon bell-icon" />
                        {notifications.length > 0 && (
                            <span className="notification-count">{notifications.length}</span>
                        )}
                        {/* Notification Dropdown */}
                        {isNotificationOpen && (
                            <div className="notification-dropdown">
                                {notifications.length > 0 ? (
                                    notifications.map((notification) => (
                                        <div className="notification-item" key={notification.productId}>
                                            <p>{notification.message}</p>
                                            <button onClick={() => deleteNotification(notification.productId)}>
                                                Delete
                                            </button>
                                        </div>
                                    ))
                                ) : (
                                    <p>No notifications.</p>
                                )}
                            </div>
                        )}
                    </div>
                    <div className="user-info" ref={userRef}>
                        <FaUser className="icon user-icon" />
                        <span>{userData ? userData.firstName : "Guest"}</span>
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
            </div>

            {/* Modal สำหรับแสดงข้อมูลผู้ใช้ */}
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