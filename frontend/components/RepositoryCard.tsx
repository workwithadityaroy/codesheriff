import { GitBranch, ChevronRight } from 'lucide-react';
import { RepositoryDto } from '@/types/api';

interface Props {
  repo: RepositoryDto;
}

export default function RepositoryCard({ repo }: Props) {
  return (
    <div className="group rounded-xl border border-neutral-800 bg-neutral-900 p-5 hover:border-neutral-700 hover:bg-neutral-800/50 transition-all duration-150 cursor-pointer">
      <div className="flex items-start justify-between gap-3">
        <div className="w-9 h-9 rounded-lg bg-neutral-800 border border-neutral-700 flex items-center justify-center shrink-0 group-hover:border-neutral-600 transition-colors">
          <GitBranch size={16} className="text-neutral-400" />
        </div>
        <ChevronRight size={15} className="mt-1 text-neutral-700 group-hover:text-neutral-500 transition-colors" />
      </div>

      <div className="mt-3">
        <p className="text-sm font-semibold text-neutral-200 truncate">{repo.name}</p>
        <p className="text-xs text-neutral-600 mt-0.5">{repo.owner}</p>
      </div>

      <div className="mt-4 pt-4 border-t border-neutral-800 flex items-center gap-2">
        <span className="inline-flex items-center gap-1.5 text-xs text-neutral-600">
          <span className="w-1.5 h-1.5 rounded-full bg-emerald-500" />
          Active
        </span>
        <span className="ml-auto text-xs text-neutral-700">
          {new Date(repo.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
        </span>
      </div>
    </div>
  );
}
