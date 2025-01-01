import './App.css';
import Register from './components/register';
import Login from './components/login';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import SelectBranch from './components/SelectBranch';
import Sale from './components/Sale';
import Department from './components/Department';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/select-branch" element={<SelectBranch />} />
        <Route path="/sale" element={<Sale />} />
        <Route path="/department" element={<Department />} />
      </Routes>
      <ToastContainer />
    </Router>
  );
}

export default App;