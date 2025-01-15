import { useEffect, useState } from "react";
import "../../styles/product.css";
import { useParams } from "react-router-dom";
import axios from "axios";
import Cookies from "js-cookie";
import { useNavigate } from "react-router-dom";

const EditProduct = () => {
    const navigate = useNavigate();
    const { productId } = useParams(); // ดึง Product ID จาก URL

    const [formData, setFormData] = useState({
        productName: "",
        productCode: "",
        price: "",
        quantity: "",
    });
    const [isLoading, setIsLoading] = useState(false);

    // ฟังก์ชันโหลดรายละเอียดของ Product
    useEffect(() => {
        if (!productId) {
            alert("Product ID is missing.");
            navigate("/ProductList");
            return;
        }

        const token = Cookies.get("authToken"); // ดึง Token จาก Cookie

        const fetchProduct = async () => {
            try {
                setIsLoading(true);
                const response = await axios.get(`/api/Product/${branchId}/products/${productId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                if (response.data.success) {
                    setFormData({
                        productName: response.data.data.productName,
                        productCode: response.data.data.productCode,
                        price: response.data.data.price,
                        quantity: response.data.data.quantity,
                    });
                } else {
                    alert(response.data.message || "Failed to fetch product details.");
                    navigate("/ProductList");
                }
            } catch (error) {
                console.error("Failed to fetch product details:", error);
                alert(error.response ? error.response.data.message : "Failed to load product details.");
                navigate("/ProductList");
            } finally {
                setIsLoading(false);
            }
        };

        fetchProduct();
    }, [productId, navigate]);

    // ฟังก์ชันจัดการการเปลี่ยนค่าช่อง Input
    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    // ฟังก์ชันจัดการการบันทึกข้อมูล
    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.productName || !formData.productCode || !formData.price || !formData.quantity) {
            alert("Please fill out all fields!");
            return;
        }

        try {
            setIsLoading(true);
            const token = Cookies.get("authToken"); // ใช้ Token จาก Cookies
            const response = await axios.put(`/api/Product/branches/${branchId}/products/${productId}`, formData, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });

            alert("Product updated successfully!");
            navigate("/ProductList"); // นำทางกลับไปที่ Product List
        } catch (error) {
            console.error("Failed to update product:", error);
            alert(error.response ? error.response.data.message : "Failed to update product.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="edit-product-container">
            <h2>Edit Product</h2>
            {isLoading ? (
                <p>Loading...</p>
            ) : (
                <form onSubmit={handleSubmit}>
                    <input
                        type="text"
                        name="productName"
                        placeholder="Product Name"
                        value={formData.productName}
                        onChange={handleChange}
                    />
                    <input
                        type="text"
                        name="productCode"
                        placeholder="Product Code"
                        value={formData.productCode}
                        onChange={handleChange}
                    />
                    <input
                        type="number"
                        name="price"
                        placeholder="Price"
                        value={formData.price}
                        onChange={handleChange}
                    />
                    <input
                        type="number"
                        name="quantity"
                        placeholder="Quantity"
                        value={formData.quantity}
                        onChange={handleChange}
                    />
                    <div className="form-buttons">
                        <button type="button" onClick={() => navigate('/ProductList')} disabled={isLoading}>
                            Cancel
                        </button>
                        <button type="submit" disabled={isLoading}>
                            {isLoading ? "Saving..." : "Save"}
                        </button>
                    </div>
                </form>
            )}
        </div>
    );
};

export default EditProduct;