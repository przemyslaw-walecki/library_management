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
    <nav className="navbar navbar-expand-lg navbar-light bg-light shadow-sm p-3">
      <div className="container">
        <span className="navbar-brand">Library System</span>
        <div className="d-flex" >
          {role ? (
            <>
              {role === 'User' && (
                <>
                  <button
                    onClick={() => navigate('/books')}
                    className="btn btn-primary me-2"
                  >
                    Book List
                  </button>
                  <button
                    onClick={() => navigate('/myaccount')}
                    className="btn btn-primary me-2"
                  >
                    My Account
                  </button>
                </>
              )}
              {role === 'Librarian' && (
                <button
                  onClick={() => navigate('/manage-books')}
                  className="btn btn-primary me-2"
                >
                  Manage Books
                </button>
              )}
              <button
                onClick={handleLogout}
                className="btn btn-danger"
              >
                Logout
              </button>
            </>
          ) : (
            <>
              <button
                onClick={() => navigate('/books')}
                className="btn btn-primary me-2"
              >
                Book List
              </button>
              <button
                onClick={() => navigate('/login')}
                className="btn btn-success"
              >
                Login
              </button>
            </>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
