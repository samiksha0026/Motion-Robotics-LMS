"use client";

import React, { useEffect, useState } from 'react';
import { fetchWithAuth, getRole, getSchoolContext, isSuperAdmin, isSchoolAdmin } from '../../../lib/auth';
import { GraduationCap, Plus, Loader, Search, Edit, Trash2, Mail, BookOpen, Users, Phone, School, AlertCircle, Lock } from 'lucide-react';

interface SchoolData {
  id: number;
  schoolName: string;
}

interface ClassData {
  id: number;
  className: string;
  schoolId: number;
}

interface TeacherData {
  id: number;
  email: string;
  fullName: string;
  phoneNumber: string;
  schoolId: number;
  schoolName: string;
  classes?: { id: number; name: string }[];
}

export default function AdminTeachers() {
  const [teachers, setTeachers] = useState<TeacherData[]>([]);
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
  const [phoneNumber, setPhoneNumber] = useState('');
  const [schoolId, setSchoolId] = useState<number | ''>('');
  const [selectedClassIds, setSelectedClassIds] = useState<number[]>([]);

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

  async function loadTeachers() {
    try {
      setLoading(true);
      // SchoolAdmin: filter by their school, SuperAdmin: all teachers
      const endpoint = userRole === 'SchoolAdmin' && userSchool 
        ? `/api/admin/teachers/school/${userSchool.id}`
        : '/api/admin/teachers';
      const data = await fetchWithAuth(endpoint);
      const teachersArray = Array.isArray(data) ? data : [];
      setTeachers(teachersArray);
      setError('');
    } catch (err) {
      console.error(err);
      setError('Failed to load teachers. Make sure backend is running.');
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

  async function loadClasses() {
    try {
      // SchoolAdmin: filter by school
      const endpoint = userRole === 'SchoolAdmin' && userSchool
        ? `/api/admin/classes/school/${userSchool.id}`
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
      loadTeachers();
      loadSchools();
      loadClasses();
    }
  }, [userRole, userSchool]);

  function resetForm() {
    setFullName('');
    setEmail('');
    setPassword('');
    setPhoneNumber('');
    setSelectedClassIds([]);
    // Preserve schoolId for SchoolAdmin
    if (userRole !== 'SchoolAdmin') {
      setSchoolId('');
    }
    setShowForm(false);
  }

  async function handleAdd() {
    // For SchoolAdmin, use their school; for SuperAdmin, require selection
    const effectiveSchoolId = userRole === 'SchoolAdmin' && userSchool 
      ? userSchool.id 
      : schoolId;
    
    if (!fullName.trim() || !email.trim() || !password.trim() || !phoneNumber.trim() || !effectiveSchoolId) {
      setError('All fields are required');
      return;
    }

    if (selectedClassIds.length === 0) {
      setError('Please assign at least one class to the teacher');
      return;
    }

    setAdding(true);
    setError('');
    try {
      await fetchWithAuth('/api/admin/teachers', {
        method: 'POST',
        body: JSON.stringify({ 
          fullName, 
          email, 
          password,
          phoneNumber, 
          schoolId: Number(effectiveSchoolId),
          classIds: selectedClassIds
        }),
      });
      resetForm();
      loadTeachers();
    } catch (err: any) {
      console.error(err);
      setError(err.message || 'Could not add teacher');
    } finally {
      setAdding(false);
    }
  }

  async function handleDelete(id: number) {
    if (!confirm('Are you sure you want to delete this teacher?')) return;
    try {
      await fetchWithAuth(`/api/admin/teachers/${id}`, { method: 'DELETE' });
      loadTeachers();
    } catch (err) {
      console.error(err);
      setError('Could not delete teacher');
    }
  }

  const filteredTeachers = teachers.filter((teacher) =>
    teacher.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    teacher.email?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="max-w-7xl mx-auto">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-lg flex items-center justify-center">
              <GraduationCap className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Teachers</h1>
              <p className="text-sm text-gray-600">Manage instructor accounts and assignments</p>
            </div>
          </div>
          {userRole === 'SchoolAdmin' && (
            <button
              onClick={() => setShowForm(!showForm)}
              className="flex items-center gap-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-4 py-2 rounded-lg hover:from-blue-700 hover:to-indigo-700 transition-all"
            >
              <Plus className="w-4 h-4" />
              Add Teacher
            </button>
          )}
        </div>

        {/* Add Teacher Form */}
        {userRole === 'SchoolAdmin' && showForm && (
          <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg p-6 border border-blue-100">
            <h3 className="text-sm font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Plus className="w-4 h-4" />
              Add New Teacher
            </h3>
            
            {error && (
              <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
                <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                <p className="text-sm text-red-700">{error}</p>
              </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Full Name *</label>
                <input
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  placeholder="e.g., Dr. Jane Smith"
                  className="w-full border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Email *</label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="e.g., jane@school.com"
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
                <label className="block text-sm font-medium text-gray-700 mb-1">Phone Number *</label>
                <input
                  value={phoneNumber}
                  onChange={(e) => setPhoneNumber(e.target.value)}
                  placeholder="e.g., +1 234 567 8900"
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
            </div>

            {/* Class Assignment - shows classes from selected school */}
            {((userRole === 'SchoolAdmin' && userSchool) || schoolId) && (
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Assign Classes * <span className="text-gray-500 font-normal">(Select classes this teacher will manage)</span>
                </label>
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-2 p-4 border border-gray-200 rounded-lg bg-white max-h-48 overflow-y-auto">
                  {classes
                    .filter(c => userRole === 'SchoolAdmin' ? true : c.schoolId === Number(schoolId))
                    .map((cls) => (
                      <label key={cls.id} className="flex items-center gap-2 cursor-pointer hover:bg-gray-50 p-2 rounded">
                        <input
                          type="checkbox"
                          checked={selectedClassIds.includes(cls.id)}
                          onChange={(e) => {
                            if (e.target.checked) {
                              setSelectedClassIds([...selectedClassIds, cls.id]);
                            } else {
                              setSelectedClassIds(selectedClassIds.filter(id => id !== cls.id));
                            }
                          }}
                          className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                        />
                        <span className="text-sm text-gray-700">{cls.className}</span>
                      </label>
                    ))}
                  {classes.filter(c => userRole === 'SchoolAdmin' ? true : c.schoolId === Number(schoolId)).length === 0 && (
                    <p className="text-sm text-gray-500 col-span-full">No classes found. Create classes first.</p>
                  )}
                </div>
                {selectedClassIds.length > 0 && (
                  <p className="text-sm text-green-600 mt-1">{selectedClassIds.length} class(es) selected</p>
                )}
              </div>
            )}

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
                    Add Teacher
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
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Teachers</p>
              <p className="text-2xl font-bold text-gray-900">{teachers.length}</p>
            </div>
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <GraduationCap className="w-6 h-6 text-blue-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Schools Covered</p>
              <p className="text-2xl font-bold text-gray-900">
                {new Set(teachers.map(t => t.schoolId)).size}
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
              <p className="text-sm text-gray-600">Classes Assigned</p>
              <p className="text-2xl font-bold text-gray-900">
                {teachers.reduce((sum, t) => sum + (t.classes?.length || 0), 0)}
              </p>
            </div>
            <div className="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center">
              <BookOpen className="w-6 h-6 text-indigo-600" />
            </div>
          </div>
        </div>
      </div>

      {/* Search and Teachers List */}
      <div className="bg-white rounded-xl shadow-sm overflow-hidden">
        <div className="p-4 border-b border-gray-100">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search teachers by name or email..."
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
        ) : filteredTeachers.length === 0 ? (
          <div className="text-center py-12">
            <GraduationCap className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500">
              {searchTerm ? 'No teachers found matching your search' : 'No teachers registered yet'}
            </p>
            {!searchTerm && schools.length === 0 && (
              <p className="text-sm text-gray-400 mt-2">Add a school first before adding teachers</p>
            )}
          </div>
        ) : (
          <div className="divide-y divide-gray-100">
            {filteredTeachers.map((t) => (
              <div
                key={t.id}
                className="p-4 hover:bg-gray-50 transition-colors flex items-center justify-between"
              >
                <div className="flex items-center gap-4 flex-1">
                  <div className="w-12 h-12 bg-gradient-to-br from-blue-100 to-indigo-100 rounded-full flex items-center justify-center">
                    <span className="text-blue-600 font-semibold text-lg">
                      {t.fullName?.charAt(0).toUpperCase()}
                    </span>
                  </div>
                  <div className="flex-1">
                    <p className="font-semibold text-gray-900">{t.fullName}</p>
                    <div className="flex flex-wrap items-center gap-4 mt-1 text-sm text-gray-500">
                      <span className="flex items-center gap-1">
                        <Mail className="w-3 h-3" />
                        {t.email}
                      </span>
                      <span className="flex items-center gap-1">
                        <Phone className="w-3 h-3" />
                        {t.phoneNumber}
                      </span>
                      <span className="flex items-center gap-1">
                        <School className="w-3 h-3" />
                        {t.schoolName}
                      </span>
                    </div>
                    {t.classes && t.classes.length > 0 && (
                      <div className="flex items-center gap-2 mt-2">
                        <span className="text-xs text-gray-500">Classes:</span>
                        {t.classes.map((c) => (
                          <span key={c.id} className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded">
                            {c.name}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                </div>

                {userRole === 'SchoolAdmin' && (
                  <div className="flex items-center gap-2">
                    <button
                      className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                      title="Edit Teacher"
                    >
                      <Edit className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleDelete(t.id)}
                      className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                      title="Delete Teacher"
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