import type { Metadata } from "next";
import { Inter, Poppins } from "next/font/google";
import "./globals.css";
import { ThemeProvider, themeInitScript } from "@/components/ThemeProvider";

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
  display: "swap",
});

const poppins = Poppins({
  variable: "--font-poppins",
  subsets: ["latin"],
  weight: ["300", "400", "500", "600", "700", "800"],
  display: "swap",
});

export const metadata: Metadata = {
  title: "Motion Robotics LMS - Learn Robotics, AI & Innovation",
  description: "Comprehensive Learning Management System for robotics education. Join 10,000+ students across 100+ schools learning robotics, AI, and 3D design through hands-on curriculum.",
  keywords: ["robotics education", "LMS", "AI learning", "STEM education", "robotics courses", "online learning"],
  authors: [{ name: "Motion Robotics" }],
  creator: "Motion Robotics",
  publisher: "Motion Robotics",
  formatDetection: {
    email: false,
    address: false,
    telephone: false,
  },
  metadataBase: new URL('https://lms.motionrobotics.in'),
  openGraph: {
    title: "Motion Robotics LMS - Master Robotics Education",
    description: "Join thousands of students learning robotics, AI, and innovation through our comprehensive LMS platform.",
    url: 'https://lms.motionrobotics.in',
    siteName: 'Motion Robotics LMS',
    locale: 'en_IN',
    type: 'website',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'Motion Robotics LMS',
    description: 'Comprehensive robotics education platform for schools and students',
  },
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      'max-video-preview': -1,
      'max-image-preview': 'large',
      'max-snippet': -1,
    },
  },
  icons: {
    icon: '/favicon.ico',
    apple: '/apple-touch-icon.png',
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="scroll-smooth" suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeInitScript }} />
        <link rel="icon" href="/favicon.ico" sizes="any" />
        <link rel="apple-touch-icon" href="/apple-touch-icon.png" />
        <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=5" />
        <meta name="theme-color" content="#0D9488" />
      </head>
      <body
        className={`${inter.variable} ${poppins.variable} antialiased`}
        style={{
          fontFamily: 'var(--font-poppins), var(--font-inter), system-ui, sans-serif',
          backgroundColor: 'var(--background)',
          color: 'var(--text-primary)',
        }}
      >
        <ThemeProvider>
          {children}
        </ThemeProvider>
      </body>
    </html>
  );
}