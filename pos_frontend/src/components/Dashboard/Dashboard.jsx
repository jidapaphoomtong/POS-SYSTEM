import { useEffect, useState } from 'react';
import axios from 'axios';
import { useParams } from 'react-router-dom';
import Cookies from 'js-cookie';
import { toast } from "react-toastify";
import Navbar from '../bar/Navbar';
import Sidebar from '../bar/Sidebar';
import SalesChart from './SalesChart';
import "../../styles/dashboard.css";
import { IoBarChart } from "react-icons/io5";
import { FaChartLine } from "react-icons/fa6";

const Dashboard = () => {
    const [summaryData, setSummaryData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [viewType, setViewType] = useState('line');  // 'line' or 'bar'
    const { branchId } = useParams();
    const token = Cookies.get("authToken");

    useEffect(() => {
        if (!branchId) {
            toast.error("Branch ID is missing!");
            return;
        }

        const fetchSalesSummary = async () => {
            try {
                const response = await axios.get(`/api/Purchase/sales-summary/${branchId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    }
                });
                setSummaryData(response.data);
            } catch (error) {
                setError('Error fetching sales data');
                console.error('Error fetching sales data', error);
            } finally {
                setLoading(false);
            }
        };

        fetchSalesSummary();
    }, [branchId, token]);

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
                                <p>฿{summaryData ? summaryData.totalSales : 0}</p>
                            </div>
                            <div className="summary-card">
                                <h3>จำนวนบิลขาย</h3>
                                <p>{summaryData ? summaryData.totalTransactions : 0}</p>
                            </div>
                            <div className="summary-card">
                                <h3>เฉลี่ย/บิล</h3>
                                <p>฿{summaryData ? (summaryData.totalSales / summaryData.totalTransactions).toFixed(2) : 0}</p>
                            </div>
                        </div>
                        <h1></h1>
                        <div className="chart-type-selector">
                            <IoBarChart onClick={() => setViewType('bar')} />
                            <FaChartLine onClick={() => setViewType('line')} />
                        </div>
                        <SalesChart dailySales={dailySales} viewType={viewType} />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Dashboard;