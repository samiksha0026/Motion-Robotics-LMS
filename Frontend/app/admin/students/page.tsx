"use client";

import React, { useEffect, useState } from 'react';
import { fetchWithAuth, getRole, getSchoolContext } from '../../../lib/auth';
import { Users, Plus, Loader, Search, Edit, Trash2, Mail, BookOpen, Award, School, AlertCircle, Hash, Upload, FileSpreadsheet, Download, CheckCircle, XCircle } from 'lucide-react';
import { API_BASE_URL, getToken, getSessionId } from '../../../lib/auth';

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

interface ImportResult {
  successCount: number;
  failedCount: number;
  errors: { row: number; error: string }[];
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

  // Excel import state
  const [showImportPanel, setShowImportPanel] = useState(false);
  const [importFile, setImportFile] = useState<File | null>(null);
  const [importing, setImporting] = useState(false);
  const [importResult, setImportResult] = useState<ImportResult | null>(null);
  const [importError, setImportError] = useState('');
  const [dragOver, setDragOver] = useState(false);
  
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

  // ── Excel import handlers ────────────────────────────────────────────────

  async function handleDownloadTemplate() {
    try {
      const token = getToken();
      const sessionId = getSessionId();
      const headers: Record<string, string> = {};
      if (token) headers['Authorization'] = `Bearer ${token}`;
      if (sessionId) headers['X-Session-Id'] = sessionId;

      const res = await fetch(`${API_BASE_URL}/api/admin/students/template`, {
        headers,
        credentials: 'include',
      });
      if (!res.ok) throw new Error('Failed to download template');

      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'students_import_template.xlsx';
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      console.error(err);
      setImportError('Could not download template.');
    }
  }

  async function handleImport() {
    if (!importFile) {
      setImportError('Please select an Excel file first.');
      return;
    }
    if (!userSchool) {
      setImportError('School context not found. Please re-login.');
      return;
    }

    setImporting(true);
    setImportError('');
    setImportResult(null);

    try {
      const token = getToken();
      const sessionId = getSessionId();
      // Do NOT set Content-Type — browser sets it automatically with multipart boundary
      const headers: Record<string, string> = {};
      if (token) headers['Authorization'] = `Bearer ${token}`;
      if (sessionId) headers['X-Session-Id'] = sessionId;

      const form = new FormData();
      form.append('file', importFile);

      const res = await fetch(`${API_BASE_URL}/api/admin/students/upload`, {
        method: 'POST',
        headers,
        credentials: 'include',
        body: form,
      });

      const json = await res.json();
      if (!res.ok) {
        setImportError(json?.message || 'Import failed.');
        return;
      }

      setImportResult(json as ImportResult);
      setImportFile(null);
      if (json.successCount > 0) loadStudents();
    } catch (err: any) {
      setImportError(err.message || 'Import failed. Check backend is running.');
    } finally {
      setImporting(false);
    }
  }

  function resetImport() {
    setShowImportPanel(false);
    setImportFile(null);
    setImportResult(null);
    setImportError('');
    setDragOver(false);
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
            <div className="flex items-center gap-2">
              <button
                onClick={() => { setShowImportPanel(!showImportPanel); setShowForm(false); }}
                className="flex items-center gap-2 border border-blue-600 text-blue-600 px-4 py-2 rounded-lg hover:bg-blue-50 transition-all"
              >
                <FileSpreadsheet className="w-4 h-4" />
                Import Excel
              </button>
              <button
                onClick={() => { setShowForm(!showForm); setShowImportPanel(false); }}
                className="flex items-center gap-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white px-4 py-2 rounded-lg hover:from-blue-700 hover:to-indigo-700 transition-all"
              >
                <Plus className="w-4 h-4" />
                Add Student
              </button>
            </div>
          )}
        </div>

        {/* Import Excel Panel */}
        {userRole === 'SchoolAdmin' && showImportPanel && (
          <div className="bg-white rounded-lg p-6 border border-gray-200 shadow-sm mb-4">

            {/* Panel header */}
            <div className="flex items-center justify-between mb-5">
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 bg-green-100 rounded-lg flex items-center justify-center">
                  <FileSpreadsheet className="w-5 h-5 text-green-700" />
                </div>
                <div>
                  <h3 className="text-base font-semibold text-gray-900">Bulk Import Students via Excel</h3>
                  <p className="text-xs text-gray-500 mt-0.5">Upload an .xlsx file to create multiple students at once</p>
                </div>
              </div>
              <button
                onClick={handleDownloadTemplate}
                className="flex items-center gap-2 text-sm font-medium text-green-700 bg-green-50 border border-green-200 px-4 py-2 rounded-lg hover:bg-green-100 transition-all"
              >
                <Download className="w-4 h-4" />
                Download Template
              </button>
            </div>

            {/* Steps */}
            <div className="flex items-stretch gap-2 mb-5">
              {[
                { num: '1', label: 'Download the template', sub: 'Click the button above' },
                { num: '2', label: 'Fill in student data', sub: 'Grade = 5, Division = A' },
                { num: '3', label: 'Upload the file', sub: 'Drag & drop or browse' },
              ].map((step) => (
                <div key={step.num} className="flex-1 flex items-start gap-3 bg-gray-50 border border-gray-200 rounded-lg px-4 py-3">
                  <span className="w-6 h-6 rounded-full bg-green-600 text-white text-xs font-bold flex items-center justify-center flex-shrink-0 mt-0.5">
                    {step.num}
                  </span>
                  <div>
                    <p className="text-sm font-semibold text-gray-800">{step.label}</p>
                    <p className="text-xs text-gray-500 mt-0.5">{step.sub}</p>
                  </div>
                </div>
              ))}
            </div>

            {/* Default password note */}
            <div className="flex items-center gap-2 px-4 py-2.5 bg-blue-50 border border-blue-200 rounded-lg mb-5">
              <AlertCircle className="w-4 h-4 text-blue-600 flex-shrink-0" />
              <p className="text-sm text-blue-800">
                Default password for every imported student: <strong className="font-semibold">Student@123</strong>
                <span className="text-blue-600 ml-1">(students can change it after first login)</span>
              </p>
            </div>

            {/* Error */}
            {importError && (
              <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
                <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                <p className="text-sm text-red-800 font-medium">{importError}</p>
              </div>
            )}

            {/* Import Result */}
            {importResult && (
              <div className="mb-5">
                <div className="flex items-center gap-3 mb-3">
                  <div className="flex items-center gap-2 px-4 py-2 bg-green-100 border border-green-200 rounded-lg">
                    <CheckCircle className="w-5 h-5 text-green-700" />
                    <span className="font-semibold text-green-800 text-sm">{importResult.successCount} students imported</span>
                  </div>
                  {importResult.failedCount > 0 && (
                    <div className="flex items-center gap-2 px-4 py-2 bg-red-100 border border-red-200 rounded-lg">
                      <XCircle className="w-5 h-5 text-red-700" />
                      <span className="font-semibold text-red-800 text-sm">{importResult.failedCount} rows failed</span>
                    </div>
                  )}
                </div>

                {importResult.errors.length > 0 && (
                  <div className="border border-gray-200 rounded-lg overflow-hidden">
                    <div className="bg-red-50 border-b border-red-200 px-4 py-2.5 flex items-center gap-2">
                      <XCircle className="w-4 h-4 text-red-600" />
                      <span className="text-sm font-semibold text-red-800">Fix these rows in your Excel and re-upload</span>
                    </div>
                    <div className="max-h-48 overflow-y-auto">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="bg-gray-50 border-b border-gray-200">
                            <th className="px-4 py-2.5 text-left text-gray-700 font-semibold w-20">Row</th>
                            <th className="px-4 py-2.5 text-left text-gray-700 font-semibold">Error</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                          {importResult.errors.map((e, i) => (
                            <tr key={i} className="hover:bg-gray-50">
                              <td className="px-4 py-2.5 text-red-700 font-mono font-semibold">{e.row}</td>
                              <td className="px-4 py-2.5 text-gray-800">{e.error}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Drop Zone */}
            {!importResult && (
              <div
                onDragOver={(ev) => { ev.preventDefault(); setDragOver(true); }}
                onDragLeave={() => setDragOver(false)}
                onDrop={(ev) => {
                  ev.preventDefault();
                  setDragOver(false);
                  const f = ev.dataTransfer.files[0];
                  if (f) setImportFile(f);
                }}
                className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
                  dragOver
                    ? 'border-green-500 bg-green-50'
                    : importFile
                    ? 'border-green-500 bg-green-50'
                    : 'border-gray-300 bg-gray-50 hover:border-green-400 hover:bg-green-50'
                }`}
              >
                {importFile ? (
                  <div className="flex flex-col items-center gap-2">
                    <FileSpreadsheet className="w-10 h-10 text-green-600" />
                    <p className="font-semibold text-gray-900 text-base">{importFile.name}</p>
                    <p className="text-sm text-gray-600">{(importFile.size / 1024).toFixed(1)} KB &middot; Ready to upload</p>
                    <button
                      onClick={() => setImportFile(null)}
                      className="text-xs text-red-600 font-medium hover:underline mt-1"
                    >
                      Remove file
                    </button>
                  </div>
                ) : (
                  <div className="flex flex-col items-center gap-2">
                    <Upload className="w-10 h-10 text-gray-500" />
                    <p className="text-gray-800 font-semibold text-base">Drag &amp; drop your Excel file here</p>
                    <p className="text-sm text-gray-500">or</p>
                    <label className="cursor-pointer text-sm font-semibold text-blue-600 bg-white border border-blue-300 px-4 py-1.5 rounded-lg hover:bg-blue-50 transition-all">
                      Browse file
                      <input
                        type="file"
                        accept=".xlsx"
                        className="hidden"
                        onChange={(e) => {
                          const f = e.target.files?.[0];
                          if (f) setImportFile(f);
                        }}
                      />
                    </label>
                    <p className="text-xs text-gray-500 mt-1">Only <strong>.xlsx</strong> files are accepted</p>
                  </div>
                )}
              </div>
            )}

            {/* Action buttons */}
            <div className="flex gap-3 mt-5">
              {!importResult ? (
                <button
                  onClick={handleImport}
                  disabled={importing || !importFile}
                  className="flex items-center gap-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-semibold px-6 py-2.5 rounded-lg transition-all"
                >
                  {importing ? (
                    <><Loader className="w-4 h-4 animate-spin" /> Importing...</>
                  ) : (
                    <><Upload className="w-4 h-4" /> Upload &amp; Import</>
                  )}
                </button>
              ) : (
                <button
                  onClick={() => { setImportResult(null); setImportFile(null); }}
                  className="flex items-center gap-2 bg-green-600 hover:bg-green-700 text-white font-semibold px-6 py-2.5 rounded-lg transition-all"
                >
                  <Upload className="w-4 h-4" /> Import Another File
                </button>
              )}
              <button
                onClick={resetImport}
                className="px-6 py-2.5 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-100 transition-all"
              >
                Close
              </button>
            </div>
          </div>
        )}

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