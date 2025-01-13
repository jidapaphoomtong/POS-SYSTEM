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
    const [searchTerm, setSearchTerm] = useState("");
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const fetchData = async () => {
            const token = Cookies.get("authToken");
            const branchId = Cookies.get("branchId");

            if (!branchId) {
                alert("Branch ID is missing!");
                return;
            }

            setLoading(true); // Start loading
            try {
                const productResponse = await axios.get(`https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/products/branches/${branchId}/products`, {
                    headers: { Authorization: `Bearer ${token}` },  
                });
                const categoryResponse = await axios.get(`https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/categories/branches/${branchId}/getCategory`, {
                    headers: { Authorization: `Bearer ${token}` },  
                });
                setItems(productResponse.data);
                setCategories(categoryResponse.data);
            } catch (error) {
                console.error("Failed to fetch data:", error);
                alert("Failed to fetch data!");
            }
            setLoading(false); // End loading
        };

        fetchData();
    }, []);

    const handleCategorySelect = (categoryId) => {
        const filteredItems = items.filter(item => item.categoryId === categoryId);
        setItems(filteredItems.length ? filteredItems : items);
    };

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

    const handleSearchChange = (e) => {
        setSearchTerm(e.target.value);
    };

    const filteredItems = items.filter(item => 
        item.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

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
        const branchId = Cookies.get("branchId");
        for (const item of Object.values(selectedItems)) {
            const response = await axios.post(`https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/products/${branchId}/products/${item.id}/reducestock`, { quantity: item.quantity }, {
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
                            value={searchTerm}
                            onChange={handleSearchChange}
                        />
                    </div>
                    <div className="categories">
                        {categories.map((category) => (
                            <button 
                                key={category.id} 
                                className="category-btn"
                                onClick={() => handleCategorySelect(category.id)}
                            >
                                {category.name} <span>{category.count} items</span>
                            </button>
                        ))}
                    </div>
                    <div className="items-grid">
                        {filteredItems.map((item) => (
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