# **ADMIN DASHBOARD - COMPLETE OPERATIONS GUIDE**

## **??? ADMIN PANEL OPERATIONS**

After logging in as Admin, here are all the operations you can perform:

---

## **1. SCHOOLS MANAGEMENT**

### **Create School**
```http
POST /api/admin/schools
Authorization: Bearer {admin_token}

{
  "schoolName": "Your School Name",
  "address": "Full Address",
  "phoneNumber": "+91-XXXXXXXXXX",
  "email": "school@email.com"
}
```
**Response:** School created with ID

---

### **View All Schools**
```http
GET /api/admin/schools
Authorization: Bearer {admin_token}
```
**Response:** List of all schools with student/class counts

---

### **View Single School**
```http
GET /api/admin/schools/{schoolId}
Authorization: Bearer {admin_token}
```
**Response:** School details with all classes and students

---

### **Update School**
```http
PUT /api/admin/schools/{schoolId}
Authorization: Bearer {admin_token}

{
  "schoolName": "Updated Name",
  "address": "New Address",
  "phoneNumber": "+91-NEW-NUMBER",
  "email": "newemail@school.com"
}
```

---

### **Delete School**
```http
DELETE /api/admin/schools/{schoolId}
Authorization: Bearer {admin_token}
```
**Note:** Deletes all related classes (cascading)

---

## **2. CLASSES MANAGEMENT**

### **Create Class**
```http
POST /api/admin/classes
Authorization: Bearer {admin_token}

{
  "className": "Class 6-A",
  "schoolId": 1
}
```
**Response:** Class created with ID

---

### **View All Classes**
```http
GET /api/admin/classes
Authorization: Bearer {admin_token}
```
**Response:** List of all classes

---

### **View Classes by School**
```http
GET /api/admin/classes/school/{schoolId}
Authorization: Bearer {admin_token}
```
**Response:** All classes in that school

---

### **Update Class**
```http
PUT /api/admin/classes/{classId}
Authorization: Bearer {admin_token}

{
  "className": "Class 7-A",
  "schoolId": 1
}
```

---

### **Delete Class**
```http
DELETE /api/admin/classes/{classId}
Authorization: Bearer {admin_token}
```

---

## **3. TEACHERS MANAGEMENT**

### **Create Teacher**
```http
POST /api/admin/teachers
Authorization: Bearer {admin_token}

{
  "email": "teacher@school.com",
  "fullName": "Full Name",
  "password": "SecurePassword123!",
  "phoneNumber": "+91-XXXXXXXXXX",
  "schoolId": 1,
  "classIds": [1, 2, 3]  // Classes to assign
}
```
**Response:** Teacher created with ID and assigned classes

---

### **View All Teachers**
```http
GET /api/admin/teachers
Authorization: Bearer {admin_token}
```
**Response:** List of all teachers with their classes

---

### **View Teachers by School**
```http
GET /api/admin/teachers/school/{schoolId}
Authorization: Bearer {admin_token}
```
**Response:** All teachers in that school

---

### **View Single Teacher**
```http
GET /api/admin/teachers/{teacherId}
Authorization: Bearer {admin_token}
```
**Response:** Teacher details with all assigned classes

---

### **Update Teacher**
```http
PUT /api/admin/teachers/{teacherId}
Authorization: Bearer {admin_token}

{
  "email": "newemail@school.com",
  "fullName": "Updated Name",
  "password": "NewPassword123!",
  "phoneNumber": "+91-NEW-NUMBER",
  "schoolId": 1,
  "classIds": [1, 2]
}
```

---

### **Assign Additional Class to Teacher**
```http
POST /api/admin/teachers/{teacherId}/classes/{classId}
Authorization: Bearer {admin_token}
```
**Response:** Class assigned successfully

---

### **Remove Class from Teacher**
```http
DELETE /api/admin/teachers/{teacherId}/classes/{classId}
Authorization: Bearer {admin_token}
```
**Response:** Class removed successfully

---

### **Delete Teacher**
```http
DELETE /api/admin/teachers/{teacherId}
Authorization: Bearer {admin_token}
```
**Note:** Deletes teacher and associated attendance records

---

## **4. STUDENTS MANAGEMENT**

### **Create Student**
```http
POST /api/admin/students
Authorization: Bearer {admin_token}

{
  "email": "student@school.com",
  "fullName": "Student Full Name",
  "password": "StudentPassword123!",
  "rollNo": "001",
  "classId": 1,
  "schoolId": 1,
  "programName": "Digi-Coding",
  "programLevel": 1
}
```
**Response:** Student created with ID, IdentityUser also created

---

### **View All Students**
```http
GET /api/admin/students
Authorization: Bearer {admin_token}
```
**Response:** List of all students

---

### **View Students by School**
```http
GET /api/admin/students/school/{schoolId}
Authorization: Bearer {admin_token}
```
**Response:** All students in that school

---

### **View Students by Class**
```http
GET /api/admin/students/class/{classId}
Authorization: Bearer {admin_token}
```
**Response:** All students in that class with class name and school name

---

### **View Single Student**
```http
GET /api/admin/students/{studentId}
Authorization: Bearer {admin_token}
```
**Response:** Student details with school and class information

---

### **Delete Student**
```http
DELETE /api/admin/students/{studentId}
Authorization: Bearer {admin_token}
```
**Note:** Deletes student and associated IdentityUser

---

## **5. DASHBOARD OVERVIEW**

### **Quick Overview Endpoints**

**All Schools Summary**
```http
GET /api/admin/schools
```
Shows:
- School names
- Number of classes per school
- Number of students per school

---

**All Classes Summary**
```http
GET /api/admin/classes
```
Shows:
- Class names
- School names
- Number of students per class

---

**All Teachers Summary**
```http
GET /api/admin/teachers
```
Shows:
- Teacher names
- School assignments
- Classes each teacher teaches

---

**All Students Summary**
```http
GET /api/admin/students
```
Shows:
- Student names
- Roll numbers
- School and class assignments
- Programs and levels

---

## **6. COMMON WORKFLOWS**

### **Workflow 1: Set Up New School (5 steps)**

1. **Create School**
   ```
   POST /api/admin/schools
   ```

2. **Create Classes** (for each class)
   ```
   POST /api/admin/classes ? Repeat for Class A, B, C, etc.
   ```

3. **Create Teachers** (for each teacher)
   ```
   POST /api/admin/teachers ? Assign multiple classes
   ```

4. **Create Students** (for each student)
   ```
   POST /api/admin/students ? Assign to class and program
   ```

5. **Verify** (check all created)
   ```
   GET /api/admin/students/school/{schoolId}
   GET /api/admin/teachers/school/{schoolId}
   ```

---

### **Workflow 2: Add New Class to Existing School**

1. **Create Class**
   ```
   POST /api/admin/classes
   ```

2. **Assign Teacher to Class** (if new)
   ```
   POST /api/admin/teachers/{teacherId}/classes/{classId}
   ```

3. **Add Students to Class** (for each student)
   ```
   POST /api/admin/students
   ```

---

### **Workflow 3: Transfer Student to Different Class**

1. **Get Student**
   ```
   GET /api/admin/students/{studentId}
   ```

2. **Update Student** (change classId)
   ```
   PUT /api/admin/students/{studentId}
   ```

---

### **Workflow 4: Remove Teacher from School**

1. **Remove all class assignments**
   ```
   DELETE /api/admin/teachers/{teacherId}/classes/{classId}
   (repeat for each class)
   ```

2. **Delete Teacher**
   ```
   DELETE /api/admin/teachers/{teacherId}
   ```

---

## **7. BULK OPERATIONS**

### **Export All Data (via GET requests)**

```bash
# Export all schools
curl -H "Authorization: Bearer {token}" \
  http://localhost:5000/api/admin/schools

# Export all classes
curl -H "Authorization: Bearer {token}" \
  http://localhost:5000/api/admin/classes

# Export all teachers
curl -H "Authorization: Bearer {token}" \
  http://localhost:5000/api/admin/teachers

# Export all students
curl -H "Authorization: Bearer {token}" \
  http://localhost:5000/api/admin/students
```

---

## **8. ERROR HANDLING**

### **Common Errors & Solutions**

**400 Bad Request** - Invalid data
```
Solution: Check required fields and data format
```

**401 Unauthorized** - Invalid or expired token
```
Solution: Login again to get new token
```

**404 Not Found** - Resource doesn't exist
```
Solution: Verify ID exists before updating/deleting
```

**409 Conflict** - Email already exists
```
Solution: Use unique email for each user
```

---

## **9. ADMIN PERMISSIONS**

? **Can Do:**
- Create/Read/Update/Delete Schools
- Create/Read/Update/Delete Classes
- Create/Read/Update/Delete Teachers
- Assign/Remove Classes to/from Teachers
- Create/Read/Delete Students
- View all attendance records
- View all dashboards

? **Cannot Do:**
- Record attendance (only teachers can)
- Complete experiments (only students can)
- Submit exams (only students can)

---

## **10. MONITORING & REPORTING**

### **View School Statistics**
```http
GET /api/admin/schools
```
Each school shows:
- `studentCount` - Total students
- `classCount` - Total classes

---

### **View Class Statistics**
```http
GET /api/admin/classes/school/{schoolId}
```
Each class shows:
- `studentCount` - Students in this class

---

### **View Teacher Assignments**
```http
GET /api/admin/teachers/{teacherId}
```
Shows:
- All classes teacher is assigned to
- Can manage class assignments

---

### **View Student Progress**
```http
GET /api/admin/students/{studentId}
```
Shows:
- Program name and level
- School and class assignment
- Creation date

---

## **ADMIN RESPONSIBILITIES CHECKLIST**

- [ ] Create schools
- [ ] Create classes for each school
- [ ] Create teachers and assign to classes
- [ ] Create students and assign to classes and programs
- [ ] Monitor student enrollment
- [ ] Verify teacher assignments
- [ ] Manage role-based access
- [ ] Handle user creation/deletion
- [ ] Oversee program assignments
- [ ] Maintain data integrity

---

## **NEXT: Teacher and Student Operations**

After setting up schools, classes, teachers, and students, you can:

1. **Teachers can:**
   - Login
   - Record attendance
   - View class and student attendance

2. **Students can:**
   - Login
   - View dashboard
   - Complete experiments
   - Submit exams
   - View certificates

See `COMPLETE_FLOW_GUIDE.md` for teacher and student workflows.

