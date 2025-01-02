import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { fetchBookById, saveBook, Book } from '../../services/api';

const BookForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [book, setBook] = useState<Book>({
    name: '',
    author: '',
    publisher: '',
    dateOfPublication: '',
    price: 0,
  });
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (localStorage.getItem('role') !== 'Librarian') {
      navigate('/books');
    }
  }, [navigate]);

  useEffect(() => {
    if (id) {
      const getBook = async () => {
        try {
          const fetchedBook = await fetchBookById(id);
          setBook(fetchedBook);
        } catch (err) {
          setError('Failed to fetch book details');
        }
      };
      getBook();
    }
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setBook((prevBook) => ({
      ...prevBook,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await saveBook(book);
      navigate('/manage-books');
    } catch (err) {
      setError('Failed to save book');
    } finally {
      setLoading(false);
    }
  };

  if (localStorage.getItem('role') !== 'Librarian') {
    navigate('/list-books');
  }

  return (
    <div className="container mt-5">
      <h1 className="text-center mb-4">{id ? 'Edit Book' : 'Add New Book'}</h1>

      {error && <div className="alert alert-danger">{error}</div>}

      <form onSubmit={handleSubmit} className="p-4 border rounded bg-light shadow-sm">
        <div className="form-group mb-3">
          <label htmlFor="name" className="form-label">Name</label>
          <input
            type="text"
            className="form-control"
            id="name"
            name="name"
            value={book.name}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group mb-3">
          <label htmlFor="author" className="form-label">Author</label>
          <input
            type="text"
            className="form-control"
            id="author"
            name="author"
            value={book.author}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group mb-3">
          <label htmlFor="publisher" className="form-label">Publisher</label>
          <input
            type="text"
            className="form-control"
            id="publisher"
            name="publisher"
            value={book.publisher}
            onChange={handleChange}
          />
        </div>

        <div className="form-group mb-3">
          <label htmlFor="dateOfPublication" className="form-label">Date of Publication</label>
          <input
            type="date"
            className="form-control"
            id="dateOfPublication"
            name="dateOfPublication"
            value={book.dateOfPublication}
            onChange={handleChange}
          />
        </div>

        <div className="form-group mb-4">
          <label htmlFor="price" className="form-label">Price</label>
          <input
            type="number"
            className="form-control"
            id="price"
            name="price"
            value={book.price}
            onChange={handleChange}
            required
          />
        </div>

        <button type="submit" className="btn btn-primary w-100" disabled={loading}>
          {loading ? 'Saving...' : 'Save'}
        </button>
      </form>
    </div>
  );
};

export default BookForm;
