// import React, { useEffect, useState } from 'react';
// import axios from 'axios';
// import Cookies from 'js-cookie';
// import './StockPage.css';

// const StockPage = () => {
//     const [items, setItems] = useState([]);
//     const [isLoading, setIsLoading] = useState(true);
//     const [editingItem, setEditingItem] = useState(null); // สำหรับเก็บข้อมูลสินค้าเมื่อแก้ไข
//     const [quantityToAdd, setQuantityToAdd] = useState(0);
//     const [message, setMessage] = useState('');

//     useEffect(() => {
//         const fetchItems = async () => {
//             const token = Cookies.get("authToken");
//             try {
//                 const response = await axios.get('http://localhost:5293/api/Stock/items', {
//                     headers: { Authorization: `Bearer ${token}` },
//                 });
//                 setItems(response.data);
//             } catch (error) {
//                 console.error("Failed to fetch items:", error);
//             } finally {
//                 setIsLoading(false);
//             }
//         };
//         fetchItems();
//     }, []);

//     const handleAddStock = async (productId) => {
//         const token = Cookies.get("authToken");
//         try {
//             const response = await axios.post(
//                 `http://localhost:5293/api/product/branchId/products/${productId}/addstock`,
//                 { quantity: quantityToAdd },
//                 { headers: { Authorization: `Bearer ${token}` } }
//             );
//             setMessage(response.data.message);
//             setQuantityToAdd(0); // Clear input after adding stock
//             fetchItems(); // Refresh the list after adding
//         } catch (error) {
//             console.error("Error adding stock:", error);
//             setMessage("Failed to add stock.");
//         }
//     };

//     const handleDeleteProduct = async (productId) => {
//         const token = Cookies.get("authToken");
//         try {
//             await axios.delete(`http://localhost:5293/api/product/branchId/products/${productId}`, {
//                 headers: { Authorization: `Bearer ${token}` },
//             });
//             setMessage("Product deleted successfully");
//             fetchItems(); // Refresh the list after deletion
//         } catch (error) {
//             console.error("Failed to delete product:", error);
//             setMessage("Failed to delete product.");
//         }
//     };

//     const handleEditProduct = async () => {
//         const token = Cookies.get("authToken");
//         try {
//             await axios.put(`http://localhost:5293/api/product/branchId/products/${editingItem.id}`, editingItem, {
//                 headers: { Authorization: `Bearer ${token}` },
//             });
//             setMessage("Product updated successfully");
//             setEditingItem(null); // Clear editing state
//             fetchItems(); // Refresh the list after editing
//         } catch (error) {
//             console.error("Failed to update product:", error);
//             setMessage("Failed to update product.");
//         }
//     };

//     if (isLoading) {
//         return <p>Loading...</p>;
//     }

//     return (
//         <div className="stock-page">
//             <h1>ระบบจัดการสต็อกสินค้า</h1>
//             {message && <p>{message}</p>}
//             <table>
//                 <thead>
//                     <tr>
//                         <th>ID</th>
//                         <th>ชื่อสินค้า</th>
//                         <th>ราคา</th>
//                         <th>จำนวน</th>
//                         <th>หมวดหมู่</th>
//                         <th>สถานะ</th>
//                         <th>จัดการ</th>
//                     </tr>
//                 </thead>
//                 <tbody>
//                     {items.map(item => (
//                         <tr key={item.id}>
//                             <td>{item.id}</td>
//                             <td>{item.productName}</td>
//                             <td>{item.price}</td>
//                             <td>{item.stock}</td>
//                             <td>{item.category}</td>
//                             <td>{item.stock > 0 ? 'พร้อมขาย' : 'หมด'}</td>
//                             <td>
//                                 <button onClick={() => setEditingItem(item)}>แก้ไข</button>
//                                 <button onClick={() => handleDeleteProduct(item.id)}>ลบ</button>
//                                 <button onClick={() => handleAddStock(item.id)}>เพิ่มสต็อก</button>
//                             </td>
//                         </tr>
//                     ))}
//                 </tbody>
//             </table>

//             {editingItem && (
//                 <div className="edit-product">
//                     <h2>แก้ไขสินค้า: {editingItem.productName}</h2>
//                     <input 
//                         type="number" 
//                         value={editingItem.stock} 
//                         onChange={e => setEditingItem({ ...editingItem, stock: Number(e.target.value) })} 
//                     />
//                     <button onClick={handleEditProduct}>บันทึกการเปลี่ยนแปลง</button>
//                     <button onClick={() => setEditingItem(null)}>ยกเลิก</button>
//                 </div>
//             )}
//         </div>
//     );
// };

// export default StockPage;