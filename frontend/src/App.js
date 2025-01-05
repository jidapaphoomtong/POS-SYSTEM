import './App.css';
import Register from './components/register';
import Login from './components/login';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import SelectBranch from './components/SelectBranch';
import Sale from './components/Sale';
import BranchList from './components/Branch/BranchList';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/select-branch" element={<SelectBranch />} />
        <Route path="/sale" element={<Sale />} />
        <Route path="/BranchList" element={<BranchList />} />
      </Routes>
      <ToastContainer />
    </Router>
  );
}

export default App;