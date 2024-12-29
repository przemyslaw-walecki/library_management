import React from 'react';
import { useNavigate } from 'react-router-dom';
import { logout } from '../../services/api';

const Navbar: React.FC = () => {
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await logout();
      localStorage.removeItem('role');
      navigate('/login');
    } catch (error) {
      console.error('Logout failed', error);
    }
  };

  const role = localStorage.getItem('role');

  return (
    <nav>
      {role ? (
        <>
          {role === 'User' && (
            <>
              <button onClick={() => navigate('/books')}>Book List</button>
              <button onClick={() => navigate('/myaccount')}>My Account</button>
            </>
          )}

          {role === 'Librarian' && (
            <button onClick={() => navigate('/manage-books')}>Manage Books</button>
          )}

          <button onClick={handleLogout}>Logout</button>
        </>
      ) : (
        <>
        <button onClick={() => navigate('/books')}>Book List</button>
        <button onClick={() => navigate('/login')}>Login</button>
        </>
      )}
    </nav>
  );
};

export default Navbar;
