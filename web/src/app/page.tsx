import { Suspense } from "react";
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

  return (
    <>
      {/* Hero */}
      <section className="mb-8 text-center">
        <h1 className="mb-2 text-3xl font-bold text-gray-900">
          나에게 맞는 정부 보조금 찾기
        </h1>
        <p className="text-gray-500">
          총 <span className="font-semibold text-blue-600">{subsidies.totalCount.toLocaleString()}</span>건의 보조금 정보
        </p>
      </section>

      {/* Search & Filter */}
      <Suspense fallback={null}>
        <SearchFilter regions={regions} categories={categories} />
      </Suspense>

      {/* Results */}
      {subsidies.items.length === 0 ? (
        <div className="rounded-xl border border-gray-200 bg-white p-12 text-center">
          <p className="text-lg text-gray-500">검색 결과가 없습니다.</p>
          <p className="mt-1 text-sm text-gray-400">
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
