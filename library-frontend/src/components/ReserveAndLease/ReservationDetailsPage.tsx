import React, { useEffect, useState } from 'react';
import { getReservationById } from '../../services/api';
import { useParams, useNavigate } from 'react-router-dom';

const ReservationDetailsPage: React.FC = () => {
  const [reservation, setReservation] = useState<any>(null);
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchReservationDetails = async () => {
      try {
        const data = await getReservationById(parseInt(id!));
        setReservation(data);
      } catch (error) {
        console.error('Error fetching Reservation details:', error);
      }
    };

    fetchReservationDetails();
  }, [id]);

  if (!reservation) {
    return (<div>Loading...</div>);
  }

  return (
    <div className="container">
      <h1>Reservation Details</h1>
      <dl className="row">
        <dt className="col-sm-2">Reservation End Date</dt>
        <dd className="col-sm-10"> {reservation.reservationEndDate}</dd>

        <dt className="col-sm-2">Book</dt>
        <dd className="col-sm-10">
          {reservation.book.name} <br />
          {reservation.book.author} <br />
          {reservation.book.publisher}
        </dd>

        <dt className="col-sm-2">User</dt>
        <dd className="col-sm-10">{reservation.user.username}<br/>
        {reservation.user.firstName}<br/>
        {reservation.user.lastName}<br/>
        {reservation.user.email}<br/>
        </dd>

      </dl>
      <button onClick={() => navigate('/reservations')} className="btn btn-primary">
        Back to List
      </button>
    </div>
  );
};

export default ReservationDetailsPage;
