"use client";

import { useState } from "react";
import { ArrowRight, User, Lock, Loader, AlertCircle } from "lucide-react";
import { useRouter } from "next/navigation";

export default function AdminLogin() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const router = useRouter();

  async function handleLogin() {
    setError("");
    if (!email.trim() || !password.trim()) {
      setError("Please enter both email and password");
      return;
    }

    setIsLoading(true);
    try {
      // ? CORRECT API endpoint
      const res = await fetch("http://localhost:5000/api/admin/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password })
      });

      if (!res.ok) {
        const errorData = await res.json().catch(() => ({}));
        setError(errorData.message || "Invalid email or password");
        setIsLoading(false);
        return;
      }

      const data = await res.json();
      console.log("Login response:", data); // Debug log

      if (data.token) {
        // ? Store token and admin info in localStorage
        localStorage.setItem('admin_token', data.token);
        localStorage.setItem('admin_email', data.adminEmail || email);
        localStorage.setItem('admin_roles', JSON.stringify(data.roles || ['Admin']));
        localStorage.setItem('role', 'admin'); // For compatibility with getRole()
        
        console.log("Token stored:", localStorage.getItem('admin_token')); // Debug log
        
        // ? Wait for storage to complete
        await new Promise(resolve => setTimeout(resolve, 200));
        
        // ? Use window.location for full page reload
        window.location.href = '/admin/dashboard';
      } else {
        setError("Login failed. No token received.");
        setIsLoading(false);
      }
    } catch (err) {
      console.error("Login error:", err);
      setError("Connection error. Make sure backend is running on http://localhost:5000");
      setIsLoading(false);
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-6">
          <div className="flex justify-center mb-4">
            <div className="bg-gradient-to-br from-blue-600 to-indigo-600 p-4 rounded-full">
              <User className="w-8 h-8 text-white" />
            </div>
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-1">Admin Console</h1>
          <p className="text-gray-600 text-sm">Manage the LMS and site settings</p>
        </div>

        <div className="bg-white rounded-2xl shadow p-6">
          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded flex items-start gap-2">
              <AlertCircle className="w-5 h-5 text-red-600" />
              <p className="text-sm text-red-700">{error}</p>
            </div>
          )}

          <div className="mb-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
            <div className="relative">
              <User className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
              <input
                type="email"
                placeholder="admin@motionrobotics.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleLogin()}
                disabled={isLoading}
                className="w-full pl-10 pr-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">Password</label>
            <div className="relative">
              <Lock className="absolute left-3 top-3 w-4 h-4 text-gray-400" />
              <input
                type="password"
                placeholder="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleLogin()}
                disabled={isLoading}
                className="w-full pl-10 pr-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          <button
            onClick={handleLogin}
            disabled={isLoading}
            className="w-full bg-gradient-to-r from-blue-600 to-indigo-600 text-white py-2 rounded hover:from-blue-700 hover:to-indigo-700 flex items-center justify-center gap-2 disabled:opacity-60 transition-all"
          >
            {isLoading ? (
              <>
                <Loader className="w-4 h-4 animate-spin" />
                Signing in...
              </>
            ) : (
              <>
                Sign in
                <ArrowRight className="w-4 h-4" />
              </>
            )}
          </button>

          {/* Debug info */}
          <div className="mt-4 p-3 bg-gray-50 rounded text-xs text-gray-600">
            <p className="font-medium mb-1">Default credentials:</p>
            <p>Email: admin@motionrobotics.com</p>
            <p>Password: Admin@123</p>
          </div>
        </div>

        <p className="text-center text-sm text-gray-500 mt-4">Only authorized administrators may access this area.</p>
      </div>
    </div>
  );
}
