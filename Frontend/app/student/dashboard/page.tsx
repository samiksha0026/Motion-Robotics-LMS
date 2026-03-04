"use client";

import { useState, useEffect } from "react";
import Image from "next/image";
import { useRouter } from "next/navigation";
import {
  Bot, BookOpen, FileText, Award, Calendar, Bell,
  User, LogOut, Menu, X, Home, CheckCircle, Clock,
  Play, Download, ChevronRight, TrendingUp, Target,
  Video, Book, ClipboardCheck, Trophy, Lock, Loader2,
  AlertCircle, RefreshCw
} from "lucide-react";
import { API_BASE_URL } from "@/lib/api-config";
import { ThemeToggle } from "@/components/ThemeToggle";

interface StudentData {
  name: string;
  username: string;
  grade: string;
  school: string;
  rollNo: string;
}

interface Experiment {
  experimentId: number;
  sequenceOrder: number;
  title: string;
  description?: string;
  estimatedMinutes: number;
  status: "locked" | "available" | "completed" | "approved";
  demoVideoUrl?: string;
  progress?: {
    completed: boolean;
    completedAt?: string;
    isApproved: boolean;
    approvedAt?: string;
    teacherRemarks?: string;
  };
}

interface DigitalBook {
  id: number;
  title: string;
  pages: number;
  downloadUrl: string;
}

interface AssignedLevel {
  id: number;
  levelNumber: number;
  name: string;
  description: string;
  totalExperiments: number;
  unlockedCount: number;
  completedExperiments: number;
  approvedExperiments: number;
  pendingApproval: number;
  progress: number;
  experiments: Experiment[];
  digitalBooks: DigitalBook[];
  examStatus: "locked" | "available" | "completed";
  examScore: number | null;
  certificateStatus: "locked" | "available" | "issued";
  levelCompleted: boolean;
}

export default function StudentDashboard() {
  const router = useRouter();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [activeTab, setActiveTab] = useState("dashboard");
  const [studentData, setStudentData] = useState<StudentData | null>(null);
  const [assignedLevel, setAssignedLevel] = useState<AssignedLevel | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetchStudentData();
  }, []);

  async function getToken() {
    const token = sessionStorage.getItem("jwt");
    if (!token) {
      router.push("/login");
      return null;
    }
    return token;
  }

  function getAuthHeaders(token: string): Record<string, string> {
    const headers: Record<string, string> = { Authorization: `Bearer ${token}` };
    const sessionId = sessionStorage.getItem("sessionId");
    if (sessionId) headers["X-Session-Id"] = sessionId;
    return headers;
  }

  const fetchStudentData = async () => {
    try {
      setLoading(true);
      setError("");

      const token = await getToken();
      if (!token) return;

      // Fetch dashboard data
      const dashboardRes = await fetch(`${API_BASE_URL}/api/student/dashboard`, {
        credentials: "include",
        headers: {
          ...getAuthHeaders(token),
          "Content-Type": "application/json"
        }
      });

      if (dashboardRes.status === 401) {
        router.push("/login");
        return;
      }

      if (!dashboardRes.ok) {
        const text = await dashboardRes.text();
        throw new Error(text || "Failed to fetch dashboard");
      }

      const dashboardData = await dashboardRes.json();

      // Map student info
      setStudentData({
        name: dashboardData.fullName,
        username: dashboardData.email,
        grade: dashboardData.className,
        school: dashboardData.schoolName,
        rollNo: dashboardData.rollNo
      });

      // Fetch experiments data
      const experimentsRes = await fetch(`${API_BASE_URL}/api/student/experiments`, {
        credentials: "include",
        headers: {
          ...getAuthHeaders(token),
          "Content-Type": "application/json"
        }
      });

      let experimentsData = null;
      if (experimentsRes.ok) {
        experimentsData = await experimentsRes.json();
      }

      // Map level/progress data
      if (dashboardData.currentLevel || experimentsData?.level) {
        const level = experimentsData?.level || dashboardData.currentLevel;
        const progress = experimentsData?.progress || dashboardData.progress;
        const experiments = experimentsData?.experiments || [];

        // Map experiments to frontend format
        const mappedExperiments: Experiment[] = experiments.map((exp: {
          experimentId: number;
          sequenceOrder: number;
          title: string;
          description?: string;
          estimatedMinutes: number;
          status: string;
          demoVideoUrl?: string;
          progress?: {
            completed: boolean;
            completedAt?: string;
            isApproved: boolean;
            approvedAt?: string;
            teacherRemarks?: string;
          };
        }) => ({
          experimentId: exp.experimentId,
          sequenceOrder: exp.sequenceOrder,
          title: exp.title,
          description: exp.description,
          estimatedMinutes: exp.estimatedMinutes,
          status: exp.status as "locked" | "available" | "completed" | "approved",
          demoVideoUrl: exp.demoVideoUrl,
          progress: exp.progress
        }));

        setAssignedLevel({
          id: level.levelId,
          levelNumber: level.levelNumber,
          name: level.levelName,
          description: level.description || "",
          totalExperiments: level.totalExperiments || progress?.totalExperiments || 0,
          unlockedCount: experimentsData?.unlockedCount || mappedExperiments.length,
          completedExperiments: progress?.completedExperiments || 0,
          approvedExperiments: progress?.approvedExperiments || 0,
          pendingApproval: progress?.pendingApproval || 0,
          progress: progress?.progressPercentage || 0,
          experiments: mappedExperiments,
          digitalBooks: [], // TODO: Fetch digital books when available
          examStatus: progress?.levelCompleted ? "available" : "locked",
          examScore: null,
          certificateStatus: dashboardData.certificates?.length > 0 ? "issued" : "locked",
          levelCompleted: progress?.levelCompleted || false
        });
      } else {
        // No level assigned
        setAssignedLevel({
          id: 0,
          levelNumber: 0,
          name: "No Level Assigned",
          description: "Your class has not been assigned a robotics level yet. Please contact your teacher or school admin.",
          totalExperiments: 0,
          unlockedCount: 0,
          completedExperiments: 0,
          approvedExperiments: 0,
          pendingApproval: 0,
          progress: 0,
          experiments: [],
          digitalBooks: [],
          examStatus: "locked",
          examScore: null,
          certificateStatus: "locked",
          levelCompleted: false
        });
      }

      setLoading(false);
    } catch (err) {
      console.error("Error fetching student data:", err);
      const errorMessage = err instanceof Error ? err.message : "Failed to load dashboard";
      setError(errorMessage);
      setLoading(false);
    }
  };

  const handleLogout = () => {
    sessionStorage.removeItem("jwt");
    sessionStorage.removeItem("refreshToken");
    sessionStorage.removeItem("userRole");
    sessionStorage.removeItem("role");
    sessionStorage.removeItem("userName");
    sessionStorage.removeItem("schoolContext");
    sessionStorage.removeItem("tokenExpiry");
    router.push("/login");
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-12 h-12 text-indigo-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600">Loading your dashboard...</p>
        </div>
      </div>
    );
  }

  if (error && !studentData) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-white rounded-2xl shadow-xl p-8 max-w-md text-center">
          <AlertCircle className="w-16 h-16 text-red-400 mx-auto mb-4" />
          <h2 className="text-xl font-bold text-gray-900 mb-2">Unable to Load Dashboard</h2>
          <p className="text-gray-500 mb-6">{error}</p>
          <button
            onClick={fetchStudentData}
            className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  const DashboardContent = () => {
    if (!studentData || !assignedLevel) {
      return <div className="text-center py-12">Loading dashboard...</div>;
    }

    return (
      <div className="space-y-6">
        {/* Welcome Section */}
        <div className="bg-gradient-to-r from-indigo-600 to-blue-600 rounded-xl p-6 text-white">
          <div className="flex items-start justify-between">
            <div>
              <h1 className="text-2xl font-bold mb-2">Welcome back, {studentData.name}!</h1>
              <p className="text-indigo-100 mb-4">{studentData.school} - {studentData.grade}</p>
              <div className="inline-flex items-center gap-2 bg-white/20 rounded-lg px-4 py-2">
                <Target size={20} />
                <span className="font-medium">{assignedLevel.name}</span>
              </div>
            </div>
            <button
              onClick={fetchStudentData}
              className="p-2 hover:bg-white/20 rounded-lg transition-colors"
              title="Refresh"
            >
              <RefreshCw size={20} />
            </button>
          </div>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-xl flex items-center gap-3">
            <AlertCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
            <p className="text-red-700 flex-1">{error}</p>
            <button onClick={() => setError("")}>
              <X className="w-5 h-5 text-red-400 hover:text-red-600" />
            </button>
          </div>
        )}

        {/* Progress Stats */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Total Experiments</p>
                <p className="text-2xl font-bold text-gray-900">{assignedLevel.totalExperiments}</p>
              </div>
              <div className="bg-blue-100 p-3 rounded-lg">
                <Video size={24} className="text-blue-600" />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Unlocked</p>
                <p className="text-2xl font-bold text-gray-900">{assignedLevel.unlockedCount}</p>
              </div>
              <div className="bg-indigo-100 p-3 rounded-lg">
                <Lock size={24} className="text-indigo-600" />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Approved</p>
                <p className="text-2xl font-bold text-gray-900">{assignedLevel.approvedExperiments}</p>
              </div>
              <div className="bg-green-100 p-3 rounded-lg">
                <CheckCircle size={24} className="text-green-600" />
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Pending</p>
                <p className="text-2xl font-bold text-gray-900">{assignedLevel.pendingApproval}</p>
              </div>
              <div className="bg-amber-100 p-3 rounded-lg">
                <Clock size={24} className="text-amber-600" />
              </div>
            </div>
          </div>
        </div>

        {/* Overall Progress Bar */}
        <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-lg font-bold text-gray-900">Level Progress</h3>
            <span className="text-sm font-semibold text-indigo-600">{assignedLevel.progress}% Complete</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-3">
            <div
              className="bg-gradient-to-r from-indigo-600 to-blue-600 h-3 rounded-full transition-all"
              style={{ width: `${assignedLevel.progress}%` }}
            />
          </div>
          <p className="text-sm text-gray-600 mt-2">
            {assignedLevel.approvedExperiments} of {assignedLevel.totalExperiments} experiments approved by teacher
          </p>
        </div>

        {/* Experiments List */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
          <div className="p-6 border-b border-gray-200">
            <h2 className="text-xl font-bold text-gray-900">Experiments & Video Tutorials</h2>
            <p className="text-sm text-gray-600 mt-1">
              {assignedLevel.unlockedCount > 0
                ? `${assignedLevel.unlockedCount} experiments unlocked by your teacher`
                : "Your teacher will unlock experiments for you to begin"}
            </p>
          </div>
          <div className="p-6">
            {assignedLevel.experiments.length === 0 ? (
              <div className="text-center py-12">
                <Lock className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-700 mb-2">No Experiments Available</h3>
                <p className="text-gray-500 max-w-md mx-auto">
                  {assignedLevel.id === 0
                    ? "Your class has not been assigned a robotics level yet."
                    : "Your teacher hasn't unlocked any experiments yet. Please wait for them to unlock experiments for your class."}
                </p>
              </div>
            ) : (
              <div className="space-y-3">
                {assignedLevel.experiments.map((exp, index) => (
                  <div
                    key={exp.experimentId}
                    className={`flex items-center justify-between p-4 rounded-lg border transition-all ${exp.status === 'locked'
                        ? 'bg-gray-50 border-gray-200 opacity-60'
                        : exp.status === 'approved'
                          ? 'bg-green-50 border-green-200'
                          : exp.status === 'completed'
                            ? 'bg-amber-50 border-amber-200'
                            : 'bg-white border-gray-200 hover:border-indigo-300 cursor-pointer'
                      }`}
                  >
                    <div className="flex items-center gap-4">
                      <div className={`w-10 h-10 rounded-full flex items-center justify-center font-bold ${exp.status === 'approved' ? 'bg-green-100 text-green-600' :
                          exp.status === 'completed' ? 'bg-amber-100 text-amber-600' :
                            exp.status === 'available' ? 'bg-blue-100 text-blue-600' :
                              'bg-gray-100 text-gray-400'
                        }`}>
                        {exp.status === 'approved' ? <CheckCircle size={20} /> :
                          exp.status === 'completed' ? <Clock size={20} /> :
                            exp.status === 'locked' ? <Lock size={20} /> : exp.sequenceOrder}
                      </div>
                      <div>
                        <h4 className="font-semibold text-gray-900">
                          #{exp.sequenceOrder}: {exp.title}
                        </h4>
                        <p className="text-sm text-gray-600">Duration: {exp.estimatedMinutes} min</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      {exp.status === 'approved' && (
                        <span className="text-xs font-medium px-3 py-1 rounded-full bg-green-100 text-green-700">
                          Approved
                        </span>
                      )}
                      {exp.status === 'completed' && (
                        <span className="text-xs font-medium px-3 py-1 rounded-full bg-amber-100 text-amber-700">
                          Pending Approval
                        </span>
                      )}
                      <button
                        disabled={exp.status === 'locked'}
                        className={`flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-all ${exp.status === 'locked'
                            ? 'bg-gray-200 text-gray-400 cursor-not-allowed'
                            : 'bg-indigo-600 text-white hover:bg-indigo-700'
                          }`}
                        onClick={() => {
                          if (exp.status !== 'locked') {
                            router.push(`/student/experiments?expand=${exp.experimentId}`);
                          }
                        }}
                      >
                        <Play size={16} />
                        {exp.status === 'approved' ? 'Review' : exp.status === 'completed' ? 'View' : 'Start'}
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Exam & Certificate Section */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* MCQ Exam */}
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-xl font-bold text-gray-900 flex items-center gap-2">
                <ClipboardCheck size={24} />
                MCQ Examination
              </h2>
            </div>
            <div className="p-6">
              {assignedLevel.examStatus === 'locked' && (
                <div className="text-center py-6">
                  <div className="bg-gray-100 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4">
                    <Lock size={32} className="text-gray-400" />
                  </div>
                  <h3 className="font-semibold text-gray-900 mb-2">Exam Locked</h3>
                  <p className="text-sm text-gray-600">
                    Complete all {assignedLevel.totalExperiments} experiments and get teacher approval to unlock the exam
                  </p>
                </div>
              )}
              {assignedLevel.examStatus === 'available' && (
                <div className="text-center py-6">
                  <div className="bg-green-100 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4">
                    <ClipboardCheck size={32} className="text-green-600" />
                  </div>
                  <h3 className="font-semibold text-gray-900 mb-2">Exam Available!</h3>
                  <p className="text-sm text-gray-600 mb-4">You've completed all experiments. Take your exam now!</p>
                  <button className="px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 font-medium">
                    Start Exam
                  </button>
                </div>
              )}
              {assignedLevel.examStatus === 'completed' && (
                <div className="text-center py-6">
                  <div className="bg-blue-100 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4">
                    <CheckCircle size={32} className="text-blue-600" />
                  </div>
                  <h3 className="font-semibold text-gray-900 mb-2">Exam Completed</h3>
                  <p className="text-sm text-gray-600 mb-2">Your Score: {assignedLevel.examScore}%</p>
                  <button className="text-indigo-600 hover:text-indigo-700 font-medium text-sm">
                    View Results
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Certificate */}
          <div className="bg-gradient-to-br from-yellow-50 to-orange-50 rounded-xl border border-yellow-200 shadow-sm">
            <div className="p-6">
              <div className="text-center">
                <div className={`w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4 ${assignedLevel.certificateStatus === 'issued' ? 'bg-yellow-100' : 'bg-gray-100'
                  }`}>
                  <Trophy size={32} className={
                    assignedLevel.certificateStatus === 'issued' ? 'text-yellow-600' : 'text-gray-400'
                  } />
                </div>
                <h3 className="font-bold text-gray-900 mb-2">Certificate of Completion</h3>
                {assignedLevel.certificateStatus === 'locked' && (
                  <p className="text-sm text-gray-600">Pass the exam to receive your certificate</p>
                )}
                {assignedLevel.certificateStatus === 'issued' && (
                  <>
                    <p className="text-sm text-gray-600 mb-4">Congratulations! Your certificate is ready</p>
                    <button className="px-6 py-3 bg-gradient-to-r from-yellow-500 to-orange-500 text-white rounded-lg hover:from-yellow-600 hover:to-orange-600 font-medium shadow-lg">
                      Download Certificate
                    </button>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Top Navigation */}
      <nav className="bg-white border-b border-gray-200 sticky top-0 z-40">
        <div className="px-4 h-16 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <button
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="p-2 hover:bg-gray-100 rounded-lg lg:hidden"
            >
              {sidebarOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
            <div className="flex items-center">
              <Image
                src="/images/logo.png"
                alt="Motion Robotics"
                width={140}
                height={45}
                className="object-contain"
                priority
              />
            </div>
          </div>

          <div className="flex items-center gap-4">
            <ThemeToggle />
            <button className="p-2 hover:bg-gray-100 rounded-lg relative">
              <Bell size={20} className="text-gray-600" />
            </button>
            <div className="flex items-center gap-3 pl-4 border-l border-gray-200">
              <div className="hidden sm:block text-right">
                <p className="text-sm font-medium text-gray-900">{studentData?.name || "User"}</p>
                <p className="text-xs text-gray-500">{studentData?.grade || "Grade"}</p>
              </div>
              <div className="w-10 h-10 bg-indigo-100 rounded-full flex items-center justify-center">
                <User size={20} className="text-indigo-600" />
              </div>
            </div>
          </div>
        </div>
      </nav>

      <div className="flex">
        {/* Sidebar */}
        <aside className={`${sidebarOpen ? "translate-x-0" : "-translate-x-full"} lg:translate-x-0 fixed lg:sticky top-16 left-0 h-[calc(100vh-4rem)] w-64 bg-white border-r border-gray-200 transition-transform z-30`}>
          <nav className="p-4 space-y-1">
            <button
              onClick={() => setActiveTab("dashboard")}
              className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${activeTab === "dashboard"
                  ? "bg-indigo-50 text-indigo-600"
                  : "text-gray-700 hover:bg-gray-100"
                }`}
            >
              <Home size={20} />
              <span className="font-medium">Dashboard</span>
            </button>

            <button
              onClick={() => router.push('/student/experiments')}
              className="w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-colors text-gray-700 hover:bg-gray-100"
            >
              <Video size={20} />
              <span className="font-medium">Experiments</span>
            </button>

            <button
              onClick={() => router.push('/student/books')}
              className="w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-colors text-gray-700 hover:bg-gray-100"
            >
              <BookOpen size={20} />
              <span className="font-medium">Digital Books</span>
            </button>

            <button
              onClick={() => router.push('/student/exam')}
              className="w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-colors text-gray-700 hover:bg-gray-100"
            >
              <ClipboardCheck size={20} />
              <span className="font-medium">Examination</span>
            </button>

            <button
              onClick={() => router.push('/student/exam')}
              className="w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-colors text-gray-700 hover:bg-gray-100"
            >
              <Award size={20} />
              <span className="font-medium">Results</span>
            </button>

            <button
              onClick={() => router.push('/student/certificate')}
              className="w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-colors text-gray-700 hover:bg-gray-100"
            >
              <Trophy size={20} />
              <span className="font-medium">Certificate</span>
            </button>

            <div className="pt-4 mt-4 border-t border-gray-200">
              <button
                onClick={handleLogout}
                className="w-full flex items-center gap-3 px-4 py-3 rounded-lg text-red-600 hover:bg-red-50 transition-colors"
              >
                <LogOut size={20} />
                <span className="font-medium">Logout</span>
              </button>
            </div>
          </nav>
        </aside>

        {/* Main Content */}
        <main className="flex-1 p-6 lg:p-8">
          <DashboardContent />
        </main>
      </div>
    </div>
  );
}
