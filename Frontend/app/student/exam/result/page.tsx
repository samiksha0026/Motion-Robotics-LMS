"use client";

import { useState, useEffect } from "react";
import {
  Trophy, XCircle, CheckCircle, Download, ArrowLeft,
  BarChart3, TrendingUp, Award, Lock, Loader, AlertCircle
} from "lucide-react";
import { API_BASE_URL } from "@/lib/api-config";

interface ExamResult {
  examId: number;
  examTitle: string;
  level: string;
  totalScore: number;
  maxScore: number;
  percentage: number;
  passingScore: number;
  submittedAt: string;
  sectionResults: Array<{
    title: string;
    attempted: number;
    total: number;
    scored: number;
    maxMarks: number;
  }>;
  certificateStatus: "available" | "issued" | "locked";
  certificateUrl?: string;
}

export default function ExamResultPage() {
  const [result, setResult] = useState<ExamResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCertificate, setShowCertificate] = useState(false);

  useEffect(() => {
    fetchExamResult();
  }, []);

  const fetchExamResult = async () => {
    try {
      setLoading(true);
      setError(null);

      const token = typeof window !== "undefined" ? sessionStorage.getItem("jwt") : null;

      if (!token) {
        throw new Error("No authentication token found");
      }

      // Get exam ID from URL query params
      const examId = new URLSearchParams(window.location.search).get("id") || "1";

      const response = await fetch(
        `${API_BASE_URL}/api/student/exams/${examId}/result`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error("Failed to fetch exam result");
      }

      const data = await response.json();
      setResult(data);
    } catch (err) {
      console.error("Error fetching result:", err);
      setError("Failed to load exam result. Please try again.");
      setLoading(false);
    }
  };

  const downloadCertificate = async () => {
    try {
      if (!result?.certificateUrl) {
        alert("Certificate not available");
        return;
      }

      const link = document.createElement("a");
      link.href = result.certificateUrl;
      link.download = `certificate-exam-${result.examId}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch (err) {
      console.error("Error downloading certificate:", err);
      alert("Failed to download certificate");
    }
  };

  // Loading state
  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader className="w-12 h-12 text-indigo-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600 font-medium">Loading your results...</p>
        </div>
      </div>
    );
  }

  // Error state
  if (error && !result) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center max-w-md">
          <AlertCircle className="w-12 h-12 text-red-600 mx-auto mb-4" />
          <p className="text-gray-900 font-medium mb-2">Failed to Load Results</p>
          <p className="text-gray-600 text-sm mb-4">{error}</p>
          <button
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Try Again
          </button>
        </div>
      </div>
    );
  }

  if (!result) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600 font-medium">No results found</p>
        </div>
      </div>
    );
  }

  const isPassed = result.percentage >= result.passingScore;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 sticky top-0 z-30">
        <div className="max-w-7xl mx-auto px-4 h-16 flex items-center justify-between">
          <button
            onClick={() => window.history.back()}
            className="flex items-center gap-2 px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <ArrowLeft size={20} />
            <span className="font-medium hidden sm:inline">Back</span>
          </button>
          <h1 className="text-xl font-bold text-gray-900">Exam Results</h1>
          <div className="w-32" />
        </div>
      </header>

      <div className="max-w-4xl mx-auto p-4 lg:p-6 space-y-6">
        {/* Main Result Card */}
        <div className="bg-white rounded-2xl shadow-lg border border-gray-200 overflow-hidden">
          <div
            className={`p-8 text-white text-center ${
              isPassed
                ? "bg-gradient-to-r from-green-600 to-emerald-600"
                : "bg-gradient-to-r from-red-600 to-orange-600"
            }`}
          >
            <div className="bg-white/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-4">
              {isPassed ? (
                <Trophy size={48} />
              ) : (
                <XCircle size={48} />
              )}
            </div>
            <h1 className="text-3xl font-bold mb-2">
              {isPassed ? "Congratulations! 🎉" : "Keep Practicing! 💪"}
            </h1>
            <p className="text-lg opacity-90">
              {isPassed
                ? "You have passed the examination!"
                : "You need more practice to pass"}
            </p>
          </div>

          <div className="p-8">
            {/* Score Overview */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
              <div className="bg-blue-50 rounded-xl p-6 text-center border border-blue-200">
                <p className="text-sm text-gray-600 mb-2">Your Score</p>
                <p className="text-4xl font-bold text-blue-600">
                  {result.percentage}%
                </p>
                <p className="text-sm text-gray-600 mt-2">
                  {result.totalScore} / {result.maxScore} marks
                </p>
              </div>

              <div className="bg-purple-50 rounded-xl p-6 text-center border border-purple-200">
                <p className="text-sm text-gray-600 mb-2">Passing Score</p>
                <p className="text-4xl font-bold text-purple-600">
                  {result.passingScore}%
                </p>
                <p className="text-sm text-gray-600 mt-2">Minimum required</p>
              </div>

              <div
                className={`rounded-xl p-6 text-center border ${
                  isPassed
                    ? "bg-green-50 border-green-200"
                    : "bg-red-50 border-red-200"
                }`}
              >
                <p className="text-sm text-gray-600 mb-2">Result</p>
                <p
                  className={`text-4xl font-bold ${
                    isPassed ? "text-green-600" : "text-red-600"
                  }`}
                >
                  {isPassed ? "PASS" : "FAIL"}
                </p>
                <p className="text-sm text-gray-600 mt-2">
                  Difference: {isPassed ? "+" : ""}{result.percentage - result.passingScore}%
                </p>
              </div>
            </div>

            {/* Exam Details */}
            <div className="bg-gray-50 rounded-xl p-6 mb-8">
              <h3 className="font-bold text-gray-900 mb-4">Exam Details</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <p className="text-sm text-gray-600 mb-1">Exam Title</p>
                  <p className="text-gray-900 font-medium">{result.examTitle}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 mb-1">Level</p>
                  <p className="text-gray-900 font-medium">{result.level}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 mb-1">Submitted At</p>
                  <p className="text-gray-900 font-medium">
                    {new Date(result.submittedAt).toLocaleDateString()} at{" "}
                    {new Date(result.submittedAt).toLocaleTimeString()}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 mb-1">Total Marks</p>
                  <p className="text-gray-900 font-medium">{result.maxScore}</p>
                </div>
              </div>
            </div>

            {/* Section-wise Performance */}
            <div className="bg-white rounded-xl p-6 mb-8 border border-gray-200">
              <h3 className="font-bold text-gray-900 mb-6 flex items-center gap-2">
                <BarChart3 size={24} className="text-indigo-600" />
                Section-wise Performance
              </h3>
              <div className="space-y-6">
                {result.sectionResults.map((section, idx) => {
                  const percentage = Math.round(
                    (section.scored / section.maxMarks) * 100
                  );
                  return (
                    <div key={idx}>
                      <div className="flex justify-between items-center mb-3">
                        <div>
                          <p className="font-semibold text-gray-900">
                            {section.title}
                          </p>
                          <p className="text-sm text-gray-600">
                            {section.attempted}/{section.total} questions answered
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="text-2xl font-bold text-indigo-600">
                            {section.scored}/{section.maxMarks}
                          </p>
                          <p className="text-sm text-gray-600">{percentage}%</p>
                        </div>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-3">
                        <div
                          className={`h-3 rounded-full transition-all ${
                            percentage >= result.passingScore
                              ? "bg-green-600"
                              : "bg-orange-500"
                          }`}
                          style={{ width: `${percentage}%` }}
                        />
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Strengths and Areas for Improvement */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
              <div className="bg-green-50 rounded-xl p-6 border border-green-200">
                <h3 className="font-bold text-gray-900 mb-4 flex items-center gap-2">
                  <CheckCircle size={20} className="text-green-600" />
                  Your Strengths
                </h3>
                <ul className="space-y-2 text-sm text-gray-700">
                  <li className="flex items-start gap-2">
                    <span className="text-green-600 mt-0.5">✓</span>
                    <span>Good understanding of core concepts</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <span className="text-green-600 mt-0.5">✓</span>
                    <span>Strong performance in multiple choice questions</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <span className="text-green-600 mt-0.5">✓</span>
                    <span>Consistent answers across sections</span>
                  </li>
                </ul>
              </div>

              <div className="bg-orange-50 rounded-xl p-6 border border-orange-200">
                <h3 className="font-bold text-gray-900 mb-4 flex items-center gap-2">
                  <TrendingUp size={20} className="text-orange-600" />
                  Areas for Improvement
                </h3>
                <ul className="space-y-2 text-sm text-gray-700">
                  <li className="flex items-start gap-2">
                    <span className="text-orange-600 mt-0.5">•</span>
                    <span>Review diagram-based questions</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <span className="text-orange-600 mt-0.5">•</span>
                    <span>Practice fill-in-the-blank answers</span>
                  </li>
                  <li className="flex items-start gap-2">
                    <span className="text-orange-600 mt-0.5">•</span>
                    <span>Improve time management during exams</span>
                  </li>
                </ul>
              </div>
            </div>

            {/* Certificate Section */}
            {isPassed && (
              <div className="bg-gradient-to-r from-yellow-50 to-orange-50 border border-yellow-200 rounded-xl p-6 mb-8 text-center">
                <Award size={32} className="text-yellow-600 mx-auto mb-3" />
                <h3 className="font-bold text-gray-900 mb-2">
                  Certificate Available!
                </h3>
                <p className="text-sm text-gray-600 mb-4">
                  {result.certificateStatus === "issued"
                    ? "Your certificate has been issued"
                    : "Download your certificate of completion"}
                </p>
                {result.certificateStatus !== "locked" && (
                  <button
                    onClick={downloadCertificate}
                    className="px-6 py-3 bg-gradient-to-r from-yellow-500 to-orange-500 text-white rounded-lg hover:from-yellow-600 hover:to-orange-600 font-medium inline-flex items-center gap-2"
                  >
                    <Download size={20} />
                    {result.certificateStatus === "issued"
                      ? "Download Certificate"
                      : "Get Certificate"}
                  </button>
                )}
              </div>
            )}

            {/* Action Buttons */}
            <div className="flex gap-4 flex-wrap">
              <button
                onClick={() => window.location.href = "/student/dashboard"}
                className="flex-1 min-w-[200px] px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-medium transition-colors"
              >
                Back to Dashboard
              </button>
              {!isPassed && (
                <button
                  onClick={() =>
                    window.location.href = `/student/exam?id=${result.examId}`
                  }
                  className="flex-1 min-w-[200px] px-6 py-3 bg-white border-2 border-indigo-600 text-indigo-600 rounded-lg hover:bg-indigo-50 font-medium transition-colors"
                >
                  Retake Exam
                </button>
              )}
              <button
                onClick={() =>
                  window.location.href = "/student/dashboard"
                }
                className="flex-1 min-w-[200px] px-6 py-3 bg-white border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 font-medium transition-colors"
              >
                View All Results
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
