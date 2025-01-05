import React, { useState } from "react";

const AddBranchModal = ({ onClose, onAdd }) => {
    const [formData, setFormData] = useState({
        id: "",
        name: "",
        location: "",
    });

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = () => {
        if (!formData.id || !formData.name || !formData.location) {
        alert("Please fill out all fields!");
        return;
        }
        onAdd(formData);
        onClose();
    };

    return (
        <div className="modal">
        <div className="modal-content">
            <h2>Add Department</h2>
            <input
            type="text"
            name="id"
            placeholder="Department ID"
            value={formData.id}
            onChange={handleChange}
            />
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
            <div className="modal-buttons">
            <button onClick={onClose}>Cancel</button>
            <button onClick={handleSubmit}>Save</button>
            </div>
        </div>
        </div>
    );
};

export default AddBranchModal;