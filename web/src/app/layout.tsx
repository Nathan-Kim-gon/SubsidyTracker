import type { Metadata } from "next";
import { Geist } from "next/font/google";
import Link from "next/link";
import "./globals.css";

const geist = Geist({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "보조금 찾기 - 정부 지원금 한눈에",
  description:
    "정부 보조금, 지원금, 청년 정책 등 나에게 맞는 혜택을 쉽게 찾아보세요. 지역별, 분야별 맞춤 검색을 지원합니다.",
  keywords: "정부 보조금, 지원금, 청년 정책, 복지, 보조금 찾기",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="ko">
      <body className={`${geist.className} bg-gray-50 text-gray-900 antialiased`}>
        <header className="sticky top-0 z-50 border-b border-gray-200 bg-white/80 backdrop-blur-md">
          <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4">
            <Link href="/" className="text-xl font-bold text-blue-600">
              보조금 찾기
            </Link>
            <nav className="flex gap-6 text-sm font-medium text-gray-600">
              <Link href="/" className="hover:text-blue-600 transition-colors">
                전체 보조금
              </Link>
              <Link href="/?categoryId=4" className="hover:text-blue-600 transition-colors">
                고용/창업
              </Link>
              <Link href="/?categoryId=3" className="hover:text-blue-600 transition-colors">
                교육
              </Link>
              <Link href="/?categoryId=5" className="hover:text-blue-600 transition-colors">
                보건/의료
              </Link>
            </nav>
          </div>
        </header>
        <main className="mx-auto max-w-6xl px-4 py-6">{children}</main>
        <footer className="border-t border-gray-200 bg-white py-8 text-center text-sm text-gray-500">
          <p>&copy; 2026 보조금 찾기. 공공데이터포털 기반.</p>
        </footer>
      </body>
    </html>
  );
}
