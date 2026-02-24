# **FIX: Login Redirect Loop Issue**

## **The Problem:**
After successful login, you're redirected to dashboard for 1 second, then back to login page.

## **Root Cause:**
The authentication guard/middleware is checking auth status **before** or **incorrectly after** the token is stored.

---

## **SOLUTION 1: Check Token Storage (Most Common)**

### **In your login handler (frontend):**

**WRONG (Causes redirect loop):**
```typescript
// ? BAD - Token not awaited properly
const handleLogin = async () => {
  const response = await fetch('/api/admin/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password })
  });
  
  const data = await response.json();
  localStorage.setItem('token', data.token);  // This might not complete
  router.push('/admin/dashboard');  // Redirect happens immediately!
}
```

**CORRECT:**
```typescript
// ? GOOD - Ensure token is saved before redirect
const handleLogin = async () => {
  try {
    const response = await fetch('http://localhost:5000/api/admin/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ 
        email, 
        password 
      })
    });

    if (!response.ok) {
      throw new Error('Login failed');
    }

    const data = await response.json();
    
    // Store token FIRST
    localStorage.setItem('admin_token', data.token);
    localStorage.setItem('admin_email', data.adminEmail);
    localStorage.setItem('admin_roles', JSON.stringify(data.roles));
    
    // WAIT a bit for storage to complete
    await new Promise(resolve => setTimeout(resolve, 100));
    
    // THEN redirect
    router.push('/admin/dashboard');
    
  } catch (error) {
    console.error('Login error:', error);
    setError('Invalid credentials');
  }
};
```

---

## **SOLUTION 2: Fix Auth Guard/Middleware**

### **If using Next.js middleware.ts:**

**WRONG:**
```typescript
// ? BAD - Immediately redirects if no token
export function middleware(request: NextRequest) {
  const token = request.cookies.get('token');
  
  if (!token) {
    return NextResponse.redirect(new URL('/admin/login', request.url));
  }
}
```

**CORRECT:**
```typescript
// ? GOOD - Check token properly
export function middleware(request: NextRequest) {
  const path = request.nextUrl.pathname;
  
  // Allow login page without token
  if (path === '/admin/login') {
    return NextResponse.next();
  }
  
  // Check for token in cookies OR localStorage
  const token = request.cookies.get('admin_token')?.value;
  
  // Protected routes
  if (path.startsWith('/admin/dashboard') && !token) {
    return NextResponse.redirect(new URL('/admin/login', request.url));
  }
  
  return NextResponse.next();
}

export const config = {
  matcher: ['/admin/:path*']
};
```

---

## **SOLUTION 3: Use useEffect to Check Auth**

### **In your dashboard page:**

```typescript
'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

export default function AdminDashboard() {
  const router = useRouter();

  useEffect(() => {
    // Check authentication on mount
    const token = localStorage.getItem('admin_token');
    
    if (!token) {
      router.push('/admin/login');
      return;
    }
    
    // Optionally verify token with backend
    verifyToken(token);
  }, []);

  const verifyToken = async (token: string) => {
    try {
      const response = await fetch('http://localhost:5000/api/admin/verify', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      
      if (!response.ok) {
        localStorage.removeItem('admin_token');
        router.push('/admin/login');
      }
    } catch (error) {
      console.error('Token verification failed:', error);
    }
  };

  return (
    <div>
      {/* Your dashboard content */}
    </div>
  );
}
```

---

## **SOLUTION 4: Check Console for Errors**

Open browser DevTools (F12) and check:

### **Console Tab:**
Look for errors like:
- ? `401 Unauthorized`
- ? `Token expired`
- ? `Invalid token format`

### **Network Tab:**
1. Click on the login request
2. Check response:
   - ? Status: `200 OK`
   - ? Response body has `token`, `adminEmail`, `roles`

### **Application Tab (Storage):**
1. Go to `Local Storage`
2. Check if token is saved:
   - ? Key: `admin_token` or `token`
   - ? Value: Long string starting with `eyJ...`

---

## **SOLUTION 5: Backend CORS Issue**

Make sure your backend allows the frontend origin.

### **Check Program.cs:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")  // ? Your frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // ? Important for cookies
    });
});

// ...

app.UseCors("AllowNextJs");  // ? Before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
```

---

## **SOLUTION 6: Check Token Format**

When making authenticated requests, use correct format:

**WRONG:**
```typescript
headers: {
  'Authorization': token  // ? Missing "Bearer"
}
```

**CORRECT:**
```typescript
headers: {
  'Authorization': `Bearer ${token}`  // ? With "Bearer" prefix
}
```

---

## **QUICK DEBUG STEPS:**

### **1. Add console.logs to login handler:**
```typescript
const handleLogin = async () => {
  console.log('1. Login started');
  
  const response = await fetch('...');
  console.log('2. Response received:', response.status);
  
  const data = await response.json();
  console.log('3. Data:', data);
  
  localStorage.setItem('admin_token', data.token);
  console.log('4. Token saved:', localStorage.getItem('admin_token'));
  
  await new Promise(resolve => setTimeout(resolve, 100));
  console.log('5. About to redirect');
  
  router.push('/admin/dashboard');
  console.log('6. Redirect called');
};
```

### **2. Add console.logs to dashboard:**
```typescript
useEffect(() => {
  console.log('Dashboard mounted');
  const token = localStorage.getItem('admin_token');
  console.log('Token from storage:', token);
  
  if (!token) {
    console.log('No token - redirecting to login');
    router.push('/admin/login');
  } else {
    console.log('Token exists - staying on dashboard');
  }
}, []);
```

---

## **MOST LIKELY FIX (Try This First):**

### **In your login page component:**

```typescript
'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';

export default function AdminLogin() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const router = useRouter();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await fetch('http://localhost:5000/api/admin/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        throw new Error('Login failed');
      }

      const data = await response.json();

      // Save token
      localStorage.setItem('admin_token', data.token);
      localStorage.setItem('admin_email', data.adminEmail);
      
      // Wait for storage
      await new Promise(resolve => setTimeout(resolve, 100));

      // Redirect
      window.location.href = '/admin/dashboard';  // Use window.location instead of router.push
      
    } catch (err) {
      setError('Invalid email or password');
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* Your form fields */}
    </form>
  );
}
```

---

## **KEY CHANGES:**

1. ? Use `window.location.href` instead of `router.push()` for redirect
2. ? Add 100ms delay after saving token
3. ? Proper error handling
4. ? Loading state

---

## **TEST STEPS:**

1. Clear browser cache and localStorage
2. Open DevTools Console (F12)
3. Try login
4. Watch console logs
5. Check Network tab for 200 response
6. Check Application > Local Storage for token
7. If token is there and you still redirect ? Check dashboard auth logic

---

## **If Still Not Working:**

Share your:
1. Login page component code
2. Dashboard page component code
3. middleware.ts (if exists)
4. Console errors (screenshot)

