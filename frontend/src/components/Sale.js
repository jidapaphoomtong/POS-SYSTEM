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
        { id: 1, image: "https://via.placeholder.com/100", name: "Item 1" },
        { id: 2, image: "https://via.placeholder.com/100", name: "Item 2" },
        { id: 3, image: "https://via.placeholder.com/100", name: "Item 3" },
        { id: 4, image: "https://via.placeholder.com/100", name: "Item 4" },
        { id: 5, image: "https://via.placeholder.com/100", name: "Item 5" },
        { id: 6, image: "https://via.placeholder.com/100", name: "Item 6" },
        { id: 7, image: "https://via.placeholder.com/100", name: "Item 7" },
        { id: 8, image: "https://via.placeholder.com/100", name: "Item 8" },
        { id: 9, image: "https://via.placeholder.com/100", name: "Item 9" },
    ]);

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
                            <div key={item.id} className="item-card">
                                <img src={item.image} alt={item.name} />
                                <p>{item.name}</p>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
}
