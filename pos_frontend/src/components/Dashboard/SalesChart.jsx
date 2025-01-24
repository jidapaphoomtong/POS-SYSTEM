import React from 'react';
import { Line, Bar } from 'react-chartjs-2';
import {
    Chart,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    BarElement,
} from 'chart.js';
import "../../styles/dashboard.css"

Chart.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement);

const SalesChart = ({ dailySales, viewType }) => {
    const chartData = {
        labels: dailySales.map(sale => sale.date),
        datasets: [
            {
                label: 'ยอดขาย',
                data: dailySales.map(sale => sale.amount),
                backgroundColor: viewType === 'bar' ? '#28c01a' : 'rgba(0, 0, 0, 0)',
                borderColor: '#28c01a',
                fill: viewType === 'line',  // ใช้สำหรับกราฟเส้น
                // ถ้าเป็น 'line', fill จะถูกตั้งค่าเป็น true และมีสีพื้นหลัง
            },
        ],
    };

    const options = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            x: {
                ticks: {
                    font: {
                        size: 14,
                    },
                },
            },
            y: {
                beginAtZero: true,
                ticks: {
                    font: {
                        size: 14,
                    },
                },
            },
        },
        plugins: {
            legend: {
                labels: {
                    font: {
                        size: 14,
                    },
                },
            },
        },
    };

    return (
        <div className="sales-chart" style={{ width: '1143px', height: '200px' }}>
            {viewType === 'line' ? (
                <Line data={chartData} options={options} />
            ) : (
                <Bar data={chartData} options={options} />
            )}
        </div>
    );
};

export default SalesChart;