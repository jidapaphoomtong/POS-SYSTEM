import { useEffect, useState } from "react";
import "../../styles/employee.css";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import Cookies from "js-cookie";

const EditEmployee = () => {
    const navigate = useNavigate();
    const { employeeId } = useParams(); // ดึง Employee ID จาก URL
    const branchId = new URLSearchParams(window.location.search).get("branch"); // ดึง Branch ID จาก URL

    const [formData, setFormData] = useState({
        firstName: "",
        lastName: "",
        email: "",
    });
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (!employeeId || !branchId) {
            alert("Employee ID or Branch ID is missing.");
            navigate(`/EmployeeList?branch=${branchId}`);
            return;
        }

        const token = Cookies.get("authToken");

        const fetchEmployee = async () => {
            setIsLoading(true);
            try {
                const response = await axios.get(`https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/Employee/getEmployeeById?branchId=${branchId}&employeeId=${employeeId}`, {
                    headers: {
                        "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                        Authorization: `Bearer ${token}`,
                    },
                    withCredentials: true,
                }); console.log(response);

                if (response.status === 200) {
                    if (response.success) {
                        // const { firstName, lastName, email } = response.data;
                        // setFormData({ firstName, lastName, email });
                        setFormData({
                            firstName: response.data.firstName,
                            lastName: response.data.lastName,
                            email: response.data.email,
                        });
                    }
                    console.log(response.data)
                }
            } catch (error) {
                alert("Failed to fetch employee details.");
                navigate(`/EmployeeList?branch=${branchId}`);
            } finally {
                setIsLoading(false);
            }
        };

        fetchEmployee();
    }, [employeeId, branchId, navigate]);

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.firstName || !formData.lastName || !formData.email) {
            alert("Please fill out all fields!");
            return;
        }

        setIsLoading(true);
        const token = Cookies.get("authToken");

        try {
            const response = await axios.put(`https://jidapa-backend-service-qh6is2mgxa-as.a.run.app/api/Employee/employees/${employeeId}`, formData, {
                headers: {
                    "x-posapp-header": "gi3hcSCTAuof5evF3uM3XF2D7JFN2DS",
                    Authorization: `Bearer ${token}`,
                },
                withCredentials: true,
            });

            if (response.status === 200) {
                alert("Employee updated successfully!");
                navigate(`/EmployeeList?branch=${branchId}`);
            }
        } catch (error) {
            alert("Failed to update employee.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="edit-employee-container">
            <h2>Edit Employee</h2>
            {isLoading ? (
                <p>Loading...</p>
            ) : (
                <form onSubmit={handleSubmit} className="edit-employee-form">
                    <input
                        type="text"
                        name="firstName"
                        placeholder="First Name"
                        value={formData.firstName}
                        onChange={handleChange}
                        required
                    />
                    <input
                        type="text"
                        name="lastName"
                        placeholder="Last Name"
                        value={formData.lastName}
                        onChange={handleChange}
                        required
                    />
                    <input
                        type="text"
                        name="email"
                        placeholder="Email"
                        value={formData.email}
                        onChange={handleChange}
                        required
                    />

                    <div className="form-buttons">
                        <button type="button" onClick={() => navigate(`/EmployeeList?branch=${branchId}`)} className="cancel-button">
                            Cancel
                        </button>
                        <button type="submit" className="save-button">
                            {isLoading ? "Saving..." : "Save"}
                        </button>
                    </div>
                </form>
            )}
        </div>
    );
};

export default EditEmployee;