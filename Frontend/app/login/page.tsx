"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { Eye, EyeOff, ArrowRight, Loader2 } from "lucide-react";
import { API_BASE_URL } from "@/lib/api-config";
import { ThemeToggle } from "@/components/ThemeToggle";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [checkingAuth, setCheckingAuth] = useState(true);

  // Always clear previous session so the user can log in fresh
  useEffect(() => {
    sessionStorage.removeItem("jwt");
    sessionStorage.removeItem("refreshToken");
    sessionStorage.removeItem("userRole");
    sessionStorage.removeItem("role");
    sessionStorage.removeItem("userName");
    sessionStorage.removeItem("admin_email");
    sessionStorage.removeItem("schoolContext");
    sessionStorage.removeItem("tokenExpiry");
    setCheckingAuth(false);
  }, []);

  function redirectByRole(role: string) {
    const roleRoutes: Record<string, string> = {
      SuperAdmin: "/admin/dashboard",
      SchoolAdmin: "/admin/dashboard",
      Admin: "/admin/dashboard",
      Teacher: "/teacher/dashboard",
      Student: "/student/dashboard",
    };
    const route = roleRoutes[role] || "/";
    window.location.href = route;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    if (!email.trim() || !password.trim()) {
      setError("Please enter both email and password");
      return;
    }

    setIsLoading(true);

    try {
      const res = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include", // Required: stores httpOnly session cookie from backend
        body: JSON.stringify({ email: email.trim(), password }),
      });

      const data = await res.json();

      if (res.ok && data.token) {
        // Store tokens in sessionStorage (per-tab, cleared on browser close)
        // The httpOnly 'sessionId' cookie is set automatically by the backend
        sessionStorage.setItem("jwt", data.token);
        if (data.refreshToken) {
          sessionStorage.setItem("refreshToken", data.refreshToken);
        }

        // Get primary role
        const role = data.role || (data.roles && data.roles[0]) || "Student";
        sessionStorage.setItem("userRole", role);

        // Store user name/email for display
        const displayName = data.name || data.user?.userName || data.user?.email || email.trim();
        sessionStorage.setItem("userName", displayName);
        sessionStorage.setItem("admin_email", data.user?.email || email.trim());

        // Store token expiry if available
        if (data.expires) {
          sessionStorage.setItem("tokenExpiry", data.expires);
        }

        // Set role flag for layout auth checks
        if (role === "SuperAdmin" || role === "SchoolAdmin" || role === "Admin") {
          sessionStorage.setItem("role", "admin");
        } else if (role === "Teacher") {
          sessionStorage.setItem("role", "teacher");
        } else if (role === "Student") {
          sessionStorage.setItem("role", "student");
        }

        // Store school context for SchoolAdmin (data comes from data.data.school)
        if (role === "SchoolAdmin" && data.data?.school) {
          const school = data.data.school;
          sessionStorage.setItem("schoolContext", JSON.stringify({
            id: school.id,
            schoolName: school.schoolName || "",
            schoolCode: school.schoolCode || "",
            logoUrl: school.logoUrl || "",
          }));
        }

        // Redirect based on role
        redirectByRole(role);
      } else {
        setError(data.message || "Invalid email or password");
      }
    } catch (err) {
      console.error("Login error:", err);
      setError("Connection error. Please check if the server is running.");
    } finally {
      setIsLoading(false);
    }
  }

  // Show loading while checking authentication status
  if (checkingAuth) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <Loader2 className="w-10 h-10 text-blue-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600">Checking authentication...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-8 relative transition-colors duration-300"
      style={{ background: 'var(--background)' }}>
      {/* Theme Toggle */}
      <div className="absolute top-4 right-4 z-10">
        <ThemeToggle />
      </div>
      {/* Compact Login Card with Video */}
      <div className="w-full max-w-4xl rounded-2xl shadow-xl overflow-hidden flex transition-colors duration-300"
        style={{ backgroundColor: 'var(--surface)', boxShadow: 'var(--shadow-lg)' }}>
        {/* Left side - Login Form */}
        <div className="w-full lg:w-1/2 p-8">
          {/* Logo */}
          <Link href="/" className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center shadow-lg shadow-blue-500/30">
              <svg viewBox="0 0 24 24" className="w-5 h-5 text-white" fill="currentColor">
                <path d="M12 2a2 2 0 012 2c0 .74-.4 1.39-1 1.73V7h1a7 7 0 017 7h1a1 1 0 011 1v3a1 1 0 01-1 1h-1v1a2 2 0 01-2 2H5a2 2 0 01-2-2v-1H2a1 1 0 01-1-1v-3a1 1 0 011-1h1a7 7 0 017-7h1V5.73c-.6-.34-1-.99-1-1.73a2 2 0 012-2zm-3 12a1 1 0 100 2 1 1 0 000-2zm6 0a1 1 0 100 2 1 1 0 000-2z" />
              </svg>
            </div>
            <span className="text-lg font-bold text-gray-800">Motion Robotics</span>
          </Link>

          {/* Welcome text */}
          <div className="mb-5">
            <h1 className="text-2xl font-bold text-gray-900 mb-1">Welcome back!</h1>
            <p className="text-gray-500 text-sm">Sign in to continue your robotics journey</p>
          </div>

          {/* Error message */}
          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
              <div className="w-5 h-5 rounded-full bg-red-100 flex items-center justify-center flex-shrink-0 mt-0.5">
                <span className="text-red-600 text-xs font-bold">!</span>
              </div>
              <p className="text-red-700 text-sm">{error}</p>
            </div>
          )}

          {/* Login Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Email field */}
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1.5">
                Email Address
              </label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Enter your email"
                className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
                disabled={isLoading}
              />
            </div>

            {/* Password field */}
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1.5">
                Password
              </label>
              <div className="relative">
                <input
                  type={showPassword ? "text" : "password"}
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter your password"
                  className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-lg text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200 pr-11"
                  disabled={isLoading}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            {/* Remember me & Forgot password */}
            <div className="flex items-center justify-between">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span className="text-sm text-gray-600">Remember me</span>
              </label>
              <Link href="/forgot-password" className="text-sm text-blue-600 hover:text-blue-700 font-medium transition-colors">
                Forgot password?
              </Link>
            </div>

            {/* Submit button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 text-white font-semibold rounded-lg hover:from-blue-700 hover:to-indigo-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-all duration-200 flex items-center justify-center gap-2 shadow-lg shadow-blue-500/25 disabled:opacity-70 disabled:cursor-not-allowed"
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-5 h-5 animate-spin" />
                  Signing in...
                </>
              ) : (
                <>
                  Sign In
                  <ArrowRight className="w-5 h-5" />
                </>
              )}
            </button>
          </form>

          {/* Footer */}
          <p className="mt-5 text-center text-sm text-gray-500">
            Don&apos;t have an account?{" "}
            <Link href="/contact" className="text-blue-600 hover:text-blue-700 font-medium">
              Contact us
            </Link>
          </p>
        </div>

        {/* Right side - Robot Video */}
        <div className="hidden lg:block w-1/2 bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 relative">
          <video
            autoPlay
            loop
            muted
            playsInline
            className="w-full h-full object-cover"
          >
            <source src="/videos/robot.mp4" type="video/mp4" />
          </video>
          {/* Overlay */}
          <div className="absolute inset-0 bg-gradient-to-t from-slate-900/60 via-transparent to-slate-900/30" />
        </div>
      </div>
    </div>
  );
}
