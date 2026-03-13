interface Props {
  title: string;
  description: string;
  icon?: React.ReactNode;
}

export default function EmptyState({ title, description, icon }: Props) {
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-neutral-800 bg-neutral-900/30 py-20 text-center">
      {icon && (
        <div className="mb-4 w-12 h-12 rounded-xl bg-neutral-800 flex items-center justify-center text-neutral-600">
          {icon}
        </div>
      )}
      <p className="text-sm font-semibold text-neutral-300">{title}</p>
      <p className="mt-1 text-sm text-neutral-600 max-w-xs">{description}</p>
    </div>
  );
}
