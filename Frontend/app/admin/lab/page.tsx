"use client";

import { useState, useEffect, useRef } from "react";
import {
  Camera, Upload, Trash2, Edit3, Save, X,
  Building2, Ruler, Users, ImageOff, ChevronRight,
  CheckCircle, AlertCircle, Loader2, Plus, ArrowLeft
} from "lucide-react";
import { API_BASE_URL, fetchWithAuth, getSchoolContext, isSuperAdmin } from "@/lib/auth";

interface LabPhoto {
  id: number;
  url: string;
  caption: string | null;
  displayOrder: number;
  uploadedAt: string;
}

interface LabInfo {
  schoolId: number;
  schoolName: string;
  labDescription: string | null;
  labArea: string | null;
  labCapacity: number | null;
  photos: LabPhoto[];
}

interface SchoolSummary {
  schoolId: number;
  schoolName: string;
  schoolCode: string;
  photoCount: number;
  hasLabInfo: boolean;
}

export default function LabPage() {
  const [superAdmin, setSuperAdmin] = useState(false);
  const [schools, setSchools] = useState<SchoolSummary[]>([]);
  const [selectedSchoolId, setSelectedSchoolId] = useState<number | null>(null);
  const [labInfo, setLabInfo] = useState<LabInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [savingInfo, setSavingInfo] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [editMode, setEditMode] = useState(false);
  const [editForm, setEditForm] = useState({ labDescription: "", labArea: "", labCapacity: "" });
  const [uploadCaption, setUploadCaption] = useState("");
  const [successMsg, setSuccessMsg] = useState("");
  const [errorMsg, setErrorMsg] = useState("");
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    const isSuper = isSuperAdmin();
    setSuperAdmin(isSuper);

    if (isSuper) {
      fetchSchoolsSummary();
    } else {
      const school = getSchoolContext();
      if (school) {
        setSelectedSchoolId(school.id);
        fetchLabInfo(school.id);
      } else {
        setLoading(false);
      }
    }
  }, []);

  useEffect(() => {
    if (selectedSchoolId !== null) {
      fetchLabInfo(selectedSchoolId);
    }
  }, [selectedSchoolId]);

  async function fetchSchoolsSummary() {
    try {
      const res = await fetchWithAuth(`${API_BASE_URL}/api/admin/lab/schools`);
      if (res.ok) setSchools(await res.json());
    } catch { /* ignore */ }
    setLoading(false);
  }

  async function fetchLabInfo(schoolId: number) {
    setLoading(true);
    try {
      const res = await fetchWithAuth(`${API_BASE_URL}/api/admin/lab/${schoolId}`);
      if (res.ok) {
        const data: LabInfo = await res.json();
        setLabInfo(data);
        setEditForm({
          labDescription: data.labDescription ?? "",
          labArea: data.labArea ?? "",
          labCapacity: data.labCapacity?.toString() ?? ""
        });
      } else {
        setLabInfo(null);
      }
    } catch { setLabInfo(null); }
    setLoading(false);
  }

  function flash(msg: string, isError = false) {
    if (isError) { setErrorMsg(msg); setTimeout(() => setErrorMsg(""), 4000); }
    else { setSuccessMsg(msg); setTimeout(() => setSuccessMsg(""), 3000); }
  }

  async function handleSaveInfo() {
    if (!selectedSchoolId) return;
    setSavingInfo(true);
    try {
      const res = await fetchWithAuth(`${API_BASE_URL}/api/admin/lab/${selectedSchoolId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          labDescription: editForm.labDescription || null,
          labArea: editForm.labArea || null,
          labCapacity: editForm.labCapacity ? parseInt(editForm.labCapacity) : null
        })
      });
      if (res.ok) {
        const updated: LabInfo = await res.json();
        setLabInfo(updated);
        setEditMode(false);
        flash("Lab information saved!");
        if (superAdmin) fetchSchoolsSummary();
      } else {
        flash("Failed to save lab information.", true);
      }
    } catch { flash("Network error.", true); }
    setSavingInfo(false);
  }

  async function handleUploadPhoto(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file || !selectedSchoolId) return;

    setUploading(true);
    const formData = new FormData();
    formData.append("file", file);
    if (uploadCaption.trim()) formData.append("caption", uploadCaption.trim());

    try {
      const res = await fetchWithAuth(`${API_BASE_URL}/api/admin/lab/${selectedSchoolId}/photos`, {
        method: "POST",
        body: formData
      });
      if (res.ok) {
        const newPhoto: LabPhoto = await res.json();
        setLabInfo(prev => prev ? { ...prev, photos: [...prev.photos, newPhoto] } : prev);
        setUploadCaption("");
        if (fileInputRef.current) fileInputRef.current.value = "";
        flash("Photo uploaded!");
        if (superAdmin) fetchSchoolsSummary();
      } else {
        const err = await res.json().catch(() => ({ message: "Upload failed." }));
        flash(err.message ?? "Upload failed.", true);
      }
    } catch { flash("Network error.", true); }
    setUploading(false);
  }

  async function handleDeletePhoto(photoId: number) {
    if (!selectedSchoolId) return;
    setDeletingId(photoId);
    try {
      const res = await fetchWithAuth(`${API_BASE_URL}/api/admin/lab/${selectedSchoolId}/photos/${photoId}`, { method: "DELETE" });
      if (res.ok) {
        setLabInfo(prev => prev ? { ...prev, photos: prev.photos.filter(p => p.id !== photoId) } : prev);
        flash("Photo deleted.");
        if (superAdmin) fetchSchoolsSummary();
      } else {
        flash("Failed to delete photo.", true);
      }
    } catch { flash("Network error.", true); }
    setDeletingId(null);
  }

  // ── Render: SuperAdmin school list ────────────────────────────────────────
  if (superAdmin && selectedSchoolId === null) {
    return (
      <div className="p-6 max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold text-gray-900 mb-2 flex items-center gap-2">
          <Camera className="w-7 h-7 text-indigo-600" /> Lab Gallery
        </h1>
        <p className="text-gray-500 mb-6">Select a school to view or manage its lab setup.</p>

        {loading ? (
          <div className="flex items-center gap-2 text-gray-500"><Loader2 className="w-5 h-5 animate-spin" /> Loading schools…</div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {schools.map(s => (
              <button key={s.schoolId} onClick={() => setSelectedSchoolId(s.schoolId)}
                className="bg-white border border-gray-200 rounded-xl p-4 text-left hover:border-indigo-400 hover:shadow-md transition-all group">
                <div className="flex items-start justify-between">
                  <div>
                    <p className="font-semibold text-gray-900 group-hover:text-indigo-600">{s.schoolName}</p>
                    <p className="text-xs text-gray-500 mt-0.5">{s.schoolCode}</p>
                  </div>
                  <ChevronRight className="w-5 h-5 text-gray-400 group-hover:text-indigo-500 mt-0.5" />
                </div>
                <div className="mt-3 flex items-center gap-3 text-sm">
                  <span className={`flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${
                    s.photoCount > 0 ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"
                  }`}>
                    <Camera className="w-3 h-3" /> {s.photoCount} photo{s.photoCount !== 1 ? "s" : ""}
                  </span>
                  {s.hasLabInfo && (
                    <span className="flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-700">
                      <CheckCircle className="w-3 h-3" /> Info added
                    </span>
                  )}
                </div>
              </button>
            ))}
          </div>
        )}
      </div>
    );
  }

  // ── Render: Lab detail view ───────────────────────────────────────────────
  return (
    <div className="p-6 max-w-5xl mx-auto">
      {/* Header */}
      <div className="flex items-center gap-3 mb-6">
        {superAdmin && (
          <button onClick={() => { setSelectedSchoolId(null); setLabInfo(null); }}
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-500 hover:text-gray-800 transition-colors">
            <ArrowLeft className="w-5 h-5" />
          </button>
        )}
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
            <Camera className="w-7 h-7 text-indigo-600" />
            {labInfo?.schoolName ?? "Lab Gallery"}
          </h1>
          <p className="text-sm text-gray-500">Lab setup, interior design & photos</p>
        </div>
      </div>

      {/* Flash messages */}
      {successMsg && (
        <div className="mb-4 flex items-center gap-2 bg-green-50 text-green-700 border border-green-200 rounded-lg px-4 py-3 text-sm">
          <CheckCircle className="w-4 h-4 shrink-0" /> {successMsg}
        </div>
      )}
      {errorMsg && (
        <div className="mb-4 flex items-center gap-2 bg-red-50 text-red-700 border border-red-200 rounded-lg px-4 py-3 text-sm">
          <AlertCircle className="w-4 h-4 shrink-0" /> {errorMsg}
        </div>
      )}

      {loading ? (
        <div className="flex items-center gap-2 text-gray-500 py-12 justify-center">
          <Loader2 className="w-5 h-5 animate-spin" /> Loading lab information…
        </div>
      ) : (
        <div className="space-y-6">
          {/* Lab Info Card */}
          <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
            <div className="flex items-center justify-between mb-4">
              <h2 className="font-semibold text-gray-900 text-lg">Lab Information</h2>
              {!superAdmin && !editMode && (
                <button onClick={() => setEditMode(true)}
                  className="flex items-center gap-1.5 text-sm text-indigo-600 hover:text-indigo-800 font-medium">
                  <Edit3 className="w-4 h-4" /> Edit
                </button>
              )}
            </div>

            {editMode && !superAdmin ? (
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Lab Description</label>
                  <textarea rows={3} value={editForm.labDescription}
                    onChange={e => setEditForm(p => ({ ...p, labDescription: e.target.value }))}
                    placeholder="Describe the lab setup, equipment, layout…"
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white text-gray-900 focus:ring-2 focus:ring-indigo-500 focus:border-transparent resize-none" />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Lab Area</label>
                    <input type="text" value={editForm.labArea}
                      onChange={e => setEditForm(p => ({ ...p, labArea: e.target.value }))}
                      placeholder="e.g. 1200 sq ft"
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white text-gray-900 focus:ring-2 focus:ring-indigo-500 focus:border-transparent" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Workstation Capacity</label>
                    <input type="number" min={0} value={editForm.labCapacity}
                      onChange={e => setEditForm(p => ({ ...p, labCapacity: e.target.value }))}
                      placeholder="e.g. 20"
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white text-gray-900 focus:ring-2 focus:ring-indigo-500 focus:border-transparent" />
                  </div>
                </div>
                <div className="flex gap-2 pt-1">
                  <button onClick={handleSaveInfo} disabled={savingInfo}
                    className="flex items-center gap-1.5 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg disabled:opacity-50 transition-colors">
                    {savingInfo ? <Loader2 className="w-4 h-4 animate-spin" /> : <Save className="w-4 h-4" />}
                    Save
                  </button>
                  <button onClick={() => setEditMode(false)}
                    className="flex items-center gap-1.5 border border-gray-300 text-gray-700 text-sm font-medium px-4 py-2 rounded-lg hover:bg-gray-50 transition-colors">
                    <X className="w-4 h-4" /> Cancel
                  </button>
                </div>
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div className="sm:col-span-3">
                  <p className="text-xs text-gray-500 uppercase tracking-wide mb-1">Description</p>
                  <p className="text-gray-700 text-sm">
                    {labInfo?.labDescription ?? <span className="italic text-gray-400">Not set yet</span>}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase tracking-wide mb-1 flex items-center gap-1">
                    <Ruler className="w-3.5 h-3.5" /> Area
                  </p>
                  <p className="font-semibold text-gray-900">
                    {labInfo?.labArea ?? <span className="text-gray-400 font-normal italic text-sm">Not set</span>}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase tracking-wide mb-1 flex items-center gap-1">
                    <Users className="w-3.5 h-3.5" /> Workstations
                  </p>
                  <p className="font-semibold text-gray-900">
                    {labInfo?.labCapacity != null
                      ? `${labInfo.labCapacity} stations`
                      : <span className="text-gray-400 font-normal italic text-sm">Not set</span>}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-gray-500 uppercase tracking-wide mb-1 flex items-center gap-1">
                    <Camera className="w-3.5 h-3.5" /> Photos
                  </p>
                  <p className="font-semibold text-gray-900">{labInfo?.photos.length ?? 0}</p>
                </div>
              </div>
            )}
          </div>

          {/* Upload Panel — SchoolAdmin only */}
          {!superAdmin && (
            <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
              <h2 className="font-semibold text-gray-900 text-lg mb-4 flex items-center gap-2">
                <Upload className="w-5 h-5 text-indigo-500" /> Upload Photo
              </h2>
              <div className="flex flex-col sm:flex-row gap-3">
                <input type="text" value={uploadCaption}
                  onChange={e => setUploadCaption(e.target.value)}
                  placeholder="Optional caption (e.g. Robot assembly area)"
                  className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white text-gray-900 focus:ring-2 focus:ring-indigo-500 focus:border-transparent" />
                <button onClick={() => fileInputRef.current?.click()} disabled={uploading}
                  className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-5 py-2 rounded-lg disabled:opacity-50 transition-colors whitespace-nowrap">
                  {uploading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Plus className="w-4 h-4" />}
                  {uploading ? "Uploading…" : "Choose Photo"}
                </button>
                <input ref={fileInputRef} type="file" accept=".jpg,.jpeg,.png,.webp"
                  className="hidden" onChange={handleUploadPhoto} />
              </div>
              <p className="text-xs text-gray-400 mt-2">Accepted: JPG, PNG, WebP · Max 10 MB per photo</p>
            </div>
          )}

          {/* Photo Gallery */}
          <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
            <h2 className="font-semibold text-gray-900 text-lg mb-4 flex items-center gap-2">
              <Building2 className="w-5 h-5 text-indigo-500" /> Lab Photos
              <span className="ml-auto text-sm font-normal text-gray-500">{labInfo?.photos.length ?? 0} photo{(labInfo?.photos.length ?? 0) !== 1 ? "s" : ""}</span>
            </h2>

            {!labInfo?.photos.length ? (
              <div className="text-center py-16 text-gray-400">
                <ImageOff className="w-12 h-12 mx-auto mb-3 opacity-40" />
                <p className="font-medium">No photos yet</p>
                {!superAdmin && <p className="text-sm mt-1">Upload your first lab photo using the panel above.</p>}
              </div>
            ) : (
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                {labInfo.photos.map(photo => (
                  <div key={photo.id} className="group relative rounded-xl overflow-hidden bg-gray-100 aspect-square shadow-sm">
                    {/* eslint-disable-next-line @next/next/no-img-element */}
                    <img src={`${API_BASE_URL}${photo.url}`} alt={photo.caption ?? "Lab photo"}
                      className="w-full h-full object-cover" loading="lazy" />
                    {/* Overlay on hover */}
                    <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex flex-col justify-between p-2">
                      {!superAdmin && (
                        <button onClick={() => handleDeletePhoto(photo.id)} disabled={deletingId === photo.id}
                          className="self-end bg-red-500 hover:bg-red-600 text-white p-1.5 rounded-lg transition-colors">
                          {deletingId === photo.id
                            ? <Loader2 className="w-3.5 h-3.5 animate-spin" />
                            : <Trash2 className="w-3.5 h-3.5" />}
                        </button>
                      )}
                      {photo.caption && (
                        <p className="text-white text-xs leading-tight bg-black/40 rounded px-1.5 py-1">{photo.caption}</p>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
