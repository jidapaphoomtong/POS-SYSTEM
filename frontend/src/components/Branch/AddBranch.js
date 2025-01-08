import React, { useState } from "react";
import axios from "axios";
import "../../styles/branch.css";
import Cookies from "js-cookie";
import { useNavigate } from "react-router-dom";

const AddBranch = () => {
    const [formData, setFormData] = useState({
        name: "",
        location: "",
        iconUrl: "",
    });
    
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Validation
        if (!formData.name || !formData.location) {
            alert("Please fill out all fields!");
            return;
        }

        setIsLoading(true);

        try {
            const token = Cookies.get("authToken");
            if (!token) {
                alert("No token found. Please log in again.");
                return;
            }

            const response = await axios.post("http://localhost:5293/api/Admin/add-branch", formData, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json",
                },
                withCredentials: true,
            });

            if (response.data?.Success) {
                alert("Branch added successfully!");
                navigate("/BranchList"); // นำทางกลับไปหน้า BranchList
            } else {
                alert(response.data?.Message || "Something went wrong!");
            }
        } catch (error) {
            console.error("Error adding branch:", error);
            alert(error.response?.data?.Message || "Failed to add branch.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="add-branch-container">
            <h2>Add Department</h2>
            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    name="name"
                    placeholder="Department Name"
                    value={formData.name}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="location"
                    placeholder="Location"
                    value={formData.location}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="iconUrl"
                    placeholder="Icon URL"
                    value={formData.iconUrl}
                    onChange={handleChange}
                />
                <div className="form-buttons">
                    <button type="button" onClick={() => navigate('/BranchList')} disabled={isLoading}>
                        Cancel
                    </button>
                    <button type="submit" disabled={isLoading}>
                        {isLoading ? "Saving..." : "Save"}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default AddBranch;