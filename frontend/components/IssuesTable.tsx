import { ReviewIssueDto } from '@/types/api';
import ReviewSeverityBadge from './ReviewSeverityBadge';

interface Props {
  issues: ReviewIssueDto[];
}

export default function IssuesTable({ issues }: Props) {
  if (issues.length === 0) {
    return <p className="text-sm text-neutral-600">No issues found.</p>;
  }

  return (
    <div className="rounded-xl border border-neutral-800 overflow-hidden">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-neutral-800 bg-neutral-900/50">
            <th className="px-4 py-3 text-left text-[11px] font-semibold text-neutral-500 uppercase tracking-widest">Severity</th>
            <th className="px-4 py-3 text-left text-[11px] font-semibold text-neutral-500 uppercase tracking-widest">Category</th>
            <th className="px-4 py-3 text-left text-[11px] font-semibold text-neutral-500 uppercase tracking-widest">File</th>
            <th className="px-4 py-3 text-left text-[11px] font-semibold text-neutral-500 uppercase tracking-widest">Description</th>
            <th className="px-4 py-3 text-left text-[11px] font-semibold text-neutral-500 uppercase tracking-widest">Suggestion</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-neutral-800/60">
          {issues.map((issue) => (
            <tr key={issue.id} className="hover:bg-neutral-900/40 transition-colors">
              <td className="px-4 py-3 whitespace-nowrap">
                <ReviewSeverityBadge severity={issue.severity} />
              </td>
              <td className="px-4 py-3">
                <span className="text-xs text-neutral-500 bg-neutral-800/60 rounded px-1.5 py-0.5">{issue.category}</span>
              </td>
              <td className="px-4 py-3">
                <span className="text-xs font-mono text-neutral-500">
                  {issue.filePath || '—'}{issue.lineNumber !== null ? `:${issue.lineNumber}` : ''}
                </span>
              </td>
              <td className="px-4 py-3 text-neutral-300 max-w-xs text-xs leading-relaxed">{issue.description}</td>
              <td className="px-4 py-3 text-neutral-500 max-w-xs text-xs leading-relaxed">{issue.suggestion}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
