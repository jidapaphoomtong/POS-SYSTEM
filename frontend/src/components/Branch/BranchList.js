import axios from "axios";
import { useEffect, useState } from "react";
// import AddBranch from "./AddBranch";
import EditBranch from "./EditBranch";
import ConfirmationModal from "./ConfirmationModal";
import { useNavigate } from "react-router-dom";
import "../../styles/branch.css";
import Cookies from "js-cookie";
import { FaEdit } from "react-icons/fa";
import { FaTrash } from "react-icons/fa6";
import { toast } from "react-toastify";

const BranchList = () => {
    const [branches, setBranches] = useState([]); 
    // const [showAddForm, setShowAddForm] = useState(false);
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [editBranch, setEditBranch] = useState(null);
    const [deleteBranchId, setDeleteBranchId] = useState(null);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);

    // Fetch Branch List
    useEffect(() => {
        const fetchBranches = async () => {
            const token = Cookies.get("authToken");
            if (!token) {
                toast.error("Your session has expired. Please login again.");
                navigate("/");
                return;
            }

            try {
                setIsLoading(true);
                const response = await axios.get(`/api/Branch/branches`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                setBranches(response.data.data || []);
            } catch (error) {
                console.error("Failed to fetch branches:", error);
                if (error.response?.status === 401) {
                    toast.error("Unauthorized. Please login.");
                    navigate("/");
                } else {
                    toast.error("Failed to fetch branches.");
                }
            } finally {
                setIsLoading(false);
            }
        };

        fetchBranches();
    }, [navigate]);

    // const handleAddBranch = (newBranch) => {
    //     setBranches([...branches, newBranch]);
    //     setShowAddForm(false); // ปิดฟอร์มหลังจากเพิ่ม branch สำเร็จ
    // };

    const handleEditBranch = (updatedBranch) => {
        setBranches(
            branches.map((branch) =>
                branch.id === updatedBranch.id ? { ...branch, ...updatedBranch } : branch
            )
        );
        // setEditBranch(null); // ปิดการแก้ไขเมื่ออัปเดตเสร็จ
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
        const token = Cookies.get("authToken");

        try {
            const response = await axios.delete(`/api/Branch/branches/${deleteBranchId}`, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });

            if (response.status === 200) {
                toast.success("Branch deleted successfully!");
                setBranches(branches.filter((branch) => branch.id !== deleteBranchId));
                handleCloseDeleteModal();
            } else {
                toast.error("Failed to delete branch.");
            }
        } catch (error) {
            console.error("Failed to delete branch:", error);
            toast.error("Failed to delete branch: " + (error.response?.data?.message || "Unknown error"));
        }
    };

    return (
        <div className="branch-container">
            <div className="header">
                <h1>Branch Management</h1>
                <button
                    className="add-button"
                    onClick={() => navigate('/add-branch')} // นำทางไปยัง AddBranch
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
                                <td>
                                        <button
                                            className="icon-button"
                                            onClick={() => {
                                                // setEditBranch({ id, name, location });
                                                navigate(`/edit-branch/${id}`);
                                            }}
                                        >
                                            <FaEdit className=" icon icon-blue" />
                                        </button>
                                        <button
                                            className="icon icon-button"
                                            onClick={() => handleOpenDeleteModal(id)}
                                        >
                                            <FaTrash className="icon-red" />
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

            {editBranch && (
                <EditBranch
                    branchId={editBranch.id}
                    // onClose={() => setEditBranch(null)}
                    onEdit={handleEditBranch} // ส่งฟังก์ชันแก้ไขไปยัง EditBranch
                />
            )}
        </div>
    );
};

export default BranchList;