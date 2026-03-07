"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import {
  Camera, ImageOff, Ruler, Users, Building2,
  Loader2, AlertCircle, ArrowLeft, ChevronLeft, ChevronRight
} from "lucide-react";
import { API_BASE_URL } from "@/lib/api-config";

interface LabPhoto {
  id: number;
  url: string;
  caption: string | null;
  displayOrder: number;
}

interface LabInfo {
  schoolId: number;
  schoolName: string;
  labDescription: string | null;
  labArea: string | null;
  labCapacity: number | null;
  photos: LabPhoto[];
}

export default function TeacherLabPage() {
  const router = useRouter();
  const [labInfo, setLabInfo] = useState<LabInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [lightboxIndex, setLightboxIndex] = useState<number | null>(null);

  useEffect(() => { fetchLab(); }, []);

  async function fetchLab() {
    const token = sessionStorage.getItem("jwt");
    const sessionId = sessionStorage.getItem("sessionId");
    if (!token) { router.push("/login"); return; }

    try {
      const res = await fetch(`${API_BASE_URL}/api/teacher/lab`, {
        credentials: "include",
        headers: {
          Authorization: `Bearer ${token}`,
          ...(sessionId ? { "X-Session-Id": sessionId } : {})
        }
      });
      if (res.status === 401) { router.push("/login"); return; }
      if (res.ok) setLabInfo(await res.json());
    } catch { setError("Failed to load lab information."); }
    setLoading(false);
  }

  const closeLightbox = () => setLightboxIndex(null);
  const prevPhoto = () => setLightboxIndex(i => (i != null && i > 0 ? i - 1 : i));
  const nextPhoto = () => setLightboxIndex(i => (i != null && labInfo && i < labInfo.photos.length - 1 ? i + 1 : i));

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Top bar */}
      <header className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 sticky top-0 z-10">
        <div className="max-w-5xl mx-auto px-4 py-3 flex items-center gap-3">
          <button onClick={() => router.back()} className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-500 transition-colors">
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div className="flex items-center gap-2">
            <Camera className="w-5 h-5 text-blue-600" />
            <h1 className="font-semibold text-gray-900 dark:text-white">Lab Gallery</h1>
          </div>
        </div>
      </header>

      <main className="max-w-5xl mx-auto px-4 py-6 space-y-6">
        {loading ? (
          <div className="flex items-center justify-center gap-2 text-gray-500 py-20">
            <Loader2 className="w-6 h-6 animate-spin" /> Loading lab info…
          </div>
        ) : error ? (
          <div className="flex items-center gap-2 bg-red-50 text-red-700 border border-red-200 rounded-xl px-4 py-3">
            <AlertCircle className="w-5 h-5 shrink-0" /> {error}
          </div>
        ) : !labInfo ? (
          <div className="text-center py-20 text-gray-400">
            <Camera className="w-14 h-14 mx-auto mb-3 opacity-30" />
            <p className="text-lg font-medium">Lab info coming soon</p>
          </div>
        ) : (
          <>
            <div>
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white">{labInfo.schoolName}</h2>
              <p className="text-sm text-gray-500 flex items-center gap-1 mt-0.5">
                <Building2 className="w-4 h-4" /> Robotics Lab
              </p>
            </div>

            {(labInfo.labDescription || labInfo.labArea || labInfo.labCapacity != null) && (
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                {labInfo.labDescription && (
                  <div className="sm:col-span-3 bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4">
                    <p className="text-xs text-gray-500 uppercase tracking-wide mb-1">About the Lab</p>
                    <p className="text-gray-700 dark:text-gray-300 text-sm leading-relaxed">{labInfo.labDescription}</p>
                  </div>
                )}
                {labInfo.labArea && (
                  <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4 flex items-center gap-3">
                    <div className="p-2 bg-blue-50 dark:bg-blue-900/30 rounded-lg">
                      <Ruler className="w-5 h-5 text-blue-600" />
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">Lab Area</p>
                      <p className="font-semibold text-gray-900 dark:text-white">{labInfo.labArea}</p>
                    </div>
                  </div>
                )}
                {labInfo.labCapacity != null && (
                  <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4 flex items-center gap-3">
                    <div className="p-2 bg-green-50 dark:bg-green-900/30 rounded-lg">
                      <Users className="w-5 h-5 text-green-600" />
                    </div>
                    <div>
                      <p className="text-xs text-gray-500">Workstations</p>
                      <p className="font-semibold text-gray-900 dark:text-white">{labInfo.labCapacity} stations</p>
                    </div>
                  </div>
                )}
              </div>
            )}

            <div>
              <h3 className="font-semibold text-gray-800 dark:text-white mb-3 flex items-center gap-2">
                <Camera className="w-5 h-5 text-blue-500" /> Photos
                <span className="text-sm font-normal text-gray-400 ml-1">({labInfo.photos.length})</span>
              </h3>

              {labInfo.photos.length === 0 ? (
                <div className="text-center py-14 bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 text-gray-400">
                  <ImageOff className="w-10 h-10 mx-auto mb-2 opacity-40" />
                  <p>Photos coming soon</p>
                </div>
              ) : (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
                  {labInfo.photos.map((photo, idx) => (
                    <button key={photo.id} onClick={() => setLightboxIndex(idx)}
                      className="relative aspect-square rounded-xl overflow-hidden bg-gray-100 dark:bg-gray-700 hover:ring-2 hover:ring-blue-400 transition-all group">
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img src={`${API_BASE_URL}${photo.url}`} alt={photo.caption ?? "Lab photo"}
                        className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" loading="lazy" />
                      {photo.caption && (
                        <div className="absolute bottom-0 inset-x-0 bg-gradient-to-t from-black/60 to-transparent px-2 py-2">
                          <p className="text-white text-xs truncate">{photo.caption}</p>
                        </div>
                      )}
                    </button>
                  ))}
                </div>
              )}
            </div>
          </>
        )}
      </main>

      {/* Lightbox */}
      {lightboxIndex !== null && labInfo && (
        <div className="fixed inset-0 z-50 bg-black/90 flex items-center justify-center p-4"
          onClick={closeLightbox}>
          <button className="absolute top-4 right-4 text-white/70 hover:text-white text-xl" onClick={closeLightbox}>✕</button>
          {lightboxIndex > 0 && (
            <button className="absolute left-4 text-white/70 hover:text-white p-2"
              onClick={e => { e.stopPropagation(); prevPhoto(); }}>
              <ChevronLeft className="w-8 h-8" />
            </button>
          )}
          {lightboxIndex < labInfo.photos.length - 1 && (
            <button className="absolute right-4 text-white/70 hover:text-white p-2"
              onClick={e => { e.stopPropagation(); nextPhoto(); }}>
              <ChevronRight className="w-8 h-8" />
            </button>
          )}
          <div className="max-w-3xl max-h-full" onClick={e => e.stopPropagation()}>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={`${API_BASE_URL}${labInfo.photos[lightboxIndex].url}`}
              alt={labInfo.photos[lightboxIndex].caption ?? "Lab photo"}
              className="max-w-full max-h-[80vh] object-contain rounded-lg" />
            {labInfo.photos[lightboxIndex].caption && (
              <p className="text-white text-center mt-3 text-sm">{labInfo.photos[lightboxIndex].caption}</p>
            )}
            <p className="text-white/40 text-center mt-1 text-xs">{lightboxIndex + 1} / {labInfo.photos.length}</p>
          </div>
        </div>
      )}
    </div>
  );
}
