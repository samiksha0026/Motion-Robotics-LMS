"use client";

import React, { useEffect, useState } from 'react';
import { fetchWithAuth, getRole, getSchoolContext } from '../../../lib/auth';
import { BookOpen, Plus, Loader, Search, Edit, Trash2, Users, School, AlertCircle, GraduationCap, Check, FlaskConical } from 'lucide-react';

interface SchoolData {
  id: number;
  schoolName: string;
}

interface RoboticsLevel {
  id: number;
  levelNumber: number;
  name: string;
  description?: string;
  experimentCount: number;
}

interface ClassData {
  id: number;
  className: string;
  schoolId: number;
  schoolName: string;
  studentCount?: number;
  roboticsLevelId?: number;
  roboticsLevelName?: string;
  roboticsLevelNumber?: number;
}

export default function AdminClasses() {
  const [classes, setClasses] = useState<ClassData[]>([]);
  const [schools, setSchools] = useState<SchoolData[]>([]);
  const [roboticsLevels, setRoboticsLevels] = useState<RoboticsLevel[]>([]);
  const [loading, setLoading] = useState(true);
  const [adding, setAdding] = useState(false);
  const [assigningLevel, setAssigningLevel] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [showForm, setShowForm] = useState(false);
  
  // Role-based context
  const [userRole, setUserRole] = useState<string | null>(null);
  const [userSchool, setUserSchool] = useState<{ id: number; schoolName: string } | null>(null);
  
  // Form fields matching backend DTO
  const [className, setClassName] = useState('');
  const [schoolId, setSchoolId] = useState<number | ''>('');
  const [selectedLevelId, setSelectedLevelId] = useState<number | ''>('');

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

  async function loadClasses() {
    try {
      setLoading(true);
      // SchoolAdmin: filter by their school
      const endpoint = userRole === 'SchoolAdmin' && userSchool 
        ? `/api/admin/classes/school/${userSchool.id}`
        : '/api/admin/classes';
      const data = await fetchWithAuth(endpoint);
      const classesArray = Array.isArray(data) ? data : [];
      setClasses(classesArray);
      setError('');
    } catch (err) {
      console.error(err);
      setError('Failed to load classes. Make sure backend is running.');
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

  async function loadRoboticsLevels() {
    try {
      const data = await fetchWithAuth('/api/admin/classes/robotics-levels');
      const levelsArray = Array.isArray(data) ? data : [];
      setRoboticsLevels(levelsArray);
    } catch (err) {
      console.error(err);
    }
  }

  useEffect(() => {
    if (userRole) {
      loadClasses();
      loadSchools();
      loadRoboticsLevels();
    }
  }, [userRole, userSchool]);

  function resetForm() {
    setClassName('');
    setSelectedLevelId('');
    // Preserve schoolId for SchoolAdmin
    if (userRole !== 'SchoolAdmin') {
      setSchoolId('');
    }
    setShowForm(false);
  }

  async function handleAdd() {
    // For SchoolAdmin, use their school
    const effectiveSchoolId = userRole === 'SchoolAdmin' && userSchool 
      ? userSchool.id 
      : schoolId;
    
    if (!className.trim() || !effectiveSchoolId) {
      setError('Class name and school are required');
      return;
    }

    setAdding(true);
    setError('');
    try {
      await fetchWithAuth('/api/admin/classes', {
        method: 'POST',
        body: JSON.stringify({ 
          className, 
          schoolId: Number(effectiveSchoolId),
          roboticsLevelId: selectedLevelId ? Number(selectedLevelId) : null
        }),
      });
      resetForm();
      setSuccess('Class created successfully');
      loadClasses();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err: unknown) {
      console.error(err);
      const errorMessage = err instanceof Error ? err.message : 'Could not add class';
      setError(errorMessage);
    } finally {
      setAdding(false);
    }
  }

  async function handleAssignLevel(classId: number, levelId: number) {
    setAssigningLevel(classId);
    setError('');
    setSuccess('');

    try {
      const res = await fetchWithAuth(`/api/admin/classes/${classId}/assign-level`, {
        method: 'POST',
        body: JSON.stringify({ roboticsLevelId: levelId }),
      });
      setSuccess(res.message || 'Level assigned successfully');
      loadClasses();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err: unknown) {
      console.error(err);
      const errorMessage = err instanceof Error ? err.message : 'Could not assign level';
      setError(errorMessage);
    } finally {
      setAssigningLevel(null);
    }
  }

  async function handleRemoveLevel(classId: number) {
    if (!confirm('Remove level assignment from this class?')) return;
    
    setAssigningLevel(classId);
    setError('');

    try {
      await fetchWithAuth(`/api/admin/classes/${classId}/assign-level`, {
        method: 'DELETE',
      });
      setSuccess('Level assignment removed');
      loadClasses();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err: unknown) {
      console.error(err);
      const errorMessage = err instanceof Error ? err.message : 'Could not remove level';
      setError(errorMessage);
    } finally {
      setAssigningLevel(null);
    }
  }

  async function handleDelete(id: number) {
    if (!confirm('Are you sure you want to delete this class?')) return;
    try {
      await fetchWithAuth(`/api/admin/classes/${id}`, { method: 'DELETE' });
      loadClasses();
    } catch (err) {
      console.error(err);
      setError('Could not delete class');
    }
  }

  const filteredClasses = classes.filter((cls) =>
    cls.className?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    cls.schoolName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="max-w-7xl mx-auto">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-lg flex items-center justify-center">
              <BookOpen className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Classes</h1>
              <p className="text-sm text-gray-600">Manage classes and assign robotics levels</p>
            </div>
          </div>
          {userRole === 'SchoolAdmin' && (
            <button
              onClick={() => setShowForm(!showForm)}
              className="flex items-center gap-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-4 py-2 rounded-lg hover:from-blue-700 hover:to-indigo-700 transition-all"
            >
              <Plus className="w-4 h-4" />
              Add Class
            </button>
          )}
        </div>

        {/* Messages */}
        {error && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-700">{error}</p>
          </div>
        )}
        {success && (
          <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg flex items-start gap-2">
            <Check className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-green-700">{success}</p>
          </div>
        )}

        {/* Add Class Form */}
        {userRole === 'SchoolAdmin' && showForm && (
          <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg p-6 border border-blue-100">
            <h3 className="text-sm font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Plus className="w-4 h-4" />
              Add New Class
            </h3>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Class Name *</label>
                <input
                  value={className}
                  onChange={(e) => setClassName(e.target.value)}
                  placeholder="e.g., Grade 5A, Section B"
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
              {userRole === 'SchoolAdmin' && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Robotics Level (Optional)</label>
                  <select
                    value={selectedLevelId}
                    onChange={(e) => setSelectedLevelId(e.target.value ? Number(e.target.value) : '')}
                    className="w-full border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">No level assigned</option>
                    {roboticsLevels.map((level) => (
                      <option key={level.id} value={level.id}>
                        Level {level.levelNumber}: {level.name}
                      </option>
                    ))}
                  </select>
                </div>
              )}
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
                    Add Class
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
              <p className="text-sm text-gray-600">Total Classes</p>
              <p className="text-2xl font-bold text-gray-900">{classes.length}</p>
            </div>
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <BookOpen className="w-6 h-6 text-blue-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">With Level Assigned</p>
              <p className="text-2xl font-bold text-gray-900">
                {classes.filter(c => c.roboticsLevelId).length}
              </p>
            </div>
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <GraduationCap className="w-6 h-6 text-green-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Schools with Classes</p>
              <p className="text-2xl font-bold text-gray-900">
                {new Set(classes.map(c => c.schoolId)).size}
              </p>
            </div>
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
              <School className="w-6 h-6 text-purple-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Students</p>
              <p className="text-2xl font-bold text-gray-900">
                {classes.reduce((sum, c) => sum + (c.studentCount || 0), 0)}
              </p>
            </div>
            <div className="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center">
              <Users className="w-6 h-6 text-indigo-600" />
            </div>
          </div>
        </div>
      </div>

      {/* Search and Classes List */}
      <div className="bg-white rounded-xl shadow-sm overflow-hidden">
        <div className="p-4 border-b border-gray-100">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search classes by name or school..."
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
        ) : filteredClasses.length === 0 ? (
          <div className="text-center py-12">
            <BookOpen className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500 mb-2">
              {searchTerm ? 'No classes found matching your search' : 'No classes created yet'}
            </p>
            {!searchTerm && schools.length === 0 && userRole !== 'SchoolAdmin' && (
              <p className="text-sm text-gray-400">Add a school first before creating classes</p>
            )}
          </div>
        ) : (
          <div className="divide-y divide-gray-100">
            {filteredClasses.map((c) => (
              <div
                key={c.id}
                className="p-4 hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4 flex-1">
                    <div className="w-12 h-12 bg-gradient-to-br from-blue-100 to-indigo-100 rounded-lg flex items-center justify-center">
                      <BookOpen className="w-6 h-6 text-blue-600" />
                    </div>
                    <div className="flex-1">
                      <p className="font-semibold text-gray-900">{c.className}</p>
                      <div className="flex flex-wrap items-center gap-4 mt-1 text-sm text-gray-500">
                        <span className="flex items-center gap-1">
                          <School className="w-3 h-3" />
                          {c.schoolName}
                        </span>
                        <span className="flex items-center gap-1">
                          <Users className="w-3 h-3" />
                          {c.studentCount || 0} students
                        </span>
                      </div>
                    </div>
                  </div>

                  {userRole === 'SchoolAdmin' && (
                    <div className="flex items-center gap-2">
                      <button
                        className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                        title="Edit Class"
                      >
                        <Edit className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(c.id)}
                        className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                        title="Delete Class"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  )}
                </div>

                {/* Level Assignment Section - Only for School Admin */}
                {userRole === 'SchoolAdmin' ? (
                  <div className="mt-4 pt-4 border-t border-gray-100">
                    <div className="flex items-center justify-between gap-4">
                      <div className="flex items-center gap-3">
                        <GraduationCap className="w-5 h-5 text-green-600" />
                        <span className="text-sm font-medium text-gray-700">Robotics Level:</span>
                      </div>
                      
                      <div className="flex items-center gap-3 flex-1 max-w-md">
                        {c.roboticsLevelId ? (
                          <div className="flex items-center gap-3 flex-1">
                            <div className="flex-1 px-4 py-2 bg-green-50 border border-green-200 rounded-lg">
                              <span className="font-medium text-green-700">
                                Level {c.roboticsLevelNumber}: {c.roboticsLevelName}
                              </span>
                            </div>
                            <button
                              onClick={() => handleRemoveLevel(c.id)}
                              disabled={assigningLevel === c.id}
                              className="px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors text-sm"
                            >
                              Remove
                            </button>
                          </div>
                        ) : (
                          <select
                            onChange={(e) => {
                              if (e.target.value) {
                                handleAssignLevel(c.id, Number(e.target.value));
                              }
                            }}
                            disabled={assigningLevel === c.id}
                            className="flex-1 border border-gray-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-green-500"
                            defaultValue=""
                          >
                            <option value="">Select a level to assign...</option>
                            {roboticsLevels.map((level) => (
                              <option key={level.id} value={level.id}>
                                Level {level.levelNumber}: {level.name}
                              </option>
                            ))}
                          </select>
                        )}
                        
                        {assigningLevel === c.id && (
                          <Loader className="w-5 h-5 text-green-600 animate-spin" />
                        )}
                      </div>
                    </div>
                  </div>
                ) : c.roboticsLevelId ? (
                  <div className="mt-4 pt-4 border-t border-gray-100">
                    <div className="flex items-center gap-3">
                      <GraduationCap className="w-5 h-5 text-green-600" />
                      <span className="text-sm font-medium text-gray-700">Robotics Level:</span>
                      <span className="px-3 py-1 bg-green-50 border border-green-200 rounded-lg text-green-700 font-medium">
                        Level {c.roboticsLevelNumber}: {c.roboticsLevelName}
                      </span>
                    </div>
                  </div>
                ) : null}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Help Section */}
      <div className="mt-6 p-6 bg-blue-50 rounded-xl border border-blue-200">
        <h3 className="font-semibold text-blue-900 mb-2 flex items-center gap-2">
          <FlaskConical className="w-5 h-5" />
          {userRole === 'SchoolAdmin' ? 'How Level Assignment Works' : 'About Classes'}
        </h3>
        <ul className="text-sm text-blue-800 space-y-1">
          {userRole === 'SchoolAdmin' ? (
            <>
              <li>• <strong>Assign a robotics level</strong> to each class to define which curriculum they will follow</li>
              <li>• Teachers can then <strong>unlock experiments</strong> within that level for their students</li>
              <li>• Students will only see experiments that have been unlocked by their teacher</li>
              <li>• You can change the level assignment at any time during the academic year</li>
            </>
          ) : (
            <>
              <li>• Classes are organizational units within each school</li>
              <li>• <strong>School Admins</strong> are responsible for assigning robotics levels to their classes</li>
              <li>• Teachers manage experiment unlocking within their assigned classes</li>
              <li>• Students access experiments based on their class and teacher permissions</li>
            </>
          )}
        </ul>
      </div>
    </div>
  );
}
