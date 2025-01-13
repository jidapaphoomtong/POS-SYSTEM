import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "../styles/SelectBranch.css";
import { FaTh, FaList } from "react-icons/fa";
import Cookies from "js-cookie";

export default function SelectBranch() {
    const [branches, setBranches] = useState([]);
    const [selectedBranch, setSelectedBranch] = useState(null);
    const [viewMode, setViewMode] = useState("grid");
    const navigate = useNavigate();
    const [searchTerm, setSearchTerm] = useState("");
    const [isLoading, setIsLoading] = useState(true);

    const filteredBranches = branches.filter((branch) =>
        branch.name && branch.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

    useEffect(() => {
        const fetchBranches = async () => {
            const token = Cookies.get("authToken"); // ดึง Token จาก Cookie
            console.log("Login successful, JWT Token received:", token);
            if(token){
                try {
                    // console.log("Fetching branches...");
                    setIsLoading(true);
                    const response = await axios.get("https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/Branch/branches", {
                        headers: {
                            "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        },
                        withCredentials: true, // ส่งคำขอพร้อม Cookie
                    });
    
                    const data = response.data.data || []; // กำหนด Default ถ้าไม่มี Data
                    // console.log("Branches loaded:", data);
    
                    setBranches(data); // บันทึก Branch ใน State
                } catch (error) {
                    // console.error("Failed to fetch branches:", error);
                    if (error.response?.status === 401) {
                        alert("Unauthorized. Please login.");
                        navigate("/"); // Redirect ไปหน้า Login
                    } else if (error.response?.status === 403) {
                        alert("Access Denied: You do not have the required permissions.");
                    }
                }
            setIsLoading(false); // ปิดสถานะการโหลด
            }
        };

        fetchBranches();
    }, [navigate]);

    const handleSelectBranch = (branchId) => {
        setSelectedBranch(branchId);
        navigate(`/sale?branch=${branchId}`); // Redirect ไป Sale หน้าต่าง ๆ
    };

    return (
        <div className="container">
            <h1 className="header">Select Department</h1>
            {isLoading ? (
                <div className="spinner"></div>
            ) : (
                <>
                    <div className="search-container">
                        <input
                            className="search-bar"
                            type="text"
                            placeholder="Search..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                        <FaTh
                            onClick={() => setViewMode("grid")}
                            style={{ cursor: "pointer", color: viewMode === "grid" ? "#5995fd" : "#c0c0c0" }}
                        />
                        <FaList
                            onClick={() => setViewMode("list")}
                            style={{ cursor: "pointer", color: viewMode === "list" ? "#5995fd" : "#c0c0c0" }}
                        />
                    </div>
                    <div className={viewMode === "grid" ? "branch-grid" : "branch-list"}>
                        {filteredBranches.length > 0 ? (
                            filteredBranches.map((branch) => (
                                <div
                                    key={branch.id}
                                    className={`branch-card ${selectedBranch === branch.id ? "active" : ""}`}
                                    onClick={() => handleSelectBranch(branch.id)} // ส่ง ID
                                >
                                    <img
                                        src={branch.iconUrl || "https://via.placeholder.com/50"}
                                        alt={branch.name || "Branch"}
                                    />
                                    <p>{branch.name || "Unnamed Branch"}</p>
                                </div>
                            ))
                        ) : (
                            <p>No branches found.</p> // แสดงข้อความเมื่อไม่มีข้อมูล
                        )}
                    </div>
                    <div className="buttons">
                        <button onClick={() => navigate("/")}>Back</button>
                        <button onClick={() => navigate("/BranchList")}>Check Department</button>
                    </div>
                </>
            )}
        </div>
    );
}