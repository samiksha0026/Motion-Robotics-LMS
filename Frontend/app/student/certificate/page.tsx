"use client";

import { useState, useEffect } from "react";
import {
  Download, Share2, Award, Calendar, User, CheckCircle,
  ArrowLeft, Loader, AlertCircle, Mail, MapPin, Trophy
} from "lucide-react";
import { API_BASE_URL } from "@/lib/api-config";

interface Certificate {
  id: number;
  studentName: string;
  level: string;
  issuedDate: string;
  certificateNumber: string;
  score: number;
  certificateUrl: string;
  status: "pending" | "issued" | "revoked";
  exams: Array<{
    title: string;
    completedDate: string;
    score: number;
  }>;
}

export default function CertificatePage() {
  const [certificates, setCertificates] = useState<Certificate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedCertificate, setSelectedCertificate] = useState<Certificate | null>(null);
  const [showPreview, setShowPreview] = useState(false);

  useEffect(() => {
    fetchCertificates();
  }, []);

  const fetchCertificates = async () => {
    try {
      setLoading(true);
      setError(null);

      const token = typeof window !== "undefined" ? sessionStorage.getItem("jwt") : null;

      if (!token) {
        window.location.href = "/login";
        return;
      }

      const sessionId = typeof window !== "undefined" ? sessionStorage.getItem("sessionId") : null;
      const response = await fetch(`${API_BASE_URL}/api/student/certificates`, {
        credentials: "include",
        headers: {
          Authorization: `Bearer ${token}`,
          ...(sessionId ? { "X-Session-Id": sessionId } : {}),
          "Content-Type": "application/json",
        },
      });

      if (!response.ok) {
        throw new Error("Failed to fetch certificates");
      }

      const data = await response.json();
      setCertificates(data);
      setLoading(false);
    } catch (err) {
      console.error("Error fetching certificates:", err);
      setError("Failed to load certificates. Please try again.");
      setCertificates([]);
      setLoading(false);
    }
  };

  const downloadCertificate = async (cert: Certificate) => {
    try {
      const link = document.createElement("a");
      link.href = cert.certificateUrl;
      link.download = `certificate-${cert.certificateNumber}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch (err) {
      console.error("Error downloading certificate:", err);
      alert("Failed to download certificate");
    }
  };

  const shareCertificate = async (cert: Certificate) => {
    try {
      const text = `I have successfully completed ${cert.level} with a score of ${cert.score}%! Certificate Number: ${cert.certificateNumber}`;
      
      if (navigator.share) {
        await navigator.share({
          title: "My Certificate",
          text: text,
          url: window.location.href,
        });
      } else {
        // Fallback: copy to clipboard
        await navigator.clipboard.writeText(text);
        alert("Certificate details copied to clipboard!");
      }
    } catch (err) {
      console.error("Error sharing certificate:", err);
    }
  };

  // Loading state
  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader className="w-12 h-12 text-indigo-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600 font-medium">Loading your certificates...</p>
        </div>
      </div>
    );
  }
  // Error state
  if (error && certificates.length === 0) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center max-w-md">
          <AlertCircle className="w-12 h-12 text-red-600 mx-auto mb-4" />
          <p className="text-gray-900 font-medium mb-2">Failed to Load Certificates</p>
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

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 sticky top-0 z-30">
        <div className="max-w-7xl mx-auto px-4 h-16 flex items-center justify-between">
          <button
            onClick={() => {
              if (showPreview) {
                setShowPreview(false);
                setSelectedCertificate(null);
              } else {
                window.history.back();
              }
            }}
            className="flex items-center gap-2 px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <ArrowLeft size={20} />
            <span className="font-medium hidden sm:inline">
              {showPreview ? "Back to Certificates" : "Back"}
            </span>
          </button>
          <h1 className="text-xl font-bold text-gray-900">My Certificates</h1>
          <div className="w-32" />
        </div>
      </header>

      {/* Certificate Preview Modal */}
      {showPreview && selectedCertificate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
            {/* Certificate Template */}
            <div className="aspect-video bg-gradient-to-br from-yellow-50 via-amber-50 to-orange-50 p-8 flex flex-col justify-between relative border-8 border-amber-900">
              {/* Decorative corners */}
              <div className="absolute top-4 left-4 w-8 h-8 border-t-2 border-l-2 border-amber-900" />
              <div className="absolute top-4 right-4 w-8 h-8 border-t-2 border-r-2 border-amber-900" />
              <div className="absolute bottom-4 left-4 w-8 h-8 border-b-2 border-l-2 border-amber-900" />
              <div className="absolute bottom-4 right-4 w-8 h-8 border-b-2 border-r-2 border-amber-900" />

              {/* Header */}
              <div className="text-center mb-6">
                <div className="flex items-center justify-center gap-2 mb-4">
                  <Trophy size={40} className="text-amber-700" />
                  <h1 className="text-4xl font-bold text-amber-900">Certificate</h1>
                  <Trophy size={40} className="text-amber-700" />
                </div>
                <p className="text-amber-700 text-lg font-script">of Achievement</p>
              </div>

              {/* Body */}
              <div className="text-center space-y-4">
                <p className="text-amber-900 text-sm">This is to certify that</p>
                <h2 className="text-3xl font-bold text-amber-900 border-b-2 border-amber-900 pb-2">
                  {selectedCertificate.studentName}
                </h2>
                <p className="text-amber-900 text-sm">has successfully completed</p>
                <p className="text-xl font-semibold text-amber-900">
                  {selectedCertificate.level}
                </p>
                <p className="text-amber-900 text-sm">
                  with a commendable score of <span className="font-bold">{selectedCertificate.score}%</span>
                </p>
              </div>

              {/* Footer */}
              <div className="flex justify-between items-end text-sm text-amber-900">
                <div>
                  <p className="font-semibold">Certificate No.</p>
                  <p>{selectedCertificate.certificateNumber}</p>
                </div>
                <div className="text-center">
                  <p className="font-semibold">Date of Issue</p>
                  <p>{new Date(selectedCertificate.issuedDate).toLocaleDateString()}</p>
                </div>
                <div className="text-right">
                  <p className="font-semibold">Director</p>
                  <p className="mt-6 border-t border-amber-900">Motion Robotics</p>
                </div>
              </div>
            </div>

            {/* Close Button */}
            <div className="bg-gray-100 p-4 flex justify-end gap-3">
              <button
                onClick={() => {
                  setShowPreview(false);
                  setSelectedCertificate(null);
                }}
                className="px-6 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 font-medium"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="max-w-6xl mx-auto p-4 lg:p-6">
        {certificates.length === 0 ? (
          // No certificates
          <div className="text-center py-12">
            <Trophy size={48} className="text-gray-400 mx-auto mb-4" />
            <h2 className="text-2xl font-bold text-gray-900 mb-2">No Certificates Yet</h2>
            <p className="text-gray-600 mb-6">
              Complete exams and courses to earn certificates
            </p>
            <button
              onClick={() => (window.location.href = "/student/exam")}
              className="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-medium"
            >
              Take an Exam
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {certificates.map((cert) => (
              <div
                key={cert.id}
                className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden hover:shadow-lg transition-shadow"
              >
                {/* Card Header */}
                <div className="bg-gradient-to-r from-amber-500 to-orange-500 p-6 text-white">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex-1">
                      <h2 className="text-xl font-bold mb-1">{cert.level}</h2>
                      <p className="text-amber-50 text-sm">{cert.studentName}</p>
                    </div>
                    <div className="bg-white/20 p-3 rounded-lg">
                      <Award size={24} />
                    </div>
                  </div>
                </div>

                {/* Card Body */}
                <div className="p-6 space-y-4">
                  {/* Status Badge */}
                  <div className="flex items-center gap-2">
                    {cert.status === "issued" ? (
                      <>
                        <CheckCircle size={18} className="text-green-600" />
                        <span className="text-sm font-medium text-green-700 bg-green-50 px-3 py-1 rounded-full">
                          Certificate Issued
                        </span>
                      </>
                    ) : cert.status === "pending" ? (
                      <>
                        <AlertCircle size={18} className="text-yellow-600" />
                        <span className="text-sm font-medium text-yellow-700 bg-yellow-50 px-3 py-1 rounded-full">
                          Pending
                        </span>
                      </>
                    ) : (
                      <>
                        <AlertCircle size={18} className="text-red-600" />
                        <span className="text-sm font-medium text-red-700 bg-red-50 px-3 py-1 rounded-full">
                          Revoked
                        </span>
                      </>
                    )}
                  </div>

                  {/* Certificate Details */}
                  <div className="space-y-3 border-t border-gray-200 pt-4">
                    {/* Score */}
                    <div className="flex justify-between items-center">
                      <span className="text-gray-600 text-sm">Score</span>
                      <span className="text-xl font-bold text-indigo-600">{cert.score}%</span>
                    </div>

                    {/* Certificate Number */}
                    <div className="flex justify-between items-center">
                      <span className="text-gray-600 text-sm">Certificate No.</span>
                      <span className="text-sm font-mono text-gray-900">
                        {cert.certificateNumber}
                      </span>
                    </div>

                    {/* Issued Date */}
                    <div className="flex justify-between items-center">
                      <span className="text-gray-600 text-sm flex items-center gap-2">
                        <Calendar size={16} />
                        Issued Date
                      </span>
                      <span className="text-sm text-gray-900">
                        {new Date(cert.issuedDate).toLocaleDateString()}
                      </span>
                    </div>
                  </div>

                  {/* Exams Completed */}
                  <div className="border-t border-gray-200 pt-4">
                    <p className="text-sm font-semibold text-gray-900 mb-3">Exams Completed</p>
                    <div className="space-y-2">
                      {cert.exams.map((exam, idx) => (
                        <div key={idx} className="text-xs text-gray-600 flex justify-between">
                          <span>{exam.title}</span>
                          <span className="font-medium">{exam.score}%</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  {/* Action Buttons */}
                  <div className="border-t border-gray-200 pt-4 flex gap-2">
                    <button
                      onClick={() => {
                        setSelectedCertificate(cert);
                        setShowPreview(true);
                      }}
                      className="flex-1 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-medium text-sm transition-colors"
                    >
                      View Certificate
                    </button>
                    {cert.status === "issued" && (
                      <>
                        <button
                          onClick={() => downloadCertificate(cert)}
                          className="px-4 py-2 bg-blue-50 text-blue-600 rounded-lg hover:bg-blue-100 font-medium text-sm transition-colors flex items-center gap-2"
                        >
                          <Download size={16} />
                          Download
                        </button>
                        <button
                          onClick={() => shareCertificate(cert)}
                          className="px-4 py-2 bg-green-50 text-green-600 rounded-lg hover:bg-green-100 font-medium text-sm transition-colors flex items-center gap-2"
                        >
                          <Share2 size={16} />
                          Share
                        </button>
                      </>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Additional Info Section */}
        {certificates.length > 0 && (
          <div className="mt-12 bg-blue-50 rounded-xl border border-blue-200 p-6">
            <h3 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
              <Award size={24} className="text-blue-600" />
              About Your Certificates
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 text-sm text-gray-700">
              <div>
                <p className="font-semibold text-gray-900 mb-2">Validity</p>
                <p>
                  Your certificates are valid for lifetime and recognized by Motion Robotics
                </p>
              </div>
              <div>
                <p className="font-semibold text-gray-900 mb-2">Sharing</p>
                <p>
                  Download and share your certificates on social media or your resume
                </p>
              </div>
              <div>
                <p className="font-semibold text-gray-900 mb-2">Verification</p>
                <p>
                  Certificates can be verified using the certificate number
                </p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}