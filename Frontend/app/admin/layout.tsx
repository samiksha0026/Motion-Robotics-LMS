"use client";

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { usePathname } from 'next/navigation';
import { getRole, getSchoolContext, logout, validateSession } from '../../lib/auth';
import { 
  LayoutDashboard, 
  School, 
  Users, 
  GraduationCap, 
  BookOpen, 
  FileText, 
  ClipboardList, 
  Award, 
  BarChart3, 
  Settings, 
  LogOut,
  Menu,
  X,
  ChevronLeft,
  ChevronRight,
  Camera
} from 'lucide-react';
import { ThemeToggle } from '@/components/ThemeToggle';

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const [isLoading, setIsLoading] = useState(true);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [userRole, setUserRole] = useState<string | null>(null);
  const [schoolName, setSchoolName] = useState<string | null>(null);

  useEffect(() => {
    // Skip auth check on login page
    if (pathname === '/admin/login') {
      setIsLoading(false);
      return;
    }

    // First check sessionStorage for quick UI (avoids flash)
    const token = sessionStorage.getItem('jwt');
    const userRoleValue = sessionStorage.getItem('userRole');
    const isAdmin = userRoleValue === 'SuperAdmin' || userRoleValue === 'SchoolAdmin' || userRoleValue === 'Admin';
    
    if (!token || !isAdmin) {
      // Clear stale session before redirecting
      sessionStorage.clear();
      window.location.href = '/login';
      return;
    }

    // Validate session server-side (checks the UserSessions table)
    validateSession().then(session => {
      if (!session) {
        sessionStorage.clear();
        window.location.href = '/login';
        return;
      }
      
      const validAdmin = session.role === 'SuperAdmin' || session.role === 'SchoolAdmin' || session.role === 'Admin';
      if (!validAdmin) {
        sessionStorage.clear();
        window.location.href = '/login';
        return;
      }

      setUserRole(session.role);
      
      if (session.role === 'SchoolAdmin') {
        const schoolContext = getSchoolContext();
        if (schoolContext) {
          setSchoolName(schoolContext.schoolName);
        }
      }
      
      setIsLoading(false);
    });
  }, [pathname]);

  const handleLogout = () => {
    logout('/login');
  };

  // Define nav items with role restrictions
  const allNavItems = [
    { href: '/admin/dashboard', label: 'Dashboard', icon: LayoutDashboard, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/schools', label: 'Schools', icon: School, roles: ['SuperAdmin'] }, // SuperAdmin only
    { href: '/admin/classes', label: 'Classes', icon: BookOpen, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/teachers', label: 'Teachers', icon: GraduationCap, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/students', label: 'Students', icon: Users, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/courses', label: 'Courses', icon: FileText, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/lab', label: 'Lab', icon: Camera, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/exams', label: 'Exams', icon: ClipboardList, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/certificates', label: 'Certificates', icon: Award, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/reports', label: 'Reports', icon: BarChart3, roles: ['SuperAdmin', 'SchoolAdmin'] },
    { href: '/admin/settings', label: 'Settings', icon: Settings, roles: ['SuperAdmin', 'SchoolAdmin'] },
  ];

  // Filter nav items based on user role
  const navItems = allNavItems.filter(item => 
    !userRole || item.roles.includes(userRole)
  );

  const isActive = (href: string) => pathname === href;

  // Show loading only for protected pages
  if (isLoading && pathname !== '/admin/login') {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="w-16 h-16 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  // Don't render sidebar layout for login page
  if (pathname === '/admin/login') {
    return <>{children}</>;
  }

  return (
    <div className="min-h-screen bg-gray-100 flex">
      {/* Sidebar - Desktop */}
      <aside className={`hidden lg:flex flex-col bg-white border-r border-gray-200 transition-all duration-300 fixed left-0 top-0 bottom-0 ${
        sidebarCollapsed ? 'w-20' : 'w-64'
      }`}>
        {/* Logo Section */}
        <div className="border-b border-gray-200">
          <div className="h-16 flex items-center justify-center px-4">
            <Link href="/admin/dashboard" className="flex items-center">
              <Image 
                src="/images/logo.png" 
                alt="Motion Robotics" 
                width={sidebarCollapsed ? 40 : 150}
                height={sidebarCollapsed ? 40 : 50}
                className="object-contain"
                priority
              />
            </Link>
          </div>
          {/* Role/School indicator */}
          {!sidebarCollapsed && (
            <div className="px-4 pb-3">
              <div className={`text-xs font-medium px-2 py-1 rounded ${
                userRole === 'SuperAdmin' 
                  ? 'bg-purple-100 text-purple-700' 
                  : 'bg-blue-100 text-blue-700'
              }`}>
                {userRole === 'SuperAdmin' ? 'Super Admin' : schoolName || 'School Admin'}
              </div>
            </div>
          )}
        </div>

        {/* Navigation */}
        <nav className="flex-1 py-4 px-3 space-y-1 overflow-y-auto">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = isActive(item.href);
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all ${
                  active
                    ? 'bg-gradient-to-r from-blue-600 to-indigo-600 text-white shadow-sm'
                    : 'text-gray-700 hover:bg-gray-100'
                } ${sidebarCollapsed ? 'justify-center' : ''}`}
                title={sidebarCollapsed ? item.label : undefined}
              >
                <Icon className="w-5 h-5 flex-shrink-0" />
                {!sidebarCollapsed && <span>{item.label}</span>}
              </Link>
            );
          })}
        </nav>

        {/* Theme Toggle */}
        <div className={`flex items-center border-t border-gray-200 px-3 py-2 ${sidebarCollapsed ? 'justify-center' : 'justify-start'}`}>
          <ThemeToggle />
        </div>

        {/* Collapse Toggle */}
        <button
          onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
          className="h-12 flex items-center justify-center border-t border-gray-200 text-gray-500 hover:bg-gray-50 transition-colors"
        >
          {sidebarCollapsed ? (
            <ChevronRight className="w-5 h-5" />
          ) : (
            <ChevronLeft className="w-5 h-5" />
          )}
        </button>

        {/* Logout */}
        <button
          onClick={handleLogout}
          className={`flex items-center gap-3 px-3 py-3 mx-3 mb-3 rounded-lg text-sm font-medium text-red-600 hover:bg-red-50 transition-colors ${
            sidebarCollapsed ? 'justify-center' : ''
          }`}
          title={sidebarCollapsed ? 'Logout' : undefined}
        >
          <LogOut className="w-5 h-5 flex-shrink-0" />
          {!sidebarCollapsed && <span>Logout</span>}
        </button>
      </aside>

      {/* Mobile Header */}
      <div className="lg:hidden fixed top-0 left-0 right-0 h-16 bg-white border-b border-gray-200 flex items-center justify-between px-4 z-50">
        <Link href="/admin/dashboard" className="flex items-center">
          <Image 
            src="/images/logo.png" 
            alt="Motion Robotics" 
            width={120}
            height={40}
            className="object-contain"
            priority
          />
        </Link>
        <button
          onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
          className="p-2 rounded-lg hover:bg-gray-100"
        >
          {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
        </button>
      </div>

      {/* Mobile Sidebar Overlay */}
      {mobileMenuOpen && (
        <div className="lg:hidden fixed inset-0 z-40">
          <div className="absolute inset-0 bg-black/50" onClick={() => setMobileMenuOpen(false)} />
          <aside className="absolute left-0 top-0 bottom-0 w-64 bg-white flex flex-col">
            <div className="border-b border-gray-200">
              <div className="h-16 flex items-center justify-center px-4">
                <Link href="/admin/dashboard" className="flex items-center">
                  <Image 
                    src="/images/logo.png" 
                    alt="Motion Robotics" 
                    width={150}
                    height={50}
                    className="object-contain"
                    priority
                  />
                </Link>
              </div>
              {/* Role/School indicator for mobile */}
              <div className="px-4 pb-3">
                <div className={`text-xs font-medium px-2 py-1 rounded ${
                  userRole === 'SuperAdmin' 
                    ? 'bg-purple-100 text-purple-700' 
                    : 'bg-blue-100 text-blue-700'
                }`}>
                  {userRole === 'SuperAdmin' ? 'Super Admin' : schoolName || 'School Admin'}
                </div>
              </div>
            </div>
            <nav className="flex-1 py-4 px-3 space-y-1 overflow-y-auto">
              {navItems.map((item) => {
                const Icon = item.icon;
                const active = isActive(item.href);
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    onClick={() => setMobileMenuOpen(false)}
                    className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all ${
                      active
                        ? 'bg-gradient-to-r from-blue-600 to-indigo-600 text-white'
                        : 'text-gray-700 hover:bg-gray-100'
                    }`}
                  >
                    <Icon className="w-5 h-5" />
                    <span>{item.label}</span>
                  </Link>
                );
              })}
            </nav>
            <div className="flex items-center px-3 py-2 border-t border-gray-200">
              <ThemeToggle />
            </div>
            <button
              onClick={handleLogout}
              className="flex items-center gap-3 px-3 py-3 mx-3 mb-3 rounded-lg text-sm font-medium text-red-600 hover:bg-red-50 transition-colors"
            >
              <LogOut className="w-5 h-5" />
              <span>Logout</span>
            </button>
          </aside>
        </div>
      )}

      {/* Main Content */}
      <main className={`flex-1 min-h-screen transition-all duration-300 ${
        sidebarCollapsed ? 'lg:ml-20' : 'lg:ml-64'
      }`}>
        <div className="lg:hidden h-16" /> {/* Spacer for mobile header */}
        <div className="p-6">{children}</div>
      </main>
    </div>
  );
}
