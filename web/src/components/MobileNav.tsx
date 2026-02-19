"use client";

import Link from "next/link";
import { useState } from "react";

interface Props {
  links: { href: string; label: string }[];
}

export default function MobileNav({ links }: Props) {
  const [open, setOpen] = useState(false);

  return (
    <div className="md:hidden">
      <button
        onClick={() => setOpen(!open)}
        aria-label="메뉴 열기"
        className="flex h-10 w-10 items-center justify-center rounded-lg text-gray-600 hover:bg-gray-100 transition-colors"
      >
        {open ? (
          <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M6 6l12 12M6 18L18 6" />
          </svg>
        ) : (
          <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        )}
      </button>

      {open && (
        <nav className="absolute left-0 top-16 z-50 w-full border-b border-gray-200 bg-white shadow-lg">
          <div className="mx-auto flex max-w-6xl flex-col px-4 py-2">
            {links.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                onClick={() => setOpen(false)}
                className="border-b border-gray-100 py-3 text-sm font-medium text-gray-700 last:border-0 hover:text-blue-600 transition-colors"
              >
                {link.label}
              </Link>
            ))}
          </div>
        </nav>
      )}
    </div>
  );
}
