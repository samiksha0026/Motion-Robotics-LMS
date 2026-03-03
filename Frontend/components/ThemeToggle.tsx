"use client";

import { Sun, Moon, Monitor } from "lucide-react";
import { useTheme, type Theme } from "./ThemeProvider";

interface ThemeToggleProps {
  /** Show a simple light/dark toggle (default) or full 3-way selector */
  variant?: "toggle" | "selector";
  className?: string;
}

export function ThemeToggle({ variant = "toggle", className = "" }: ThemeToggleProps) {
  const { theme, resolvedTheme, setTheme, toggleTheme } = useTheme();

  if (variant === "selector") {
    const options: { value: Theme; icon: typeof Sun; label: string }[] = [
      { value: "light", icon: Sun, label: "Light" },
      { value: "dark", icon: Moon, label: "Dark" },
      { value: "system", icon: Monitor, label: "System" },
    ];

    return (
      <div
        className={`inline-flex items-center rounded-lg p-1 ${className}`}
        style={{ backgroundColor: "var(--surface-secondary)" }}
      >
        {options.map(({ value, icon: Icon, label }) => (
          <button
            key={value}
            onClick={() => setTheme(value)}
            title={label}
            className={`relative flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-all duration-200 ${theme === value
                ? "shadow-sm"
                : "hover:opacity-80"
              }`}
            style={{
              backgroundColor: theme === value ? "var(--surface)" : "transparent",
              color: theme === value ? "var(--text-primary)" : "var(--text-secondary)",
            }}
          >
            <Icon size={14} />
            <span className="hidden sm:inline">{label}</span>
          </button>
        ))}
      </div>
    );
  }

  // Simple toggle button
  return (
    <button
      onClick={toggleTheme}
      className={`relative inline-flex items-center justify-center rounded-lg p-2.5 transition-all duration-200 hover:scale-105 active:scale-95 border shadow-md ${className}`}
      style={{
        backgroundColor: resolvedTheme === "light" ? "#FFFFFF" : "#1E293B",
        borderColor: resolvedTheme === "light" ? "#E2E8F0" : "#475569",
        color: resolvedTheme === "light" ? "#1E293B" : "#F1F5F9",
      }}
      title={resolvedTheme === "light" ? "Switch to dark mode" : "Switch to light mode"}
      aria-label={resolvedTheme === "light" ? "Switch to dark mode" : "Switch to light mode"}
    >
      {resolvedTheme === "light" ? <Moon size={20} /> : <Sun size={20} />}
    </button>
  );
}
