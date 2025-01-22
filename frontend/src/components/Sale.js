import React, { useState, useEffect } from "react";
import { useNavigate, useParams } from 'react-router-dom';
import NavBar from "../components/bar/Navbar";
import SideBar from "../components/bar/Sidebar";
import "../styles/Sale.css";
import axios from 'axios';
import Cookies from 'js-cookie';
import { FcPlus } from "react-icons/fc";
import { AiFillMinusCircle } from "react-icons/ai";
import { FaTrash } from "react-icons/fa";
import { jwtDecode } from "jwt-decode";
import { toast } from "react-toastify";

export default function Sale() {
    const [categories, setCategories] = useState([]);
    const [items, setItems] = useState([]);
    const [selectedItems, setSelectedItems] = useState({});
    const [searchTerm, setSearchTerm] = useState("");
    const [paymentMethod, setPaymentMethod] = useState(); // ตัวแปรที่ใช้เก็บวิธีการชำระเงิน
    const [loading, setLoading] = useState(false);
    const [showOrderSummary, setShowOrderSummary] = useState(false);
    const [filterItems, setFilterItems] = useState([]);
    const navigate = useNavigate();
    const [selectedCategory, setSelectedCategory] = useState("all");
    const [showPaymentModal, setShowPaymentModal] = useState(false);
    const [paidAmount, setPaidAmount] = useState(0); // จำนวนที่จ่าย
    const [change, setChange] = useState(0); // เงินทอน
    const [errorMessage, setErrorMessage] = useState("");
    const { branchId } = useParams(); // สำหรับการดึงค่า branchId
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 10; // จำนวนสินค้าที่จะแสดงต่อหน้า

    useEffect(() => {
        const fetchData = async () => {
            const token = Cookies.get("authToken");
            
            if (!branchId) {
                toast.error("Branch ID is missing!");
                return; // ถ้าไม่มี Branch ID จะแสดงข้อความเตือนและหยุดการทำงาน
            }
    
            setLoading(true); // ตั้งค่าสถานะ loading เริ่มต้น
    
            try {
                // Fetch Products
                const productResponse = await axios.get(`/api/Product/branches/${branchId}/products`, {
                    headers: { 
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}` 
                    },
                    withCredentials: true,
                });
    
                if (productResponse.data.success) {
                    const products = productResponse.data.data.map(item => ({
                        Id: item.data.Id,
                        ImgUrl: item.data.ImgUrl,
                        productName: item.data.productName,
                        description: item.data.description,
                        stock: item.data.stock,
                        price: item.data.price,
                        categoryId: item.data.categoryId,
                        branchId: item.data.branchId,
                    }));
                    setItems(products); // ตั้งค่าข้อมูลสินค้าทั้งหมด
                    setFilterItems(products); // ตั้งค่าเริ่มต้นให้แสดงสินค้าทั้งหมด
                } else {
                    alert(productResponse.data.message); // แสดงข้อความเมื่อไม่สามารถนำเข้าข้อมูลได้
                }
    
                // Fetch Categories
                const categoryResponse = await axios.get(`/api/Category/branches/${branchId}/getCategories`, {
                    headers: { 
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}` 
                    },
                    withCredentials: true,  
                });
                
                if (categoryResponse.data.success) {
                    // รวมหมวดหมู่ "All" เข้าไปในรายการหมวดหมู่
                    setCategories(categoryResponse.data.data);
                }
                
            } catch (error) {
                console.error("Failed to fetch data:", error); // แสดงข้อผิดพลาดใน Console
                toast.error("Failed to fetch data!"); // แสดงข้อความเตือนเมื่อมีข้อผิดพลาดเกิดขึ้น
            } finally {
                setLoading(false); // สถานะ loading จะถูกปิดเมื่อเสร็จสิ้น
            }
        };
    
        fetchData();
    }, [navigate]);

    const handleCategorySelect = (categoryId) => {
        setSelectedCategory(categoryId); // ตั้งค่า selectedCategory ตามที่ได้รับ
        if (categoryId === "all") {
            setFilterItems(items); // ถ้าเลือก "All" ให้แสดงสินค้าทั้งหมด
        } else {
            const filtered = items.filter(item => item.categoryId === categoryId); // กรองเฉพาะสินค้าที่มี categoryId ตรง
            setFilterItems(filtered); // แสดงสินค้าที่กรองแล้ว
        }
    };

    const handleSelectItem = (item) => {
        setSelectedItems((prevItems) => ({
            ...prevItems,
            [item.Id]: {
                ...item,
                quantity: (prevItems[item.Id] ? prevItems[item.Id].quantity : 0) + 1,
            }
        }));
        
        setShowOrderSummary(true);
    };

    const handleIncreaseQuantity = (itemId) => {
        setSelectedItems((prevItems) => ({
            ...prevItems,
            [itemId]: {
                ...prevItems[itemId],
                quantity: prevItems[itemId].quantity + 1,
            }
        }));
    };

    const handleDecreaseQuantity = (itemId) => {
        setSelectedItems((prevItems) => {
            const newItems = { ...prevItems };
            if (newItems[itemId].quantity > 1) {
                newItems[itemId].quantity -= 1;
            } else {
                delete newItems[itemId];
            }
            return newItems;
        });
    };

    const handleRemoveItem = (id) => {
        setSelectedItems(prevItems => {
            const newItems = { ...prevItems };
            delete newItems[id];

            if (Object.keys(newItems).length === 0) {
                setShowOrderSummary(false);
            }
            
            return newItems;
        });
    };

    const handleSearchChange = (e) => {
        setSearchTerm(e.target.value);
    };

    const calculateTotal = () => {
        return Object.values(selectedItems).reduce((total, item) => total + item.price * item.quantity, 0);
    };

    const handlePlaceOrder = () => {
        if (Object.keys(selectedItems).length === 0) {
            toast.error("Please select at least one item before placing an order.");
            return;
        }
        // ส่ง selectedItems ไปยังหน้า Order ด้วย navigate
        navigate({ state: { selectedItems, total: calculateTotal() } }); // เพิ่ม total
    };

    const handleOpenPaymentModal = () => {
        setShowPaymentModal(true);
    };

    const handleClosePaymentModal = () => {
        setShowPaymentModal(false);

         // รีเซ็ตทั้งหมด
        setPaidAmount(0); // จำนวนที่จ่าย
        setChange(0); // เงินทอน
    };

    const handlePayment = async (type) => {
        const total = calculateTotal();
    
        // ตรวจสอบจำนวนที่จ่ายและเงินทอน
        if (paidAmount < total) {
            setErrorMessage("คิดเงินผิด! กรุณาจ่ายเงินให้ครบถ้วน.");
            return; // ถ้าเงินไม่ครบไม่ให้ proceed
        }

        setPaymentMethod(type); // บันทึกประเภทการชำระเงิน
        toast.success(`ชำระเงินสำเร็จด้วย ${type}`);

        // Generate receipt
        await generateReceipt(type);

        // บันทึกคำสั่งซื้อ
        await saveOrder(type); // <-- เรียกใช้ฟังก์ชันสำหรับบันทึกคำสั่งซื้อ

        handleClosePaymentModal(); // ปิด modal หลังจากชำระเงินสำเร็จ
    };

    const handleAmountChange = (amount) => {
        setPaidAmount(prev => prev + amount);
        setChange(paidAmount + amount - calculateTotal());
    };

    const generateReceipt = async (type) => {
        const token = Cookies.get("authToken");
        // console.log(token)
        let firstName = "";
            
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                const firstNameFromToken = decodedToken["firstName"] || 'No role found';
                firstName = firstNameFromToken; // Adjust according to your JWT structure
                // console.log(firstName);
        
            }catch (error) {
                console.error("Invalid token:", error);
                alert("Have Something wrong")
            }
        }

        const receipt = {
            items: Object.values(selectedItems),
            total: calculateTotal(),
            paidAmount: paidAmount,
            change: change,
            date: new Date().toLocaleString(),
            seller: firstName,
            paymentMethod: type, // เพิ่มประเภทการชำระเงินในใบเสร็จ
        };

        // Call print function
        printReceipt(receipt);
    };

    const printReceipt = (receipt) => {
        const receiptWindow = window.open('', '', 'width=600,height=400');
        receiptWindow.document.write('<html><head><title>ใบเสร็จ</title></head><body>');
        receiptWindow.document.write('<h2>ใบเสร็จ</h2>');
        receiptWindow.document.write(`<p>วันที่: ${receipt.date}</p>`);
        receiptWindow.document.write('<h3>รายการสินค้า:</h3>');
        receiptWindow.document.write('<ul>');
    
        receipt.items.forEach(item => {
            receiptWindow.document.write(`<li>${item.productName} : ฿${item.price} x ${item.quantity}</li>`);
        });
        
        receiptWindow.document.write('</ul>');
        receiptWindow.document.write('-------------------------<br>');
        receiptWindow.document.write(`<p><strong>ยอดรวม: ฿${receipt.total}</strong></p>`);
        receiptWindow.document.write(`<p>จำนวนเงินที่จ่าย: ฿${receipt.paidAmount}</p>`);
        receiptWindow.document.write(`<p>เงินทอน: ฿${receipt.change}</p>`);
        receiptWindow.document.write(`<p>ประเภทการจ่ายเงิน: ${receipt.paymentMethod}</p>`); // แก้ไขจาก receipt.type เป็น receipt.paymentMethod
        receiptWindow.document.write(`<p>ผู้ขาย: ${receipt.seller}</p>`);
        receiptWindow.document.write('</body></html>');
        
        receiptWindow.document.close();
        receiptWindow.focus();
        receiptWindow.print();
        receiptWindow.close();
    };

    const saveOrder = async (type) => {
        const token = Cookies.get("authToken");
    
        // เช็คว่า branchId มีค่าหรือไม่
        if (!branchId) {
            toast.error("Branch ID is missing!");
            return;
        }
    
        // ตรวจสอบการ Decode Token
        let firstName = "";
        if (token) {
            try {
                const decodedToken = jwtDecode(token);
                const firstNameFromToken = decodedToken["firstName"] || 'No role found';
                firstName = firstNameFromToken;
            } catch (error) {
                console.error("Invalid token:", error);
                toast.error("Have Something wrong");
            }
        }
    
        // สร้าง Purchase Object
        const purchase = {
            products: Object.values(selectedItems).map(item => ({
                id: item.Id,
                productName: item.productName,
                price: item.price,
                stock: item.quantity,
                categoryId: item.categoryId,
            })),
            total: calculateTotal(),
            paidAmount: paidAmount,
            change: change,
            date: new Date().toISOString(),
            seller: firstName,
            paymentMethod: type,
        };
    
        // Debugging: แสดง Purchase Object ใน Console
        // console.log("Purchase Object:", JSON.stringify(purchase, null, 2));
    
        try {
            const response = await axios.post(`/api/Purchase/add-purchase/${branchId}`, purchase, {
                headers: { 
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}` 
                },
                withCredentials: true,
            });
    
            if (response.status === 200) {
                // อัปเดต stock สินค้า
                for (const item of purchase.products) {
                    // ดึง productId ของ item
                    const productId = item.id; // ใช้ id จาก Purchase object
    
                    await axios.put(`/api/Product/update-stock/${branchId}/${productId}`, {
                        quantity: item.stock // ปรับปรุงจำนวน stock ที่ใช้งาน
                    }, {
                        headers: { 
                            "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                            Authorization: `Bearer ${token}` 
                        },
                        withCredentials: true,
                    });
                }
    
                toast.success("Purchases added successfully!");
            } else {
                toast.error(`Request failed with status: ${response.status}`);
            }
    
        } catch (error) {
            console.error("Error during purchase:", error);
            toast.error("Failed to save the purchase: " + error.message);
        }
    };

    // ฟังก์ชันสำหรับการจัดการ pagination
    const handlePageChange = (pageNumber) => {
        setCurrentPage(pageNumber);
    };

    // คำนวณสินค้าที่จะแสดง
    const indexOfLastProduct = currentPage * itemsPerPage;
    const indexOfFirstProduct = indexOfLastProduct - itemsPerPage;
    const currentProducts = items.slice(indexOfFirstProduct, indexOfLastProduct);

    const totalPages = Math.ceil(items.length / itemsPerPage);

    return (
        <div className="sale-page">
            <NavBar />
            <div className="content">
                <SideBar />
                <div className="main-content">
                    {loading && <div>Loading...</div>} 
                    
                    <div className="search-container">
                        <input
                            className="search-bar"
                            type="text"
                            placeholder="Search for items..."
                            value={searchTerm}
                            onChange={handleSearchChange}
                        />
                    </div>

                    <div className="categories">
                    <button 
                        className="category-btn"
                        onClick={() => handleCategorySelect("all")} // เรียกใช้ฟังก์ชันเมื่อคลิก
                    >
                        All
                    </button>
                        {categories.map((category) => (
                            <button 
                                key={category.id}
                                className="category-btn"
                                onClick={() => handleCategorySelect(category.id)}
                            >
                                {category.name}
                            </button>
                        ))}
                    </div>

                    <div className="main-content-order">
                        <div className="items-grid">
                            {filterItems.filter(item => 
                                item.productName.toLowerCase().includes(searchTerm.toLowerCase())
                            ).map((item) => (
                                <div key={item.Id} className="item-card" onClick={() => handleSelectItem(item)}>
                                    <img src={item.ImgUrl} alt={item.productName} />
                                    <p>{item.productName}</p>
                                    <p>{item.price} บาท</p>
                                </div>
                            ))}
                        </div>
                        {/* ส่วนของรายการสินค้าที่เลือก */}
                        {showOrderSummary && ( 
                            <div className="order-summary">
                                <h2>ORDER SUMMARY</h2>
                                {Object.values(selectedItems).map(item => (
                                    <div key={item.Id} className="order-item">
                                        <div className="row">
                                            <p>{item.productName} : {item.price} บาท</p>
                                            <button className="icon-button" onClick={() => handleRemoveItem(item.Id)}>
                                                <FaTrash className="icon-red" />
                                            </button>
                                        </div>
                                        <div className="row">
                                            <button className="icon-button" onClick={() => handleDecreaseQuantity(item.Id)}>
                                                <AiFillMinusCircle className="icon icon-blue" />
                                            </button>
                                            <p> x {item.quantity}</p>
                                            <button className="icon-button" onClick={() => handleIncreaseQuantity(item.Id)}>
                                                <FcPlus className="icon" />
                                            </button>
                                        </div>
                                    </div>
                                ))}
                                <p>Total: {calculateTotal()} บาท</p>
                                <button onClick={handleOpenPaymentModal}>ชำระเงิน</button>
                            </div>
                        )}

                        {/* Modal สำหรับการชำระเงิน */}
                        {showPaymentModal && (
                            <div className="payment-modal">
                                <div className="modal-content">
                                    <h3>ตรวจสอบการสั่งซื้อ</h3>
                                    {Object.values(selectedItems).map(item => (
                                        <div key={item.Id} className="payment-item">
                                            <p>{item.productName} : ฿{item.price} x {item.quantity}</p>
                                        </div>
                                    ))}
                                    <hr />
                                    <p>ยอดรวม: ฿{calculateTotal()}</p>
                                    
                                    <p>เงินที่จ่าย: ฿{paidAmount}</p>
                                    <p>เงินทอน: ฿{change >= 0 ? change : 0}</p>
                                    
                                    {/* แสดงข้อความผิดพลาดถ้ามี */}
                                    {errorMessage && <p style={{ color: "red" }}>{errorMessage}</p>}

                                    {/* ปุ่มสำหรับเพิ่มจำนวนเงิน */}
                                    <div className="number-pad">
                                        {[1000, 500, 100, 50, 20, 10].map((amount) => (
                                            <button key={amount} onClick={() => handleAmountChange(amount)}>
                                                ฿{amount}
                                            </button>
                                        ))}
                                    </div>

                                    {/* ตัวเลือกการชำระเงิน */}
                                    <div className="payment-methods">
                                        <button className="payment-button" onClick={() => handlePayment('cash')}>เงินสด</button>
                                        <button className="payment-button" onClick={() => handlePayment('mobile')}>Mobile Banking</button>
                                    </div>

                                    <button className="cancel-button" onClick={handleClosePaymentModal}>ยกเลิก</button>
                                </div>
                            </div>
                        )}
                    </div>
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
                </div>
            </div>
        </div>
    );
}