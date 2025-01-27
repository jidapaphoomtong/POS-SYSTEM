import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "../styles/SelectBranch.css";
import { FaTh, FaList } from "react-icons/fa";
import Cookies from "js-cookie";
import { toast } from "react-toastify";

export default function SelectBranch() {
    const [branches, setBranches] = useState([]);
    const [selectedBranch, setSelectedBranch] = useState(null);
    const [viewMode, setViewMode] = useState("grid");
    const navigate = useNavigate();
    const [searchTerm, setSearchTerm] = useState("");
    const [isLoading, setIsLoading] = useState(true);

    const itemsPerPage = 6; // จำนวน Branch ที่ต้องการแสดงต่อหน้า
    const [currentPage, setCurrentPage] = useState(1);

    // กรอง Branch
    const filteredBranches = branches.filter((branch) =>
        branch.name && branch.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const indexOfLastBranch = currentPage * itemsPerPage;
    const indexOfFirstBranch = indexOfLastBranch - itemsPerPage;
    const currentBranches = filteredBranches.slice(indexOfFirstBranch, indexOfLastBranch);

    const totalPages = Math.ceil(filteredBranches.length / itemsPerPage);

    useEffect(() => {
        const fetchBranches = async () => {
            const token = Cookies.get("authToken");
            if (token) {
                try {
                    setIsLoading(true);
                    const response = await axios.get("/api/Branch/branches", {
                        headers: {
                            Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                        },
                        withCredentials: true,
                    });

                    const data = response.data.data || [];
                    setBranches(data);
                } catch (error) {
                    if (error.response?.status === 401) {
                        toast.error("Unauthorized. Please login.");
                        navigate("/"); 
                    } else if (error.response?.status === 403) {
                        toast.error("Access Denied: You do not have the required permissions.");
                    }
                }
                setIsLoading(false);
            }
        };

        fetchBranches();
    }, [navigate]);

    const handleSelectBranch = (branchId) => {
        setSelectedBranch(branchId);
        navigate(`/${branchId}/sale`);
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
                        {currentBranches.length > 0 ? (
                            currentBranches.map((branch) => (
                                <div
                                    key={branch.id}
                                    className={`branch-card ${selectedBranch === branch.id ? "active" : ""}`}
                                    onClick={() => handleSelectBranch(branch.id)}
                                >
                                    <img
                                        src={branch.iconUrl || "https://via.placeholder.com/50"}
                                        alt={branch.name || "Branch"}
                                    />
                                    <p>{branch.name || "Unnamed Branch"}</p>
                                </div>
                            ))
                        ) : (
                            <p>No branches found.</p>
                        )}
                    </div>

                    {/* Pagination Controls */}
                    <div className="pagination">
                        {Array.from({ length: totalPages }, (_, index) => (
                            <button
                                key={index + 1}
                                onClick={() => setCurrentPage(index + 1)}
                                className={currentPage === index + 1 ? 'active' : ''}
                            >
                                {index + 1}
                            </button>
                        ))}
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