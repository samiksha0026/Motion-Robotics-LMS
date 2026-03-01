# Motion Robotics LMS

A full-stack Learning Management System built for robotics education. It manages schools, teachers, students, experiments, exams, certificates, and level-based progression through a structured robotics curriculum.

---

## Table of Contents

- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Backend Setup](#backend-setup)
  - [Frontend Setup](#frontend-setup)
- [Authentication and Session Management](#authentication-and-session-management)
- [API Endpoints](#api-endpoints)
- [Database Schema](#database-schema)
- [Roles and Access Control](#roles-and-access-control)
- [Configuration](#configuration)

---

## Tech Stack

### Backend
- ASP.NET Core 8.0 (Web API)
- Entity Framework Core 8.0 (SQL Server)
- ASP.NET Identity (authentication/authorization)
- MediatR (CQRS pattern)
- JWT Bearer tokens + httpOnly session cookies
- Swagger / OpenAPI

### Frontend
- Next.js 16 (App Router)
- React 19
- TypeScript
- Tailwind CSS 4
- Lucide React (icons)

---

## Architecture

The backend follows the **CQRS (Command Query Responsibility Segregation)** pattern using MediatR. Features are organized by domain area (Admin, Auth, Student, Teacher, Certificates) with separate command/query handlers.

Authentication uses a **dual-layer approach**:
1. **JWT access tokens** (15-minute expiry) stored in `sessionStorage` on the client
2. **Server-side sessions** tracked in a `UserSessions` database table with an httpOnly `sessionId` cookie
3. **Refresh token rotation** for seamless token renewal

Every authenticated API request is validated against the `UserSessions` table via middleware, ensuring sessions can be revoked server-side at any time.

---

## Project Structure

```
Motion-Robotics LMS/
|
|-- backend/
|   |-- MotionRobotics.LMS.sln
|   |-- MotionRobotics.LMS.API/
|   |   |-- Program.cs                  # App startup, DI, middleware pipeline
|   |   |-- Controllers/                # Legacy controllers (being migrated to Features)
|   |   |-- Features/                   # CQRS feature modules
|   |   |   |-- Admin/                  # School, teacher, student, class management
|   |   |   |-- Auth/                   # Login, logout, refresh, session management
|   |   |   |-- Certificates/           # Certificate generation and queries
|   |   |   |-- Student/                # Dashboard, experiments, exams, books
|   |   |   |-- Teacher/                # Dashboard, approvals, levels, classes
|   |   |-- Models/                     # EF Core entities
|   |   |-- Data/                       # DbContext, seeding
|   |   |-- Services/                   # Business logic (SessionService, etc.)
|   |   |-- Middleware/                  # Global exception handler, session validation
|   |   |-- Repositories/               # Data access layer
|   |   |-- Migrations/                 # EF Core migrations
|   |   |-- Helpers/                    # Certificate generator, exam evaluator
|   |   |-- DTOs/                       # Data transfer objects
|   |   |-- Seed/                       # Seed data
|   |   |-- wwwroot/                    # Static files (syllabus PDFs, etc.)
|   |-- MotionRobotics.LMS.Tests/       # Unit tests
|
|-- Frontend/
|   |-- app/                            # Next.js App Router pages
|   |   |-- login/                      # Login page
|   |   |-- admin/                      # Admin dashboard, schools management
|   |   |-- teacher/                    # Teacher dashboard, levels, approvals
|   |   |-- student/                    # Student dashboard, experiments, exams, books, certificates
|   |-- components/                     # Shared UI components
|   |-- lib/                            # Auth helpers, API config, types
|   |-- public/                         # Static assets (images, videos)
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (with npm)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or any SQL Server instance)
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

---

## Getting Started

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/samiksha0026/Motion-Robotics-LMS.git
   cd Motion-Robotics-LMS
   ```

2. **Configure the database connection**

   Edit `backend/MotionRobotics.LMS.API/appsettings.json` and update the connection string to point to your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=MotionRoboticsLMS;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
   }
   ```

3. **Apply database migrations**
   ```bash
   cd backend/MotionRobotics.LMS.API
   dotnet ef database update
   ```

4. **Run the backend**
   ```bash
   dotnet run
   ```
   The API will start at `http://localhost:5235`. Swagger UI is available at `http://localhost:5235/swagger`.

### Frontend Setup

1. **Install dependencies**
   ```bash
   cd Frontend
   npm install
   ```

2. **Configure the API URL** (optional)

   Create a `.env.local` file in the `Frontend/` directory:
   ```
   NEXT_PUBLIC_API_URL=http://localhost:5235
   ```
   If not set, it defaults to `http://localhost:5235`.

3. **Run the frontend**
   ```bash
   npm run dev
   ```
   The frontend will start at `http://localhost:3000`.

---

## Authentication and Session Management

The system uses a **database-backed session management** approach:

| Layer | Mechanism | Storage |
|---|---|---|
| Access Token | JWT (15 min expiry) | `sessionStorage` (browser) |
| Session Tracking | `UserSessions` DB table | SQL Server |
| Session Cookie | httpOnly `sessionId` cookie | Browser (not accessible via JS) |
| Refresh Token | Rotation-based (7 day expiry) | `RefreshTokens` DB table |

### Login Flow
1. User submits credentials to `POST /api/auth/login`
2. Backend validates credentials, creates a JWT + refresh token
3. A `UserSession` record is created in the database
4. An httpOnly cookie (`sessionId`) is set on the response
5. Frontend stores the JWT in `sessionStorage` and redirects by role

### Request Validation
- Every authenticated request passes through `SessionValidationMiddleware`
- The middleware reads the `sessionId` cookie, extracts the Bearer token, and validates both against the `UserSessions` table
- If the session is expired, revoked, or mismatched, a 401 is returned

### Session Expiry (Role-Based)
| Role | Session Duration |
|---|---|
| SuperAdmin | 8 hours |
| SchoolAdmin | 10 hours |
| Admin | 8 hours |
| Teacher | 12 hours |
| Student | 24 hours |

---

## API Endpoints

### Auth
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/login` | Login and create session |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | End current session |
| POST | `/api/auth/logout-all` | End all sessions for the user |
| GET | `/api/auth/me` | Get current session info |
| GET | `/api/auth/sessions` | List active sessions |
| DELETE | `/api/auth/sessions/{id}` | Revoke a specific session |

### Admin
| Method | Endpoint | Description |
|---|---|---|
| GET/POST | `/api/admin/schools` | List / create schools |
| GET/PUT/DELETE | `/api/admin/schools/{id}` | Get / update / delete a school |
| POST | `/api/admin/schools/{id}/logo` | Upload school logo |
| GET/POST | `/api/admin/teachers` | List / create teachers |
| GET/POST | `/api/admin/students` | List / create students |
| GET | `/api/admin/dashboard` | Admin dashboard stats |

### Teacher
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/teacher/dashboard` | Teacher dashboard |
| GET | `/api/teacher/levels/my-classes` | Classes and level assignments |
| POST | `/api/teacher/approvals/{id}` | Approve/reject experiment |

### Student
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/student/dashboard` | Student dashboard |
| GET | `/api/student/experiments` | List experiments for assigned level |
| POST | `/api/student/experiments/{id}/submit` | Submit experiment completion |
| GET | `/api/student/exams/{id}` | Get exam questions |
| POST | `/api/student/exams/{id}/submit` | Submit exam answers |
| GET | `/api/student/exams/{id}/result` | Get exam result |
| GET | `/api/student/certificates` | List earned certificates |
| GET | `/api/student/books` | Get digital books for current level |

---

## Database Schema

Key entities:

| Entity | Description |
|---|---|
| `AspNetUsers` | ASP.NET Identity users (all roles) |
| `Schools` | Registered schools |
| `Teachers` | Teacher profiles linked to users |
| `Students` | Student profiles linked to users |
| `Classes` | School classes |
| `RoboticsLevels` | Curriculum levels (Level 1, 2, etc.) |
| `Experiments` | Hands-on experiments within each level |
| `StudentProgress` | Tracks experiment completion and teacher approval |
| `Exams` | Level completion exams |
| `ExamResults` | Student exam scores |
| `Certificates` | Issued certificates |
| `UserSessions` | Active and historical login sessions |
| `RefreshTokens` | JWT refresh token records |
| `ClassExperimentUnlocks` | Teacher-controlled experiment availability per class |

---

## Roles and Access Control

| Role | Access |
|---|---|
| **SuperAdmin** | Full system access, manage all schools |
| **SchoolAdmin** | Manage their own school, teachers, students, classes |
| **Admin** | Administrative operations |
| **Teacher** | View assigned classes, approve experiments, manage levels |
| **Student** | View dashboard, complete experiments, take exams, earn certificates |

Role-based route guards are enforced on both the backend (JWT claims + middleware) and frontend (layout-level auth checks with server-side session validation).

---

## Configuration

All configuration is in `backend/MotionRobotics.LMS.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=MotionRoboticsLMS;..."
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "Issuer": "MotionRobotics",
    "Audience": "MotionRoboticsUsers",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Session": {
    "SuperAdminExpiryHours": 8,
    "SchoolAdminExpiryHours": 10,
    "AdminExpiryHours": 8,
    "TeacherExpiryHours": 12,
    "StudentExpiryHours": 24,
    "CleanupOlderThanDays": 30
  }
}
```

> **Note**: For production, move secrets (JWT key, connection string) to environment variables or a secrets manager. Never commit real credentials to version control.

---

## License

This project is proprietary to Motion Robotics.
