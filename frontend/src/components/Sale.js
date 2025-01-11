import React, { useState } from "react";
import NavBar from "../components/bar/Navbar";
import SideBar from "../components/bar/Sidebar";
import "../styles/Sale.css";

export default function Sale() {
    const [categories, setCategories] = useState([
        { name: "All", count: 116 },
        { name: "Pizza", count: 20 },
        { name: "Burger", count: 15 },
        { name: "Chicken", count: 10 },
        { name: "Bakery", count: 18 },
        { name: "Beverage", count: 12 },
        { name: "Seafood", count: 16 },
    ]);
    
    const [items, setItems] = useState([
        { id: 1, image: "https://via.placeholder.com/100", name: "Fried Chicken Original", price: 35 },
        { id: 2, image: "https://via.placeholder.com/100", name: "Cheese Ball", price: 25 },
        { id: 3, image: "https://via.placeholder.com/100", name: "French Fries", price: 10 },
        { id: 4, image: "https://via.placeholder.com/100", name: "Fried Chicken Original", price: 35 },
        { id: 5, image: "https://via.placeholder.com/100", name: "Cheese Ball", price: 25 },
        { id: 6, image: "https://via.placeholder.com/100", name: "French Fries", price: 10 },
        { id: 7, image: "https://via.placeholder.com/100", name: "Fried Chicken Original", price: 35 },
        { id: 8, image: "https://via.placeholder.com/100", name: "Cheese Ball", price: 25 },
        { id: 9, image: "https://via.placeholder.com/100", name: "French Fries", price: 10 },
        { id: 10, image: "https://via.placeholder.com/100", name: "French Fries", price: 10 },
        // เพิ่มสินค้าอื่น ๆ ได้ที่นี่
    ]);

    const [selectedItems, setSelectedItems] = useState({});
    
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
                        <button onClick={() => alert('Order placed!')}>Place Order</button>
                    </div>
                </div>
            </div>
        </div>
    );
}