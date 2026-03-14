interface Props {
  label: string;
  value: string | number;
  suffix?: string;
  icon: React.ReactNode;
  accent?: 'blue' | 'violet' | 'emerald' | 'red';
  description?: string;
}

const accents = {
  blue:    { bg: 'from-blue-500/10 to-blue-600/5',    icon: 'bg-blue-500/10 text-blue-400',      val: 'text-white' },
  violet:  { bg: 'from-violet-500/10 to-violet-600/5', icon: 'bg-violet-500/10 text-violet-400',  val: 'text-white' },
  emerald: { bg: 'from-emerald-500/10 to-emerald-600/5', icon: 'bg-emerald-500/10 text-emerald-400', val: 'text-white' },
  red:     { bg: 'from-red-500/10 to-red-600/5',      icon: 'bg-red-500/10 text-red-400',         val: 'text-red-400' },
};

export default function StatsCard({ label, value, suffix, icon, accent = 'blue', description }: Props) {
  const a = accents[accent];
  return (
    <div className="relative rounded-xl border border-neutral-800 bg-neutral-900 overflow-hidden p-5">
      <div className={`absolute inset-0 bg-linear-to-br ${a.bg} pointer-events-none`} />
      <div className="relative flex items-start justify-between gap-3">
        <div className="flex-1 min-w-0">
          <p className="text-[11px] font-semibold text-neutral-500 uppercase tracking-widest mb-3">{label}</p>
          <p className={`text-3xl font-bold tracking-tight ${a.val}`}>
            {value}
            {suffix && <span className="text-base font-normal text-neutral-600 ml-1">{suffix}</span>}
          </p>
          {description && <p className="mt-1.5 text-xs text-neutral-600">{description}</p>}
        </div>
        <div className={`shrink-0 w-9 h-9 rounded-lg flex items-center justify-center ${a.icon}`}>
          {icon}
        </div>
      </div>
    </div>
  );
}
