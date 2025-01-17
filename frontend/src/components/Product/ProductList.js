import axios from "axios";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "../../styles/product.css";
import Cookies from "js-cookie";
import ConfirmationModal from "./ConfirmationModal";
import Navbar from "../bar/Navbar";
import Sidebar from "../bar/Sidebar";
import { FaEdit, FaTrash } from "react-icons/fa";

const ProductList = () => {
    const [products, setProducts] = useState([]);
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [deleteProductId, setDeleteProductId] = useState(null);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
    
    // ดึง branchId และ categoryId
    const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");
    const categoryId = new URLSearchParams(window.location.search).get("category") || Cookies.get("categoryId");

    const getProductStatus = (stock) => {
        const UNLIMITED_STOCK = Number.MAX_SAFE_INTEGER; // ใช้ค่านี้เป็นตัวแทนสำหรับ stock ที่ไม่มีวันหมด

    if (stock === UNLIMITED_STOCK) {
        return { text: "Ready to sell", color: "green", icon: "✅" };
    }
        if (stock <= 0) {
            return { text: "Out of stock", color: "red", icon: "❌" };
        }
        if (stock < 25) {
            return { text: "Low stock", color: "orange", icon: "⚠️" };
        }
        return { text: "Ready to sell", color: "green", icon: "✅" };
    };

    useEffect(() => {
        const fetchProducts = async () => {
            const token = Cookies.get("authToken");
            if (!token) {
                alert("Your session has expired. Please login again.");
                navigate("/");
                return;
            }

            try {
                setIsLoading(true);
                const response = await axios.get(`/api/Product/branches/${branchId}/products`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                });

                if (response.data.success) {
                    const productData = response.data.data.map(pro => ({
                        id: pro.id,
                        ...pro.data
                    }));
                    setProducts(productData);
                } else {
                    alert(response.data.message || "Failed to fetch products.");
                }
            } catch (error) {
                console.error("Failed to fetch products:", error);
                if (error.response?.status === 401) {
                    alert("Unauthorized. Please login.");
                    navigate("/");
                } else {
                    alert("Failed to fetch products.");
                }
            } finally {
                setIsLoading(false);
            }
        };

        fetchProducts();
    }, [navigate, branchId, categoryId]);

    const handleOpenDeleteModal = (id) => {
        setDeleteProductId(id);
        setIsConfirmModalOpen(true);
    };

    const handleCloseDeleteModal = () => {
        setDeleteProductId(null);
        setIsConfirmModalOpen(false);
    };

    const handleConfirmDelete = async () => {
        const token = Cookies.get("authToken");
        if (!token) {
            alert("Your session has expired. Please login again.");
            navigate("/");
            return;
        }

        try {
            const response = await axios.delete(`/api/Product/branches/${branchId}/products/${deleteProductId}`, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });

            if (response.status === 200) {
                alert("Product deleted successfully!");
                setProducts(products.filter((product) => product.id !== deleteProductId));
                handleCloseDeleteModal();
            } else {
                alert("Failed to delete product.");
            }
        } catch (error) {
            console.error("Failed to delete product:", error);
            alert("Failed to delete product: " + (error.response?.data?.message || "Unknown error"));
        }
    };

    return (
        <div className="product-container">
            <Navbar />
            <div className="content">
                <Sidebar />
                <div className="main-content">
                    <div className="header">
                        <h2>Product Management ({products.length})</h2>
                        <button
                            className="add-button"
                            onClick={() => navigate(`/add-product/${branchId}`)}
                            disabled={isLoading}
                        >
                            Add Product
                        </button>
                    </div>

                    {isLoading ? (
                        <p>Loading products...</p>
                    ) : (
                        <table className="product-table">
                            <thead>
                                <tr>
                                    <th>Product ID</th>
                                    <th>Product Image</th>
                                    <th>Product Name</th>
                                    <th>Price</th>
                                    <th>Stock</th>
                                    <th>Status</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {products.map(({ id, ImgUrl, productName, price, stock }) => {
                                    const status = getProductStatus(stock);
                                    return (
                                        <tr key={id}>
                                            <td>{id}</td>
                                            <td><img src={ImgUrl} alt={productName} style={{ width: "50px", height: "50px" }} /></td>
                                            <td>{productName}</td>
                                            <td>{price}</td>
                                            <td>{stock}</td>
                                            <td style={{ color: status.color }}>{status.icon} {status.text}</td>
                                            <td>
                                                <div className="row-product">
                                                    <button className="icon-button" onClick={() => navigate(`/edit-product/${id}?branch=${branchId}`)}>
                                                        <FaEdit className="icon icon-blue" />
                                                    </button>
                                                    <button className="icon-button" onClick={() => handleOpenDeleteModal(id)}>
                                                        <FaTrash className="icon icon-red" />
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    )}

                    {isConfirmModalOpen && (
                        <ConfirmationModal
                            isOpen={isConfirmModalOpen}
                            onClose={handleCloseDeleteModal}
                            onConfirm={handleConfirmDelete}
                            message="Are you sure you want to delete this product?"
                        />
                    )}
                </div>
            </div>
        </div>
    );
};

export default ProductList;