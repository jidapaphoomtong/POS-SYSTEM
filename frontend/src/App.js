import './App.css';
import Register from './components/register';
import Login from './components/login';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import SelectBranch from './components/SelectBranch';
import Sale from './components/Sale';
import BranchList from './components/Branch/BranchList';
import EditBranch from './components/Branch/EditBranch';
import AddBranch from './components/Branch/AddBranch';
import BranchDetail from './components/Branch/BranchDetail';
import EmployeeList from './components/Employee/EmployeeList';
import ProductList from './components/Product/ProductList';
import EditEmployee from './components/Employee/EditEmployee';
import AddEmployee from './components/Employee/AddEmployee';
import AddProduct from './components/Product/AddProduct';
import EditProduct from './components/Product/EditProduct'
import EmployeeDetail from './components/Employee/EmployeeDetail';
import ProductDetail from './components/Product/ProductDetail';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/select-branch" element={<SelectBranch />} />
        <Route path="/sale" element={<Sale />} />
        <Route path="/BranchList" element={<BranchList />} />
        <Route path="/edit-branch/:branchId" element={<EditBranch />} />
        <Route path="/add-branch" element={<AddBranch />} />
        <Route path="/branch/:branchId" element={<BranchDetail />} />
        <Route path='/EmployeeList' element={<EmployeeList />}/>
        <Route path="/edit-employee/:employeeId" element={<EditEmployee />} />
        <Route path="/add-employee/:branchId" element={<AddEmployee />} />
        <Route path="/employee/:employeeId" element={<EmployeeDetail />} />
        <Route path='/ProductList' element={<ProductList />}/>
        <Route path="/edit-product/:productId" element={<EditProduct/>} />
        <Route path="/add-product/:branchId" element={<AddProduct />} />
        <Route path="/product/:productId" element={<ProductDetail />} />
      </Routes>
      
      {/* <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/select-branch" element={<SelectBranch />} />

        <Route path="/sale/:branchId" element={<Sale />} />

        <Route path="/BranchList" element={<BranchList />} />
        <Route path="/:branchId/BranchList/edit-branch" element={<EditBranch />} />
        <Route path="/:branchId/BranchList/add-branch" element={<AddBranch />} />
        <Route path="/:branchId/BranchList/branch" element={<BranchDetail />} />

        <Route path="/:branchId/EmployeeList" element={<EmployeeList />} />
        <Route path="/:branchId/EmployeeList/edit-employee/:employeeId" element={<EditEmployee />} />
        <Route path="/:branchId/EmployeeList/add-employee" element={<AddEmployee />} />
        <Route path="/:branchId/EmployeeList/employee/:employeeId" element={<EmployeeDetail />} />

        <Route path="/:branchId/ProductList" element={<ProductList />} />
        <Route path="/:branchId/ProductList/edit-product/:productId" element={<EditProduct />} />
        <Route path="/:branchId/ProductList/add-product" element={<AddProduct />} />
        <Route path="/:branchId/ProductList/product/:productId" element={<ProductDetail />} />
      </Routes> */}
      <ToastContainer />
    </Router>
  );
}

export default App;