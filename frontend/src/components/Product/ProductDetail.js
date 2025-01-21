import { useEffect, useState } from "react";
import axios from "axios";
import Cookies from "js-cookie";
import { useParams } from "react-router-dom";
import { useNavigate } from "react-router-dom";
import "../../styles/branch.css";

const ProductDetail = () => {
    // const branchId = new URLSearchParams(window.location.search).get("branch"); // ดึง Branch ID จาก URL
    const { productId, branchId } = useParams();
    const [product, setProduct] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const navigate = useNavigate();
    
    const [formData, setFormData] = useState({
        productName:"",
        imgUrl:"",
        description:"",
        price:"",
        stock:"",
    });

    useEffect(() => {
        const fetchProductDetails = async () => {
            const token = Cookies.get("authToken"); // ดึง Token จาก Cookie

            try {
                const response = await axios.get(`/api/Product/branches/${branchId}/products/${productId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                if (response.status === 200 && response.data){
                    setFormData({
                        id: response.data.id, // เก็บ ID ที่ถูกต้อง
                        productName: response.data.productName,
                        imgUrl: response.data.imgUrl,
                        description: response.data.description,
                        price: response.data.price,
                        stock: response.data.stock
                    });
                } else {
                    alert(response.data.message || "Failed to fetch product details.");
                }
            } catch (error) {
                console.error("Failed to fetch product details:", error);
                alert(error.response ? error.response.data.message : "Failed to load product details.");
            } finally {
                setIsLoading(false);
            }
        };

        fetchProductDetails();
    }, [branchId, productId]); // ใช้ productId ด้วยใน dependencies

    if (isLoading) {
        return <p>Loading...</p>;
    }

    if (!formData.productName) {
        return <p>Product not found.</p>;
    }

    return (
        <div className="branch-detail-container">
            <h2>{formData.id} : {formData.productName}</h2>
            <img src={formData.imgUrl} alt={formData.productName} style={{ maxWidth: "50%", height: "auto" }} />
            <p>Description: {formData.description}</p>
            <p>Price: {formData.price.toFixed(2)} บาท</p>
            <p>Stock: {formData.stock}</p>
            <button onClick={() => navigate(-1)}>Back</button>
        </div>
    );
};

export default ProductDetail
