"use client";

import React, { useEffect, useState } from 'react';
import { fetchWithAuth } from '../../../lib/auth';
import { BarChart3, Download, Loader, TrendingUp, Users, BookOpen, Award, FileText, Eye } from 'lucide-react';

interface ReportStats {
  totalStudents: number;
  totalCourses: number;
  certificatesIssued: number;
}

export default function AdminReports() {
  const [stats, setStats] = useState<ReportStats>({
    totalStudents: 0,
    totalCourses: 6,
    certificatesIssued: 0
  });
  const [loading, setLoading] = useState(true);
  const [selectedPeriod, setSelectedPeriod] = useState('month');
  const [reportData, setReportData] = useState<any>(null);

  async function load() {
    try {
      setLoading(true);
      
      // Fetch actual stats from various endpoints
      const [students, certificates, reportResponse] = await Promise.all([
        fetchWithAuth('/api/admin/students').catch(() => []),
        fetchWithAuth('/api/admin/certificates').catch(() => ({ certificates: [], totalCount: 0 })),
        fetchWithAuth(`/api/admin/reports/${selectedPeriod === 'week' ? 'weekly' : 'monthly'}`).catch(() => null)
      ]);
      
      const studentsArray = Array.isArray(students) ? students : [];
      const certsArray = Array.isArray(certificates) ? certificates : (certificates?.certificates || []);
      
      setStats({
        totalStudents: studentsArray.length,
        totalCourses: 6, // 6 fixed robotics levels
        certificatesIssued: certsArray.length
      });
      
      if (reportResponse?.data) {
        setReportData(reportResponse.data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, [selectedPeriod]);

  const quickStats = [
    {
      title: 'Total Students',
      value: stats.totalStudents.toLocaleString(),
      change: '',
      icon: Users,
      color: 'from-blue-500 to-blue-600',
      bgColor: 'bg-blue-100',
      iconColor: 'text-blue-600'
    },
    {
      title: 'Robotics Levels',
      value: stats.totalCourses.toString(),
      change: '',
      icon: BookOpen,
      color: 'from-indigo-500 to-indigo-600',
      bgColor: 'bg-indigo-100',
      iconColor: 'text-indigo-600'
    },
    {
      title: 'Certificates Issued',
      value: stats.certificatesIssued.toLocaleString(),
      change: '',
      icon: Award,
      color: 'from-purple-500 to-purple-600',
      bgColor: 'bg-purple-100',
      iconColor: 'text-purple-600'
    }
  ];

  const reportCategories = [
    { name: 'Student Performance', icon: Users, count: reportData?.studentCount || 0 },
    { name: 'Course Analytics', icon: BookOpen, count: 6 },
    { name: 'Certificate Reports', icon: Award, count: stats.certificatesIssued },
  ];

  return (
    <div className="max-w-7xl mx-auto">
      {/* Header */}
      <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-lg flex items-center justify-center">
              <BarChart3 className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Analytics & Reports</h1>
              <p className="text-sm text-gray-600">View performance metrics and generate reports</p>
            </div>
          </div>

          {/* Period Selector */}
          <div className="flex gap-2">
            {['week', 'month', 'year'].map((period) => (
              <button
                key={period}
                onClick={() => setSelectedPeriod(period)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                  selectedPeriod === period
                    ? 'bg-gradient-to-r from-blue-600 to-indigo-600 text-white shadow-sm'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                {period.charAt(0).toUpperCase() + period.slice(1)}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        {quickStats.map((stat, index) => {
          const Icon = stat.icon;
          return (
            <div key={index} className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow">
              <div className="flex items-center justify-between mb-3">
                <div className={`w-12 h-12 ${stat.bgColor} rounded-lg flex items-center justify-center`}>
                  <Icon className={`w-6 h-6 ${stat.iconColor}`} />
                </div>
                <span className="text-sm font-medium text-green-600 flex items-center gap-1">
                  <TrendingUp className="w-3 h-3" />
                  {stat.change}
                </span>
              </div>
              <p className="text-sm text-gray-600 mb-1">{stat.title}</p>
              <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
            </div>
          );
        })}
      </div>

      {/* Report Categories */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
        {reportCategories.map((category, index) => {
          const Icon = category.icon;
          return (
            <div
              key={index}
              className="bg-white rounded-xl shadow-sm p-5 hover:shadow-md transition-shadow cursor-pointer group"
            >
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-blue-100 to-indigo-100 rounded-lg flex items-center justify-center group-hover:scale-110 transition-transform">
                    <Icon className="w-6 h-6 text-blue-600" />
                  </div>
                  <div>
                    <p className="font-semibold text-gray-900">{category.name}</p>
                    <p className="text-sm text-gray-500">{category.count} reports available</p>
                  </div>
                </div>
                <button className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                  <Eye className="w-5 h-5" />
                </button>
              </div>
            </div>
          );
        })}
      </div>

      {/* Generated Reports */}
      <div className="bg-white rounded-xl shadow-sm overflow-hidden">
        <div className="p-4 border-b border-gray-100 flex items-center justify-between">
          <h3 className="font-semibold text-gray-900 flex items-center gap-2">
            <FileText className="w-5 h-5" />
            Generated Reports
          </h3>
          <button className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-lg hover:from-blue-700 hover:to-indigo-700 transition-all shadow-sm hover:shadow text-sm">
            <Download className="w-4 h-4" />
            Export All
          </button>
        </div>

        {loading ? (
          <div className="flex items-center justify-center py-12">
            <Loader className="w-8 h-8 text-blue-600 animate-spin" />
          </div>
        ) : !reportData ? (
          <div className="text-center py-12">
            <BarChart3 className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500 mb-4">No report data available</p>
            <button 
              onClick={load}
              className="px-4 py-2 bg-gradient-to-r from-blue-600 to-indigo-600 text-white rounded-lg hover:from-blue-700 hover:to-indigo-700 transition-all shadow-sm text-sm"
            >
              Refresh Reports
            </button>
          </div>
        ) : (
          <div className="divide-y divide-gray-100">
            <div className="p-4 hover:bg-gray-50 transition-colors flex items-center justify-between">
              <div className="flex items-center gap-4 flex-1">
                <div className="w-10 h-10 bg-gradient-to-br from-blue-100 to-indigo-100 rounded-lg flex items-center justify-center">
                  <FileText className="w-5 h-5 text-blue-600" />
                </div>
                <div className="flex-1">
                  <p className="font-medium text-gray-900">{selectedPeriod === 'week' ? 'Weekly' : 'Monthly'} Summary Report</p>
                  <div className="flex items-center gap-3 mt-1">
                    <p className="text-sm text-gray-500">
                      Generated on {new Date().toLocaleDateString()}
                    </p>
                    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-700">
                      {selectedPeriod === 'week' ? 'Last 7 Days' : 'Last 30 Days'}
                    </span>
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <button
                  className="flex items-center gap-2 px-3 py-1.5 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors text-sm"
                  title="View Report"
                >
                  <Eye className="w-4 h-4" />
                  View
                </button>
                <button
                  className="flex items-center gap-2 px-3 py-1.5 text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors text-sm"
                  title="Download Report"
                >
                  <Download className="w-4 h-4" />
                  Download
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}