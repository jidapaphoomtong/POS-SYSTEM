import React, { useState } from "react";
import axios from "axios";
import "../../styles/product.css";
import Cookies from "js-cookie";
import { useParams, useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

const AddProduct = () => {
    const token = Cookies.get("authToken");
    const { branchId } = useParams();

    // ปรับให้สอดคล้องกับ Products class ใน Backend
    const [formData, setFormData] = useState({
        productName: "",
        ImgUrl: "",
        description: "",
        price: "",
        stock: "",
        reorderPoint: "",
        categoryId: "",
        branchId: branchId
    });
    
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Validation
        if (!formData.productName || !formData.ImgUrl || !formData.price || !formData.stock) {
            toast.error("Please fill out all fields!");
            return;
        }

        setIsLoading(true);

        try {
            if (!token) {
                toast.error("No token found. Please log in again.");
                return;
            }
        
            // ส่งข้อมูลไปยัง API
            const response = await axios.post(`/api/Product/add-product/${branchId}`, formData, {
                headers: {
                    Authorization: `Bearer ${token}`, // ใช้แค่ Authorization
                },
                withCredentials: true,
            });

            // console.log(response);
        
            if (response.status === 200) {
                // const message = `Product added: ${formData.productName}`;
                // await axios.post('/api/notifications', { message });

                toast.success("Product added successfully!");
                navigate(`/${branchId}/ProductList`);
            } else {
                toast.error(`Request failed with status: ${response.status}`);
            }
        } catch (error) {
            // console.error("Error adding product:", error);
            if (error.response) {
                toast.error(error.response.data?.Message || "Failed to add product.");
            } else {
                toast.error("Error: " + error.message);
            }
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="add-product-container">
            <h2>Add Product</h2>
            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    name="productName"
                    placeholder="Product Name"
                    value={formData.productName}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="ImgUrl"
                    placeholder="Image URL"
                    value={formData.ImgUrl}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="description"
                    placeholder="Description"
                    value={formData.description}
                    onChange={handleChange}
                />
                <input
                    type="text"
                    name="price"
                    placeholder="Price"
                    value={formData.price}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="stock"
                    placeholder="Stock"
                    value={formData.stock}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="reorderPoint"
                    placeholder="Reorder Point" 
                    value={formData.reorderPoint}
                    onChange={handleChange}
                    required
                />
                <input
                    type="text"
                    name="categoryId"
                    placeholder="Category ID" // เพิ่มตัวเลือกสำหรับ category ID
                    value={formData.categoryId}
                    onChange={handleChange}
                />
                <div className="form-buttons">
                    <button type="button" onClick={() => navigate(`/${branchId}/ProductList`)} disabled={isLoading}>
                        Cancel
                    </button>
                    <button type="submit" disabled={isLoading}>
                        {isLoading ? "Saving..." : "Save"}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default AddProduct;