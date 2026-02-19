export default function SubsidyLoading() {
  return (
    <div className="mx-auto max-w-3xl">
      <div className="mb-4 h-5 w-36 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />

      {/* Header card skeleton */}
      <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6 dark:border-gray-700 dark:bg-gray-800">
        <div className="mb-3 flex gap-2">
          <div className="h-6 w-16 animate-pulse rounded-full bg-gray-200 dark:bg-gray-700" />
          <div className="h-6 w-14 animate-pulse rounded-full bg-gray-200 dark:bg-gray-700" />
          <div className="h-6 w-16 animate-pulse rounded-full bg-gray-200 dark:bg-gray-700" />
        </div>
        <div className="mb-2 h-8 w-2/3 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
        <div className="h-5 w-1/4 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
      </div>

      {/* Content sections skeleton */}
      <div className="space-y-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <div
            key={i}
            className="rounded-xl border border-gray-200 bg-white p-6 dark:border-gray-700 dark:bg-gray-800"
          >
            <div className="mb-3 h-6 w-24 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
            <div className="space-y-2">
              <div className="h-4 w-full animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
              <div className="h-4 w-5/6 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
              <div className="h-4 w-2/3 animate-pulse rounded bg-gray-200 dark:bg-gray-700" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
