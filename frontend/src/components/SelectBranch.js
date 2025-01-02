import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "../styles/SelectBranch.css";
import { FaTh, FaList, FaStore } from "react-icons/fa";

export default function SelectBranch() {
    const [branches, setBranches] = useState([]);
    const [selectedBranch, setSelectedBranch] = useState(null);
    const [viewMode, setViewMode] = useState("grid");
    const navigate = useNavigate();
    const [searchTerm, setSearchTerm] = useState("");

    const filteredBranches = branches.filter(branch =>
        branch.name && branch.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

    useEffect(() => {
        const fetchBranches = async () => {
            try {
                const response = await axios.get("http://localhost:5293/api/Admin/branches", {
                    headers: { Authorization: `Bearer ${localStorage.getItem("authToken")}` },
                });
                const data = response.data.data;
                if (Array.isArray(data)) {
                    setBranches(data);
                } else {
                    console.error("Unexpected data format:", response.data);
                    setBranches([]);
                }
            } catch (error) {
                console.error("Error fetching branches:", error);
            }
        };

        fetchBranches();
    }, []);

    const handleSelectBranch = (branchName) => {
        setSelectedBranch(branchName);
        navigate(`/sale?branch=${branchName}`);
    };

    return (
        <div className="container">
            <h1 className="header">Select Department</h1>
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
                {filteredBranches.map((branch) => (
                    <div 
                        key={branch.id} 
                        className={`branch-card ${selectedBranch === branch.name ? "active" : ""}`} 
                        onClick={() => handleSelectBranch(branch.name)}
                    >
                        <img 
                            src={branch.iconUrl || "https://via.placeholder.com/50"} 
                            // alt={branch.name || "Branch"}
                        />
                        <p>{branch.name || "Unnamed Branch"}</p>
                    </div>
                ))}
            </div>
            <div className="buttons">
                <button onClick={() => navigate("/")}>Back</button>
                <button onClick={() => navigate("/department")}>Check Department</button>
            </div>
        </div>
    );
}
