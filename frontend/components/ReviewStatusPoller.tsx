'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useApiClient } from '@/hooks/useApiClient';
import { ReviewDetailDto } from '@/types/api';

const ACTIVE_STATUSES = new Set(['Pending', 'Processing']);
const POLL_INTERVAL_MS = 3000;

interface Props {
  reviewId: string;
  currentStatus: string;
}

export default function ReviewStatusPoller({ reviewId, currentStatus }: Props) {
  const router = useRouter();
  const { get } = useApiClient();

  useEffect(() => {
    if (!ACTIVE_STATUSES.has(currentStatus)) return;

    const interval = setInterval(async () => {
      try {
        const review = await get<ReviewDetailDto>(`/api/v1/reviews/${reviewId}`);
        if (!ACTIVE_STATUSES.has(review.status)) {
          clearInterval(interval);
          router.refresh();
        }
      } catch {
        // silent
      }
    }, POLL_INTERVAL_MS);

    return () => clearInterval(interval);
  }, [reviewId, currentStatus]); // eslint-disable-line react-hooks/exhaustive-deps

  if (!ACTIVE_STATUSES.has(currentStatus)) return null;

  return (
    <div className="flex items-center gap-3 rounded-xl border border-blue-500/20 bg-blue-500/5 px-5 py-4">
      <span className="relative flex h-2.5 w-2.5 shrink-0">
        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75" />
        <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-blue-400" />
      </span>
      <div>
        <p className="text-sm font-medium text-blue-300">AI analysis in progress</p>
        <p className="text-xs text-neutral-500 mt-0.5">
          This usually takes 10–20 seconds. The page will update automatically.
        </p>
      </div>
    </div>
  );
}
