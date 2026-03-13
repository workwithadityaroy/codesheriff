import { fetchApi } from '@/lib/api';
import { DashboardSummaryDto } from '@/types/api';
import StatsCard from '@/components/StatsCard';
import { Database, GitPullRequest, BarChart3, ShieldAlert } from 'lucide-react';

export default async function DashboardPage() {
  let summary: DashboardSummaryDto | null = null;
  let error: string | null = null;

  try {
    summary = await fetchApi<DashboardSummaryDto>('/api/v1/dashboard/summary');
  } catch {
    error = 'Failed to load dashboard data.';
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-xl font-semibold text-white tracking-tight">Overview</h1>
        <p className="mt-0.5 text-sm text-neutral-500">Your code quality at a glance.</p>
      </div>

      {error ? (
        <div className="rounded-xl border border-red-500/20 bg-red-500/5 px-5 py-4">
          <p className="text-sm text-red-400">{error}</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <StatsCard
            label="Repositories"
            value={summary?.totalRepositories ?? 0}
            icon={<Database size={16} />}
            accent="blue"
            description="Connected GitHub repos"
          />
          <StatsCard
            label="PRs Reviewed"
            value={summary?.totalPrsReviewed ?? 0}
            icon={<GitPullRequest size={16} />}
            accent="violet"
            description="Total AI-reviewed pull requests"
          />
          <StatsCard
            label="Avg Tech Debt"
            value={Math.round(summary?.averageTechDebtScore ?? 0)}
            suffix="/ 100"
            icon={<BarChart3 size={16} />}
            accent="emerald"
            description="Lower is better"
          />
          <StatsCard
            label="Critical Issues"
            value={summary?.criticalIssueCount ?? 0}
            icon={<ShieldAlert size={16} />}
            accent="red"
            description="Across all reviewed PRs"
          />
        </div>
      )}

      {!error && summary?.totalRepositories === 0 && (
        <div className="rounded-xl border border-neutral-800 bg-neutral-900/40 px-8 py-12 text-center">
          <div className="mx-auto mb-4 w-12 h-12 rounded-xl bg-blue-500/10 border border-blue-500/20 flex items-center justify-center">
            <Database size={20} className="text-blue-400" />
          </div>
          <p className="text-sm font-medium text-neutral-300">No repositories connected</p>
          <p className="mt-1 text-xs text-neutral-600">Head to Repositories to register your first GitHub repo.</p>
        </div>
      )}
    </div>
  );
}
