// const fetchAllPurchases = async () => {
//     const token = Cookies.get("authToken");
//     const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");

//     if (token) {
//         try {
//             const response = await axios.get(`http://your-api-url/api/purchases/all-purchases/${branchId}`, {
//                 headers: {
//                     Authorization: `Bearer ${token}`,
//                 },
//             });
//             console.log("All Purchases:", response.data);
//             // ประมวลผลข้อมูลที่ดึงมาได้
//         } catch (error) {
//             console.error("Error fetching all purchases:", error);
//         }
//     }
// };

// const fetchMonthlySales = async (year, month) => {
//     const token = Cookies.get("authToken");
//     const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");

//     if (token) {
//         try {
//             const response = await axios.get(`http://your-api-url/api/purchases/monthly-sales/${branchId}/${year}/${month}`, {
//                 headers: {
//                     Authorization: `Bearer ${token}`,
//                 },
//             });
//             console.log("Monthly Sales:", response.data);
//             // ประมวลผลข้อมูลที่ดึงมาได้
//         } catch (error) {
//             console.error("Error fetching monthly sales:", error);
//         }
//     }
// };

// const fetchEmployeeSales = async (employeeId) => {
//     const token = Cookies.get("authToken");
//     const branchId = new URLSearchParams(window.location.search).get("branch") || Cookies.get("branchId");

//     if (token) {
//         try {
//             const response = await axios.get(`http://your-api-url/api/purchases/employee-sales/${branchId}/${employeeId}`, {
//                 headers: {
//                     Authorization: `Bearer ${token}`,
//                 },
//             });
//             console.log("Employee Sales:", response.data);
//             // ประมวลผลข้อมูลที่ดึงมาได้
//         } catch (error) {
//             console.error("Error fetching employee sales:", error);
//         }
//     }
// };