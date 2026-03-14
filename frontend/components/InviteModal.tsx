'use client';

import { useState } from 'react';
import { useAuth } from '@clerk/nextjs';
import { X, UserPlus, Loader2, Copy, CheckCircle } from 'lucide-react';
import { InviteMemberResult } from '@/types/api';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

interface Props {
  repoId: string;
  onClose: () => void;
  onSuccess: () => void;
}

export default function InviteModal({ repoId, onClose, onSuccess }: Props) {
  const { getToken } = useAuth();
  const [email, setEmail] = useState('');
  const [role, setRole] = useState('Reviewer');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [inviteResult, setInviteResult] = useState<InviteMemberResult | null>(null);
  const [copied, setCopied] = useState(false);

  const inviteLink = inviteResult
    ? `${typeof window !== 'undefined' ? window.location.origin : ''}/invites/${inviteResult.inviteToken}`
    : '';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const token = await getToken();
      const res = await fetch(`${API_URL}/api/v1/repositories/${repoId}/members`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, role }),
      });
      const data = await res.json();
      if (!res.ok) {
        setError(data?.error ?? 'Failed to send invite.');
      } else {
        setInviteResult(data);
        onSuccess();
      }
    } catch {
      setError('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleCopy = () => {
    navigator.clipboard.writeText(inviteLink);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative w-full max-w-md rounded-2xl border border-neutral-800 bg-neutral-950 shadow-2xl">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-neutral-800">
          <div className="flex items-center gap-2.5">
            <UserPlus size={15} className="text-blue-400" />
            <h2 className="text-sm font-semibold text-white">Invite Team Member</h2>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-neutral-500 hover:text-neutral-300 hover:bg-neutral-800 transition-colors"
          >
            <X size={15} />
          </button>
        </div>

        <div className="px-5 py-5">
          {inviteResult ? (
            /* Invite created — show link */
            <div className="space-y-4">
              <p className="text-sm text-neutral-400">
                Invite created! Share this link with your team member:
              </p>
              <div className="flex items-center gap-2 p-3 rounded-lg bg-neutral-900 border border-neutral-700">
                <code className="flex-1 text-xs text-neutral-300 break-all">{inviteLink}</code>
                <button
                  onClick={handleCopy}
                  className="shrink-0 p-1.5 rounded text-neutral-500 hover:text-blue-400 transition-colors"
                >
                  {copied ? <CheckCircle size={14} className="text-emerald-400" /> : <Copy size={14} />}
                </button>
              </div>
              <p className="text-xs text-neutral-600">
                When they visit this link while logged in, they&apos;ll gain access to the repository.
              </p>
              <button
                onClick={onClose}
                className="w-full py-2 rounded-lg bg-neutral-800 text-sm font-medium text-neutral-300 hover:bg-neutral-700 transition-colors"
              >
                Done
              </button>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-xs font-medium text-neutral-400 mb-1.5">Email Address</label>
                <input
                  type="email"
                  required
                  value={email}
                  onChange={e => setEmail(e.target.value)}
                  placeholder="teammate@example.com"
                  className="w-full bg-neutral-800 border border-neutral-700 text-neutral-200 text-sm rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-500 placeholder-neutral-600"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-neutral-400 mb-1.5">Role</label>
                <select
                  value={role}
                  onChange={e => setRole(e.target.value)}
                  className="w-full bg-neutral-800 border border-neutral-700 text-neutral-200 text-sm rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="Reviewer">Reviewer — can view reviews and re-analyze</option>
                  <option value="Viewer">Viewer — read-only access</option>
                </select>
              </div>

              {error && (
                <p className="text-xs text-red-400">{error}</p>
              )}

              <div className="flex items-center gap-2 pt-1">
                <button
                  type="button"
                  onClick={onClose}
                  className="flex-1 py-2 rounded-lg bg-neutral-800 text-sm font-medium text-neutral-300 hover:bg-neutral-700 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  className="flex-1 flex items-center justify-center gap-2 py-2 rounded-lg bg-blue-600 hover:bg-blue-500 text-sm font-semibold text-white disabled:opacity-50 transition-colors"
                >
                  {loading && <Loader2 size={13} className="animate-spin" />}
                  Send Invite
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}
