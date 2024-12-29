import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar/Navbar';
import LoginPage from './components/Account/LoginPage';
import BookListPage from './components/Books/BookListPage';
import MyAccount from './components/Account/MyAccount';
import ManageBooks from './components/Books/ManageBooks';
import BookForm from './components/Books/BookForm';

const App: React.FC = () => {
  return (
    <Router>
      <Navbar />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/books" element={<BookListPage />} />
        <Route path="/myaccount" element={<MyAccount />} />
        <Route path="/manage-books" element={<ManageBooks />} />
        <Route path="/edit-book/:id" element={<BookForm />} />
        <Route path="/add-book" element={<BookForm />} />
        <Route path="/" element={<BookListPage />} />
      </Routes>
    </Router>
  );
};

export default App;
