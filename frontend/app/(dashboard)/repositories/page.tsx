import Link from 'next/link';
import { fetchApi } from '@/lib/api';
import { RepositoryDto } from '@/types/api';
import RepositoryCard from '@/components/RepositoryCard';
import EmptyState from '@/components/EmptyState';
import { FolderGit2, Plus } from 'lucide-react';

export default async function RepositoriesPage() {
  let repos: RepositoryDto[] = [];
  let error: string | null = null;

  try {
    repos = await fetchApi<RepositoryDto[]>('/api/v1/repositories');
  } catch {
    error = 'Failed to load repositories.';
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-white tracking-tight">Repositories</h1>
          <p className="mt-0.5 text-sm text-neutral-500">
            {repos.length > 0 ? `${repos.length} connected` : 'No repositories connected yet'}
          </p>
        </div>
        <Link
          href="/repositories/new"
          className="inline-flex items-center gap-1.5 rounded-lg bg-blue-600 hover:bg-blue-500 px-3.5 py-2 text-sm font-medium text-white transition-colors"
        >
          <Plus size={14} />
          Add Repository
        </Link>
      </div>

      {error && (
        <div className="rounded-xl border border-red-500/20 bg-red-500/5 px-5 py-4">
          <p className="text-sm text-red-400">{error}</p>
        </div>
      )}

      {!error && repos.length === 0 && (
        <EmptyState
          title="No repositories yet"
          description="Register a GitHub repository to start getting AI code reviews."
          icon={<FolderGit2 size={22} className="text-blue-400" />}
        />
      )}

      {repos.length > 0 && (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {repos.map((repo) => (
            <Link key={repo.id} href={`/repositories/${repo.id}`}>
              <RepositoryCard repo={repo} />
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
