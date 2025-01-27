import { useEffect, useState } from 'react';
import axios from 'axios';
import { useParams } from 'react-router-dom';
import Cookies from 'js-cookie';
import Navbar from '../bar/Navbar';
import Sidebar from '../bar/Sidebar';
import SalesChart from './SalesChart';
import "../../styles/dashboard.css";
import { IoBarChart } from "react-icons/io5";
import { FaChartLine } from "react-icons/fa6";
import { toast } from "react-toastify";
import DatePicker from 'react-datepicker';
import "react-datepicker/dist/react-datepicker.css";

const Dashboard = () => {
    const [summaryData, setSummaryData] = useState(null);
    const [hourlySalesData, setHourlySalesData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [viewType, setViewType] = useState('line'); // 'line' or 'bar'
    const [selectedDate, setSelectedDate] = useState(new Date()); // สำหรับปฏิทิน
    const [employeeId, setEmployeeId] = useState('');
    const { branchId } = useParams();
    const token = Cookies.get("authToken");

    const fetchSalesData = async () => {
        try {
            const selectedYear = selectedDate.getFullYear();
            const selectedMonth = selectedDate.getMonth() + 1;
            const selectedDateValue = selectedDate.getDate();

            // เปลี่ยน selectedYear เป็นปี พ.ศ.
            const yearInBC = selectedYear + 543; // แปลงเป็นปีพ.ศ.
    
            const summaryResponse = await axios.get(`/api/Purchase/sales-summary/${branchId}?year=${selectedYear}&month=${selectedMonth}&day=${selectedDateValue}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });
    
            // คัดกรองข้อมูลตามวันที่และชื่อพนักงาน
                const filteredSummaryData = Array.isArray(summaryResponse.data) 
                ? summaryResponse.data.filter(sale => {
                    const saleDate = new Date(sale.date);
                    const employeeIdLower = employeeId.toLowerCase(); // แปลงเป็นตัวพิมพ์เล็ก
                    
                    return saleDate.getFullYear() === yearInBC &&
                        saleDate.getMonth() + 1 === selectedMonth &&
                        saleDate.getDate() === selectedDateValue &&
                        (!employeeId || sale.seller.toLowerCase().startsWith(employeeIdLower)); // เช็คชื่อพนักงาน
                })
                : [];

            // คำนวณยอดขายรวมและจำนวนบิลขาย
            const totalSales = filteredSummaryData.reduce((acc, cur) => acc + cur.amount, 0);
            const totalTransactions = filteredSummaryData.length;

            setSummaryData({
                totalSales,
                totalTransactions,
                dailySales: filteredSummaryData,
            });
    
            // ดึงข้อมูลยอดขายทั้งหมดสำหรับกราฟหรือตาราง
            const response = await axios.get(`/api/Purchase/all-purchases/${branchId}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });
    
            if (response.data) {
                // นี่คือโค้ดการกรองข้อมูลตามวันที่
                const filteredData = response.data.filter(pur => {
                    const saleDate = new Date(pur.date);
                    const employeeIdLower = employeeId.toLowerCase(); 
    
                    return saleDate.getFullYear() === selectedYear &&
                           saleDate.getMonth() + 1 === selectedMonth &&
                           saleDate.getDate() === selectedDateValue && // ตรวจสอบวันที่นี้
                           (!employeeId || pur.seller.toLowerCase().startsWith(employeeIdLower));
                });
    
                const hourlyData = Array(24).fill(0);
                filteredData.forEach(pur => {
                    const saleHour = new Date(pur.date).getHours();
                    hourlyData[saleHour] += pur.total;
                });
    
                setHourlySalesData(hourlyData.map((amount, hour) => ({
                    hour: `${hour}:00`,
                    amount,
                })));
    
                const dailySalesSummary = {};
                filteredData.forEach(pur => {
                    const saleDate = new Date(pur.date).toISOString().split('T')[0];
                    if (!dailySalesSummary[saleDate]) {
                        dailySalesSummary[saleDate] = { date: saleDate, total: 0 };
                    }
                    dailySalesSummary[saleDate].total += pur.total;
                });
    
                setSummaryData(prev => ({
                    ...prev,
                    dailySales: Object.values(dailySalesSummary),
                }));
            }
        } catch (error) {
            toast.error('Error fetching sales data');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        setHourlySalesData([]);
        setSummaryData(null);
        fetchSalesData();
    }, [branchId, token, selectedDate, employeeId]);

    const dailySales = summaryData ? summaryData.dailySales : [];

    return (
        <div className="dashboard">
            <Navbar />
            <div className="content">
                <Sidebar />
                <div className="main-content">
                {loading && <div>Loading...</div>}
                {error && <div>{error}</div>}

                <div className="sales-summary">
                    <h2>Sales Summary</h2>
                    <div className="summary-cards">
                        <div className="summary-card">
                            <h3>ยอดขายรวม</h3>
                            <p>฿{summaryData && summaryData.totalSales ? summaryData.totalSales : 'ไม่มีข้อมูล'}</p>
                        </div>
                        <div className="summary-card">
                            <h3>จำนวนบิลขาย</h3>
                            <p>{summaryData && summaryData.totalTransactions ? summaryData.totalTransactions : 'ไม่มีข้อมูล'}</p>
                        </div>
                        <div className="summary-card">
                            <h3>เฉลี่ย/บิล</h3>
                            <p>฿{summaryData && summaryData.totalTransactions ? (summaryData.totalSales / summaryData.totalTransactions).toFixed(2) : 'ไม่มีข้อมูล'}</p>
                        </div>
                    </div>

                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                        <div>
                            <label style={{ marginRight: "5px" }}>เลือกวันที่: </label>
                            <DatePicker
                                selected={selectedDate}
                                onChange={(date) => setSelectedDate(date)}
                                dateFormat="yyyy/MM/dd"
                                isClearable={false} // ไม่ต้องแสดงปุ่มเคลียร์
                            />
                        </div>

                        <div>
                            {/* ปรับสไตล์ให้กับการกรอกชื่อพนักงาน */}
                            <label style={{ marginLeft: "30px" }}>กรอกชื่อพนักงาน: </label>
                            <input
                                type="text-dashboard"
                                value={employeeId}
                                onChange={(e) => setEmployeeId(e.target.value)}
                                placeholder="กรอกชื่อพนักงาน"
                                style={{ marginLeft: '5px', width: '200px' }} // เพิ่มความกว้างให้กับ input
                            />
                        </div>
                    </div>

                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                        <h3>Daily Sales</h3>
                        <div className="chart-type-selector">
                            <FaChartLine
                                className={`chart-icon line-icon ${viewType === 'line' ? 'active' : ''}`}
                                onClick={() => setViewType('line')}
                            />
                            <IoBarChart
                                className={`chart-icon bar-icon ${viewType === 'bar' ? 'active' : ''}`}
                                onClick={() => setViewType('bar')}
                            />
                        </div>
                    </div>

                    <SalesChart dailySales={dailySales} hourlySalesData={hourlySalesData} viewType={viewType} />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Dashboard;