import { Suspense } from "react";
import { Metadata } from "next";
import { getCategories, getRegions, getSubsidies } from "@/lib/api";
import SearchFilter from "@/components/SearchFilter";
import SubsidyCard from "@/components/SubsidyCard";
import Pagination from "@/components/Pagination";

interface Props {
  searchParams: Promise<{
    keyword?: string;
    regionId?: string;
    categoryId?: string;
    sortBy?: string;
    page?: string;
  }>;
}

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || "https://bojogeum.co.kr";

export async function generateMetadata({ searchParams }: Props): Promise<Metadata> {
  const params = await searchParams;
  const parts: string[] = [];

  if (params.keyword) parts.push(`"${params.keyword}"`);

  if (params.categoryId) {
    try {
      const categories = await getCategories();
      const cat = categories.find((c) => c.id === Number(params.categoryId));
      if (cat) parts.push(cat.name);
    } catch {}
  }

  if (params.regionId) {
    try {
      const regions = await getRegions();
      const findRegion = (list: typeof regions): string | null => {
        for (const r of list) {
          if (r.id === Number(params.regionId)) return r.name;
          const child = findRegion(r.children);
          if (child) return child;
        }
        return null;
      };
      const name = findRegion(regions);
      if (name) parts.push(name);
    } catch {}
  }

  if (parts.length === 0) return {};

  const label = parts.join(" ");
  const title = `${label} 보조금 - 보조금 찾기`;
  const description = `${label} 관련 정부 보조금·지원금을 한눈에 검색하세요. 지역별, 분야별 맞춤 보조금 정보를 제공합니다.`;

  return {
    title,
    description,
    openGraph: {
      title,
      description,
      url: SITE_URL,
    },
  };
}

export default async function Home({ searchParams }: Props) {
  const params = await searchParams;
  const [subsidies, regions, categories] = await Promise.all([
    getSubsidies({
      keyword: params.keyword,
      regionId: params.regionId ? Number(params.regionId) : undefined,
      categoryId: params.categoryId ? Number(params.categoryId) : undefined,
      sortBy: params.sortBy,
      page: params.page ? Number(params.page) : 1,
      pageSize: 20,
    }),
    getRegions(),
    getCategories(),
  ]);

  // 동적 히어로 제목
  const headingParts: string[] = [];
  if (params.keyword) headingParts.push(`"${params.keyword}"`);
  if (params.categoryId) {
    const cat = categories.find((c) => c.id === Number(params.categoryId));
    if (cat) headingParts.push(cat.name);
  }
  if (params.regionId) {
    const findRegion = (list: typeof regions): string | null => {
      for (const r of list) {
        if (r.id === Number(params.regionId)) return r.name;
        const child = findRegion(r.children);
        if (child) return child;
      }
      return null;
    };
    const name = findRegion(regions);
    if (name) headingParts.push(name);
  }
  const heading = headingParts.length > 0
    ? `${headingParts.join(" ")} 보조금`
    : "나에게 맞는 정부 보조금 찾기";

  return (
    <>
      {/* Hero */}
      <section className="relative mb-8 text-center">
        <div className="absolute inset-0 -z-10 rounded-2xl bg-gradient-to-b from-blue-50 to-transparent dark:from-blue-950/30 dark:to-transparent" />
        <h1 className="mb-2 text-3xl font-bold text-gray-900 dark:text-white">
          {heading}
        </h1>
        <p className="text-gray-500 dark:text-gray-400">
          총 <span className="font-semibold text-blue-600">{subsidies.totalCount.toLocaleString()}</span>건의 보조금 정보
        </p>
      </section>

      {/* Search & Filter */}
      <Suspense fallback={null}>
        <SearchFilter regions={regions} categories={categories} />
      </Suspense>

      {/* Results */}
      {subsidies.items.length === 0 ? (
        <div className="rounded-xl border border-gray-200 bg-white p-12 text-center dark:border-gray-700 dark:bg-gray-800">
          <p className="text-lg text-gray-500 dark:text-gray-400">검색 결과가 없습니다.</p>
          <p className="mt-1 text-sm text-gray-400 dark:text-gray-500">
            다른 키워드나 필터로 다시 검색해보세요.
          </p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {subsidies.items.map((subsidy) => (
            <SubsidyCard key={subsidy.id} subsidy={subsidy} />
          ))}
        </div>
      )}

      {/* Pagination */}
      <Suspense fallback={null}>
        <Pagination page={subsidies.page} totalPages={subsidies.totalPages} />
      </Suspense>
    </>
  );
}
