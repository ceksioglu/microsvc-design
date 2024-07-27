import React from 'react';
import './DataOperations.css'; // Make sure this CSS file includes button styles

const Button = ({ children, onClick, type = 'button', className = '' }) => {
  return (
    <button
      className={`custom-button ${className}`}
      onClick={onClick}
      type={type}
    >
      {children}
    </button>
  );
};

export default Button;