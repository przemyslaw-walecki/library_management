// EndLeasePage.tsx

import React, { useEffect, useState } from 'react';
import { getLeaseById, endLease } from '../../services/api';
import { useNavigate, useParams } from 'react-router-dom';

const EndLeasePage: React.FC = () => {
  const [lease, setLease] = useState<any | null>(null);
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchLeaseDetails = async () => {
      try {
        const data = await getLeaseById(parseInt(id!));
        setLease(data);
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
    return <div>Loading...</div>;
  }

  const handleEndLease = async () => {
    try {
      await endLease(lease.leaseId);
      navigate('/leases'); 
    } catch (error) {
      console.error('Error ending lease:', error);
    }
  };

  return (
    <div className="container">
      <h3>Are you sure you want to end this lease?</h3>
      <div>
        <h4>Lease Details</h4>
        <hr />
        <dl className="row">
          <dt className="col-sm-2">Lease Start Date</dt>
          <dd className="col-sm-10">{lease.leaseStartDate}</dd>

          <dt className="col-sm-2">Book</dt>
          <dd className="col-sm-10">{lease.book.name}</dd>

          <dt className="col-sm-2">User</dt>
          <dd className="col-sm-10">{lease.user.username}</dd>
        </dl>

        <form onSubmit={(e) => e.preventDefault()}>
          <input type="hidden" value={lease.leaseId} />
          <button onClick={handleEndLease} className="btn btn-danger">
            End Lease
          </button>
          <button onClick={() => navigate('/leases')} className="btn btn-secondary ms-2">
            Cancel
          </button>
        </form>
      </div>
    </div>
  );
};

export default EndLeasePage;
