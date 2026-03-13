import Link from 'next/link';
import { fetchApi } from '@/lib/api';
import { RepositoryDto, PullRequestSummaryDto } from '@/types/api';
import PullRequestStatusBadge from '@/components/PullRequestStatusBadge';
import TechDebtGauge from '@/components/TechDebtGauge';
import EmptyState from '@/components/EmptyState';
import { GitPullRequest, ArrowRight, GitBranch } from 'lucide-react';

interface Props {
  params: Promise<{ repoId: string }>;
}

export default async function RepositoryDetailPage({ params }: Props) {
  const { repoId } = await params;

  let repo: RepositoryDto | null = null;
  let pullRequests: PullRequestSummaryDto[] = [];
  let error: string | null = null;

  try {
    [repo, pullRequests] = await Promise.all([
      fetchApi<RepositoryDto>(`/api/v1/repositories/${repoId}`),
      fetchApi<PullRequestSummaryDto[]>(`/api/v1/repositories/${repoId}/pull-requests`),
    ]);
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
          {pullRequests.length > 0 ? `${pullRequests.length} pull request${pullRequests.length !== 1 ? 's' : ''}` : 'No pull requests yet'}
        </p>
      </div>

      {pullRequests.length === 0 && (
        <EmptyState
          title="No pull requests"
          description="Pull requests appear here once opened and the webhook triggers."
          icon={<GitPullRequest size={22} className="text-blue-400" />}
        />
      )}

      {pullRequests.length > 0 && (
        <div className="rounded-xl border border-neutral-800 overflow-hidden">
          <div className="divide-y divide-neutral-800/60">
            {pullRequests.map((pr) => (
              <div
                key={pr.id}
                className="flex items-center justify-between px-5 py-4 hover:bg-neutral-900/40 transition-colors"
              >
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2.5">
                    <span className="text-[11px] font-mono text-neutral-600">#{pr.gitHubPrNumber}</span>
                    <span className="truncate text-sm font-medium text-neutral-200">{pr.title}</span>
                  </div>
                  <div className="mt-1.5 flex items-center gap-1.5 text-[11px] text-neutral-600">
                    <GitBranch size={11} />
                    <span className="font-mono">{pr.headBranch}</span>
                    <ArrowRight size={10} />
                    <span className="font-mono">{pr.baseBranch}</span>
                    <span className="ml-1 text-neutral-700">·</span>
                    <span>{pr.authorLogin}</span>
                  </div>
                </div>

                <div className="ml-6 flex items-center gap-3 shrink-0">
                  {pr.latestTechDebtScore !== null && (
                    <TechDebtGauge score={pr.latestTechDebtScore} small />
                  )}
                  <PullRequestStatusBadge status={pr.status} />
                  {pr.latestReviewId && (
                    <Link
                      href={`/reviews/${pr.latestReviewId}`}
                      className="inline-flex items-center gap-1 text-xs font-medium text-blue-400 hover:text-blue-300 transition-colors"
                    >
                      View <ArrowRight size={11} />
                    </Link>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
