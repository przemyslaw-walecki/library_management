// LeaseDetailsPage.tsx

import React, { useEffect, useState } from 'react';
import { getLeaseById } from '../../services/api';
import { useParams, useNavigate } from 'react-router-dom';

const LeaseDetailsPage: React.FC = () => {
  const [lease, setLease] = useState<any>(null);
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchLeaseDetails = async () => {
      try {
        const data = await getLeaseById(parseInt(id!));
        setLease(data.$values);
      } catch (error) {
        console.error('Error fetching lease details:', error);
      }
    };

    fetchLeaseDetails();
  }, [id]);

  if (localStorage.getItem('role') !== 'Librarian') {
    navigate('/list-books');
  }

  if (!lease) {
    return (<div>Loading...</div>);
  }

  return (
    <div className="container">
      <h1>Lease Details</h1>
      <dl className="row">
        <dt className="col-sm-2">Lease Start Date</dt>
        <dd className="col-sm-10">{lease.leaseStartDate}</dd>

        <dt className="col-sm-2">Lease End Date</dt>
        <dd className="col-sm-10">{lease.leaseEndDate}</dd>

        <dt className="col-sm-2">Book</dt>
        <dd className="col-sm-10">
          {lease.book.name} <br />
          {lease.book.author} <br />
          {lease.book.publisher}
        </dd>

        <dt className="col-sm-2">User</dt>
        <dd className="col-sm-10">{lease.user.username}</dd>
      </dl>
      <button onClick={() => navigate('/leases')} className="btn btn-primary">
        Back to List
      </button>
    </div>
  );
};

export default LeaseDetailsPage;
