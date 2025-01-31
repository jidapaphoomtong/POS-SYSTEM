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
        averagePerTransaction: 0,
    });
    const [hourlySalesData, setHourlySalesData] = useState([]);
    const [monthlySalesData, setMonthlySalesData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [viewType, setViewType] = useState('line');
    const [startDate, setStartDate] = useState(new Date());
    const [endDate, setEndDate] = useState(new Date());
    const [employeeId, setEmployeeId] = useState('');
    const [employees, setEmployees] = useState([]);
    const { branchId } = useParams();
    const token = Cookies.get("authToken");

    const fetchEmployees = async () => {
        try {
            const response = await axios.get(`/api/Employee/branches/${branchId}/employees`, {
                headers: { Authorization: `Bearer ${token}` },
                withCredentials: true,
            });

            if (response.data && Array.isArray(response.data.data)) {
                setEmployees(response.data.data);
            } else {
                toast.error('No employees found or data format is incorrect.');
            }
        } catch (error) {
            toast.error('Error fetching employees');
            console.error('Fetching employees error:', error);
        }
    };

    const fetchSalesData = async () => {
        try {
            const selectedStartDate = startDate.toISOString().split('T')[0];
            const selectedEndDate = endDate.toISOString().split('T')[0];

            const summaryResponse = await axios.get(`/api/Purchase/sales-summary/${branchId}?startDate=${selectedStartDate}&endDate=${selectedEndDate}`, {
                headers: { Authorization: `Bearer ${token}` },
                withCredentials: true,
            });

            const dailySalesData = summaryResponse.data.dailySales || [];
            
            // ฟิลเตอร์ข้อมูลตามวันและพนักงานที่เลือก
            const filteredDailySalesData = dailySalesData.filter(sale => {
                const saleDate = new Date(sale.date);
                const employeeIdLower = employeeId.toLowerCase();
                
                // เอาเวลาออกแล้วเพียงแค่ตรวจสอบปี เดือน วัน
                const isSameDay = saleDate.getFullYear() === startDate.getFullYear() &&
                                saleDate.getMonth() === startDate.getMonth() &&
                                saleDate.getDate() === startDate.getDate();
                
                return (
                    isSameDay &&
                    (employeeId === '' || sale.seller.toLowerCase().includes(employeeIdLower))
                );
            });

            // คำนวณข้อมูลสรุปจากข้อมูลที่กรองแล้ว
            const totalSales = filteredDailySalesData.reduce((sum, sale) => sum + sale.amount, 0);
            const totalTransactions = filteredDailySalesData.reduce((sum, sale) => sum + sale.transactionCount, 0);
            const averagePerTransaction = totalTransactions > 0 
                ? totalSales / totalTransactions 
                : 0;

            // อัปเดต summaryData state
            setSummaryData({
                dailySales: filteredDailySalesData, // ใช้ข้อมูลที่กรองแล้ว
                totalSales: totalSales,
                totalTransactions: totalTransactions,
                averagePerTransaction: averagePerTransaction,
            });

            const response = await axios.get(`/api/Purchase/all-purchases/${branchId}`, {
                headers: { Authorization: `Bearer ${token}` },
                withCredentials: true,
            });

            // ฟิลเตอร์ข้อมูลตามวันและพนักงานที่เลือก
            const filteredData = response.data.filter(pur => {
                const saleDate = new Date(pur.date);
                const employeeIdLower = employeeId.toLowerCase();
            
                // เอาเวลาออกแล้วเพียงแค่ตรวจสอบปี เดือน วัน
                const isSameDay = saleDate.getFullYear() === startDate.getFullYear() &&
                                  saleDate.getMonth() === startDate.getMonth() &&
                                  saleDate.getDate() === startDate.getDate();
            
                return (
                    isSameDay &&
                    (employeeId === '' || pur.seller.toLowerCase().includes(employeeIdLower))
                );
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
        } catch (error) {
            toast.error('Error fetching sales data');
        } finally {
            setLoading(false);
        }
    };

    const fetchMonthlySalesData = async () => {
        try {
            const selectedYear = startDate.getFullYear();
            const selectedMonth = startDate.getMonth() + 1;
    
            const monthlySalesResponse = await axios.get(`/api/Purchase/monthly-sales/${branchId}/${selectedYear}/${selectedMonth}`, {
                headers: { Authorization: `Bearer ${token}` },
                withCredentials: true,
            });
    
            if (monthlySalesResponse.data) {
                const monthlySales = monthlySalesResponse.data;
                const filteredMonthlySales = monthlySales.filter(purchase =>
                    purchase.seller.toLowerCase().includes(employeeId.toLowerCase())
                );

                const dailySalesData = Array(31).fill(0);
                filteredMonthlySales.forEach(purchase => {
                    const purchaseDate = new Date(purchase.date);
                    const day = purchaseDate.getDate();
                    dailySalesData[day - 1] += purchase.total;
                });
    
                setMonthlySalesData(dailySalesData);
            } else {
                toast.error('No monthly sales data found.');
            }
        } catch (error) {
            toast.error('Error fetching monthly sales data');
            console.error('Fetching monthly sales error:', error);
        }
    };

    useEffect(() => {
        setHourlySalesData([]);
        fetchEmployees();
        fetchSalesData();
        fetchMonthlySalesData();
    }, [branchId, token, startDate, endDate, employeeId]);

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
                                <label style={{ marginRight: "5px" }}>เลือกวันที่เริ่มต้น: </label>
                                <DatePicker
                                    selected={startDate}
                                    onChange={(date) => {
                                        setStartDate(date);
                                        setEndDate(date);
                                    }}
                                    dateFormat="yyyy/MM/dd"
                                    isClearable={false}
                                />
                            </div>
                            <div>
                                <label style={{ marginLeft: "30px" }}>เลือกวันที่สิ้นสุด: </label>
                                <DatePicker
                                    selected={endDate}
                                    onChange={(date) => setEndDate(date)}
                                    dateFormat="yyyy/MM/dd"
                                    isClearable={false}
                                    minDate={startDate}
                                />
                            </div>
                            <div>
                                <label style={{ marginLeft: "30px" }}>เลือกชื่อพนักงาน: </label>
                                <select
                                    value={employeeId}
                                    onChange={(e) => setEmployeeId(e.target.value)}
                                    className="custom-dropdown" /* เพิ่ม class ที่เราสร้างขึ้น */
                                >
                                    <option value="" disabled>เลือกพนักงาน</option>
                                    <option value="">ไม่เลือกพนักงาน</option>
                                    {Array.isArray(employees) && employees.map(employee => (
                                        <option key={employee.data.id} value={employee.data.firstName.toLowerCase()}>
                                            {employee.data.firstName || 'ไม่ระบุชื่อ'}
                                        </option>
                                    ))}
                                </select>
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