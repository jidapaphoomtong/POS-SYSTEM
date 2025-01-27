// import React, { useState, useEffect } from 'react';
// import Cookies from 'js-cookie';
// import { jwtDecode } from 'jwt-decode';
// import "../styles/Navbar.css"

// const UserDetail = () => {
//     const [userData, setUserData] = useState(null);
//     const [isModalOpen, setIsModalOpen] = useState(false); // State สำหรับการเปิด/ปิดโมเดล

//     useEffect(() => {
//         const token = Cookies.get('authToken'); // ดึง token จาก cookies

//         if (token) {
//             try {
//                 const decodedToken = jwtDecode(token); // decode token เพื่อดึงข้อมูล
//                 setUserData(decodedToken); // เก็บข้อมูลผู้ใช้ใน state
//             } catch (error) {
//                 console.error('Invalid token:', error);
//                 setUserData(null); // ถ้าเกิดข้อผิดพลาด ให้รีเซ็ตข้อมูลผู้ใช้
//             }
//         }
//     }, []); // ทำการเรียกเมื่อมีการเรนเดอร์เป็นครั้งแรก

//     const handleUserInfoClick = () => {
//         setIsModalOpen(true); // เปิดโมเดล
//     };

//     const closeModal = () => {
//         setIsModalOpen(false); // ปิดโมเดล
//     };

//     return (
//         <div>
//             <div className="dropdown">
//                 <ul>
//                     <li onClick={handleUserInfoClick}>User Info</li>
//                 </ul>
//             </div>

//             {isModalOpen && (
//                 <div className="modal">
//                     <div className="modal-content">
//                         <span className="close" onClick={closeModal}>&times;</span>
//                         <h2>User Detail</h2>
//                         {userData ? (
//                             <div>
//                                 <p><strong>First Name:</strong> {userData.firstName || 'N/A'}</p>
//                                 <p><strong>Email:</strong> {userData['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || 'N/A'}</p>
//                                 <p><strong>Role:</strong> {userData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'N/A'}</p>
//                             </div>
//                         ) : (
//                             <p>No user data found. Please log in.</p>
//                         )}
//                     </div>
//                 </div>
//             )}
//         </div>
//     );
// }

// export default UserDetail;