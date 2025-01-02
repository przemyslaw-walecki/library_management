import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchUserAccount, deleteAccount, cancelReservation, User } from '../../services/api';

const MyAccount: React.FC = () => {
  const [user, setUser] = useState<User | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const navigate = useNavigate();

  useEffect(() => {
    const loadUserAccount = async () => {
      try {
        const userData = await fetchUserAccount();
        setUser(userData);
      } catch (error) {
        console.error('Error fetching user account:', error);
        setErrorMessage('Failed to load account data.');
      } finally {
        setLoading(false);
      }
    };

    loadUserAccount();
  }, []);

  useEffect(() => {
    if (localStorage.getItem('role') !== 'User') {
      navigate('/manage-books');
    }
  }, [navigate]);

  const handleDeleteAccount = async () => {
    try {
      await deleteAccount();
      setSuccessMessage('Your account has been deleted.');
      setUser(null);
      setTimeout(() => navigate('/books'), 1000);
    } catch (error) {
      console.error('Error deleting account:', error);
      setErrorMessage('Failed to delete your account.');
    }
  };

  const handleCancelReservation = async (reservationId: number) => {
    try {
      await cancelReservation(reservationId);
      setUser((prevUser) =>
        prevUser
          ? {
              ...prevUser,
              reservations: {
                ...prevUser.reservations,
                $values: prevUser.reservations?.$values?.filter(
                  (r) => r.reservationId !== reservationId
                ) || [],
              },
            }
          : null
      );
      setSuccessMessage('Reservation canceled successfully.');
    } catch (error) {
      console.error('Error canceling reservation:', error);
      setErrorMessage('Failed to cancel reservation.');
    }
  };

  if (loading) {
    return <div className="text-center mt-5">Loading...</div>;
  }

  if (!user) {
    return (
      <div className="container mt-5">
        {successMessage && <div className="alert alert-success">{successMessage}</div>}
        <div className="alert alert-warning text-center">
          You are not logged in. Please log in to view your account.
        </div>
      </div>
    );
  }

  return (
    <div className="container mt-5">
      <h2 className="text-center mb-4">My Account</h2>

      {successMessage && <div className="alert alert-success">{successMessage}</div>}
      {errorMessage && <div className="alert alert-danger">{errorMessage}</div>}

      <div className="card mb-4 shadow-sm">
        <div className="card-header bg-info text-white">
          <h4>Your Information</h4>
        </div>
        <div className="card-body">
          <p>
            <strong>Name:</strong> {user.firstName} {user.lastName}
          </p>
          <p>
            <strong>Email:</strong> {user.email}
          </p>
          <p>
            <strong>Phone Number:</strong> {user.phoneNumber || 'Not provided'}
          </p>
        </div>
      </div>

      <div className="mb-4">
        <h4>Your Reservations</h4>
        {(user.reservations?.$values?.length || 0) > 0 ? (
          <ul className="list-group">
            {user.reservations.$values.map((reservation) => (
              <li
                key={reservation.reservationId}
                className="list-group-item d-flex justify-content-between align-items-center shadow-sm"
              >
                <span>
                  <strong>{reservation.book.name}</strong> - Reserved until{' '}
                  {new Date(reservation.reservationEndDate).toLocaleDateString()}
                </span>
                <button
                  onClick={() => handleCancelReservation(reservation.reservationId)}
                  className="btn btn-warning btn-sm shadow-sm"
                >
                  Cancel
                </button>
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-muted">You have no reservations.</p>
        )}
      </div>

      <div>
        <h4>Your Leases</h4>
        {(user.leases?.$values?.length || 0) > 0 ? (
          <ul className="list-group">
            {user.leases.$values.map((lease) => (
              <li key={lease.leaseId} className="list-group-item shadow-sm">
                <strong>{lease.book.name}</strong> - Leased since{' '}
                {new Date(lease.leaseStartDate).toLocaleDateString()}{' '}
                {lease.leaseEndDate ? (
                  <span className="text-muted">
                    - Ended on {new Date(lease.leaseEndDate).toLocaleDateString()}
                  </span>
                ) : (
                  <span className="badge bg-success">Lease is active</span>
                )}
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-muted">You have no leases.</p>
        )}
      </div>

      <button
        onClick={handleDeleteAccount}
        className="btn btn-danger btn-lg w-100 shadow-sm mb-4 p-50"
      >
        Delete My Account
      </button>

    </div>
  );
};

export default MyAccount;
