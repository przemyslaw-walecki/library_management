import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar/Navbar';
import LoginPage from './components/Auth/LoginPage';
import BookListPage from './components/Books/BookListPage';

const App: React.FC = () => {
  return (
    <Router>
      <Navbar />
      <Routes>
        <Route path="/login" element={<LoginPage/>} />
        <Route path="/books" element={<BookListPage/>} />
        <Route path="/"> </Route>
      </Routes>
    </Router>
  );
};

export default App;
