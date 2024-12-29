// src/index.tsx
import React from 'react';
import ReactDOM from 'react-dom/client'; // React 18 uses this for creating the root
import './index.css';
import App from './App';

const root = ReactDOM.createRoot(document.getElementById('root')!);
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
