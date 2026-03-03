"use client";

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { Users, BookOpen, School, GraduationCap, ClipboardList, FileText, TrendingUp, Building, Layers } from 'lucide-react';
import { fetchWithAuth, getRole, getSchoolContext } from '../../../lib/auth';

interface DashboardStats {
  schoolCount: number;
  teacherCount: number;
  studentCount: number;
  classCount: number;
  examCount: number;
}

interface SchoolSummary {
  id: number;
  schoolName: string;
  studentCount: number;
  teacherCount: number;
  classCount: number;
}

interface ClassSummary {
  id: number;
  name: string;
  studentCount: number;
  teacherName?: string;
}

interface SchoolInfo {
  id: number;
  schoolName: string;
  schoolCode: string;
  city: string;
  state: string;
}

export default function AdminDashboard() {
  const [adminEmail, setAdminEmail] = useState('');
  const [userRole, setUserRole] = useState<string | null>(null);
  const [schoolInfo, setSchoolInfo] = useState<SchoolInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState<DashboardStats>({
    schoolCount: 0,
    teacherCount: 0,
    studentCount: 0,
    classCount: 0,
    examCount: 0
  });
  const [schoolSummaries, setSchoolSummaries] = useState<SchoolSummary[]>([]);
  const [classSummaries, setClassSummaries] = useState<ClassSummary[]>([]);

  useEffect(() => {
    const email = sessionStorage.getItem('admin_email');
    setAdminEmail(email || 'Admin');
    
    const role = getRole();
    setUserRole(role);
    
    if (role === 'SchoolAdmin') {
      const schoolContext = getSchoolContext();
      if (schoolContext) {
        setSchoolInfo({
          id: schoolContext.id,
          schoolName: schoolContext.schoolName,
          schoolCode: schoolContext.schoolCode,
          city: '',
          state: ''
        });
      }
      loadSchoolAdminStats();
    } else {
      loadSuperAdminStats();
    }
  }, []);

  async function loadSchoolAdminStats() {
    try {
      const schoolContext = getSchoolContext();
      if (!schoolContext) {
        setIsLoading(false);
        return;
      }

      const [classes, teachers, students] = await Promise.all([
        fetchWithAuth(`/api/admin/classes/school/${schoolContext.id}`).catch(() => []),
        fetchWithAuth(`/api/admin/teachers/school/${schoolContext.id}`).catch(() => []),
        fetchWithAuth(`/api/admin/students/school/${schoolContext.id}`).catch(() => [])
      ]);
      
      setStats({
        schoolCount: 1,
        teacherCount: Array.isArray(teachers) ? teachers.length : 0,
        studentCount: Array.isArray(students) ? students.length : 0,
        classCount: Array.isArray(classes) ? classes.length : 0,
        examCount: 0
      });

      // Build class summaries
      if (classes && Array.isArray(classes)) {
        const summaries: ClassSummary[] = classes.map((cls: any) => ({
          id: cls.id,
          name: cls.className || cls.name,
          studentCount: cls.studentCount || 0,
          teacherName: cls.teacherName
        }));
        setClassSummaries(summaries);
      }
    } catch (err) {
      console.error('Failed to load stats:', err);
    } finally {
      setIsLoading(false);
    }
  }

  async function loadSuperAdminStats() {
    try {
      const [schools, teachers, students, examsData] = await Promise.all([
        fetchWithAuth('/api/admin/schools').catch(() => []),
        fetchWithAuth('/api/admin/teachers').catch(() => []),
        fetchWithAuth('/api/admin/students').catch(() => []),
        fetchWithAuth('/api/admin/exams').catch(() => ({ exams: [], totalCount: 0 }))
      ]);
      
      const exams = Array.isArray(examsData) ? examsData : (examsData?.exams || []);
      
      setStats({
        schoolCount: Array.isArray(schools) ? schools.length : 0,
        teacherCount: Array.isArray(teachers) ? teachers.length : 0,
        studentCount: Array.isArray(students) ? students.length : 0,
        classCount: 0,
        examCount: exams.length
      });

      // Build school summaries
      if (schools && Array.isArray(schools)) {
        const summaries: SchoolSummary[] = schools.map((school: any) => ({
          id: school.id,
          schoolName: school.schoolName,
          studentCount: school.studentCount || 0,
          teacherCount: school.teacherCount || 0,
          classCount: school.classCount || 0
        }));
        setSchoolSummaries(summaries);
      }
    } catch (err) {
      console.error('Failed to load stats:', err);
    } finally {
      setIsLoading(false);
    }
  }

  // Different stat cards based on role
  const superAdminStatCards = [
    { title: 'Total Schools', value: stats.schoolCount, icon: School, color: 'bg-blue-500', link: '/admin/schools' },
    { title: 'Total Teachers', value: stats.teacherCount, icon: GraduationCap, color: 'bg-green-500', link: '/admin/teachers' },
    { title: 'Total Students', value: stats.studentCount, icon: Users, color: 'bg-purple-500', link: '/admin/students' },
    { title: 'Total Exams', value: stats.examCount, icon: ClipboardList, color: 'bg-pink-500', link: '/admin/exams' },
  ];

  const schoolAdminStatCards = [
    { title: 'Total Classes', value: stats.classCount, icon: BookOpen, color: 'bg-blue-500', link: '/admin/classes' },
    { title: 'Total Teachers', value: stats.teacherCount, icon: GraduationCap, color: 'bg-green-500', link: '/admin/teachers' },
    { title: 'Total Students', value: stats.studentCount, icon: Users, color: 'bg-purple-500', link: '/admin/students' },
    { title: 'Robotics Levels', value: 6, icon: Layers, color: 'bg-orange-500', link: '/admin/robotics-levels' },
  ];

  const statCards = userRole === 'SchoolAdmin' ? schoolAdminStatCards : superAdminStatCards;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="w-12 h-12 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading dashboard...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          {userRole === 'SchoolAdmin' && schoolInfo ? (
            <p className="text-gray-600">{schoolInfo.schoolName} - Welcome, {adminEmail}</p>
          ) : (
            <p className="text-gray-600">Welcome back, {adminEmail}</p>
          )}
        </div>
        {userRole === 'SchoolAdmin' && schoolInfo && (
          <div className="text-right">
            <span className="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm font-medium">
              School Admin
            </span>
          </div>
        )}
        {userRole === 'SuperAdmin' && (
          <div className="text-right">
            <span className="px-3 py-1 bg-purple-100 text-purple-700 rounded-full text-sm font-medium">
              Super Admin
            </span>
          </div>
        )}
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {statCards.map((card) => {
          const Icon = card.icon;
          return (
            <Link
              key={card.title}
              href={card.link}
              className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center gap-4">
                <div className={`w-12 h-12 ${card.color} rounded-lg flex items-center justify-center`}>
                  <Icon className="w-6 h-6 text-white" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-gray-900">{card.value}</p>
                  <p className="text-sm text-gray-600">{card.title}</p>
                </div>
              </div>
            </Link>
          );
        })}
      </div>

      {/* SuperAdmin: School-wise Summary */}
      {userRole === 'SuperAdmin' && (
        <div className="bg-white rounded-xl shadow-sm">
          <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Building className="w-5 h-5 text-gray-600" />
              <h2 className="text-lg font-semibold text-gray-900">School-wise Summary</h2>
            </div>
            <Link href="/admin/schools" className="text-sm text-blue-600 hover:underline">
              View All Schools
            </Link>
          </div>
          
          {schoolSummaries.length === 0 ? (
            <div className="p-8 text-center text-gray-500">
              <School className="w-12 h-12 mx-auto mb-3 text-gray-300" />
              <p>No schools registered yet</p>
              <Link href="/admin/schools" className="text-blue-600 hover:underline text-sm mt-2 inline-block">
                Add your first school
              </Link>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      School Name
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Classes
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Teachers
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Students
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {schoolSummaries.map((school) => (
                    <tr key={school.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                            <School className="w-5 h-5 text-blue-600" />
                          </div>
                          <span className="font-medium text-gray-900">{school.schoolName}</span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-center text-gray-600">{school.classCount}</td>
                      <td className="px-6 py-4 text-center text-gray-600">{school.teacherCount}</td>
                      <td className="px-6 py-4 text-center text-gray-600">{school.studentCount}</td>
                      <td className="px-6 py-4 text-right">
                        <Link 
                          href={`/admin/schools/${school.id}`}
                          className="text-blue-600 hover:underline text-sm"
                        >
                          View Details
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* SchoolAdmin: Class-wise Summary */}
      {userRole === 'SchoolAdmin' && (
        <div className="bg-white rounded-xl shadow-sm">
          <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <BookOpen className="w-5 h-5 text-gray-600" />
              <h2 className="text-lg font-semibold text-gray-900">Class-wise Summary</h2>
            </div>
            <Link href="/admin/classes" className="text-sm text-blue-600 hover:underline">
              Manage Classes
            </Link>
          </div>
          
          {classSummaries.length === 0 ? (
            <div className="p-8 text-center text-gray-500">
              <BookOpen className="w-12 h-12 mx-auto mb-3 text-gray-300" />
              <p>No classes created yet</p>
              <Link href="/admin/classes" className="text-blue-600 hover:underline text-sm mt-2 inline-block">
                Create your first class
              </Link>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Class Name
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Students
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Assigned Teacher
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {classSummaries.map((cls) => (
                    <tr key={cls.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                            <BookOpen className="w-5 h-5 text-blue-600" />
                          </div>
                          <span className="font-medium text-gray-900">{cls.name}</span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-center text-gray-600">{cls.studentCount}</td>
                      <td className="px-6 py-4 text-center text-gray-600">{cls.teacherName || '-'}</td>
                      <td className="px-6 py-4 text-right">
                        <Link 
                          href={`/admin/students?classId=${cls.id}`}
                          className="text-blue-600 hover:underline text-sm"
                        >
                          View Students
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Quick Actions - Different for each role */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {userRole === 'SuperAdmin' ? (
          <>
            <Link 
              href="/admin/schools"
              className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow flex items-center gap-4"
            >
              <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                <School className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">Add New School</h3>
                <p className="text-sm text-gray-500">Register a new institution</p>
              </div>
            </Link>

            <Link 
              href="/admin/teachers"
              className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow flex items-center gap-4"
            >
              <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                <GraduationCap className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">Add New Teacher</h3>
                <p className="text-sm text-gray-500">Onboard an instructor</p>
              </div>
            </Link>

            <Link 
              href="/admin/students"
              className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow flex items-center gap-4"
            >
              <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
                <Users className="w-6 h-6 text-purple-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">Add New Student</h3>
                <p className="text-sm text-gray-500">Enroll a new student</p>
              </div>
            </Link>
          </>
        ) : (
          <>
            <Link 
              href="/admin/classes"
              className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow flex items-center gap-4"
            >
              <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                <BookOpen className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">Add New Class</h3>
                <p className="text-sm text-gray-500">Create a new class</p>
              </div>
            </Link>

            <Link 
              href="/admin/teachers"
              className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow flex items-center gap-4"
            >
              <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                <GraduationCap className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">Add New Teacher</h3>
                <p className="text-sm text-gray-500">Onboard an instructor</p>
              </div>
            </Link>

            <Link 
              href="/admin/students"
              className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow flex items-center gap-4"
            >
              <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
                <Users className="w-6 h-6 text-purple-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">Add New Student</h3>
                <p className="text-sm text-gray-500">Enroll a new student</p>
              </div>
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
