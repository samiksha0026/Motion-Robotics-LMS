# **MOTION ROBOTICS LMS - COMPLETE CORE FLOW GUIDE**

## **?? STEP-BY-STEP WALKTHROUGH**

Follow these steps in order using Postman, Thunder Client, or cURL.

---

## **PHASE 1: SETUP & ADMIN CREATION**

### **Step 1.1: Check if Admin Exists**

```http
GET http://localhost:5000/api/admin/setup/check-admin-exists
```

**Expected Response:**
```json
{
  "adminExists": false,
  "adminCount": 0,
  "message": "No admin found. Use POST /api/admin/setup/create-first-admin"
}
```

---

### **Step 1.2: Create First Admin Account (ONE-TIME ONLY)**

```http
POST http://localhost:5000/api/admin/setup/create-first-admin
Content-Type: application/json

{
  "email": "admin@motionrobotics.com",
  "password": "Admin@123456"
}
```

**Expected Response:**
```json
{
  "message": "Admin account created successfully!",
  "adminEmail": "admin@motionrobotics.com",
  "instructions": {
    "step1": "Use the email and password to login at /api/admin/auth/login",
    "step2": "After login, you can create schools, classes, teachers, and students",
    "roles_created": [
      "Admin",
      "Teacher",
      "Student"
    ]
  }
}
```

?? **Save the Admin Credentials:**
```
Email: admin@motionrobotics.com
Password: Admin@123456
```

---

## **PHASE 2: ADMIN LOGIN & AUTHENTICATION**

### **Step 2.1: Admin Login**

```http
POST http://localhost:5000/api/admin/auth/login
Content-Type: application/json

{
  "email": "admin@motionrobotics.com",
  "password": "Admin@123456"
}
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "adminEmail": "admin@motionrobotics.com",
  "roles": ["Admin"],
  "expiresAt": "2025-01-12T15:30:00Z"
}
```

?? **Copy the Token and save it. You'll need it for all admin operations:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## **PHASE 3: SCHOOL MANAGEMENT**

### **Step 3.1: Create School**

```http
POST http://localhost:5000/api/admin/schools
Authorization: Bearer {YOUR_ADMIN_TOKEN}
Content-Type: application/json

{
  "schoolName": "Delhi Public School",
  "address": "123 Main Street, New Delhi",
  "phoneNumber": "+91-9876543210",
  "email": "info@dps.edu"
}
```

**Expected Response:**
```json
{
  "id": 1,
  "schoolName": "Delhi Public School",
  "address": "123 Main Street, New Delhi",
  "phoneNumber": "+91-9876543210",
  "email": "info@dps.edu",
  "createdAt": "2025-01-11T13:30:00Z",
  "studentCount": 0,
  "classCount": 0
}
```

? **Save the School ID:** `1`

---

### **Step 3.2: Get All Schools**

```http
GET http://localhost:5000/api/admin/schools
Authorization: Bearer {YOUR_ADMIN_TOKEN}
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "schoolName": "Delhi Public School",
    ...
  }
]
```

---

## **PHASE 4: CLASS MANAGEMENT**

### **Step 4.1: Create Class (in the School)**

```http
POST http://localhost:5000/api/admin/classes
Authorization: Bearer {YOUR_ADMIN_TOKEN}
Content-Type: application/json

{
  "className": "Class 6-A",
  "schoolId": 1
}
```

**Expected Response:**
```json
{
  "id": 1,
  "className": "Class 6-A",
  "schoolId": 1,
  "schoolName": "Delhi Public School",
  "createdAt": "2025-01-11T13:30:00Z",
  "studentCount": 0
}
```

? **Save the Class ID:** `1`

---

### **Step 4.2: Create More Classes**

Create at least 2-3 classes for testing:

```http
POST http://localhost:5000/api/admin/classes
Authorization: Bearer {YOUR_ADMIN_TOKEN}
Content-Type: application/json

{
  "className": "Class 6-B",
  "schoolId": 1
}
```

---

### **Step 4.3: Get Classes by School**

```http
GET http://localhost:5000/api/admin/classes/school/1
Authorization: Bearer {YOUR_ADMIN_TOKEN}
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "className": "Class 6-A",
    ...
  },
  {
    "id": 2,
    "className": "Class 6-B",
    ...
  }
]
```

---

## **PHASE 5: TEACHER MANAGEMENT**

### **Step 5.1: Create First Teacher**

```http
POST http://localhost:5000/api/admin/teachers
Authorization: Bearer {YOUR_ADMIN_TOKEN}
Content-Type: application/json

{
  "email": "teacher1@school.com",
  "fullName": "Mrs. Jane Smith",
  "password": "Teacher@123456",
  "phoneNumber": "+91-9876543211",
  "schoolId": 1,
  "classIds": [1, 2]
}
```

**Expected Response:**
```json
{
  "id": 1,
  "email": "teacher1@school.com",
  "fullName": "Mrs. Jane Smith",
  "phoneNumber": "+91-9876543211",
  "schoolId": 1,
  "schoolName": "Delhi Public School",
  "classes": [
    {
      "id": 1,
      "className": "Class 6-A",
      ...
    },
    {
      "id": 2,
      "className": "Class 6-B",
      ...
    }
  ],
  "createdAt": "2025-01-11T13:30:00Z"
}
```

? **Save Teacher Info:**
```
Teacher ID: 1
Email: teacher1@school.com
Password: Teacher@123456
```

---

### **Step 5.2: Create Second Teacher**

```http
POST http://localhost:5000/api/admin/teachers
Authorization: Bearer {YOUR_ADMIN_TOKEN}
Content-Type: application/json

{
  "email": "teacher2@school.com",
  "fullName": "Mr. Robert Johnson",
  "password": "Teacher@123456",
  "phoneNumber": "+91-9876543212",
  "schoolId": 1,
  "classIds": [1]
}
```

---

### **Step 5.3: Get All Teachers**

```http
GET http://localhost:5000/api/admin/teachers
Authorization: Bearer {YOUR_ADMIN_TOKEN}
```

---

## **PHASE 6: STUDENT MANAGEMENT**

### **Step 6.1: Create First Student**

```http
POST http://localhost:5000/api/admin/students
Authorization: Bearer {YOUR_ADMIN_TOKEN}
Content-Type: application/json

{
  "email": "student1@school.com",
  "fullName": "Aarav Sharma",
  "password": "Student@123456",
  "rollNo": "001",
  "classId": 1,
  "schoolId": 1,
  "programName": "Digi-Coding",
  "programLevel": 1
}
```

**Expected Response:**
```json
{
  "id": 1,
  "email": "student1@school.com",
  "fullName": "Aarav Sharma",
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

? **Save Student Info:**
```
Student ID: 1
Email: student1@school.com
Password: Student@123456
```

---

### **Step 6.2: Create More Students**

```http
POST http://localhost:5000/api/admin/students
Authorization: Bearer {YOUR_ADMIN_TOKEN}
Content-Type: application/json

{
  "email": "student2@school.com",
  "fullName": "Priya Patel",
  "password": "Student@123456",
  "rollNo": "002",
  "classId": 1,
  "schoolId": 1,
  "programName": "Mech-Tech",
  "programLevel": 1
}
```

---

### **Step 6.3: Create Student in Different Class**

```http
POST http://localhost:5000/api/admin/students
Authorization: Bearer {YOUR_ADMIN_TOKEN}
Content-Type: application/json

{
  "email": "student3@school.com",
  "fullName": "Raj Kumar",
  "password": "Student@123456",
  "rollNo": "003",
  "classId": 2,
  "schoolId": 1,
  "programName": "Digi-Sense",
  "programLevel": 1
}
```

---

### **Step 6.4: Get All Students**

```http
GET http://localhost:5000/api/admin/students
Authorization: Bearer {YOUR_ADMIN_TOKEN}
```

---

### **Step 6.5: Get Students by Class**

```http
GET http://localhost:5000/api/admin/students/class/1
Authorization: Bearer {YOUR_ADMIN_TOKEN}
```

---

## **PHASE 7: TEACHER MODULE - ATTENDANCE**

### **Step 7.1: Teacher Login**

```http
POST http://localhost:5000/api/teacher/auth/login
Content-Type: application/json

{
  "email": "teacher1@school.com",
  "password": "Teacher@123456"
}
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "adminEmail": "teacher1@school.com",
  "roles": ["Teacher"],
  "expiresAt": "2025-01-12T23:30:00Z"
}
```

? **Save Teacher Token:** `{TEACHER_TOKEN}`

---

### **Step 7.2: Record Attendance for Student 1**

```http
POST http://localhost:5000/api/teacher/attendance/record
Authorization: Bearer {TEACHER_TOKEN}
Content-Type: application/json

{
  "studentId": 1,
  "classId": 1,
  "attendanceDate": "2025-01-11T00:00:00Z",
  "isPresent": true,
  "remarks": "Present - Active Participation"
}
```

**Expected Response:**
```json
{
  "id": 1,
  "studentId": 1,
  "studentName": "Aarav Sharma",
  "classId": 1,
  "className": "Class 6-A",
  "teacherId": 1,
  "teacherName": "Mrs. Jane Smith",
  "attendanceDate": "2025-01-11T00:00:00Z",
  "isPresent": true,
  "remarks": "Present - Active Participation",
  "recordedAt": "2025-01-11T13:30:00Z"
}
```

---

### **Step 7.3: Record Attendance for Student 2**

```http
POST http://localhost:5000/api/teacher/attendance/record
Authorization: Bearer {TEACHER_TOKEN}
Content-Type: application/json

{
  "studentId": 2,
  "classId": 1,
  "attendanceDate": "2025-01-11T00:00:00Z",
  "isPresent": false,
  "remarks": "Absent - Sick Leave"
}
```

---

### **Step 7.4: Get Class Attendance for Specific Date**

```http
GET http://localhost:5000/api/teacher/attendance/class/1/date?date=2025-01-11
Authorization: Bearer {TEACHER_TOKEN}
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "studentId": 1,
    "studentName": "Aarav Sharma",
    "isPresent": true,
    ...
  },
  {
    "id": 2,
    "studentId": 2,
    "studentName": "Priya Patel",
    "isPresent": false,
    ...
  }
]
```

---

### **Step 7.5: Get Student Attendance History**

```http
GET http://localhost:5000/api/teacher/attendance/student/1
Authorization: Bearer {TEACHER_TOKEN}
```

---

## **PHASE 8: STUDENT MODULE - DASHBOARD & EXPERIMENTS**

### **Step 8.1: Student Login**

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "student1@school.com",
  "password": "Student@123456"
}
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": ["Student"],
  "expires": "2025-01-12T15:30:00Z"
}
```

? **Save Student Token:** `{STUDENT_TOKEN}`

---

### **Step 8.2: View Student Dashboard**

```http
GET http://localhost:5000/api/students/dashboard
Authorization: Bearer {STUDENT_TOKEN}
```

**Expected Response:**
```json
{
  "student": {
    "fullName": "Aarav Sharma",
    "classId": 1,
    "programName": "Digi-Coding",
    "programLevel": 1
  },
  "progress": {
    "percent": 0,
    "completedExperiments": 0,
    "totalExperiments": 0
  },
  "exams": {
    "pending": []
  },
  "certificates": []
}
```

---

### **Step 8.3: Complete an Experiment**

Assuming there's an experiment with ID 1:

```http
POST http://localhost:5000/api/students/complete-experiment/1
Authorization: Bearer {STUDENT_TOKEN}
```

**Expected Response:**
```json
{
  "message": "Experiment marked as completed",
  "experimentId": 1
}
```

---

### **Step 8.4: Submit an Exam**

Assuming there's an exam with ID 1:

```http
POST http://localhost:5000/api/students/submit-exam/1
Authorization: Bearer {STUDENT_TOKEN}
Content-Type: application/json

{
  "score": 85
}
```

**Expected Response:**
```json
{
  "message": "Exam passed! Certificate issued.",
  "certificate": {
    "id": 1,
    "studentEmail": "student1@school.com",
    "programName": "Digi-Coding",
    "level": 1,
    "title": "Certificate of Completion - Digi-Coding Final Exam",
    "issuedDate": "2025-01-11T13:30:00Z"
  }
}
```

---

## **PHASE 9: ADMIN DASHBOARD OPERATIONS**

### **Step 9.1: View All Schools**
```http
GET http://localhost:5000/api/admin/schools
Authorization: Bearer {ADMIN_TOKEN}
```

### **Step 9.2: View All Classes**
```http
GET http://localhost:5000/api/admin/classes
Authorization: Bearer {ADMIN_TOKEN}
```

### **Step 9.3: View All Teachers**
```http
GET http://localhost:5000/api/admin/teachers
Authorization: Bearer {ADMIN_TOKEN}
```

### **Step 9.4: View All Students**
```http
GET http://localhost:5000/api/admin/students
Authorization: Bearer {ADMIN_TOKEN}
```

### **Step 9.5: View Teachers by School**
```http
GET http://localhost:5000/api/admin/teachers/school/1
Authorization: Bearer {ADMIN_TOKEN}
```

### **Step 9.6: View Students by School**
```http
GET http://localhost:5000/api/admin/students/school/1
Authorization: Bearer {ADMIN_TOKEN}
```

### **Step 9.7: View Students by Class**
```http
GET http://localhost:5000/api/admin/students/class/1
Authorization: Bearer {ADMIN_TOKEN}
```

---

## **SUMMARY OF CORE FLOW**

```
???????????????????????????????????????????????????
?      1. ADMIN SETUP & AUTHENTICATION            ?
?   Create Admin ? Login ? Get Admin Token        ?
???????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????
?       2. ADMIN CREATES SCHOOLS & CLASSES        ?
?   School ? Classes (assign to school)           ?
???????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????
?      3. ADMIN CREATES TEACHERS & STUDENTS       ?
?   Teachers (assign to classes) ? Students       ?
???????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????
?       4. TEACHER LOGIN & RECORD ATTENDANCE      ?
?   Teacher Login ? Record Attendance             ?
???????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????
?     5. STUDENT LOGIN & ACCESS DASHBOARD         ?
?   Student Login ? View Dashboard ? Complete     ?
?   Experiments ? Submit Exams ? Get Certificates ?
???????????????????????????????????????????????????
```

---

## **QUICK REFERENCE TABLE**

| Role | Email | Password | Token From | Permissions |
|------|-------|----------|-----------|-------------|
| Admin | admin@motionrobotics.com | Admin@123456 | `/api/admin/auth/login` | Create Schools, Classes, Teachers, Students |
| Teacher 1 | teacher1@school.com | Teacher@123456 | `/api/teacher/auth/login` | Record/View Attendance |
| Student 1 | student1@school.com | Student@123456 | `/api/auth/login` | View Dashboard, Complete Experiments, Submit Exams |

---

## **TESTING CHECKLIST**

- [ ] Step 1.1: Check if admin exists
- [ ] Step 1.2: Create first admin
- [ ] Step 2.1: Admin login
- [ ] Step 3.1: Create school
- [ ] Step 4.1: Create class
- [ ] Step 5.1: Create teacher
- [ ] Step 6.1: Create student
- [ ] Step 7.1: Teacher login
- [ ] Step 7.2: Record attendance
- [ ] Step 8.1: Student login
- [ ] Step 8.2: View student dashboard
- [ ] Step 8.3: Complete experiment
- [ ] Step 8.4: Submit exam

---

## **NEXT STEPS**

1. **Run Database Migration** (if not done)
   ```bash
   dotnet ef database update
   ```

2. **Start the Application**
   ```bash
   dotnet run
   ```

3. **Follow the steps above** using Postman/Thunder Client

4. **Test each phase** and verify responses

5. **Monitor the application logs** for any errors

