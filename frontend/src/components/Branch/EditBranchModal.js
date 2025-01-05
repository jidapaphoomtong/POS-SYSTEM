import React, { useState } from "react";

const EditBranchModal = ({ department, onClose, onEdit }) => {
    const [formData, setFormData] = useState(department);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = () => {
        if (!formData.name || !formData.location) {
        alert("Please fill out all fields!");
        return;
        }
        onEdit(department.id, formData);
    };

    return (
        <div className="modal">
        <div className="modal-content">
            <h2>Edit Department</h2>
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
            <button onClick={handleSubmit}>Confirm</button>
            </div>
        </div>
        </div>
    );
};

export default EditBranchModal;