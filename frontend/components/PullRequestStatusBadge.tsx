interface Props {
  status: string;
}

const styles: Record<string, { dot: string; text: string; bg: string; border: string }> = {
  Pending:   { dot: 'bg-neutral-500',  text: 'text-neutral-400',  bg: 'bg-neutral-800',     border: 'border-neutral-700' },
  Reviewing: { dot: 'bg-blue-400',     text: 'text-blue-400',     bg: 'bg-blue-500/10',     border: 'border-blue-500/20' },
  Reviewed:  { dot: 'bg-emerald-400',  text: 'text-emerald-400',  bg: 'bg-emerald-500/10',  border: 'border-emerald-500/20' },
  Failed:    { dot: 'bg-red-400',      text: 'text-red-400',      bg: 'bg-red-500/10',      border: 'border-red-500/20' },
};

export default function PullRequestStatusBadge({ status }: Props) {
  const s = styles[status] ?? styles.Pending;
  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-medium ${s.bg} ${s.border} ${s.text}`}>
      <span className={`w-1.5 h-1.5 rounded-full ${s.dot}`} />
      {status}
    </span>
  );
}
