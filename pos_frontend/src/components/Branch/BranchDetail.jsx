import React, { useEffect, useState } from "react";
import axios from "axios";
import Cookies from "js-cookie";
import { useParams } from "react-router-dom";
import "../../styles/branch.css";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

const BranchDetail = () => {
    const { branchId } = useParams(); // ดึง Branch ID จาก URL
    const [branch, setBranch] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const navigate = useNavigate();

    // ฟังก์ชันดึงข้อมูล Branch
    useEffect(() => {
        const fetchBranchDetails = async () => {
            const token = Cookies.get("authToken"); // ดึง Token จาก Cookie

            try {
                const response = await axios.get(`/api/Branch/branches/${branchId}`, {
                    headers: {
                        Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                    },
                    withCredentials: true,
                });

                if (response.data.success) {
                    setBranch(response.data.data);
                } else {
                    toast.error(response.data.message || "Failed to fetch branch details.");
                }
            } catch (error) {
                console.error("Failed to fetch branch details:", error);
                toast.error(error.response ? error.response.data.message : "Failed to load branch details.");
            } finally {
                setIsLoading(false);
            }
        };

        fetchBranchDetails();
    }, [branchId]);

    if (isLoading) {
        return <p>Loading...</p>;
    }

    if (!branch) {
        return <p>Branch not found.</p>;
    }

    return (
        <div className="branch-detail-container">
            <h2>{branch.id} : {branch.name}</h2>
            <img src={branch.iconUrl} alt={branch.name} style={{ maxWidth: "100%", height: "auto" }} />
            {/* <p>รหัสสาขา: {branch.id}</p>
            <p>{branch.name}</p> */}
            <p>ตำแหน่ง📍: {branch.location}</p>
            <button onClick={() => navigate(-1)}>Back</button>
        </div>
    );
};

export default BranchDetail;