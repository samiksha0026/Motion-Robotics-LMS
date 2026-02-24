# **MOTION ROBOTICS LMS - QUICK START GUIDE**

## **?? 5-MINUTE SETUP**

### **Prerequisites**
- .NET 8 SDK installed
- SQL Server installed
- Postman or Thunder Client (for API testing)
- Visual Studio Code or Visual Studio

---

## **STEP 1: Database Migration (2 minutes)**

```bash
# Navigate to project directory
cd F:\MotionRobotics.LMS\MotionRobotics.LMS.API

# Create and apply migration
dotnet ef database update
```

? Database is ready with all tables!

---

## **STEP 2: Start Application (1 minute)**

```bash
# Start the API
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

? API is running on `http://localhost:5000`

---

## **STEP 3: Import Postman Collection (1 minute)**

### **Option A: Import Directly**
1. Open Postman
2. Click **File** ? **Import**
3. Select `Postman_Collection.json` from project
4. Click **Import**

### **Option B: Manual Setup**
- Set `base_url` = `http://localhost:5000/api`
- Create folders for: Admin Setup, Auth, Schools, Classes, Teachers, Students, Attendance

---

## **STEP 4: Create Admin (30 seconds)**

### **In Postman:**

1. **Check if admin exists**
   ```
   GET http://localhost:5000/api/admin/setup/check-admin-exists
   ```

2. **Create First Admin**
   ```
   POST http://localhost:5000/api/admin/setup/create-first-admin
   
   {
     "email": "admin@motionrobotics.com",
     "password": "Admin@123456"
   }
   ```

? Save credentials:
- **Email:** admin@motionrobotics.com
- **Password:** Admin@123456

---

## **STEP 5: Admin Login (30 seconds)**

```
POST http://localhost:5000/api/admin/auth/login

{
  "email": "admin@motionrobotics.com",
  "password": "Admin@123456"
}
```

? **Copy the token and save it** as `{ADMIN_TOKEN}`

In Postman: Set environment variable:
```
admin_token = {copied_token}
```

---

## **COMPLETE CORE FLOW IN 5 MINUTES**

### **Phase 1: Create School (30 sec)**
```
POST {{base_url}}/admin/schools
Authorization: Bearer {{admin_token}}

{
  "schoolName": "Delhi Public School",
  "address": "123 Main Street, Delhi",
  "phoneNumber": "+91-9876543210",
  "email": "info@dps.edu"
}
```
? Copy `school_id`

---

### **Phase 2: Create Classes (30 sec)**
```
POST {{base_url}}/admin/classes
Authorization: Bearer {{admin_token}}

{
  "className": "Class 6-A",
  "schoolId": {{school_id}}
}
```
? Copy `class_id_1`

Repeat for "Class 6-B" ? Copy `class_id_2`

---

### **Phase 3: Create Teacher (30 sec)**
```
POST {{base_url}}/admin/teachers
Authorization: Bearer {{admin_token}}

{
  "email": "teacher1@school.com",
  "fullName": "Mrs. Jane Smith",
  "password": "Teacher@123456",
  "phoneNumber": "+91-9876543211",
  "schoolId": {{school_id}},
  "classIds": [{{class_id_1}}, {{class_id_2}}]
}
```
? Save teacher credentials

---

### **Phase 4: Create Students (1 min)**
```
POST {{base_url}}/admin/students
Authorization: Bearer {{admin_token}}

{
  "email": "student1@school.com",
  "fullName": "Aarav Sharma",
  "password": "Student@123456",
  "rollNo": "001",
  "classId": {{class_id_1}},
  "schoolId": {{school_id}},
  "programName": "Digi-Coding",
  "programLevel": 1
}
```

Repeat for Student 2 with different email/name

---

### **Phase 5: Teacher Records Attendance (1 min)**

**Teacher Login:**
```
POST {{base_url}}/teacher/auth/login

{
  "email": "teacher1@school.com",
  "password": "Teacher@123456"
}
```
? Copy `teacher_token`

**Record Attendance:**
```
POST {{base_url}}/teacher/attendance/record
Authorization: Bearer {{teacher_token}}

{
  "studentId": {{student_id_1}},
  "classId": {{class_id_1}},
  "attendanceDate": "2025-01-11T00:00:00Z",
  "isPresent": true,
  "remarks": "Present"
}
```

---

### **Phase 6: Student Views Dashboard (30 sec)**

**Student Login:**
```
POST {{base_url}}/auth/login

{
  "email": "student1@school.com",
  "password": "Student@123456"
}
```
? Copy `student_token`

**View Dashboard:**
```
GET {{base_url}}/students/dashboard
Authorization: Bearer {{student_token}}
```

? See student progress, experiments, exams!

---

## **?? CORE FLOW SUMMARY**

```
Admin Setup
    ?
Admin Login ? Token
    ?
Create School
    ?
Create Classes
    ?
Create Teachers (assign to classes)
    ?
Create Students
    ?
Teacher Login & Record Attendance
    ?
Student Login & View Dashboard
```

---

## **?? CREDENTIALS REFERENCE**

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@motionrobotics.com | Admin@123456 |
| Teacher | teacher1@school.com | Teacher@123456 |
| Student 1 | student1@school.com | Student@123456 |
| Student 2 | student2@school.com | Student@123456 |

---

## **? VERIFICATION CHECKLIST**

- [ ] Database migrated successfully
- [ ] API running on localhost:5000
- [ ] Admin account created
- [ ] Admin login works (got token)
- [ ] School created
- [ ] Classes created
- [ ] Teacher created
- [ ] Students created
- [ ] Attendance recorded
- [ ] Student dashboard visible

---

## **?? KEY FILES**

| File | Purpose |
|------|---------|
| `COMPLETE_FLOW_GUIDE.md` | Detailed step-by-step walkthrough |
| `Postman_Collection.json` | Ready-to-use Postman collection |
| `API_DOCUMENTATION.md` | Full API endpoint documentation |
| `IMPLEMENTATION_SUMMARY.md` | What's been implemented |

---

## **?? TROUBLESHOOTING**

### **Issue: Database migration fails**
```
Solution: 
1. Check connection string in appsettings.json
2. Verify SQL Server is running
3. Run: dotnet ef database drop -f
4. Run: dotnet ef database update
```

### **Issue: Admin already exists error**
```
Solution:
GET {{base_url}}/admin/setup/check-admin-exists
If true, use login endpoint instead
```

### **Issue: 401 Unauthorized**
```
Solution:
1. Make sure token is copied correctly
2. Set Authorization header: Bearer {token}
3. Check token expiration (2 hours for admin)
```

### **Issue: Teacher/Student cannot login**
```
Solution:
1. Verify email exists in database
2. Check password is correct
3. Verify user has correct role
```

---

## **?? NEXT STEPS**

1. ? Follow this guide step-by-step
2. ? Test each endpoint
3. ? Verify responses match documentation
4. ? Try different operations (update, delete)
5. ? Build frontend integration

---

## **?? DOCUMENTATION**

- **Complete Flow:** `COMPLETE_FLOW_GUIDE.md`
- **API Docs:** `API_DOCUMENTATION.md`
- **Implementation:** `IMPLEMENTATION_SUMMARY.md`

---

**Ready? Start with:** `Step 1: Database Migration`

