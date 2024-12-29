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
    <div>
            {message && (
        <div className="alert alert-success">
            {message}
        </div>
        )}

        {errorMessage && (
        <div className="alert alert-danger">
            {errorMessage}
        </div>
        )}

      <div className="input-group mb-3">
        <input
          type="text"
          className="form-control"
          placeholder="Search by author, publisher, or name"
          value={searchString}
          onChange={(e) => setSearchString(e.target.value)}
        />
      </div>

      {books.map((book) => (
        <div className="book-item" key={book.bookId}>
          <h4>{book.name}</h4>
          <p>{book.author}</p>
          <p>{book.publisher}</p>
          <p>{new Date(book.dateOfPublication).toLocaleDateString()}</p>
          <p>{book.price}</p>

          {book.isReserved ? (
            <span className="text-danger">Reserved</span>
          ) : book.isLeased ? (
            <span className="text-danger">Leased</span>
          ) : (
            <button
              className="btn btn-success"
              onClick={() => handleReserve(book.bookId)}
            >
              Reserve
            </button>
          )}
        </div>
      ))}
    </div>
  );
};


export default BookListPage;
