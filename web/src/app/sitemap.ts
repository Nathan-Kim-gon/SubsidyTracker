import type { MetadataRoute } from "next";

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || "https://bojogeum.co.kr";
const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5173";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  // 정적 페이지
  const staticPages: MetadataRoute.Sitemap = [
    {
      url: SITE_URL,
      lastModified: new Date(),
      changeFrequency: "daily",
      priority: 1,
    },
  ];

  // 카테고리 페이지
  try {
    const categoriesRes = await fetch(`${API_BASE}/api/categories`, {
      next: { revalidate: 86400 },
    });
    if (categoriesRes.ok) {
      const categories = await categoriesRes.json();
      for (const cat of categories) {
        staticPages.push({
          url: `${SITE_URL}/?categoryId=${cat.id}`,
          lastModified: new Date(),
          changeFrequency: "daily",
          priority: 0.8,
        });
      }
    }
  } catch {}

  // 보조금 상세 페이지 (최근 업데이트 순으로 최대 5000개)
  try {
    const subsidiesRes = await fetch(
      `${API_BASE}/api/subsidies?pageSize=100&sortBy=CreatedAt&sortDescending=true`,
      { next: { revalidate: 86400 } }
    );
    if (subsidiesRes.ok) {
      const data = await subsidiesRes.json();
      const totalPages = Math.min(data.totalPages, 50); // 최대 50페이지 = 5000개

      // 첫 페이지 데이터
      for (const item of data.items) {
        staticPages.push({
          url: `${SITE_URL}/subsidy/${item.id}`,
          lastModified: new Date(item.createdAt),
          changeFrequency: "weekly",
          priority: 0.6,
        });
      }

      // 나머지 페이지
      for (let page = 2; page <= totalPages; page++) {
        try {
          const res = await fetch(
            `${API_BASE}/api/subsidies?page=${page}&pageSize=100&sortBy=CreatedAt&sortDescending=true`,
            { next: { revalidate: 86400 } }
          );
          if (res.ok) {
            const pageData = await res.json();
            for (const item of pageData.items) {
              staticPages.push({
                url: `${SITE_URL}/subsidy/${item.id}`,
                lastModified: new Date(item.createdAt),
                changeFrequency: "weekly",
                priority: 0.6,
              });
            }
          }
        } catch {}
      }
    }
  } catch {}

  return staticPages;
}
