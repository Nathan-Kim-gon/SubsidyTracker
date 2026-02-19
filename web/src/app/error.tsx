"use client";

export default function Error({
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="flex min-h-[50vh] flex-col items-center justify-center text-center">
      <h2 className="mb-2 text-2xl font-bold text-gray-900 dark:text-white">
        문제가 발생했습니다
      </h2>
      <p className="mb-6 text-gray-500 dark:text-gray-400">
        일시적인 오류가 발생했습니다. 잠시 후 다시 시도해주세요.
      </p>
      <button
        onClick={reset}
        className="rounded-lg bg-blue-600 px-6 py-3 font-medium text-white hover:bg-blue-700 dark:bg-blue-500 transition-colors"
      >
        다시 시도
      </button>
    </div>
  );
}
