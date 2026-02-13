"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { Category, Region } from "@/lib/types";
import { useState, useTransition } from "react";

interface Props {
  regions: Region[];
  categories: Category[];
}

export default function SearchFilter({ regions, categories }: Props) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();

  const [keyword, setKeyword] = useState(searchParams.get("keyword") || "");

  function applyFilters(overrides: Record<string, string | undefined> = {}) {
    const params = new URLSearchParams(searchParams.toString());
    params.delete("page");

    for (const [key, value] of Object.entries(overrides)) {
      if (value) params.set(key, value);
      else params.delete(key);
    }

    startTransition(() => {
      router.push(`/?${params.toString()}`);
    });
  }

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    applyFilters({ keyword: keyword || undefined });
  }

  return (
    <div className="mb-6 space-y-4">
      <form onSubmit={handleSearch} className="flex gap-2">
        <input
          type="text"
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          placeholder="보조금 검색 (예: 청년, 주거, 출산)"
          className="flex-1 rounded-lg border border-gray-300 px-4 py-3 text-base focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
        />
        <button
          type="submit"
          disabled={isPending}
          className="rounded-lg bg-blue-600 px-6 py-3 font-medium text-white hover:bg-blue-700 disabled:opacity-50 transition-colors"
        >
          검색
        </button>
      </form>

      <div className="flex flex-wrap gap-3">
        <select
          value={searchParams.get("regionId") || ""}
          onChange={(e) => applyFilters({ regionId: e.target.value || undefined })}
          className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        >
          <option value="">전체 지역</option>
          {regions.map((r) => (
            <option key={r.id} value={r.id}>
              {r.name}
            </option>
          ))}
        </select>

        <select
          value={searchParams.get("categoryId") || ""}
          onChange={(e) => applyFilters({ categoryId: e.target.value || undefined })}
          className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        >
          <option value="">전체 분야</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>

        {(searchParams.get("keyword") ||
          searchParams.get("regionId") ||
          searchParams.get("categoryId")) && (
          <button
            onClick={() => {
              setKeyword("");
              startTransition(() => router.push("/"));
            }}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-500 hover:bg-gray-100 transition-colors"
          >
            필터 초기화
          </button>
        )}
      </div>
    </div>
  );
}
