const API_URL = 'http://localhost:7276/api';

interface Book {
    bookId: number;
    name: string;
    author: string;
    publisher: string;
    dateOfPublication: string;
    price: number;
    isPermanentlyUnavailable: boolean;
    leases: any[];
    reservations: any[];
  }
  
  interface BooksResponse {
    $id: string;
    $values: Book[];
  }

interface LoginResponse {
  token: string;
}

// Update function signatures to include types

export const login = async (Username: string, password: string): Promise<LoginResponse> => {
const response = await fetch(`${API_URL}/Account/login?Username=${encodeURIComponent(Username)}&password=${encodeURIComponent(password)}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({Username,password }),
    credentials: 'include', 
  });

  if (!response.ok) throw new Error('Login failed');
  return response.json();
};

export const logout = async (): Promise<void> => {
  const response = await fetch(`${API_URL}/Account/logout`, {
    method: 'POST',
    credentials: 'include',
  });

  if (!response.ok) throw new Error('Logout failed');
};

export const fetchBooks = async (searchString: string = ''): Promise<BooksResponse> => {
  const response = await fetch(`${API_URL}/Books?searchString=${searchString}`, {
    method: 'GET',
    credentials: 'include',
  });

  if (!response.ok) throw new Error('Failed to fetch books');
  return response.json();
};
