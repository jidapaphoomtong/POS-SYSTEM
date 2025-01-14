import React from "react";
import "../../styles/confirmationModal.css";

const ConfirmationModal = ({ isOpen, onClose, onConfirm, message }) => {
    if (!isOpen) return null; // ถ้า `isOpen` เป็น false ไม่ต้องแสดง Modal

    return (
        <div className="modal-overlay">
            <div className="modal-container">
                <h3>Confirmation</h3>
                <p>{message}</p>
                <div className="modal-buttons">
                    <button className="cancel-button" onClick={onClose}>
                        Cancel
                    </button>
                    <button className="confirm-button" onClick={onConfirm}>
                        Confirm
                    </button>
                </div>
            </div>
        </div>
    );
};

export default ConfirmationModal;