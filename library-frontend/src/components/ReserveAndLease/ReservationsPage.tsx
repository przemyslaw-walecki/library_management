import React, { useEffect, useState } from 'react';
import { getReservations } from '../../services/api';
import { useNavigate } from 'react-router-dom';

const ReservationsPage: React.FC = () => {
  const [reservations, setReservations] = useState<any[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchReservations = async () => {
      try {
        const data = await getReservations();
        setReservations(data.$values);
      } catch (error) {
        console.error('Error fetching reservations:', error);
      }
    };

    fetchReservations();
  }, []);

  const handleLease = (reservationId: number) => {
    navigate(`/leases/lease?reservationId=${reservationId}`);
  };

  return (
    <div className="container">
      <h1>Active Reservations</h1>
      <table className="table">
        <thead>
          <tr>
            <th>Reservation End Date</th>
            <th>Book</th>
            <th>User</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {reservations.map((reservation) => (
            <tr key={reservation.reservationId}>
              <td>{reservation.reservationEndDate}</td>
              <td>{reservation.book.name}</td>
              <td>{reservation.user.username}</td>
              <td>
                <button onClick={() => handleLease(reservation.reservationId)} className="btn btn-primary">
                  Lease
                </button>
                <button onClick={() => navigate(`/reservations/details/${reservation.reservationId}`)} className="btn btn-secondary ms-2">
                  Details
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default ReservationsPage;
