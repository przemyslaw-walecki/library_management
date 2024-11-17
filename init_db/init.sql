--
-- PostgreSQL database dump
--

-- Dumped from database version 16.3 (Debian 16.3-1.pgdg120+1)
-- Dumped by pg_dump version 16.3

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: books; Type: TABLE; Schema: public; Owner: library_user
--

CREATE TABLE public.books (
    book_id integer NOT NULL,
    name character varying(100) NOT NULL,
    author character varying(100) NOT NULL,
    publisher character varying(100),
    date_of_publication date,
    price numeric(10,2),
    is_permanently_unavailable boolean DEFAULT false
);


ALTER TABLE public.books OWNER TO library_user;

--
-- Name: books_book_id_seq; Type: SEQUENCE; Schema: public; Owner: library_user
--

CREATE SEQUENCE public.books_book_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.books_book_id_seq OWNER TO library_user;

--
-- Name: books_book_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: library_user
--

ALTER SEQUENCE public.books_book_id_seq OWNED BY public.books.book_id;


--
-- Name: leases; Type: TABLE; Schema: public; Owner: library_user
--

CREATE TABLE public.leases (
    lease_id integer NOT NULL,
    user_id integer NOT NULL,
    book_id integer NOT NULL,
    lease_start_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    lease_end_date timestamp without time zone,
    is_active boolean DEFAULT true
);


ALTER TABLE public.leases OWNER TO library_user;

--
-- Name: leases_lease_id_seq; Type: SEQUENCE; Schema: public; Owner: library_user
--

CREATE SEQUENCE public.leases_lease_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.leases_lease_id_seq OWNER TO library_user;

--
-- Name: leases_lease_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: library_user
--

ALTER SEQUENCE public.leases_lease_id_seq OWNED BY public.leases.lease_id;


--
-- Name: reservations; Type: TABLE; Schema: public; Owner: library_user
--

CREATE TABLE public.reservations (
    reservation_id integer NOT NULL,
    user_id integer NOT NULL,
    book_id integer NOT NULL,
    reservation_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    reservation_expiry timestamp without time zone,
    is_active boolean DEFAULT true
);


ALTER TABLE public.reservations OWNER TO library_user;

--
-- Name: reservations_reservation_id_seq; Type: SEQUENCE; Schema: public; Owner: library_user
--

CREATE SEQUENCE public.reservations_reservation_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.reservations_reservation_id_seq OWNER TO library_user;

--
-- Name: reservations_reservation_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: library_user
--

ALTER SEQUENCE public.reservations_reservation_id_seq OWNED BY public.reservations.reservation_id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: library_user
--

CREATE TABLE public.users (
    id integer NOT NULL,
    username character varying(50) NOT NULL,
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    email character varying(100),
    phone_number character varying(20),
    password character varying(255) NOT NULL,
    is_librarian boolean DEFAULT false
);


ALTER TABLE public.users OWNER TO library_user;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: library_user
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO library_user;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: library_user
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: books book_id; Type: DEFAULT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.books ALTER COLUMN book_id SET DEFAULT nextval('public.books_book_id_seq'::regclass);


--
-- Name: leases lease_id; Type: DEFAULT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.leases ALTER COLUMN lease_id SET DEFAULT nextval('public.leases_lease_id_seq'::regclass);


--
-- Name: reservations reservation_id; Type: DEFAULT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.reservations ALTER COLUMN reservation_id SET DEFAULT nextval('public.reservations_reservation_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Data for Name: books; Type: TABLE DATA; Schema: public; Owner: library_user
--

INSERT INTO public.books VALUES (1525, 'To Kill a Mockingbird', 'Harper Lee', 'J.B. Lippincott & Co.', '1960-07-11', 39.99, false);
INSERT INTO public.books VALUES (2516, '1984', 'George Orwell', 'Secker & Warburg', '1949-06-08', 29.99, false);
INSERT INTO public.books VALUES (31256, 'The Great Gatsby', 'F. Scott Fitzgerald', 'Charles Scribners Sons', '1925-04-10', 24.99, false);
INSERT INTO public.books VALUES (4312, 'Pride and Prejudice', 'Jane Austen', NULL, '1813-01-28', 19.99, false);
INSERT INTO public.books VALUES (556, 'Moby Dick', 'Herman Melville', 'Harper & Brothers', '1851-10-18', 34.99, false);
INSERT INTO public.books VALUES (621, 'War and Peace', 'Leo Tolstoy', 'The Russian Messenger', '1869-01-01', 49.99, false);
INSERT INTO public.books VALUES (767, 'The Catcher in the Rye', 'J.D. Salinger', 'Little, Brown and Company', '1951-07-16', 22.99, false);
INSERT INTO public.books VALUES (8125, 'The Hobbit', 'J.R.R. Tolkien', 'George Allen & Unwin', '1937-09-21', 25.99, false);
INSERT INTO public.books VALUES (978, 'Fahrenheit 451', 'Ray Bradbury', 'Ballantine Books', '1953-10-19', 21.99, false);
INSERT INTO public.books VALUES (1012, 'Jane Eyre', 'Charlotte Bronte', 'Smith, Elder & Co.', '1847-10-16', 26.99, false);
INSERT INTO public.books VALUES (1175, 'Animal Farm', 'George Orwell', 'Secker & Warburg', '1945-08-17', 18.99, false);
INSERT INTO public.books VALUES (1215, 'The Adventures of Huckleberry Finn', 'Mark Twain', 'Charles L. Webster And Company', '1885-12-10', 27.99, false);
INSERT INTO public.books VALUES (1353, 'The Odyssey', 'Homer', NULL, '0800-01-01', 39.99, false);
INSERT INTO public.books VALUES (1467, 'Crime and Punishment', 'Fyodor Dostoevsky', 'The Russian Messenger', '1866-01-01', 31.99, false);
INSERT INTO public.books VALUES (1541, 'The Brothers Karamazov', 'Fyodor Dostoevsky', 'The Russian Messenger', '1880-01-01', 36.99, false);
INSERT INTO public.books VALUES (1612, 'Wuthering Heights', 'Emily Bronte', 'Thomas Cautley Newby', '1847-12-01', 29.99, false);
INSERT INTO public.books VALUES (17125, 'A Tale of Two Cities', 'Charles Dickens', 'Chapman & Hall', '1859-04-30', 23.99, false);
INSERT INTO public.books VALUES (1815, 'The Picture of Dorian Gray', 'Oscar Wilde', 'Ward, Lock & Company', '1890-06-20', 28.99, false);
INSERT INTO public.books VALUES (1912, 'Les Misérables', 'Victor Hugo', 'A. Lacroix, Verboeckhoven & Cie.', '1862-01-01', 42.99, false);
INSERT INTO public.books VALUES (20126, 'Don Quixote', 'Miguel de Cervantes', NULL, '1605-01-01', 33.99, false);
INSERT INTO public.books VALUES (12683, 'The Lord of the Rings: The Fellowship of the Ring', 'J.R.R. Tolkien', 'George Allen & Unwin', '1954-07-03', 14.99, false);
INSERT INTO public.books VALUES (7124, 'The Hobbit', 'J.R.R. Tolkien', 'George Allen & Unwin', '1937-08-31', 11.99, false);


--
-- Data for Name: leases; Type: TABLE DATA; Schema: public; Owner: library_user
--



--
-- Data for Name: reservations; Type: TABLE DATA; Schema: public; Owner: library_user
--



--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: library_user
--

INSERT INTO public.users VALUES (6, 'test', 'Przemysław', 'Walecki', 'p.walecki12@gmail.com', '', 'n4bQgYhMfWWaL+qgxVrQFaO/TxsrC4Is0V1sFbDwCgg=', false);
INSERT INTO public.users VALUES (7, 'librarian_admin', 'Przemysław', 'Walecki', 'p.walecki12@gmail.com', '', 'oFpJiaS4zjK9hVhq5B1bHUiGScNHrgUqLEwaQByPfrg=', true);


--
-- Name: books_book_id_seq; Type: SEQUENCE SET; Schema: public; Owner: library_user
--

SELECT pg_catalog.setval('public.books_book_id_seq', 10, true);


--
-- Name: leases_lease_id_seq; Type: SEQUENCE SET; Schema: public; Owner: library_user
--

SELECT pg_catalog.setval('public.leases_lease_id_seq', 3, true);


--
-- Name: reservations_reservation_id_seq; Type: SEQUENCE SET; Schema: public; Owner: library_user
--

SELECT pg_catalog.setval('public.reservations_reservation_id_seq', 21, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: library_user
--

SELECT pg_catalog.setval('public.users_id_seq', 9, true);


--
-- Name: books books_pkey; Type: CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.books
    ADD CONSTRAINT books_pkey PRIMARY KEY (book_id);


--
-- Name: leases leases_pkey; Type: CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.leases
    ADD CONSTRAINT leases_pkey PRIMARY KEY (lease_id);


--
-- Name: reservations reservations_pkey; Type: CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.reservations
    ADD CONSTRAINT reservations_pkey PRIMARY KEY (reservation_id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: leases leases_book_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.leases
    ADD CONSTRAINT leases_book_id_fkey FOREIGN KEY (book_id) REFERENCES public.books(book_id);


--
-- Name: leases leases_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.leases
    ADD CONSTRAINT leases_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: reservations reservations_book_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.reservations
    ADD CONSTRAINT reservations_book_id_fkey FOREIGN KEY (book_id) REFERENCES public.books(book_id);


--
-- Name: reservations reservations_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: library_user
--

ALTER TABLE ONLY public.reservations
    ADD CONSTRAINT reservations_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- PostgreSQL database dump complete
--

