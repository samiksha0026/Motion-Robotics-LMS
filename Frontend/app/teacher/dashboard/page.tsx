"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import Image from "next/image";
import {
  Users,
  LogOut,
  Clock,
  CheckCircle,
  XCircle,
  Beaker,
  Loader2,
  RefreshCw,
  AlertCircle,
  ChevronRight,
  Unlock,
  GraduationCap,
  BookOpen,
  Award,
  Eye,
  X
} from "lucide-react";
import { API_BASE_URL } from "@/lib/api-config";
import { ThemeToggle } from "@/components/ThemeToggle";

interface TeacherDashboard {
  teacherId: number;
  fullName: string;
  email: string;
  schoolName: string;
  totalClasses: number;
  totalStudents: number;
  pendingApprovals: number;
  currentAcademicYear: string;
  assignedClasses: AssignedClass[];
  recentPendingApprovals: PendingApproval[];
}

interface AssignedClass {
  classId: number;
  className: string;
  studentCount: number;
  roboticsLevelId: number;
  levelNumber: number;
  levelName: string;
  experimentsCompleted: number;
  totalExperiments: number;
  pendingApprovals: number;
}

interface PendingApproval {
  progressId: number;
  studentId: number;
  studentName: string;
  rollNumber: string;
  classId: number;
  className: string;
  experimentId: number;
  experimentSequence: number;
  experimentTitle: string;
  completedAt: string;
  submissionNotes: string | null;
  submissionImageUrl: string | null;
}

export default function TeacherDashboard() {
  const router = useRouter();
  const [dashboard, setDashboard] = useState<TeacherDashboard | null>(null);
  const [pendingApprovals, setPendingApprovals] = useState<PendingApproval[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isApproving, setIsApproving] = useState<number | null>(null);
  const [error, setError] = useState("");
  const [showApprovalModal, setShowApprovalModal] = useState(false);
  const [selectedApproval, setSelectedApproval] = useState<PendingApproval | null>(null);
  const [rejectReason, setRejectReason] = useState("");

  useEffect(() => {
    loadDashboard();
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

  async function loadDashboard() {
    try {
      setIsLoading(true);
      setError("");
      const token = await getToken();
      if (!token) return;

      const res = await fetch(`${API_BASE_URL}/api/teacher/dashboard`, {
        credentials: "include",
        headers: getAuthHeaders(token)
      });

      if (res.status === 401) {
        router.push("/login");
        return;
      }

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Failed to load dashboard");
      }

      const data = await res.json();
      setDashboard(data);
      setPendingApprovals(data.recentPendingApprovals || []);
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : "Failed to load dashboard";
      setError(errorMessage);
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  }

  async function handleApproval(progressId: number, approve: boolean, remarks?: string) {
    try {
      setIsApproving(progressId);
      const token = await getToken();
      if (!token) return;

      const res = await fetch(`${API_BASE_URL}/api/teacher/approvals/${progressId}`, {
        method: "POST",
        credentials: "include",
        headers: {
          ...getAuthHeaders(token),
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ approve, teacherRemarks: remarks })
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Failed to process approval");
      }

      setPendingApprovals(prev => prev.filter(p => p.progressId !== progressId));

      if (dashboard) {
        setDashboard({
          ...dashboard,
          pendingApprovals: Math.max(0, dashboard.pendingApprovals - 1)
        });
      }

      setShowApprovalModal(false);
      setSelectedApproval(null);
      setRejectReason("");
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : "Failed to process";
      setError(errorMessage);
    } finally {
      setIsApproving(null);
    }
  }

  function handleLogout() {
    sessionStorage.removeItem("jwt");
    sessionStorage.removeItem("refreshToken");
    sessionStorage.removeItem("userRole");
    sessionStorage.removeItem("role");
    sessionStorage.removeItem("userName");
    sessionStorage.removeItem("schoolContext");
    sessionStorage.removeItem("tokenExpiry");
    router.push("/login");
  }

  function openApprovalDetails(approval: PendingApproval) {
    setSelectedApproval(approval);
    setShowApprovalModal(true);
    setRejectReason("");
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-12 h-12 text-indigo-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600">Loading your dashboard...</p>
        </div>
      </div>
    );
  }

  if (!dashboard) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-white rounded-2xl shadow-xl p-8 max-w-md text-center">
          <AlertCircle className="w-16 h-16 text-red-400 mx-auto mb-4" />
          <h2 className="text-xl font-bold text-gray-900 mb-2">Unable to Load Dashboard</h2>
          <p className="text-gray-500 mb-6">{error || "Please try again"}</p>
          <button
            onClick={loadDashboard}
            className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm sticky top-0 z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center gap-3">
              <Image
                src="/images/logo.png"
                alt="Motion Robotics"
                width={40}
                height={40}
                className="rounded-lg"
              />
              <div>
                <h1 className="text-lg font-bold text-gray-900">Motion Robotics</h1>
                <p className="text-xs text-gray-500">Teacher Portal</p>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <ThemeToggle />
              <button
                onClick={loadDashboard}
                className="p-2 text-gray-500 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
                title="Refresh"
              >
                <RefreshCw className="w-5 h-5" />
              </button>
              <button
                onClick={handleLogout}
                className="flex items-center gap-2 px-4 py-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
              >
                <LogOut className="w-5 h-5" />
                <span className="hidden sm:inline">Logout</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Welcome Section */}
        <div className="bg-gradient-to-r from-indigo-600 to-blue-600 rounded-xl p-6 text-white mb-8">
          <h2 className="text-2xl sm:text-3xl font-bold">
            Welcome back, {dashboard.fullName}!
          </h2>
          <p className="text-indigo-100 mt-1">
            {dashboard.schoolName} - {dashboard.currentAcademicYear}
          </p>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl flex items-center gap-3">
            <AlertCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
            <p className="text-red-700 flex-1">{error}</p>
            <button onClick={() => setError("")}>
              <X className="w-5 h-5 text-red-400 hover:text-red-600" />
            </button>
          </div>
        )}

        {/* Quick Stats */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <div className="bg-white rounded-xl p-5 shadow-sm border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-blue-100 rounded-xl flex items-center justify-center">
                <BookOpen className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{dashboard.totalClasses}</p>
                <p className="text-sm text-gray-500">Classes</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl p-5 shadow-sm border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-indigo-100 rounded-xl flex items-center justify-center">
                <Users className="w-6 h-6 text-indigo-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{dashboard.totalStudents}</p>
                <p className="text-sm text-gray-500">Students</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl p-5 shadow-sm border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-amber-100 rounded-xl flex items-center justify-center">
                <Clock className="w-6 h-6 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{dashboard.pendingApprovals}</p>
                <p className="text-sm text-gray-500">Pending</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl p-5 shadow-sm border border-gray-200">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center">
                <Beaker className="w-6 h-6 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">
                  {dashboard.assignedClasses.reduce((sum, c) => sum + c.totalExperiments, 0)}
                </p>
                <p className="text-sm text-gray-500">Experiments</p>
              </div>
            </div>
          </div>
        </div>

        <div className="grid lg:grid-cols-3 gap-8">
          {/* Left Column - Classes */}
          <div className="lg:col-span-2 space-y-6">
            {/* My Classes */}
            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
              <div className="p-6 border-b border-gray-200">
                <h3 className="text-lg font-bold text-gray-900">My Classes</h3>
                <p className="text-sm text-gray-500 mt-1">Click on a class to manage experiments</p>
              </div>

              {dashboard.assignedClasses.length === 0 ? (
                <div className="p-12 text-center">
                  <BookOpen className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                  <p className="text-gray-500">No classes assigned yet</p>
                </div>
              ) : (
                <div className="divide-y divide-gray-100">
                  {dashboard.assignedClasses.map((cls) => (
                    <div
                      key={cls.classId}
                      className="p-5 hover:bg-gray-50 transition-colors cursor-pointer"
                      onClick={() => router.push(`/teacher/experiments/${cls.classId}`)}
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className="w-12 h-12 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-xl flex items-center justify-center text-white font-bold">
                            L{cls.levelNumber}
                          </div>
                          <div>
                            <h4 className="font-semibold text-gray-900">{cls.className}</h4>
                            <p className="text-sm text-indigo-600">{cls.levelName}</p>
                          </div>
                        </div>
                        <div className="flex items-center gap-6">
                          <div className="text-right hidden sm:block">
                            <p className="text-sm font-medium text-gray-900">{cls.studentCount} students</p>
                            <p className="text-xs text-gray-500">{cls.totalExperiments} experiments</p>
                          </div>
                          {cls.pendingApprovals > 0 && (
                            <span className="px-3 py-1 bg-amber-100 text-amber-700 text-sm font-medium rounded-full">
                              {cls.pendingApprovals} pending
                            </span>
                          )}
                          <ChevronRight className="w-5 h-5 text-gray-400" />
                        </div>
                      </div>

                      {/* Progress bar */}
                      <div className="mt-4">
                        <div className="flex items-center justify-between text-xs text-gray-500 mb-1">
                          <span>Overall Progress</span>
                          <span>
                            {cls.experimentsCompleted} / {cls.totalExperiments * cls.studentCount} completions
                          </span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div
                            className="bg-gradient-to-r from-indigo-600 to-blue-600 h-2 rounded-full transition-all"
                            style={{
                              width: cls.totalExperiments * cls.studentCount > 0
                                ? `${(cls.experimentsCompleted / (cls.totalExperiments * cls.studentCount)) * 100}%`
                                : "0%"
                            }}
                          />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Right Column - Quick Actions & Approvals */}
          <div className="space-y-6">
            {/* Quick Actions */}
            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 p-6">
              <h3 className="text-lg font-bold text-gray-900 mb-4">Quick Actions</h3>
              <div className="space-y-3">
                <button
                  onClick={() => router.push("/teacher/levels")}
                  className="w-full flex items-center gap-3 p-4 bg-indigo-50 hover:bg-indigo-100 rounded-xl text-left transition-colors"
                >
                  <div className="w-10 h-10 bg-indigo-100 rounded-lg flex items-center justify-center">
                    <Unlock className="w-5 h-5 text-indigo-600" />
                  </div>
                  <div>
                    <p className="font-medium text-gray-900">Unlock Experiments</p>
                    <p className="text-xs text-gray-500">Manage class experiments</p>
                  </div>
                </button>

                <button
                  onClick={() => router.push("/teacher/reports")}
                  className="w-full flex items-center gap-3 p-4 bg-blue-50 hover:bg-blue-100 rounded-xl text-left transition-colors"
                >
                  <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                    <Award className="w-5 h-5 text-blue-600" />
                  </div>
                  <div>
                    <p className="font-medium text-gray-900">View Reports</p>
                    <p className="text-xs text-gray-500">Student progress reports</p>
                  </div>
                </button>
              </div>
            </div>

            {/* Pending Approvals */}
            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
              <div className="p-6 border-b border-gray-200">
                <div className="flex items-center justify-between">
                  <h3 className="text-lg font-bold text-gray-900">Pending Approvals</h3>
                  {pendingApprovals.length > 0 && (
                    <span className="px-2 py-1 bg-amber-100 text-amber-700 text-xs font-medium rounded-full">
                      {pendingApprovals.length}
                    </span>
                  )}
                </div>
              </div>

              {pendingApprovals.length === 0 ? (
                <div className="p-8 text-center">
                  <CheckCircle className="w-12 h-12 text-green-300 mx-auto mb-3" />
                  <p className="text-gray-500">All caught up!</p>
                  <p className="text-xs text-gray-400 mt-1">No pending approvals</p>
                </div>
              ) : (
                <div className="divide-y divide-gray-100 max-h-96 overflow-y-auto">
                  {pendingApprovals.map((approval) => (
                    <div
                      key={approval.progressId}
                      className="p-4 hover:bg-gray-50 transition-colors"
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div className="flex-1 min-w-0">
                          <p className="font-medium text-gray-900 truncate">
                            {approval.studentName}
                          </p>
                          <p className="text-xs text-gray-500 mt-0.5">
                            {approval.className}
                          </p>
                          <p className="text-sm text-indigo-600 mt-1 truncate">
                            Exp #{approval.experimentSequence}: {approval.experimentTitle}
                          </p>
                        </div>
                        <div className="flex items-center gap-2 flex-shrink-0">
                          <button
                            onClick={() => openApprovalDetails(approval)}
                            className="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                            title="View Details"
                          >
                            <Eye className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => handleApproval(approval.progressId, true)}
                            disabled={isApproving === approval.progressId}
                            className="p-2 text-green-600 hover:bg-green-50 rounded-lg disabled:opacity-50 transition-colors"
                            title="Approve"
                          >
                            {isApproving === approval.progressId ? (
                              <Loader2 className="w-4 h-4 animate-spin" />
                            ) : (
                              <CheckCircle className="w-4 h-4" />
                            )}
                          </button>
                          <button
                            onClick={() => openApprovalDetails(approval)}
                            className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                            title="Reject"
                          >
                            <XCircle className="w-4 h-4" />
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </main>

      {/* Approval Details Modal */}
      {showApprovalModal && selectedApproval && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl max-w-lg w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-bold text-gray-900">Approval Details</h3>
                <button
                  onClick={() => {
                    setShowApprovalModal(false);
                    setSelectedApproval(null);
                  }}
                  className="p-2 hover:bg-gray-100 rounded-lg"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>
            </div>

            <div className="p-6 space-y-4">
              <div className="flex items-center gap-4">
                <div className="w-14 h-14 bg-indigo-100 rounded-full flex items-center justify-center">
                  <GraduationCap className="w-7 h-7 text-indigo-600" />
                </div>
                <div>
                  <h4 className="font-semibold text-gray-900 text-lg">{selectedApproval.studentName}</h4>
                  <p className="text-sm text-gray-500">
                    {selectedApproval.className} | Roll: {selectedApproval.rollNumber}
                  </p>
                </div>
              </div>

              <div className="bg-gray-50 rounded-xl p-4">
                <p className="text-sm text-gray-500 mb-1">Experiment</p>
                <p className="font-medium text-gray-900">
                  #{selectedApproval.experimentSequence}: {selectedApproval.experimentTitle}
                </p>
              </div>

              {selectedApproval.submissionNotes && (
                <div className="bg-blue-50 rounded-xl p-4">
                  <p className="text-sm text-blue-600 mb-1">Student Notes</p>
                  <p className="text-gray-700">{selectedApproval.submissionNotes}</p>
                </div>
              )}

              <div>
                <p className="text-xs text-gray-400">
                  Submitted: {new Date(selectedApproval.completedAt).toLocaleString()}
                </p>
              </div>

              <div className="pt-4 border-t border-gray-200">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Rejection Reason (optional)
                </label>
                <textarea
                  value={rejectReason}
                  onChange={(e) => setRejectReason(e.target.value)}
                  placeholder="Enter reason if rejecting..."
                  rows={3}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 resize-none"
                />
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex gap-3">
              <button
                onClick={() => handleApproval(selectedApproval.progressId, false, rejectReason || "Needs improvement")}
                disabled={isApproving === selectedApproval.progressId}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-red-50 text-red-600 rounded-xl hover:bg-red-100 font-medium disabled:opacity-50 transition-colors"
              >
                {isApproving === selectedApproval.progressId ? (
                  <Loader2 className="w-5 h-5 animate-spin" />
                ) : (
                  <XCircle className="w-5 h-5" />
                )}
                Reject
              </button>
              <button
                onClick={() => handleApproval(selectedApproval.progressId, true)}
                disabled={isApproving === selectedApproval.progressId}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-indigo-600 text-white rounded-xl hover:bg-indigo-700 font-medium disabled:opacity-50 transition-colors"
              >
                {isApproving === selectedApproval.progressId ? (
                  <Loader2 className="w-5 h-5 animate-spin" />
                ) : (
                  <CheckCircle className="w-5 h-5" />
                )}
                Approve
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
