'use client';

import { useState, useTransition } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { registerRepo } from '@/app/actions/registerRepo';
import { ArrowLeft, FolderGit2, HelpCircle, Loader2 } from 'lucide-react';

export default function NewRepositoryPage() {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [owner, setOwner] = useState('');
  const [name, setName] = useState('');
  const [installationId, setInstallationId] = useState('');
  const [error, setError] = useState<string | null>(null);

  const fullName = owner && name ? `${owner}/${name}` : null;

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    const instId = parseInt(installationId, 10);
    if (!owner.trim() || !name.trim()) {
      setError('Owner and repository name are required.');
      return;
    }
    if (isNaN(instId) || instId <= 0) {
      setError('Installation ID must be a positive number.');
      return;
    }

    startTransition(async () => {
      try {
        await registerRepo(owner.trim(), name.trim(), instId);
        router.push('/repositories');
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Something went wrong.');
      }
    });
  }

  return (
    <div className="max-w-lg space-y-6">
      {/* Back link */}
      <Link
        href="/repositories"
        className="inline-flex items-center gap-1.5 text-xs text-neutral-500 hover:text-neutral-300 transition-colors"
      >
        <ArrowLeft size={13} />
        Back to Repositories
      </Link>

      {/* Header */}
      <div>
        <div className="flex items-center gap-2 mb-1">
          <div className="w-8 h-8 rounded-lg bg-blue-500/10 border border-blue-500/20 flex items-center justify-center">
            <FolderGit2 size={15} className="text-blue-400" />
          </div>
          <h1 className="text-xl font-semibold text-white tracking-tight">Add Repository</h1>
        </div>
        <p className="mt-1 text-sm text-neutral-500">
          Connect a GitHub repository to start receiving AI code reviews on pull requests.
        </p>
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="rounded-xl border border-neutral-800 bg-neutral-900/50 p-5 space-y-4">

          {/* Owner */}
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-neutral-400 uppercase tracking-wider">
              Owner
            </label>
            <input
              type="text"
              value={owner}
              onChange={(e) => setOwner(e.target.value)}
              placeholder="e.g. octocat"
              disabled={isPending}
              className="w-full rounded-lg border border-neutral-700 bg-neutral-800 px-3 py-2 text-sm text-neutral-200 placeholder-neutral-600 outline-none focus:border-blue-500/60 focus:ring-1 focus:ring-blue-500/20 disabled:opacity-50 transition-colors"
            />
          </div>

          {/* Repository Name */}
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-neutral-400 uppercase tracking-wider">
              Repository Name
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. my-api"
              disabled={isPending}
              className="w-full rounded-lg border border-neutral-700 bg-neutral-800 px-3 py-2 text-sm text-neutral-200 placeholder-neutral-600 outline-none focus:border-blue-500/60 focus:ring-1 focus:ring-blue-500/20 disabled:opacity-50 transition-colors"
            />
          </div>

          {/* Full name preview */}
          {fullName && (
            <div className="rounded-lg border border-neutral-800 bg-neutral-950/60 px-3 py-2 flex items-center gap-2">
              <FolderGit2 size={13} className="text-neutral-600 shrink-0" />
              <span className="text-xs font-mono text-neutral-400">{fullName}</span>
            </div>
          )}

          {/* Installation ID */}
          <div className="space-y-1.5">
            <div className="flex items-center gap-1.5">
              <label className="text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                GitHub App Installation ID
              </label>
              <a
                href="https://github.com/settings/installations"
                target="_blank"
                rel="noopener noreferrer"
                className="text-neutral-600 hover:text-neutral-400 transition-colors"
                title="Find your installation ID in GitHub → Settings → Installations"
              >
                <HelpCircle size={13} />
              </a>
            </div>
            <input
              type="number"
              value={installationId}
              onChange={(e) => setInstallationId(e.target.value)}
              placeholder="e.g. 12345678"
              disabled={isPending}
              min={1}
              className="w-full rounded-lg border border-neutral-700 bg-neutral-800 px-3 py-2 text-sm text-neutral-200 placeholder-neutral-600 outline-none focus:border-blue-500/60 focus:ring-1 focus:ring-blue-500/20 disabled:opacity-50 transition-colors"
            />
            <p className="text-[11px] text-neutral-600">
              Found at GitHub → Settings → Applications → CodeSheriff → Configure → URL contains the ID.
            </p>
          </div>
        </div>

        {/* Error */}
        {error && (
          <div className="rounded-lg border border-red-500/20 bg-red-500/5 px-4 py-3">
            <p className="text-sm text-red-400">{error}</p>
          </div>
        )}

        {/* Actions */}
        <div className="flex items-center gap-3">
          <button
            type="submit"
            disabled={isPending}
            className="inline-flex items-center gap-2 rounded-lg bg-blue-600 hover:bg-blue-500 disabled:opacity-50 disabled:cursor-not-allowed px-4 py-2 text-sm font-medium text-white transition-colors"
          >
            {isPending && <Loader2 size={14} className="animate-spin" />}
            {isPending ? 'Connecting...' : 'Connect Repository'}
          </button>
          <Link
            href="/repositories"
            className="text-sm text-neutral-500 hover:text-neutral-300 transition-colors"
          >
            Cancel
          </Link>
        </div>
      </form>
    </div>
  );
}
