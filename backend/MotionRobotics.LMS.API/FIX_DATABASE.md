# **FIX: Database Migration Required**

## **The Problem:**
Your database is missing the new tables (Schools, Classes, Teachers, Attendances) and the Student table structure is outdated.

## **STEP 1: Open PowerShell/Terminal in the Project Directory**

```bash
cd F:\MotionRobotics.LMS\MotionRobotics.LMS.API
```

## **STEP 2: Add EF Core Tools (if not installed)**

```bash
dotnet tool install --global dotnet-ef
```

## **STEP 3: Create New Migration**

```bash
dotnet ef migrations add UpdateSchemaWithSchoolsTeachersAttendance
```

## **STEP 4: Apply Migration to Database**

```bash
dotnet ef database update
```

## **STEP 5: Run the Application**

```bash
dotnet run
```

**Expected Output:**
```
Admin account created successfully!
   Email: admin@motionrobotics.com
   Password: Admin@123
Database seeding completed!
Now listening on: http://localhost:5000
```

## **STEP 6: Try Login Again**

Open your frontend:
```
http://localhost:3000/admin/login
```

Enter:
- **Email:** admin@motionrobotics.com
- **Password:** Admin@123

## **If Migration Fails (Alternative):**

If you get errors, try dropping and recreating the database:

```bash
# WARNING: This deletes all data!
dotnet ef database drop -f
dotnet ef database update
dotnet run
```

## **Quick Commands Summary:**

```bash
cd F:\MotionRobotics.LMS\MotionRobotics.LMS.API
dotnet ef migrations add UpdateSchemaWithSchoolsTeachersAttendance
dotnet ef database update
dotnet run
```

