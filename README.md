# Autism Center Website

A comprehensive digital platform for the Autism Center featuring e-commerce, educational courses, appointment scheduling, and administrative capabilities with bilingual support (Arabic/English).

## Architecture

This project follows Clean Architecture principles with the following layers:

### Backend (.NET 8 Web API)
- **Domain Layer** (`AutismCenter.Domain`): Core business logic and entities
- **Application Layer** (`AutismCenter.Application`): Use cases and application services
- **Infrastructure Layer** (`AutismCenter.Infrastructure`): Data access and external services
- **Presentation Layer** (`AutismCenter.WebApi`): API controllers and HTTP concerns

### Frontend (React TypeScript)
- **React 18+** with TypeScript
- **Vite** for build tooling
- **Tailwind CSS** for styling
- **React Router** for navigation
- **Axios** for API communication

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- PostgreSQL 12+

## Getting Started

### Backend Setup

1. Navigate to the project root directory
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
3. Build the solution:
   ```bash
   dotnet build
   ```
4. Update the connection string in `src/AutismCenter.WebApi/appsettings.json`
5. Run the API:
   ```bash
   dotnet run --project src/AutismCenter.WebApi
   ```

The API will be available at `https://localhost:7000`

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm run dev
   ```

The frontend will be available at `http://localhost:5173`

### Database Setup

#### Option 1: Using Docker Compose (Recommended)
1. Start PostgreSQL and Redis using Docker Compose:
   ```bash
   docker-compose up -d
   ```
   This will start:
   - PostgreSQL on port 5432
   - Redis on port 6379

#### Option 2: Local Installation
1. Ensure PostgreSQL is running locally
2. Create a database named `AutismCenterDb_Dev` for development
3. Update the connection string in `src/AutismCenter.WebApi/appsettings.json` if needed

The application will create the necessary tables on first run using Entity Framework migrations.

## Project Structure

```
├── src/
│   ├── AutismCenter.Domain/          # Domain entities and business logic
│   ├── AutismCenter.Application/     # Application services and use cases
│   ├── AutismCenter.Infrastructure/  # Data access and external services
│   └── AutismCenter.WebApi/         # API controllers and configuration
├── frontend/
│   ├── src/
│   │   ├── components/              # Reusable UI components
│   │   ├── pages/                   # Page components
│   │   ├── services/                # API service layer
│   │   └── types/                   # TypeScript type definitions
│   └── public/                      # Static assets
└── tests/                           # Test projects
```

## Features

- **Bilingual Support**: Arabic (RTL) and English (LTR) layouts
- **E-commerce**: Product catalog, shopping cart, and order management
- **Course Management**: Secure video streaming and progress tracking
- **Appointment Scheduling**: Calendar-based booking with Zoom integration
- **Admin Dashboard**: Comprehensive management interface
- **Authentication**: JWT-based auth with Google OAuth support
- **Responsive Design**: Mobile-first approach with Tailwind CSS

## Development

### Backend Development
- Uses Entity Framework Core with PostgreSQL
- Implements CQRS pattern with MediatR
- Includes validation with FluentValidation
- Follows Clean Architecture principles

### Frontend Development
- TypeScript for type safety
- Tailwind CSS for styling
- React Router for navigation
- Axios for API communication
- Path aliases configured for clean imports

## Next Steps

1. Implement domain entities and value objects
2. Set up Entity Framework configurations and migrations
3. Create authentication and authorization system
4. Build API endpoints for each module
5. Develop frontend components and pages
6. Implement testing strategy
7. Set up CI/CD pipeline

## Contributing

Please follow the established architecture patterns and coding standards when contributing to this project.