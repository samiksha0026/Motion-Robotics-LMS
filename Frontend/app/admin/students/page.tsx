"use client";

import React, { useEffect, useState } from 'react';
import { fetchWithAuth, getRole, getSchoolContext } from '../../../lib/auth';
import { Users, Plus, Loader, Search, Edit, Trash2, Mail, BookOpen, Award, School, AlertCircle, Hash } from 'lucide-react';

interface SchoolData {
  id: number;
  schoolName: string;
}

interface ClassData {
  id: number;
  name?: string;
  className?: string;
  schoolId: number;
  roboticsLevelId?: number;
  roboticsLevelName?: string;
  roboticsLevelNumber?: number;
}

interface StudentData {
  id: number;
  email: string;
  fullName: string;
  rollNo: string;
  classId: number;
  className: string;
  schoolId: number;
  schoolName: string;
  programName: string;
  programLevel: number;
}

// The 6 Fixed Robotics Programs mapped to levels
const PROGRAMS = [
  'Mech Tech',
  'Electronics', 
  'Electro Mechanical',
  'Digi-Coding',
  'Digi-Sense',
  'Wireless & IOT'
];

// Map robotics level to program (Level 1 -> Mech Tech, Level 2 -> Electronics, etc.
function getProgramForLevel(levelNumber?: number): string {
  if (!levelNumber || levelNumber < 1 || levelNumber > 6) {
    return PROGRAMS[0]; // Default to first program
  }
  return PROGRAMS[levelNumber - 1];
}

export default function AdminStudents() {
  const [students, setStudents] = useState<StudentData[]>([]);
  const [schools, setSchools] = useState<SchoolData[]>([]);
  const [classes, setClasses] = useState<ClassData[]>([]);
  const [loading, setLoading] = useState(true);
  const [adding, setAdding] = useState(false);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [showForm, setShowForm] = useState(false);
  
  // Role-based context
  const [userRole, setUserRole] = useState<string | null>(null);
  const [userSchool, setUserSchool] = useState<{ id: number; schoolName: string } | null>(null);
  
  // Form fields matching backend DTO
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rollNo, setRollNo] = useState('');
  const [schoolId, setSchoolId] = useState<number | ''>('');
  const [classId, setClassId] = useState<number | ''>('');
  const [programName, setProgramName] = useState('');

  // Initialize role context
  useEffect(() => {
    const role = getRole();
    setUserRole(role);
    
    if (role === 'SchoolAdmin') {
      const schoolContext = getSchoolContext();
      if (schoolContext) {
        setUserSchool({ id: schoolContext.id, schoolName: schoolContext.schoolName });
        setSchoolId(schoolContext.id);
      }
    }
  }, []);

  async function loadStudents() {
    try {
      setLoading(true);
      // SchoolAdmin: filter by their school
      const endpoint = userRole === 'SchoolAdmin' && userSchool 
        ? `/api/admin/students/school/${userSchool.id}`
        : '/api/admin/students';
      const data = await fetchWithAuth(endpoint);
      const studentsArray = Array.isArray(data) ? data : [];
      setStudents(studentsArray);
      setError('');
    } catch (err) {
      console.error(err);
      setError('Failed to load students. Make sure backend is running.');
    } finally {
      setLoading(false);
    }
  }

  async function loadSchools() {
    // SchoolAdmin doesn't need to load schools list
    if (userRole === 'SchoolAdmin') return;
    
    try {
      const data = await fetchWithAuth('/api/admin/schools');
      const schoolsArray = Array.isArray(data) ? data : [];
      setSchools(schoolsArray);
    } catch (err) {
      console.error(err);
    }
  }

  async function loadClasses(selectedSchoolId?: number) {
    try {
      const effectiveSchoolId = selectedSchoolId || (userRole === 'SchoolAdmin' && userSchool ? userSchool.id : undefined);
      const endpoint = effectiveSchoolId 
        ? `/api/admin/classes/school/${effectiveSchoolId}` 
        : '/api/admin/classes';
      const data = await fetchWithAuth(endpoint);
      const classesArray = Array.isArray(data) ? data : [];
      setClasses(classesArray);
    } catch (err) {
      console.error(err);
    }
  }

  useEffect(() => {
    if (userRole) {
      loadStudents();
      loadSchools();
      // Load classes for SchoolAdmin's school automatically
      if (userRole === 'SchoolAdmin' && userSchool) {
        loadClasses(userSchool.id);
      } else {
        loadClasses();
      }
    }
  }, [userRole, userSchool]);

  useEffect(() => {
    // Only trigger when SuperAdmin changes school
    if (userRole !== 'SchoolAdmin' && schoolId) {
      loadClasses(Number(schoolId));
      setClassId(''); // Reset class when school changes
    }
  }, [schoolId]);

  function resetForm() {
    setFullName('');
    setEmail('');
    setPassword('');
    setRollNo('');
    // Preserve schoolId for SchoolAdmin
    if (userRole !== 'SchoolAdmin') {
      setSchoolId('');
    }
    setClassId('');
    setProgramName(''); // Keep for state cleanup
    setShowForm(false);
  }

  async function handleAdd() {
    // For SchoolAdmin, use their school
    const effectiveSchoolId = userRole === 'SchoolAdmin' && userSchool 
      ? userSchool.id 
      : schoolId;
    
    if (!fullName.trim() || !email.trim() || !password.trim() || !rollNo.trim() || 
        !effectiveSchoolId || !classId) {
      setError('All fields are required');
      return;
    }

    // Find the selected class to get its robotics level
    const selectedClass = classes.find(c => c.id === Number(classId));
    if (!selectedClass) {
      setError('Selected class not found');
      return;
    }

    // Check if class has assigned level
    if (!selectedClass.roboticsLevelNumber) {
      setError('Class must have a robotics level assigned before adding students');
      return;
    }

    // Auto-assign program based on class level
    const autoProgram = getProgramForLevel(selectedClass.roboticsLevelNumber);

    setAdding(true);
    setError('');
    try {
      await fetchWithAuth('/api/admin/students', {
        method: 'POST',
        body: JSON.stringify({ 
          fullName, 
          email, 
          password,
          rollNo,
          schoolId: Number(effectiveSchoolId),
          classId: Number(classId),
          programName: autoProgram,
          programLevel: 1  // Default to level 1, teacher will assign actual level
        }),
      });
      resetForm();
      loadStudents();
    } catch (err: any) {
      console.error(err);
      setError(err.message || 'Could not add student');
    } finally {
      setAdding(false);
    }
  }

  async function handleDelete(id: number) {
    if (!confirm('Are you sure you want to delete this student?')) return;
    try {
      await fetchWithAuth(`/api/admin/students/${id}`, { method: 'DELETE' });
      loadStudents();
    } catch (err) {
      console.error(err);
      setError('Could not delete student');
    }
  }

  const filteredStudents = students.filter((student) =>
    student.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    student.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    student.rollNo?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // For SchoolAdmin, use all classes (already filtered by API)
  // For SuperAdmin, filter by selected school
  const filteredClasses = userRole === 'SchoolAdmin' 
    ? classes 
    : classes.filter(c => !schoolId || c.schoolId === Number(schoolId));

  return (
    <div className="max-w-7xl mx-auto">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-lg flex items-center justify-center">
              <Users className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Students</h1>
              <p className="text-sm text-gray-600">Manage student accounts and enrollments</p>
            </div>
          </div>
          {userRole === 'SchoolAdmin' && (
            <button
              onClick={() => setShowForm(!showForm)}
              className="flex items-center gap-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-4 py-2 rounded-lg hover:from-blue-700 hover:to-indigo-700 transition-all"
            >
              <Plus className="w-4 h-4" />
              Add Student
            </button>
          )}
        </div>

        {/* Add Student Form */}
        {userRole === 'SchoolAdmin' && showForm && (
          <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg p-6 border border-blue-100">
            <h3 className="text-sm font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Plus className="w-4 h-4" />
              Add New Student
            </h3>
            
            {error && (
              <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
                <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                <p className="text-sm text-red-700">{error}</p>
              </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Full Name *</label>
                <input
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  placeholder="e.g., John Doe"
                  className="w-full border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Email *</label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="e.g., john@school.com"
                  className="w-full border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Password *</label>
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Min 6 characters"
                  className="w-full border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Roll Number *</label>
                <input
                  value={rollNo}
                  onChange={(e) => setRollNo(e.target.value)}
                  placeholder="e.g., STU001"
                  className="w-full border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">School *</label>
                {userRole === 'SchoolAdmin' && userSchool ? (
                  <div className="w-full border border-gray-200 bg-gray-50 rounded-lg px-4 py-2 text-gray-700">
                    {userSchool.schoolName}
                  </div>
                ) : (
                  <select
                    value={schoolId}
                    onChange={(e) => setSchoolId(e.target.value ? Number(e.target.value) : '')}
                    className="w-full border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Select a school</option>
                    {schools.map((school) => (
                      <option key={school.id} value={school.id}>
                        {school.schoolName}
                      </option>
                    ))}
                  </select>
                )}
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Class *</label>
                <select
                  value={classId}
                  onChange={(e) => setClassId(e.target.value ? Number(e.target.value) : '')}
                  disabled={userRole !== 'SchoolAdmin' && !schoolId}
                  className="w-full border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100"
                >
                  <option value="">{(userRole === 'SchoolAdmin' || schoolId) ? 'Select a class' : 'Select school first'}</option>
                  {filteredClasses.map((cls) => (
                    <option key={cls.id} value={cls.id}>
                      {cls.className || cls.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            
            <div className="mb-4">
              <div className="flex items-start gap-2 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                <div className="text-blue-600 flex-shrink-0 mt-0.5">
                  <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="text-sm text-blue-800">
                  <p className="font-medium mb-1">Program Auto-Assignment</p>
                  <p>The robotics program will be automatically assigned based on the class's robotics level. Make sure the class has a level assigned first.</p>
                </div>
              </div>
            </div>

            <div className="flex gap-3">
              <button
                onClick={handleAdd}
                disabled={adding}
                className="flex items-center justify-center gap-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-6 py-2 rounded-lg hover:from-blue-700 hover:to-indigo-700 disabled:opacity-50 transition-all"
              >
                {adding ? (
                  <>
                    <Loader className="w-4 h-4 animate-spin" />
                    Adding...
                  </>
                ) : (
                  <>
                    <Plus className="w-4 h-4" />
                    Add Student
                  </>
                )}
              </button>
              <button
                onClick={resetForm}
                className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-all"
              >
                Cancel
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Students</p>
              <p className="text-2xl font-bold text-gray-900">{students.length}</p>
            </div>
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <Users className="w-6 h-6 text-blue-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Schools</p>
              <p className="text-2xl font-bold text-gray-900">
                {new Set(students.map(s => s.schoolId)).size}
              </p>
            </div>
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <School className="w-6 h-6 text-green-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Programs</p>
              <p className="text-2xl font-bold text-gray-900">
                {new Set(students.map(s => s.programName)).size}
              </p>
            </div>
            <div className="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center">
              <BookOpen className="w-6 h-6 text-indigo-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Classes</p>
              <p className="text-2xl font-bold text-gray-900">
                {new Set(students.map(s => s.classId)).size}
              </p>
            </div>
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
              <Award className="w-6 h-6 text-purple-600" />
            </div>
          </div>
        </div>
      </div>

      {/* Search and Students List */}
      <div className="bg-white rounded-xl shadow-sm overflow-hidden">
        <div className="p-4 border-b border-gray-100">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search students by name, email, or roll number..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        {loading ? (
          <div className="flex items-center justify-center py-12">
            <Loader className="w-8 h-8 text-blue-600 animate-spin" />
          </div>
        ) : filteredStudents.length === 0 ? (
          <div className="text-center py-12">
            <Users className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500">
              {searchTerm ? 'No students found matching your search' : 'No students registered yet'}
            </p>
            {!searchTerm && schools.length === 0 && (
              <p className="text-sm text-gray-400 mt-2">Add schools and classes first before adding students</p>
            )}
          </div>
        ) : (
          <div className="divide-y divide-gray-100">
            {filteredStudents.map((s) => (
              <div
                key={s.id}
                className="p-4 hover:bg-gray-50 transition-colors flex items-center justify-between"
              >
                <div className="flex items-center gap-4 flex-1">
                  <div className="w-12 h-12 bg-gradient-to-br from-blue-100 to-indigo-100 rounded-full flex items-center justify-center">
                    <span className="text-blue-600 font-semibold text-lg">
                      {s.fullName?.charAt(0).toUpperCase()}
                    </span>
                  </div>
                  <div className="flex-1">
                    <p className="font-semibold text-gray-900">{s.fullName}</p>
                    <div className="flex flex-wrap items-center gap-4 mt-1 text-sm text-gray-500">
                      <span className="flex items-center gap-1">
                        <Hash className="w-3 h-3" />
                        {s.rollNo}
                      </span>
                      <span className="flex items-center gap-1">
                        <Mail className="w-3 h-3" />
                        {s.email}
                      </span>
                      <span className="flex items-center gap-1">
                        <School className="w-3 h-3" />
                        {s.schoolName}
                      </span>
                    </div>
                    <div className="flex items-center gap-2 mt-2">
                      <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded">
                        {s.className}
                      </span>
                      <span className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded">
                        {s.programName}
                      </span>
                      <span className="text-xs bg-purple-100 text-purple-700 px-2 py-0.5 rounded">
                        Level {s.programLevel}
                      </span>
                    </div>
                  </div>
                </div>

                {userRole === 'SchoolAdmin' && (
                  <div className="flex items-center gap-2">
                    <button
                      className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                      title="Edit Student"
                    >
                      <Edit className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleDelete(s.id)}
                      className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                      title="Delete Student"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}