# **SWAGGER UI GUIDE - Testing Your API**

## **?? WHY USE SWAGGER?**

Swagger UI is **built into your application** and provides:
- ? Interactive API documentation
- ? Test endpoints directly in browser
- ? No need for Postman
- ? Auto-generated from your controllers
- ? See all available endpoints
- ? Easy authentication with JWT tokens

---

## **?? QUICK START (3 Steps)**

### **Step 1: Start Your Application**
```bash
cd F:\MotionRobotics.LMS\MotionRobotics.LMS.API
dotnet run
```

**Expected Output:**
```
? Admin account created successfully!
   Email: admin@motionrobotics.com
   Password: Admin@123
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
```

---

### **Step 2: Open Swagger UI**

Open your browser and go to:
```
http://localhost:5000/swagger
```

or

```
https://localhost:5001/swagger
```

**You'll see:**
- All your API endpoints organized by controller
- Interactive documentation
- "Try it out" buttons for each endpoint

---

### **Step 3: Authenticate in Swagger**

Since most endpoints require authentication, you need to login first and add the token to Swagger.

#### **A. Login as Admin**

1. In Swagger UI, find **"AdminAuth"** section
2. Click on **POST /api/admin/auth/login**
3. Click **"Try it out"**
4. Enter the request body:
```json
{
  "email": "admin@motionrobotics.com",
  "password": "Admin@123"
}
```
5. Click **"Execute"**
6. **Copy the token** from the response

#### **B. Add Token to Swagger**

1. Click the **"Authorize"** button at the top (?? lock icon)
2. In the "Value" field, type: `Bearer {your_token}`
   - Replace `{your_token}` with the actual token you copied
   - Example: `Bearer eyJhbGciOiJIUzI1NiIs...`
3. Click **"Authorize"**
4. Click **"Close"**

? **Now all your requests will include the authentication token!**

---

## **?? COMPLETE WORKFLOW IN SWAGGER**

### **Phase 1: Admin Login**

1. **POST /api/admin/auth/login**
   - Click "Try it out"
   - Request Body:
   ```json
   {
     "email": "admin@motionrobotics.com",
     "password": "Admin@123"
   }
   ```
   - Click "Execute"
   - **Copy the token from response**
   - Click "Authorize" at top and add: `Bearer {token}`

---

### **Phase 2: Create School**

1. **POST /api/admin/schools**
   - Click "Try it out"
   - Request Body:
   ```json
   {
     "schoolName": "Delhi Public School",
     "address": "123 Main Street, New Delhi",
     "phoneNumber": "+91-9876543210",
     "email": "info@dps.edu"
   }
   ```
   - Click "Execute"
   - **Note the school ID** from response (e.g., `"id": 1`)

---

### **Phase 3: Create Classes**

1. **POST /api/admin/classes**
   - Click "Try it out"
   - Request Body:
   ```json
   {
     "className": "Class 6-A",
     "schoolId": 1
   }
   ```
   - Click "Execute"
   - **Note the class ID** (e.g., `"id": 1`)

2. Create another class (Class 6-B):
   ```json
   {
     "className": "Class 6-B",
     "schoolId": 1
   }
   ```
   - **Note the class ID** (e.g., `"id": 2`)

---

### **Phase 4: Create Teacher**

1. **POST /api/admin/teachers**
   - Click "Try it out"
   - Request Body:
   ```json
   {
     "email": "teacher1@school.com",
     "fullName": "Mrs. Jane Smith",
     "password": "Teacher@123",
     "phoneNumber": "+91-9876543211",
     "schoolId": 1,
     "classIds": [1, 2]
   }
   ```
   - Click "Execute"
   - Teacher created and assigned to both classes!

---

### **Phase 5: Create Students**

1. **POST /api/admin/students**
   - Click "Try it out"
   - Request Body (Student 1):
   ```json
   {
     "email": "student1@school.com",
     "fullName": "Aarav Sharma",
     "password": "Student@123",
     "rollNo": "001",
     "classId": 1,
     "schoolId": 1,
     "programName": "Digi-Coding",
     "programLevel": 1
   }
   ```
   - Click "Execute"
   - **Note student ID** (e.g., `"id": 1`)

2. Create Student 2:
   ```json
   {
     "email": "student2@school.com",
     "fullName": "Priya Patel",
     "password": "Student@123",
     "rollNo": "002",
     "classId": 1,
     "schoolId": 1,
     "programName": "Mech-Tech",
     "programLevel": 1
   }
   ```

---

### **Phase 6: Teacher Login & Record Attendance**

#### **A. Teacher Login**

1. **POST /api/teacher/auth/login**
   - Click "Try it out"
   - Request Body:
   ```json
   {
     "email": "teacher1@school.com",
     "password": "Teacher@123"
   }
   ```
   - Click "Execute"
   - **Copy teacher token**
   - Click "Authorize" and replace with: `Bearer {teacher_token}`

#### **B. Record Attendance**

1. **POST /api/teacher/attendance/record**
   - Click "Try it out"
   - Request Body:
   ```json
   {
     "studentId": 1,
     "classId": 1,
     "attendanceDate": "2025-01-11T00:00:00Z",
     "isPresent": true,
     "remarks": "Present - Active Participation"
   }
   ```
   - Click "Execute"

2. Record for Student 2 (Absent):
   ```json
   {
     "studentId": 2,
     "classId": 1,
     "attendanceDate": "2025-01-11T00:00:00Z",
     "isPresent": false,
     "remarks": "Absent - Sick Leave"
   }
   ```

#### **C. View Class Attendance**

1. **GET /api/teacher/attendance/class/{classId}/date**
   - Click "Try it out"
   - classId: `1`
   - date: `2025-01-11`
   - Click "Execute"
   - **See all attendance records for that class and date**

---

### **Phase 7: Student Login & Dashboard**

#### **A. Student Login**

1. **POST /api/auth/login**
   - Click "Try it out"
   - Request Body:
   ```json
   {
     "email": "student1@school.com",
     "password": "Student@123"
   }
   ```
   - Click "Execute"
   - **Copy student token**
   - Click "Authorize" and replace with: `Bearer {student_token}`

#### **B. View Dashboard**

1. **GET /api/students/dashboard**
   - Click "Try it out"
   - Click "Execute"
   - **See student progress, experiments, exams, certificates**

---

## **?? SWAGGER UI FEATURES**

### **1. Explore Endpoints**
```
- Expand any section (e.g., "AdminAuth", "Schools", "Classes")
- Click on any endpoint to see details
- See request/response models
- View parameters and data types
```

### **2. Try It Out**
```
- Click "Try it out" button
- Modify the request body
- Click "Execute"
- See live response
```

### **3. Authentication**
```
- Click "Authorize" button at top
- Add: Bearer {your_token}
- All subsequent requests use this token
```

### **4. Response Details**
```
- See HTTP status code
- View response body
- Check response headers
- See execution time
```

---

## **?? SWAGGER ENDPOINTS ORGANIZATION**

In Swagger UI, you'll see endpoints grouped like this:

```
?? AdminAuth
   - POST /api/admin/auth/login

?? Schools (Admin)
   - GET /api/admin/schools
   - POST /api/admin/schools
   - GET /api/admin/schools/{id}
   - PUT /api/admin/schools/{id}
   - DELETE /api/admin/schools/{id}

?? Classes (Admin)
   - GET /api/admin/classes
   - POST /api/admin/classes
   - GET /api/admin/classes/{id}
   - GET /api/admin/classes/school/{schoolId}
   - PUT /api/admin/classes/{id}
   - DELETE /api/admin/classes/{id}

?? Teachers (Admin)
   - GET /api/admin/teachers
   - POST /api/admin/teachers
   - GET /api/admin/teachers/{id}
   - GET /api/admin/teachers/school/{schoolId}
   - PUT /api/admin/teachers/{id}
   - DELETE /api/admin/teachers/{id}
   - POST /api/admin/teachers/{teacherId}/classes/{classId}
   - DELETE /api/admin/teachers/{teacherId}/classes/{classId}

?? Students (Admin)
   - GET /api/admin/students
   - POST /api/admin/students
   - GET /api/admin/students/{id}
   - GET /api/admin/students/school/{schoolId}
   - GET /api/admin/students/class/{classId}
   - DELETE /api/admin/students/{id}

?? TeacherAuth
   - POST /api/teacher/auth/login

?? TeacherAttendance
   - POST /api/teacher/attendance/record
   - GET /api/teacher/attendance/{id}
   - GET /api/teacher/attendance/class/{classId}
   - GET /api/teacher/attendance/class/{classId}/date
   - GET /api/teacher/attendance/student/{studentId}
   - PUT /api/teacher/attendance/{id}
   - DELETE /api/teacher/attendance/{id}

?? Auth (Student)
   - POST /api/auth/login

?? Students (Dashboard)
   - GET /api/students/dashboard
   - POST /api/students/complete-experiment/{experimentId}
   - POST /api/students/submit-exam/{examId}
```

---

## **?? TIPS FOR USING SWAGGER**

### **1. Keep Track of IDs**
When you create resources, note their IDs:
- School ID: 1
- Class ID 1: 1
- Class ID 2: 2
- Teacher ID: 1
- Student ID 1: 1
- Student ID 2: 2

### **2. Switch Between Users**
To test different roles:
1. Login as Admin ? Get token ? Authorize
2. Test admin endpoints
3. Login as Teacher ? Get token ? Authorize (replace admin token)
4. Test teacher endpoints
5. Login as Student ? Get token ? Authorize (replace teacher token)
6. Test student endpoints

### **3. Use Swagger Models**
Click on "Schemas" at the bottom to see all DTOs:
- SchoolCreateDto
- ClassCreateDto
- TeacherCreateDto
- StudentCreateDto
- AttendanceCreateDto

### **4. Error Responses**
Swagger shows all possible responses:
- 200 OK
- 201 Created
- 400 Bad Request
- 401 Unauthorized
- 404 Not Found

---

## **?? COMMON WORKFLOWS**

### **Workflow 1: Create Complete School Setup**
1. Login as Admin
2. Create School
3. Create 2-3 Classes
4. Create 2-3 Teachers (assign to classes)
5. Create 5-10 Students (assign to classes)
6. Verify: GET /api/admin/schools (see counts)

### **Workflow 2: Record Daily Attendance**
1. Login as Teacher
2. Record attendance for each student in class
3. View class attendance for today
4. Update any mistakes
5. View student-specific attendance history

### **Workflow 3: Student Views Progress**
1. Login as Student
2. View dashboard (see progress, experiments, exams)
3. Complete experiments
4. Submit exams
5. View certificates

---

## **?? SCREENSHOTS GUIDE**

### **What You'll See in Swagger:**

1. **Top Section:**
   - "Authorize" button (??)
   - API title and description

2. **Endpoints List:**
   - Grouped by controller
   - Color-coded by HTTP method:
     - ?? GET (green)
     - ?? POST (blue)
     - ?? PUT (yellow)
     - ?? DELETE (red)

3. **Endpoint Details:**
   - Parameters
   - Request body (with example)
   - Responses (with examples)
   - "Try it out" button

4. **Execute Section:**
   - Request body editor
   - "Execute" button
   - Response viewer

---

## **?? TROUBLESHOOTING**

### **Issue: 401 Unauthorized**
```
Solution:
1. Make sure you logged in
2. Copy the token correctly
3. Click "Authorize" at top
4. Format: Bearer {token} (with space)
5. Click "Authorize" button
```

### **Issue: Can't see all endpoints**
```
Solution:
1. Scroll down in Swagger UI
2. Expand each controller section
3. All 30+ endpoints should be visible
```

### **Issue: Token expired**
```
Solution:
1. Login again to get new token
2. Update authorization with new token
3. Admin tokens expire after 2 hours
4. Teacher tokens expire after 8 hours
```

### **Issue: Swagger not loading**
```
Solution:
1. Check application is running
2. Clear browser cache
3. Try http://localhost:5000/swagger
4. Check console for errors
```

---

## **?? ADVANTAGES OF SWAGGER OVER POSTMAN**

| Feature | Swagger | Postman |
|---------|---------|---------|
| Built-in | ? Yes | ? Separate app |
| Setup | ? Zero setup | ? Import collection |
| Documentation | ? Auto-generated | ? Manual |
| Authentication | ? One-click | ? Manual headers |
| Testing | ? In-browser | ? Separate window |
| Updates | ? Auto-sync | ? Manual update |

---

## **?? QUICK REFERENCE**

### **URLs:**
```
Swagger UI: http://localhost:5000/swagger
API Base:   http://localhost:5000/api
```

### **Default Credentials:**
```
Admin:
  Email:    admin@motionrobotics.com
  Password: Admin@123

Teacher (after creation):
  Email:    teacher1@school.com
  Password: Teacher@123

Student (after creation):
  Email:    student1@school.com
  Password: Student@123
```

### **Authorization Format:**
```
Bearer {your_token_here}
```

---

## **?? YOU'RE READY!**

1. **Start application:** `dotnet run`
2. **Open Swagger:** `http://localhost:5000/swagger`
3. **Login as Admin** and get token
4. **Authorize** in Swagger
5. **Start testing!**

**No Postman needed - everything is in Swagger! ??**

