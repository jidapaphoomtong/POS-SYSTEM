import axios from "axios";
import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import "../../styles/product.css";
import Cookies from "js-cookie";
import ConfirmationModal from "./ConfirmationModal";
import Navbar from "../bar/Navbar";
import Sidebar from "../bar/Sidebar";
import { FaEdit, FaTrash } from "react-icons/fa";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";

const ProductList = () => {
    const [products, setProducts] = useState([]);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 6; // จำนวนสินค้าที่จะแสดงต่อหน้า
    const navigate = useNavigate();
    const [isLoading, setIsLoading] = useState(false);
    const [deleteProductId, setDeleteProductId] = useState(null);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
    const [userRole, setUserRole] = useState(""); // state สำหรับบทบาทผู้ใช้
    const { branchId } = useParams();
    
    // ดึง branchId และ categoryId
    // const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");
    const categoryId = new URLSearchParams(window.location.search).get("category") || Cookies.get("categoryId");

    const getProductStatus = (stock) => {
        const UNLIMITED_STOCK = Number.MAX_SAFE_INTEGER; // ใช้ค่านี้เป็นตัวแทนสำหรับ stock ที่ไม่มีวันหมด

    if (stock === UNLIMITED_STOCK) {
        return { text: "Ready to sell", color: "green", icon: "✅" };
    }
        if (stock <= 0) {
            return { text: "Out of stock", color: "red", icon: "❌" };
        }
        if (stock < 50) {
            return { text: "Low stock", color: "orange", icon: "⚠️" };
        }
        return { text: "Ready to sell", color: "green", icon: "✅" };
    };

    useEffect(() => {
        const fetchProducts = async () => {
            const token = Cookies.get("authToken");
            if (!token) {
                toast.error("Your session has expired. Please login again.");
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
                    toast.error(response.data.message || "Failed to fetch products.");
                }
            } catch (error) {
                console.error("Failed to fetch products:", error);
                if (error.response?.status === 401) {
                    toast.error("Unauthorized. Please login.");
                    navigate("/");
                } else {
                    toast.error("Failed to fetch products.");
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
            toast.error("Your session has expired. Please login again.");
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
                toast.success("Product deleted successfully!");
                setProducts(products.filter((product) => product.id !== deleteProductId));
                handleCloseDeleteModal();
            } else {
                toast.error("Failed to delete product.");
            }
        } catch (error) {
            console.error("Failed to delete product:", error);
            toast.error("Failed to delete product: " + (error.response?.data?.message || "Unknown error"));
        }
    };

    useEffect(() => {
        const fetchUserRole = async () => {
            const token = Cookies.get("authToken");
            if (token) {
                // สมมุติว่าเราใส่ข้อมูล role ใน token หรือ API อื่น
                const decodedToken = jwtDecode(token); // ใช้ jwt-decode หรือวิธีการที่จะเข้าถึงข้อมูลนี้
                const roleFromToken = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || 'No role found';
                setUserRole(roleFromToken);
            }
            console.log(setUserRole);
        };

        fetchUserRole();
    }, []);

    // ฟังก์ชันที่ใช้ในการตรวจสอบบทบาท
    const canEditOrDelete = userRole !== "employee";

    // ฟังก์ชันสำหรับการจัดการ pagination
    const handlePageChange = (pageNumber) => {
        setCurrentPage(pageNumber);
    };

    // คำนวณสินค้าที่จะแสดง
    const indexOfLastProduct = currentPage * itemsPerPage;
    const indexOfFirstProduct = indexOfLastProduct - itemsPerPage;
    const currentProducts = products.slice(indexOfFirstProduct, indexOfLastProduct);

    const totalPages = Math.ceil(products.length / itemsPerPage);

    return (
        <div className="product-container">
            <Navbar />
            <div className="content">
                <Sidebar />
                <div className="main-content">
                    <div className="header">
                        <h2>Product Management ({products.length})</h2>
                        {canEditOrDelete && (
                            <button
                                className="add-button"
                                onClick={() => navigate(`/${branchId}/add-product`)}
                                disabled={isLoading}
                            >
                                Add Product
                            </button>
                        )}
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
                                    {canEditOrDelete && <th>Actions</th>}
                                </tr>
                            </thead>
                            <tbody>
                                {products.map(({ id, ImgUrl, productName, price, stock }) => {
                                    const status = getProductStatus(stock);
                                    return (
                                        <tr key={id}>
                                            <td >
                                                <a href={`/${branchId}/product/${id}`} className="detail-link">{id}</a>
                                            </td>
                                            <td><img src={ImgUrl} alt={productName} style={{ width: "50px", height: "50px" }} /></td>
                                            <td>{productName}</td>
                                            <td>{price}</td>
                                            <td>{stock}</td>
                                            <td style={{ color: status.color }}>{status.icon} {status.text}</td>
                                            {canEditOrDelete && (  
                                                <td>
                                                    <div className="row-product">
                                                        <button className="icon-button" onClick={() => navigate(`/${branchId}/edit-product/${id}`)}>
                                                            <FaEdit className="icon icon-blue" />
                                                        </button>
                                                        <button className="icon-button" onClick={() => handleOpenDeleteModal(id)}>
                                                            <FaTrash className="icon-red" />
                                                        </button>
                                                    </div>
                                                </td>
                                            )}
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    )}

                    <div className="pagination">
                        {Array.from({ length: totalPages }, (_, index) => (
                            <button
                                key={index + 1}
                                onClick={() => handlePageChange(index + 1)}
                                className={currentPage === index + 1 ? 'active' : ''}
                            >
                                {index + 1}
                            </button>
                        ))}
                    </div>

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