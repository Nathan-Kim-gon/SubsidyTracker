export default function Loading() {
  return (
    <>
      {/* Hero skeleton */}
      <section className="mb-8 text-center">
        <div className="mx-auto mb-2 h-9 w-64 animate-pulse rounded-lg bg-gray-200" />
        <div className="mx-auto h-5 w-40 animate-pulse rounded bg-gray-200" />
      </section>

      {/* Search skeleton */}
      <div className="mb-6 space-y-4">
        <div className="h-12 animate-pulse rounded-lg bg-gray-200" />
        <div className="flex gap-3">
          <div className="h-10 w-32 animate-pulse rounded-lg bg-gray-200" />
          <div className="h-10 w-32 animate-pulse rounded-lg bg-gray-200" />
        </div>
      </div>

      {/* Card grid skeleton */}
      <div className="grid gap-4 sm:grid-cols-2">
        {Array.from({ length: 6 }).map((_, i) => (
          <div
            key={i}
            className="rounded-xl border border-gray-200 bg-white p-5"
          >
            <div className="mb-2 flex gap-2">
              <div className="h-5 w-16 animate-pulse rounded-full bg-gray-200" />
              <div className="h-5 w-14 animate-pulse rounded-full bg-gray-200" />
            </div>
            <div className="mb-1 h-6 w-3/4 animate-pulse rounded bg-gray-200" />
            <div className="mb-3 h-4 w-1/3 animate-pulse rounded bg-gray-200" />
            <div className="h-4 w-1/2 animate-pulse rounded bg-gray-200" />
          </div>
        ))}
      </div>
    </>
  );
}
