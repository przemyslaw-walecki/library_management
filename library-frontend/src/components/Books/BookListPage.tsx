import React, { useState, useEffect } from 'react';
import { fetchBooks, reserveBook } from '../../services/api';

const BookListPage: React.FC = () => {
  const [books, setBooks] = useState<any[]>([]);
  const [searchString, setSearchString] = useState('');
  const [message, setMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    const loadBooks = async () => {
      try {
        const data = await fetchBooks(searchString);
        setBooks(data.$values);
      } catch (error) {
        console.error('Failed to fetch books', error);
      }
    };

    loadBooks();
  }, [searchString]);

  const handleReserve = async (bookId: number) => {
    try {
      await reserveBook(bookId);
      setMessage('Book reserved successfully!');
      setErrorMessage(null);

      const data = await fetchBooks(searchString);
      setBooks(data.$values);
    } catch (error) {
      console.error('Failed to reserve book', error);
      setMessage(null);
      alert(error instanceof Error ? error.message : 'An error occurred');
    }
  };

  return (
    <div className="container mt-5">
      <h1 className="text-center mb-4">Book List</h1>

      {message && <div className="alert alert-success">{message}</div>}
      {errorMessage && <div className="alert alert-danger">{errorMessage}</div>}

      <div className="input-group mb-4">
        <input
          type="text"
          className="form-control"
          placeholder="Search by author, publisher, or name"
          value={searchString}
          onChange={(e) => setSearchString(e.target.value)}
        />
      </div>

      <div className="list-group">
        {books.map((book) => (
          <div
            className="list-group-item d-flex justify-content-between align-items-start"
            key={book.bookId}
          >
            <div>
              <h4 className="mb-1">{book.name}</h4>
              <p className="mb-1"><strong>Author:</strong> {book.author}</p>
              <p className="mb-1"><strong>Publisher:</strong> {book.publisher}</p>
              <p className="mb-1">
                <strong>Date:</strong> {new Date(book.dateOfPublication).toLocaleDateString()}
              </p>
              <p className="mb-1"><strong>Price:</strong> ${book.price}</p>
              {book.isReserved && (
                <span className="badge bg-danger mt-2">Reserved</span>
              )}
              {book.isLeased && (
                <span className="badge bg-warning mt-2">Leased</span>
              )}
            </div>
            {!book.isReserved && !book.isLeased && (
              <button
                className="btn btn-success align-self-center"
                onClick={() => handleReserve(book.bookId)}
              >
                Reserve
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default BookListPage;
