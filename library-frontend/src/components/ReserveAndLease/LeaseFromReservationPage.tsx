// LeaseFromReservationPage.tsx

import React, { useEffect, useState } from 'react';
import { getReservationById, leaseBookFromReservation } from '../../services/api';
import { useNavigate, useParams } from 'react-router-dom';

const LeaseFromReservationPage: React.FC = () => {
  const [reservation, setReservation] = useState<any | null>(null);
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchReservationDetails = async () => {
      try {
        const data = await getReservationById(parseInt(id!));
        setReservation(data);
      } catch (error) {
        console.error('Error fetching reservation details:', error);
      }
    };

    fetchReservationDetails();
  }, [id]);

  if (localStorage.getItem('role') !== 'Librarian') {
    navigate('/list-books');
  }

  if (!reservation) {
    return <div>Loading...</div>;
  }

  const handleConfirmLease = async () => {
    try {
      await leaseBookFromReservation(reservation.reservationId);
      navigate('/reservations');
    } catch (error) {
      console.error('Error leasing book:', error);
    }
  };

  return (
    <div className="container">
      <h3>Are you sure you want to lease a book based on this reservation?</h3>
      <div>
        <h4>Reservation Details</h4>
        <hr />
        <dl className="row">
          <dt className="col-sm-2">Reservation End Date</dt>
          <dd className="col-sm-10">{reservation.reservationEndDate}</dd>

          <dt className="col-sm-2">Book</dt>
          <dd className="col-sm-10">{reservation.book.name}</dd>

          <dt className="col-sm-2">User</dt>
          <dd className="col-sm-10">{reservation.user.username}</dd>
        </dl>

        <form onSubmit={(e) => e.preventDefault()}>
          <input type="hidden" value={reservation.reservationId} />
          <button onClick={handleConfirmLease} className="btn btn-success">
            Confirm Lease
          </button>
          <button onClick={() => navigate('/reservations')} className="btn btn-secondary ms-2">
            Cancel
          </button>
        </form>
      </div>
    </div>
  );
};

export default LeaseFromReservationPage;
