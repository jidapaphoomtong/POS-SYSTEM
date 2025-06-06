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
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 6; // จำนวนสินค้าที่จะแสดงต่อหน้า

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
                        Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
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
        const options = {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            hour12: false, // ถ้าคุณต้องการเวลาในรูปแบบ 24 ชั่วโมง
        };
        const date = new Date(dateString);
        return date.toLocaleString('th-TH', options); // ปรับเป็น 'th-TH' เพื่อให้ได้รูปแบบ DD/MM/YYYY HH:mm
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
        receiptWindow.document.write(`<p>วันที่: ${new Date(receipt.date).toLocaleString('th-TH', { timeZone: 'Asia/Bangkok' })}</p>`);
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
                    Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
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

    // ฟังก์ชันสำหรับการจัดการ pagination
    const handlePageChange = (pageNumber) => {
        setCurrentPage(pageNumber);
    };

    // คำนวณสินค้าที่จะแสดง
    const indexOfLastPurchase = currentPage * itemsPerPage;
    const indexOfFirstPurchase = indexOfLastPurchase - itemsPerPage;
    const currentPurchases = purchases.slice(indexOfFirstPurchase, indexOfLastPurchase);

    const totalPages = Math.ceil(purchases.length / itemsPerPage);

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
                                            <td style={{ textAlign: 'center' }}>
                                                <a href={`/${branchId}/purchase/${purchase.id}`} className="detail-link">{purchase.id}</a>
                                            </td>
                                            <td>{formatDate(purchase.date)}</td>
                                            <td>฿{purchase.total}</td>
                                            <td>{purchase.seller}</td>
                                            <td>{purchase.paymentMethod}</td>
                                            <td className="actions" style={{ textAlign: 'center' }}>
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

                    <div className="pagination">
                        {Array.from({ length: totalPages }, (_, index) => (
                            <button
                                key={index + 1}
                                onClick={() => handlePageChange(index + 1)}
                                className={currentPage === index + 1 ? 'active' : ''}
                            >
                                {index + 1}
                            </button>
                        ))}
                    </div>

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