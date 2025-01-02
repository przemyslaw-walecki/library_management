import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar/Navbar';
import LoginPage from './components/Account/LoginPage';
import BookListPage from './components/Books/BookListPage';
import MyAccount from './components/Account/MyAccount';
import ManageBooks from './components/Books/ManageBooks';
import BookForm from './components/Books/BookForm';
import RegisterPage from './components/Account/RegisterPage';
import ReservationsPage from './components/ReserveAndLease/ReservationsPage';
import LeasesPage from './components/ReserveAndLease/LeasesPage';
import LeaseDetailsPage from './components/ReserveAndLease/LeaseDetailsPage';




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
        <Route path="/register" element={<RegisterPage/>} />
          <Route path="/reservations" element={<ReservationsPage />} />
          <Route path="/reservations/details/:id" element={<LeaseDetailsPage />} />
          <Route path="/leases" element={<LeasesPage />} />
          <Route path="/leases/details/:id" element={<LeaseDetailsPage />} />
      </Routes>
    </Router>
  );
};

export default App;
