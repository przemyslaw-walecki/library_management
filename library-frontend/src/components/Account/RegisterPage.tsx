import React, { useState } from 'react';
import { unstable_setDevServerHooks, useNavigate } from 'react-router-dom';
import { registerUser, UserRegisterForm, login } from '../../services/api';

const RegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const [user, setUser] = useState<UserRegisterForm>({
    username: '',
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: undefined,
    password: '',
  });
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setUser((prevUser: UserRegisterForm) => ({
      ...prevUser,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(null);

    try {
      await registerUser(user);
      setSuccess('Registration successful');
      await login(user.username, user.password);
      setTimeout(() => {
        navigate('/books');
      }, 500);
    }  catch (err: any) {
        if (err.response && err.response.data && err.response.data.message) {
            setError(err.response.data.message);
          } else {
            setError('An unexpected error occurred. Please try again.');
          }
    }
  };

  return (
    <div>
      <h2>Register</h2>
      {success && <div className="alert alert-success">{success}</div>}
      {error && <div className="alert alert-danger">{error}</div>}
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="Username">Username</label>
          <input
            type="text"
            className="form-control"
            id="Username"
            name="username"
            value={user.username}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="FirstName">First Name</label>
          <input
            type="text"
            className="form-control"
            id="FirstName"
            name="firstName"
            value={user.firstName}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="LastName">Last Name</label>
          <input
            type="text"
            className="form-control"
            id="LastName"
            name="lastName"
            value={user.lastName}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="Email">Email</label>
          <input
            type="email"
            className="form-control"
            id="Email"
            name="email"
            value={user.email}
            onChange={handleChange}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="PhoneNumber">Phone Number</label>
          <input
            type="text"
            className="form-control"
            id="PhoneNumber"
            name="phoneNumber"
            value={user.phoneNumber}
            onChange={handleChange}
          />
        </div>

        <div className="form-group">
          <label htmlFor="Password">Password</label>
          <input
            type="password"
            className="form-control"
            id="Password"
            name="password"
            value={user.password}
            onChange={handleChange}
            required
          />
        </div>

        <button type="submit" className="btn btn-primary">
          Register
        </button>
      </form>
    </div>
  );
};

export default RegisterPage;
