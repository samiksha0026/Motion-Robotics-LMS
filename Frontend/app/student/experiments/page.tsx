"use client";

import { useState, useEffect, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { 
  ArrowLeft, Play, ChevronDown, ChevronUp, Eye, FileText,
  Target, AlertCircle, Clock, Lock, Loader, CheckCircle,
  Cpu, Code, Image, ShieldAlert, BookOpen, Check, Hourglass,
  ListOrdered, Zap, Settings, Lightbulb, CheckSquare, Square,
  RotateCcw, Microscope, Wrench, BookMarked, CircuitBoard,
  Video, Download, ExternalLink, ZoomIn, ChevronRight,
  Clipboard, Trophy, AlertTriangle, Info, Star, X
} from "lucide-react";
import { API_BASE_URL } from "@/lib/api-config";

interface ExperimentProgress {
  progressId: number;
  completed: boolean;
  completedAt: string | null;
  submissionNotes: string | null;
  submissionImageUrl: string | null;
  isApproved: boolean;
  approvedAt: string | null;
  teacherRemarks: string | null;
  approvedByTeacher: string | null;
}

interface Experiment {
  experimentId: number;
  sequenceOrder: number;
  title: string;
  description: string | null;
  objective: string | null;
  components: string | null;
  procedure: string | null;
  wiringDiagram: string | null;
  circuitDiagram: string | null;
  codeSnippet: string | null;
  demoVideoUrl: string | null;
  safetyNotes: string | null;
  estimatedMinutes: number;
  status: "locked" | "available" | "in-progress" | "completed" | "approved";
  progress: ExperimentProgress | null;
}

interface LevelInfo {
  levelId: number;
  levelNumber: number;
  levelName: string;
  description: string | null;
  totalExperiments: number;
  syllabusUrl?: string;
}

interface ProgressSummary {
  totalExperiments: number;
  completedExperiments: number;
  approvedExperiments: number;
  pendingApproval: number;
  progressPercentage: number;
  levelCompleted: boolean;
}

interface ExperimentsData {
  level: LevelInfo;
  progress: ProgressSummary;
  experiments: Experiment[];
}

function ExperimentsContent() {
  const searchParams = useSearchParams();
  const expandParam = searchParams.get('expand');
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<ExperimentsData | null>(null);
  const [expandedExperiment, setExpandedExperiment] = useState<number | null>(
    expandParam ? parseInt(expandParam) : null
  );
  const [submitting, setSubmitting] = useState(false);
  const [submissionNotes, setSubmissionNotes] = useState("");
  const [activeTab, setActiveTab] = useState<string>('video');
  const [completedSteps, setCompletedSteps] = useState<{[key: number]: number[]}>({});
  const [selectedDiagram, setSelectedDiagram] = useState<string | null>(null);

  useEffect(() => {
    fetchExperiments();
  }, []);

  useEffect(() => {
    // Update expanded experiment when URL param changes
    if (expandParam) {
      setExpandedExperiment(parseInt(expandParam));
    }
  }, [expandParam]);

  const fetchExperiments = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const token = typeof window !== 'undefined' ? sessionStorage.getItem('jwt') : null;
      
      if (!token) {
        throw new Error('No authentication token found. Please login again.');
      }

      const sessionId = typeof window !== 'undefined' ? sessionStorage.getItem('sessionId') : null;
      const response = await fetch(`${API_BASE_URL}/api/student/experiments`, {
        credentials: 'include',
        headers: {
          'Authorization': `Bearer ${token}`,
          ...(sessionId ? { 'X-Session-Id': sessionId } : {}),
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        if (response.status === 401) {
          throw new Error('Session expired. Please login again.');
        }
        throw new Error('Failed to fetch experiments');
      }

      const result = await response.json();
      setData(result);
      
      // Auto-expand first available experiment only if no URL param specified
      if (!expandParam) {
        const firstAvailable = result.experiments?.find((e: Experiment) => e.status === 'available');
        if (firstAvailable) {
          setExpandedExperiment(firstAvailable.experimentId);
        }
      }
    } catch (err) {
      console.error('Error fetching experiments:', err);
      setError(err instanceof Error ? err.message : 'Failed to load experiments');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitExperiment = async (experimentId: number) => {
    try {
      setSubmitting(true);
      const token = sessionStorage.getItem('jwt');
      
      const sessionId = typeof window !== 'undefined' ? sessionStorage.getItem('sessionId') : null;
      const response = await fetch(`${API_BASE_URL}/api/student/experiments/${experimentId}/submit`, {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Authorization': `Bearer ${token}`,
          ...(sessionId ? { 'X-Session-Id': sessionId } : {}),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ submissionNotes })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to submit experiment');
      }

      // Refresh data
      await fetchExperiments();
      setSubmissionNotes("");
      alert('Experiment submitted successfully! Waiting for teacher approval.');
    } catch (err) {
      console.error('Error submitting experiment:', err);
      alert(err instanceof Error ? err.message : 'Failed to submit experiment');
    } finally {
      setSubmitting(false);
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'approved':
        return (
          <span className="flex items-center gap-1.5 px-3 py-1 bg-green-100 text-green-700 text-xs font-semibold rounded-full">
            <Check size={14} /> Approved
          </span>
        );
      case 'completed':
        return (
          <span className="flex items-center gap-1.5 px-3 py-1 bg-yellow-100 text-yellow-700 text-xs font-semibold rounded-full">
            <Hourglass size={14} /> Pending Approval
          </span>
        );
      case 'available':
        return (
          <span className="flex items-center gap-1.5 px-3 py-1 bg-blue-100 text-blue-700 text-xs font-semibold rounded-full">
            <Play size={14} /> Available
          </span>
        );
      case 'locked':
      default:
        return (
          <span className="flex items-center gap-1.5 px-3 py-1 bg-gray-100 text-gray-500 text-xs font-semibold rounded-full">
            <Lock size={14} /> Locked
          </span>
        );
    }
  };

  const parseComponents = (components: string | null): string[] => {
    if (!components) return [];
    try {
      return JSON.parse(components);
    } catch {
      return components.split(',').map(c => c.trim()).filter(Boolean);
    }
  };

  const parseProcedure = (procedure: string | null): string[] => {
    if (!procedure) return [];
    try {
      return JSON.parse(procedure);
    } catch {
      return procedure.split('\n').map(s => s.trim()).filter(Boolean);
    }
  };

  // Loading state
  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader className="w-12 h-12 text-indigo-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600 font-medium">Loading your experiments...</p>
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center max-w-md">
          <AlertCircle className="w-12 h-12 text-red-600 mx-auto mb-4" />
          <p className="text-gray-900 font-medium mb-2">Failed to Load</p>
          <p className="text-gray-600 text-sm mb-4">{error}</p>
          <div className="flex gap-3 justify-center">
            <button 
              onClick={() => fetchExperiments()}
              className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
            >
              Try Again
            </button>
            <button 
              onClick={() => window.location.href = '/login'}
              className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
            >
              Login Again
            </button>
          </div>
        </div>
      </div>
    );
  }

  // No level assigned
  if (!data || !data.level || data.level.levelId === 0) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center max-w-md p-8">
          <BookOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h2 className="text-xl font-bold text-gray-900 mb-2">No Level Assigned Yet</h2>
          <p className="text-gray-600">Your school admin will assign your robotics level soon. Please check back later!</p>
          <button 
            onClick={() => window.location.href = '/student/dashboard'}
            className="mt-6 px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Back to Dashboard
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 sticky top-0 z-30">
        <div className="max-w-5xl mx-auto px-4 h-16 flex items-center justify-between">
          <button 
            onClick={() => window.location.href = '/student/dashboard'}
            className="flex items-center gap-2 px-3 py-2 text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <ArrowLeft size={20} />
            <span className="font-medium hidden sm:inline">Dashboard</span>
          </button>
          <h1 className="text-lg font-bold text-gray-900">My Experiments</h1>
          <div className="w-24"></div>
        </div>
      </header>

      <div className="max-w-5xl mx-auto p-4 lg:p-6 space-y-6">
        {/* Level Card */}
        <div className="bg-gradient-to-r from-indigo-600 to-purple-600 rounded-2xl p-6 text-white shadow-lg">
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div>
              <p className="text-indigo-200 text-sm mb-1">Assigned Level</p>
              <h2 className="text-2xl md:text-3xl font-bold mb-2">
                Level {data.level.levelNumber}: {data.level.levelName}
              </h2>
              {data.level.description && (
                <p className="text-indigo-100 text-sm max-w-xl">{data.level.description}</p>
              )}
            </div>
            {data.level.syllabusUrl && (
              <a
                href={`/student/books/viewer?url=${encodeURIComponent(`${API_BASE_URL}${data.level.syllabusUrl}`)}`}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-2 px-5 py-3 bg-white/20 hover:bg-white/30 rounded-lg font-medium transition-colors self-start"
              >
                <Eye size={20} />
                View Syllabus
              </a>
            )}
          </div>
        </div>

        {/* Progress Summary */}
        <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 mb-4">
            <h3 className="text-lg font-bold text-gray-900">Your Progress</h3>
            <div className="flex flex-wrap gap-3 text-sm">
              <span className="px-3 py-1 bg-green-100 text-green-700 rounded-full">
                {data.progress.approvedExperiments} Approved
              </span>
              <span className="px-3 py-1 bg-yellow-100 text-yellow-700 rounded-full">
                {data.progress.pendingApproval} Pending
              </span>
              <span className="px-3 py-1 bg-gray-100 text-gray-600 rounded-full">
                {data.progress.totalExperiments - data.progress.completedExperiments} Remaining
              </span>
            </div>
          </div>
          <div className="relative h-4 bg-gray-200 rounded-full overflow-hidden">
            <div 
              className="absolute inset-y-0 left-0 bg-gradient-to-r from-green-500 to-emerald-500 transition-all duration-500"
              style={{ width: `${data.progress.progressPercentage}%` }}
            />
          </div>
          <div className="flex justify-between mt-2 text-sm">
            <span className="text-gray-600">
              {data.progress.approvedExperiments} of {data.progress.totalExperiments} experiments completed
            </span>
            <span className="font-semibold text-green-600">{data.progress.progressPercentage}%</span>
          </div>
          {data.progress.levelCompleted && (
            <div className="mt-4 p-3 bg-green-50 border border-green-200 rounded-lg text-green-700 text-sm font-medium flex items-center gap-2">
              <CheckCircle size={18} />
              Congratulations! You have completed all experiments in this level. You can now take the final exam!
            </div>
          )}
        </div>

        {/* Experiments List */}
        <div className="space-y-4">
          <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
            <ListOrdered size={20} className="text-indigo-600" />
            Experiments ({data.experiments.length})
          </h3>

          {data.experiments.map((experiment) => (
            <div 
              key={experiment.experimentId}
              className={`bg-white rounded-xl border shadow-sm overflow-hidden transition-all ${
                experiment.status === 'locked' 
                  ? 'border-gray-200 opacity-60' 
                  : experiment.status === 'approved'
                  ? 'border-green-200'
                  : 'border-gray-200'
              }`}
            >
              {/* Experiment Header */}
              <button
                onClick={() => {
                  if (experiment.status !== 'locked') {
                    const newExpandedId = expandedExperiment === experiment.experimentId ? null : experiment.experimentId;
                    setExpandedExperiment(newExpandedId);
                    // Reset to video tab when expanding an experiment
                    if (newExpandedId) {
                      setActiveTab('video');
                    }
                  }
                }}
                disabled={experiment.status === 'locked'}
                className={`w-full p-4 flex items-center justify-between text-left ${
                  experiment.status !== 'locked' ? 'hover:bg-gray-50 cursor-pointer' : 'cursor-not-allowed'
                }`}
              >
                <div className="flex items-center gap-4">
                  <div className={`w-10 h-10 rounded-full flex items-center justify-center font-bold text-lg ${
                    experiment.status === 'approved' 
                      ? 'bg-green-100 text-green-600' 
                      : experiment.status === 'completed'
                      ? 'bg-yellow-100 text-yellow-600'
                      : experiment.status === 'available'
                      ? 'bg-blue-100 text-blue-600'
                      : 'bg-gray-100 text-gray-400'
                  }`}>
                    {experiment.sequenceOrder}
                  </div>
                  <div>
                    <h4 className="font-semibold text-gray-900">{experiment.title}</h4>
                    <div className="flex items-center gap-3 mt-1 text-sm text-gray-500">
                      <span className="flex items-center gap-1">
                        <Clock size={14} />
                        {experiment.estimatedMinutes} min
                      </span>
                      {experiment.description && (
                        <span className="hidden sm:inline truncate max-w-xs">
                          {experiment.description.substring(0, 50)}...
                        </span>
                      )}
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  {getStatusBadge(experiment.status)}
                  {experiment.status !== 'locked' && (
                    expandedExperiment === experiment.experimentId 
                      ? <ChevronUp size={20} className="text-gray-400" />
                      : <ChevronDown size={20} className="text-gray-400" />
                  )}
                </div>
              </button>

              {/* Expanded Content */}
              {expandedExperiment === experiment.experimentId && experiment.status !== 'locked' && (
                <div className="border-t border-gray-200 bg-gray-50">
                  {/* Experiment Header */}
                  <div className="p-6 bg-gradient-to-r from-blue-600 to-purple-600 text-white">
                    <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <Target className="w-5 h-5" />
                          <span className="text-blue-100 text-sm font-medium">Experiment {experiment.sequenceOrder}</span>
                        </div>
                        <h3 className="text-2xl font-bold mb-2">{experiment.title}</h3>
                        {experiment.description && (
                          <p className="text-blue-100 text-sm">{experiment.description}</p>
                        )}
                      </div>
                      <div className="flex flex-col sm:flex-row gap-3">
                        <div className="flex items-center gap-2 px-3 py-2 bg-white/10 rounded-lg">
                          <Clock className="w-4 h-4" />
                          <span className="text-sm font-medium">{experiment.estimatedMinutes} min</span>
                        </div>
                        <div className="flex items-center gap-2 px-3 py-2 bg-white/10 rounded-lg">
                          <BookMarked className="w-4 h-4" />
                          <span className="text-sm font-medium">Practical</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Tab Navigation */}
                  <div className="px-6 pt-6">
                    <div className="flex flex-wrap gap-2 mb-6">
                      {[
                        { id: 'overview', label: 'Overview', icon: Target },
                        { id: 'video', label: 'Demo Video', icon: Video },
                        { id: 'components', label: 'Components', icon: Cpu },
                        { id: 'procedure', label: 'Steps', icon: ListOrdered },
                        { id: 'diagrams', label: 'Diagrams', icon: CircuitBoard }
                      ].map((tab) => {
                        const IconComponent = tab.icon;
                        return (
                          <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id)}
                            className={`flex items-center gap-2 px-4 py-2 rounded-lg font-medium text-sm transition-all ${
                              activeTab === tab.id
                                ? 'bg-blue-600 text-white shadow-md'
                                : 'bg-white text-gray-600 hover:bg-gray-100 border border-gray-200'
                            }`}
                          >
                            <IconComponent size={16} />
                            <span className="hidden sm:inline">{tab.label}</span>
                          </button>
                        );
                      })}
                    </div>
                  </div>

                  {/* Tab Content */}
                  <div className="px-6 pb-6">
                    {activeTab === 'overview' && (
                      <div className="space-y-6">
                        {/* Objective */}
                        {experiment.objective && (
                          <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
                            <h4 className="text-lg font-bold text-gray-900 mb-3 flex items-center gap-2">
                              <Target className="text-blue-600" size={20} />
                              Objective
                            </h4>
                            <p className="text-gray-700 leading-relaxed">{experiment.objective}</p>
                          </div>
                        )}

                        {/* Quick Info */}
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                          <div className="bg-white rounded-lg p-4 border border-gray-200">
                            <div className="flex items-center gap-3 mb-2">
                              <Clock className="text-green-600" size={18} />
                              <span className="font-semibold text-gray-900">Duration</span>
                            </div>
                            <p className="text-2xl font-bold text-green-600">{experiment.estimatedMinutes}</p>
                            <p className="text-sm text-gray-600">minutes</p>
                          </div>
                          <div className="bg-white rounded-lg p-4 border border-gray-200">
                            <div className="flex items-center gap-3 mb-2">
                              <Settings className="text-blue-600" size={18} />
                              <span className="font-semibold text-gray-900">Level</span>
                            </div>
                            <p className="text-lg font-bold text-blue-600">Level {data?.level?.levelNumber}</p>
                            <p className="text-sm text-gray-600">{data?.level?.levelName}</p>
                          </div>
                          <div className="bg-white rounded-lg p-4 border border-gray-200">
                            <div className="flex items-center gap-3 mb-2">
                              <Wrench className="text-purple-600" size={18} />
                              <span className="font-semibold text-gray-900">Type</span>
                            </div>
                            <p className="text-lg font-bold text-purple-600">Practical</p>
                            <p className="text-sm text-gray-600">Hands-on experiment</p>
                          </div>
                        </div>

                        {/* Safety Notes */}
                        {experiment.safetyNotes && (
                          <div className="bg-yellow-50 rounded-xl p-6 border border-yellow-200">
                            <h4 className="text-lg font-bold text-gray-900 mb-3 flex items-center gap-2">
                              <ShieldAlert className="text-yellow-600" size={20} />
                              Safety Guidelines
                            </h4>
                            <p className="text-gray-700 leading-relaxed">{experiment.safetyNotes}</p>
                          </div>
                        )}
                      </div>
                    )}

                    {activeTab === 'video' && (
                      <div className="space-y-6">
                        {/* Demo Video */}
                        {experiment.demoVideoUrl ? (
                          <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
                            <div className="flex items-center justify-between mb-4">
                              <h4 className="text-lg font-bold text-gray-900 flex items-center gap-2">
                                <Video className="text-red-600" size={20} />
                                Experiment Demo Video
                              </h4>
                              <div className="flex items-center gap-2 px-3 py-1 bg-red-100 text-red-700 text-sm rounded-full">
                                <span className="w-2 h-2 bg-red-500 rounded-full animate-pulse"></span>
                                <span className="font-medium">Watch First!</span>
                              </div>
                            </div>
                            <div className="aspect-video bg-gray-900 rounded-lg overflow-hidden mb-4">
                              <video 
                                src={experiment.demoVideoUrl?.startsWith('http') ? experiment.demoVideoUrl : `${API_BASE_URL}${experiment.demoVideoUrl}`}
                                controls
                                className="w-full h-full"
                                controlsList="nodownload"
                                onContextMenu={(e) => e.preventDefault()}
                              >
                                Your browser does not support the video tag.
                              </video>
                            </div>
                            
                            {/* Video Guide */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                              <div className="p-4 bg-blue-50 rounded-lg border border-blue-200">
                                <h5 className="font-semibold text-blue-900 mb-2 flex items-center gap-2">
                                  <Play className="w-4 h-4" />
                                  How to Watch
                                </h5>
                                <ul className="space-y-1 text-sm text-blue-800">
                                  <li>• Watch the complete demo first</li>
                                  <li>• Pause and replay sections as needed</li>
                                  <li>• Take notes of important steps</li>
                                  <li>• Compare with your actual results</li>
                                </ul>
                              </div>
                              <div className="p-4 bg-green-50 rounded-lg border border-green-200">
                                <h5 className="font-semibold text-green-900 mb-2 flex items-center gap-2">
                                  <CheckCircle className="w-4 h-4" />
                                  What You'll See
                                </h5>
                                <ul className="space-y-1 text-sm text-green-800">
                                  <li>• Component identification</li>
                                  <li>• Proper wiring technique</li>
                                  <li>• Testing and troubleshooting</li>
                                  <li>• Expected output behavior</li>
                                </ul>
                              </div>
                            </div>
                          </div>
                        ) : (
                          <div className="bg-white rounded-xl p-8 border border-gray-200 shadow-sm">
                            <div className="text-center">
                              <Video className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                              <h4 className="text-lg font-semibold text-gray-600">Demo Video Coming Soon</h4>
                              <p className="text-gray-500 mt-2">We're preparing a demonstration video for this experiment.</p>
                            </div>
                          </div>
                        )}

                        {/* Help Section */}
                        <div className="bg-gradient-to-br from-purple-50 to-blue-50 rounded-xl p-6 border border-purple-200">
                          <h5 className="text-lg font-semibold text-gray-900 mb-3 flex items-center gap-2">
                            <Lightbulb className="text-yellow-500" size={20} />
                            Need Additional Help?
                          </h5>
                          <p className="text-gray-700 mb-4">
                            The demo video is your primary learning resource. For detailed theory and concepts, 
                            you can also refer to your digital books.
                          </p>
                          <div className="flex flex-wrap gap-3">
                            <button
                              onClick={() => window.open('/student/books', '_blank')}
                              className="flex items-center gap-2 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors"
                            >
                              <BookOpen size={16} />
                              Open Digital Books
                            </button>
                            <button
                              onClick={() => setActiveTab('procedure')}
                              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                            >
                              <ListOrdered size={16} />
                              View Step-by-Step
                            </button>
                          </div>
                        </div>
                      </div>
                    )}

                    {activeTab === 'components' && (
                      <div className="space-y-6">
                        {experiment.components && (
                          <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
                            <h4 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                              <Cpu className="text-blue-600" size={20} />
                              Required Components
                            </h4>
                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                              {parseComponents(experiment.components).map((comp, idx) => (
                                <div key={idx} className="bg-gradient-to-br from-blue-50 to-indigo-50 p-4 rounded-xl border border-blue-200 hover:shadow-md transition-all">
                                  <div className="flex items-center gap-3 mb-2">
                                    <div className="w-3 h-3 bg-blue-500 rounded-full"></div>
                                    <span className="font-semibold text-gray-900">{comp}</span>
                                  </div>
                                  <div className="flex items-center gap-2 text-xs text-gray-600">
                                    <CheckCircle size={12} className="text-green-500" />
                                    <span>Available in kit</span>
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>
                    )}

                    {activeTab === 'procedure' && experiment.procedure && (
                      <div className="space-y-6">
                        <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
                          <h4 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                            <ListOrdered className="text-green-600" size={20} />
                            Step-by-Step Instructions
                          </h4>
                          <div className="space-y-4">
                            {parseProcedure(experiment.procedure).map((step, idx) => {
                              const isCompleted = completedSteps[experiment.experimentId]?.includes(idx) || false;
                              return (
                                <div key={idx} className={`flex items-start gap-4 p-4 rounded-xl border transition-all ${
                                  isCompleted 
                                    ? 'bg-green-50 border-green-200' 
                                    : 'bg-gray-50 border-gray-200 hover:bg-blue-50'
                                }`}>
                                  <button
                                    onClick={() => {
                                      const expSteps = completedSteps[experiment.experimentId] || [];
                                      if (isCompleted) {
                                        setCompletedSteps({
                                          ...completedSteps,
                                          [experiment.experimentId]: expSteps.filter(s => s !== idx)
                                        });
                                      } else {
                                        setCompletedSteps({
                                          ...completedSteps,
                                          [experiment.experimentId]: [...expSteps, idx]
                                        });
                                      }
                                    }}
                                    className="flex-shrink-0 mt-1"
                                  >
                                    {isCompleted ? (
                                      <CheckSquare className="text-green-600" size={20} />
                                    ) : (
                                      <Square className="text-gray-400 hover:text-blue-600" size={20} />
                                    )}
                                  </button>
                                  <div className="flex-1">
                                    <div className="flex items-center gap-3 mb-2">
                                      <span className={`w-6 h-6 rounded-full flex items-center justify-center text-sm font-bold ${
                                        isCompleted 
                                          ? 'bg-green-500 text-white' 
                                          : 'bg-blue-500 text-white'
                                      }`}>
                                        {idx + 1}
                                      </span>
                                      <span className={`text-sm font-medium ${
                                        isCompleted ? 'text-green-800' : 'text-gray-600'
                                      }`}>
                                        Step {idx + 1}
                                      </span>
                                    </div>
                                    <p className={`text-gray-700 leading-relaxed ${
                                      isCompleted ? 'line-through text-gray-500' : ''
                                    }`}>
                                      {step}
                                    </p>
                                  </div>
                                </div>
                              );
                            })}
                          </div>
                          
                          {/* Progress Summary */}
                          <div className="mt-6 p-4 bg-gradient-to-r from-blue-50 to-purple-50 rounded-lg border border-blue-200">
                            <div className="flex items-center justify-between">
                              <span className="font-semibold text-gray-800">Progress</span>
                              <span className="text-blue-600 font-bold">
                                {completedSteps[experiment.experimentId]?.length || 0} of {parseProcedure(experiment.procedure).length} steps
                              </span>
                            </div>
                            <div className="mt-2 h-2 bg-gray-200 rounded-full overflow-hidden">
                              <div 
                                className="h-full bg-gradient-to-r from-blue-500 to-purple-500 transition-all duration-500"
                                style={{ 
                                  width: `${((completedSteps[experiment.experimentId]?.length || 0) / parseProcedure(experiment.procedure).length) * 100}%` 
                                }}
                              />
                            </div>
                          </div>
                        </div>
                      </div>
                    )}

                    {activeTab === 'diagrams' && (
                      <div className="space-y-6">
                        {(experiment.wiringDiagram || experiment.circuitDiagram) ? (
                          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                            {experiment.wiringDiagram && (
                              <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
                                <h4 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                                  <Image className="text-orange-600" size={20} />
                                  Wiring Diagram
                                </h4>
                                <div className="relative group">
                                  <img 
                                    src={experiment.wiringDiagram.startsWith('http') ? experiment.wiringDiagram : `${API_BASE_URL}${experiment.wiringDiagram}`}
                                    alt="Wiring Diagram"
                                    className="w-full rounded-lg border shadow cursor-pointer hover:shadow-lg transition-all"
                                    onClick={() => setSelectedDiagram(experiment.wiringDiagram)}
                                  />
                                  <button
                                    onClick={() => setSelectedDiagram(experiment.wiringDiagram)}
                                    className="absolute top-3 right-3 p-2 bg-black/20 hover:bg-black/40 rounded-lg opacity-0 group-hover:opacity-100 transition-all"
                                  >
                                    <ZoomIn className="text-white" size={18} />
                                  </button>
                                </div>
                                <p className="text-sm text-gray-600 mt-3">
                                  Click to view in full size. Shows how to connect wires.
                                </p>
                              </div>
                            )}
                            {experiment.circuitDiagram && (
                              <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
                                <h4 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                                  <CircuitBoard className="text-green-600" size={20} />
                                  Circuit Diagram
                                </h4>
                                <div className="relative group">
                                  <img 
                                    src={experiment.circuitDiagram.startsWith('http') ? experiment.circuitDiagram : `${API_BASE_URL}${experiment.circuitDiagram}`}
                                    alt="Circuit Diagram"
                                    className="w-full rounded-lg border shadow cursor-pointer hover:shadow-lg transition-all"
                                    onClick={() => setSelectedDiagram(experiment.circuitDiagram)}
                                  />
                                  <button
                                    onClick={() => setSelectedDiagram(experiment.circuitDiagram)}
                                    className="absolute top-3 right-3 p-2 bg-black/20 hover:bg-black/40 rounded-lg opacity-0 group-hover:opacity-100 transition-all"
                                  >
                                    <ZoomIn className="text-white" size={18} />
                                  </button>
                                </div>
                                <p className="text-sm text-gray-600 mt-3">
                                  Schematic showing electrical connections.
                                </p>
                              </div>
                            )}
                          </div>
                        ) : (
                          <div className="bg-white rounded-xl p-8 border border-gray-200 shadow-sm">
                            <div className="text-center">
                              <Image className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                              <h4 className="text-lg font-semibold text-gray-600">No Diagrams Available</h4>
                              <p className="text-gray-500 mt-2">Diagrams will be added soon.</p>
                            </div>
                          </div>
                        )}

                        {/* Arduino Code Section */}
                        {experiment.codeSnippet && (
                          <div className="bg-white rounded-xl p-6 border border-gray-200 shadow-sm">
                            <h4 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                              <Code className="text-cyan-600" size={20} />
                              Arduino Code
                            </h4>
                            <div className="relative">
                              <pre className="bg-gray-900 text-green-400 p-6 rounded-lg overflow-x-auto text-sm font-mono border">
                                {experiment.codeSnippet}
                              </pre>
                              <button
                                onClick={() => navigator.clipboard.writeText(experiment.codeSnippet || '')}
                                className="absolute top-4 right-4 p-2 bg-gray-700 hover:bg-gray-600 rounded-lg text-white text-xs"
                              >
                                <Clipboard size={16} />
                              </button>
                            </div>
                            <div className="mt-4 p-4 bg-blue-50 rounded-lg border border-blue-200">
                              <h5 className="font-semibold text-blue-900 mb-2">How to Use</h5>
                              <ul className="space-y-1 text-sm text-blue-800">
                                <li>• Copy this code to Arduino IDE</li>
                                <li>• Upload to your Arduino board</li>
                                <li>• Test the functionality</li>
                              </ul>
                            </div>
                          </div>
                        )}
                      </div>
                    )}
                  </div>

                  {/* Diagram Modal */}
                  {selectedDiagram && (
                    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                      <div className="bg-white rounded-xl max-w-4xl max-h-[90vh] overflow-auto">
                        <div className="p-4 border-b flex items-center justify-between">
                          <h3 className="text-lg font-semibold">Diagram View</h3>
                          <button
                            onClick={() => setSelectedDiagram(null)}
                            className="p-2 hover:bg-gray-100 rounded-lg"
                          >
                            <X size={20} />
                          </button>
                        </div>
                        <div className="p-4">
                          <img 
                            src={selectedDiagram.startsWith('http') ? selectedDiagram : `${API_BASE_URL}${selectedDiagram}`}
                            alt="Full Size Diagram"
                            className="w-full h-auto"
                          />
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Progress Info / Submit Section */}
                  <div className="px-6 pb-6">
                    {experiment.progress?.isApproved && (
                      <div className="bg-green-50 rounded-lg p-4 border border-green-200">
                        <h5 className="font-semibold text-green-800 mb-2 flex items-center gap-2">
                          <CheckCircle size={18} />
                          Experiment Approved
                        </h5>
                        <p className="text-green-700 text-sm">
                          Approved by {experiment.progress.approvedByTeacher} on{' '}
                          {experiment.progress.approvedAt && new Date(experiment.progress.approvedAt).toLocaleDateString()}
                        </p>
                        {experiment.progress.teacherRemarks && (
                          <p className="mt-2 text-gray-700 text-sm">
                            <strong>Teacher Remarks:</strong> {experiment.progress.teacherRemarks}
                          </p>
                        )}
                      </div>
                    )}

                    {experiment.progress?.completed && !experiment.progress.isApproved && (
                      <div className="bg-yellow-50 rounded-lg p-4 border border-yellow-200">
                        <h5 className="font-semibold text-yellow-800 mb-2 flex items-center gap-2">
                          <Hourglass size={18} />
                          Waiting for Teacher Approval
                        </h5>
                        <p className="text-yellow-700 text-sm">
                          Submitted on {experiment.progress.completedAt && new Date(experiment.progress.completedAt).toLocaleDateString()}
                        </p>
                        {experiment.progress.submissionNotes && (
                          <p className="mt-2 text-gray-700 text-sm">
                            <strong>Your Notes:</strong> {experiment.progress.submissionNotes}
                          </p>
                        )}
                      </div>
                    )}

                    {experiment.status === 'available' && !experiment.progress?.completed && (
                      <div className="bg-blue-50 rounded-lg p-4 border border-blue-200">
                        <h5 className="font-semibold text-gray-900 mb-3">Mark as Complete</h5>
                        <p className="text-gray-600 text-sm mb-4">
                          After completing this experiment with your kit, submit it for teacher approval.
                        </p>
                        <textarea
                          value={submissionNotes}
                          onChange={(e) => setSubmissionNotes(e.target.value)}
                          placeholder="Add any notes about your experiment (optional)..."
                          className="w-full p-3 border border-gray-300 rounded-lg mb-3 text-sm resize-none"
                          rows={3}
                        />
                        <button
                          onClick={() => handleSubmitExperiment(experiment.experimentId)}
                          disabled={submitting}
                          className="flex items-center gap-2 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 font-medium transition-colors disabled:opacity-50"
                        >
                          {submitting ? (
                            <>
                              <Loader size={18} className="animate-spin" />
                              Submitting...
                            </>
                          ) : (
                            <>
                              <CheckCircle size={18} />
                              Submit for Approval
                            </>
                          )}
                        </button>
                      </div>
                    )}
                  </div>

                  {/* Diagram Modal */}
                  {selectedDiagram && (
                    <div 
                      className="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4"
                      onClick={() => setSelectedDiagram(null)}
                    >
                      <div className="relative max-w-4xl max-h-full">
                        <button
                          onClick={() => setSelectedDiagram(null)}
                          className="absolute -top-12 right-0 text-white hover:text-gray-300"
                        >
                          <X size={24} />
                        </button>
                        <img 
                          src={selectedDiagram.startsWith('http') ? selectedDiagram : `${API_BASE_URL}${selectedDiagram}`}
                          alt="Enlarged Diagram"
                          className="w-full h-full object-contain rounded-lg"
                          onClick={(e) => e.stopPropagation()}
                        />
                      </div>
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Empty State */}
        {data.experiments.length === 0 && (
          <div className="text-center py-12">
            <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500">No experiments found for this level yet.</p>
          </div>
        )}
      </div>
    </div>
  );
}

// Wrapper with Suspense for useSearchParams
export default function ExperimentsPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader className="w-12 h-12 text-indigo-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600 font-medium">Loading...</p>
        </div>
      </div>
    }>
      <ExperimentsContent />
    </Suspense>
  );
}
