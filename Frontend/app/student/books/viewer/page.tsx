"use client";

import { useSearchParams } from "next/navigation";
import { useState, useEffect, Suspense } from "react";
import { ArrowLeft, ZoomIn, ZoomOut, Maximize2, BookOpen } from "lucide-react";

function PDFViewerContent() {
  const searchParams = useSearchParams();
  const pdfUrl = searchParams.get("url") || "";
  const [zoom, setZoom] = useState(100);
  const [isFullscreen, setIsFullscreen] = useState(false);

  // Disable right-click and keyboard shortcuts to prevent downloading
  useEffect(() => {
    const handleContextMenu = (e: MouseEvent) => {
      e.preventDefault();
    };
    
    const handleKeyDown = (e: KeyboardEvent) => {
      // Prevent Ctrl+S (Save), Ctrl+P (Print), Ctrl+A (Select All), F12 (DevTools)
      if (
        (e.ctrlKey || e.metaKey) && (e.key === 's' || e.key === 'p' || e.key === 'a') ||
        e.key === 'F12' ||
        (e.ctrlKey && e.shiftKey && e.key === 'I') || // Ctrl+Shift+I (DevTools)
        (e.ctrlKey && e.shiftKey && e.key === 'J') || // Ctrl+Shift+J (Console)
        (e.ctrlKey && e.key === 'u') // Ctrl+U (View Source)
      ) {
        e.preventDefault();
      }
    };

    const handleSelectStart = (e: Event) => {
      e.preventDefault();
    };

    // Add styles to hide PDF viewer controls
    const style = document.createElement('style');
    style.textContent = `
      iframe { 
        -webkit-user-select: none !important;
        -moz-user-select: none !important;
        user-select: none !important;
      }
    `;
    document.head.appendChild(style);

    document.addEventListener('contextmenu', handleContextMenu);
    document.addEventListener('keydown', handleKeyDown);
    document.addEventListener('selectstart', handleSelectStart);

    return () => {
      document.removeEventListener('contextmenu', handleContextMenu);
      document.removeEventListener('keydown', handleKeyDown);  
      document.removeEventListener('selectstart', handleSelectStart);
      document.head.removeChild(style);
    };
  }, []);

  const handleZoomIn = () => setZoom(prev => Math.min(prev + 25, 200));
  const handleZoomOut = () => setZoom(prev => Math.max(prev - 25, 50));

  const toggleFullscreen = () => {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
      setIsFullscreen(true);
    } else {
      document.exitFullscreen();
      setIsFullscreen(false);
    }
  };

  const handleGoBack = () => {
    window.close();
    // If window.close doesn't work (not opened by script), go back
    setTimeout(() => window.history.back(), 100);
  };

  if (!pdfUrl) {
    return (
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="text-center text-white">
          <BookOpen size={64} className="mx-auto mb-4 opacity-50" />
          <p className="text-xl">No book specified</p>
          <button 
            onClick={handleGoBack}
            className="mt-4 px-4 py-2 bg-indigo-600 rounded-lg hover:bg-indigo-700"
          >
            Go Back
          </button>
        </div>
      </div>
    );
  }

  // Use a more robust PDF viewing approach
  const decodedUrl = decodeURIComponent(pdfUrl);

  return (
    <div className="min-h-screen bg-gray-900 flex flex-col">
      {/* Header */}
      <header className="bg-gray-800 border-b border-gray-700 h-14 flex items-center justify-between px-4 flex-shrink-0">
        <button 
          onClick={handleGoBack}
          className="flex items-center gap-2 px-3 py-1.5 text-gray-300 hover:text-white hover:bg-gray-700 rounded-lg transition-colors"
        >
          <ArrowLeft size={18} />
          <span className="hidden sm:inline">Close Viewer</span>
        </button>

        <div className="flex items-center gap-2">
          <span className="text-gray-400 text-sm">Digital Book Viewer</span>
        </div>

        <div className="text-gray-400 text-sm hidden md:block">
          📖 View Only - No Download
        </div>
      </header>

      {/* PDF Viewer */}
      <div className="flex-1 bg-gray-100 flex items-center justify-center overflow-hidden">
        <div className="w-full h-full">
          {/* Embed PDF with download disabled */}
          <embed
            src={`${decodedUrl}#toolbar=0&navpanes=0&scrollbar=0&view=FitH&pagemode=none`}
            type="application/pdf"
            className="w-full border-0"
            style={{ 
              height: 'calc(100vh - 112px)',
            }}
          />
        </div>
      </div>

      {/* Footer Notice */}
      <div className="bg-gray-800 border-t border-gray-700 py-2 px-4 text-center flex-shrink-0">
        <p className="text-gray-500 text-xs">
          🔒 This content is protected. Downloading, printing, and copying are disabled.
        </p>
      </div>
    </div>
  );
}

export default function PDFViewerPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen bg-gray-900 flex items-center justify-center">
        <div className="text-white">Loading viewer...</div>
      </div>
    }>
      <PDFViewerContent />
    </Suspense>
  );
}
