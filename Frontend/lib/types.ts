// Shared TypeScript types for the LMS application

// ==================== User Types ====================
export interface User {
  id: string;
  name: string;
  email: string;
  role: 'admin' | 'teacher' | 'student';
  avatar?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Admin extends User {
  role: 'admin';
  permissions: string[];
}

export interface Teacher extends User {
  role: 'teacher';
  school: string;
  schoolId: string;
  assignedClasses: string[];
  assignedProgram: string;
  assignedLevel: string;
  courseCount: number;
  studentCount: number;
}

export interface Student extends User {
  role: 'student';
  username: string;
  school: string;
  schoolId: string;
  grade: string;
  rollNo: string;
  enrolledCourses: number;
  assignedLevel?: AssignedLevel;
}

// ==================== School Types ====================
export interface School {
  id: string;
  name: string;
  address?: string;
  city?: string;
  state?: string;
  pincode?: string;
  phone?: string;
  email?: string;
  studentCount: number;
  teacherCount: number;
  isActive: boolean;
  createdAt: string;
}

// ==================== Course Types ====================
export interface Course {
  id: string;
  name: string;
  description?: string;
  level: 'beginner' | 'intermediate' | 'advanced';
  duration: string;
  totalExperiments: number;
  studentCount: number;
  isActive: boolean;
  thumbnail?: string;
  createdAt: string;
}

export interface AssignedLevel {
  id: number;
  name: string;
  description: string;
  totalExperiments: number;
  completedExperiments: number;
  progress: number;
  experiments: Experiment[];
  digitalBooks: DigitalBook[];
  examStatus: 'locked' | 'available' | 'completed';
  examScore: number | null;
  certificateStatus: 'locked' | 'available' | 'issued';
}

// ==================== Content Types ====================
export interface Experiment {
  id: number;
  title: string;
  description?: string;
  duration: string;
  level?: string;
  status: 'completed' | 'in-progress' | 'locked';
  videoUrl: string;
  objectives?: string[];
  components?: string[];
  steps?: string[];
  resources?: Resource[];
  quiz?: QuizQuestion[];
}

export interface DigitalBook {
  id: number;
  title: string;
  description?: string;
  pages: number;
  fileSize?: string;
  downloadUrl: string;
  coverImage?: string;
  author?: string;
  version?: string;
}

export interface Resource {
  id: number;
  name: string;
  type: 'pdf' | 'video' | 'image' | 'document';
  url: string;
}

export interface ContentItem {
  id: string;
  title: string;
  filename: string;
  fileType: string;
  fileSize: number;
  uploadedAt: string;
  uploadedBy: string;
  downloadUrl: string;
}

// ==================== Exam Types ====================
export interface Exam {
  id: string;
  title: string;
  level: string;
  totalQuestions: number;
  duration: number; // in minutes
  passingScore: number;
  date?: string;
  status: 'draft' | 'scheduled' | 'active' | 'completed';
  sections: ExamSection[];
  totalStudents?: number;
  completedStudents?: number;
}

export interface ExamSection {
  id: string;
  title: string;
  type: 'mcq' | 'image' | 'fillblank';
  totalQuestions: number;
  marks: number;
  questions: Question[];
}

export interface Question {
  id: number;
  question: string;
  imageUrl?: string;
  imageAlt?: string;
  options?: string[];
  correctAnswer?: number | string;
  acceptedAnswers?: string[];
  marks: number;
}

export interface QuizQuestion {
  id: number;
  question: string;
  options: string[];
  correctAnswer: number;
}

export interface ExamResult {
  examId: number;
  examTitle: string;
  level: string;
  totalScore: number;
  maxScore: number;
  percentage: number;
  passingScore: number;
  passed: boolean;
  submittedAt: string;
  sectionResults: SectionResult[];
  certificateStatus: 'available' | 'issued' | 'locked';
  certificateUrl?: string;
}

export interface SectionResult {
  title: string;
  attempted: number;
  total: number;
  scored: number;
  maxMarks: number;
}

// ==================== Certificate Types ====================
export interface Certificate {
  id: string;
  studentId: string;
  studentName: string;
  level: string;
  issuedDate: string;
  certificateNumber: string;
  score: number;
  certificateUrl: string;
  status: 'pending' | 'issued' | 'revoked';
  exams: CertificateExam[];
}

export interface CertificateExam {
  title: string;
  completedDate: string;
  score: number;
}

// ==================== Report Types ====================
export interface Report {
  id: string;
  title: string;
  type: 'class-performance' | 'student-progress' | 'exam-analysis';
  category: 'performance' | 'courses' | 'certificates';
  generatedDate: string;
  downloadUrl: string;
}

// ==================== Notification Types ====================
export interface Notification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  read: boolean;
  createdAt: string;
  link?: string;
}

// ==================== API Response Types ====================
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ==================== Auth Types ====================
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  role: string;
  name: string;
  school?: string;
  assignedClass?: string;
  assignedProgram?: string;
  assignedLevel?: string;
}

export interface SessionData {
  token: string;
  role: string;
  name?: string;
  [key: string]: unknown;
}

// ==================== Dashboard Types ====================
export interface DashboardStats {
  totalStudents: number;
  totalTeachers: number;
  totalSchools: number;
  totalCourses: number;
  activeSessions: number;
  certificatesIssued: number;
  completionRate: number;
}

export interface StudentDashboardData {
  student: Student;
  assignedLevel: AssignedLevel;
  recentActivity: ActivityItem[];
  upcomingExams: Exam[];
}

export interface ActivityItem {
  id: string;
  type: 'experiment' | 'exam' | 'certificate' | 'book';
  title: string;
  timestamp: string;
  status?: string;
}

// ==================== Settings Types ====================
export interface SystemSettings {
  siteName: string;
  siteDescription: string;
  contactEmail: string;
  timezone: string;
  language: string;
  maintenanceMode: boolean;
  allowRegistration: boolean;
  requireEmailVerification: boolean;
  sessionTimeout: number;
  maxLoginAttempts: number;
  enableNotifications: boolean;
  emailNotifications: boolean;
  smsNotifications: boolean;
}
