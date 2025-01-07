import { useEffect, useState } from "react";
import "../../styles/branch.css";
import { useParams } from "react-router-dom";
import axios from "axios";
import Cookies from "js-cookie";

const EditBranchModal = ({ onClose, onBranchUpdated }) => {
    const { branchId } = useParams(); // ดึง Branch ID จาก URL
    const [formData, setFormData] = useState({
        name: "",
        location: "",
        iconUrl: "",
    });
    const [isLoading, setIsLoading] = useState(false);
    // ในตัว component

    console.log("Branch ID:", branchId);

    // ฟังก์ชันโหลดรายละเอียดของ Branch
    useEffect(() => {
        const token = Cookies.get("authToken"); // ดึง Token จาก Cookie
        console.log("Login successful, JWT Token received:", token);
        // console.log("Branch ID:", branchId); // ลอกรับ Branch ID
    
        const fetchBranch = async () => {
            try {
                setIsLoading(true);
                const response = await axios.get(`http://localhost:5293/api/Admin/branches/${branchId}`, {
                    headers: {
                        "Authorization": `Bearer ${token}`,
                    },
                    withCredentials: true, // ส่งคำขอพร้อม Cookie
                });
                if (response.data.success) {
                    setFormData({
                        name: response.data.data.name,
                        location: response.data.data.location,
                        iconUrl: response.data.data.iconUrl,
                    });
                } else {
                    alert(response.data.message || "Failed to fetch branch details.");
                    onClose();
                }
            } catch (error) {
                console.error("Failed to fetch branch details:", error);
                alert(error.response ? error.response.data.message : "Failed to load branch details.");
                onClose();
            } finally {
                setIsLoading(false);
            }
        };
    
        fetchBranch();
    }, [branchId, onClose]);

    // ฟังก์ชันจัดการการเปลี่ยนค่าช่อง Input
    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    // ฟังก์ชันจัดการการบันทึกข้อมูล
    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.name || !formData.location) {
            alert("Please fill out all fields!");
            return;
        }
    
        try {
            setIsLoading(true);
            await axios.put(`http://localhost:5293/api/Admin/branches/${branchId}`, {
                name: formData.name,
                location: formData.location,
                iconUrl: formData.iconUrl,
            }, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem("authToken")}`,
                },
            });
            alert("Branch updated successfully!");
            onBranchUpdated();
            onClose();
        } catch (error) {
            console.error("Failed to update branch:", error);
            alert(error.response ? error.response.data.message : "Failed to update branch.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="modal">
            <div className="modal-content">
                <h2>Edit Branch</h2>
                {isLoading ? (
                    <p>Loading...</p>
                ) : (
                    <form onSubmit={handleSubmit}>
                        <input
                            type="text"
                            name="name"
                            placeholder="Branch name"
                            value={formData.name}
                            onChange={handleChange}
                        />
                        <input
                            type="text"
                            name="location"
                            placeholder="location"
                            value={formData.location}
                            onChange={handleChange}
                        />
                        <input
                            type="text"
                            name="iconUrl"
                            placeholder="URL Icon"
                            value={formData.iconUrl}
                            onChange={handleChange}
                        />
                        <div className="modal-buttons">
                            <button type="button" onClick={onClose} disabled={isLoading}>
                                ยกเลิก
                            </button>
                            <button type="submit" disabled={isLoading}>
                                {isLoading ? "Saving..." : "Save"}
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
};

export default EditBranchModal;