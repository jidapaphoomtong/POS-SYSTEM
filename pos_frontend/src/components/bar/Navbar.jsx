import { useState, useEffect, useRef } from "react";
import "../../styles/Navbar.css";
import { FaBell, FaUser, FaBars } from "react-icons/fa";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";
import axios from "axios";
import { useParams } from "react-router-dom";

const Navbar = () => {
    const [isDropdownOpen, setDropdownOpen] = useState(false);
    const [userData, setUserData] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [notifications, setNotifications] = useState([]);
    const [isNotificationOpen, setNotificationOpen] = useState(false);
    const notificationRef = useRef(null);
    const userRef = useRef(null);
    const { branchId } = useParams();
    const [unreadCount, setUnreadCount] = useState(0);

    useEffect(() => {
        const token = Cookies.get("authToken");
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                setUserData(decodedToken);
                fetchNotifications();
            } catch (error) {
                console.error("Invalid token:", error);
                toast.error("มีบางอย่างผิดพลาด");
            }
        }
    }, []);

    const fetchNotifications = async () => {
        const token = Cookies.get("authToken");
        const currentBranchId = branchId;
        if (!token || !currentBranchId) {
            console.error("Token or branchId is missing.");
            return;
        }

        try {
            const response = await axios.get(`/api/Notification/notification/${currentBranchId}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            if (response.data.success) {
                setNotifications(response.data.data);
                const unreadCount = response.data.data.filter(notif => !notif.IsRead).length;
                setUnreadCount(unreadCount);
            } else {
                toast.error("ไม่สามารถดึงการแจ้งเตือนได้.");
            }
        } catch (error) {
            console.error("Error fetching notifications:", error);
            toast.error("เกิดข้อผิดพลาดในการดึงการแจ้งเตือน.");
        }
    };

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (notificationRef.current && !notificationRef.current.contains(event.target)) {
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
        console.log(setNotificationOpen)
        
        // เมื่อเปิดดรอปดาวน์การแจ้งเตือนให้ทำเครื่องหมายทุกการแจ้งเตือนว่าอ่าน
        if (!isNotificationOpen) {
            markAllNotificationsAsRead();
        }
    };

    const markAllNotificationsAsRead = async () => {
        const token = Cookies.get("authToken");
        if (!token) return;

        try {
            await axios.put(`/api/Notification/read-all-notifications/${branchId}`, {}, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            });

            const updatedNotifications = notifications.map(notification => ({
                ...notification,
                IsRead: true // ตั้งค่า IsRead เป็น true
            }));

            setNotifications(updatedNotifications);
            console.log(setNotifications)
            setUnreadCount(0); // อัปเดตจำนวนแจ้งเตือนที่ยังไม่ได้อ่าน
        } catch (error) {
            console.error("Error marking all notifications as read:", error);
        }
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

    return (
        <nav className="navbar">
            <div className="navbar-container">
                <div className="navbar-icons">
                    <div className="notification-icon" onClick={handleToggleNotifications} ref={notificationRef}>
                        <FaBell className="icon bell-icon" />
                        {/* ปรับการแสดงจำนวนการแจ้งเตือนที่ยังไม่ได้อ่าน */}
                        {notifications.some(notification => !notification.IsRead) && (
                            <span className="notification-dot">
                                {unreadCount} {/* จำนวนแจ้งเตือนที่ยังไม่ได้อ่าน */}
                            </span>
                        )}
                        {isNotificationOpen && (
                            <div className="notification-dropdown">
                                {notifications.map((notification) => (
                                    <div className={`notification-item ${notification.IsRead ? 'read' : 'unread'}`} key={notification.productId}>
                                        <p>{notification.message}</p>
                                        {notification.IsRead && <span>✅</span>}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                    <div className="user-info" ref={userRef}>
                        <FaUser className="user-icon" />
                        <span style={{ marginRight: "10px", marginTop: "3.3px" }}>
                            {userData ? userData.firstName : "Guest"}
                        </span>
                        <div className="icon menu-icon" onClick={handleToggleDropdown}>
                            <FaBars />
                            {isDropdownOpen && (
                                <div className="dropdown">
                                    <ul>
                                        <li style={{ fontSize: "16" }} onClick={showUserInfoModal}>User Info</li>
                                    </ul>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
    
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