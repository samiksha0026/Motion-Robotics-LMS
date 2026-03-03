// Centralized API configuration
// Uses environment variable or falls back to localhost for development

export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5235';

// API endpoints configuration
export const API_ENDPOINTS = {
  // Auth
  AUTH: {
    LOGIN: '/api/auth/login',
    LOGOUT: '/api/auth/logout',
    REFRESH: '/api/auth/refresh',
    FORGOT_PASSWORD: '/api/auth/forgot-password',
    RESET_PASSWORD: '/api/auth/reset-password',
  },
  
  // Admin
  ADMIN: {
    SCHOOLS: '/api/admin/schools',
    TEACHERS: '/api/admin/teachers',
    STUDENTS: '/api/admin/students',
    COURSES: '/api/admin/courses',
    CONTENT: '/api/admin/content',
    EXAMS: '/api/admin/exams',
    CERTIFICATES: '/api/admin/certificates',
    REPORTS: '/api/admin/reports',
    SETTINGS: '/api/admin/settings',
    DASHBOARD: '/api/admin/dashboard',
  },
  
  // Teacher
  TEACHER: {
    DASHBOARD: '/api/teacher/dashboard',
    CLASSES: '/api/teacher/classes',
    STUDENTS: '/api/teacher/students',
    EXAMS: '/api/teacher/exams',
    REPORTS: '/api/teacher/reports',
  },
  
  // Student
  STUDENT: {
    DASHBOARD: '/api/student/dashboard',
    EXPERIMENTS: '/api/student/experiments',
    BOOKS: '/api/student/books',
    EXAMS: '/api/student/exams',
    CERTIFICATES: '/api/student/certificates',
    PROGRESS: '/api/student/progress',
  },
};

// Helper to build full API URL
export function buildApiUrl(endpoint: string): string {
  return `${API_BASE_URL}${endpoint}`;
}

// HTTP methods helper with authentication
export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const token = typeof window !== 'undefined' ? sessionStorage.getItem('jwt') : null;
  
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(options.headers || {}),
  };
  
  if (token) {
    (headers as Record<string, string>)['Authorization'] = `Bearer ${token}`;
  }
  
  const response = await fetch(buildApiUrl(endpoint), {
    ...options,
    headers,
    credentials: 'include',
  });
  
  if (!response.ok) {
    const errorText = await response.text().catch(() => 'Unknown error');
    throw new Error(`API Error (${response.status}): ${errorText}`);
  }
  
  return response.json();
}

// Convenience methods
export const api = {
  get: <T>(endpoint: string) => apiRequest<T>(endpoint, { method: 'GET' }),
  
  post: <T>(endpoint: string, data: unknown) => 
    apiRequest<T>(endpoint, { 
      method: 'POST', 
      body: JSON.stringify(data) 
    }),
  
  put: <T>(endpoint: string, data: unknown) => 
    apiRequest<T>(endpoint, { 
      method: 'PUT', 
      body: JSON.stringify(data) 
    }),
  
  patch: <T>(endpoint: string, data: unknown) => 
    apiRequest<T>(endpoint, { 
      method: 'PATCH', 
      body: JSON.stringify(data) 
    }),
  
  delete: <T>(endpoint: string) => 
    apiRequest<T>(endpoint, { method: 'DELETE' }),
};
