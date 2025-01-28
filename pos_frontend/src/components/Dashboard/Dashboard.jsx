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
    const [summaryData, setSummaryData] = useState({
        dailySales: [],
        totalSales: 0,
        totalTransactions: 0,
        averagePerTransaction: 0
    });
    const [hourlySalesData, setHourlySalesData] = useState([]);
    const [monthlySalesData, setMonthlySalesData] = useState([]);
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
            
            const summaryResponse = await axios.get(`/api/Purchase/sales-summary/${branchId}?year=${selectedYear}&month=${selectedMonth}&day=${selectedDateValue}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });
            
            // คัดกรองข้อมูลตามวันที่
            const filteredSummaryData = Array.isArray(summaryResponse.data.dailySales) 
                ? summaryResponse.data.dailySales.filter(sale => {
                    const saleDate = new Date(sale.date);
                    return (
                        saleDate.getFullYear() === selectedYear &&
                        saleDate.getMonth() + 1 === selectedMonth &&
                        saleDate.getDate() === selectedDateValue)
                }) 
                : [];
                
            // ตรวจสอบข้อมูล
            if (filteredSummaryData.length > 0) {
                const dailySalesData = filteredSummaryData[0];
                setSummaryData({
                    dailySales: [dailySalesData],
                    totalSales: dailySalesData.amount,
                    totalTransactions: dailySalesData.transactionCount,
                    averagePerTransaction: dailySalesData.averagePerTransaction
                });
            } else {
                // ไม่มีข้อมูล
                setSummaryData({
                    dailySales: [],
                    totalSales: 0,
                    totalTransactions: 0,
                    averagePerTransaction: 0
                });
            }
        
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
                        saleDate.getDate() === selectedDateValue &&
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

    const fetchMonthlySalesData = async () => {
        try {
            const selectedYear = selectedDate.getFullYear();
            const selectedMonth = selectedDate.getMonth() + 1; // เดือนใน JavaScript จะเริ่มจาก 0
    
            const monthlySalesResponse = await axios.get(`/api/Purchase/monthly-sales/${branchId}/${selectedYear}/${selectedMonth}`, {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });
    
            if (monthlySalesResponse.data) {
                const monthlySales = monthlySalesResponse.data;
    
                // คำนวณยอดขายต่อวัน
                const dailySalesData = Array(31).fill(0);
                monthlySales.forEach(purchase => {
                    const purchaseDate = new Date(purchase.date);
                    const day = purchaseDate.getDate();
                    dailySalesData[day - 1] += purchase.total; // แสดงรวมยอดขาย
                });
    
                setMonthlySalesData(dailySalesData); // สร้าง state ใหม่สำหรับเก็บข้อมูลยอดขายรายเดือน
            }
        } catch (error) {
            toast.error('Error fetching monthly sales data');
        }
    };

    useEffect(() => {
        setHourlySalesData([]);
        fetchSalesData();
        fetchMonthlySalesData();
    }, [branchId, token, selectedDate, employeeId]);

    const dailySales = summaryData.dailySales;

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
                                <p>฿{summaryData.totalSales.toFixed(2) || 'ไม่มีข้อมูล'}</p>
                            </div>
                            <div className="summary-card">
                                <h3>จำนวนบิลขาย</h3>
                                <p>{summaryData.totalTransactions || 'ไม่มีข้อมูล'}</p>
                            </div>
                            <div className="summary-card">
                                <h3>เฉลี่ย/บิล</h3>
                                <p>฿{summaryData.averagePerTransaction.toFixed(2) || 'ไม่มีข้อมูล'}</p>
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
                                <label style={{ marginLeft: "30px" }}>กรอกชื่อพนักงาน: </label>
                                <input
                                    type="text"
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

                        <SalesChart dailySales={dailySales} hourlySalesData={hourlySalesData} monthlySalesData={monthlySalesData} viewType={viewType} />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Dashboard;