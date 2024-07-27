import React, { useState, useEffect } from 'react';
import './DataOperations.css';
import Button from './Button';

const API_BASE_URL = 'http://localhost:5000/api/dataoperations'; // Adjust this to match your API's URL

const DataOperations = () => {
  const [userDashboard, setUserDashboard] = useState(null);
  const [inventoryStatus, setInventoryStatus] = useState(null);
  const [newOrder, setNewOrder] = useState('');
  const [customerFeedback, setCustomerFeedback] = useState('');
  const [showOrderModal, setShowOrderModal] = useState(false);
  const [showFeedbackModal, setShowFeedbackModal] = useState(false);

  const orderExample = {
    orderId: "ORD-2023-001",
    customerId: "CUST-12345",
    orderDate: "2023-07-27T15:30:00Z",
    items: [
      {
        productId: "PROD-A1",
        productName: "Wireless Earbuds",
        quantity: 2,
        unitPrice: 79.99
      },
      {
        productId: "PROD-B2",
        productName: "Smartphone Case",
        quantity: 1,
        unitPrice: 19.99
      }
    ],
    shippingAddress: {
      street: "123 Main St",
      city: "Anytown",
      state: "CA",
      zipCode: "12345",
      country: "USA"
    },
    paymentMethod: "Credit Card",
    totalAmount: 179.97
  };

  const feedbackExample = {
    feedbackId: "FB-2023-001",
    customerId: "CUST-12345",
    orderReference: "ORD-2023-001",
    submissionDate: "2023-07-27T16:45:00Z",
    overallRating: 4,
    categories: {
      productQuality: 5,
      delivery: 4,
      customerService: 3
    },
    comments: "The product exceeded my expectations, but delivery was slightly delayed. Customer service was helpful but could be more responsive.",
    wouldRecommend: true,
    tags: ["quality", "delivery", "improvement"]
  };

  useEffect(() => {
    fetchUserDashboard();
    fetchInventoryStatus();
  }, []);

  const fetchUserDashboard = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/userdashboard`);
      const data = await response.json();
      setUserDashboard(data);
    } catch (error) {
      console.error('Error fetching user dashboard:', error);
    }
  };

  const fetchInventoryStatus = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/inventorystatus`);
      const data = await response.json();
      setInventoryStatus(data);
    } catch (error) {
      console.error('Error fetching inventory status:', error);
    }
  };

  const handleOrderSubmit = async (e) => {
    e.preventDefault();
    try {
      const orderData = JSON.parse(newOrder);
      const response = await fetch(`${API_BASE_URL}/createorder`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(orderData),
      });
      const result = await response.json();
      console.log('Order creation result:', result);
      setNewOrder('');
    } catch (error) {
      console.error('Error creating order:', error);
      alert('Error creating order. Please check your input and try again.');
    }
  };

  const handleFeedbackSubmit = async (e) => {
    e.preventDefault();
    try {
      const feedbackData = JSON.parse(customerFeedback);
      const response = await fetch(`${API_BASE_URL}/submitfeedback`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(feedbackData),
      });
      const result = await response.json();
      console.log('Feedback submission result:', result);
      setCustomerFeedback('');
    } catch (error) {
      console.error('Error submitting feedback:', error);
      alert('Error submitting feedback. Please check your input and try again.');
    }
  };

  const Modal = ({ show, onClose, title, content }) => {
    if (!show) return null;
    return (
      <div className="modal-overlay">
        <div className="modal">
          <h3>{title}</h3>
          <pre>{JSON.stringify(content, null, 2)}</pre>
          <Button onClick={onClose}>Close</Button>
        </div>
      </div>
    );
  };

  return (
    <div className="dashboard">
      <h1 className="dashboard-title">Data Operations Dashboard</h1>
      <div className="dashboard-grid">
        <div className="dashboard-item">
          <h2>User Dashboard Summary</h2>
          <div className="data-display">
            <pre>{JSON.stringify(userDashboard, null, 2)}</pre>
          </div>
        </div>
        <div className="dashboard-item">
          <h2>Product Inventory Status</h2>
          <div className="data-display">
            <pre>{JSON.stringify(inventoryStatus, null, 2)}</pre>
          </div>
        </div>
        <div className="dashboard-item">
          <h2>Create Order <span className="info-icon" onClick={() => setShowOrderModal(true)}>?</span></h2>
          <form onSubmit={handleOrderSubmit}>
            <textarea 
              value={newOrder} 
              onChange={(e) => setNewOrder(e.target.value)}
              placeholder="Enter order JSON data"
            />
            <Button type="submit">Submit Order</Button>
          </form>
        </div>
        <div className="dashboard-item">
          <h2>Submit Customer Feedback <span className="info-icon" onClick={() => setShowFeedbackModal(true)}>?</span></h2>
          <form onSubmit={handleFeedbackSubmit}>
            <textarea 
              value={customerFeedback} 
              onChange={(e) => setCustomerFeedback(e.target.value)}
              placeholder="Enter feedback JSON data"
            />
            <Button type="submit">Submit Feedback</Button>
          </form>
        </div>
      </div>
      <Modal 
        show={showOrderModal} 
        onClose={() => setShowOrderModal(false)} 
        title="Order JSON Structure"
        content={orderExample}
      />
      <Modal 
        show={showFeedbackModal} 
        onClose={() => setShowFeedbackModal(false)} 
        title="Feedback JSON Structure"
        content={feedbackExample}
      />
    </div>
  );
};

export default DataOperations;