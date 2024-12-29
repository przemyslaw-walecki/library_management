import React from 'react';
import { useNavigate } from 'react-router-dom';
import { logout } from '../../services/api';

const Navbar: React.FC = () => {
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout failed', error);
    }
  };

  return (
    <nav>
      <button onClick={() => navigate('/login')}>Login</button>
      <button onClick={() => navigate('/books')}>Book List</button>
      <button onClick={handleLogout}>Logout</button>
    </nav>
  );
};

export default Navbar;
