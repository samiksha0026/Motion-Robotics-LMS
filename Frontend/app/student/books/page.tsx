"use client";

import { useState, useEffect } from "react";
import { 
  ArrowLeft, BookOpen, Eye, FileText, 
  ZoomIn, ZoomOut, ChevronLeft, ChevronRight,
  Maximize2, Lock, Loader
} from "lucide-react";

import { API_BASE_URL } from '@/lib/api-config';

interface BookData {
  id: number;
  title: string;
  level: string;
  description: string;
  totalPages: number;
  fileSize: string;
  uploadedDate: string;
  pdfUrl: string;
  coverImage: string | null;
  author: string;
  version: string;
}

export default function DigitalBookPage() {
  const [currentPage, setCurrentPage] = useState(1);
  const [zoom, setZoom] = useState(100);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [book, setBook] = useState<BookData | null>(null);

  // Fetch student's level and get the actual syllabus URL
  useEffect(() => {
    const fetchStudentBook = async () => {
      try {
        setLoading(true);
        const token = sessionStorage.getItem('jwt');
        
        if (!token) {
          throw new Error('Please login first');
        }

        // Fetch student's experiments/level data which includes syllabusUrl
        const sessionId = sessionStorage.getItem('sessionId');
        const response = await fetch(`${API_BASE_URL}/api/student/experiments`, {
          credentials: 'include',
          headers: {
            'Authorization': `Bearer ${token}`,
            ...(sessionId ? { 'X-Session-Id': sessionId } : {}),
            'Content-Type': 'application/json'
          }
        });

        if (!response.ok) {
          throw new Error('Failed to fetch student data');
        }

        const data = await response.json();
        
        if (!data.level || !data.level.syllabusUrl) {
          throw new Error('No digital book assigned to your level. Please contact your teacher.');
        }

        // Create book data from the actual level information
        const bookData: BookData = {
          id: data.level.levelId,
          title: `${data.level.levelName} - Digital Book`,
          level: data.level.levelName,
          description: data.level.description || `Complete guide for ${data.level.levelName} level`,
          totalPages: getEstimatedPages(data.level.levelName),
          fileSize: getEstimatedFileSize(data.level.levelName),
          uploadedDate: "2025-01-15",
          pdfUrl: `${API_BASE_URL}${data.level.syllabusUrl}`, // Use the actual syllabus URL
          coverImage: null,
          author: getAuthorFromLevel(data.level.levelName),
          version: "2025 Edition"
        };

        setBook(bookData);

      } catch (err) {
        console.error('Error fetching student book:', err);
        setError(err instanceof Error ? err.message : 'Failed to load book');
      } finally {
        setLoading(false);
      }
    };

    fetchStudentBook();
  }, []);

  // Helper functions for book metadata based on level
  const getEstimatedPages = (levelName: string): number => {
    const pageMap: Record<string, number> = {
      "Mech-Tech": 180,
      "Electronics": 200, 
      "Electro-Mechanical": 160,
      "Digi-Coding": 150,
      "Digi-Sense": 140,
      "Wireless & IOT": 175,
    };
    
    for (const [key, pages] of Object.entries(pageMap)) {
      if (levelName.toLowerCase().includes(key.toLowerCase()) || 
          key.toLowerCase().includes(levelName.toLowerCase())) {
        return pages;
      }
    }
    return 150; // Default
  };

  const getEstimatedFileSize = (levelName: string): string => {
    const sizeMap: Record<string, string> = {
      "Mech-Tech": "15.2 MB",
      "Electronics": "18.0 MB", 
      "Electro-Mechanical": "14.8 MB",
      "Digi-Coding": "12.5 MB",
      "Digi-Sense": "11.5 MB",
      "Wireless & IOT": "16.5 MB",
    };
    
    for (const [key, size] of Object.entries(sizeMap)) {
      if (levelName.toLowerCase().includes(key.toLowerCase()) || 
          key.toLowerCase().includes(levelName.toLowerCase())) {
        return size;
      }
    }
    return "12.0 MB"; // Default
  };

  const getAuthorFromLevel = (levelName: string): string => {
    const authorMap: Record<string, string> = {
      "Mech-Tech": "Samiksha Thorat",
      "Electronics": "Swara Thorat", 
      "Electro-Mechanical": "Swaranjali",
      "Digi-Coding": "Swaranjali",
      "Digi-Sense": "Swara",
      "Wireless & IOT": "Swaranjali",
    };
    
    for (const [key, author] of Object.entries(authorMap)) {
      if (levelName.toLowerCase().includes(key.toLowerCase()) || 
          key.toLowerCase().includes(levelName.toLowerCase())) {
        return author;
      }
    }
    return "Robotics Education Team"; // Default
  };

  const handleViewOnline = () => {
    if (!book) return;
    // Open PDF in embedded viewer (no download option)
    window.open(`/student/books/viewer?url=${encodeURIComponent(book.pdfUrl)}`, '_blank');
  };

  const handleGoBack = () => {
    window.history.back();
  };

  // Loading state
  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader className="w-8 h-8 animate-spin text-indigo-600 mx-auto mb-4" />
          <p className="text-gray-600">Loading your digital book...</p>
        </div>
      </div>
    );
  }

  // Error state  
  if (error || !book) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="max-w-md mx-auto text-center p-8">
          <BookOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h2 className="text-xl font-semibold text-gray-900 mb-3">Unable to Load Book</h2>
          <p className="text-gray-600 mb-6">
            {error || "We couldn't find your assigned digital book. Please contact your teacher."}
          </p>
          <button 
            onClick={handleGoBack}
            className="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Go Back
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
            onClick={handleGoBack}
            className="flex items-center gap-2 px-4 py-2 text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <ArrowLeft size={20} />
            <span className="font-medium hidden sm:inline">Back to Dashboard</span>
          </button>
          <div className="flex items-center gap-3">
            <span className="text-sm text-gray-600 hidden md:inline">{book.level}</span>
          </div>
        </div>
      </header>

      <div className="max-w-5xl mx-auto p-4 lg:p-8">
        {/* Book Header Card */}
        <div className="bg-gradient-to-r from-indigo-600 to-blue-600 rounded-xl p-8 text-white mb-6 shadow-lg">
          <div className="flex items-start gap-6">
            {/* Book Cover Placeholder */}
            <div className="hidden md:block bg-white/10 backdrop-blur-sm rounded-lg p-6 w-48 h-64 flex-shrink-0">
              <div className="w-full h-full flex flex-col items-center justify-center border-2 border-white/30 rounded">
                <BookOpen size={64} className="text-white/70 mb-4" />
                <p className="text-sm text-white/70 text-center">Digital Book</p>
              </div>
            </div>

            {/* Book Info */}
            <div className="flex-1">
              <div className="inline-flex items-center gap-2 bg-white/20 rounded-lg px-3 py-1 mb-3">
                <FileText size={16} />
                <span className="text-sm font-medium">{book.version}</span>
              </div>
              <h1 className="text-3xl font-bold mb-3">{book.title}</h1>
              <p className="text-indigo-100 mb-4">{book.description}</p>
              
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-6">
                <div>
                  <p className="text-indigo-200 text-sm mb-1">Total Pages</p>
                  <p className="font-semibold text-lg">{book.totalPages}</p>
                </div>
                <div>
                  <p className="text-indigo-200 text-sm mb-1">File Size</p>
                  <p className="font-semibold text-lg">{book.fileSize}</p>
                </div>
                <div>
                  <p className="text-indigo-200 text-sm mb-1">Published</p>
                  <p className="font-semibold text-lg">{book.uploadedDate}</p>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex flex-wrap gap-3">
                <button 
                  onClick={handleViewOnline}
                  className="flex items-center gap-2 px-6 py-3 bg-white text-indigo-600 rounded-lg hover:bg-indigo-50 font-semibold transition-colors shadow-md"
                >
                  <Eye size={20} />
                  Read Online
                </button>
                <div className="flex items-center gap-2 px-5 py-3 bg-white/10 backdrop-blur-sm text-white/70 rounded-lg border border-white/20">
                  <Lock size={18} />
                  <span className="text-sm">View Only - No Download</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Book Preview Section */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          <div className="bg-gray-50 border-b border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-bold text-gray-900">Book Preview</h2>
              <div className="flex items-center gap-2">
                <button className="p-2 hover:bg-gray-200 rounded-lg transition-colors" title="Zoom Out">
                  <ZoomOut size={20} className="text-gray-600" />
                </button>
                <span className="text-sm font-medium text-gray-700 px-3">{zoom}%</span>
                <button className="p-2 hover:bg-gray-200 rounded-lg transition-colors" title="Zoom In">
                  <ZoomIn size={20} className="text-gray-600" />
                </button>
                <div className="w-px h-6 bg-gray-300 mx-2"></div>
                <button className="p-2 hover:bg-gray-200 rounded-lg transition-colors" title="Fullscreen">
                  <Maximize2 size={20} className="text-gray-600" />
                </button>
              </div>
            </div>
          </div>

          {/* PDF Preview Area */}
          <div className="bg-gray-100 p-8 min-h-[600px] flex items-center justify-center">
            <div className="bg-white shadow-2xl rounded-lg p-12 max-w-2xl w-full">
              <div className="text-center">
                <div className="bg-indigo-100 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-6">
                  <BookOpen size={40} className="text-indigo-600" />
                </div>
                <h3 className="text-xl font-bold text-gray-900 mb-3">
                  {book.title}
                </h3>
                <p className="text-gray-600 mb-6">
                  Click "Read Online" to view the full book in your browser. Books are available for online reading only.
                </p>
                <div className="flex flex-col sm:flex-row gap-3 justify-center">
                  <button 
                    onClick={handleViewOnline}
                    className="flex items-center justify-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-medium transition-colors"
                  >
                    <Eye size={20} />
                    Read Book
                  </button>
                </div>
                
                <div className="mt-8 pt-6 border-t border-gray-200">
                  <p className="text-sm text-gray-500 mb-2">Quick Info</p>
                  <div className="flex justify-center gap-6 text-sm">
                    <span className="text-gray-700"><strong>{book.totalPages}</strong> pages</span>
                    <span className="text-gray-300">•</span>
                    <span className="text-gray-700"><strong>{book.fileSize}</strong> size</span>
                    <span className="text-gray-300">•</span>
                    <span className="text-gray-700"><strong>PDF</strong> format</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Page Navigation (Optional - if you implement page-by-page viewing) */}
          <div className="bg-gray-50 border-t border-gray-200 p-4">
            <div className="flex items-center justify-center gap-4">
              <button 
                disabled
                className="flex items-center gap-2 px-4 py-2 bg-gray-200 text-gray-400 rounded-lg cursor-not-allowed"
              >
                <ChevronLeft size={20} />
                Previous
              </button>
              <span className="text-sm text-gray-600">
                Page <input 
                  type="number" 
                  value={currentPage} 
                  min="1" 
                  max={book.totalPages}
                  className="w-16 px-2 py-1 border border-gray-300 rounded text-center mx-2"
                  readOnly
                /> of {book.totalPages}
              </span>
              <button 
                disabled
                className="flex items-center gap-2 px-4 py-2 bg-gray-200 text-gray-400 rounded-lg cursor-not-allowed"
              >
                Next
                <ChevronRight size={20} />
              </button>
            </div>
          </div>
        </div>

        {/* Additional Information */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
          {/* About This Book */}
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-lg font-bold text-gray-900 mb-4">About This Book</h3>
            <div className="space-y-3 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-600">Author:</span>
                <span className="font-medium text-gray-900">{book.author}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Edition:</span>
                <span className="font-medium text-gray-900">{book.version}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Level:</span>
                <span className="font-medium text-gray-900">{book.level}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Format:</span>
                <span className="font-medium text-gray-900">PDF</span>
              </div>
            </div>
          </div>

          {/* How to Use */}
          <div className="bg-blue-50 rounded-xl border border-blue-200 p-6">
            <h3 className="text-lg font-bold text-gray-900 mb-4">How to Use This Book</h3>
            <ul className="space-y-2 text-sm text-gray-700">
              <li className="flex items-start gap-2">
                <span className="text-blue-600 mt-1">1.</span>
                <span>Click "Read Online" to open the book in viewer</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-blue-600 mt-1">2.</span>
                <span>Use this book alongside video experiments</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-blue-600 mt-1">3.</span>
                <span>Refer to specific chapters for detailed concepts</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-blue-600 mt-1">4.</span>
                <span>Practice exercises at the end of each chapter</span>
              </li>
            </ul>
            <div className="mt-4 pt-4 border-t border-blue-200">
              <p className="text-xs text-blue-600 flex items-center gap-2">
                <Lock size={14} />
                Books are for online viewing only. Downloads are not available.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}