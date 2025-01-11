import axios from "axios";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "../../styles/employee.css"; // Add appropriate CSS file for styling

const EmployeeList = () => {
    const [employees, setEmployees] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    // Fetch Employee List
    useEffect(() => {
        const fetchEmployees = async () => {
            try {
                setIsLoading(true);
                const response = await axios.get("http://localhost:5293/api/Admin/GetEmployees");
                setEmployees(response.data.data || []);
            } catch (error) {
                console.error("Failed to fetch employees:", error);
                alert("Failed to fetch employee data.");
            } finally {
                setIsLoading(false);
            }
        };
        
        fetchEmployees();
    }, []);

    return (
        <div className="employee-container">
            <div className="header">
                <h1>Staff Management</h1>
                <button
                    className="add-button"
                    onClick={() => navigate('/add-employee')} // Navigate to add employee page
                >
                    Add Staff
                </button>
            </div>

            {isLoading ? (
                <p>Loading employees...</p>
            ) : (
                <table className="employee-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Name</th>
                            <th>Email</th>
                            <th>Phone</th>
                            <th>Age</th>
                            <th>Salary</th>
                            <th>Timings</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {employees.map((employee) => (
                            <tr key={employee.id}>
                                <td>{employee.id}</td>
                                <td>{employee.name}</td>
                                <td>{employee.email}</td>
                                <td>{employee.phone}</td>
                                <td>{employee.age}</td>
                                <td>${employee.salary.toFixed(2)}</td>
                                <td>{employee.timings}</td>
                                <td className="action-buttons">
                                    <button onClick={() => navigate(`/edit-employee/${employee.id}`)}>✏️</button>
                                    <button onClick={() => handleDelete(employee.id)}>🗑️</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            {employees.length === 0 && !isLoading && <p>No employees found.</p>}
        </div>
    );
};

const handleDelete = async (id) => {
    // Implement delete functionality
    // Show confirmation modal
    if (window.confirm(`Are you sure you want to delete employee #${id}?`)) {
        // Call the API to delete
        try {
            await axios.delete(`http://localhost:5293/api/Admin/DeleteEmployee/${id}`);
            alert("Employee deleted successfully!");
            // Refresh the employee list after deletion
        } catch (error) {
            console.error("Failed to delete employee:", error);
            alert("Failed to delete employee.");
        }
    }
};

export default EmployeeList;