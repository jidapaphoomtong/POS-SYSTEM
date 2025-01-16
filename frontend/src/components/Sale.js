import React, { useState, useEffect } from "react";
import { useNavigate } from 'react-router-dom';
import NavBar from "../components/bar/Navbar";
import SideBar from "../components/bar/Sidebar";
import "../styles/Sale.css";
import axios from 'axios';
import Cookies from 'js-cookie';
import { FcPlus } from "react-icons/fc";
import { AiFillMinusCircle } from "react-icons/ai";
import { FaTrash } from "react-icons/fa";

export default function Sale() {
    const [categories, setCategories] = useState([]);
    const [items, setItems] = useState([]);
    const [selectedItems, setSelectedItems] = useState({});
    const [searchTerm, setSearchTerm] = useState("");
    const [loading, setLoading] = useState(false);
    const [showOrderSummary, setShowOrderSummary] = useState(false);
    const [filterItems, setFilterItems] = useState([]);
    const navigate = useNavigate();
    const [selectedCategory, setSelectedCategory] = useState("all");

    useEffect(() => {
        const fetchData = async () => {
            const token = Cookies.get("authToken");
            const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");
            
            if (!branchId) {
                alert("Branch ID is missing!");
                return;
            }

            setLoading(true); 

            try {
                // Fetch Products
                const productResponse = await axios.get(`/api/Product/branches/${branchId}/products`, {
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
                    setItems(products);
                    setFilterItems(products); // Initialize filtered items
                } else {
                    alert(productResponse.data.message);
                }

                // Fetch Categories
                const categoryResponse = await axios.get(`/api/Category/branches/${branchId}/getCategory`, {
                    headers: { 
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}` 
                    },
                    withCredentials: true,  
                });
                console.log(categoryResponse.data); // เพิ่มบรรทัดนี้เพื่อตรวจสอบข้อมูล
                
                if (categoryResponse.data.success) {
                    setCategories([{ Id: "all", Name: "All" }, ...categoryResponse.data.data]); // Include "All" category
                }
            } catch (error) {
                console.error("Failed to fetch data:", error);
                alert("Failed to fetch data!");
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [navigate]);

    const handleCategorySelect = (categoryId) => {
        setSelectedCategory(categoryId);
        if (categoryId === "all") {
            setFilterItems(items); // Show all items
        } else {
            const filtered = items.filter(item => item.categoryId === categoryId);
            setFilterItems(filtered); // Show filtered items
        }
    };

    const handleSelectItem = (item) => {
        setSelectedItems((prevItems) => ({
            ...prevItems,
            [item.Id]: {
                ...item,
                quantity: (prevItems[item.Id] ? prevItems[item.Id].quantity : 0) + 1,
            }
        }));
        
        setShowOrderSummary(true);
    };

    const handleIncreaseQuantity = (itemId) => {
        setSelectedItems((prevItems) => ({
            ...prevItems,
            [itemId]: {
                ...prevItems[itemId],
                quantity: prevItems[itemId].quantity + 1,
            }
        }));
    };

    const handleDecreaseQuantity = (itemId) => {
        setSelectedItems((prevItems) => {
            const newItems = { ...prevItems };
            if (newItems[itemId].quantity > 1) {
                newItems[itemId].quantity -= 1;
            } else {
                delete newItems[itemId];
                setShowOrderSummary(false);
            }
            return newItems;
        });
    };

    const handleRemoveItem = (id) => {
        setSelectedItems(prevItems => {
            const newItems = { ...prevItems };
            delete newItems[id];

            if (Object.keys(newItems).length === 0) {
                setShowOrderSummary(false);
            }
            
            return newItems;
        });
    };

    const handleSearchChange = (e) => {
        setSearchTerm(e.target.value);
    };

    const calculateTotal = () => {
        return Object.values(selectedItems).reduce((total, item) => total + item.price * item.quantity, 0);
    };

    const handlePlaceOrder = () => {
        if (Object.keys(selectedItems).length === 0) {
            alert("Please select at least one item before placing an order.");
            return;
        }
        navigate('/order', { state: { selectedItems } });
    };

    return (
        <div className="sale-page">
            <NavBar />
            <div className="content">
                <SideBar />
                <div className="main-content">
                    {loading && <div>Loading...</div>} 
                    
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
                                key={category.Id} 
                                className="category-btn"
                                onClick={() => handleCategorySelect(category.Id)}
                            >
                                {category.Name}
                            </button>
                        ))}
                    </div>

                    <div className="main-content-order">
                        <div className="items-grid">
                            {filterItems.filter(item => 
                                item.productName.toLowerCase().includes(searchTerm.toLowerCase())
                            ).map((item) => (
                                <div key={item.Id} className="item-card" onClick={() => handleSelectItem(item)}>
                                    <img src={item.ImgUrl} alt={item.productName} />
                                    <p>{item.productName}</p>
                                    <p>{item.price} บาท</p>
                                </div>
                            ))}
                        </div>

                        {showOrderSummary && ( 
                            <div className="order-summary">
                                <h2>ORDER SUMMARY</h2>
                                {Object.values(selectedItems).map(item => (
                                    <div key={item.Id} className="order-item">
                                        <p>{item.productName} : {item.price} บาท</p>
                                        <button className="icon-button" onClick={() => handleDecreaseQuantity(item.Id)}>
                                            <AiFillMinusCircle className="icon icon-blue" />
                                        </button>
                                        <p> x {item.quantity}</p>
                                        <button className="icon-button" onClick={() => handleIncreaseQuantity(item.Id)}>
                                            <FcPlus className="icon" />
                                        </button>
                                        <button className="icon-button" onClick={() => handleRemoveItem(item.Id)}>
                                            <FaTrash className="icon icon-red" />
                                        </button>
                                    </div>
                                ))}
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