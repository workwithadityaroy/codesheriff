'use client';

import { useEffect, useRef, useState } from 'react';
import Link from 'next/link';
import { ArrowRight, GitBranch, RefreshCw } from 'lucide-react';
import { PullRequestSummaryDto } from '@/types/api';
import PullRequestStatusBadge from './PullRequestStatusBadge';
import TechDebtGauge from './TechDebtGauge';
import { useApiClient } from '@/hooks/useApiClient';

const ACTIVE_STATUSES = new Set(['Pending', 'Reviewing']);
const POLL_INTERVAL_MS = 3000;

interface Props {
  initialPrs: PullRequestSummaryDto[];
  repoId: string;
}

export default function PullRequestsTableLive({ initialPrs, repoId }: Props) {
  const [prs, setPrs] = useState<PullRequestSummaryDto[]>(initialPrs);
  const [reanalyzing, setReanalyzing] = useState<Set<string>>(new Set());
  const [reanalyzeError, setReanalyzeError] = useState<string | null>(null);
  const { get, post } = useApiClient();
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const hasActive = prs.some((pr) => ACTIVE_STATUSES.has(pr.status));

  useEffect(() => {
    if (!hasActive) {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
      return;
    }

    intervalRef.current = setInterval(async () => {
      try {
        const updated = await get<PullRequestSummaryDto[]>(
          `/api/v1/repositories/${repoId}/pull-requests`
        );
        setPrs(updated);
      } catch {
        // silent — keep polling
      }
    }, POLL_INTERVAL_MS);

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [hasActive, repoId]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleReanalyze = async (prId: string) => {
    setReanalyzeError(null);
    setReanalyzing((prev) => new Set(prev).add(prId));
    try {
      await post(`/api/v1/pull-requests/${prId}/reanalyze`);
      // optimistically set the PR status to Reviewing so polling starts
      setPrs((prev) =>
        prev.map((pr) => (pr.id === prId ? { ...pr, status: 'Reviewing' } : pr))
      );
    } catch (err) {
      setReanalyzeError(err instanceof Error ? err.message : 'Re-analyze failed.');
    } finally {
      setReanalyzing((prev) => {
        const next = new Set(prev);
        next.delete(prId);
        return next;
      });
    }
  };

  if (prs.length === 0) return null;

  return (
    <div className="space-y-3">
      {reanalyzeError && (
        <div className="rounded-lg border border-red-500/20 bg-red-500/5 px-4 py-2.5">
          <p className="text-xs text-red-400">{reanalyzeError}</p>
        </div>
      )}

      <div className="rounded-xl border border-neutral-800 overflow-hidden">
        <div className="divide-y divide-neutral-800/60">
          {prs.map((pr) => {
            const isActive = ACTIVE_STATUSES.has(pr.status);
            const isReanalyzing = reanalyzing.has(pr.id);
            const canReanalyze = pr.status === 'Reviewed' || pr.status === 'Failed';

            return (
              <div
                key={pr.id}
                className="flex items-center justify-between px-5 py-4 hover:bg-neutral-900/40 transition-colors"
              >
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2.5">
                    <span className="text-[11px] font-mono text-neutral-600">
                      #{pr.gitHubPrNumber}
                    </span>
                    <span className="truncate text-sm font-medium text-neutral-200">
                      {pr.title}
                    </span>
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
                  {pr.latestTechDebtScore !== null && !isActive && (
                    <TechDebtGauge score={pr.latestTechDebtScore} small />
                  )}

                  {isActive ? (
                    <span className="inline-flex items-center gap-1.5 rounded-full border border-blue-500/20 bg-blue-500/10 px-2.5 py-0.5 text-xs font-medium text-blue-400">
                      <span className="relative flex h-1.5 w-1.5">
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75" />
                        <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-blue-400" />
                      </span>
                      {pr.status === 'Pending' ? 'Queued' : 'Analyzing…'}
                    </span>
                  ) : (
                    <PullRequestStatusBadge status={pr.status} />
                  )}

                  {canReanalyze && (
                    <button
                      onClick={() => handleReanalyze(pr.id)}
                      disabled={isReanalyzing}
                      className="inline-flex items-center gap-1 text-xs font-medium text-neutral-500 hover:text-neutral-300 disabled:opacity-40 transition-colors"
                      title="Re-analyze with AI"
                    >
                      <RefreshCw size={11} className={isReanalyzing ? 'animate-spin' : ''} />
                      {isReanalyzing ? 'Queuing…' : 'Re-analyze'}
                    </button>
                  )}

                  {pr.latestReviewId && !isActive && (
                    <Link
                      href={`/reviews/${pr.latestReviewId}`}
                      className="inline-flex items-center gap-1 text-xs font-medium text-blue-400 hover:text-blue-300 transition-colors"
                    >
                      View <ArrowRight size={11} />
                    </Link>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
