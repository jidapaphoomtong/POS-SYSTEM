import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import axios from "axios";
import Cookies from "js-cookie";
import "../styles/history.css";
import { FaPrint } from "react-icons/fa";
import Navbar from "./bar/Navbar";
import Sidebar from "./bar/Sidebar";
import { toast } from "react-toastify";

const SalesHistory = () => {
    const [purchases, setPurchases] = useState([]);
    const [loading, setLoading] = useState(true);
    const { branchId } = useParams();
    const [selectedPurchase, setSelectedPurchase] = useState(null);

    useEffect(() => {
        const fetchPurchases = async () => {
            const token = Cookies.get("authToken");
        
            if (!branchId) {
                toast.error("Branch ID is missing!");
                return;
            }
        
            try {
                const response = await axios.get(`/api/Purchase/all-purchases/${branchId}`, { // เปลี่ยนให้ตรงกับ URL ที่คุณได้สร้างไว้
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
                        paymentMethod: pur.paymentMethod || "Unknown",
                        products: pur.products // เพิ่มฟิลด์ products ในที่นี้
                    }));
                    setPurchases(purchasesData);
                } else {
                    toast.error(`Failed to fetch purchases: ${response.data.message}`);
                }
            } catch (error) {
                console.error("Error fetching purchases:", error);
                toast.error("Error fetching purchases.");
            } finally {
                setLoading(false);
            }
        };

        fetchPurchases();
    }, [branchId]);

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
    };

    const handlePrint = (id) => {
        const purchase = purchases.find(p => p.id === id);
        if (purchase) {
            // Call the print function here
            printReceipt(purchase);
        }
    };

    const printReceipt = (receipt) => {
        const receiptWindow = window.open('', '', 'width=600,height=400');
        receiptWindow.document.write('<html><head><title>ใบเสร็จ</title></head><body>');
        receiptWindow.document.write('<h2>ใบเสร็จ</h2>');
        receiptWindow.document.write(`<p>วันที่: ${new Date(receipt.date).toLocaleString()}</p>`);
        receiptWindow.document.write('<h3>รายการสินค้า:</h3>');
        receiptWindow.document.write('<ul>');
    
        receipt.products.forEach(item => { // เปลี่ยนจาก items เป็น products
            receiptWindow.document.write(`<li>${item.productName} : ฿${item.price} x ${item.stock}</li>`); // ใช้ item.stock
        });
    
        receiptWindow.document.write('</ul>');
        receiptWindow.document.write('-------------------------<br>');
        receiptWindow.document.write(`<p><strong>ยอดรวม: ฿${receipt.total}</strong></p>`);
        receiptWindow.document.write(`<p>จำนวนเงินที่จ่าย: ฿${receipt.paidAmount}</p>`);
        receiptWindow.document.write(`<p>เงินทอน: ฿${receipt.change}</p>`);
        receiptWindow.document.write(`<p>ประเภทการจ่ายเงิน: ${receipt.paymentMethod}</p>`);
        receiptWindow.document.write(`<p>ผู้ขาย: ${receipt.seller}</p>`);
        receiptWindow.document.write('</body></html>');
    
        receiptWindow.document.close();
        receiptWindow.focus();
        receiptWindow.print();
        receiptWindow.close();
    };

    const viewPurchaseDetails = async (id) => {
        try {
            const token = Cookies.get("authToken");
            const response = await axios.get(`/api/Purchase/branches/${branchId}/purchases/${id}`, { // ใช้ URL ที่ตรงกับ API
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });
    
            if (response.data) {
                setSelectedPurchase(response.data);
            } else {
                toast.error(`Failed to fetch purchase details: ${response.data.message}`);
            }
        } catch (error) {
            console.error("Error fetching purchase details:", error);
            toast.error("Error fetching purchase details.");
        }
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
                                    <th>Payment Method</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {purchases.length > 0 ? (
                                    purchases.map((purchase) => (
                                        <tr key={purchase.id}>
                                            <td>
                                                <a href={`/${branchId}/purchase/${purchase.id}`} style={{ textAlign: 'center' }} className="detail-link">{purchase.id}</a>
                                            </td>
                                            <td>{formatDate(purchase.date)}</td>
                                            <td>฿{purchase.total}</td>
                                            <td>{purchase.seller}</td>
                                            <td>{purchase.paymentMethod}</td>
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

                    {selectedPurchase && (
                        <div className="modal">
                            <h2>Purchase Details</h2>
                            <p>Bill No: {selectedPurchase.id}</p>
                            <p>Date: {formatDate(selectedPurchase.date)}</p> {/* ใช้ฟังก์ชันที่สร้างขึ้น */}
                            <p>Total: ฿{selectedPurchase.total}</p>
                            <p>Paid Amount: ฿{selectedPurchase.paidAmount}</p>
                            <p>Change: ฿{selectedPurchase.change}</p>
                            <p>Seller: {selectedPurchase.seller}</p>
                            <p>Payment Method: {selectedPurchase.paymentMethod}</p>
                            <h3>Items:</h3>
                            <ul>
                                {selectedPurchase.products.map(item => (
                                    <li key={item.id}>{item.productName} - ฿{item.price} x {item.stock}</li>
                                ))}
                            </ul>
                            <button onClick={() => setSelectedPurchase(null)}>Close</button>
                        </div>
                    )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SalesHistory;