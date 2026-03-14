'use client';

import { useRouter, usePathname, useSearchParams } from 'next/navigation';
import { GitPullRequest, Users } from 'lucide-react';
import TeamTab from './TeamTab';
import { useUser } from '@clerk/nextjs';

interface Props {
  activeTab: string;
  repoId: string;
  currentClerkUserId: string;
  pullRequestsContent: React.ReactNode;
}

export default function RepoTabs({ activeTab, repoId, currentClerkUserId, pullRequestsContent }: Props) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const { user } = useUser();

  const clerkUserId = user?.id ?? currentClerkUserId;

  const navigate = (tab: string) => {
    const params = new URLSearchParams(searchParams.toString());
    params.set('tab', tab);
    router.push(`${pathname}?${params.toString()}`);
  };

  const tabs = [
    { id: 'prs', label: 'Pull Requests', icon: <GitPullRequest size={13} /> },
    { id: 'team', label: 'Team', icon: <Users size={13} /> },
  ];

  return (
    <div className="space-y-4">
      {/* Tab bar */}
      <div className="flex gap-1 border-b border-neutral-800">
        {tabs.map(t => (
          <button
            key={t.id}
            onClick={() => navigate(t.id)}
            className={`flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium border-b-2 -mb-px transition-colors ${
              activeTab === t.id
                ? 'border-blue-500 text-white'
                : 'border-transparent text-neutral-500 hover:text-neutral-300'
            }`}
          >
            {t.icon}
            {t.label}
          </button>
        ))}
      </div>

      {/* Tab content */}
      {activeTab === 'team' ? (
        <TeamTab
          repoId={repoId}
          ownerClerkUserId={currentClerkUserId}
          currentClerkUserId={clerkUserId}
        />
      ) : (
        pullRequestsContent
      )}
    </div>
  );
}
