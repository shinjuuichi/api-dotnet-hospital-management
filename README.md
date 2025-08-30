# Hospital Management System API

A comprehensive .NET 8 Web API for hospital management with JWT authentication, role-based authorization, and state machine workflow for appointments.

## Features

### Authentication & Authorization

- JWT token-based authentication
- Role-based access control (Manager, Doctor, Customer)
- OTP verification for registration and password reset
- Redis caching for OTP storage

### Core Modules

1. **User Management** - Profile management with image upload
2. **Specialty Management** - Medical specialty administration
3. **Doctor Management** - Doctor profiles and activation/deactivation
4. **Patient Management** - Customer/patient profiles
5. **Appointment Management** - Appointment booking with state machine workflow
6. **Prescription Management** - Medicine prescription by doctors
7. **Medicine Management** - Medicine inventory management

### State Machine Workflow

Appointment status transitions using Stateless library:

- Pending → Confirmed → Completed
- Cancel allowed from Pending/Confirmed states

## API Endpoints

### Guest Endpoints (No Authentication)

```
GET /guest/specialties
GET /guest/doctors
GET /guest/doctors/{doctorId}
```

### Authentication

```
POST /auth/register
POST /auth/register/confirm
POST /auth/login
POST /auth/logout
POST /auth/forgot-password
POST /auth/forgot-password/reset
```

### Profile Management

```
GET /profile
PUT /profile (with image upload)
```

### Specialty Management (Manager only)

```
GET /specialties
POST /specialties
GET /specialties/{id}
PUT /specialties/{id}
DELETE /specialties/{id}
```

### Doctor Management (Manager only)

```
GET /doctors
POST /doctors
GET /doctors/{doctorId}
PUT /doctors/{doctorId}
DELETE /doctors/{doctorId}
PATCH /doctors/{doctorId}/activate
PATCH /doctors/{doctorId}/deactivate
```

### Patient Management (Manager/Doctor only)

```
GET /patients
GET /patients/{patientId}
```

### Appointment Management

```
POST /appointments (Customer only)
GET /appointments
GET /appointments/{appointmentId}
DELETE /appointments/{appointmentId}
PATCH /appointments/{appointmentId}/confirm (Doctor/Manager)
PATCH /appointments/{appointmentId}/cancel
PATCH /appointments/{appointmentId}/complete (Doctor only)
PATCH /appointments/{appointmentId}/assign-doctor (Manager only)
```

### Prescription Management

```
POST /appointments/{appointmentId}/prescriptions (Doctor only)
GET /prescriptions
GET /prescriptions/{id}
```

### Medicine Management (Manager only)

```
GET /medicines
POST /medicines
GET /medicines/{id}
PUT /medicines/{id}
DELETE /medicines/{id}
```

## Technology Stack

- **.NET 8** - Web API framework
- **Entity Framework Core** - ORM with SQL Server
- **JWT Authentication** - Token-based security
- **Redis** - Caching for OTP storage
- **Stateless** - State machine for appointment workflow
- **BCrypt.Net** - Password hashing
- **AutoMapper** - Object mapping
- **Repository + UnitOfWork Pattern** - Data access layer
- **Swagger/OpenAPI** - API documentation

## Architecture

### Clean Architecture Layers

1. **Controllers** - HTTP endpoints and request handling
2. **Services** - Business logic and domain operations
3. **Repositories** - Data access abstraction
4. **Models** - Domain entities and DTOs
5. **Utils** - Cross-cutting concerns (JWT, crypto, email, images)

### Key Patterns

- Repository + Unit of Work pattern
- Dependency Injection
- Global exception handling
- JWT middleware for authentication
- State machine for appointment workflow

## Setup Instructions

1. **Update Connection Strings**

   ```json
   {
   	"ConnectionStrings": {
   		"DefaultConnection": "Server=.;Database=HospitalManagement;Trusted_Connection=true;TrustServerCertificate=true;",
   		"RedisConnection": "localhost:6379"
   	}
   }
   ```

2. **Configure JWT Settings**

   ```json
   {
   	"Jwt": {
   		"Key": "your-secret-key-here-at-least-256-bits",
   		"Issuer": "HospitalManagementAPI",
   		"Audience": "HospitalManagementClients",
   		"ExpiresMinutes": "1440"
   	}
   }
   ```

3. **Configure Email Settings**

   ```json
   {
   	"Email": {
   		"SmtpHost": "smtp.gmail.com",
   		"SmtpPort": "587",
   		"FromEmail": "your-email@gmail.com",
   		"FromPassword": "your-app-password"
   	}
   }
   ```

4. **Run Migrations**

   ```bash
   dotnet ef database update
   ```

5. **Start the Application**
   ```bash
   dotnet run
   ```

## Default Users

The system comes with pre-seeded users:

- **Manager**: manager@gmail.com / Admin@123
- **Doctor**: sarah.wilson@hospital.com / Admin@123
- **Customer**: alice.johnson@email.com / Admin@123

## Project Structure

```
WebAPI/
├── Controllers/          # API endpoints
├── Services/            # Business logic
├── Repositories/        # Data access layer
├── Models/             # Domain entities
├── DTOs/               # Data transfer objects
├── Utils/              # Utility classes
├── Data/               # DbContext and seeds
├── Middlewares/        # Custom middleware
└── Images/             # Image storage folder
```
