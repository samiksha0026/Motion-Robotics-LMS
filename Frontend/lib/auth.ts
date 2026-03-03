// Client-side auth helpers — uses sessionStorage for tokens (per-tab, cleared on close)
// The backend also sets an httpOnly 'sessionId' cookie for server-side session tracking.

// API Base URL - must match backend
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5235';

export type SessionData = {
  token: string;
  refreshToken?: string;
  role: string;
  name?: string;
  expires?: string;
  refreshTokenExpires?: string;
  [key: string]: any;
};

// Token storage keys
const TOKEN_KEY = 'jwt';
const REFRESH_TOKEN_KEY = 'refreshToken';
const TOKEN_EXPIRY_KEY = 'tokenExpiry';

function getStorage(): Storage | null {
  if (typeof window === "undefined") return null;
  return sessionStorage;
}

export function setSession(data: SessionData) {
  const storage = getStorage();
  if (!storage) return;
  storage.setItem(TOKEN_KEY, data.token);
  if (data.refreshToken) {
    storage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
  }
  if (data.expires) {
    storage.setItem(TOKEN_EXPIRY_KEY, data.expires);
  }
  storage.setItem("userRole", data.role);
  // Set simplified role flag
  if (data.role === 'SuperAdmin' || data.role === 'SchoolAdmin' || data.role === 'Admin') {
    storage.setItem("role", "admin");
  } else if (data.role === 'Teacher') {
    storage.setItem("role", "teacher");
  } else if (data.role === 'Student') {
    storage.setItem("role", "student");
  }
  if (data.name) storage.setItem("userName", data.name);
}

export function getToken(): string | null {
  const storage = getStorage();
  if (!storage) return null;
  return storage.getItem(TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  const storage = getStorage();
  if (!storage) return null;
  return storage.getItem(REFRESH_TOKEN_KEY);
}

export function getSessionId(): string | null {
  const storage = getStorage();
  if (!storage) return null;
  return storage.getItem('sessionId');
}

export function getTokenExpiry(): Date | null {
  const storage = getStorage();
  if (!storage) return null;
  const expiry = storage.getItem(TOKEN_EXPIRY_KEY);
  return expiry ? new Date(expiry) : null;
}

export function isTokenExpired(): boolean {
  const expiry = getTokenExpiry();
  if (!expiry) return true;
  // Consider token expired 1 minute before actual expiry to allow for network latency
  return new Date() >= new Date(expiry.getTime() - 60000);
}

export function getRole(): string | null {
  const storage = getStorage();
  if (!storage) return null;
  return storage.getItem("userRole");
}

// School context for SchoolAdmin
export interface SchoolContext {
  id: number;
  schoolName: string;
  schoolCode: string;
  logoUrl?: string;
}

export function setSchoolContext(school: SchoolContext | null) {
  const storage = getStorage();
  if (!storage) return;
  if (school) {
    storage.setItem("schoolContext", JSON.stringify(school));
  } else {
    storage.removeItem("schoolContext");
  }
}

export function getSchoolContext(): SchoolContext | null {
  const storage = getStorage();
  if (!storage) return null;
  const data = storage.getItem("schoolContext");
  if (!data) return null;
  try {
    return JSON.parse(data);
  } catch {
    return null;
  }
}

export function isSuperAdmin(): boolean {
  return getRole() === "SuperAdmin";
}

export function isSchoolAdmin(): boolean {
  return getRole() === "SchoolAdmin";
}

export function clearSession() {
  const storage = getStorage();
  if (!storage) return;
  storage.removeItem(TOKEN_KEY);
  storage.removeItem(REFRESH_TOKEN_KEY);
  storage.removeItem(TOKEN_EXPIRY_KEY);
  storage.removeItem("userRole");
  storage.removeItem("role");
  storage.removeItem("userName");
  storage.removeItem("admin_email");
  storage.removeItem("admin_roles");
  storage.removeItem("school");
  storage.removeItem("schoolContext");
  storage.removeItem("assignedClass");
  storage.removeItem("assignedProgram");
  storage.removeItem("assignedLevel");
  storage.removeItem("sessionId");
}

export function logout(redirect: string = '/login') {
  // Try to revoke refresh token on server (fire and forget)
  const refreshToken = getRefreshToken();
  const token = getToken();
  
  if (refreshToken && token) {
    const sessionId = getSessionId();
    fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
        ...(sessionId ? { 'X-Session-Id': sessionId } : {})
      },
      credentials: 'include',
      body: JSON.stringify({ refreshToken })
    }).catch(() => {});
  }
  
  clearSession();
  if (typeof window !== "undefined") {
    window.location.href = redirect;
  }
}

/**
 * Validate the current session against the server.
 * Returns the session info (role, user, data) or null if invalid.
 * This is used by layout guards instead of trusting sessionStorage.
 */
export async function validateSession(): Promise<{
  role: string;
  roles: string[];
  user: { email: string; userName: string };
  data: any;
  schoolId: number | null;
  expiresAt: string;
} | null> {
  const token = getToken();
  if (!token) return null;

  try {
    const sessionId = getSessionId();
    const res = await fetch(`${API_BASE_URL}/api/auth/me`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        // iOS Safari (ITP) blocks cross-site cookies — send sessionId as header fallback
        ...(sessionId ? { 'X-Session-Id': sessionId } : {})
      },
      credentials: 'include'
    });

    if (!res.ok) return null;

    const data = await res.json();
    return data;
  } catch {
    return null;
  }
}

// Refresh the access token using the refresh token
async function refreshAccessToken(): Promise<boolean> {
  const refreshToken = getRefreshToken();
  
  if (!refreshToken) {
    return false;
  }
  
  try {
    const res = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include', // Send httpOnly session cookie
      body: JSON.stringify({ refreshToken })
    });
    
    if (!res.ok) {
      return false;
    }
    
    const contentType = res.headers.get("content-type");
    if (!contentType || !contentType.includes("application/json")) {
      return false;
    }
    
    const text = await res.text();
    if (text.trim() === '') {
      return false;
    }
    
    let data;
    try {
      data = JSON.parse(text);
    } catch (e) {
      return false;
    }
    
    if (data.token && data.refreshToken) {
      setSession({
        token: data.token,
        refreshToken: data.refreshToken,
        role: Array.isArray(data.role) ? data.role[0] : data.role,
        expires: data.expires,
        refreshTokenExpires: data.refreshTokenExpires
      });
      return true;
    }
    
    return false;
  } catch {
    return false;
  }
}

// Flag to prevent multiple simultaneous refresh attempts
let isRefreshing = false;
let refreshPromise: Promise<boolean> | null = null;

export async function fetchWithAuth(input: RequestInfo, init?: RequestInit) {
  let token = getToken();
  
  // Check if token is expired and try to refresh
  if (isTokenExpired() && getRefreshToken()) {
    if (!isRefreshing) {
      isRefreshing = true;
      refreshPromise = refreshAccessToken().finally(() => {
        isRefreshing = false;
        refreshPromise = null;
      });
    }
    
    // Wait for the refresh to complete
    const refreshed = await refreshPromise;
    if (refreshed) {
      token = getToken();
    }
  }
  
  if (!token) {
    clearSession();
    if (typeof window !== 'undefined') {
      window.location.href = '/login';
    }
    throw new Error('No authentication token');
  }
  
  const headers = { ...(init?.headers ?? {}), "Content-Type": "application/json" } as Record<string,string>;
  headers["Authorization"] = `Bearer ${token}`;
  // iOS Safari (ITP) blocks cross-site cookies — send sessionId as header fallback
  const sessionId = getSessionId();
  if (sessionId) headers["X-Session-Id"] = sessionId;
  
  // Build full URL if input is a relative path
  const url = typeof input === 'string' && input.startsWith('/') 
    ? `${API_BASE_URL}${input}` 
    : input;
    
  let res = await fetch(url, { ...init, headers, credentials: 'include' });
  
  // Handle 401 Unauthorized - try to refresh token
  if (res.status === 401) {
    const refreshed = await refreshAccessToken();
    
    if (refreshed) {
      // Retry the original request with new token
      const newToken = getToken();
      headers["Authorization"] = `Bearer ${newToken}`;
      const newSessionId = getSessionId();
      if (newSessionId) headers["X-Session-Id"] = newSessionId;
      res = await fetch(url, { ...init, headers, credentials: 'include' });
    } else {
      // Refresh failed - clear session and redirect to login
      clearSession();
      if (typeof window !== 'undefined') {
        window.location.href = '/login';
      }
      throw new Error('Session expired. Please login again.');
    }
  }
  
  if (!res.ok) {
    const text = await res.text().catch(() => '');
    throw new Error(`API error: ${res.status} ${text}`);
  }
  
  // Handle empty responses gracefully
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
    // Non-JSON response, return as text or empty object
    const text = await res.text();
    return text ? { message: text } : {};
  }
}
