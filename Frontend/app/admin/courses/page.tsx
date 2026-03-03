"use client";

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { fetchWithAuth, getRole } from '../../../lib/auth';
import { 
  BookOpen, Loader, Search, Eye, FileText, ClipboardList, 
  Beaker, Award, Download, ExternalLink, Layers
} from 'lucide-react';

interface RoboticsLevel {
  id: number;
  name: string;
  levelNumber: number;
  description: string;
  totalExperiments: number;
  syllabusUrl?: string;
}

export default function AdminCourses() {
  const [levels, setLevels] = useState<RoboticsLevel[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [userRole, setUserRole] = useState<string | null>(null);

  useEffect(() => {
    const role = getRole();
    setUserRole(role);
    loadLevels();
  }, []);

  async function loadLevels() {
    try {
      setLoading(true);
      const data = await fetchWithAuth('/api/admin/robotics-levels');
      const levelsArray = Array.isArray(data) ? data : [];
      setLevels(levelsArray.sort((a, b) => a.levelNumber - b.levelNumber));
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  const filteredLevels = levels.filter((level) =>
    level.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    level.description?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getLevelColor = (levelNumber: number) => {
    const colors = [
      { bg: 'from-green-500 to-emerald-600', light: 'bg-green-100', text: 'text-green-600' },
      { bg: 'from-blue-500 to-cyan-600', light: 'bg-blue-100', text: 'text-blue-600' },
      { bg: 'from-purple-500 to-violet-600', light: 'bg-purple-100', text: 'text-purple-600' },
      { bg: 'from-orange-500 to-amber-600', light: 'bg-orange-100', text: 'text-orange-600' },
      { bg: 'from-pink-500 to-rose-600', light: 'bg-pink-100', text: 'text-pink-600' },
      { bg: 'from-indigo-500 to-blue-600', light: 'bg-indigo-100', text: 'text-indigo-600' },
    ];
    return colors[(levelNumber - 1) % colors.length];
  };

  const totalExperiments = levels.reduce((sum, l) => sum + (l.totalExperiments || 0), 0);

  return (
    <div className="max-w-7xl mx-auto">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-lg flex items-center justify-center">
              <Layers className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Robotics Courses</h1>
              <p className="text-sm text-gray-600">6 Progressive levels of robotics education</p>
            </div>
          </div>
        </div>

        {/* Info Banner */}
        <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg p-4 border border-blue-100">
          <p className="text-sm text-blue-800">
            <strong>Motion Robotics Program:</strong> Students progress through 6 levels, from basic concepts to advanced robotics. 
            Each level includes hands-on experiments and assessments.
          </p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Levels</p>
              <p className="text-2xl font-bold text-gray-900">{levels.length}</p>
            </div>
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <Layers className="w-6 h-6 text-blue-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Experiments</p>
              <p className="text-2xl font-bold text-gray-900">{totalExperiments}</p>
            </div>
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <Beaker className="w-6 h-6 text-green-600" />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Certifications</p>
              <p className="text-2xl font-bold text-gray-900">{levels.length}</p>
            </div>
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
              <Award className="w-6 h-6 text-purple-600" />
            </div>
          </div>
        </div>
      </div>

      {/* Search */}
      <div className="bg-white rounded-xl shadow-sm p-4 mb-6">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            type="text"
            placeholder="Search levels..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Levels Grid */}
      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader className="w-8 h-8 text-blue-600 animate-spin" />
        </div>
      ) : filteredLevels.length === 0 ? (
        <div className="bg-white rounded-xl shadow-sm p-12 text-center">
          <Layers className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <p className="text-gray-500">No levels found</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredLevels.map((level) => {
            const colors = getLevelColor(level.levelNumber);
            return (
              <div
                key={level.id}
                className="bg-white rounded-xl shadow-sm overflow-hidden hover:shadow-md transition-shadow"
              >
                {/* Level Header */}
                <div className={`bg-gradient-to-r ${colors.bg} p-4`}>
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-white/20 backdrop-blur rounded-lg flex items-center justify-center">
                      <span className="text-white font-bold text-xl">{level.levelNumber}</span>
                    </div>
                    <div className="text-white">
                      <h3 className="font-bold text-lg">{level.name}</h3>
                      <p className="text-white/80 text-sm">Level {level.levelNumber}</p>
                    </div>
                  </div>
                </div>

                {/* Level Content */}
                <div className="p-4">
                  <p className="text-gray-600 text-sm mb-4 line-clamp-2">
                    {level.description || 'Learn robotics concepts through hands-on experiments and projects.'}
                  </p>

                  {/* Stats */}
                  <div className="flex items-center gap-4 mb-4">
                    <div className="flex items-center gap-1 text-sm text-gray-500">
                      <Beaker className="w-4 h-4" />
                      <span>{level.totalExperiments} experiments</span>
                    </div>
                    <div className="flex items-center gap-1 text-sm text-gray-500">
                      <ClipboardList className="w-4 h-4" />
                      <span>1 exam</span>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex items-center gap-2">
                    {level.syllabusUrl && (
                      <a
                        href={level.syllabusUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className={`flex items-center gap-1 px-3 py-1.5 rounded-lg text-sm ${colors.light} ${colors.text} hover:opacity-80`}
                      >
                        <FileText className="w-4 h-4" />
                        Syllabus
                      </a>
                    )}
                    <Link
                      href={`/admin/exams?level=${level.id}`}
                      className="flex items-center gap-1 px-3 py-1.5 rounded-lg text-sm bg-gray-100 text-gray-700 hover:bg-gray-200"
                    >
                      <ClipboardList className="w-4 h-4" />
                      View Exams
                    </Link>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Level Progression Info */}
      <div className="mt-8 bg-white rounded-xl shadow-sm p-6">
        <h3 className="font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <BookOpen className="w-5 h-5 text-blue-600" />
          Level Progression
        </h3>
        <div className="relative">
          <div className="absolute top-5 left-6 right-6 h-0.5 bg-gray-200"></div>
          <div className="flex justify-between relative">
            {levels.map((level, index) => {
              const colors = getLevelColor(level.levelNumber);
              return (
                <div key={level.id} className="flex flex-col items-center">
                  <div className={`w-10 h-10 rounded-full bg-gradient-to-br ${colors.bg} flex items-center justify-center text-white font-bold shadow-sm z-10`}>
                    {level.levelNumber}
                  </div>
                  <p className="text-xs text-gray-600 mt-2 text-center max-w-[80px]">
                    {level.name.split(' ').slice(0, 2).join(' ')}
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}