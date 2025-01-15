import axios from "axios";
import { useEffect, useState } from "react";
import EditProduct from "./EditProduct";
import ConfirmationModal from "./ConfirmationModal";
import { useNavigate } from "react-router-dom";
import "../../styles/product.css";
import Cookies from "js-cookie";

const ProductList = () => {
    const [products, setProducts] = useState([]); 
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [editProduct, setEditProduct] = useState(null);
    const [deleteProductId, setDeleteProductId] = useState(null);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);

    // Fetch Product List
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

                setProducts(response.data.data || []);
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
    }, [navigate]);

    const handleEditProduct = (updatedProduct) => {
        setProducts(
            products.map((product) =>
                product.id === updatedProduct.id ? { ...product, ...updatedProduct } : product
            )
        );
    };

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

        try {
            const response = await axios.delete(`/api/Product/products/${deleteProductId}`, {
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
            <div className="header">
                <h1>Product Management</h1>
                <button
                    className="add-button"
                    onClick={() => navigate('/add-product')} // นำทางไปยัง AddProduct
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
                            {/* <th>Detail</th> */}
                            <th>Product ID</th>
                            <th>Product Name</th>
                            <th>Product Code</th>
                            <th>Price</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {products.map(({ id, productName, productCode, price }) => (
                            <tr key={id}>
                                <td>
                                    <a href={`/product/${id}`} className="detail-link">Detail</a>
                                </td>
                                <td>{id}</td>
                                <td>{productName}</td>
                                <td>{productCode}</td>
                                <td>{price}</td>
                                <td className="action-buttons">
                                    <button
                                        className="edit-button"
                                        onClick={() => {
                                            setEditProduct({ id, productName, productCode, price });
                                            navigate(`/edit-product/${id}`);
                                        }}
                                    >
                                        ✏️
                                    </button>
                                    <button
                                        className="delete-button"
                                        onClick={() => handleOpenDeleteModal(id)}
                                    >
                                        🗑️
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            <button
                className="back-button"
                onClick={() => navigate("/select-branch")}
            >
                Back
            </button>

            {isConfirmModalOpen && (
                <ConfirmationModal
                    isOpen={isConfirmModalOpen}
                    onClose={handleCloseDeleteModal}
                    onConfirm={handleConfirmDelete}
                    message="Are you sure you want to delete this product?"
                />
            )}

            {editProduct && (
                <EditProduct
                    productId={editProduct.id}
                    onEdit={handleEditProduct} // ส่งฟังก์ชันแก้ไขไปยัง EditProduct
                />
            )}
        </div>
    );
};

export default ProductList;