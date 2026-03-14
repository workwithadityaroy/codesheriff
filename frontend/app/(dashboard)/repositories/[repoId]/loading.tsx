export default function RepoDetailLoading() {
  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <div className="h-3 w-20 rounded bg-neutral-800 animate-pulse" />
        <div className="h-6 w-48 rounded-lg bg-neutral-800 animate-pulse" />
        <div className="h-4 w-28 rounded-lg bg-neutral-800/60 animate-pulse" />
      </div>

      <div className="rounded-xl border border-neutral-800 overflow-hidden animate-pulse">
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="flex items-center justify-between px-5 py-4 border-b border-neutral-800/60 last:border-0">
            <div className="space-y-2 flex-1">
              <div className="flex items-center gap-2">
                <div className="h-3 w-8 rounded bg-neutral-800" />
                <div className="h-4 w-56 rounded bg-neutral-800" />
              </div>
              <div className="h-3 w-40 rounded bg-neutral-800/60" />
            </div>
            <div className="ml-6 flex items-center gap-3">
              <div className="h-8 w-20 rounded-xl bg-neutral-800" />
              <div className="h-5 w-16 rounded-full bg-neutral-800" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
