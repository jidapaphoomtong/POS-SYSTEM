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
    const [showOrderSummary, setShowOrderSummary] = useState(false);  

    useEffect(() => {
        const fetchData = async () => {
            const token = Cookies.get("authToken");
            const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");
            
            if (!branchId) {
                alert("Branch ID is missing!");
                return;
            }

            setLoading(true); // Start loading

            try {
                // Fetch Products
                const productResponse = await axios.get(`http://localhost:5293/api/Product/branches/${branchId}/products`, {
                    headers: { 
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}` 
                    },
                    withCredentials: true,
                });

                if (productResponse.data.success) {
                    const products = productResponse.data.data.map(item => ({
                        Id: item.data.Id,
                        ImgUrl: item.data.ImgUrl,
                        productName: item.data.productName,
                        description: item.data.description,
                        stock: item.data.stock,
                        price: item.data.price,
                        categoryId: item.data.categoryId,
                        branchId: item.data.branchId,
                    }));
                    setItems(products); // Set the products in state
                } else {
                    alert(productResponse.data.message);
                }

                // Fetch Categories
                const categoryResponse = await axios.get(`http://localhost:5293/api/Category/branches/${branchId}/getCategory`, {
                    headers: { 
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}` 
                    },
                    withCredentials: true,  
                });
                
                if (categoryResponse.data.success) {
                    setCategories(categoryResponse.data.data); // Set categories
                }

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
            if (newItems[item.Id]) {
                newItems[item.Id].quantity += 1 ;
            } else {
                newItems[item.Id] = { ...item, quantity: 1 };
            }
            return newItems;
        });
    
        setShowOrderSummary(true);
    };

    const handleRemoveItem = (id) => {
        setSelectedItems(prevItems => {
            const newItems = { ...prevItems };
            delete newItems[id];
    
            // ตรวจสอบว่าหากไม่มีสินค้าใน selectedItems ให้ซ่อน Order Summary
            if (Object.keys(newItems).length === 0) {
                setShowOrderSummary(false);
            }
            
            return newItems;
        });
    };

    const handleSearchChange = (e) => {
        setSearchTerm(e.target.value);
    };

    const filteredItems = items.filter(item => 
        item.productName.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const calculateTotal = () => {
        return Object.values(selectedItems).reduce((total, item) => total + item.price * item.quantity, 0);
    };

    const handlePlaceOrder = async () => {
        const token = Cookies.get("authToken");
        const branchId = Cookies.get("branchId");
        
        for (const item of Object.values(selectedItems)) {
            const response = await axios.post(`http://localhost:5293/api/Product/${branchId}/products/${item.Id}/reducestock`, { quantity: item.quantity }, {
                headers: { 
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}` 
                },
                withCredentials: true,
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
                    {loading && <div>Loading...</div>} {/* Loading indicator */}
                    
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
                                {category.name} <span>{category.count}</span>
                            </button>
                        ))}
                    </div>
                    <div className="main-content-order">
                    <div className="items-grid">
                        {filteredItems.map((item) => (
                            <div key={item.Id} className="item-card" onClick={() => handleSelectItem(item)}>
                                <img src={item.ImgUrl} alt={item.productName} />
                                <p>{item.productName}</p>
                                <p>{item.price} บาท</p>
                            </div>
                        ))}
                    </div>
                    
                    {showOrderSummary && ( // แสดง Order Summary ถ้ามีการเลือกสินค้า
                        <div className="order-summary">
                            <h2>ORDER SUMMARY</h2>
                            {Object.values(selectedItems).length > 0 ? ( // ตรวจสอบว่ามีการเลือกสินค้าหรือไม่
                                Object.values(selectedItems).map(item => (
                                    <div key={item.Id} className="order-item">
                                        <p>{item.productName} : ฿{item.price} x {item.quantity}</p>
                                        <button onClick={() => handleRemoveItem(item.Id)}>❌</button>
                                    </div>
                                ))
                            ) : (
                                <p>No items selected</p> // แจ้งเตือนเมื่อไม่มีรายการที่เลือก
                            )}
                            <hr />
                            <p>Total: {calculateTotal()} บาท</p>
                            <button onClick={handlePlaceOrder}>Place Order</button>
                        </div>
                    )}
                    </div>
                </div>
            </div>
        </div>
    );
}