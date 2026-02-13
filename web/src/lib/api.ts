import { Category, PagedResult, Region, Subsidy, SubsidyDetail, SubsidyFilter } from "./types";

const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5173";

async function fetchApi<T>(path: string): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, { next: { revalidate: 300 } });
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  return res.json();
}

export async function getSubsidies(filter: SubsidyFilter = {}): Promise<PagedResult<Subsidy>> {
  const params = new URLSearchParams();
  if (filter.keyword) params.set("keyword", filter.keyword);
  if (filter.regionId) params.set("regionId", String(filter.regionId));
  if (filter.categoryId) params.set("categoryId", String(filter.categoryId));
  params.set("page", String(filter.page || 1));
  params.set("pageSize", String(filter.pageSize || 20));
  return fetchApi(`/api/subsidies?${params.toString()}`);
}

export async function getSubsidy(id: number): Promise<SubsidyDetail> {
  return fetchApi(`/api/subsidies/${id}`);
}

export async function getRegions(): Promise<Region[]> {
  return fetchApi("/api/regions");
}

export async function getCategories(): Promise<Category[]> {
  return fetchApi("/api/categories");
}
