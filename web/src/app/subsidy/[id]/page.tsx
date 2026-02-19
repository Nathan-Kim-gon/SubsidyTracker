import { Metadata } from "next";
import Link from "next/link";
import { getSubsidy } from "@/lib/api";
import { notFound } from "next/navigation";

interface Props {
  params: Promise<{ id: string }>;
}

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || "https://bojogeum.co.kr";

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { id } = await params;
  try {
    const subsidy = await getSubsidy(Number(id));
    const description =
      subsidy.description?.slice(0, 160) ||
      `${subsidy.organization} - ${subsidy.title}`;
    return {
      title: subsidy.title,
      description,
      openGraph: {
        title: `${subsidy.title} - 보조금 찾기`,
        description,
        url: `${SITE_URL}/subsidy/${id}`,
        type: "article",
      },
      alternates: {
        canonical: `${SITE_URL}/subsidy/${id}`,
      },
    };
  } catch {
    return { title: "보조금 찾기" };
  }
}

export default async function SubsidyDetailPage({ params }: Props) {
  const { id } = await params;
  let subsidy;
  try {
    subsidy = await getSubsidy(Number(id));
  } catch {
    notFound();
  }

  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "GovernmentService",
    name: subsidy.title,
    description: subsidy.description,
    provider: {
      "@type": "GovernmentOrganization",
      name: subsidy.organization,
    },
    areaServed: subsidy.regionName,
    category: subsidy.categoryName,
  };

  return (
    <article className="mx-auto max-w-3xl">
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }}
      />
      {/* Back button */}
      <Link
        href="/"
        className="mb-4 inline-flex items-center text-sm text-gray-500 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-400 transition-colors"
      >
        &larr; 목록으로 돌아가기
      </Link>

      {/* Header */}
      <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6 dark:border-gray-700 dark:bg-gray-800">
        <div className="mb-3 flex flex-wrap gap-2">
          <span className="rounded-full bg-blue-100 px-3 py-1 text-sm font-medium text-blue-700 dark:bg-blue-900/50 dark:text-blue-300">
            {subsidy.categoryName}
          </span>
          <span className="rounded-full bg-gray-100 px-3 py-1 text-sm font-medium text-gray-600 dark:bg-gray-700 dark:text-gray-300">
            {subsidy.regionName}
          </span>
          <span className="rounded-full bg-green-100 px-3 py-1 text-sm font-medium text-green-700 dark:bg-green-900/50 dark:text-green-300">
            {subsidy.status === "Active" ? "신청 가능" : subsidy.status}
          </span>
        </div>

        <h1 className="mb-2 text-2xl font-bold text-gray-900 dark:text-white">{subsidy.title}</h1>
        <p className="text-gray-500 dark:text-gray-400">{subsidy.organization}</p>
      </div>

      {/* Content sections */}
      <div className="space-y-4">
        {subsidy.description && (
          <Section title="서비스 요약">
            <p className="whitespace-pre-line">{subsidy.description}</p>
          </Section>
        )}

        {subsidy.amount && (
          <Section title="지원 내용">
            <p className="whitespace-pre-line">{subsidy.amount}</p>
          </Section>
        )}

        {subsidy.eligibilityCriteria && (
          <Section title="지원 대상 / 선정 기준">
            <p className="whitespace-pre-line">{subsidy.eligibilityCriteria}</p>
          </Section>
        )}

        {subsidy.applicationMethod && (
          <Section title="신청 방법">
            <p className="whitespace-pre-line">{subsidy.applicationMethod}</p>
          </Section>
        )}

        {subsidy.applicationEndDate && (
          <Section title="신청 기한">
            <p>~ {new Date(subsidy.applicationEndDate).toLocaleDateString("ko-KR")}</p>
          </Section>
        )}

        {subsidy.contactInfo && (
          <Section title="문의처">
            <p className="whitespace-pre-line">{subsidy.contactInfo}</p>
          </Section>
        )}

        {/* Action buttons */}
        <div className="flex flex-wrap gap-3 pt-2">
          {subsidy.applicationUrl && (
            <a
              href={subsidy.applicationUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="rounded-lg bg-blue-600 px-6 py-3 font-medium text-white hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 transition-colors"
            >
              신청 바로가기
            </a>
          )}
          {subsidy.sourceUrl && (
            <a
              href={subsidy.sourceUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="rounded-lg border border-gray-300 px-6 py-3 font-medium text-gray-700 hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700 transition-colors"
            >
              상세 정보 보기
            </a>
          )}
        </div>
      </div>
    </article>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-6 dark:border-gray-700 dark:bg-gray-800">
      <h2 className="mb-3 text-lg font-semibold text-gray-900 dark:text-white">{title}</h2>
      <div className="text-sm leading-relaxed text-gray-700 dark:text-gray-300">{children}</div>
    </div>
  );
}
