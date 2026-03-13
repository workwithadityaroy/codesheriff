export default function DashboardLoading() {
  return (
    <div className="space-y-8">
      <div className="space-y-1">
        <div className="h-6 w-24 rounded-lg bg-neutral-800 animate-pulse" />
        <div className="h-4 w-48 rounded-lg bg-neutral-800/60 animate-pulse" />
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <div
            key={i}
            className="rounded-xl border border-neutral-800 bg-neutral-900 p-5 space-y-3 animate-pulse"
          >
            <div className="flex items-center justify-between">
              <div className="h-3 w-20 rounded bg-neutral-800" />
              <div className="h-8 w-8 rounded-lg bg-neutral-800" />
            </div>
            <div className="h-8 w-16 rounded bg-neutral-800" />
            <div className="h-3 w-32 rounded bg-neutral-800/60" />
          </div>
        ))}
      </div>
    </div>
  );
}
