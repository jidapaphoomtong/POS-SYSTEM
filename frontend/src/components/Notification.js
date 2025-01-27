// // Notifications.js

// import React, { useEffect, useState } from 'react';
// import axios from 'axios';
// import { toast } from 'react-toastify';
// import 'react-toastify/dist/ReactToastify.css';

// const Notifications = () => {
//     const [notifications, setNotifications] = useState([]);
//     const [unreadCount, setUnreadCount] = useState(0);
    
//     const fetchNotifications = async () => {
//         try {
//             const response = await axios.get('http://localhost:3000/api/notifications');
//             setNotifications(response.data);
//             // นับจำนวน notifications ที่ยังไม่ได้อ่าน
//             setUnreadCount(response.data.filter(n => !n.read).length);
//         } catch (error) {
//             console.error('Error fetching notifications:', error);
//             toast.error('Failed to fetch notifications');
//         }
//     };

//     useEffect(() => {
//         // เรียก fetchNotifications ทุก 5 วินาที
//         const interval = setInterval(fetchNotifications, 5000);
//         fetchNotifications(); // เรียกครั้งแรกเมื่อ component mount
//         return () => clearInterval(interval); // เคลียร์ interval
//     }, []);

//     const markAllAsRead = async () => {
//         await axios.put('http://localhost:3000/api/notifications/read');
//         fetchNotifications(); // อัปเดตการดึงข้อมูล
//     };

//     return (
//         <div>
//             <h2>Notifications</h2>
//             {unreadCount > 0 && <span>You have {unreadCount} unread notifications.</span>}
//             <ul>
//                 {notifications.map((notification, index) => (
//                     <li key={index}>
//                         {notification.message} {notification.read ? '(Read)' : '(Unread)'}
//                     </li>
//                 ))}
//             </ul>
//             <button onClick={markAllAsRead}>Mark all as read</button>
//         </div>
//     );
// };

// export default Notifications;