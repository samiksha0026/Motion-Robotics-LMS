"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { 
  GraduationCap, 
  Users, 
  Loader,
  AlertCircle,
  BookOpen,
  FlaskConical,
  Unlock,
  Lock
} from "lucide-react";
import { API_BASE_URL } from "@/lib/api-config";

interface AssignedLevel {
  mappingId: number;
  levelId: number;
  levelNumber: number;
  levelName: string;
}

interface ClassData {
  classId: number;
  className: string;
  schoolId: number;
  schoolName: string;
  studentCount: number;
  assignedLevel: AssignedLevel | null;
}

interface ClassesResponse {
  academicYear: { id: number; yearName: string };
  classes: ClassData[];
}

export default function TeacherLevelsPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(true);
  const [classesData, setClassesData] = useState<ClassesResponse | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    setLoading(true);
    setError("");
    
    try {
      const token = sessionStorage.getItem("jwt");
      if (!token) {
        window.location.href = "/login";
        setLoading(false);
        return;
      }

      const res = await fetch(`${API_BASE_URL}/api/teacher/levels/my-classes`, {
        headers: { Authorization: `Bearer ${token}` }
      });

      if (!res.ok) {
        throw new Error("Failed to load data");
      }

      const data = await res.json();
      setClassesData(data);
    } catch (err) {
      console.error(err);
      setError("Failed to load data. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader className="animate-spin h-10 w-10 text-green-600 mx-auto mb-4" />
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
            <FlaskConical className="text-green-600" size={36} />
            Manage Class Experiments
          </h1>
          <p className="text-gray-600 mt-2">
            Unlock experiments for your classes to allow students to work on them
          </p>
          {classesData?.academicYear && (
            <p className="text-sm text-green-700 mt-1 font-medium">
              Academic Year: {classesData.academicYear.yearName}
            </p>
          )}
        </div>

        {/* Messages */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3 text-red-700">
            <AlertCircle size={20} />
            {error}
          </div>
        )}

        {/* Classes List */}
        {!classesData?.classes?.length ? (
          <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
            <Users size={48} className="mx-auto text-gray-400 mb-4" />
            <h3 className="text-lg font-semibold text-gray-900 mb-2">No Classes Assigned</h3>
            <p className="text-gray-600">
              You haven&apos;t been assigned to any classes yet. Contact your school admin.
            </p>
          </div>
        ) : (
          <div className="space-y-6">
            {classesData.classes.map((cls) => (
              <div 
                key={cls.classId} 
                className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden"
              >
                {/* Class Header */}
                <div className={`p-4 text-white ${
                  cls.assignedLevel 
                    ? "bg-gradient-to-r from-green-600 to-emerald-600" 
                    : "bg-gradient-to-r from-gray-500 to-gray-600"
                }`}>
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="text-xl font-bold">{cls.className}</h3>
                      <p className="text-green-100 text-sm">
                        {cls.schoolName}
                      </p>
                    </div>
                    <div className="flex items-center gap-2 bg-white/20 px-4 py-2 rounded-lg">
                      <Users size={18} />
                      <span className="font-medium">{cls.studentCount} Students</span>
                    </div>
                  </div>
                </div>

                {/* Level Info & Actions */}
                <div className="p-6">
                  {cls.assignedLevel ? (
                    <>
                      {/* Assigned Level Display */}
                      <div className="flex items-center gap-4 mb-4">
                        <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                          <GraduationCap className="text-green-600" size={24} />
                        </div>
                        <div>
                          <p className="text-sm text-gray-500">Assigned Level</p>
                          <p className="font-semibold text-gray-900">
                            Level {cls.assignedLevel.levelNumber}: {cls.assignedLevel.levelName}
                          </p>
                        </div>
                      </div>

                      {/* Manage Experiments Button */}
                      <button
                        onClick={() => router.push(`/teacher/experiments/${cls.classId}`)}
                        className="w-full p-4 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-lg hover:from-blue-700 hover:to-indigo-700 transition-colors flex items-center justify-center gap-3 font-medium"
                      >
                        <Unlock size={20} />
                        Manage Experiments for {cls.className}
                      </button>

                      <p className="text-sm text-gray-500 mt-3 text-center">
                        Click above to unlock experiments, set deadlines, and track student progress
                      </p>
                    </>
                  ) : (
                    // No Level Assigned
                    <div className="text-center py-6">
                      <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                        <Lock size={32} className="text-gray-400" />
                      </div>
                      <h4 className="font-semibold text-gray-900 mb-2">No Level Assigned</h4>
                      <p className="text-gray-600 text-sm">
                        Contact your school admin to assign a robotics level to this class.
                      </p>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Help Section */}
        <div className="mt-8 p-6 bg-blue-50 rounded-xl border border-blue-200">
          <h3 className="font-semibold text-blue-900 mb-2 flex items-center gap-2">
            <BookOpen size={18} />
            How Experiment Management Works
          </h3>
          <ul className="text-sm text-blue-800 space-y-1">
            <li>• Your school admin assigns a robotics level to each class</li>
            <li>• Click <strong>&quot;Manage Experiments&quot;</strong> to see all experiments in that level</li>
            <li>• <strong>Unlock experiments</strong> one by one as you teach them</li>
            <li>• Set <strong>deadlines</strong> for each experiment</li>
            <li>• Students can only see and work on experiments you have unlocked</li>
            <li>• Track <strong>student progress</strong> for each experiment</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
