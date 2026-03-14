import { fetchApi } from '@/lib/api';
import { RepositoryDto, PullRequestSummaryDto, PagedResult } from '@/types/api';
import PullRequestsTableLive from '@/components/PullRequestsTableLive';
import EmptyState from '@/components/EmptyState';
import RepoTabs from '@/components/RepoTabs';
import { GitPullRequest } from 'lucide-react';
import { currentUser } from '@clerk/nextjs/server';

interface Props {
  params: Promise<{ repoId: string }>;
  searchParams: Promise<{ tab?: string }>;
}

export default async function RepositoryDetailPage({ params, searchParams }: Props) {
  const { repoId } = await params;
  const { tab = 'prs' } = await searchParams;

  let repo: RepositoryDto | null = null;
  let pullRequests: PullRequestSummaryDto[] = [];
  let totalCount = 0;
  let error: string | null = null;
  let clerkUserId = '';

  try {
    const [repoData, paged, user] = await Promise.all([
      fetchApi<RepositoryDto>(`/api/v1/repositories/${repoId}`),
      fetchApi<PagedResult<PullRequestSummaryDto>>(`/api/v1/repositories/${repoId}/pull-requests`),
      currentUser(),
    ]);
    repo = repoData;
    pullRequests = paged.items;
    totalCount = paged.totalCount;
    clerkUserId = user?.id ?? '';
  } catch {
    error = 'Failed to load repository data.';
  }

  if (error || !repo) {
    return (
      <div className="rounded-xl border border-red-500/20 bg-red-500/5 px-5 py-4">
        <p className="text-sm text-red-400">{error ?? 'Repository not found.'}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <div className="flex items-center gap-2 mb-1">
          <span className="text-[11px] font-semibold uppercase tracking-widest text-neutral-600">Repository</span>
        </div>
        <h1 className="text-xl font-semibold text-white tracking-tight">{repo.fullName}</h1>
        <p className="mt-0.5 text-sm text-neutral-500">
          {totalCount > 0
            ? `${totalCount} pull request${totalCount !== 1 ? 's' : ''}`
            : 'No pull requests yet'}
        </p>
      </div>

      <RepoTabs
        activeTab={tab}
        repoId={repoId}
        currentClerkUserId={clerkUserId}
        pullRequestsContent={
          pullRequests.length === 0 ? (
            <EmptyState
              title="No pull requests"
              description="Pull requests appear here once opened and the webhook triggers."
              icon={<GitPullRequest size={22} className="text-blue-400" />}
            />
          ) : (
            <PullRequestsTableLive initialPrs={pullRequests} initialTotal={totalCount} repoId={repoId} />
          )
        }
      />
    </div>
  );
}
