import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchManageBooks, deleteBook } from '../../services/api';

const ManageBooks: React.FC = () => {
  const [books, setBooks] = useState<any[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const getBooks = async () => {
      try {
        const data = await fetchManageBooks();
        setBooks(data.$values);
      } catch (err) {
        setError('Failed to load books');
      } finally {
        setLoading(false);
      }
    };

    getBooks();
  }, []);

  useEffect(() => {
    if (localStorage.getItem('role') !== 'Librarian') {
      navigate('/books');
    }
  }, [navigate]);

  const handleDelete = async (bookId: number) => {
    const confirmDelete = window.confirm('Are you sure you want to delete this book?');
    if (!confirmDelete) return;

    try {
      await deleteBook(bookId);
      setBooks((prevBooks) => prevBooks.filter((book) => book.bookId !== bookId));
    } catch (err) {
      setError('Failed to delete book');
    }
  };

  const role = localStorage.getItem('role');
  if (!role || role !== 'Librarian') {
    navigate('/list-books');
  }

  if (loading) return <div className="text-center mt-5">Loading...</div>;
  if (error) return <div className="alert alert-danger">{error}</div>;

  return (
    <div className="container mt-5">
      <h1 className="mb-4 text-center">Manage Books</h1>
      <div className="d-flex justify-content-end mb-3">
        <button
          onClick={() => navigate('/add-book')}
          className="btn btn-primary"
        >
          Add Book
        </button>
      </div>
      <div className="list-group">
        {books.map((book) => (
          <div
            className="list-group-item d-flex justify-content-between align-items-center"
            key={book.bookId}
          >
            <div>
              <h4 className="mb-1">{book.name}</h4>
              <p className="mb-0"><strong>Author:</strong> {book.author}</p>
              <p className="mb-0"><strong>Publisher:</strong> {book.publisher}</p>
              <p className="mb-0"><strong>Date:</strong> {new Date(book.dateOfPublication).toLocaleDateString()}</p>
              <p className="mb-0"><strong>Price:</strong> ${book.price}</p>
              {book.isPermanentlyUnavailable && (
                <p className="text-danger mt-2">This book is marked as permanently unavailable.</p>
              )}
            </div>
            {!book.isPermanentlyUnavailable ? (
              <div className="d-flex flex-column align-items-end">
                <button
                  onClick={() => navigate(`/edit-book/${book.bookId}`)}
                  className="btn btn-warning btn-sm mb-2 w-100"
                >
                  Edit
                </button>
                <button
                  onClick={() => handleDelete(book.bookId)}
                  className="btn btn-danger btn-sm mb-2 w-100"
                >
                  Delete
                  </button>
                  <button
                  onClick={() => navigate(`/manage-books/history/${book.bookId}`)}
                  className="btn btn-success btn-sm mb-2 w-100"
                >History</button>
                  
                
              </div>
            ) : (
              <button
                  onClick={() => navigate(`/manage-books/history/${book.bookId}`)}
                  className="btn btn-success btn-sm mb-2 w-10"
                >History</button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default ManageBooks;
