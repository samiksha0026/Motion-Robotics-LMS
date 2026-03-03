"use client";

import React, { useEffect, useState } from 'react';
import { fetchWithAuth } from '../../../lib/auth';
import { API_BASE_URL } from '@/lib/api-config';
import { 
  School, 
  Plus, 
  Loader, 
  Search, 
  Edit, 
  Trash2, 
  Users, 
  BookOpen, 
  AlertCircle, 
  Phone, 
  Mail, 
  MapPin,
  User,
  X,
  Key,
  Copy,
  Check,
  Upload,
  GraduationCap
} from 'lucide-react';

interface SchoolData {
  id: number;
  schoolName: string;
  schoolCode: string;
  address: string;
  city: string;
  state: string;
  pincode: string;
  contactEmail: string;
  contactPhone: string;
  logoUrl?: string;
  principalName?: string;
  loginUsername?: string;
  studentCount: number;
  classCount: number;
  teacherCount: number;
}

interface NewSchoolCredentials {
  username: string;
  password: string;
}

export default function AdminSchools() {
  const [schools, setSchools] = useState<SchoolData[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingSchool, setEditingSchool] = useState<SchoolData | null>(null);
  const [newCredentials, setNewCredentials] = useState<NewSchoolCredentials | null>(null);
  const [copiedField, setCopiedField] = useState<string | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [uploadingLogo, setUploadingLogo] = useState(false);
  
  // Form fields
  const [formData, setFormData] = useState({
    schoolName: '',
    schoolCode: '',
    address: '',
    city: '',
    state: '',
    pincode: '',
    contactEmail: '',
    contactPhone: '',
    principalName: ''
  });

  async function loadSchools() {
    try {
      setLoading(true);
      const data = await fetchWithAuth('/api/admin/schools');
      const schoolsArray = Array.isArray(data) ? data : [];
      setSchools(schoolsArray);
      setError('');
    } catch (err) {
      console.error(err);
      setError('Failed to load schools');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadSchools();
  }, []);

  function resetForm() {
    setFormData({
      schoolName: '',
      schoolCode: '',
      address: '',
      city: '',
      state: '',
      pincode: '',
      contactEmail: '',
      contactPhone: '',
      principalName: ''
    });
    setShowForm(false);
    setEditingSchool(null);
    setLogoFile(null);
    setLogoPreview(null);
  }

  function handleLogoChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith('image/')) {
        setError('Please select an image file');
        return;
      }
      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        setError('Image must be less than 5MB');
        return;
      }
      setLogoFile(file);
      setLogoPreview(URL.createObjectURL(file));
    }
  }

  async function uploadLogo(schoolId: number) {
    if (!logoFile) return;
    
    setUploadingLogo(true);
    try {
      const formData = new FormData();
      formData.append('file', logoFile);
      
      const token = sessionStorage.getItem('jwt');
      const res = await fetch(`${API_BASE_URL}/api/admin/schools/${schoolId}/logo`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      });
      
      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || 'Failed to upload logo');
      }
      
      // Handle logo upload response
      const contentType = res.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        const text = await res.text();
        if (text.trim() === '') {
          return {}; // Return empty object for empty JSON responses
        }
        try {
          return JSON.parse(text);
        } catch (e) {
          throw new Error(`Invalid JSON response: ${text}`);
        }
      } else {
        return {}; // Return empty object for non-JSON responses
      }
    } finally {
      setUploadingLogo(false);
    }
  }

  function openEditForm(school: SchoolData) {
    setFormData({
      schoolName: school.schoolName,
      schoolCode: school.schoolCode,
      address: school.address,
      city: school.city || '',
      state: school.state || '',
      pincode: school.pincode || '',
      contactEmail: school.contactEmail,
      contactPhone: school.contactPhone,
      principalName: school.principalName || ''
    });
    setEditingSchool(school);
    setShowForm(true);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    
    if (!formData.schoolName.trim() || !formData.schoolCode.trim() || 
        !formData.address.trim() || !formData.city.trim() ||
        !formData.state.trim() || !formData.pincode.trim() ||
        !formData.contactEmail.trim() || !formData.contactPhone.trim()) {
      setError('Please fill in all required fields');
      return;
    }

    setSaving(true);
    setError('');
    setSuccess('');

    try {
      let schoolId: number;
      
      if (editingSchool) {
        // Update existing school
        await fetchWithAuth(`/api/admin/schools/${editingSchool.id}`, {
          method: 'PUT',
          body: JSON.stringify(formData)
        });
        schoolId = editingSchool.id;
        
        // Upload logo if selected
        if (logoFile) {
          await uploadLogo(schoolId);
        }
        setSuccess('School updated successfully');
      } else {
        // Create new school
        const result = await fetchWithAuth('/api/admin/schools', {
          method: 'POST',
          body: JSON.stringify(formData)
        });
        schoolId = result.id;
        
        // Upload logo if selected
        if (logoFile && schoolId) {
          await uploadLogo(schoolId);
        }
        
        // Show the generated credentials
        if (result.loginUsername && result.loginPassword) {
          setNewCredentials({
            username: result.loginUsername,
            password: result.loginPassword
          });
        }
        setSuccess('School created successfully');
      }
      
      resetForm();
      loadSchools();
    } catch (err: any) {
      console.error(err);
      setError(err.message || 'Failed to save school');
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id: number, name: string) {
    if (!confirm(`Are you sure you want to delete "${name}"? This action cannot be undone.`)) {
      return;
    }

    try {
      await fetchWithAuth(`/api/admin/schools/${id}`, { method: 'DELETE' });
      setSuccess('School deleted successfully');
      loadSchools();
    } catch (err) {
      console.error(err);
      setError('Failed to delete school');
    }
  }

  function copyToClipboard(text: string, field: string) {
    navigator.clipboard.writeText(text);
    setCopiedField(field);
    setTimeout(() => setCopiedField(null), 2000);
  }

  const filteredSchools = schools.filter(s => 
    s.schoolName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    s.schoolCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
    s.contactEmail.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Schools</h1>
          <p className="text-gray-600">Manage registered schools and institutions</p>
        </div>
        <button
          onClick={() => { resetForm(); setShowForm(true); }}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <Plus className="w-5 h-5" />
          Add School
        </button>
      </div>

      {/* Messages */}
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-2 text-red-700">
          <AlertCircle className="w-5 h-5" />
          {error}
          <button onClick={() => setError('')} className="ml-auto">
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {success && (
        <div className="p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-2 text-green-700">
          <Check className="w-5 h-5" />
          {success}
          <button onClick={() => setSuccess('')} className="ml-auto">
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* New School Credentials Modal */}
      {newCredentials && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full p-6">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                <Key className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">School Login Created</h3>
                <p className="text-sm text-gray-600">Copy and share these credentials securely</p>
              </div>
            </div>
            
            <div className="space-y-3 mb-6">
              <div className="bg-gray-50 rounded-lg p-3">
                <label className="text-xs font-medium text-gray-500">Username</label>
                <div className="flex items-center justify-between">
                  <span className="font-mono text-gray-900">{newCredentials.username}</span>
                  <button 
                    onClick={() => copyToClipboard(newCredentials.username, 'username')}
                    className="p-1 hover:bg-gray-200 rounded"
                  >
                    {copiedField === 'username' ? <Check className="w-4 h-4 text-green-600" /> : <Copy className="w-4 h-4 text-gray-400" />}
                  </button>
                </div>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <label className="text-xs font-medium text-gray-500">Password</label>
                <div className="flex items-center justify-between">
                  <span className="font-mono text-gray-900">{newCredentials.password}</span>
                  <button 
                    onClick={() => copyToClipboard(newCredentials.password, 'password')}
                    className="p-1 hover:bg-gray-200 rounded"
                  >
                    {copiedField === 'password' ? <Check className="w-4 h-4 text-green-600" /> : <Copy className="w-4 h-4 text-gray-400" />}
                  </button>
                </div>
              </div>
            </div>

            <p className="text-xs text-amber-600 bg-amber-50 p-2 rounded mb-4">
              Save these credentials now. The password cannot be retrieved later.
            </p>

            <button
              onClick={() => setNewCredentials(null)}
              className="w-full py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Got it
            </button>
          </div>
        </div>
      )}

      {/* Add/Edit Form */}
      {showForm && (
        <div className="bg-white rounded-xl shadow-sm p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-lg font-semibold text-gray-900">
              {editingSchool ? 'Edit School' : 'Add New School'}
            </h2>
            <button onClick={resetForm} className="p-2 hover:bg-gray-100 rounded-lg">
              <X className="w-5 h-5 text-gray-500" />
            </button>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  School Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.schoolName}
                  onChange={(e) => setFormData({ ...formData, schoolName: e.target.value })}
                  placeholder="Enter school name"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  School Code <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.schoolCode}
                  onChange={(e) => setFormData({ ...formData, schoolCode: e.target.value.toUpperCase() })}
                  placeholder="e.g., SCH001"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                  disabled={!!editingSchool}
                />
                {!editingSchool && (
                  <p className="text-xs text-gray-500 mt-1">This will be used for login credentials</p>
                )}
              </div>

              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Address <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.address}
                  onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                  placeholder="Enter full address"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  City <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.city}
                  onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                  placeholder="Enter city"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  State <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.state}
                  onChange={(e) => setFormData({ ...formData, state: e.target.value })}
                  placeholder="Enter state"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Pincode <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.pincode}
                  onChange={(e) => setFormData({ ...formData, pincode: e.target.value })}
                  placeholder="Enter pincode"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                  maxLength={6}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Contact Email <span className="text-red-500">*</span>
                </label>
                <input
                  type="email"
                  value={formData.contactEmail}
                  onChange={(e) => setFormData({ ...formData, contactEmail: e.target.value })}
                  placeholder="school@example.com"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Contact Phone <span className="text-red-500">*</span>
                </label>
                <input
                  type="tel"
                  value={formData.contactPhone}
                  onChange={(e) => setFormData({ ...formData, contactPhone: e.target.value })}
                  placeholder="+91 XXXXX XXXXX"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Principal Name
                </label>
                <input
                  type="text"
                  value={formData.principalName}
                  onChange={(e) => setFormData({ ...formData, principalName: e.target.value })}
                  placeholder="Enter principal's name"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  School Logo
                </label>
                <div className="flex items-center gap-3">
                  {logoPreview || (editingSchool?.logoUrl) ? (
                    <div className="relative w-16 h-16">
                      <img 
                        src={logoPreview || `${API_BASE_URL}${editingSchool?.logoUrl}`} 
                        alt="Logo preview" 
                        className="w-16 h-16 object-cover rounded-lg border border-gray-200"
                      />
                      <button
                        type="button"
                        onClick={() => { setLogoFile(null); setLogoPreview(null); }}
                        className="absolute -top-2 -right-2 w-5 h-5 bg-red-500 text-white rounded-full flex items-center justify-center"
                      >
                        <X className="w-3 h-3" />
                      </button>
                    </div>
                  ) : (
                    <div className="w-16 h-16 bg-gray-100 rounded-lg border-2 border-dashed border-gray-300 flex items-center justify-center">
                      <School className="w-6 h-6 text-gray-400" />
                    </div>
                  )}
                  <div>
                    <label className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 text-gray-600 cursor-pointer">
                      <Upload className="w-4 h-4" />
                      {logoFile ? 'Change Logo' : 'Upload Logo'}
                      <input
                        type="file"
                        accept="image/*"
                        onChange={handleLogoChange}
                        className="hidden"
                      />
                    </label>
                    <p className="text-xs text-gray-500 mt-1">Max 5MB (jpg, png, gif)</p>
                  </div>
                </div>
              </div>
            </div>

            <div className="flex gap-3 pt-4">
              <button
                type="submit"
                disabled={saving || uploadingLogo}
                className="flex items-center gap-2 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
              >
                {(saving || uploadingLogo) ? <Loader className="w-4 h-4 animate-spin" /> : <Check className="w-4 h-4" />}
                {editingSchool ? 'Update School' : 'Create School'}
              </button>
              <button
                type="button"
                onClick={resetForm}
                className="px-6 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
        <input
          type="text"
          placeholder="Search schools..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      {/* Schools List */}
      {loading ? (
        <div className="flex items-center justify-center py-12">
          <Loader className="w-8 h-8 animate-spin text-blue-600" />
        </div>
      ) : filteredSchools.length === 0 ? (
        <div className="bg-white rounded-xl shadow-sm p-12 text-center">
          <School className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No schools found</h3>
          <p className="text-gray-600 mb-4">
            {searchTerm ? 'Try a different search term' : 'Get started by adding your first school'}
          </p>
          {!searchTerm && (
            <button
              onClick={() => { resetForm(); setShowForm(true); }}
              className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <Plus className="w-5 h-5" />
              Add School
            </button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {filteredSchools.map((school) => (
            <div key={school.id} className="bg-white rounded-xl shadow-sm p-6 hover:shadow-md transition-shadow">
              <div className="flex items-start justify-between mb-4">
                <div className="flex items-center gap-3">
                  {school.logoUrl ? (
                    <img 
                      src={`${API_BASE_URL}${school.logoUrl}`} 
                      alt={school.schoolName}
                      className="w-12 h-12 object-cover rounded-lg"
                    />
                  ) : (
                    <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                      <School className="w-6 h-6 text-blue-600" />
                    </div>
                  )}
                  <div>
                    <h3 className="font-semibold text-gray-900">{school.schoolName}</h3>
                    <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded">
                      {school.schoolCode}
                    </span>
                  </div>
                </div>
                <div className="flex gap-1">
                  <button
                    onClick={() => openEditForm(school)}
                    className="p-2 hover:bg-gray-100 rounded-lg text-gray-600"
                    title="Edit"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => handleDelete(school.id, school.schoolName)}
                    className="p-2 hover:bg-red-50 rounded-lg text-red-600"
                    title="Delete"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>

              <div className="space-y-2 text-sm text-gray-600 mb-4">
                <div className="flex items-center gap-2">
                  <MapPin className="w-4 h-4 text-gray-400" />
                  <span>{school.address}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Mail className="w-4 h-4 text-gray-400" />
                  <span>{school.contactEmail}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Phone className="w-4 h-4 text-gray-400" />
                  <span>{school.contactPhone}</span>
                </div>
                {school.principalName && (
                  <div className="flex items-center gap-2">
                    <User className="w-4 h-4 text-gray-400" />
                    <span>Principal: {school.principalName}</span>
                  </div>
                )}
                {school.loginUsername && (
                  <div className="flex items-center gap-2">
                    <Key className="w-4 h-4 text-gray-400" />
                    <span className="font-mono text-xs">{school.loginUsername}</span>
                  </div>
                )}
              </div>

              <div className="flex gap-4 pt-4 border-t border-gray-100">
                <div className="flex items-center gap-2">
                  <BookOpen className="w-4 h-4 text-blue-500" />
                  <span className="text-sm"><strong>{school.classCount}</strong> Classes</span>
                </div>
                <div className="flex items-center gap-2">
                  <GraduationCap className="w-4 h-4 text-green-500" />
                  <span className="text-sm"><strong>{school.teacherCount}</strong> Teachers</span>
                </div>
                <div className="flex items-center gap-2">
                  <Users className="w-4 h-4 text-purple-500" />
                  <span className="text-sm"><strong>{school.studentCount}</strong> Students</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
