'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { UserButton } from '@clerk/nextjs';
import { LayoutDashboard, GitBranch, Shield, ChevronRight, Settings } from 'lucide-react';

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-screen bg-neutral-950 overflow-hidden">
      {/* Dark Sidebar */}
      <aside className="w-60 shrink-0 flex flex-col bg-neutral-950 border-r border-neutral-800">
        {/* Logo */}
        <div className="px-5 py-5 border-b border-neutral-800">
          <Link href="/dashboard" className="flex items-center gap-2.5">
            <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-blue-500 to-violet-600 flex items-center justify-center shadow-lg shadow-blue-500/20">
              <Shield size={15} className="text-white" />
            </div>
            <span className="text-sm font-semibold text-white tracking-tight">CodeSheriff</span>
          </Link>
        </div>

        {/* Nav */}
        <nav className="flex-1 px-3 py-4 space-y-0.5 overflow-y-auto">
          <p className="px-3 mb-2 text-[10px] font-semibold text-neutral-600 uppercase tracking-widest">Menu</p>
          <NavLink href="/dashboard" icon={<LayoutDashboard size={15} />} label="Dashboard" />
          <NavLink href="/repositories" icon={<GitBranch size={15} />} label="Repositories" />
          <NavLink href="/settings" icon={<Settings size={15} />} label="Settings" />
        </nav>

        {/* User */}
        <div className="border-t border-neutral-800 p-3">
          <div className="flex items-center gap-3 px-2 py-2 rounded-lg hover:bg-neutral-900 transition-colors cursor-pointer">
            <UserButton />
            <div className="flex-1 min-w-0">
              <p className="text-xs font-medium text-neutral-300 truncate">Account</p>
            </div>
          </div>
        </div>
      </aside>

      {/* Main */}
      <div className="flex-1 flex flex-col overflow-hidden bg-neutral-950">
        {/* Top bar */}
        <header className="shrink-0 h-14 border-b border-neutral-800 flex items-center px-6 gap-2">
          <Breadcrumb />
        </header>

        {/* Scrollable content */}
        <main className="flex-1 overflow-auto p-6">{children}</main>
      </div>
    </div>
  );
}

function NavLink({ href, icon, label }: { href: string; icon: React.ReactNode; label: string }) {
  const pathname = usePathname();
  const active = pathname === href || (href !== '/dashboard' && pathname.startsWith(href));

  return (
    <Link
      href={href}
      className={`flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm font-medium transition-all duration-150 ${
        active
          ? 'bg-neutral-800 text-white'
          : 'text-neutral-400 hover:bg-neutral-900 hover:text-neutral-200'
      }`}
    >
      <span className={active ? 'text-blue-400' : ''}>{icon}</span>
      {label}
      {active && <ChevronRight size={13} className="ml-auto text-neutral-600" />}
    </Link>
  );
}

function Breadcrumb() {
  const pathname = usePathname();
  const segments = pathname.split('/').filter(Boolean);

  return (
    <div className="flex items-center gap-1.5 text-sm">
      {segments.map((seg, i) => {
        const isLast = i === segments.length - 1;
        const label = seg.charAt(0).toUpperCase() + seg.slice(1);
        return (
          <span key={i} className="flex items-center gap-1.5">
            {i > 0 && <span className="text-neutral-700">/</span>}
            <span className={isLast ? 'text-neutral-200 font-medium' : 'text-neutral-600'}>
              {label.length > 20 ? `${label.slice(0, 20)}…` : label}
            </span>
          </span>
        );
      })}
    </div>
  );
}
