export default function ReviewDetailLoading() {
  return (
    <div className="space-y-6 max-w-5xl animate-pulse">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2">
        <div className="h-3 w-20 rounded bg-neutral-800" />
        <div className="h-3 w-2 rounded bg-neutral-800/60" />
        <div className="h-3 w-28 rounded bg-neutral-800" />
        <div className="h-3 w-2 rounded bg-neutral-800/60" />
        <div className="h-3 w-14 rounded bg-neutral-800" />
      </div>

      {/* Header */}
      <div className="space-y-2">
        <div className="h-3 w-24 rounded bg-neutral-800/60" />
        <div className="h-6 w-72 rounded-lg bg-neutral-800" />
        <div className="h-3 w-40 rounded bg-neutral-800/60" />
      </div>

      {/* Score + Summary card */}
      <div className="flex items-start gap-6 rounded-xl border border-neutral-800 bg-neutral-900/50 p-5">
        <div className="rounded-xl border border-neutral-800 w-32 h-24 bg-neutral-800/60 shrink-0" />
        <div className="flex-1 space-y-2">
          <div className="h-3 w-16 rounded bg-neutral-800" />
          <div className="h-4 w-full rounded bg-neutral-800" />
          <div className="h-4 w-4/5 rounded bg-neutral-800" />
          <div className="h-4 w-3/5 rounded bg-neutral-800" />
        </div>
      </div>

      {/* Issues table */}
      <div className="space-y-2">
        <div className="h-4 w-20 rounded bg-neutral-800" />
        <div className="rounded-xl border border-neutral-800 overflow-hidden">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="flex items-start gap-4 px-4 py-3 border-b border-neutral-800/60 last:border-0">
              <div className="h-5 w-16 rounded-full bg-neutral-800" />
              <div className="h-5 w-20 rounded-full bg-neutral-800" />
              <div className="flex-1 space-y-1.5">
                <div className="h-3 w-full rounded bg-neutral-800" />
                <div className="h-3 w-4/5 rounded bg-neutral-800/60" />
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
