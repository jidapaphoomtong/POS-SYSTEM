import React, { useState } from "react";
import axios from "axios";
import "../../styles/branch.css";
import Cookies from "js-cookie";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

const AddBranch = ({ onAddSuccess }) => {
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
            toast.error("Please fill out all fields!");
            return;
        }

        setIsLoading(true);

        try {
            const token = Cookies.get("authToken");
            if (!token) {
                toast.error("No token found. Please log in again.");
                return;
            }
        
            const response = await axios.post(`/api/Branch/add-branch`, formData, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                    "Content-Type": "application/json",
                },
                withCredentials: true,
            });
        
            // Check for successful response
            if (response.status === 200) {
                // Check if response.data is defined
                if (response.data) {
                        toast.success("Branch added successfully!");
                        navigate("/BranchList");
                } else {
                    toast.error("Response data is empty.");
                }
            } else {
                toast.error(`Request failed with status: ${response.status}`);
            }
        } catch (error) {
            console.error("Error adding branch:", error);
            
            // Improved error handling
            if (error.response) {
                // Server responded with a status other than 200 range
                console.error("Response data:", error.response.data);
                toast.error(error.response.data?.Message || "Failed to add branch.");
            } else if (error.request) {
                // Request was made but no response was received
                toast.error("No response from server. Please try again later.");
            } else {
                // Something happened in setting up the request
                toast.error("Error: " + error.message);
            }
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