import { useState } from "react";
import "../../styles/Navbar.css";
import { FaSearch, FaBell, FaUser, FaBars } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";

const Navbar = () => {
    const [isDropdownOpen, setDropdownOpen] = useState(false);
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [isNotificationOpen, setNotificationOpen] = useState(false);

    // const fetchNotifications = async () => {
    //     try {
    //         const response = await axios.get('/api/notifications'); // URL ของ API
    //         setNotifications(response.data);
    //         setUnreadCount(response.data.filter(n => !n.read).length);
    //     } catch (error) {
    //         console.error('Error fetching notifications:', error);
    //         toast.error('Failed to fetch notifications');
    //     }
    // };

    // const handleNotificationClick = () => {
    //     setNotificationOpen(!isNotificationOpen);
    //     setUnreadCount(0); // หรือสามารถรีเซ็ตจำนวนที่ยังไม่ได้อ่านได้ที่นี่
    // };

    // useEffect(() => {
    //     fetchNotifications(); // เรียก notifications ตอน mount
    // }, []);

    const handleToggleDropdown = () => {
        setDropdownOpen(!isDropdownOpen);
    };
    const navigate = useNavigate();
    // Extract token and decode it to get user role
        const token = Cookies.get("authToken");
        // console.log(token)
        let firstName = "";
    
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                const firstNameFromToken = decodedToken["firstName"] || 'No role found';
                firstName = firstNameFromToken; // Adjust according to your JWT structure
                // console.log(firstName);

            } catch (error) {
                console.error("Invalid token:", error);
                toast.error("Have Something wrong");
            }
        }
    return (
        <nav className="navbar">
        <div className="navbar-container">
            {/* <div className="search-bar">
            <input type="text" placeholder="Search..." />
            <FaSearch className="search-icon" />
            </div> */}

            <div className="navbar-icons">
                <div className="notification-icon">
                    <FaBell className="icon bell-icon" />
                    {/* {unreadCount > 0 && (
                            <span className="notification-badge">{unreadCount}</span>
                        )} */}
                    </div>
                    {/* {isNotificationOpen && (
                        <div className="notification-dropdown">
                            <ul>
                                {notifications.map((notification, index) => (
                                    <li key={index}>
                                        {notification.message} {notification.read ? '(Read)' : '(Unread)'}
                                    </li>
                                ))}
                                {notifications.length === 0 && <li>No notifications</li>}
                            </ul>
                        </div>
                    )} */}
                <div className="user-info">
                    <FaUser className="icon user-icon" />
                    <span>{firstName}</span>
                </div>
                <div className="menu-icon" onClick={handleToggleDropdown}>
                    <FaBars />
                    {isDropdownOpen && (
                        <div className="dropdown">
                            <ul>
                                <li>Admin Info 1</li>
                                <li>Admin Info 2</li>
                                <li>Admin Info 3</li>
                            </ul>
                        </div>
                    )}
                </div>
            </div>
        </div>
        </nav>
    );
};

export default Navbar;