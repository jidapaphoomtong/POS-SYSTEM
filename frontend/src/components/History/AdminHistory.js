// import React, { useState } from 'react';
// import './HistoryPage.css'; // สไตล์สำหรับหน้า

// const HistoryPage = () => {
//     const [salesHistory, setSalesHistory] = useState([
//         { billNo: '000123', date: '10-12-2024', total: 70, employee: 'Ruby', payType: 'Cash' },
//         { billNo: '000124', date: '11-12-2024', total: 80, employee: 'Ruby', payType: 'Qr code' },
//         { billNo: '000125', date: '11-12-2024', total: 36, employee: 'Ruby', payType: 'Cash' },
//         { billNo: '000126', date: '12-12-2024', total: 40, employee: 'Ruby', payType: 'Qr code' },
//         { billNo: '000127', date: '12-12-2024', total: 37, employee: 'Ruby', payType: 'Cash' },
//         { billNo: '000128', date: '12-12-2024', total: 60, employee: 'Ruby', payType: 'Qr code' },
//         // เพิ่มข้อมูลการขายอื่น ๆ ได้ที่นี่
//     ]);
    
//     const [filter, setFilter] = useState({
//         month: 'All',
//         search: ''
//     });

//     const handleDelete = (billNo) => {
//         setSalesHistory(salesHistory.filter(item => item.billNo !== billNo));
//     };

//     return (
//         <div className="history-page">
//             <h1>ประวัติการขาย</h1>
//             <div className="filter-bar">
//                 <input
//                     type="text"
//                     placeholder="ค้นหา Bill No."
//                     value={filter.search}
//                     onChange={(e) => setFilter({ ...filter, search: e.target.value })}
//                 />
//                 <select onChange={(e) => setFilter({ ...filter, month: e.target.value })}>
//                     <option value="All">ทั้งหมด</option>
//                     <option value="Month">เดือน</option>
//                 </select>
//             </div>
//             <table>
//                 <thead>
//                     <tr>
//                         <th>Bill No.</th>
//                         <th>Date</th>
//                         <th>Total</th>
//                         <th>Employee</th>
//                         <th>Pay Type</th>
//                         <th>Actions</th>
//                     </tr>
//                 </thead>
//                 <tbody>
//                     {salesHistory.filter(item => 
//                         item.billNo.includes(filter.search)
//                     ).map(item => (
//                         <tr key={item.billNo}>
//                             <td>{item.billNo}</td>
//                             <td>{item.date}</td>
//                             <td>${item.total}</td>
//                             <td>{item.employee}</td>
//                             <td>{item.payType}</td>
//                             <td>
//                                 <button onClick={() => handleDelete(item.billNo)}>🗑️ ลบ</button>
//                                 <button>📄 พิมพ์</button>
//                             </td>
//                         </tr>
//                     ))}
//                 </tbody>
//             </table>
//             <div className="pagination">
//                 {/* แบ่งหน้า */}
//                 <button>1</button>
//                 <button>2</button>
//                 <button>3</button>
//                 <button>4</button>
//                 <button>5</button>
//                 <button>6</button>
//                 <button>ถัดไป</button>
//             </div>
//         </div>
//     );
// };

// export default HistoryPage;