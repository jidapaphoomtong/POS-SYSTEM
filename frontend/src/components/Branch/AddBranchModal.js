import React, { useState } from "react";
import axios from "axios";
import "../../styles/branch.css"

const AddBranchModal = ({ onClose, onAddSuccess }) => {
    const [formData, setFormData] = useState({
        name: "",
        location: "",
        iconUrl: "",
    });
    const [isLoading, setIsLoading] = useState(false);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async () => {
        if (!formData.name || !formData.location) {
            alert("Please fill out all fields!");
            return;
        }

        setIsLoading(true); // เริ่มสถานะ Loading

        try {
            const response = await axios.post("http://localhost:5293/api/Admin/add-branch", formData, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("authToken")}`, // เพิ่ม Token จาก LocalStorage
                    "Content-Type": "application/json",
                },
            });

            if (response.data?.Success) {
                alert("Branch added successfully!");
                onAddSuccess(response.data?.BranchId); // เรียก Callback หลังเพิ่มสำเร็จ
                onClose(); // ปิด Modal
            } else {
                alert(response.data?.Message || "Something went wrong!");
            }
        } catch (error) {
            console.error("Error adding branch:", error);
            alert(error.response?.data?.Message || "Failed to add branch.");
        } finally {
            setIsLoading(false); // ปิดสถานะ Loading
        }
    };

    return (
        <div className="modal">
            <div className="modal-content">
                <h2>Add Department</h2>
                <input
                    type="text"
                    name="name"
                    placeholder="Department Name"
                    value={formData.name}
                    onChange={handleChange}
                />
                <input
                    type="text"
                    name="location"
                    placeholder="Location"
                    value={formData.location}
                    onChange={handleChange}
                />
                <input
                    type="text"
                    name="iconUrl"
                    placeholder="Icon URL"
                    value={formData.iconUrl}
                    onChange={handleChange}
                />
                <div className="modal-buttons">
                    <button onClick={onClose} disabled={isLoading}>Cancel</button>
                    <button onClick={handleSubmit} disabled={isLoading}>
                        {isLoading ? "Saving..." : "Save"}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default AddBranchModal;