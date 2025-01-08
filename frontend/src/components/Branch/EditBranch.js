import { useEffect, useState } from "react";
import "../../styles/branch.css";
import { useParams } from "react-router-dom";
import axios from "axios";
import Cookies from "js-cookie";
import { useNavigate } from "react-router-dom";

const EditBranch = () => {
    const navigate = useNavigate();
    const { branchId } = useParams(); // ดึง Branch ID จาก URL
    // console.log("Branch ID:", branchId);  // ดูค่า branchId

    const [formData, setFormData] = useState({
        name: "",
        location: "",
        iconUrl: "",
    });
    const [isLoading, setIsLoading] = useState(false);

    // ฟังก์ชันโหลดรายละเอียดของ Branch
    useEffect(() => {
        if (!branchId) {
            alert("Branch ID is missing.");
            navigate("/BranchList");
            return;
        }

        const token = Cookies.get("authToken"); // ดึง Token จาก Cookie

        const fetchBranch = async () => {
            try {
                setIsLoading(true);
                const response = await axios.get(`http://localhost:5293/api/Admin/branches/${branchId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                if (response.data.success) {
                    setFormData({
                        name: response.data.data.name,
                        location: response.data.data.location,
                        iconUrl: response.data.data.iconUrl,
                    });
                } else {
                    alert(response.data.message || "Failed to fetch branch details.");
                    navigate("/BranchList");
                }
            } catch (error) {
                console.error("Failed to fetch branch details:", error);
                alert(error.response ? error.response.data.message : "Failed to load branch details.");
                navigate("/BranchList");
            } finally {
                setIsLoading(false);
            }
        };

        fetchBranch();
    }, [branchId, navigate]);

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
            const token = Cookies.get("authToken"); // ใช้ Token จาก Cookies
            await axios.put(`http://localhost:5293/api/Admin/branches/${branchId}`, {
                name: formData.name,
                location: formData.location,
                iconUrl: formData.iconUrl,
            }, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`, // ใช้ Cookies
                },
                withCredentials: true,
            });

            alert("Branch updated successfully!");
            navigate("/BranchList"); // นำทางกลับไปที่ Branch List
        } catch (error) {
            console.error("Failed to update branch:", error);
            alert(error.response ? error.response.data.message : "Failed to update branch.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="edit-branch-container">
            <h2>Edit Branch</h2>
            {isLoading ? (
                <p>Loading...</p>
            ) : (
                <form onSubmit={handleSubmit}>
                    <input
                        type="text"
                        name="name"
                        placeholder="Branch Name"
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
                    <div className="form-buttons">
                        <button type="button" onClick={() => navigate('/BranchList')} disabled={isLoading}>
                            Cancel
                        </button>
                        <button type="submit" disabled={isLoading}>
                            {isLoading ? "Saving..." : "Save"}
                        </button>
                    </div>
                </form>
            )}
        </div>
    );
};

export default EditBranch;