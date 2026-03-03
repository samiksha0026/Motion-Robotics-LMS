import type { NextConfig } from "next";

// Motion Robotics LMS - Next.js Configuration
const nextConfig: NextConfig = {
  images: {
    remotePatterns: [
      // Local development
      {
        protocol: "http",
        hostname: "localhost",
        port: "5235",
        pathname: "/**",
      },
      // Production backend on Render
      {
        protocol: "https",
        hostname: "motion-robotics-lms.onrender.com",
        port: "",
        pathname: "/**",
      },
    ],
  },
};

export default nextConfig;
