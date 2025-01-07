import axios from "axios";
import { useEffect, useState } from "react";
import AddBranchModal from "./AddBranchModal";
import EditBranchModal from "./EditBranchModal";
import ConfirmationModal from "./ConfirmationModal";
import { useNavigate } from "react-router-dom";
import "../../styles/branch.css";

const BranchList = () => {
    const [branches, setBranches] = useState([]); // ใช้ State `branches` ตัวเดียว
    const [showAddModal, setShowAddModal] = useState(false);
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(true);
    const [editBranch, setEditBranch] = useState(null);
    const [deleteBranchId, setDeleteBranchId] = useState(null); // เก็บ ID ที่ต้องการลบ
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);

    // Fetch Branch List
    useEffect(() => {
        const fetchBranches = async () => {
            try {
                setIsLoading(true);
                const response = await axios.get("http://localhost:5293/api/Admin/branches", {
                    withCredentials: true, // ส่ง Cookie
                });

                setBranches(response.data.data || []); // บันทึก Branch
            } catch (error) {
                if (error.response?.status === 401) {
                    alert("Unauthorized. Please login.");
                    navigate("/"); // Redirect ไปหน้า Login
                } else if (error.response?.status === 403) {
                    alert("Access denied.");
                }
            } finally {
                setIsLoading(false);
            }
        };

        fetchBranches();
    }, [navigate]);

    const handleAddBranch = (newBranch) => {
        setBranches([...branches, newBranch]);
    };

    const handleEditBranch = (id, updatedBranch) => {
        setBranches(
            branches.map((branch) =>
                branch.id === id ? { ...branch, ...updatedBranch } : branch
            )
        );
    };

    const handleOpenDeleteModal = (id) => {
        setDeleteBranchId(id);
        setIsConfirmModalOpen(true);
    };

    const handleCloseDeleteModal = () => {
        setDeleteBranchId(null);
        setIsConfirmModalOpen(false);
    };

    const handleConfirmDelete = async () => {
        try {
            await axios.delete(`http://localhost:5293/api/Admin/branches/${deleteBranchId}`, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("authToken")}`,
                },
            });
            alert("Branch deleted successfully!");
            setBranches(branches.filter((branch) => branch.id !== deleteBranchId));
            handleCloseDeleteModal();
        } catch (error) {
            alert(error.response?.data?.message || "Failed to delete branch.");
        }
    };

    return (
        <div className="branch-container">
            <div className="header">
                <h1>Branch Management</h1>
                <button
                    className="add-button"
                    onClick={() => setShowAddModal(true)}
                    disabled={isLoading}
                >
                    Add Department
                </button>
            </div>

            {isLoading ? (
                <p>Loading branches...</p>
            ) : (
                <table className="branch-table">
                    <thead>
                        <tr>
                            <th>Detail</th>
                            <th>Department ID</th>
                            <th>Department Name</th>
                            <th>Location</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {branches.map(({ id, name, location }) => (
                            <tr key={id}>
                                <td>
                                    <a href={`/branch/${id}`} className="detail-link">Detail</a>
                                </td>
                                <td>{id}</td>
                                <td>{name}</td>
                                <td>{location}</td>
                                <td className="action-buttons">
                                    <button
                                        className="edit-button"
                                        onClick={() => setEditBranch({ id, name, location })}
                                    >
                                        ✏️
                                    </button>
                                    <button
                                        className="delete-button"
                                        onClick={() => handleOpenDeleteModal(id)}
                                    >
                                        🗑️
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            <button
                className="back-button"
                onClick={() => navigate("/select-branch")}
            >
                Back
            </button>

            {isConfirmModalOpen && (
                <ConfirmationModal
                    isOpen={isConfirmModalOpen}
                    onClose={handleCloseDeleteModal}
                    onConfirm={handleConfirmDelete}
                    message="Are you sure you want to delete this branch?"
                />
            )}

            {showAddModal && (
                <AddBranchModal
                    onClose={() => setShowAddModal(false)}
                    onAdd={handleAddBranch}
                />
            )}

            {editBranch && (
                <EditBranchModal
                    department={editBranch}
                    onClose={() => setEditBranch(null)}
                    onEdit={handleEditBranch}
                />
            )}
        </div>
    );
};

export default BranchList;