// import React, { useState } from 'react';
// import '../styles/order.css';

// const Order = ({ selectedItems, total, onPlaceOrder }) => {
//     const [showModal, setShowModal] = useState(false);
//     const [paymentType, setPaymentType] = useState(null);

//     const handlePaymentTypeSelect = (type) => {
//         setPaymentType(type);
//         setShowModal(false); // Close the modal
//     };

//     return (
//         <div className="order-page">
//             <h2>Order Summary</h2>
//             {Object.values(selectedItems).length ? (
//                 Object.values(selectedItems).map(item => (
//                     <div key={item.Id} className="order-item">
//                         <p>{item.productName} : ฿{item.price} x {item.quantity}</p>
//                     </div>
//                 ))
//             ) : (
//                 <p>No items selected</p>
//             )}
//             <hr />
//             <p>Total: ฿{total}</p>
//             <button onClick={() => setShowModal(true)}>Pay Now</button>

//             {showModal && (
//                 <div className="modal">
//                     <div className="modal-content">
//                         <h3>Choose Payment Method</h3>
//                         <button onClick={() => handlePaymentTypeSelect('cash')}>Cash</button>
//                         <button onClick={() => handlePaymentTypeSelect('mobile')}>Mobile Banking</button>
//                         <button onClick={() => setShowModal(false)}>Cancel</button>
//                     </div>
//                 </div>
//             )}

//             {paymentType === 'cash' && (
//                 <div className="payment-method">
//                     <h3>Cash Payment</h3>
//                     {/* Add your cash payment UI here */}
//                     {/* For example: show the keypad */}
//                 </div>
//             )}

//             {paymentType === 'mobile' && (
//                 <div className="payment-method">
//                     <h3>Mobile Banking Payment</h3>
//                     {/* Add your mobile banking UI here */}
//                     {/* For example: show the bank options */}
//                 </div>
//             )}
//         </div>
//     );
// };

// export default Order;