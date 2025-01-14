import React from "react";
import { useLocation } from "react-router-dom";

export default function Order() {
    const location = useLocation();
    const selectedItems = location.state?.selectedItems || {}; // ดึงข้อมูลที่ถูกเลือกจาก Sale

    const calculateTotal = () => {
        return Object.values(selectedItems).reduce((total, item) => total + item.price * item.quantity, 0);
    };

    return (
        <div className="order-page">
            <h2>ORDER SUMMARY</h2>
            {Object.values(selectedItems).length > 0 ? (
                Object.values(selectedItems).map(item => (
                    <div key={item.Id} className="order-item">
                        <p>{item.productName} : ฿{item.price} x {item.quantity}</p>
                    </div>
                ))
            ) : (
                <p>No items selected</p>
            )}
            <hr />
            <p>Total: {calculateTotal()} บาท</p>
            <button>Place Order</button> {/* เพิ่มฟังก์ชันการวาง ORDER ตามต้องการ */}
        </div>
    );
}