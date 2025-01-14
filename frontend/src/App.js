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
import Order from './components/order';
import EmployeeList from './components/Employee/EmployeeList';
import ProductList from './components/Product/ProductList';
import EditEmployee from './components/Employee/EditEmployee';
import AddEmployee from './components/Employee/AddEmployee';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/select-branch" element={<SelectBranch />} />
        <Route path="/sale" element={<Sale />} />
        <Route path='/order' element={<Order />}/>
        <Route path="/BranchList" element={<BranchList />} />
        <Route path="/edit-branch/:branchId" element={<EditBranch />} />
        <Route path="/add-branch" element={<AddBranch />} />
        <Route path="/branch/:branchId" element={<BranchDetail />} />
        <Route path='/EmployeeList' element={<EmployeeList />}/>
        {/* <Route path='/ProductList' element={<ProductList />}/> */}
        <Route path="/edit-employee/:employeeId" element={<EditEmployee />} />
        <Route path="/add-employee" element={<AddEmployee />} />
      </Routes>
      <ToastContainer />
    </Router>
  );
}

export default App;