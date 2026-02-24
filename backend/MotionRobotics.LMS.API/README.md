# MOTION ROBOTICS LMS - BACKEND API

## BUILD STATUS: SUCCESSFUL

## PROFESSIONAL ADMIN AUTHENTICATION

### How It Works:
- Admin is AUTO-CREATED on first application startup
- No public registration endpoint (security best practice)
- One-time seed - runs only when admin doesn't exist
- Industry standard for enterprise LMS systems

### Admin Credentials (Pre-created at startup):
```
Email:    admin@motionrobotics.com
Password: Admin@123
```

### What Happens When You Run the Application:
```
1. Application starts
2. Database migrations applied
3. AdminSeeder checks if admin exists
4. If not -> Creates admin account
5. Console shows: "Admin account created successfully!"
6. Admin can now login
```

---

## QUICK START (3 Steps)

### Step 1: Database Migration
```bash
cd MotionRobotics.LMS.API
dotnet ef database update
```

### Step 2: Start Application
```bash
dotnet run
```

Expected Output:
```
Admin account created successfully!
   Email: admin@motionrobotics.com
   Password: Admin@123
   IMPORTANT: Change password after first login!
Database seeding completed!
Now listening on: http://localhost:5000
```

### Step 3: Login as Admin
```http
POST http://localhost:5000/api/admin/auth/login
Content-Type: application/json

{
  "email": "admin@motionrobotics.com",
  "password": "Admin@123"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "adminEmail": "admin@motionrobotics.com",
  "roles": ["Admin"],
  "expiresAt": "2025-01-12T15:30:00Z"
}
```

---

## CORE MODULES

### Admin Module
- Create Schools
- Create Classes
- Create Teachers
- Create Students
- Manage All Resources

### Teacher Module
- Login
- Record Attendance
- View Class Attendance
- Update Attendance

### Student Module
- Login
- View Dashboard
- Complete Experiments
- Submit Exams
- View Certificates

---

## API ENDPOINTS

### Authentication
- POST /api/admin/auth/login (Admin)
- POST /api/teacher/auth/login (Teacher)
- POST /api/auth/login (Student)

### Schools
- GET /api/admin/schools
- POST /api/admin/schools
- PUT /api/admin/schools/{id}
- DELETE /api/admin/schools/{id}

### Classes
- GET /api/admin/classes
- GET /api/admin/classes/school/{schoolId}
- POST /api/admin/classes
- PUT /api/admin/classes/{id}
- DELETE /api/admin/classes/{id}

### Teachers
- GET /api/admin/teachers
- GET /api/admin/teachers/school/{schoolId}
- POST /api/admin/teachers
- PUT /api/admin/teachers/{id}
- DELETE /api/admin/teachers/{id}

### Students
- GET /api/admin/students
- GET /api/admin/students/school/{schoolId}
- GET /api/admin/students/class/{classId}
- POST /api/admin/students
- DELETE /api/admin/students/{id}

### Attendance (Teacher)
- POST /api/teacher/attendance/record
- GET /api/teacher/attendance/class/{classId}
- GET /api/teacher/attendance/student/{studentId}

### Student Dashboard
- GET /api/students/dashboard
- POST /api/students/complete-experiment/{id}
- POST /api/students/submit-exam/{id}

---

## DEFAULT CREDENTIALS

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@motionrobotics.com | Admin@123 |
| Teacher | (created by admin) | Teacher@123 |
| Student | (created by admin) | Student@123 |

---

## SECURITY FEATURES

- JWT Authentication
- Role-based Authorization (Admin/Teacher/Student)
- Password Hashing (ASP.NET Identity)
- No public registration endpoints
- Admin seeded securely at startup

---

## KEY FILES

| File | Purpose |
|------|---------|
| Data/AdminSeeder.cs | Creates admin on startup |
| COMPLETE_FLOW_GUIDE.md | Step-by-step walkthrough |
| API_DOCUMENTATION.md | All API endpoints |
| Postman_Collection.json | API testing |

---

## DOCUMENTATION

- QUICK_START_GUIDE.md - Quick setup
- COMPLETE_FLOW_GUIDE.md - Full walkthrough
- ADMIN_DASHBOARD_GUIDE.md - Admin operations
- API_DOCUMENTATION.md - API reference

---

## READY TO USE

1. Run: dotnet ef database update
2. Run: dotnet run
3. Admin is auto-created
4. Login with admin@motionrobotics.com / Admin@123
5. Start managing schools, teachers, students!


