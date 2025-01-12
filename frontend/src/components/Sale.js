import React, { useState, useEffect } from "react";
import NavBar from "../components/bar/Navbar";
import SideBar from "../components/bar/Sidebar";
import "../styles/Sale.css";
import axios from 'axios';
import Cookies from 'js-cookie';

export default function Sale() {
    const [categories, setCategories] = useState([]);
    const [items, setItems] = useState([]);
    const [selectedItems, setSelectedItems] = useState({});
    
    useEffect(() => {
        // ดึงข้อมูลสินค้าและหมวดหมู่จาก API
        const fetchData = async () => {
            const token = Cookies.get("authToken");
            const response = await axios.get('http://localhost:5293/api/products', {
                headers: { Authorization: `Bearer ${token}` },
            });
            setItems(response.data); // สมมติว่า response.data มีสินค้า
            // ต้องการดึง categories ก็ให้ทำการดึงข้อมูลตามความเหมาะสม
        };
        fetchData();
    }, []);

    const handleSelectItem = (item) => {
        setSelectedItems((prevItems) => {
            const newItems = { ...prevItems };
            if (newItems[item.id]) {
                newItems[item.id].quantity += 1;
            } else {
                newItems[item.id] = { ...item, quantity: 1 };
            }
            return newItems;
        });
    };

    const handleRemoveItem = (id) => {
        setSelectedItems((prevItems) => {
            const newItems = { ...prevItems };
            delete newItems[id];
            return newItems;
        });
    };

    const calculateTotal = () => {
        return Object.values(selectedItems).reduce((total, item) => total + item.price * item.quantity, 0);
    };

    const handlePlaceOrder = async () => {
        const token = Cookies.get("authToken");
        for (const item of Object.values(selectedItems)) {
            const response = await axios.post(`http://localhost:5293/api/product/branchId/products/${item.id}/reducestock`, item.quantity, {
                headers: { Authorization: `Bearer ${token}` },
            });
            if (!response.data.success) {
                alert("Failed to reduce stock for some items, please check!");
                return;
            }
        }
        alert('Order placed successfully!');
        setSelectedItems({}); // Clear selected items after order
    };

    return (
        <div className="sale-page">
            <NavBar />
            <div className="content">
                <SideBar />
                <div className="main-content">
                    <div className="search-container">
                        <input
                            className="search-bar"
                            type="text"
                            placeholder="Search for items..."
                        />
                    </div>
                    <div className="categories">
                        {categories.map((category, index) => (
                            <button key={index} className="category-btn">
                                {category.name} <span>{category.count} items</span>
                            </button>
                        ))}
                    </div>
                    <div className="items-grid">
                        {items.map((item) => (
                            <div key={item.id} className="item-card" onClick={() => handleSelectItem(item)}>
                                <img src={item.image} alt={item.name} />
                                <p>{item.name}</p>
                                <p>${item.price}</p>
                            </div>
                        ))}
                    </div>
                    <div className="order-summary">
                        <h2>ORDER SUMMARY</h2>
                        {Object.values(selectedItems).map(item => (
                            <div key={item.id} className="order-item">
                                <p>{item.name} - ${item.price} x {item.quantity}</p>
                                <button onClick={() => handleRemoveItem(item.id)}>❌</button>
                            </div>
                        ))}
                        <hr />
                        <p>Total: ${calculateTotal()}</p>
                        <button onClick={handlePlaceOrder}>Place Order</button>
                    </div>
                </div>
            </div>
        </div>
    );
}