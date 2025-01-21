import { useEffect, useState } from "react";
import "../../styles/product.css";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import Cookies from "js-cookie";

const EditProduct = () => {
    const navigate = useNavigate();
    const { productId, branchId  } = useParams(); // ดึง Product ID จาก URL
    // const branchId = new URLSearchParams(window.location.search).get("branch"); // ดึง Branch ID จาก URL

    const [formData, setFormData] = useState({
        productName: "",
        ImgUrl: "", // ถูกตั้งค่าให้สามารถอัปเดต URL ของภาพได้
        description: "",
        price: "",
        stock: "",
        categoryId: ""
    });
    const [isLoading, setIsLoading] = useState(false);

    // ฟังก์ชันโหลดรายละเอียดของ Product
    useEffect(() => {
        if (!productId) {
            alert("Product ID is missing.");
            navigate(`/${branchId}/ProductList`);
            return;
        }

        const token = Cookies.get("authToken"); // ดึง Token จาก Cookie

        const fetchProduct = async () => {
            try {
                setIsLoading(true);
                const response = await axios.get(`/api/Product/branches/${branchId}/products/${productId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                // console.log(response)

                if (response.status === 200 && response.data) {
                    // กำหนดค่า formData ตามข้อมูลที่ได้จาก API
                    setFormData({
                        productName: response.data.productName,
                        ImgUrl: response.data.imgUrl,
                        description: response.data.description,
                        price: response.data.price,
                        stock: response.data.stock,
                        categoryId: response.data.categoryId,
                    });
                } else {
                    alert(response.data.message || "Failed to fetch product details.");
                    navigate(`/${branchId}/ProductList`);
                }
            } catch (error) {
                console.error("Failed to fetch product details:", error);
                alert(error.response ? error.response.data.message : "Failed to load product details.");
                navigate(`/${branchId}/ProductList`);
            } finally {
                setIsLoading(false);
            }
        };

        fetchProduct();
    }, [productId, branchId, navigate]);

    // ฟังก์ชันจัดการการเปลี่ยนค่าช่อง Input
    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    // ฟังก์ชันจัดการการบันทึกข้อมูล
    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.productName || !formData.ImgUrl || !formData.price || !formData.stock) {
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

            if (response.data.message) {
                alert(response.data.message); 
            }
            navigate(`/${branchId}/ProductList`); // นำทางกลับไปที่ Product List
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
                        name="categoryId"
                        placeholder="Category ID"
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
            )}
        </div>
    );
};

export default EditProduct;