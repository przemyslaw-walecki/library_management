CREATE TABLE IF NOT EXISTS users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    phone_number VARCHAR(20),
    password VARCHAR(255) NOT NULL
);

CREATE TABLE IF NOT EXISTS books (
    book_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    author VARCHAR(100) NOT NULL,
    publisher VARCHAR(100),
    date_of_publication DATE,
    price DECIMAL(10, 2),
    is_permanently_unavailable BOOLEAN DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS leases (
    lease_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id),
    book_id INT NOT NULL REFERENCES books(book_id),
    lease_start_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    lease_end_date TIMESTAMP
);

CREATE TABLE IF NOT EXISTS reservations (
    reservation_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id),
    book_id INT NOT NULL REFERENCES books(book_id),
    reservation_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    reservation_expiry TIMESTAMP
);
