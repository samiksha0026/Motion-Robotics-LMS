"use client";

import React, { useEffect, useState, use } from 'react';
import { fetchWithAuth } from '../../../../lib/auth';
import Link from 'next/link';
import { 
  School, 
  ArrowLeft,
  Loader, 
  Users, 
  BookOpen, 
  Phone, 
  Mail, 
  MapPin,
  User,
  GraduationCap,
  Building,
  Calendar,
  AlertCircle,
  Key,
  Copy,
  Check,
  RefreshCw
} from 'lucide-react';

interface SchoolData {
  id: number;
  schoolName: string;
  schoolCode: string;
  address: string;
  city: string;
  state: string;
  pincode: string;
  contactEmail: string;
  contactPhone: string;
  logoUrl?: string;
  principalName?: string;
  loginUsername?: string;
  isActive: boolean;
  createdAt: string;
  studentCount: number;
  classCount: number;
  teacherCount: number;
}

interface ClassData {
  id: number;
  name: string;
  section: string;
  studentCount: number;
  teacherName?: string;
}

interface TeacherData {
  id: number;
  name: string;
  email: string;
  phone: string;
  subject?: string;
}

export default function SchoolDetailsPage({ params }: { params: Promise<{ id: string }> }) {
  const resolvedParams = use(params);
  const schoolId = resolvedParams.id;
  
  const [school, setSchool] = useState<SchoolData | null>(null);
  const [classes, setClasses] = useState<ClassData[]>([]);
  const [teachers, setTeachers] = useState<TeacherData[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [resettingPassword, setResettingPassword] = useState(false);
  const [newPassword, setNewPassword] = useState<string | null>(null);
  const [copiedPassword, setCopiedPassword] = useState(false);

  async function loadSchoolData() {
    try {
      setLoading(true);
      setError('');
      
      // Load school details
      const schoolData = await fetchWithAuth(`/api/admin/schools/${schoolId}`);
      setSchool(schoolData);
      
      // Try to load classes for this school
      try {
        const classesData = await fetchWithAuth(`/api/admin/schools/${schoolId}/classes`);
        setClasses(Array.isArray(classesData) ? classesData : []);
      } catch {
        // Classes endpoint might not exist yet
        setClasses([]);
      }
      
      // Try to load teachers for this school
      try {
        const teachersData = await fetchWithAuth(`/api/admin/schools/${schoolId}/teachers`);
        setTeachers(Array.isArray(teachersData) ? teachersData : []);
      } catch {
        // Teachers endpoint might not exist yet  
        setTeachers([]);
      }
    } catch (err) {
      console.error('Error loading school:', err);
      setError('Failed to load school details');
    } finally {
      setLoading(false);
    }
  }

  async function handleResetPassword() {
    if (!confirm('Are you sure you want to reset the SchoolAdmin password for this school?')) {
      return;
    }
    
    try {
      setResettingPassword(true);
      const response = await fetchWithAuth(`/api/admin/schools/${schoolId}/reset-password`, {
        method: 'POST'
      });
      setNewPassword(response.newPassword);
    } catch (err) {
      console.error('Error resetting password:', err);
      setError('Failed to reset password');
    } finally {
      setResettingPassword(false);
    }
  }

  function copyPassword() {
    if (newPassword) {
      navigator.clipboard.writeText(newPassword);
      setCopiedPassword(true);
      setTimeout(() => setCopiedPassword(false), 2000);
    }
  }

  useEffect(() => {
    loadSchoolData();
  }, [schoolId]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-96">
        <Loader className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (error && !school) {
    return (
      <div className="space-y-4">
        <Link 
          href="/admin/schools"
          className="flex items-center gap-2 text-blue-600 hover:underline"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Schools
        </Link>
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex items-center gap-3">
          <AlertCircle className="w-5 h-5 text-red-500" />
          <span className="text-red-700">{error}</span>
        </div>
      </div>
    );
  }

  if (!school) {
    return (
      <div className="space-y-4">
        <Link 
          href="/admin/schools"
          className="flex items-center gap-2 text-blue-600 hover:underline"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Schools
        </Link>
        <div className="text-center py-12 text-gray-500">
          School not found
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link 
            href="/admin/schools"
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <ArrowLeft className="w-5 h-5 text-gray-600" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{school.schoolName}</h1>
            <p className="text-gray-600">School Code: {school.schoolCode}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className={`px-3 py-1 rounded-full text-sm font-medium ${
            school.isActive 
              ? 'bg-green-100 text-green-700' 
              : 'bg-red-100 text-red-700'
          }`}>
            {school.isActive ? 'Active' : 'Inactive'}
          </span>
          <Link
            href="/admin/schools"
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Edit School
          </Link>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white rounded-xl shadow-sm p-5">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <BookOpen className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{school.classCount}</p>
              <p className="text-sm text-gray-600">Classes</p>
            </div>
          </div>
        </div>
        
        <div className="bg-white rounded-xl shadow-sm p-5">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <GraduationCap className="w-6 h-6 text-green-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{school.teacherCount}</p>
              <p className="text-sm text-gray-600">Teachers</p>
            </div>
          </div>
        </div>
        
        <div className="bg-white rounded-xl shadow-sm p-5">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
              <Users className="w-6 h-6 text-purple-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{school.studentCount}</p>
              <p className="text-sm text-gray-600">Students</p>
            </div>
          </div>
        </div>
      </div>

      {/* School Info Card */}
      <div className="bg-white rounded-xl shadow-sm">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">School Information</h2>
        </div>
        <div className="p-6">
          <div className="flex gap-6">
            {/* School Logo */}
            <div className="flex-shrink-0">
              {school.logoUrl ? (
                <img 
                  src={school.logoUrl} 
                  alt={school.schoolName}
                  className="w-24 h-24 rounded-lg object-cover border border-gray-200"
                />
              ) : (
                <div className="w-24 h-24 bg-gray-100 rounded-lg flex items-center justify-center border border-gray-200">
                  <School className="w-10 h-10 text-gray-400" />
                </div>
              )}
            </div>
            
            {/* School Details */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 flex-grow">
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Principal Name</h3>
                <div className="flex items-center gap-2 text-gray-900">
                  <User className="w-4 h-4 text-gray-400" />
                  {school.principalName || 'Not specified'}
                </div>
              </div>
              
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Contact Email</h3>
                <div className="flex items-center gap-2 text-gray-900">
                  <Mail className="w-4 h-4 text-gray-400" />
                  {school.contactEmail}
                </div>
              </div>
              
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Contact Phone</h3>
                <div className="flex items-center gap-2 text-gray-900">
                  <Phone className="w-4 h-4 text-gray-400" />
                  {school.contactPhone}
                </div>
              </div>
              
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Address</h3>
                <div className="flex items-start gap-2 text-gray-900">
                  <MapPin className="w-4 h-4 text-gray-400 mt-0.5" />
                  <span>{school.address}, {school.city}, {school.state} - {school.pincode}</span>
                </div>
              </div>
              
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Created On</h3>
                <div className="flex items-center gap-2 text-gray-900">
                  <Calendar className="w-4 h-4 text-gray-400" />
                  {new Date(school.createdAt).toLocaleDateString('en-IN', {
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric'
                  })}
                </div>
              </div>
              
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-1">Admin Username</h3>
                <div className="flex items-center gap-2 text-gray-900">
                  <User className="w-4 h-4 text-gray-400" />
                  {school.loginUsername || school.schoolCode.toLowerCase() + '@motionrobotics.in'}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Admin Password Reset */}
      <div className="bg-white rounded-xl shadow-sm">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">SchoolAdmin Credentials</h2>
        </div>
        <div className="p-6">
          <p className="text-gray-600 mb-4">
            Reset the SchoolAdmin password for this school if the admin has forgotten their credentials.
          </p>
          
          {newPassword ? (
            <div className="bg-green-50 border border-green-200 rounded-lg p-4 mb-4">
              <p className="text-green-700 font-medium mb-2">Password reset successfully!</p>
              <div className="flex items-center gap-2">
                <span className="text-gray-700">New Password:</span>
                <code className="bg-white px-3 py-1 rounded border text-gray-900 font-mono">
                  {newPassword}
                </code>
                <button
                  onClick={copyPassword}
                  className="p-1.5 rounded hover:bg-green-100 transition-colors"
                  title="Copy password"
                >
                  {copiedPassword ? (
                    <Check className="w-4 h-4 text-green-600" />
                  ) : (
                    <Copy className="w-4 h-4 text-gray-500" />
                  )}
                </button>
              </div>
              <p className="text-sm text-green-600 mt-2">
                Please share this password with the school administrator securely.
              </p>
            </div>
          ) : null}
          
          <button
            onClick={handleResetPassword}
            disabled={resettingPassword}
            className="flex items-center gap-2 px-4 py-2 bg-orange-500 text-white rounded-lg hover:bg-orange-600 transition-colors disabled:opacity-50"
          >
            {resettingPassword ? (
              <Loader className="w-4 h-4 animate-spin" />
            ) : (
              <Key className="w-4 h-4" />
            )}
            Reset Admin Password
          </button>
        </div>
      </div>

      {/* Quick Links */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Link
          href={`/admin/classes?school=${schoolId}`}
          className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow"
        >
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <BookOpen className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <h3 className="font-semibold text-gray-900">View Classes</h3>
              <p className="text-sm text-gray-500">Manage classes for this school</p>
            </div>
          </div>
        </Link>
        
        <Link
          href={`/admin/teachers?school=${schoolId}`}
          className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow"
        >
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <GraduationCap className="w-6 h-6 text-green-600" />
            </div>
            <div>
              <h3 className="font-semibold text-gray-900">View Teachers</h3>
              <p className="text-sm text-gray-500">Manage teachers for this school</p>
            </div>
          </div>
        </Link>
        
        <Link
          href={`/admin/students?school=${schoolId}`}
          className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow"
        >
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
              <Users className="w-6 h-6 text-purple-600" />
            </div>
            <div>
              <h3 className="font-semibold text-gray-900">View Students</h3>
              <p className="text-sm text-gray-500">Manage students for this school</p>
            </div>
          </div>
        </Link>
      </div>
    </div>
  );
}
