import Link from "next/link";
import { Subsidy } from "@/lib/types";

export default function SubsidyCard({ subsidy }: { subsidy: Subsidy }) {
  return (
    <Link
      href={`/subsidy/${subsidy.id}`}
      className="block rounded-xl border border-gray-200 bg-white p-5 shadow-sm hover:shadow-md hover:border-blue-300 transition-all border-l-4 border-l-blue-500 dark:bg-gray-800 dark:border-gray-700 dark:hover:border-blue-500"
    >
      <div className="mb-2 flex flex-wrap items-center gap-2">
        <span className="rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-700 dark:bg-blue-900/50 dark:text-blue-300">
          {subsidy.categoryName}
        </span>
        <span className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-600 dark:bg-gray-700 dark:text-gray-300">
          {subsidy.regionName}
        </span>
        {subsidy.applicationEndDate && (
          <span className="rounded-full bg-orange-100 px-2.5 py-0.5 text-xs font-medium text-orange-700 dark:bg-orange-900/50 dark:text-orange-300">
            ~{new Date(subsidy.applicationEndDate).toLocaleDateString("ko-KR")}
          </span>
        )}
      </div>

      <h3 className="mb-1 text-lg font-semibold text-gray-900 line-clamp-2 dark:text-gray-100">
        {subsidy.title}
      </h3>

      <p className="mb-3 text-sm text-gray-500 dark:text-gray-400">{subsidy.organization}</p>

      {subsidy.amount && (
        <p className="text-sm text-gray-700 line-clamp-2 dark:text-gray-400">{subsidy.amount}</p>
      )}
    </Link>
  );
}
