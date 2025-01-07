import axios from "axios";
import { useEffect, useState } from "react";
import AddBranchModal from "./AddBranchModal";
import EditBranchModal from "./EditBranchModal";
import { useNavigate } from "react-router-dom";
import "../../styles/branch.css";

const BranchList = () => {
    const [branches, setBranches] = useState([]); // ใช้แค่ `branches` แทน `departments`
    const [showAddModal, setShowAddModal] = useState(false);
    const [editBranch, setEditBranch] = useState(null); // เปลี่ยนชื่อให้ชัดเจน
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(true);

    // Fetch Branch List
    useEffect(() => {
        const fetchBranches = async () => {
            try {
                setIsLoading(true);
                const response = await axios.get("http://localhost:5293/api/Admin/branches", {
                    withCredentials: true // ส่งคำขอพร้อม Cookie
                });

                const data = response.data.data || []; // กำหนด Default ถ้าไม่มี Data
                setBranches(data); // Update State
            } catch (error) {
                if (error.response?.status === 401) {
                    alert("Unauthorized. Please login.");
                    navigate("/"); // Redirect ไปหน้า Login
                } else if (error.response?.status === 403) {
                    alert("Access Denied: You do not have the required permissions.");
                }
            } finally {
                setIsLoading(false); // ปิดสถานะการโหลด
            }
        };

        fetchBranches();
    }, [navigate]);

    const handleAddBranch = (newBranch) => {
        setBranches([...branches, newBranch]); // อัปเดต Branch ใหม่
    };

    const handleDeleteBranch = async (id) => {
        if (!window.confirm("Are you sure you want to delete this branch?")) return;

        try {
            await axios.delete(`http://localhost:5293/api/Admin/branches/${id}`, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("authToken")}`
                }
            });
            alert("Branch deleted successfully!");
            setBranches(branches.filter(branch => branch.id !== id)); // ลบ Branch ออกจาก State
        } catch (error) {
            console.error("Failed to delete branch:", error);
            alert(error.response?.data?.message || "Failed to delete branch.");
        }
    };

    const handleEditBranch = async (id, updatedBranch) => {
        try {
            await axios.put(
                `http://localhost:5293/api/Admin/branches/${id}`,
                updatedBranch,
                {
                    headers: {
                        Authorization: `Bearer ${localStorage.getItem("authToken")}`
                    }
                }
            );
            alert("Branch updated successfully!");
            setBranches(
                branches.map(branch =>
                    branch.id === id ? { ...branch, ...updatedBranch } : branch
                )
            );
            setEditBranch(null);
        } catch (error) {
            console.error("Failed to update branch:", error);
            alert(error.response?.data?.message || "Failed to update branch.");
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
                <p>Loading branches...</p> // แสดงข้อความขณะกำลังโหลด
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
                                <td><a href={`/branch/${id}`} className="detail-link">Detail</a></td>
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
                                        onClick={() => handleDeleteBranch(id)}
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