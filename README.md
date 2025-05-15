# Employee Feedback Management System

A full-stack web application for managing employee feedback, built with Angular and .NET Core.

## Features

### User Roles
- **Admin**: Full system access, user management, and analytics
- **Manager**: Feedback management and team insights
- **Employee**: Submit and view feedback

### Key Functionalities
1. **Authentication & Authorization**
   - Secure login/register system
   - Role-based access control
   - JWT token authentication

2. **Feedback Management**
   - Submit feedback with categories and sentiment
   - Anonymous feedback option
   - Feedback filtering and sorting
   - Sentiment analysis visualization

3. **Admin Dashboard**
   - User management (CRUD operations)
   - System analytics
   - CSV export functionality
   - Total users and feedback statistics

4. **Manager Dashboard**
   - Feedback overview
   - Sentiment trends
   - Category-wise analysis
   - Team feedback management

## Tech Stack

### Frontend
- Angular 17
- Bootstrap 5
- TypeScript
- RxJS
- Angular Material (for some components)

### Backend
- .NET Core 8
- Entity Framework Core
- SQL Server
- JWT Authentication
- CORS enabled

## Getting Started

### Prerequisites
- Node.js (v18 or higher)
- .NET Core 8 SDK
- SQL Server
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   ```

2. **Backend Setup**
   ```bash
   cd EF.Server
   dotnet restore
   dotnet run
   ```

3. **Frontend Setup**
   ```bash
   cd ef.client
   npm install
   ng serve
   ```

4. **Database Setup**
   - Update connection string in `appsettings.json`
   - Run migrations:
   ```bash
   dotnet ef database update
   ```

### Default Users
- Admin: admin@example.com / admin123
- Manager: manager@example.com / manager123
- Employee: employee@example.com / employee123

## API Endpoints

### Authentication
- POST `/api/auth/register` - Register new user
- POST `/api/auth/login` - User login

### Feedback
- GET `/api/feedback` - Get all feedback
- POST `/api/feedback` - Submit new feedback
- GET `/api/feedback/team` - Get team feedback
- GET `/api/feedback/analytics` - Get feedback analytics

### Admin
- GET `/api/admin/dashboard` - Get dashboard data
- GET `/api/admin/users` - Get all users
- PUT `/api/admin/users/{id}` - Update user
- DELETE `/api/admin/users/{id}` - Delete user

## Project Structure
