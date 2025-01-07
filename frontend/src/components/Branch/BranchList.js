import axios from "axios";
import { useEffect, useState } from "react";
import AddBranchModal from "./AddBranchModal";
import EditBranchModal from "./EditBranchModal";
import ConfirmationModal from "./ConfirmationModal";
import { useNavigate } from "react-router-dom";
import "../../styles/branch.css";
import Cookies from "js-cookie";

const BranchList = () => {
    const [branches, setBranches] = useState([]); 
    const [showAddModal, setShowAddModal] = useState(false);
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [editBranch, setEditBranch] = useState(null);
    const [deleteBranchId, setDeleteBranchId] = useState(null);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);

    // Fetch Branch List
    useEffect(() => {
        const fetchBranches = async () => {
            const token = Cookies.get("authToken"); // ดึง Token
            if (!token) {
                alert("Your session has expired. Please login again.");
                navigate("/"); // Redirect ไปหน้า Login
                return;
            }

            try {
                setIsLoading(true);
                const response = await axios.get("http://localhost:5293/api/Admin/branches", {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    },
                    withCredentials: true,
                });

                setBranches(response.data.data || []); // อัปเดต State Branches
            } catch (error) {
                console.error("Failed to fetch branches:", error);
                if (error.response?.status === 401) {
                    alert("Unauthorized. Please login.");
                    navigate("/");
                } else {
                    alert("Failed to fetch branches.");
                }
            } finally {
                setIsLoading(false);
            }
        };

        fetchBranches();
    }, [navigate]);

    const handleAddBranch = (newBranch) => {
        setBranches([...branches, newBranch]); // อัปเดต Branch ใหม่
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
        const token = Cookies.get("authToken"); // ดึง Token

        try {
            const response = await axios.delete(`https://your-backend-url/api/Admin/branches/${deleteBranchId}`, {
                headers: {
                    "x-posapp-header": "your-header",
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });

            if (response.status === 200) {
                alert("Branch deleted successfully!");
                // Update state to remove the deleted branch
                setBranches(branches.filter((branch) => branch.id !== deleteBranchId));
                handleCloseDeleteModal();
            } else {
                alert("Failed to delete branch.");
            }
        } catch (error) {
            console.error("Failed to delete branch:", error);
            alert("Failed to delete branch: " + (error.response?.data?.message || "Unknown error"));
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
                                    onClick={() => {
                                        setEditBranch({ id, name, location });
                                        navigate(`/edit-branch/${id}`); // หรือลิงก์ไปที่ EditBranchModal
                                    }}
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