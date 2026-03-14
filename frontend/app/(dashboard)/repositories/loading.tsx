export default function RepositoriesLoading() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <div className="h-6 w-32 rounded-lg bg-neutral-800 animate-pulse" />
          <div className="h-4 w-24 rounded-lg bg-neutral-800/60 animate-pulse" />
        </div>
        <div className="h-9 w-36 rounded-lg bg-neutral-800 animate-pulse" />
      </div>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <div
            key={i}
            className="rounded-xl border border-neutral-800 bg-neutral-900 p-5 space-y-3 animate-pulse"
          >
            <div className="flex items-center justify-between">
              <div className="h-4 w-36 rounded bg-neutral-800" />
              <div className="h-5 w-16 rounded-full bg-neutral-800" />
            </div>
            <div className="h-3 w-24 rounded bg-neutral-800/60" />
            <div className="h-3 w-20 rounded bg-neutral-800/40" />
          </div>
        ))}
      </div>
    </div>
  );
}
