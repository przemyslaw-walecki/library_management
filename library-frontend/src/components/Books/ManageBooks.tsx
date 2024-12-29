import React, { useEffect, useState} from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchManageBooks, deleteBook } from '../../services/api';



const ManageBooks: React.FC = () => {
  const [books, setBooks] = useState<any[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const role = localStorage.getItem('role');
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

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  

  return (
    <div>
      <h1>Manage Books</h1>
      <button onClick={() => navigate('/add-book')}>Add Book</button>
      <ul>
        {books.map((book) => (
          <div className="book-item" key={book.bookId}>
            <h4>{book.name}</h4>
            <p>{book.author}</p>
            <p>{book.publisher}</p>
            <p>{new Date(book.dateOfPublication).toLocaleDateString()}</p>
            <p>{book.price}</p>
            {book.is_permanently_available ? (
              <p>This book is marked as permanently unavailable.</p>
            ) : (
              <div>
                <button onClick={() => navigate(`/edit-book/${book.bookId}`)}>Edit</button>
                <button onClick={() => handleDelete(book.bookId)}>Delete</button>
              </div>
            )}
          </div>
        ))}
      </ul>
    </div>
  );
};

export default ManageBooks;
