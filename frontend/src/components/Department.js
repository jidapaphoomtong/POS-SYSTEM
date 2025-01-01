import React from 'react';
import { useNavigate } from 'react-router-dom';

export default function Department() {
    const navigate = useNavigate();

    return (
        <div style={{ textAlign: 'center', marginTop: '50px' }}>
        <h1>Department Page</h1>
        <p>List or manage departments here</p>
        <button onClick={() => navigate('/select-branch')}>Back</button>
        </div>
    );
}

