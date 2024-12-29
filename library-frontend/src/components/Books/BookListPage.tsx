import React, { useState, useEffect } from 'react';
import { fetchBooks } from '../../services/api';

const BookListPage: React.FC = () => {
  const [books, setBooks] = useState<any[]>([]);
  const [searchString, setSearchString] = useState('');
  const [message, setMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const token = true;

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

          {token ? (
            book.isReserved ? (
              <span className="text-danger">Reserved</span>
            ) : book.isLeased ? (
              <span className="text-danger">Leased</span>
            ) : (
              <form method="post" action="/api/books/reserve">
                <input type="hidden" name="bookId" value={book.bookId.toString()} />
                <button type="submit" className="btn btn-success">
                  Reserve
                </button>
              </form>
            )
          ) : (
            <p>Login to reserve this book.</p>
          )}
        </div>
      ))}
    </div>
  );
};


export default BookListPage;
