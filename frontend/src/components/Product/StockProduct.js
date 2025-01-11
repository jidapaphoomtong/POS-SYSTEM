// import React, { useState } from 'react';
// import './StockPage.css'; // สไตล์สำหรับหน้า

// const StockPage = () => {
//     const [items, setItems] = useState([
//         { id: '0003', name: 'เสื่อน้ำมัน', price: 500, quantity: 400, category: 'หมวด 1', status: 'พร้อมขาย' },
//         { id: '0004', name: 'เสื่อน้ำมันป้องกันน้ำ', price: 600, quantity: 200, category: 'หมวด 2', status: 'ใกล้หมด' },
//         { id: '0005', name: 'กล่องกระดาษม้วน', price: 300, quantity: 200, category: 'หมวด 3', status: 'พร้อมขาย' },
//         // เพิ่มสินค้ารายการอื่น ๆ ได้ที่นี่
//     ]);
    
//     const [filter, setFilter] = useState({
//         status: 'ทั้งหมด',
//         category: 'ทั้งหมด',
//         location: '',
//         search: ''
//     });
    
//     const handleAddProduct = () => {
//         // ฟังก์ชันสำหรับเพิ่มสินค้า
//     };

//     return (
//         <div className="stock-page">
//             <h1>ระบบจัดการสต็อกสินค้า</h1>
//             <div className="stats">
//                 <span>จำนวนสินค้าที่พร้อมขาย: {items.filter(item => item.status === 'พร้อมขาย').length}</span>
//                 <span>จำนวนสินค้าที่มีส่วนลด: 0</span>
//                 <span>จำนวนสินค้าที่หมด: 0</span>
//                 <span>จำนวนสินค้าที่ใกล้หมด: 0</span>
//             </div>
//             <div className="filter-bar">
//                 <select onChange={(e) => setFilter({ ...filter, status: e.target.value })}>
//                     <option value="ทั้งหมด">สถานะ</option>
//                     <option value="พร้อมขาย">พร้อมขาย</option>
//                     <option value="ใกล้หมด">ใกล้หมด</option>
//                 </select>
//                 <select onChange={(e) => setFilter({ ...filter, category: e.target.value })}>
//                     <option value="ทั้งหมด">หมวดหมู่</option>
//                     <option value="หมวด 1">หมวด 1</option>
//                     <option value="หมวด 2">หมวด 2</option>
//                 </select>
//                 <input
//                     type="text"
//                     placeholder="ที่อยู่สินค้า"
//                     value={filter.location}
//                     onChange={(e) => setFilter({ ...filter, location: e.target.value })}
//                 />
//                 <input
//                     type="text"
//                     placeholder="ค้นหา"
//                     value={filter.search}
//                     onChange={(e) => setFilter({ ...filter, search: e.target.value })}
//                 />
//                 <button onClick={handleAddProduct}>เพิ่มสินค้า +</button>
//             </div>
//             <table>
//                 <thead>
//                     <tr>
//                         <th>รหัส</th>
//                         <th>ชื่อสินค้า</th>
//                         <th>ราคา</th>
//                         <th>จำนวน</th>
//                         <th>หมวดหมู่</th>
//                         <th>สถานะ</th>
//                     </tr>
//                 </thead>
//                 <tbody>
//                     {items.filter(item => 
//                         (filter.status === 'ทั้งหมด' || item.status === filter.status) &&
//                         (filter.category === 'ทั้งหมด' || item.category === filter.category) &&
//                         (filter.location === '' || item.location?.includes(filter.location)) &&
//                         (filter.search === '' || item.name.includes(filter.search))
//                     ).map(item => (
//                         <tr key={item.id}>
//                             <td>{item.id}</td>
//                             <td>{item.name}</td>
//                             <td>{item.price}</td>
//                             <td>{item.quantity} ชิ้น</td>
//                             <td>{item.category}</td>
//                             <td>{item.status}</td>
//                         </tr>
//                     ))}
//                 </tbody>
//             </table>
//             <div className="pagination">
//                 {/* นี่อาจจะเป็นการจัดการหน้าเพจเพิ่มเติม */}
//                 <button>ก่อนหน้า</button>
//                 <button>ถัดไป</button>
//             </div>
//         </div>
//     );
// };

// export default StockPage;