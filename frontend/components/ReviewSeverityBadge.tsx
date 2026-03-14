interface Props {
  severity: string;
}

const styles: Record<string, { text: string; bg: string; border: string }> = {
  Info:     { text: 'text-sky-400',    bg: 'bg-sky-500/10',    border: 'border-sky-500/20' },
  Warning:  { text: 'text-yellow-400', bg: 'bg-yellow-500/10', border: 'border-yellow-500/20' },
  Error:    { text: 'text-orange-400', bg: 'bg-orange-500/10', border: 'border-orange-500/20' },
  Critical: { text: 'text-red-400',    bg: 'bg-red-500/10',    border: 'border-red-500/20' },
};

export default function ReviewSeverityBadge({ severity }: Props) {
  const s = styles[severity] ?? styles.Info;
  return (
    <span className={`inline-flex items-center rounded-md border px-2 py-0.5 text-xs font-semibold ${s.bg} ${s.border} ${s.text}`}>
      {severity}
    </span>
  );
}
