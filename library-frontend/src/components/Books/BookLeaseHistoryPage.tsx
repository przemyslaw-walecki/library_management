import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { fetchBookWithLeases, BookWithLeases } from '../../services/api';


const BookLeaseHistoryPage: React.FC = () => {
  const [book, setBook] = useState<BookWithLeases | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchBookData = async () => {
      try {
        const data = await fetchBookWithLeases(parseInt(id!));
        setBook(data);
        setLoading(false);
      } catch (err) {
        setError('Error while fetching Book data.');
        setLoading(false);
      }
    };

    fetchBookData();
  }, [id]);

 const role = localStorage.getItem('role');
    if (!role || role !== 'Librarian') {
      navigate('/list-books');
    }

  if (loading) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>{error}</div>;
  }

  return (
    <div className="container">
      <h1>{book?.name}</h1>
      <p><strong>Author:</strong> {book?.author}</p>
      <p><strong>Publisher:</strong> {book?.publisher}</p>
      <p><strong>Published on:</strong> {book?.dateOfPublication}</p>
      <p><strong>Price:</strong> ${book?.price}</p>

      <h3>Leases</h3>
      {book?.leases.$values.length === 0 ? (
        <p>No leases available for this book.</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th scope="col">Lease Start Date</th>
              <th scope="col">Lease End Date</th>
              <th scope="col">User</th>
            </tr>
          </thead>
          <tbody>
            {book?.leases.$values.map((lease, index) => (
              <tr key={index}>
                <td>{new Date(lease.leaseStartDate).toLocaleDateString()}</td>
                <td>{new Date(lease.leaseEndDate).toLocaleDateString()}</td>
                <td>
                  <p>{lease.user.firstName} {lease.user.lastName}</p>
                  <p>{lease.user.email}</p>
                  {lease.user.phoneNumber && <p>{lease.user.phoneNumber}</p>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default BookLeaseHistoryPage;
