import React from "react";
import "../../styles/Navbar.css";
import { FaSearch, FaBell, FaUser, FaBars } from "react-icons/fa";

const Navbar = () => {
    return (
        <nav className="navbar">
        <div className="navbar-container">
            {/* <div className="search-bar">
            <input type="text" placeholder="Search..." />
            <FaSearch className="search-icon" />
            </div> */}

            <div className="navbar-icons">
            <FaBell className="icon bell-icon" />
            <div className="user-info">
                <FaUser className="icon user-icon" />
                <span>Jesika</span>
            </div>
            <FaBars className="icon menu-icon" />
            </div>
        </div>
        </nav>
    );
};

export default Navbar;