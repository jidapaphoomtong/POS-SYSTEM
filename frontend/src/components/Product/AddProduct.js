// import React, { useState } from "react";
// import axios from "axios";
// import "../../styles/product.css";
// import Cookies from "js-cookie";
// import { useNavigate } from "react-router-dom";

// const AddProduct = () => {
//     const [formData, setFormData] = useState({
//         productName: "",
//         productCode: "",
//         price: "",
//         quantity: "",
//     });
    
//     const [isLoading, setIsLoading] = useState(false);
//     const navigate = useNavigate();

//     const handleChange = (e) => {
//         setFormData({ ...formData, [e.target.name]: e.target.value });
//     };

//     const handleSubmit = async (e) => {
//         e.preventDefault();

//         // Validation
//         if (!formData.productName || !formData.productCode || !formData.price || !formData.quantity) {
//             alert("Please fill out all fields!");
//             return;
//         }

//         setIsLoading(true);

//         try {
//             const token = Cookies.get("authToken");
//             if (!token) {
//                 alert("No token found. Please log in again.");
//                 return;
//             }
        
//             const response = await axios.post(`http://localhost:5293/api/Product/add-product`, formData, {
//                 headers: {
//                     "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
//                     Authorization: `Bearer ${token}`,
//                     "Content-Type": "application/json",
//                 },
//                 withCredentials: true,
//             });
        
//             if (response.status === 200) {
//                 alert("Product added successfully!");
//                 navigate("/ProductList");
//             } else {
//                 alert(`Request failed with status: ${response.status}`);
//             }
//         } catch (error) {
//             console.error("Error adding product:", error);
//             if (error.response) {
//                 alert(error.response.data?.Message || "Failed to add product.");
//             } else {
//                 alert("Error: " + error.message);
//             }
//         } finally {
//             setIsLoading(false);
//         }
//     };

//     return (
//         <div className="add-product-container">
//             <h2>Add Product</h2>
//             <form onSubmit={handleSubmit}>
//                 <input
//                     type="text"
//                     name="productName"
//                     placeholder="Product Name"
//                     value={formData.productName}
//                     onChange={handleChange}
//                     required
//                 />
//                 <input
//                     type="text"
//                     name="productCode"
//                     placeholder="Product Code"
//                     value={formData.productCode}
//                     onChange={handleChange}
//                     required
//                 />
//                 <input
//                     type="number"
//                     name="price"
//                     placeholder="Price"
//                     value={formData.price}
//                     onChange={handleChange}
//                     required
//                 />
//                 <input
//                     type="number"
//                     name="quantity"
//                     placeholder="Quantity"
//                     value={formData.quantity}
//                     onChange={handleChange}
//                     required
//                 />
//                 <div className="form-buttons">
//                     <button type="button" onClick={() => navigate('/ProductList')} disabled={isLoading}>
//                         Cancel
//                     </button>
//                     <button type="submit" disabled={isLoading}>
//                         {isLoading ? "Saving..." : "Save"}
//                     </button>
//                 </div>
//             </form>
//         </div>
//     );
// };

// export default AddProduct;