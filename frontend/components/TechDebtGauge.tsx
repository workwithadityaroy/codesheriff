interface Props {
  score: number;
  small?: boolean;
}

function getLevel(score: number) {
  if (score <= 30) return { label: 'Healthy',  color: 'text-emerald-400', bg: 'bg-emerald-500/10', border: 'border-emerald-500/20', bar: 'bg-emerald-500' };
  if (score <= 60) return { label: 'Moderate', color: 'text-yellow-400',  bg: 'bg-yellow-500/10',  border: 'border-yellow-500/20',  bar: 'bg-yellow-500' };
  if (score <= 80) return { label: 'High',     color: 'text-orange-400',  bg: 'bg-orange-500/10',  border: 'border-orange-500/20',  bar: 'bg-orange-500' };
  return              { label: 'Critical', color: 'text-red-400',     bg: 'bg-red-500/10',     border: 'border-red-500/20',     bar: 'bg-red-500' };
}

export default function TechDebtGauge({ score, small }: Props) {
  const level = getLevel(score);

  if (small) {
    return (
      <span className={`inline-flex items-center gap-1.5 rounded-md border px-2 py-0.5 text-xs font-semibold ${level.bg} ${level.border} ${level.color}`}>
        <span className={`w-1.5 h-1.5 rounded-full ${level.bar}`} />
        {score}
      </span>
    );
  }

  return (
    <div className={`rounded-xl border p-4 min-w-30 ${level.bg} ${level.border}`}>
      <p className={`text-4xl font-bold tabular-nums ${level.color}`}>{score}</p>
      <p className="text-neutral-600 text-xs mt-0.5">/ 100</p>
      <div className="mt-3 h-1 rounded-full bg-neutral-800">
        <div className={`h-1 rounded-full transition-all ${level.bar}`} style={{ width: `${score}%` }} />
      </div>
      <p className={`mt-1.5 text-xs font-semibold ${level.color}`}>{level.label}</p>
    </div>
  );
}
