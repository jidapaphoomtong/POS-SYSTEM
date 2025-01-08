import React, { useEffect, useState } from "react";
import axios from "axios";
import Cookies from "js-cookie";
import { useParams } from "react-router-dom";
import "../../styles/branch.css";
import { useNavigate } from "react-router-dom";

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
                const response = await axios.get(`https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/Admin/branches/${branchId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                if (response.data.success) {
                    setBranch(response.data.data);
                } else {
                    alert(response.data.message || "Failed to fetch branch details.");
                }
            } catch (error) {
                console.error("Failed to fetch branch details:", error);
                alert(error.response ? error.response.data.message : "Failed to load branch details.");
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