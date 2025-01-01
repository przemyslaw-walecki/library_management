# Project for Library Management System in ASP\.NET MVC

## Running this project:
- #### Database:
    - Run docker-compose:
      - `docker-compose up -d `
    - The databse is initially seeded with example books and two users:
      - > username: test, password: test
        > username: librarian_admin, password: librariran_password (library admin)
- #### ASP\.NET backend:
  - Enter the solution directory
  - Run the dotnet config:
  - ` dotnet restore - restores neccessary NuGet Packages  `
    `dotnet build`
    `dotnet run`
- #### React frontend:
  - Enter library-frontend
  - Run React config:
  - `npm install`
    `npm start`