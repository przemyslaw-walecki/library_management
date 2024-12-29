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
                ...prevUser.reservations,  // Ensure you maintain other properties in reservations
                $values: prevUser.reservations.$values.filter(
                  (r) => r.reservationId !== reservationId
                ),
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
    return <div>Loading...</div>;
  }

  if (!user) {
    return (
      <div>
        <p>You are not logged in. Please log in to view your account.</p>
      </div>
    );
  }

  return (
    <div>
      <h2>My Account</h2>

      {successMessage && <div className="alert alert-success">{successMessage}</div>}
      {errorMessage && <div className="alert alert-danger">{errorMessage}</div>}

      <div className="user-info">
        <h4>Your Information</h4>
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

        <button onClick={handleDeleteAccount} className="btn btn-danger">
          Delete My Account
        </button>

      <div className="reservations">
        <h4>Your Reservations</h4>
        {user.reservations.$values.length > 0 ? (
          <ul>
            {user.reservations.$values.map((reservation) => (
              <li key={reservation.reservationId}>
                <strong>{reservation.book.name}</strong> - Reserved until{' '}
                {new Date(reservation.reservationEndDate).toLocaleDateString()}
                <button
                  onClick={() => handleCancelReservation(reservation.reservationId)}
                  className="btn btn-warning"
                >
                  Cancel Reservation
                </button>
              </li>
            ))}
          </ul>
        ) : (
          <p>You have no reservations.</p>
        )}
      </div>

      <div className="leases">
        <h4>Your Leases</h4>
        {user.leases.$values.length > 0 ? (
          <ul>
            {user.leases.$values.map((lease) => (
              <li key={lease.leaseId}>
                <strong>{lease.book.name}</strong> - Leased since{' '}
                {new Date(lease.leaseStartDate).toLocaleDateString()}{' '}
                {lease.leaseEndDate ? (
                  <span className="text-muted">
                    - Ended on {new Date(lease.leaseEndDate).toLocaleDateString()}
                  </span>
                ) : (
                  <span className="text-muted">Lease is active</span>
                )}
              </li>
            ))}
          </ul>
        ) : (
          <p>You have no leases.</p>
        )}
      </div>
    </div>
  );
};

export default MyAccount;
