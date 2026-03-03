"use client";

import React, { useEffect, useState } from 'react';
import { fetchWithAuth, getRole } from '../../../lib/auth';
import { 
  ClipboardList, Plus, Loader, Search, Edit, Trash2, Eye, 
  ChevronDown, ChevronUp, Check, X, Clock, Award, Users,
  BookOpen, GraduationCap, AlertCircle, Save, ArrowLeft
} from 'lucide-react';

// Types
interface RoboticsLevel {
  id: number;
  name: string;
  levelNumber: number;
  description: string;
  totalExperiments: number;
  syllabusUrl?: string;
}

interface Question {
  questionNumber: number;
  questionText: string;
  questionType: 'MCQ' | 'TrueFalse' | 'ShortAnswer';
  options: string[];
  correctAnswer: string;
  marks: number;
  explanation?: string;
}

interface Exam {
  id: number;
  roboticsLevelId: number;
  roboticsLevelName: string;
  levelNumber: number;
  title: string;
  description?: string;
  durationMinutes: number;
  totalQuestions: number;
  totalMarks: number;
  passingPercentage: number;
  passingMarks: number;
  isActive: boolean;
  createdAt: string;
  questions?: Question[];
}

interface ExamResult {
  id: number;
  studentName: string;
  studentRollNo: string;
  schoolName: string;
  className: string;
  scoreObtained: number;
  totalMarks: number;
  percentage: number;
  isPassed: boolean;
  attemptedAt: string;
}

export default function AdminExams() {
  const [userRole, setUserRole] = useState<string | null>(null);
  const [levels, setLevels] = useState<RoboticsLevel[]>([]);
  const [exams, setExams] = useState<Exam[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedLevel, setSelectedLevel] = useState<number | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  
  // Modal states
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showViewModal, setShowViewModal] = useState(false);
  const [showResultsModal, setShowResultsModal] = useState(false);
  const [selectedExam, setSelectedExam] = useState<Exam | null>(null);
  const [examResults, setExamResults] = useState<ExamResult[]>([]);
  
  // Form states
  const [formData, setFormData] = useState({
    roboticsLevelId: 0,
    title: '',
    description: '',
    durationMinutes: 30,
    passingPercentage: 40,
    questions: [] as Question[]
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    const role = getRole();
    setUserRole(role);
    loadData();
  }, []);

  async function loadData() {
    try {
      setLoading(true);
      setError('');
      
      // Load levels and exams in parallel
      const [levelsData, examsData] = await Promise.all([
        fetchWithAuth('/api/admin/robotics-levels'),
        fetchWithAuth('/api/admin/exams')
      ]);
      
      setLevels(Array.isArray(levelsData) ? levelsData : []);
      const examsArray = examsData?.exams || (Array.isArray(examsData) ? examsData : []);
      setExams(examsArray);
    } catch (err) {
      console.error('Error loading data:', err);
      setError('Failed to load data');
    } finally {
      setLoading(false);
    }
  }

  // Filter exams by level and search
  const filteredExams = exams.filter(exam => {
    const matchesLevel = selectedLevel === null || exam.roboticsLevelId === selectedLevel;
    const matchesSearch = !searchTerm || 
      exam.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exam.roboticsLevelName.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesLevel && matchesSearch;
  });

  // Get exams for a specific level
  const getExamsForLevel = (levelId: number) => exams.filter(e => e.roboticsLevelId === levelId);

  // Opens create modal for a specific level
  function openCreateModal(levelId?: number) {
    setFormData({
      roboticsLevelId: levelId || (levels[0]?.id || 0),
      title: '',
      description: '',
      durationMinutes: 30,
      passingPercentage: 40,
      questions: [createEmptyQuestion(1)]
    });
    setError('');
    setShowCreateModal(true);
  }

  function createEmptyQuestion(num: number): Question {
    return {
      questionNumber: num,
      questionText: '',
      questionType: 'MCQ',
      options: ['', '', '', ''],
      correctAnswer: '',
      marks: 1,
      explanation: ''
    };
  }

  function addQuestion() {
    setFormData(prev => ({
      ...prev,
      questions: [...prev.questions, createEmptyQuestion(prev.questions.length + 1)]
    }));
  }

  function removeQuestion(index: number) {
    setFormData(prev => ({
      ...prev,
      questions: prev.questions
        .filter((_, i) => i !== index)
        .map((q, i) => ({ ...q, questionNumber: i + 1 }))
    }));
  }

  function updateQuestion(index: number, field: keyof Question, value: any) {
    setFormData(prev => ({
      ...prev,
      questions: prev.questions.map((q, i) => 
        i === index ? { ...q, [field]: value } : q
      )
    }));
  }

  function updateOption(qIndex: number, oIndex: number, value: string) {
    setFormData(prev => ({
      ...prev,
      questions: prev.questions.map((q, i) => 
        i === qIndex 
          ? { ...q, options: q.options.map((o, j) => j === oIndex ? value : o) }
          : q
      )
    }));
  }

  async function handleCreateExam() {
    if (!formData.title.trim()) {
      setError('Please enter exam title');
      return;
    }
    if (!formData.roboticsLevelId) {
      setError('Please select a level');
      return;
    }
    if (formData.questions.length === 0) {
      setError('Please add at least one question');
      return;
    }
    
    // Validate questions
    for (let i = 0; i < formData.questions.length; i++) {
      const q = formData.questions[i];
      if (!q.questionText.trim()) {
        setError(`Question ${i + 1}: Please enter question text`);
        return;
      }
      if (!q.correctAnswer.trim()) {
        setError(`Question ${i + 1}: Please select correct answer`);
        return;
      }
      if (q.questionType === 'MCQ' && q.options.filter(o => o.trim()).length < 2) {
        setError(`Question ${i + 1}: MCQ must have at least 2 options`);
        return;
      }
    }

    try {
      setSaving(true);
      setError('');
      
      await fetchWithAuth('/api/admin/exams', {
        method: 'POST',
        body: JSON.stringify({
          roboticsLevelId: formData.roboticsLevelId,
          title: formData.title,
          description: formData.description,
          durationMinutes: formData.durationMinutes,
          passingPercentage: formData.passingPercentage,
          questions: formData.questions.map(q => ({
            ...q,
            options: q.questionType === 'MCQ' ? q.options.filter(o => o.trim()) : 
                     q.questionType === 'TrueFalse' ? ['True', 'False'] : []
          }))
        })
      });
      
      setShowCreateModal(false);
      loadData();
    } catch (err: any) {
      console.error('Error creating exam:', err);
      setError(err.message || 'Failed to create exam');
    } finally {
      setSaving(false);
    }
  }

  async function handleViewExam(examId: number) {
    try {
      const exam = await fetchWithAuth(`/api/admin/exams/${examId}`);
      setSelectedExam(exam);
      setShowViewModal(true);
    } catch (err) {
      console.error('Error loading exam:', err);
    }
  }

  async function handleEditExam(examId: number) {
    try {
      const exam = await fetchWithAuth(`/api/admin/exams/${examId}`);
      setSelectedExam(exam);
      setFormData({
        roboticsLevelId: exam.roboticsLevelId,
        title: exam.title,
        description: exam.description || '',
        durationMinutes: exam.durationMinutes,
        passingPercentage: exam.passingPercentage,
        questions: exam.questions || []
      });
      setShowEditModal(true);
    } catch (err) {
      console.error('Error loading exam:', err);
    }
  }

  async function handleUpdateExam() {
    if (!selectedExam) return;
    
    try {
      setSaving(true);
      setError('');
      
      await fetchWithAuth(`/api/admin/exams/${selectedExam.id}`, {
        method: 'PUT',
        body: JSON.stringify({
          title: formData.title,
          description: formData.description,
          durationMinutes: formData.durationMinutes,
          passingPercentage: formData.passingPercentage,
          questions: formData.questions.map(q => ({
            ...q,
            options: q.questionType === 'MCQ' ? q.options.filter(o => o.trim()) : 
                     q.questionType === 'TrueFalse' ? ['True', 'False'] : []
          }))
        })
      });
      
      setShowEditModal(false);
      loadData();
    } catch (err: any) {
      console.error('Error updating exam:', err);
      setError(err.message || 'Failed to update exam');
    } finally {
      setSaving(false);
    }
  }

  async function handleDeleteExam(examId: number) {
    if (!confirm('Are you sure you want to delete this exam?')) return;
    
    try {
      await fetchWithAuth(`/api/admin/exams/${examId}`, { method: 'DELETE' });
      loadData();
    } catch (err) {
      console.error('Error deleting exam:', err);
    }
  }

  async function handleViewResults(examId: number) {
    try {
      const results = await fetchWithAuth(`/api/admin/exams/${examId}/results`);
      setExamResults(results?.results || []);
      const exam = exams.find(e => e.id === examId);
      setSelectedExam(exam || null);
      setShowResultsModal(true);
    } catch (err) {
      console.error('Error loading results:', err);
    }
  }

  const getLevelColor = (levelNumber: number) => {
    const colors = [
      'from-green-500 to-emerald-600',
      'from-blue-500 to-cyan-600',
      'from-purple-500 to-violet-600',
      'from-orange-500 to-amber-600',
      'from-pink-500 to-rose-600',
      'from-indigo-500 to-blue-600'
    ];
    return colors[(levelNumber - 1) % colors.length];
  };

  const isSuperAdmin = userRole === 'SuperAdmin';

  return (
    <div className="max-w-7xl mx-auto">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-lg flex items-center justify-center">
              <ClipboardList className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Exam Management</h1>
              <p className="text-sm text-gray-600">Create and manage exams for all 6 robotics levels</p>
            </div>
          </div>
          {isSuperAdmin && (
            <button
              onClick={() => openCreateModal()}
              className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-lg hover:from-blue-700 hover:to-indigo-700 transition-all shadow-sm"
            >
              <Plus className="w-5 h-5" />
              Create Exam
            </button>
          )}
        </div>

        {/* Level Stats */}
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
          {levels.map(level => {
            const levelExams = getExamsForLevel(level.id);
            return (
              <button
                key={level.id}
                onClick={() => setSelectedLevel(selectedLevel === level.id ? null : level.id)}
                className={`p-4 rounded-lg border-2 transition-all ${
                  selectedLevel === level.id 
                    ? 'border-blue-500 bg-blue-50' 
                    : 'border-gray-200 hover:border-gray-300'
                }`}
              >
                <div className={`w-10 h-10 rounded-lg bg-gradient-to-br ${getLevelColor(level.levelNumber)} flex items-center justify-center text-white font-bold text-lg mb-2`}>
                  {level.levelNumber}
                </div>
                <p className="font-semibold text-gray-900 text-sm">{level.name}</p>
                <p className="text-xs text-gray-500">{levelExams.length} exam(s)</p>
              </button>
            );
          })}
        </div>
      </div>

      {/* Search and Filter */}
      <div className="bg-white rounded-xl shadow-sm p-4 mb-6">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search exams..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          {selectedLevel && (
            <button
              onClick={() => setSelectedLevel(null)}
              className="px-4 py-2 text-sm text-gray-600 hover:text-gray-900 border border-gray-200 rounded-lg flex items-center gap-2"
            >
              <X className="w-4 h-4" />
              Clear Filter
            </button>
          )}
        </div>
      </div>

      {/* Exams List */}
      <div className="bg-white rounded-xl shadow-sm overflow-hidden">
        {loading ? (
          <div className="flex items-center justify-center py-12">
            <Loader className="w-8 h-8 text-blue-600 animate-spin" />
          </div>
        ) : filteredExams.length === 0 ? (
          <div className="text-center py-12">
            <ClipboardList className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500 mb-4">No exams found</p>
            {isSuperAdmin && (
              <button
                onClick={() => openCreateModal(selectedLevel || undefined)}
                className="text-blue-600 hover:underline"
              >
                Create your first exam
              </button>
            )}
          </div>
        ) : (
          <div className="divide-y divide-gray-100">
            {filteredExams.map(exam => (
              <div
                key={exam.id}
                className="p-4 hover:bg-gray-50 transition-colors"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <div className={`w-12 h-12 rounded-lg bg-gradient-to-br ${getLevelColor(exam.levelNumber)} flex items-center justify-center`}>
                      <span className="text-white font-bold text-lg">{exam.levelNumber}</span>
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900">{exam.title}</h3>
                      <div className="flex items-center gap-4 mt-1">
                        <span className="text-sm text-gray-500">{exam.roboticsLevelName}</span>
                        <span className="text-sm text-gray-400 flex items-center gap-1">
                          <Clock className="w-3 h-3" /> {exam.durationMinutes} min
                        </span>
                        <span className="text-sm text-gray-400 flex items-center gap-1">
                          <BookOpen className="w-3 h-3" /> {exam.totalQuestions} questions
                        </span>
                        <span className="text-sm text-gray-400 flex items-center gap-1">
                          <Award className="w-3 h-3" /> Pass: {exam.passingPercentage}%
                        </span>
                      </div>
                    </div>
                  </div>
                  
                  <div className="flex items-center gap-2">
                    <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${
                      exam.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                    }`}>
                      {exam.isActive ? 'Active' : 'Inactive'}
                    </span>
                    <button
                      onClick={() => handleViewExam(exam.id)}
                      className="p-2 text-gray-600 hover:bg-gray-100 rounded-lg"
                      title="View"
                    >
                      <Eye className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleViewResults(exam.id)}
                      className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg"
                      title="View Results"
                    >
                      <Users className="w-4 h-4" />
                    </button>
                    {isSuperAdmin && (
                      <>
                        <button
                          onClick={() => handleEditExam(exam.id)}
                          className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg"
                          title="Edit"
                        >
                          <Edit className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDeleteExam(exam.id)}
                          className="p-2 text-red-600 hover:bg-red-50 rounded-lg"
                          title="Delete"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Create/Edit Exam Modal */}
      {(showCreateModal || showEditModal) && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl w-full max-w-4xl max-h-[90vh] overflow-hidden flex flex-col">
            <div className="p-6 border-b border-gray-200 flex items-center justify-between">
              <h2 className="text-xl font-bold text-gray-900">
                {showCreateModal ? 'Create New Exam' : 'Edit Exam'}
              </h2>
              <button
                onClick={() => {
                  setShowCreateModal(false);
                  setShowEditModal(false);
                  setError('');
                }}
                className="p-2 hover:bg-gray-100 rounded-lg"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            
            <div className="flex-1 overflow-y-auto p-6">
              {error && (
                <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-center gap-2 text-red-700">
                  <AlertCircle className="w-5 h-5" />
                  {error}
                </div>
              )}
              
              {/* Exam Details */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Level</label>
                  <select
                    value={formData.roboticsLevelId}
                    onChange={(e) => setFormData(prev => ({ ...prev, roboticsLevelId: parseInt(e.target.value) }))}
                    disabled={showEditModal}
                    className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100"
                  >
                    {levels.map(level => (
                      <option key={level.id} value={level.id}>
                        Level {level.levelNumber}: {level.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Exam Title</label>
                  <input
                    type="text"
                    value={formData.title}
                    onChange={(e) => setFormData(prev => ({ ...prev, title: e.target.value }))}
                    placeholder="e.g., Level 1 Final Assessment"
                    className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Description (Optional)</label>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                    placeholder="Brief description of the exam..."
                    rows={2}
                    className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Duration (minutes)</label>
                  <input
                    type="number"
                    value={formData.durationMinutes}
                    onChange={(e) => setFormData(prev => ({ ...prev, durationMinutes: parseInt(e.target.value) || 30 }))}
                    min={5}
                    max={180}
                    className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Passing Percentage</label>
                  <input
                    type="number"
                    value={formData.passingPercentage}
                    onChange={(e) => setFormData(prev => ({ ...prev, passingPercentage: parseInt(e.target.value) || 40 }))}
                    min={1}
                    max={100}
                    className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>
              
              {/* Questions */}
              <div className="border-t pt-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="font-semibold text-gray-900">Questions ({formData.questions.length})</h3>
                  <button
                    onClick={addQuestion}
                    className="flex items-center gap-1 px-3 py-1.5 text-sm text-blue-600 hover:bg-blue-50 rounded-lg"
                  >
                    <Plus className="w-4 h-4" />
                    Add Question
                  </button>
                </div>
                
                <div className="space-y-4">
                  {formData.questions.map((question, qIndex) => (
                    <div key={qIndex} className="border border-gray-200 rounded-lg p-4">
                      <div className="flex items-start justify-between mb-3">
                        <span className="font-medium text-gray-700">Question {qIndex + 1}</span>
                        {formData.questions.length > 1 && (
                          <button
                            onClick={() => removeQuestion(qIndex)}
                            className="p-1 text-red-500 hover:bg-red-50 rounded"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                      
                      <div className="grid grid-cols-1 md:grid-cols-4 gap-3 mb-3">
                        <div className="md:col-span-3">
                          <input
                            type="text"
                            value={question.questionText}
                            onChange={(e) => updateQuestion(qIndex, 'questionText', e.target.value)}
                            placeholder="Enter question text..."
                            className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                          />
                        </div>
                        <div>
                          <select
                            value={question.questionType}
                            onChange={(e) => updateQuestion(qIndex, 'questionType', e.target.value)}
                            className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                          >
                            <option value="MCQ">Multiple Choice</option>
                            <option value="TrueFalse">True/False</option>
                            <option value="ShortAnswer">Short Answer</option>
                          </select>
                        </div>
                      </div>
                      
                      {question.questionType === 'MCQ' && (
                        <div className="space-y-2 mb-3">
                          <label className="text-sm text-gray-600">Options (select correct answer)</label>
                          {question.options.map((opt, oIndex) => (
                            <div key={oIndex} className="flex items-center gap-2">
                              <input
                                type="radio"
                                name={`correct-${qIndex}`}
                                checked={question.correctAnswer === opt && opt !== ''}
                                onChange={() => updateQuestion(qIndex, 'correctAnswer', opt)}
                                className="w-4 h-4 text-blue-600"
                                disabled={!opt.trim()}
                              />
                              <input
                                type="text"
                                value={opt}
                                onChange={(e) => updateOption(qIndex, oIndex, e.target.value)}
                                placeholder={`Option ${oIndex + 1}`}
                                className="flex-1 px-3 py-1.5 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                              />
                            </div>
                          ))}
                        </div>
                      )}
                      
                      {question.questionType === 'TrueFalse' && (
                        <div className="flex gap-4 mb-3">
                          <label className="flex items-center gap-2">
                            <input
                              type="radio"
                              name={`tf-${qIndex}`}
                              checked={question.correctAnswer === 'True'}
                              onChange={() => updateQuestion(qIndex, 'correctAnswer', 'True')}
                              className="w-4 h-4 text-blue-600"
                            />
                            <span>True</span>
                          </label>
                          <label className="flex items-center gap-2">
                            <input
                              type="radio"
                              name={`tf-${qIndex}`}
                              checked={question.correctAnswer === 'False'}
                              onChange={() => updateQuestion(qIndex, 'correctAnswer', 'False')}
                              className="w-4 h-4 text-blue-600"
                            />
                            <span>False</span>
                          </label>
                        </div>
                      )}
                      
                      {question.questionType === 'ShortAnswer' && (
                        <div className="mb-3">
                          <label className="text-sm text-gray-600">Expected Answer</label>
                          <input
                            type="text"
                            value={question.correctAnswer}
                            onChange={(e) => updateQuestion(qIndex, 'correctAnswer', e.target.value)}
                            placeholder="Enter expected answer..."
                            className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                          />
                        </div>
                      )}
                      
                      <div className="grid grid-cols-2 gap-3">
                        <div>
                          <label className="text-sm text-gray-600">Marks</label>
                          <input
                            type="number"
                            value={question.marks}
                            onChange={(e) => updateQuestion(qIndex, 'marks', parseFloat(e.target.value) || 1)}
                            min={0.5}
                            step={0.5}
                            className="w-full px-3 py-1.5 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                          />
                        </div>
                        <div>
                          <label className="text-sm text-gray-600">Explanation (optional)</label>
                          <input
                            type="text"
                            value={question.explanation || ''}
                            onChange={(e) => updateQuestion(qIndex, 'explanation', e.target.value)}
                            placeholder="Why is this the answer?"
                            className="w-full px-3 py-1.5 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                          />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
            
            <div className="p-6 border-t border-gray-200 flex justify-end gap-3">
              <button
                onClick={() => {
                  setShowCreateModal(false);
                  setShowEditModal(false);
                  setError('');
                }}
                className="px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg"
              >
                Cancel
              </button>
              <button
                onClick={showCreateModal ? handleCreateExam : handleUpdateExam}
                disabled={saving}
                className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-lg hover:from-blue-700 hover:to-indigo-700 disabled:opacity-50"
              >
                {saving ? (
                  <>
                    <Loader className="w-4 h-4 animate-spin" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save className="w-4 h-4" />
                    {showCreateModal ? 'Create Exam' : 'Update Exam'}
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* View Exam Modal */}
      {showViewModal && selectedExam && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl w-full max-w-3xl max-h-[90vh] overflow-hidden flex flex-col">
            <div className="p-6 border-b border-gray-200 flex items-center justify-between">
              <div>
                <h2 className="text-xl font-bold text-gray-900">{selectedExam.title}</h2>
                <p className="text-sm text-gray-500">{selectedExam.roboticsLevelName}</p>
              </div>
              <button
                onClick={() => setShowViewModal(false)}
                className="p-2 hover:bg-gray-100 rounded-lg"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            
            <div className="flex-1 overflow-y-auto p-6">
              <div className="grid grid-cols-3 gap-4 mb-6">
                <div className="bg-gray-50 p-3 rounded-lg">
                  <p className="text-sm text-gray-500">Duration</p>
                  <p className="font-semibold">{selectedExam.durationMinutes} minutes</p>
                </div>
                <div className="bg-gray-50 p-3 rounded-lg">
                  <p className="text-sm text-gray-500">Total Marks</p>
                  <p className="font-semibold">{selectedExam.totalMarks}</p>
                </div>
                <div className="bg-gray-50 p-3 rounded-lg">
                  <p className="text-sm text-gray-500">Passing</p>
                  <p className="font-semibold">{selectedExam.passingPercentage}% ({selectedExam.passingMarks} marks)</p>
                </div>
              </div>
              
              {selectedExam.description && (
                <p className="text-gray-600 mb-6">{selectedExam.description}</p>
              )}
              
              <h3 className="font-semibold text-gray-900 mb-4">Questions ({selectedExam.totalQuestions})</h3>
              <div className="space-y-4">
                {selectedExam.questions?.map((q, i) => (
                  <div key={i} className="border border-gray-200 rounded-lg p-4">
                    <div className="flex items-start justify-between mb-2">
                      <span className="font-medium">Q{q.questionNumber}. {q.questionText}</span>
                      <span className="text-sm text-gray-500">{q.marks} mark(s)</span>
                    </div>
                    {q.questionType === 'MCQ' && (
                      <div className="space-y-1 ml-4">
                        {q.options.map((opt, j) => (
                          <div key={j} className={`flex items-center gap-2 ${opt === q.correctAnswer ? 'text-green-600 font-medium' : 'text-gray-600'}`}>
                            {opt === q.correctAnswer ? <Check className="w-4 h-4" /> : <span className="w-4 h-4" />}
                            {opt}
                          </div>
                        ))}
                      </div>
                    )}
                    {q.questionType === 'TrueFalse' && (
                      <p className="ml-4 text-green-600 font-medium">Answer: {q.correctAnswer}</p>
                    )}
                    {q.questionType === 'ShortAnswer' && (
                      <p className="ml-4 text-green-600">Expected: {q.correctAnswer}</p>
                    )}
                    {q.explanation && (
                      <p className="mt-2 text-sm text-gray-500 italic">Explanation: {q.explanation}</p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Results Modal */}
      {showResultsModal && selectedExam && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl w-full max-w-4xl max-h-[90vh] overflow-hidden flex flex-col">
            <div className="p-6 border-b border-gray-200 flex items-center justify-between">
              <div>
                <h2 className="text-xl font-bold text-gray-900">Exam Results</h2>
                <p className="text-sm text-gray-500">{selectedExam.title}</p>
              </div>
              <button
                onClick={() => setShowResultsModal(false)}
                className="p-2 hover:bg-gray-100 rounded-lg"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
            
            <div className="flex-1 overflow-y-auto">
              {examResults.length === 0 ? (
                <div className="text-center py-12">
                  <Users className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                  <p className="text-gray-500">No results yet</p>
                </div>
              ) : (
                <table className="w-full">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-700">Student</th>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-700">School</th>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-700">Class</th>
                      <th className="px-4 py-3 text-center text-sm font-medium text-gray-700">Score</th>
                      <th className="px-4 py-3 text-center text-sm font-medium text-gray-700">Status</th>
                      <th className="px-4 py-3 text-left text-sm font-medium text-gray-700">Date</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {examResults.map(result => (
                      <tr key={result.id} className="hover:bg-gray-50">
                        <td className="px-4 py-3">
                          <div>
                            <p className="font-medium text-gray-900">{result.studentName}</p>
                            <p className="text-sm text-gray-500">{result.studentRollNo}</p>
                          </div>
                        </td>
                        <td className="px-4 py-3 text-gray-600">{result.schoolName}</td>
                        <td className="px-4 py-3 text-gray-600">{result.className}</td>
                        <td className="px-4 py-3 text-center">
                          <span className="font-medium">{result.scoreObtained}/{result.totalMarks}</span>
                          <span className="text-sm text-gray-500 ml-1">({result.percentage.toFixed(1)}%)</span>
                        </td>
                        <td className="px-4 py-3 text-center">
                          <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${
                            result.isPassed ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                          }`}>
                            {result.isPassed ? 'Passed' : 'Failed'}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-sm text-gray-500">
                          {new Date(result.attemptedAt).toLocaleDateString()}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}