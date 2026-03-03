"use client";

import { useEffect, useState } from "react";

interface AnimatedRobotProps {
  className?: string;
}

export default function AnimatedRobot({ className = "" }: AnimatedRobotProps) {
  const [eyeBlink, setEyeBlink] = useState(false);
  const [waving, setWaving] = useState(false);

  useEffect(() => {
    // Blink animation
    const blinkInterval = setInterval(() => {
      setEyeBlink(true);
      setTimeout(() => setEyeBlink(false), 150);
    }, 3000);

    // Wave animation
    const waveInterval = setInterval(() => {
      setWaving(true);
      setTimeout(() => setWaving(false), 1000);
    }, 5000);

    return () => {
      clearInterval(blinkInterval);
      clearInterval(waveInterval);
    };
  }, []);

  return (
    <div className={`relative ${className}`}>
      {/* Floating particles */}
      <div className="absolute inset-0 overflow-hidden">
        {[...Array(12)].map((_, i) => (
          <div
            key={i}
            className="absolute rounded-full bg-orange-400/30"
            style={{
              width: `${Math.random() * 10 + 4}px`,
              height: `${Math.random() * 10 + 4}px`,
              left: `${Math.random() * 100}%`,
              top: `${Math.random() * 100}%`,
              animation: `float ${3 + Math.random() * 4}s ease-in-out infinite`,
              animationDelay: `${Math.random() * 2}s`,
            }}
          />
        ))}
      </div>

      {/* Main Robot SVG */}
      <svg
        viewBox="0 0 400 500"
        className="w-full h-auto max-w-md mx-auto drop-shadow-2xl"
        style={{ filter: "drop-shadow(0 25px 50px rgba(240, 90, 40, 0.3))" }}
      >
        {/* Glow effect */}
        <defs>
          <radialGradient id="robotGlow" cx="50%" cy="50%" r="50%">
            <stop offset="0%" stopColor="#F05A28" stopOpacity="0.3" />
            <stop offset="100%" stopColor="#F05A28" stopOpacity="0" />
          </radialGradient>
          <linearGradient id="bodyGradient" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" stopColor="#4A5568" />
            <stop offset="50%" stopColor="#2D3748" />
            <stop offset="100%" stopColor="#1A202C" />
          </linearGradient>
          <linearGradient id="orangeGradient" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" stopColor="#FF7A4D" />
            <stop offset="100%" stopColor="#F05A28" />
          </linearGradient>
          <linearGradient id="screenGradient" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" stopColor="#1a1a2e" />
            <stop offset="100%" stopColor="#16213e" />
          </linearGradient>
          <filter id="glow">
            <feGaussianBlur stdDeviation="3" result="coloredBlur"/>
            <feMerge>
              <feMergeNode in="coloredBlur"/>
              <feMergeNode in="SourceGraphic"/>
            </feMerge>
          </filter>
        </defs>

        {/* Background glow */}
        <ellipse cx="200" cy="280" rx="180" ry="200" fill="url(#robotGlow)" />

        {/* Antenna */}
        <g className="animate-bounce" style={{ animationDuration: "2s", transformOrigin: "200px 80px" }}>
          <line x1="200" y1="110" x2="200" y2="70" stroke="#4A5568" strokeWidth="6" strokeLinecap="round" />
          <circle cx="200" cy="60" r="15" fill="url(#orangeGradient)" filter="url(#glow)">
            <animate attributeName="opacity" values="1;0.6;1" dur="1.5s" repeatCount="indefinite" />
          </circle>
          <circle cx="200" cy="60" r="8" fill="#fff" opacity="0.6" />
        </g>

        {/* Head */}
        <rect x="120" y="110" width="160" height="120" rx="25" fill="url(#bodyGradient)" />
        <rect x="130" y="120" width="140" height="100" rx="18" fill="url(#screenGradient)" />

        {/* Eyes */}
        <g>
          {/* Left eye */}
          <ellipse
            cx="160"
            cy="165"
            rx={eyeBlink ? 15 : 15}
            ry={eyeBlink ? 3 : 20}
            fill="#F05A28"
            filter="url(#glow)"
          >
            <animate attributeName="opacity" values="1;0.8;1" dur="2s" repeatCount="indefinite" />
          </ellipse>
          <ellipse cx="165" cy="160" rx="5" ry={eyeBlink ? 1 : 6} fill="#fff" opacity="0.7" />

          {/* Right eye */}
          <ellipse
            cx="240"
            cy="165"
            rx={eyeBlink ? 15 : 15}
            ry={eyeBlink ? 3 : 20}
            fill="#F05A28"
            filter="url(#glow)"
          >
            <animate attributeName="opacity" values="1;0.8;1" dur="2s" repeatCount="indefinite" />
          </ellipse>
          <ellipse cx="245" cy="160" rx="5" ry={eyeBlink ? 1 : 6} fill="#fff" opacity="0.7" />
        </g>

        {/* Mouth/Speaker */}
        <rect x="165" y="195" width="70" height="12" rx="6" fill="#F05A28" opacity="0.8">
          <animate attributeName="width" values="70;50;70" dur="3s" repeatCount="indefinite" />
          <animate attributeName="x" values="165;175;165" dur="3s" repeatCount="indefinite" />
        </rect>

        {/* Ear panels */}
        <rect x="100" y="140" width="20" height="60" rx="5" fill="url(#orangeGradient)" />
        <rect x="280" y="140" width="20" height="60" rx="5" fill="url(#orangeGradient)" />

        {/* Neck */}
        <rect x="175" y="230" width="50" height="30" fill="#4A5568" />
        <rect x="180" y="235" width="40" height="20" rx="3" fill="#2D3748" />

        {/* Body */}
        <rect x="100" y="260" width="200" height="150" rx="20" fill="url(#bodyGradient)" />
        
        {/* Chest panel */}
        <rect x="130" y="280" width="140" height="80" rx="12" fill="url(#screenGradient)" />
        
        {/* Heart/Core */}
        <circle cx="200" cy="320" r="25" fill="url(#orangeGradient)" filter="url(#glow)">
          <animate attributeName="r" values="25;28;25" dur="1s" repeatCount="indefinite" />
        </circle>
        <circle cx="200" cy="320" r="15" fill="#fff" opacity="0.3">
          <animate attributeName="r" values="15;18;15" dur="1s" repeatCount="indefinite" />
        </circle>
        
        {/* Chest lights */}
        <g>
          {[0, 1, 2].map((i) => (
            <circle
              key={i}
              cx={155 + i * 25}
              cy="370"
              r="6"
              fill="#F05A28"
              opacity="0.8"
            >
              <animate
                attributeName="opacity"
                values="0.4;1;0.4"
                dur="1s"
                repeatCount="indefinite"
                begin={`${i * 0.2}s`}
              />
            </circle>
          ))}
        </g>

        {/* Left Arm */}
        <g style={{ 
          transform: waving ? "rotate(-20deg)" : "rotate(0deg)",
          transformOrigin: "100px 280px",
          transition: "transform 0.3s ease-in-out"
        }}>
          {/* Shoulder */}
          <circle cx="100" cy="280" r="20" fill="#4A5568" />
          <circle cx="100" cy="280" r="12" fill="url(#orangeGradient)" />
          
          {/* Upper arm */}
          <rect x="60" y="280" width="40" height="60" rx="10" fill="url(#bodyGradient)" />
          
          {/* Elbow */}
          <circle cx="80" cy="340" r="12" fill="#4A5568" />
          
          {/* Lower arm */}
          <rect x="65" y="340" width="30" height="50" rx="8" fill="url(#bodyGradient)" />
          
          {/* Hand */}
          <circle cx="80" cy="400" r="18" fill="#4A5568" />
          <circle cx="80" cy="400" r="10" fill="url(#orangeGradient)" />
        </g>

        {/* Right Arm - Waving */}
        <g style={{ 
          transform: waving ? "rotate(30deg)" : "rotate(0deg)",
          transformOrigin: "300px 280px",
          transition: "transform 0.3s ease-in-out"
        }}>
          {/* Shoulder */}
          <circle cx="300" cy="280" r="20" fill="#4A5568" />
          <circle cx="300" cy="280" r="12" fill="url(#orangeGradient)" />
          
          {/* Upper arm */}
          <rect x="300" y="280" width="40" height="60" rx="10" fill="url(#bodyGradient)" />
          
          {/* Elbow */}
          <circle cx="320" cy="340" r="12" fill="#4A5568" />
          
          {/* Lower arm */}
          <rect x="305" y="340" width="30" height="50" rx="8" fill="url(#bodyGradient)" />
          
          {/* Hand */}
          <circle cx="320" cy="400" r="18" fill="#4A5568" />
          <circle cx="320" cy="400" r="10" fill="url(#orangeGradient)" />
          
          {/* Fingers for waving */}
          {waving && (
            <g>
              <rect x="310" y="415" width="6" height="15" rx="3" fill="#4A5568" />
              <rect x="318" y="415" width="6" height="18" rx="3" fill="#4A5568" />
              <rect x="326" y="415" width="6" height="15" rx="3" fill="#4A5568" />
            </g>
          )}
        </g>

        {/* Base/Wheels */}
        <ellipse cx="200" cy="420" rx="100" ry="20" fill="#2D3748" />
        <ellipse cx="200" cy="420" rx="80" ry="15" fill="#4A5568" />
        
        {/* Wheel details */}
        <ellipse cx="150" cy="430" rx="30" ry="15" fill="#1A202C" />
        <ellipse cx="150" cy="430" rx="20" ry="10" fill="#2D3748" />
        <ellipse cx="250" cy="430" rx="30" ry="15" fill="#1A202C" />
        <ellipse cx="250" cy="430" rx="20" ry="10" fill="#2D3748" />
        
        {/* Orange accents on wheels */}
        <ellipse cx="150" cy="430" rx="8" ry="4" fill="url(#orangeGradient)" />
        <ellipse cx="250" cy="430" rx="8" ry="4" fill="url(#orangeGradient)" />

        {/* Ground shadow */}
        <ellipse cx="200" cy="470" rx="120" ry="15" fill="#000" opacity="0.2" />
      </svg>

      {/* Decorative elements */}
      <div className="absolute top-10 left-10 w-20 h-20 border-2 border-orange-400/20 rounded-full animate-ping" style={{ animationDuration: "3s" }} />
      <div className="absolute bottom-20 right-10 w-16 h-16 border-2 border-orange-400/20 rounded-full animate-ping" style={{ animationDuration: "4s" }} />
      
      {/* Code snippets floating */}
      <div className="absolute top-20 right-0 text-orange-400/40 font-mono text-xs animate-pulse">
        {"<Robot />"}
      </div>
      <div className="absolute bottom-32 left-0 text-orange-400/40 font-mono text-xs animate-pulse" style={{ animationDelay: "1s" }}>
        {"{ learn: true }"}
      </div>

      <style jsx>{`
        @keyframes float {
          0%, 100% {
            transform: translateY(0) rotate(0deg);
            opacity: 0.3;
          }
          50% {
            transform: translateY(-20px) rotate(180deg);
            opacity: 0.8;
          }
        }
      `}</style>
    </div>
  );
}
