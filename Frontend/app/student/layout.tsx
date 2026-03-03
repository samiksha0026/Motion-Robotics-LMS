"use client";

import { useEffect, useState } from "react";
import { usePathname } from "next/navigation";
import { validateSession } from "../../lib/auth";

export default function StudentLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Skip auth check on login redirect page
    if (pathname === "/student/login") {
      setIsLoading(false);
      return;
    }

    const token = sessionStorage.getItem("jwt");
    const userRole = sessionStorage.getItem("userRole");

    if (!token || userRole !== "Student") {
      sessionStorage.clear();
      window.location.href = "/login";
      return;
    }

    // Validate session server-side
    validateSession().then(session => {
      if (!session || session.role !== "Student") {
        sessionStorage.clear();
        window.location.href = "/login";
        return;
      }
      setIsLoading(false);
    });
  }, [pathname]);

  if (isLoading && pathname !== "/student/login") {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="w-10 h-10 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mx-auto mb-3" />
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
