## **MOTION ROBOTICS LMS - COMPLETE API DOCUMENTATION**

### **BASE URL**
```
http://localhost:5000/api
```

---

## **AUTHENTICATION & AUTHORIZATION**

### **1. Admin Login**
```http
POST /admin/auth/login
Content-Type: application/json

{
  "email": "admin@school.com",
  "password": "AdminPassword123!"
}

Response: 200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "adminEmail": "admin@school.com",
  "roles": ["Admin"],
  "expiresAt": "2025-01-12T15:30:00Z"
}
```

### **2. Teacher Login**
```http
POST /teacher/auth/login
Content-Type: application/json

{
  "email": "teacher@school.com",
  "password": "TeacherPassword123!"
}

Response: 200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "adminEmail": "teacher@school.com",
  "roles": ["Teacher"],
  "expiresAt": "2025-01-12T23:30:00Z"
}
```

### **3. Student Login**
```http
POST /auth/login
Content-Type: application/json

{
  "email": "student@school.com",
  "password": "StudentPassword123!"
}

Response: 200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "role": ["Student"],
  "expires": "2025-01-12T15:30:00Z"
}
```

---

## **ADMIN ENDPOINTS** (Requires Admin Role)

### **Schools Management**

#### Create School
```http
POST /admin/schools
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "schoolName": "Delhi Public School",
  "address": "123 Main St, Delhi",
  "phoneNumber": "+91-9876543210",
  "email": "info@dps.edu"
}

Response: 201 Created
{
  "id": 1,
  "schoolName": "Delhi Public School",
  "address": "123 Main St, Delhi",
  "phoneNumber": "+91-9876543210",
  "email": "info@dps.edu",
  "createdAt": "2025-01-11T13:30:00Z",
  "studentCount": 0,
  "classCount": 0
}
```

#### Get All Schools
```http
GET /admin/schools
Authorization: Bearer {admin_token}

Response: 200 OK
[
  {
    "id": 1,
    "schoolName": "Delhi Public School",
    ...
  }
]
```

#### Get School by ID
```http
GET /admin/schools/{id}
Authorization: Bearer {admin_token}

Response: 200 OK
{
  "id": 1,
  ...
}
```

#### Update School
```http
PUT /admin/schools/{id}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "schoolName": "Updated School Name",
  ...
}

Response: 200 OK
```

#### Delete School
```http
DELETE /admin/schools/{id}
Authorization: Bearer {admin_token}

Response: 200 OK
{
  "message": "School deleted successfully"
}
```

---

### **Classes Management**

#### Create Class
```http
POST /admin/classes
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "className": "Class 6-A",
  "schoolId": 1
}

Response: 201 Created
{
  "id": 1,
  "className": "Class 6-A",
  "schoolId": 1,
  "schoolName": "Delhi Public School",
  "createdAt": "2025-01-11T13:30:00Z",
  "studentCount": 0
}
```

#### Get Classes by School
```http
GET /admin/classes/school/{schoolId}
Authorization: Bearer {admin_token}

Response: 200 OK
[...]
```

#### Get All Classes
```http
GET /admin/classes
Authorization: Bearer {admin_token}

Response: 200 OK
[...]
```

#### Update Class
```http
PUT /admin/classes/{id}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "className": "Class 6-B",
  "schoolId": 1
}

Response: 200 OK
```

#### Delete Class
```http
DELETE /admin/classes/{id}
Authorization: Bearer {admin_token}

Response: 200 OK
```

---

### **Students Management (Admin Creates)**

#### Create Student
```http
POST /admin/students
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "email": "student@school.com",
  "fullName": "John Doe",
  "password": "StudentPass123!",
  "rollNo": "001",
  "classId": 1,
  "schoolId": 1,
  "programName": "Digi-Coding",
  "programLevel": 1
}

Response: 201 Created
{
  "id": 1,
  "email": "student@school.com",
  "fullName": "John Doe",
  "rollNo": "001",
  "classId": 1,
  "className": "Class 6-A",
  "schoolId": 1,
  "schoolName": "Delhi Public School",
  "programName": "Digi-Coding",
  "programLevel": 1,
  "createdAt": "2025-01-11T13:30:00Z"
}
```

#### Get All Students
```http
GET /admin/students
Authorization: Bearer {admin_token}

Response: 200 OK
[...]
```

#### Get Students by School
```http
GET /admin/students/school/{schoolId}
Authorization: Bearer {admin_token}

Response: 200 OK
[...]
```

#### Get Students by Class
```http
GET /admin/students/class/{classId}
Authorization: Bearer {admin_token}

Response: 200 OK
[...]
```

#### Get Student by ID
```http
GET /admin/students/{id}
Authorization: Bearer {admin_token}

Response: 200 OK
{...}
```

#### Delete Student
```http
DELETE /admin/students/{id}
Authorization: Bearer {admin_token}

Response: 200 OK
{
  "message": "Student deleted successfully"
}
```

---

### **Teachers Management (Admin Creates)**

#### Create Teacher
```http
POST /admin/teachers
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "email": "teacher@school.com",
  "fullName": "Mrs. Jane Smith",
  "password": "TeacherPass123!",
  "phoneNumber": "+91-9876543210",
  "schoolId": 1,
  "classIds": [1, 2, 3]  // Classes to assign
}

Response: 201 Created
{
  "id": 1,
  "email": "teacher@school.com",
  "fullName": "Mrs. Jane Smith",
  "phoneNumber": "+91-9876543210",
  "schoolId": 1,
  "schoolName": "Delhi Public School",
  "classes": [
    {
      "id": 1,
      "className": "Class 6-A",
      ...
    }
  ],
  "createdAt": "2025-01-11T13:30:00Z"
}
```

#### Get All Teachers
```http
GET /admin/teachers
Authorization: Bearer {admin_token}

Response: 200 OK
[...]
```

#### Get Teachers by School
```http
GET /admin/teachers/school/{schoolId}
Authorization: Bearer {admin_token}

Response: 200 OK
[...]
```

#### Get Teacher by ID
```http
GET /admin/teachers/{id}
Authorization: Bearer {admin_token}

Response: 200 OK
{...}
```

#### Update Teacher
```http
PUT /admin/teachers/{id}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "email": "teacher@school.com",
  "fullName": "Updated Name",
  "password": "NewPass123!",
  "phoneNumber": "+91-1234567890",
  "schoolId": 1,
  "classIds": [1, 2]
}

Response: 200 OK
{...}
```

#### Assign Class to Teacher
```http
POST /admin/teachers/{teacherId}/classes/{classId}
Authorization: Bearer {admin_token}

Response: 200 OK
{
  "message": "Class assigned successfully"
}
```

#### Remove Class from Teacher
```http
DELETE /admin/teachers/{teacherId}/classes/{classId}
Authorization: Bearer {admin_token}

Response: 200 OK
{
  "message": "Class removed successfully"
}
```

#### Delete Teacher
```http
DELETE /admin/teachers/{id}
Authorization: Bearer {admin_token}

Response: 200 OK
{
  "message": "Teacher deleted successfully"
}
```

---

## **TEACHER ENDPOINTS** (Requires Teacher Role)

### **Attendance Management**

#### Record Attendance
```http
POST /teacher/attendance/record
Authorization: Bearer {teacher_token}
Content-Type: application/json

{
  "studentId": 1,
  "classId": 1,
  "attendanceDate": "2025-01-11T00:00:00Z",
  "isPresent": true,
  "remarks": "Present"
}

Response: 201 Created
{
  "id": 1,
  "studentId": 1,
  "studentName": "John Doe",
  "classId": 1,
  "className": "Class 6-A",
  "teacherId": 1,
  "teacherName": "Mrs. Jane Smith",
  "attendanceDate": "2025-01-11T00:00:00Z",
  "isPresent": true,
  "remarks": "Present",
  "recordedAt": "2025-01-11T13:30:00Z"
}
```

#### Get Class Attendance
```http
GET /teacher/attendance/class/{classId}
Authorization: Bearer {teacher_token}

Response: 200 OK
[...]
```

#### Get Class Attendance by Date
```http
GET /teacher/attendance/class/{classId}/date?date=2025-01-11
Authorization: Bearer {teacher_token}

Response: 200 OK
[...]
```

#### Get Student Attendance
```http
GET /teacher/attendance/student/{studentId}
Authorization: Bearer {teacher_token}

Response: 200 OK
[...]
```

#### Update Attendance
```http
PUT /teacher/attendance/{id}
Authorization: Bearer {teacher_token}
Content-Type: application/json

{
  "studentId": 1,
  "classId": 1,
  "attendanceDate": "2025-01-11T00:00:00Z",
  "isPresent": false,
  "remarks": "Absent - Sick"
}

Response: 200 OK
{...}
```

#### Delete Attendance
```http
DELETE /teacher/attendance/{id}
Authorization: Bearer {teacher_token}

Response: 200 OK
{
  "message": "Attendance record deleted successfully"
}
```

---

## **STUDENT ENDPOINTS** (Requires Student Role)

### **Dashboard**
```http
GET /students/dashboard
Authorization: Bearer {student_token}

Response: 200 OK
{
  "student": {
    "fullName": "John Doe",
    "classId": 1,
    "programName": "Digi-Coding",
    "programLevel": 1
  },
  "progress": {
    "percent": 75,
    "completedExperiments": 3,
    "totalExperiments": 4
  },
  "exams": {
    "pending": [
      {
        "id": 1,
        "title": "Digi-Coding Final Exam",
        "timerMinutes": 60
      }
    ]
  },
  "certificates": [...]
}
```

### **Experiments**
```http
POST /students/complete-experiment/{experimentId}
Authorization: Bearer {student_token}

Response: 200 OK
{
  "message": "Experiment marked as completed",
  "experimentId": 1
}
```

### **Exams**
```http
POST /students/submit-exam/{examId}
Authorization: Bearer {student_token}
Content-Type: application/json

{
  "score": 85
}

Response: 200 OK
{
  "message": "Exam passed! Certificate issued.",
  "certificate": {...}
}
```

---

## **ERROR RESPONSES**

### **400 Bad Request**
```json
{
  "message": "Invalid request data or business logic violation"
}
```

### **401 Unauthorized**
```json
{
  "message": "Invalid credentials or expired token"
}
```

### **404 Not Found**
```json
{
  "message": "Resource not found"
}
```

### **500 Internal Server Error**
```json
{
  "message": "An internal server error occurred"
}
```

---

## **IMPLEMENTATION CHECKLIST**

? Admin Module
- ? School CRUD
- ? Class CRUD
- ? Student Creation (with User account)
- ? Teacher Creation (with User account)
- ? Class Assignment to Teachers
- ? Admin Auth

? Teacher Module
- ? Teacher Auth
- ? Attendance Recording
- ? View Class Attendance
- ? View Student Attendance

? Student Module (Existing)
- ? Student Auth
- ? Dashboard
- ? Complete Experiments
- ? Submit Exams
- ? View Certificates

---

## **DATABASE MIGRATION REQUIRED**

Run this command to create migration:
```bash
dotnet ef migrations add AddTeacherAndAttendance -o Migrations
dotnet ef database update
```

This will create tables:
- Teachers
- TeacherClasses
- Attendances

