'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@clerk/nextjs';
import { CheckCircle, XCircle, Loader2 } from 'lucide-react';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

interface Props {
  params: Promise<{ token: string }>;
}

export default function AcceptInvitePage({ params }: Props) {
  const router = useRouter();
  const { getToken } = useAuth();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [message, setMessage] = useState('');

  useEffect(() => {
    (async () => {
      const { token } = await params;
      try {
        const authToken = await getToken();
        const res = await fetch(`${API_URL}/api/v1/invites/${token}`, {
          method: 'POST',
          headers: { Authorization: `Bearer ${authToken}` },
        });
        const data = await res.json();
        if (res.ok) {
          setStatus('success');
          setMessage('You now have access to the repository.');
          setTimeout(() => {
            router.push(`/repositories/${data.repositoryId}`);
          }, 2000);
        } else {
          setStatus('error');
          setMessage(data?.error ?? 'This invite is invalid or has already been used.');
        }
      } catch {
        setStatus('error');
        setMessage('Failed to accept invite. Please try again.');
      }
    })();
  }, [getToken, params, router]);

  return (
    <div className="flex items-center justify-center min-h-[50vh]">
      <div className="w-full max-w-sm text-center space-y-4">
        {status === 'loading' && (
          <>
            <Loader2 size={36} className="mx-auto animate-spin text-blue-400" />
            <p className="text-sm text-neutral-400">Accepting invite…</p>
          </>
        )}
        {status === 'success' && (
          <>
            <CheckCircle size={36} className="mx-auto text-emerald-400" />
            <p className="text-sm font-medium text-white">Invite accepted!</p>
            <p className="text-xs text-neutral-500">{message} Redirecting…</p>
          </>
        )}
        {status === 'error' && (
          <>
            <XCircle size={36} className="mx-auto text-red-400" />
            <p className="text-sm font-medium text-white">Could not accept invite</p>
            <p className="text-xs text-neutral-500">{message}</p>
            <button
              onClick={() => router.push('/dashboard')}
              className="mt-2 px-4 py-2 rounded-lg bg-neutral-800 text-sm text-neutral-300 hover:bg-neutral-700 transition-colors"
            >
              Go to Dashboard
            </button>
          </>
        )}
      </div>
    </div>
  );
}
