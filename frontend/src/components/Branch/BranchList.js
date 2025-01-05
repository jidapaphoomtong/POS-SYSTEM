import React, { useState } from "react";
import AddDepartmentModal from "./AddBranchModal";
import EditDepartmentModal from "./EditBranchModal";
import "../../styles/branch.css"; // ใช้ CSS แบบใหม่

const BranchList = () => {
    const [departments, setDepartments] = useState([
        { id: "0000123", name: "Chickkai", location: "Bangkok" },
        { id: "0000124", name: "Punjai", location: "Khon Kean" },
        { id: "0000125", name: "Meesuk", location: "Mahasarakham" },
        { id: "0000126", name: "YimYim", location: "Bangkok" },
        { id: "0000127", name: "Kao", location: "Khon Kean" },
        { id: "0000128", name: "Patthai", location: "Mahasarakham" },
    ]);

    const [showAddModal, setShowAddModal] = useState(false);
    const [editDepartment, setEditDepartment] = useState(null);

    const handleAddDepartment = (newDepartment) => {
        setDepartments([...departments, newDepartment]);
    };

    const handleDeleteDepartment = (id) => {
        if (window.confirm("Are you sure you want to delete this department?")) {
            setDepartments(departments.filter((department) => department.id !== id));
        }
    };

    const handleEditDepartment = (id, updatedDepartment) => {
        setDepartments(
            departments.map((dept) =>
                dept.id === id ? { ...dept, ...updatedDepartment } : dept
            )
        );
        setEditDepartment(null);
    };

    return (
        <div className="branch-container">
            <div className="header">
                <h1>Branch Management</h1>
                <button className="add-button" onClick={() => setShowAddModal(true)}>
                    Add Department
                </button>
            </div>

            <table className="branch-table">
                <thead>
                    <tr>
                        <th>Department ID</th>
                        <th>Department Name</th>
                        <th>Location</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {departments.map(({ id, name, location }) => (
                        <tr key={id}>
                            <td>{id}</td>
                            <td>{name}</td>
                            <td>{location}</td>
                            <td className="action-buttons">
                                <button
                                    className="edit-button"
                                    onClick={() => setEditDepartment({ id, name, location })}
                                >
                                    ✏️
                                </button>
                                <button
                                    className="delete-button"
                                    onClick={() => handleDeleteDepartment(id)}
                                >
                                    🗑️
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            <button
                className="back-button"
                onClick={() => (window.location.href = "/select-branch")}
            >
                Back
            </button>

            {showAddModal && (
                <AddDepartmentModal
                    onClose={() => setShowAddModal(false)}
                    onAdd={handleAddDepartment}
                />
            )}

            {editDepartment && (
                <EditDepartmentModal
                    department={editDepartment}
                    onClose={() => setEditDepartment(null)}
                    onEdit={handleEditDepartment}
                />
            )}
        </div>
    );
};

export default BranchList;