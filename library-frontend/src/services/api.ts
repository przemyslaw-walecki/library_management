import jwtDecode from 'jwt-decode';

const API_URL = 'http://localhost:7276/api';

export interface Book {
    bookId?: number;
    name: string;
    author: string;
    publisher: string;
    dateOfPublication: string;
    price: number;
    isPermanentlyUnavailable?: boolean;
  }
  
  interface BooksResponse {
    $id: string;
    $values: Book[];
  }

  interface LoginResponse {
    token: string;
  }
  
  interface DecodedToken {
    name: string;
    nameid: string; 
    role: 'Librarian' | 'User'; 
    exp: number; 
    iat: number;
  }
  
  export const login = async (Username: string, password: string): Promise<void> => {
    try {
      const response = await fetch(`${API_URL}/Account/login?Username=${encodeURIComponent(Username)}&password=${encodeURIComponent(password)}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Username, password }),
        credentials: 'include',
      });
  
      if (!response.ok) throw new Error('Login failed');
  
      const loginResponse: LoginResponse = await response.json();
  
      const decodedToken: DecodedToken = jwtDecode(loginResponse.token);
      localStorage.setItem('role', decodedToken.role);
  
    } catch (error) {
      console.error('Login failed', error);
      throw error;
    }
  };
  

export const logout = async (): Promise<void> => {
  const response = await fetch(`${API_URL}/Account/logout`, {
    method: 'POST',
    credentials: 'include',
  });

  if (!response.ok) throw new Error('Logout failed');
  localStorage.removeItem('role')
};

export const fetchBooks = async (searchString: string = ''): Promise<BooksResponse> => {
  const response = await fetch(`${API_URL}/Books?searchString=${searchString}`, {
    method: 'GET',
    credentials: 'include',
  });

  if (!response.ok) throw new Error('Failed to fetch books');
  return response.json();
};

export const reserveBook = async (bookId: number): Promise<void> => {
  const response = await fetch(`${API_URL}/Books/reserve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(bookId),
    credentials: 'include',
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("You must be logged in to reserve a book.");
    }
    const errorText = await response.text();
    throw new Error(errorText || 'Failed to reserve book');
  }
};

interface DecodedToken {
  name: string;
  nameid: string;
  role: 'Librarian' | 'User';
  exp: number;
  iat: number;
}

export interface Reservation {
  reservationId: number;
  book: Book;
  reservationEndDate: string;
}

export interface Lease {
  leaseId: number;
  book: Book;
  leaseStartDate: string;
  leaseEndDate?: string | null;
}

interface Collection<T> {
  $values: T[];
}

export interface User {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  reservations: Collection<Reservation>;
  leases: Collection<Lease>;
}

export const fetchUserAccount = async (): Promise<User> => {
  const response = await fetch(`${API_URL}/Account/myaccount`, {
    method: 'GET',
    credentials: 'include',
  });

  if (!response.ok) throw new Error('Failed to fetch user account data');
  return response.json();
};

export const deleteAccount = async (): Promise<void> => {
  const response = await fetch(`${API_URL}/Account/DeleteAccount`, {
    method: 'POST',
    credentials: 'include',
  });

  if (!response.ok) throw new Error('Failed to delete account');
};

export const cancelReservation = async (reservationId: number): Promise<void> => {
  const response = await fetch(`${API_URL}/Reservations/${reservationId}`, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
  });

  if (!response.ok) throw new Error('Failed to cancel reservation');
};

export const fetchManageBooks = async (): Promise<BooksResponse> => {
  const response = await fetch(`${API_URL}/books/manage`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
  });

  if (!response.ok) {
    throw new Error('Failed to fetch books');
  }

  return response.json();
};

export const fetchBookById = async (id: string) => {
  const response = await fetch(`${API_URL}/books/${id}`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
  });


  if (!response.ok) {
    throw new Error('Failed to fetch book');
  }

  const book = await response.json();
  return book;
};

export const saveBook = async (book: Book) => {
  const url = book.bookId ? `/books/edit/${book.bookId}` : `/books/add`
  const response = await fetch(`${API_URL}${url}`, {
    method: book.bookId ? 'PUT' : 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(book),
  });

  if (!response.ok) {
    throw new Error('Failed to save book');
  }
};

export const deleteBook = async (bookId: number) => {
  const response = await fetch(`${API_URL}/books/delete/${bookId}`, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
  });

  if (!response.ok) {
    throw new Error('Failed to delete book');
  }
};