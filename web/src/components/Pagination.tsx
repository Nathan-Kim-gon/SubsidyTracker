"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useTransition } from "react";

interface Props {
  page: number;
  totalPages: number;
}

export default function Pagination({ page, totalPages }: Props) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();

  function goToPage(p: number) {
    const params = new URLSearchParams(searchParams.toString());
    params.set("page", String(p));
    startTransition(() => router.push(`/?${params.toString()}`));
  }

  if (totalPages <= 1) return null;

  const start = Math.max(1, page - 2);
  const end = Math.min(totalPages, start + 4);
  const pages = Array.from({ length: end - start + 1 }, (_, i) => start + i);

  // 모바일: 현재 페이지 기준 앞뒤 1개만
  const mobileStart = Math.max(1, page - 1);
  const mobileEnd = Math.min(totalPages, mobileStart + 2);
  const mobilePages = Array.from(
    { length: mobileEnd - mobileStart + 1 },
    (_, i) => mobileStart + i
  );

  return (
    <div className="mt-8 flex items-center justify-center gap-1">
      <button
        onClick={() => goToPage(page - 1)}
        disabled={page <= 1 || isPending}
        className="rounded-lg border border-gray-300 px-3 py-2.5 text-sm disabled:opacity-30 hover:bg-gray-100 transition-colors"
      >
        이전
      </button>

      {/* 데스크톱 페이지네이션 */}
      <div className="hidden items-center gap-1 sm:flex">
        {start > 1 && (
          <>
            <button
              onClick={() => goToPage(1)}
              className="rounded-lg border border-gray-300 px-3 py-2 text-sm hover:bg-gray-100"
            >
              1
            </button>
            {start > 2 && <span className="px-1 text-gray-400">...</span>}
          </>
        )}

        {pages.map((p) => (
          <button
            key={p}
            onClick={() => goToPage(p)}
            disabled={isPending}
            className={`rounded-lg border px-3 py-2 text-sm transition-colors ${
              p === page
                ? "border-blue-600 bg-blue-600 text-white"
                : "border-gray-300 hover:bg-gray-100"
            }`}
          >
            {p}
          </button>
        ))}

        {end < totalPages && (
          <>
            {end < totalPages - 1 && (
              <span className="px-1 text-gray-400">...</span>
            )}
            <button
              onClick={() => goToPage(totalPages)}
              className="rounded-lg border border-gray-300 px-3 py-2 text-sm hover:bg-gray-100"
            >
              {totalPages}
            </button>
          </>
        )}
      </div>

      {/* 모바일 페이지네이션 (간소화) */}
      <div className="flex items-center gap-1 sm:hidden">
        {mobilePages.map((p) => (
          <button
            key={p}
            onClick={() => goToPage(p)}
            disabled={isPending}
            className={`rounded-lg border px-3 py-2.5 text-sm transition-colors ${
              p === page
                ? "border-blue-600 bg-blue-600 text-white"
                : "border-gray-300 hover:bg-gray-100"
            }`}
          >
            {p}
          </button>
        ))}
      </div>

      <button
        onClick={() => goToPage(page + 1)}
        disabled={page >= totalPages || isPending}
        className="rounded-lg border border-gray-300 px-3 py-2.5 text-sm disabled:opacity-30 hover:bg-gray-100 transition-colors"
      >
        다음
      </button>
    </div>
  );
}
