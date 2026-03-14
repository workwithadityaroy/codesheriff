'use client';

import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  flexRender,
  createColumnHelper,
  SortingState,
} from '@tanstack/react-table';
import { useState } from 'react';
import { ChevronUp, ChevronDown, ChevronsUpDown, Download, ChevronLeft, ChevronRight } from 'lucide-react';
import { ReviewIssueDto } from '@/types/api';
import ReviewSeverityBadge from './ReviewSeverityBadge';

const SEVERITIES = ['Critical', 'Error', 'Warning', 'Info'];
const PAGE_SIZE = 20;

const columnHelper = createColumnHelper<ReviewIssueDto>();

const columns = [
  columnHelper.accessor('severity', {
    header: 'Severity',
    cell: (info) => <ReviewSeverityBadge severity={info.getValue()} />,
    sortingFn: (a, b) =>
      SEVERITIES.indexOf(a.original.severity) - SEVERITIES.indexOf(b.original.severity),
  }),
  columnHelper.accessor('category', {
    header: 'Category',
    cell: (info) => (
      <span className="text-xs text-neutral-500 bg-neutral-800/60 rounded px-1.5 py-0.5">
        {info.getValue()}
      </span>
    ),
  }),
  columnHelper.accessor('filePath', {
    header: 'File',
    cell: (info) => {
      const issue = info.row.original;
      return (
        <span className="text-xs font-mono text-neutral-500">
          {info.getValue() || '—'}{issue.lineNumber !== null ? `:${issue.lineNumber}` : ''}
        </span>
      );
    },
  }),
  columnHelper.accessor('description', {
    header: 'Description',
    cell: (info) => (
      <span className="text-xs text-neutral-300 leading-relaxed">{info.getValue()}</span>
    ),
  }),
  columnHelper.accessor('suggestion', {
    header: 'Suggestion',
    cell: (info) => (
      <span className="text-xs text-neutral-500 leading-relaxed">{info.getValue()}</span>
    ),
  }),
];

interface Props {
  issues: ReviewIssueDto[];
}

export default function IssuesTable({ issues }: Props) {
  const [sorting, setSorting] = useState<SortingState>([
    { id: 'severity', desc: false },
  ]);
  const [globalFilter, setGlobalFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');

  const filteredData = issues.filter((issue) => {
    if (severityFilter && issue.severity !== severityFilter) return false;
    if (categoryFilter && issue.category !== categoryFilter) return false;
    return true;
  });

  const table = useReactTable({
    data: filteredData,
    columns,
    state: { sorting, globalFilter },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: { pagination: { pageSize: PAGE_SIZE } },
  });

  const categories = Array.from(new Set(issues.map((i) => i.category))).sort();

  const exportCsv = () => {
    const headers = ['Severity', 'Category', 'File', 'Line', 'Description', 'Suggestion'];
    const rows = table.getFilteredRowModel().rows.map((row) => {
      const i = row.original;
      return [
        i.severity, i.category,
        `"${(i.filePath || '').replace(/"/g, '""')}"`,
        i.lineNumber ?? '',
        `"${i.description.replace(/"/g, '""')}"`,
        `"${i.suggestion.replace(/"/g, '""')}"`,
      ].join(',');
    });
    const csv = [headers.join(','), ...rows].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a'); a.href = url; a.download = 'issues.csv'; a.click();
    URL.revokeObjectURL(url);
  };

  if (issues.length === 0) {
    return <p className="text-sm text-neutral-600">No issues found.</p>;
  }

  const { pageIndex } = table.getState().pagination;
  const totalPages = table.getPageCount();

  return (
    <div className="space-y-3">
      {/* Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <input
            value={globalFilter}
            onChange={(e) => setGlobalFilter(e.target.value)}
            placeholder="Search issues…"
            className="h-8 rounded-lg border border-neutral-800 bg-neutral-900 px-3 text-xs text-neutral-300 placeholder-neutral-600 outline-none focus:border-neutral-600 w-44"
          />
          <select
            value={severityFilter}
            onChange={(e) => setSeverityFilter(e.target.value)}
            className="h-8 rounded-lg border border-neutral-800 bg-neutral-900 px-2 text-xs text-neutral-300 outline-none focus:border-neutral-600"
          >
            <option value="">All severities</option>
            {SEVERITIES.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="h-8 rounded-lg border border-neutral-800 bg-neutral-900 px-2 text-xs text-neutral-300 outline-none focus:border-neutral-600"
          >
            <option value="">All categories</option>
            {categories.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
        </div>

        <div className="flex items-center gap-2">
          <span className="text-xs text-neutral-600">
            {table.getFilteredRowModel().rows.length} issue{table.getFilteredRowModel().rows.length !== 1 ? 's' : ''}
          </span>
          <button onClick={exportCsv}
            className="inline-flex items-center gap-1.5 h-8 rounded-lg border border-neutral-800 bg-neutral-900 px-3 text-xs text-neutral-400 hover:text-neutral-200 hover:border-neutral-700 transition-colors">
            <Download size={12} /> Export CSV
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="rounded-xl border border-neutral-800 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            {table.getHeaderGroups().map((hg) => (
              <tr key={hg.id} className="border-b border-neutral-800 bg-neutral-900/50">
                {hg.headers.map((header) => (
                  <th key={header.id}
                    className="px-4 py-3 text-left text-[11px] font-semibold text-neutral-500 uppercase tracking-widest whitespace-nowrap"
                    onClick={header.column.getToggleSortingHandler()}
                    style={{ cursor: header.column.getCanSort() ? 'pointer' : 'default' }}>
                    <span className="inline-flex items-center gap-1">
                      {flexRender(header.column.columnDef.header, header.getContext())}
                      {header.column.getCanSort() && (
                        header.column.getIsSorted() === 'asc' ? <ChevronUp size={11} /> :
                        header.column.getIsSorted() === 'desc' ? <ChevronDown size={11} /> :
                        <ChevronsUpDown size={11} className="opacity-30" />
                      )}
                    </span>
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody className="divide-y divide-neutral-800/60">
            {table.getRowModel().rows.length === 0 ? (
              <tr><td colSpan={columns.length} className="px-4 py-6 text-center text-xs text-neutral-600">No issues match the filter.</td></tr>
            ) : (
              table.getRowModel().rows.map((row) => (
                <tr key={row.id} className="hover:bg-neutral-900/40 transition-colors">
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id} className="px-4 py-3 max-w-xs">
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination (only if needed) */}
      {totalPages > 1 && (
        <div className="flex items-center justify-end gap-3 text-xs text-neutral-500">
          <span>Page {pageIndex + 1} of {totalPages}</span>
          <div className="flex items-center gap-1">
            <button onClick={() => table.previousPage()} disabled={!table.getCanPreviousPage()}
              className="p-1 rounded hover:bg-neutral-800 disabled:opacity-30 disabled:cursor-not-allowed">
              <ChevronLeft size={14} />
            </button>
            <button onClick={() => table.nextPage()} disabled={!table.getCanNextPage()}
              className="p-1 rounded hover:bg-neutral-800 disabled:opacity-30 disabled:cursor-not-allowed">
              <ChevronRight size={14} />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
