// LeasesPage.tsx

import React, { useEffect, useState } from 'react';
import { getLeases } from '../../services/api';
import { useNavigate } from 'react-router-dom';

const LeasesPage: React.FC = () => {
  const [leases, setLeases] = useState<any[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchLeases = async () => {
      try {
        const data = await getLeases();
        setLeases(data.$values);
      } catch (error) {
        console.error('Error fetching leases:', error);
      }
    };

    fetchLeases();
  }, []);

  const handleEndLease = (leaseId: number) => {
    navigate(`/leases/end/${leaseId}`);
  };

  return (
    <div className="container">
      <h1>Active Leases</h1>
      <table className="table">
        <thead>
          <tr>
            <th>Lease Start Date</th>
            <th>Lease End Date</th>
            <th>Book</th>
            <th>User</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {leases.map((lease) => (
            <tr key={lease.leaseId}>
              <td>{lease.leaseStartDate}</td>
              <td>{lease.leaseEndDate}</td>
              <td>{lease.book.name}</td>
              <td>{lease.user.firstName}</td>
              <td>
                <button onClick={() => navigate(`/leases/details/${lease.leaseId}`)} className="btn btn-secondary ms-2">
                  Details
                </button>
                <button onClick={() => handleEndLease(lease.leaseId)} className="btn btn-danger ms-2">
                  End Lease
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default LeasesPage;
