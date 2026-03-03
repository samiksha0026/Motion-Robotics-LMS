"use client"

import { useState, useEffect, use } from "react"
import { useRouter } from "next/navigation"
import { fetchWithAuth } from "@/lib/auth"
import {
  ArrowLeft,
  Beaker,
  Lock,
  Unlock,
  Clock,
  Users,
  AlertCircle,
  Loader2,
  Eye,
  Calendar,
  FileText,
  CheckCircle,
  GraduationCap
} from "lucide-react"

interface Experiment {
  experimentId: number
  sequenceOrder: number
  title: string
  description?: string
  estimatedMinutes: number
  isUnlocked: boolean
  unlockInfo?: {
    unlockId: number
    unlockedAt: string
    deadline?: string
    instructions?: string
  }
}

interface LevelData {
  levelId: number
  levelNumber: number
  levelName: string
  description?: string
}

interface ClassInfo {
  classId: number
  className: string
  level: LevelData | null
  totalExperiments: number
  unlockedCount: number
  experiments: Experiment[]
}

interface StudentProgress {
  studentId: number
  studentName: string
  rollNo: string
  email: string
  status: string
  completed: boolean
  completedAt?: string
  submissionNotes?: string
  submissionImageUrl?: string
  isApproved: boolean
  approvedAt?: string
  teacherRemarks?: string
}

interface ExperimentProgress {
  classId: number
  experimentId: number
  experimentTitle: string
  unlockedAt: string
  deadline?: string
  instructions?: string
  totalStudents: number
  completedCount: number
  approvedCount: number
  pendingApprovalCount: number
  notStartedCount: number
  students: StudentProgress[]
}

export default function TeacherExperimentsPage({ params }: { params: Promise<{ classId: string }> }) {
  const { classId } = use(params)
  const router = useRouter()
  const [classInfo, setClassInfo] = useState<ClassInfo | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Modal states
  const [showUnlockModal, setShowUnlockModal] = useState(false)
  const [showProgressModal, setShowProgressModal] = useState(false)
  const [selectedExperiment, setSelectedExperiment] = useState<Experiment | null>(null)
  const [deadline, setDeadline] = useState("")
  const [instructions, setInstructions] = useState("")
  const [unlocking, setUnlocking] = useState(false)

  // Progress state
  const [experimentProgress, setExperimentProgress] = useState<ExperimentProgress | null>(null)
  const [loadingProgress, setLoadingProgress] = useState(false)

  useEffect(() => {
    fetchExperiments()
  }, [classId])

  const fetchExperiments = async () => {
    try {
      setLoading(true)
      setError(null)
      const response = await fetchWithAuth(`/api/teacher/levels/classes/${classId}/experiments`)
      setClassInfo(response)
    } catch (err: any) {
      console.error('Error fetching experiments:', err)
      setError("Failed to load experiments")
    } finally {
      setLoading(false)
    }
  }

  const openUnlockModal = (experiment: Experiment) => {
    setSelectedExperiment(experiment)
    setDeadline(experiment.unlockInfo?.deadline ? new Date(experiment.unlockInfo.deadline).toISOString().slice(0, 16) : "")
    setInstructions(experiment.unlockInfo?.instructions || "")
    setShowUnlockModal(true)
  }

  const handleUnlock = async () => {
    if (!selectedExperiment) return

    try {
      setUnlocking(true)
      await fetchWithAuth("/api/teacher/levels/experiments/unlock", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          classId: parseInt(classId),
          experimentId: selectedExperiment.experimentId,
          deadline: deadline ? new Date(deadline).toISOString() : null,
          instructions: instructions || null,
        })
      })

      setShowUnlockModal(false)
      fetchExperiments()
    } catch (err: any) {
      console.error('Error unlocking experiment:', err)
      alert('Failed to unlock experiment')
    } finally {
      setUnlocking(false)
    }
  }

  const handleLock = async (experiment: Experiment) => {
    if (!confirm("Are you sure you want to lock this experiment? Students will no longer be able to access it.")) {
      return
    }

    try {
      if (experiment.unlockInfo) {
        await fetchWithAuth(`/api/teacher/levels/experiments/unlock/${experiment.unlockInfo.unlockId}`, {
          method: "DELETE"
        })
      }
      fetchExperiments()
    } catch (err: any) {
      console.error('Error locking experiment:', err)
      alert('Failed to lock experiment')
    }
  }

  const openProgressModal = async (experiment: Experiment) => {
    setSelectedExperiment(experiment)
    setShowProgressModal(true)
    setLoadingProgress(true)

    try {
      const response = await fetchWithAuth(`/api/teacher/levels/classes/${classId}/experiments/${experiment.experimentId}/progress`)
      setExperimentProgress(response)
    } catch (err: any) {
      console.error('Error loading progress:', err)
      alert('Failed to load progress')
    } finally {
      setLoadingProgress(false)
    }
  }

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "approved":
        return <span className="px-2 py-1 bg-green-500 text-white text-xs rounded-full">Approved</span>
      case "pending_approval":
        return <span className="px-2 py-1 bg-yellow-500 text-white text-xs rounded-full">Pending Approval</span>
      case "in_progress":
        return <span className="px-2 py-1 bg-blue-500 text-white text-xs rounded-full">In Progress</span>
      default:
        return <span className="px-2 py-1 border border-gray-300 text-gray-700 text-xs rounded-full">Not Started</span>
    }
  }

  const formatDate = (dateString?: string) => {
    if (!dateString) return "N/A"
    return new Date(dateString).toLocaleString()
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center gap-4 mb-6">
          <button 
            className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-md transition-colors"
            onClick={() => router.push("/teacher/levels")}
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Classes
          </button>
        </div>
        <div className="bg-red-50 border border-red-200 rounded-lg">
          <div className="p-6">
            <div className="flex items-center gap-2 text-red-600">
              <AlertCircle className="h-5 w-5" />
              <span>{error}</span>
            </div>
          </div>
        </div>
      </div>
    )
  }

  const experiments = classInfo?.experiments || []

  return (
    <div className="container mx-auto p-6">
      {/* Header */}
      <div className="flex items-center gap-4 mb-6">
        <button 
          className="flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-md transition-colors"
          onClick={() => router.push("/teacher/levels")}
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Classes
        </button>
      </div>

      {/* Class Info */}
      <div className="bg-white rounded-lg border shadow-sm mb-6">
        <div className="p-6 border-b">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="flex items-center gap-2 text-xl font-semibold">
                <GraduationCap className="h-5 w-5" />
                {classInfo?.className}
              </h2>
              <p className="text-sm text-gray-600 mt-1">Manage experiments for this class</p>
            </div>
            {classInfo?.level && (
              <span className="bg-blue-500 text-white text-lg px-4 py-1 rounded-full">
                Level {classInfo.level.levelNumber}: {classInfo.level.levelName}
              </span>
            )}
          </div>
        </div>
        <div className="p-6">
          <div className="flex gap-4 text-sm text-gray-600">
            <div className="flex items-center gap-1">
              <Beaker className="h-4 w-4" />
              <span>{classInfo?.totalExperiments || 0} Total Experiments</span>
            </div>
            <div className="flex items-center gap-1">
              <Unlock className="h-4 w-4 text-green-600" />
              <span>{classInfo?.unlockedCount || 0} Unlocked</span>
            </div>
            <div className="flex items-center gap-1">
              <Lock className="h-4 w-4 text-red-600" />
              <span>{(classInfo?.totalExperiments || 0) - (classInfo?.unlockedCount || 0)} Locked</span>
            </div>
          </div>
        </div>
      </div>

      {/* Experiments List */}
      {!classInfo?.level ? (
        <div className="bg-white rounded-lg border shadow-sm">
          <div className="p-6">
            <div className="text-center text-gray-500">
              <AlertCircle className="h-12 w-12 mx-auto mb-4 text-yellow-500" />
              <p className="text-lg font-medium">No Robotics Level Assigned</p>
              <p className="text-sm">Contact your school admin to assign a robotics level to this class.</p>
            </div>
          </div>
        </div>
      ) : experiments.length === 0 ? (
        <div className="bg-white rounded-lg border shadow-sm">
          <div className="p-6">
            <div className="text-center text-gray-500">
              <Beaker className="h-12 w-12 mx-auto mb-4" />
              <p>No experiments available for this level.</p>
            </div>
          </div>
        </div>
      ) : (
        <div className="grid gap-4">
          {experiments.map((experiment) => (
            <div
              key={experiment.experimentId}
              className={`bg-white rounded-lg border shadow-sm ${
                experiment.isUnlocked ? "border-green-200 bg-green-50/50" : "border-gray-200"
              }`}
            >
              <div className="p-6">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <span className="px-2 py-1 border rounded text-xs font-medium text-gray-700 bg-white">
                        Exp #{experiment.sequenceOrder}
                      </span>
                      <h3 className="font-semibold text-lg">{experiment.title}</h3>
                      {experiment.isUnlocked ? (
                        <span className="bg-green-500 text-white px-2 py-1 rounded text-xs font-medium flex items-center gap-1">
                          <Unlock className="h-3 w-3" />
                          Unlocked
                        </span>
                      ) : (
                        <span className="bg-red-500 text-white px-2 py-1 rounded text-xs font-medium flex items-center gap-1">
                          <Lock className="h-3 w-3" />
                          Locked
                        </span>
                      )}
                    </div>
                    <p className="text-gray-600 text-sm mb-3">{experiment.description}</p>

                    {/* Show deadline and instructions if unlocked */}
                    {experiment.isUnlocked && experiment.unlockInfo && (
                      <div className="flex flex-wrap gap-4 text-sm">
                        {experiment.unlockInfo.deadline && (
                          <div className="flex items-center gap-1 text-orange-600">
                            <Clock className="h-4 w-4" />
                            <span>Deadline: {formatDate(experiment.unlockInfo.deadline)}</span>
                          </div>
                        )}
                        {experiment.unlockInfo.unlockedAt && (
                          <div className="flex items-center gap-1 text-green-600">
                            <Calendar className="h-4 w-4" />
                            <span>Unlocked: {formatDate(experiment.unlockInfo.unlockedAt)}</span>
                          </div>
                        )}
                        {experiment.unlockInfo.instructions && (
                          <div className="text-blue-600 text-sm mt-1 w-full">
                            <strong>Instructions:</strong> {experiment.unlockInfo.instructions}
                          </div>
                        )}
                      </div>
                    )}
                  </div>

                  <div className="flex gap-2 ml-4">
                    {!experiment.isUnlocked ? (
                      <button 
                        onClick={() => openUnlockModal(experiment)} 
                        className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-md text-sm font-medium flex items-center gap-2"
                      >
                        <Unlock className="h-4 w-4" />
                        Unlock
                      </button>
                    ) : (
                      <>
                        <button 
                          onClick={() => openProgressModal(experiment)}
                          className="border border-gray-300 text-gray-700 hover:bg-gray-50 px-4 py-2 rounded-md text-sm font-medium flex items-center gap-2"
                        >
                          <Eye className="h-4 w-4" />
                          Progress
                        </button>
                        <button 
                          onClick={() => openUnlockModal(experiment)}
                          className="border border-gray-300 text-gray-700 hover:bg-gray-50 px-4 py-2 rounded-md text-sm font-medium flex items-center gap-2"
                        >
                          <Clock className="h-4 w-4" />
                          Edit
                        </button>
                        <button 
                          onClick={() => handleLock(experiment)}
                          className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-md text-sm font-medium flex items-center gap-2"
                        >
                          <Lock className="h-4 w-4" />
                          Lock
                        </button>
                      </>
                    )}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Unlock Modal */}
      {showUnlockModal && selectedExperiment && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl border w-full max-w-md mx-4">
            <div className="p-6 border-b">
              <h2 className="text-xl font-semibold">
                {selectedExperiment.isUnlocked ? "Edit" : "Unlock"} Experiment
              </h2>
              <p className="text-sm text-gray-600 mt-1">
                {selectedExperiment.title}
              </p>
            </div>
            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">
                  Deadline (Optional)
                </label>
                <input
                  type="datetime-local"
                  value={deadline}
                  onChange={(e) => setDeadline(e.target.value)}
                  min={new Date().toISOString().slice(0, 16)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Set a deadline for students to complete this experiment
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Instructions (Optional)
                </label>
                <textarea
                  value={instructions}
                  onChange={(e) => setInstructions(e.target.value)}
                  placeholder="Add any special instructions for students..."
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-none"
                />
              </div>

              <div className="flex gap-2 pt-4">
                <button
                  className="flex-1 border border-gray-300 text-gray-700 hover:bg-gray-50 px-4 py-2 rounded-md text-sm font-medium"
                  onClick={() => setShowUnlockModal(false)}
                >
                  Cancel
                </button>
                <button
                  className="flex-1 bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-md text-sm font-medium flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                  onClick={handleUnlock}
                  disabled={unlocking}
                >
                  {unlocking ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Unlock className="h-4 w-4" />
                  )}
                  {selectedExperiment.isUnlocked ? "Update" : "Unlock"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Progress Modal */}
      {showProgressModal && selectedExperiment && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 overflow-y-auto py-8">
          <div className="bg-white rounded-lg shadow-xl border w-full max-w-3xl mx-4">
            <div className="p-6 border-b">
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="flex items-center gap-2 text-xl font-semibold">
                    <Users className="h-5 w-5" />
                    Student Progress
                  </h2>
                  <p className="text-sm text-gray-600 mt-1">
                    {selectedExperiment.title}
                  </p>
                </div>
                <button 
                  className="text-gray-400 hover:text-gray-600 text-2xl leading-none"
                  onClick={() => setShowProgressModal(false)}
                >
                  &times;
                </button>
              </div>
            </div>
            <div className="p-6">
              {loadingProgress ? (
                <div className="flex justify-center py-8">
                  <Loader2 className="h-8 w-8 animate-spin" />
                </div>
              ) : experimentProgress ? (
                <div className="space-y-4">
                  {/* Progress Summary */}
                  <div className="grid grid-cols-4 gap-4">
                    <div className="bg-blue-50 rounded-lg border">
                      <div className="p-4 text-center">
                        <div className="text-2xl font-bold text-blue-600">
                          {experimentProgress.totalStudents}
                        </div>
                        <div className="text-xs text-gray-600">Total Students</div>
                      </div>
                    </div>
                    <div className="bg-green-50 rounded-lg border">
                      <div className="p-4 text-center">
                        <div className="text-2xl font-bold text-green-600">
                          {experimentProgress.approvedCount}
                        </div>
                        <div className="text-xs text-gray-600">Approved</div>
                      </div>
                    </div>
                    <div className="bg-yellow-50 rounded-lg border">
                      <div className="p-4 text-center">
                        <div className="text-2xl font-bold text-yellow-600">
                          {experimentProgress.pendingApprovalCount}
                        </div>
                        <div className="text-xs text-gray-600">Pending Approval</div>
                      </div>
                    </div>
                    <div className="bg-gray-50 rounded-lg border">
                      <div className="p-4 text-center">
                        <div className="text-2xl font-bold text-gray-600">
                          {experimentProgress.notStartedCount}
                        </div>
                        <div className="text-xs text-gray-600">Not Started</div>
                      </div>
                    </div>
                  </div>

                  {/* Deadline Info */}
                  {experimentProgress.deadline && (
                    <div className="flex items-center gap-2 text-orange-600 bg-orange-50 p-3 rounded">
                      <Clock className="h-4 w-4" />
                      <span>Deadline: {formatDate(experimentProgress.deadline)}</span>
                    </div>
                  )}

                  {/* Student List */}
                  <div className="border rounded-lg overflow-hidden">
                    <table className="w-full">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-4 py-2 text-left text-sm font-medium">Student</th>
                          <th className="px-4 py-2 text-left text-sm font-medium">Roll No</th>
                          <th className="px-4 py-2 text-left text-sm font-medium">Status</th>
                          <th className="px-4 py-2 text-left text-sm font-medium">Completed At</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y">
                        {experimentProgress.students.map((student) => (
                          <tr key={student.studentId} className="hover:bg-gray-50">
                            <td className="px-4 py-2">
                              <div>
                                <div className="font-medium">{student.studentName}</div>
                                <div className="text-xs text-gray-500">{student.email}</div>
                              </div>
                            </td>
                            <td className="px-4 py-2 text-sm">{student.rollNo}</td>
                            <td className="px-4 py-2">{getStatusBadge(student.status)}</td>
                            <td className="px-4 py-2 text-sm text-gray-500">
                              {student.completedAt ? formatDate(student.completedAt) : "-"}
                            </td>
                          </tr>
                        ))}
                        {experimentProgress.students.length === 0 && (
                          <tr>
                            <td colSpan={4} className="px-4 py-8 text-center text-gray-500">
                              No students in this class
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                </div>
              ) : (
                <div className="text-center py-8 text-gray-500">
                  Failed to load progress data
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
