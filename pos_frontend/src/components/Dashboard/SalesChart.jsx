import React from 'react';
import { Line } from 'react-chartjs-2';
import {
    Chart,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
} from 'chart.js';
import "../../styles/dashboard.css"

// ลงทะเบียน elements และ scales ที่จำเป็น
Chart.register(CategoryScale, LinearScale, PointElement, LineElement);

const SalesChart = ({ dailySales }) => {
    // แปลงข้อมูลให้เหมาะสมสำหรับกราฟ
    const chartData = {
        labels: dailySales.map(sale => sale.date),
        datasets: [
            {
                label: 'ยอดขาย',
                data: dailySales.map(sale => sale.amount),
                borderColor: 'rgba(75,192,192,1)',
                fill: false,
            },
        ],
    };

    return (
        <div className="sales-chart" >
            <Line data={chartData} />
        </div>
    );
};

export default SalesChart;