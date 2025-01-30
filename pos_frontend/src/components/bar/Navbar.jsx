import { useState, useEffect, useRef } from "react";
import "../../styles/Navbar.css";
import { FaBell, FaUser, FaBars } from "react-icons/fa";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";
import axios from "axios";

const Navbar = () => {
    const [isDropdownOpen, setDropdownOpen] = useState(false);
    const [userData, setUserData] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [notifications, setNotifications] = useState([]);
    const [isNotificationOpen, setNotificationOpen] = useState(false);
    const notificationRef = useRef(null);
    const userRef = useRef(null);

    // ฟังก์ชันดึงการแจ้งเตือนจาก API
    const fetchNotifications = async () => {
        const token = Cookies.get("authToken");
        const branchId = userData ? userData.branchId : "";
        if (!token || !branchId) {
            return;
        }

        try {
            const response = await axios.get(`/api/Notification/notification/${branchId}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            console.log(response)

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

    useEffect(() => {
    }, [notifications]);

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

    const markNotificationAsRead = async (notificationId) => {
        const token = Cookies.get("authToken");
        const branchId = userData ? userData.branchId : "";

        if (!token || !branchId) return;

        try {
            await axios.put(`/api/Notification/read-notification/${branchId}/${notificationId}`, {}, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });
            setNotifications(notifications.map(notification => 
                notification.productId === notificationId ? { ...notification, IsRead: true } : notification
            ));
        } catch (error) {
            console.error("Error marking notification as read:", error);
        }
    };

    return (
        <nav className="navbar">
            <div className="navbar-container">
                <div className="navbar-icons">
                    <div className="notification-icon" onClick={handleToggleNotifications} ref={notificationRef}>
                        <FaBell className="icon bell-icon" />
                        {notifications.some(notif => !notif.IsRead) && (
                            <span className="notification-dot"></span>
                        )}
                        {/* Notification Dropdown */}
                        {isNotificationOpen && (
                            <div className="notification-dropdown">
                                {notifications.length > 0 ? (
                                    notifications.map((notification) => (
                                        <div className={`notification-item ${notification.IsRead ? '' : 'unread'}`} key={notification.productId}>
                                            <p>{notification.message}</p>
                                            <button onClick={() => markNotificationAsRead(notification.productId)}>Mark as Read</button>
                                        </div>
                                    ))
                                ) : (
                                    <p>No notifications.</p>
                                )}
                            </div>
                        )}
                    </div>
                    <div className="user-info" ref={userRef}>
                        <FaUser className="user-icon" />
                        <span style={{marginRight:"10px", fontSize:"20", marginTop:"3.3px"}}>{userData ? userData.firstName : "Guest"}</span>
                        <div className="icon menu-icon" onClick={handleToggleDropdown}>
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