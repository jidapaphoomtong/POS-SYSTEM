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
    
            const summaryResponse = await axios.get(`/api/Purchase/sales-summary/${branchId}?year=${selectedYear}&month=${selectedMonth}&employeeId=${employeeId}`, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });
    
            setSummaryData(summaryResponse.data);
    
            const response = await axios.get(`/api/Purchase/all-purchases/${branchId}`, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });
    
            if (response.data) {
                const filteredData = response.data.filter(pur => {
                    const saleDate = new Date(pur.date);
                    return saleDate.getFullYear() === selectedYear && 
                           saleDate.getMonth() + 1 === selectedMonth && 
                           (!employeeId || pur.seller === employeeId);
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
                    dailySales: Object.values(dailySalesSummary), // เก็บข้อมูลของวันที่มียอดขาย
                }));
            }
        } catch (error) {
            setError('Error fetching sales data');
            console.error('Error fetching sales data', error);
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
                                    isClearable={true} // ไม่ต้องแสดงปุ่มเคลียร์
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