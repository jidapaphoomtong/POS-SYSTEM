import React, { useEffect, useState } from 'react';
import axios from 'axios';
import Cookies from 'js-cookie';
import { useParams } from 'react-router-dom';
import { toast } from "react-toastify";
import "../styles/purchase.css"

const PurchaseDetail = () => {
    const { branchId, purchaseId } = useParams();
    const [purchase, setPurchase] = useState(null);
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
    
    useEffect(() => {
        const fetchPurchaseDetails = async () => {
            const token = Cookies.get("authToken");
            try {
                const response = await axios.get(`/api/Purchase/branches/${branchId}/purchases/${purchaseId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });
                setPurchase(response.data);
            } catch (error) {
                console.error("Error fetching purchase details:", error);
                toast.error("Error fetching purchase details:", error);

            }
        };

        fetchPurchaseDetails();
    }, [branchId, purchaseId]);

    if (!purchase) {
        return <p>Loading...</p>;
    }

    return (
        <div className="purchase-detail">
            <h2 style={{ textAlign: 'center' }} >Bill No: {purchase.id}</h2>
            <p>Date: {formatDate(purchase.date)}</p>
            <p>Items:</p>
            <ul>
                {purchase.products.map(item => (
                    <li key={item.id}>{item.productName} - ฿{item.price} x {item.stock}</li>
                ))}
            </ul>
            <p>Total: ฿{purchase.total}</p>
            <p>Paid Amount: ฿{purchase.paidAmount}</p>
            <p>Change: ฿{purchase.change}</p>
            <p>Seller: {purchase.seller}</p>
            <p>Payment Method: {purchase.paymentMethod}</p>
            <button onClick={() => window.history.back()}>Back</button>
        </div>
    );
};

export default PurchaseDetail;