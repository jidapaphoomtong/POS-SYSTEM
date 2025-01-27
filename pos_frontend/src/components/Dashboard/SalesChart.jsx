// import {
//     Chart,
//     CategoryScale,
//     LinearScale,
//     PointElement,
//     LineElement,
//     BarElement,
//     Tooltip,
//     Filler // อย่าลืมเพิ่ม Filler ที่นี่
// } from 'chart.js';

// Chart.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement, Tooltip, Filler)

// const SalesChart = ({ dailySales = [], hourlySalesData = [], viewType }) => {
//     const salesData = viewType === 'line' ? hourlySalesData : dailySales;

//     const chartData = {
//         labels: viewType === 'line' 
//             ? hourlySalesData.map(sale => sale.hour) 
//             : dailySales.map(sale => sale.date),
//         datasets: [
//             {
//                 label: 'ยอดขาย',
//                 data: salesData.map(sale => viewType === 'line' ? sale.amount : sale.total || 0),
//                 backgroundColor: viewType === 'line' ? 'rgba(0, 123, 255, 0.5)' : '#58c5f0',
//                 borderColor: '#9bc8f8',
//                 borderWidth: 2,
//                 fill: viewType === 'line',
//             },
//         ],
//     };

//     const options = {
//         responsive: true,
//         maintainAspectRatio: false,
//         scales: {
//             x: {
//                 ticks: { font: { size: 14 } },
//             },
//             y: {
//                 beginAtZero: true,
//                 ticks: { font: { size: 14 } },
//             },
//         },
//         plugins: {
//             legend: {
//                 labels: { font: { size: 14 } },
//             },
//         },
//         tooltip: {
//             callbacks: {
//                 label: (tooltipItem) => {
//                     const label = tooltipItem.dataset.label || '';
//                     const value = tooltipItem.raw; 
//                     return `${label}: ฿${value}`;
//                 },
//                 title: (tooltipItem) => tooltipItem[0].label, 
//             },
//         },
//     };

//     return (
//         <div className="sales-chart" style={{ width: '100%', height: '200px' }}>
//             {viewType === 'line' ? (
//                 <Line data={chartData} options={options} />
//             ) : (
//                 <Bar data={chartData} options={options} />
//             )}
//         </div>
//     );
// };
// export default SalesChart;
import React from 'react';
import { Line, Bar } from 'react-chartjs-2'; // นำเข้า Line และ Bar
import {
    Chart,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    BarElement,
    Tooltip,
    Filler
} from 'chart.js';

Chart.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement, Tooltip, Filler);

const SalesChart = ({ dailySales = [], hourlySalesData = [], viewType }) => {
    const salesData = viewType === 'line' ? hourlySalesData : hourlySalesData; // ใช้ hourlySalesData สำหรับทั้งสองกราฟ

    const chartData = {
        labels: hourlySalesData.map(sale => sale.hour), // ใช้ชั่วโมงสำหรับ labels ทั้งกราฟ
        datasets: [
            {
                label: 'ยอดขาย',
                data: salesData.map(sale => sale.amount || 0), // แสดง amount สำหรับกราฟทั้งสองแบบ
                backgroundColor: viewType === 'line' ? 'rgba(0, 123, 255, 0.5)' : '#58c5f0',
                borderColor: '#9bc8f8',
                borderWidth: 2,
                fill: viewType === 'line',
            },
        ],
    };

    const options = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            x: {
                ticks: { font: { size: 14 } },
            },
            y: {
                beginAtZero: true,
                ticks: { font: { size: 14 } },
            },
        },
        plugins: {
            legend: {
                labels: { font: { size: 14 } },
            },
        },
        tooltip: {
            callbacks: {
                label: (tooltipItem) => {
                    const label = tooltipItem.dataset.label || '';
                    const value = tooltipItem.raw; 
                    return `${label}: ฿${value}`;
                },
                title: (tooltipItem) => tooltipItem[0].label, 
            },
        },
    };

    return (
        <div className="sales-chart" style={{ width: '100%', height: '200px' }}>
            {viewType === 'line' ? (
                <Line data={chartData} options={options} />
            ) : (
                <Bar data={chartData} options={options} />
            )}
        </div>
    );
};

export default SalesChart;