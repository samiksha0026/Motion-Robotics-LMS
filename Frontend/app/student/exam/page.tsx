"use client";

import { useState, useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import {
  Clock, AlertCircle, CheckCircle, XCircle, Send,
  BookOpen, Edit3, Trophy, Loader2, ArrowLeft, Lock
} from "lucide-react";

import { API_BASE_URL } from "@/lib/api-config";

interface ExamPreview {
  examId: number;
  title: string;
  description?: string;
  durationMinutes: number;
  totalQuestions: number;
  totalMarks: number;
  passingPercentage: number;
  passingMarks: number;
  roboticsLevelName: string;
  levelNumber: number;
}

interface EligibilityData {
  isEligible: boolean;
  message: string;
  totalExperiments: number;
  completedExperiments: number;
  pendingExperiments: number;
  hasAttemptedExam: boolean;
  hasPassedExam: boolean;
  exam?: ExamPreview;
}

interface ApiQuestion {
  questionNumber: number;
  questionText: string;
  questionType: string;
  options: string[];
  marks: number;
}

interface ExamQuestionsData {
  examId: number;
  title: string;
  durationMinutes: number;
  totalQuestions: number;
  totalMarks: number;
  startedAt: string;
  mustEndBy: string;
  questions: ApiQuestion[];
}

interface QuestionResult {
  questionNumber: number;
  questionText: string;
  yourAnswer: string;
  correctAnswer: string;
  isCorrect: boolean;
  marksObtained: number;
  totalMarks: number;
  explanation?: string;
}

interface ExamResultData {
  resultId: number;
  examTitle: string;
  roboticsLevelName: string;
  scoreObtained: number;
  totalMarks: number;
  percentage: number;
  isPassed: boolean;
  resultMessage: string;
  timeTakenFormatted: string;
  certificateGenerated: boolean;
  questionResults?: QuestionResult[];
}

function getHeaders(): Record<string, string> {
  const token = typeof window !== "undefined" ? sessionStorage.getItem("jwt") : null;
  const sessionId = typeof window !== "undefined" ? sessionStorage.getItem("sessionId") : null;
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(sessionId ? { "X-Session-Id": sessionId } : {}),
  };
}

type PageState = "loading" | "ineligible" | "eligible" | "taking" | "result" | "error";

export default function StudentExamPage() {
  const router = useRouter();
  const startTimeRef = useRef<number>(0);

  const [pageState, setPageState] = useState<PageState>("loading");
  const [eligibility, setEligibility] = useState<EligibilityData | null>(null);
  const [examData, setExamData] = useState<ExamQuestionsData | null>(null);
  const [result, setResult] = useState<ExamResultData | null>(null);
  const [error, setError] = useState("");
  const [answers, setAnswers] = useState<Record<number, string>>({});
  const [timeRemaining, setTimeRemaining] = useState(0);
  const [submitting, setSubmitting] = useState(false);
  const [showReview, setShowReview] = useState(false);

  useEffect(() => {
    (async () => {
      setPageState("loading");
      try {
        const token = sessionStorage.getItem("jwt");
        if (!token) { router.push("/login"); return; }
        const res = await fetch(`${API_BASE_URL}/api/student/exam/eligibility`, {
          credentials: "include",
          headers: getHeaders(),
        });
        if (res.status === 401) { router.push("/login"); return; }
        if (!res.ok) throw new Error("Failed to check exam eligibility");
        const data: EligibilityData = await res.json();
        setEligibility(data);
        setPageState(data.isEligible ? "eligible" : "ineligible");
      } catch (err) {
        setError(err instanceof Error ? err.message : "Something went wrong");
        setPageState("error");
      }
    })();
  }, [router]);

  useEffect(() => {
    if (pageState !== "taking" || timeRemaining <= 0) return;
    const id = setInterval(() => {
      setTimeRemaining((prev) => {
        if (prev <= 1) { clearInterval(id); submitExam(true); return 0; }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(id);
  }, [pageState]);

  async function startExam() {
    setPageState("loading");
    try {
      const res = await fetch(`${API_BASE_URL}/api/student/exam/start`, {
        method: "POST",
        credentials: "include",
        headers: getHeaders(),
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        throw new Error(body.message || "Cannot start exam");
      }
      const data: ExamQuestionsData = await res.json();
      setExamData(data);
      setAnswers({});
      setTimeRemaining(data.durationMinutes * 60);
      startTimeRef.current = Date.now();
      setPageState("taking");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to start exam");
      setPageState("error");
    }
  }

  async function submitExam(force = false) {
    if (!examData) return;
    const unanswered = examData.questions.filter((q) => !answers[q.questionNumber]).length;
    if (!force && unanswered > 0) {
      if (!window.confirm(`You have ${unanswered} unanswered question(s). Submit anyway?`)) return;
    }
    setSubmitting(true);
    try {
      const timeTaken = Math.round((Date.now() - startTimeRef.current) / 1000);
      const payload = {
        examId: examData.examId,
        timeTakenSeconds: timeTaken,
        answers: examData.questions.map((q) => ({
          questionNumber: q.questionNumber,
          answer: answers[q.questionNumber] ?? "",
        })),
      };
      const res = await fetch(`${API_BASE_URL}/api/student/exam/submit`, {
        method: "POST",
        credentials: "include",
        headers: getHeaders(),
        body: JSON.stringify(payload),
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        throw new Error(body.message || "Failed to submit exam");
      }
      const data: ExamResultData = await res.json();
      setResult(data);
      setPageState("result");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to submit exam");
      setPageState("error");
    } finally {
      setSubmitting(false);
    }
  }

  const formatTime = (sec: number) => {
    const m = Math.floor(sec / 60).toString().padStart(2, "0");
    const s = (sec % 60).toString().padStart(2, "0");
    return `${m}:${s}`;
  };

  const answeredCount = examData ? examData.questions.filter((q) => answers[q.questionNumber]).length : 0;

  if (pageState === "loading") {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-12 h-12 animate-spin text-indigo-600 mx-auto mb-4" />
          <p className="text-gray-600">Loading exam...</p>
        </div>
      </div>
    );
  }

  if (pageState === "error") {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="max-w-md w-full bg-white rounded-2xl shadow-lg p-8 text-center">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-xl font-bold text-gray-900 mb-2">Something went wrong</h2>
          <p className="text-gray-600 mb-6">{error}</p>
          <button onClick={() => router.push("/student/dashboard")} className="bg-indigo-600 text-white px-6 py-2 rounded-lg hover:bg-indigo-700 transition-colors">
            Back to Dashboard
          </button>
        </div>
      </div>
    );
  }

  if (pageState === "ineligible" && eligibility) {
    const { hasPassedExam, totalExperiments, completedExperiments, pendingExperiments, message, exam } = eligibility;
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="max-w-lg w-full bg-white rounded-2xl shadow-lg overflow-hidden">
          <div className={`p-8 text-white text-center ${hasPassedExam ? "bg-gradient-to-r from-green-500 to-emerald-600" : "bg-gradient-to-r from-orange-500 to-amber-600"}`}>
            <div className="bg-white/20 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-4">
              {hasPassedExam ? <Trophy size={40} /> : <Lock size={40} />}
            </div>
            <h1 className="text-2xl font-bold mb-1">{hasPassedExam ? "Exam Passed!" : "Exam Locked"}</h1>
            {exam && <p className="text-white/80 text-sm">{exam.title}</p>}
          </div>
          <div className="p-8">
            <p className="text-gray-700 text-center mb-6">{message}</p>
            {!hasPassedExam && totalExperiments > 0 && (
              <div className="mb-6">
                <div className="flex justify-between text-sm text-gray-600 mb-2">
                  <span>Experiments completed</span>
                  <span className="font-semibold">{completedExperiments} / {totalExperiments}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-3">
                  <div className="bg-indigo-600 h-3 rounded-full transition-all" style={{ width: `${totalExperiments > 0 ? (completedExperiments / totalExperiments) * 100 : 0}%` }} />
                </div>
                {pendingExperiments > 0 && (
                  <p className="text-sm text-orange-600 mt-2 text-center">{pendingExperiments} experiment(s) pending teacher approval</p>
                )}
              </div>
            )}
            <div className="flex gap-3">
              <button onClick={() => router.push("/student/dashboard")} className="flex-1 border border-gray-300 text-gray-700 py-2.5 rounded-lg hover:bg-gray-50 transition-colors">Dashboard</button>
              {!hasPassedExam && (
                <button onClick={() => router.push("/student/experiments")} className="flex-1 bg-indigo-600 text-white py-2.5 rounded-lg hover:bg-indigo-700 transition-colors">Go to Experiments</button>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (pageState === "eligible" && eligibility?.exam) {
    const { exam } = eligibility;
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="max-w-2xl w-full bg-white rounded-2xl shadow-lg border border-gray-200 overflow-hidden">
          <div className="bg-gradient-to-r from-indigo-600 to-blue-600 p-8 text-white text-center">
            <div className="bg-white/20 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-4"><BookOpen size={40} /></div>
            <h1 className="text-3xl font-bold mb-1">{exam.title}</h1>
            <p className="text-indigo-100 text-sm">{exam.roboticsLevelName} — Level {exam.levelNumber}</p>
          </div>
          <div className="p-8">
            {exam.description && <p className="text-gray-600 text-center mb-6">{exam.description}</p>}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
              {[
                { label: "Questions", value: exam.totalQuestions, icon: <Edit3 className="w-5 h-5" /> },
                { label: "Total Marks", value: exam.totalMarks, icon: <Trophy className="w-5 h-5" /> },
                { label: "Duration", value: `${exam.durationMinutes} min`, icon: <Clock className="w-5 h-5" /> },
                { label: "Pass Mark", value: `${exam.passingPercentage}%`, icon: <CheckCircle className="w-5 h-5" /> },
              ].map((s) => (
                <div key={s.label} className="bg-indigo-50 rounded-xl p-4 text-center">
                  <div className="flex justify-center text-indigo-600 mb-1">{s.icon}</div>
                  <p className="text-2xl font-bold text-indigo-700">{s.value}</p>
                  <p className="text-xs text-indigo-500">{s.label}</p>
                </div>
              ))}
            </div>
            <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 mb-8">
              <h3 className="font-semibold text-amber-800 mb-2">Instructions</h3>
              <ul className="text-sm text-amber-700 space-y-1 list-disc list-inside">
                <li>Read each question carefully before answering.</li>
                <li>The exam will auto-submit when time runs out.</li>
                <li>Do not refresh or close the browser during the exam.</li>
                <li>You cannot change answers after submission.</li>
              </ul>
            </div>
            <div className="flex gap-3">
              <button onClick={() => router.push("/student/dashboard")} className="flex gap-2 items-center border border-gray-300 text-gray-700 px-5 py-2.5 rounded-xl hover:bg-gray-50 transition-colors">
                <ArrowLeft className="w-4 h-4" /> Back
              </button>
              <button onClick={startExam} className="flex-1 bg-indigo-600 text-white py-2.5 rounded-xl hover:bg-indigo-700 transition-colors font-semibold text-lg">
                Start Exam
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (pageState === "taking" && examData) {
    const urgent = timeRemaining <= 300;
    return (
      <div className="min-h-screen bg-gray-50">
        <div className={`sticky top-0 z-30 border-b shadow-sm ${urgent ? "bg-red-600" : "bg-indigo-700"} text-white`}>
          <div className="max-w-4xl mx-auto px-4 py-3 flex items-center justify-between">
            <div>
              <p className="font-semibold text-sm truncate">{examData.title}</p>
              <p className="text-xs opacity-75">{answeredCount} / {examData.totalQuestions} answered</p>
            </div>
            <div className={`flex items-center gap-2 font-mono text-xl font-bold px-4 py-1.5 rounded-lg ${urgent ? "bg-white/20 animate-pulse" : "bg-white/10"}`}>
              <Clock className="w-5 h-5" />{formatTime(timeRemaining)}
            </div>
          </div>
          <div className="w-full bg-white/20 h-1">
            <div className="bg-white h-1 transition-all" style={{ width: `${(answeredCount / examData.totalQuestions) * 100}%` }} />
          </div>
        </div>

        <div className="max-w-4xl mx-auto px-4 py-6 space-y-6">
          {examData.questions.map((q, idx) => {
            const current = answers[q.questionNumber];
            const type = q.questionType.toUpperCase();
            return (
              <div key={q.questionNumber} className={`bg-white rounded-2xl shadow-sm border-2 p-6 transition-colors ${current ? "border-indigo-200" : "border-gray-200"}`}>
                <div className="flex items-start gap-3 mb-4">
                  <span className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold ${current ? "bg-indigo-600 text-white" : "bg-gray-100 text-gray-500"}`}>{idx + 1}</span>
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${type === "MCQ" ? "bg-blue-100 text-blue-700" : type === "TRUEFALSE" ? "bg-purple-100 text-purple-700" : "bg-green-100 text-green-700"}`}>
                        {type === "MCQ" ? "MCQ" : type === "TRUEFALSE" ? "True / False" : "Short Answer"}
                      </span>
                      <span className="text-xs text-gray-400">{q.marks} mark{q.marks !== 1 ? "s" : ""}</span>
                    </div>
                    <p className="text-gray-800 font-medium">{q.questionText}</p>
                  </div>
                </div>

                {type === "MCQ" && q.options.length > 0 && (
                  <div className="ml-11 space-y-2">
                    {q.options.map((opt, oi) => (
                      <label key={oi} className={`flex items-center gap-3 p-3 rounded-xl cursor-pointer transition-colors border-2 ${current === opt ? "border-indigo-500 bg-indigo-50" : "border-gray-100 hover:border-gray-300 hover:bg-gray-50"}`}>
                        <input type="radio" name={`q-${q.questionNumber}`} value={opt} checked={current === opt} onChange={() => setAnswers((prev) => ({ ...prev, [q.questionNumber]: opt }))} className="text-indigo-600" />
                        <span className="text-gray-700">{opt}</span>
                      </label>
                    ))}
                  </div>
                )}

                {type === "TRUEFALSE" && (
                  <div className="ml-11 flex gap-3">
                    {["True", "False"].map((val) => (
                      <label key={val} className={`flex items-center gap-2 px-6 py-3 rounded-xl cursor-pointer border-2 font-medium transition-colors ${current === val ? "border-indigo-500 bg-indigo-50 text-indigo-700" : "border-gray-100 hover:border-gray-300"}`}>
                        <input type="radio" name={`q-${q.questionNumber}`} value={val} checked={current === val} onChange={() => setAnswers((prev) => ({ ...prev, [q.questionNumber]: val }))} className="text-indigo-600" />
                        {val}
                      </label>
                    ))}
                  </div>
                )}

                {type === "SHORTANSWER" && (
                  <div className="ml-11">
                    <input type="text" placeholder="Type your answer here..." value={current ?? ""} onChange={(e) => setAnswers((prev) => ({ ...prev, [q.questionNumber]: e.target.value }))} className="w-full border-2 border-gray-200 rounded-xl px-4 py-3 focus:outline-none focus:border-indigo-500 transition-colors" />
                  </div>
                )}
              </div>
            );
          })}

          <div className="sticky bottom-4">
            <button onClick={() => submitExam()} disabled={submitting} className="w-full bg-indigo-600 text-white py-4 rounded-2xl hover:bg-indigo-700 disabled:opacity-60 transition-colors font-semibold text-lg flex items-center justify-center gap-2 shadow-lg">
              {submitting ? <Loader2 className="w-5 h-5 animate-spin" /> : <Send className="w-5 h-5" />}
              {submitting ? "Submitting..." : `Submit Exam (${answeredCount}/${examData.totalQuestions} answered)`}
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (pageState === "result" && result) {
    const passed = result.isPassed;
    return (
      <div className="min-h-screen bg-gray-50 py-8 px-4">
        <div className="max-w-2xl mx-auto">
          <div className="rounded-2xl shadow-lg overflow-hidden mb-6">
            <div className={`p-8 text-white text-center ${passed ? "bg-gradient-to-r from-green-500 to-emerald-600" : "bg-gradient-to-r from-red-500 to-rose-600"}`}>
              <div className="bg-white/20 w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-4">
                {passed ? <Trophy size={48} /> : <XCircle size={48} />}
              </div>
              <h1 className="text-3xl font-bold mb-1">{passed ? "Congratulations!" : "Better luck next time"}</h1>
              <p className="text-white/80">{result.examTitle}</p>
              <div className="flex justify-center gap-8 mt-6">
                <div><p className="text-4xl font-bold">{result.percentage}%</p><p className="text-white/70 text-sm">Score</p></div>
                <div><p className="text-4xl font-bold">{result.scoreObtained}/{result.totalMarks}</p><p className="text-white/70 text-sm">Marks</p></div>
              </div>
            </div>
            <div className="bg-white p-6">
              <p className="text-gray-600 text-center mb-4">{result.resultMessage}</p>
              <div className="flex justify-center gap-6 text-sm text-gray-500">
                <span>Time: {result.timeTakenFormatted}</span>
                {result.certificateGenerated && <span className="text-green-600 font-medium">Certificate earned!</span>}
              </div>
            </div>
          </div>

          {result.questionResults && result.questionResults.length > 0 && (
            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 mb-6">
              <button onClick={() => setShowReview((v) => !v)} className="w-full p-4 flex items-center justify-between font-semibold text-gray-700">
                <span>Review Answers ({result.questionResults.length} questions)</span>
                <span className="text-gray-400">{showReview ? "▲" : "▼"}</span>
              </button>
              {showReview && (
                <div className="border-t divide-y">
                  {result.questionResults.map((qr, idx) => (
                    <div key={qr.questionNumber} className="p-4">
                      <div className="flex items-start gap-3">
                        <span className={`flex-shrink-0 w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold ${qr.isCorrect ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}`}>{idx + 1}</span>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-800 mb-2">{qr.questionText}</p>
                          <div className="space-y-1 text-sm">
                            <p className={qr.isCorrect ? "text-green-700" : "text-red-600"}>Your answer: <span className="font-medium">{qr.yourAnswer || "(no answer)"}</span></p>
                            {!qr.isCorrect && <p className="text-green-700">Correct answer: <span className="font-medium">{qr.correctAnswer}</span></p>}
                            {qr.explanation && <p className="text-gray-500 italic">{qr.explanation}</p>}
                          </div>
                        </div>
                        <div className="flex-shrink-0 text-right">
                          <span className={`text-sm font-semibold ${qr.isCorrect ? "text-green-600" : "text-red-500"}`}>{qr.marksObtained}/{qr.totalMarks}</span>
                          {qr.isCorrect ? <CheckCircle className="w-4 h-4 text-green-500 ml-auto mt-1" /> : <XCircle className="w-4 h-4 text-red-400 ml-auto mt-1" />}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          <div className="flex gap-3">
            <button onClick={() => router.push("/student/dashboard")} className="flex-1 border border-gray-300 text-gray-700 py-3 rounded-xl hover:bg-gray-50 transition-colors">Dashboard</button>
            {result.certificateGenerated && (
              <button onClick={() => router.push("/student/certificate")} className="flex-1 bg-green-600 text-white py-3 rounded-xl hover:bg-green-700 transition-colors font-semibold">View Certificate</button>
            )}
          </div>
        </div>
      </div>
    );
  }

  return null;
}
