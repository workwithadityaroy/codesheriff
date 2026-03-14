import Link from 'next/link';
import { fetchApi } from '@/lib/api';
import { ReviewDetailDto } from '@/types/api';
import TechDebtGauge from '@/components/TechDebtGauge';
import IssuesTable from '@/components/IssuesTable';
import ReviewStatusPoller from '@/components/ReviewStatusPoller';
import { Clock, GitPullRequest } from 'lucide-react';

interface Props {
  params: Promise<{ reviewId: string }>;
}

export default async function ReviewDetailPage({ params }: Props) {
  const { reviewId } = await params;

  let review: ReviewDetailDto | null = null;
  let error: string | null = null;

  try {
    review = await fetchApi<ReviewDetailDto>(`/api/v1/reviews/${reviewId}`);
  } catch {
    error = 'Review not found.';
  }

  if (error || !review) {
    return (
      <div className="rounded-xl border border-red-500/20 bg-red-500/5 px-5 py-4">
        <p className="text-sm text-red-400">{error ?? 'Review not found.'}</p>
      </div>
    );
  }

  const isActive = review.status === 'Pending' || review.status === 'Processing';

  return (
    <div className="space-y-6 max-w-5xl">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-xs text-neutral-600">
        <Link href="/repositories" className="hover:text-neutral-400 transition-colors">Repositories</Link>
        <span>/</span>
        <span className="text-neutral-500">{review.repositoryFullName}</span>
        <span>/</span>
        <span className="text-neutral-500 font-mono">PR #{review.gitHubPrNumber}</span>
      </div>

      {/* Header */}
      <div>
        <div className="flex items-center gap-2 mb-1">
          <GitPullRequest size={14} className="text-neutral-600" />
          <span className="text-[11px] font-semibold uppercase tracking-widest text-neutral-600">
            {review.repositoryFullName}
          </span>
        </div>
        <h1 className="text-xl font-semibold text-white tracking-tight">{review.pullRequestTitle}</h1>
        {!isActive && (
          <div className="mt-1 flex items-center gap-4 text-xs text-neutral-600">
            {review.processingTimeMs && (
              <span className="flex items-center gap-1">
                <Clock size={11} />
                {(review.processingTimeMs / 1000).toFixed(1)}s analysis
              </span>
            )}
            <span>{review.issues.length} issue{review.issues.length !== 1 ? 's' : ''} found</span>
          </div>
        )}
      </div>

      {/* Live analysis indicator (client island — polls until done) */}
      <ReviewStatusPoller reviewId={reviewId} currentStatus={review.status} />

      {/* Score + Summary — only shown when completed */}
      {!isActive && (
        <>
          <div className="flex items-start gap-6 rounded-xl border border-neutral-800 bg-neutral-900/50 p-5">
            <TechDebtGauge score={review.techDebtScore} />
            <div className="flex-1 min-w-0">
              <p className="text-[11px] font-semibold uppercase tracking-widest text-neutral-600 mb-2">AI Summary</p>
              <p className="text-sm text-neutral-400 leading-relaxed">{review.summary}</p>
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-3">
              <p className="text-sm font-semibold text-neutral-300">
                Issues
                <span className="ml-2 text-xs font-normal text-neutral-600">({review.issues.length})</span>
              </p>
            </div>
            <IssuesTable issues={review.issues} />
          </div>
        </>
      )}
    </div>
  );
}
