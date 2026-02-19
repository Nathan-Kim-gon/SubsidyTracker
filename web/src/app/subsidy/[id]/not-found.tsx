import Link from "next/link";

export default function SubsidyNotFound() {
  return (
    <div className="flex min-h-[50vh] flex-col items-center justify-center text-center">
      <h2 className="mb-2 text-2xl font-bold text-gray-900">
        보조금 정보를 찾을 수 없습니다
      </h2>
      <p className="mb-6 text-gray-500">
        해당 보조금이 삭제되었거나 존재하지 않는 번호입니다.
      </p>
      <Link
        href="/"
        className="rounded-lg bg-blue-600 px-6 py-3 font-medium text-white hover:bg-blue-700 transition-colors"
      >
        보조금 목록으로
      </Link>
    </div>
  );
}
