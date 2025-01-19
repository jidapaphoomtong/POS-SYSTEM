import { useState } from "react";
import "../../styles/Navbar.css";
import { FaSearch, FaBell, FaUser, FaBars } from "react-icons/fa";
import { useNavigate } from "react-router-dom";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";

const Navbar = () => {
    const [isDropdownOpen, setDropdownOpen] = useState(false);
    const [notifications] = useState(["Notification 1", "Notification 2"]); // Simulated notifications

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
                console.log(firstName);

            } catch (error) {
                console.error("Invalid token:", error);
                alert("Have Something wrong")
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
                <div className="notification-icon" onClick={() => alert(notifications.join(', '))}>
                    <FaBell className="icon bell-icon" />
                </div>
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