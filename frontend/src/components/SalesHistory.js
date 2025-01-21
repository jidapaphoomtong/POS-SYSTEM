import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import axios from "axios";
import Cookies from "js-cookie";
import "../styles/history.css";
import { FaPrint } from "react-icons/fa";
import Navbar from "./bar/Navbar"; // นำเข้า Navbar
import Sidebar from "./bar/Sidebar"; // นำเข้า Sidebar

const SalesHistory = () => {
    const [purchases, setPurchases] = useState([]);
    const [loading, setLoading] = useState(true);
    const { branchId } = useParams();

    useEffect(() => {
        const fetchPurchases = async () => {
            const token = Cookies.get("authToken");

            if (!branchId) {
                alert("Branch ID is missing!");
                return;
            }

            try {
                const response = await axios.get(`/api/Purchase/all-purchases/${branchId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                if (response.data) {
                    const purchasesData = response.data.map(pur => ({
                        id: pur.id || "N/A", 
                        total: pur.total || 0,
                        paidAmount: pur.paidAmount || 0,
                        change: pur.change || 0,
                        date: pur.date || new Date().toISOString(),
                        seller: pur.seller || "Unknown",
                        paymentMethod: pur.paymentMethod || "Unknown", // เพิ่ม paymentMethod
                    }));
                    setPurchases(purchasesData);
                    console.log("Purchase Data", purchasesData);
                } else {
                    console.log("API response was not successful:", response.data);
                    alert(`Failed to fetch purchases: ${response.data.message}`);
                }
            } catch (error) {
                console.error("Error fetching purchases:", error);
                alert("Error fetching purchases.");
            } finally {
                setLoading(false);
            }
        };

        fetchPurchases();
    }, [branchId]);

    const handlePrint = (id) => {
        window.print(); // คุณสามารถปรับปรุงให้แสดงผลแบบ bill จริงได้ที่นี่
    };

    return (
        <div className="history-container">
            <Navbar />
            <div className="content">
                <Sidebar />
                <div className="main-content">
                    <div className="header">
                        <h2>Sales History ({purchases.length})</h2>
                    </div>
                    <div className="sales-history">
                        {loading ? (
                            <p>Loading...</p>
                        ) : (
                            <table>
                                <thead>
                                    <tr>
                                        <th>Bill No.</th>
                                        <th>Date</th>
                                        <th>Total</th>
                                        <th>Employee</th>
                                        <th>Payment Method</th> {/* เปลี่ยนเป็น Payment Method */}
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {purchases.length > 0 ? (
                                        purchases.map((purchase) => (
                                            <tr key={purchase.id}>
                                                <td>{purchase.id}</td>
                                                <td>{new Date(purchase.date).toLocaleDateString()}</td>
                                                <td>${purchase.total}</td>
                                                <td>{purchase.seller}</td>
                                                <td>{purchase.paymentMethod}</td> {/* เปลี่ยนเป็น paymentMethod */}
                                                <td className="actions">
                                                    <button onClick={() => handlePrint(purchase.id)}>
                                                        <FaPrint className="icon" />
                                                    </button>
                                                </td>
                                            </tr>
                                        ))
                                    ) : (
                                        <tr>
                                            <td colSpan="6">No purchases found.</td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SalesHistory;