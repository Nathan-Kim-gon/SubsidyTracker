import type { Metadata } from "next";
import { Geist } from "next/font/google";
import Link from "next/link";
import MobileNav from "@/components/MobileNav";
import ThemeToggle from "@/components/ThemeToggle";
import GoogleAnalytics from "@/components/GoogleAnalytics";
import "./globals.css";

const geist = Geist({ subsets: ["latin"] });

const navLinks = [
  { href: "/", label: "전체 보조금" },
  { href: "/?categoryId=10", label: "청년" },
  { href: "/?categoryId=4", label: "고용/창업" },
  { href: "/?categoryId=3", label: "교육" },
  { href: "/?categoryId=5", label: "보건/의료" },
];

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || "https://bojogeum.co.kr";

export const metadata: Metadata = {
  title: {
    default: "보조금 찾기 - 정부 보조금·지원금 한눈에",
    template: "%s | 보조금 찾기",
  },
  description:
    "정부 보조금, 지원금, 청년 정책 등 나에게 맞는 혜택을 쉽게 찾아보세요. 12,000건 이상의 보조금 정보를 지역별, 분야별로 검색할 수 있습니다.",
  keywords:
    "정부 보조금, 지원금, 청년 정책, 복지, 보조금 찾기, 청년 보조금, 정부 혜택, 지원금 검색",
  metadataBase: new URL(SITE_URL),
  openGraph: {
    title: "보조금 찾기 - 정부 보조금·지원금 한눈에",
    description:
      "12,000건 이상의 정부 보조금·지원금을 지역별, 분야별로 검색하세요.",
    url: SITE_URL,
    siteName: "보조금 찾기",
    locale: "ko_KR",
    type: "website",
  },
  twitter: {
    card: "summary_large_image",
    title: "보조금 찾기 - 정부 보조금·지원금 한눈에",
    description:
      "12,000건 이상의 정부 보조금·지원금을 지역별, 분야별로 검색하세요.",
  },
  alternates: {
    canonical: SITE_URL,
  },
  verification: {
    google: process.env.NEXT_PUBLIC_GOOGLE_VERIFICATION,
  },
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="ko" suppressHydrationWarning>
      <head>
        <meta name="naver-site-verification" content="09901c6189797bac8b8349e85a4951a46e185aa9" />
        <script
          dangerouslySetInnerHTML={{
            __html: `(function(){try{var t=localStorage.getItem('theme');if(t==='dark'||(!t&&matchMedia('(prefers-color-scheme:dark)').matches))document.documentElement.classList.add('dark')}catch(e){}})()`,
          }}
        />
      </head>
      <GoogleAnalytics />
      <body
        className={`${geist.className} bg-gray-50 text-gray-900 antialiased transition-colors dark:bg-gray-900 dark:text-gray-100`}
      >
        <header className="sticky top-0 z-50 border-b border-gray-200 bg-white/80 backdrop-blur-md dark:border-gray-700 dark:bg-gray-900/80">
          <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4">
            <Link
              href="/"
              className="bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-xl font-bold text-transparent"
            >
              보조금 찾기
            </Link>
            <nav className="hidden gap-6 text-sm font-medium text-gray-600 dark:text-gray-300 md:flex">
              {navLinks.map((link) => (
                <Link
                  key={link.href}
                  href={link.href}
                  className="hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
                >
                  {link.label}
                </Link>
              ))}
            </nav>
            <div className="flex items-center gap-1">
              <ThemeToggle />
              <MobileNav links={navLinks} />
            </div>
          </div>
        </header>
        <main className="mx-auto max-w-6xl px-4 py-6">{children}</main>
        <footer className="border-t border-gray-200 bg-white py-8 text-center text-sm text-gray-500 dark:border-gray-700 dark:bg-gray-900 dark:text-gray-400">
          <p>&copy; 2026 보조금 찾기. 공공데이터포털 기반.</p>
          <p className="mx-auto mt-2 max-w-md text-xs text-gray-400 dark:text-gray-500">
            본 서비스는 공공데이터를 기반으로 정보를 제공하며, 정보의 정확성·완전성을
            보장하지 않습니다. 실제 보조금 신청 시 해당 기관에 직접 확인하시기
            바랍니다.
          </p>
          <div className="mt-4 flex justify-center gap-4 text-xs text-gray-400 dark:text-gray-500">
            <Link href="/privacy" className="hover:text-gray-600 dark:hover:text-gray-300 transition-colors">
              개인정보처리방침
            </Link>
            <span>|</span>
            <Link href="/terms" className="hover:text-gray-600 dark:hover:text-gray-300 transition-colors">
              이용약관
            </Link>
          </div>
        </footer>
      </body>
    </html>
  );
}
