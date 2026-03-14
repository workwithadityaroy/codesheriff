'use client';

import { useEffect, useRef, useState, useCallback } from 'react';
import Link from 'next/link';
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  flexRender,
  createColumnHelper,
  SortingState,
} from '@tanstack/react-table';
import { ArrowRight, GitBranch, RefreshCw, ChevronUp, ChevronDown, ChevronsUpDown, Download, ChevronLeft, ChevronRight } from 'lucide-react';
import { PullRequestSummaryDto, PagedResult } from '@/types/api';
import PullRequestStatusBadge from './PullRequestStatusBadge';
import TechDebtGauge from './TechDebtGauge';
import { useApiClient } from '@/hooks/useApiClient';

const ACTIVE_STATUSES = new Set(['Pending', 'Reviewing']);
const POLL_INTERVAL_MS = 3000;
const PAGE_SIZES = [10, 20, 50];

const columnHelper = createColumnHelper<PullRequestSummaryDto>();

interface Props {
  initialPrs: PullRequestSummaryDto[];
  initialTotal: number;
  repoId: string;
}

export default function PullRequestsTableLive({ initialPrs, initialTotal, repoId }: Props) {
  const [prs, setPrs] = useState<PullRequestSummaryDto[]>(initialPrs);
  const [totalCount, setTotalCount] = useState(initialTotal);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [statusFilter, setStatusFilter] = useState('');
  const [sorting, setSorting] = useState<SortingState>([]);
  const [globalFilter, setGlobalFilter] = useState('');
  const [reanalyzing, setReanalyzing] = useState<Set<string>>(new Set());
  const [reanalyzeError, setReanalyzeError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { get, post } = useApiClient();
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const hasActive = prs.some((pr) => ACTIVE_STATUSES.has(pr.status));
  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  const fetchPage = useCallback(async (p: number, ps: number, sf: string) => {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(p), pageSize: String(ps) });
      if (sf) params.set('status', sf);
      const data = await get<PagedResult<PullRequestSummaryDto>>(
        `/api/v1/repositories/${repoId}/pull-requests?${params}`
      );
      setPrs(data.items);
      setTotalCount(data.totalCount);
    } catch {
      // silent
    } finally {
      setLoading(false);
    }
  }, [repoId]); // eslint-disable-line react-hooks/exhaustive-deps

  // server-side poll when active reviews exist
  useEffect(() => {
    if (!hasActive) {
      if (intervalRef.current) { clearInterval(intervalRef.current); intervalRef.current = null; }
      return;
    }
    intervalRef.current = setInterval(() => fetchPage(page, pageSize, statusFilter), POLL_INTERVAL_MS);
    return () => { if (intervalRef.current) clearInterval(intervalRef.current); };
  }, [hasActive, page, pageSize, statusFilter, fetchPage]);

  const handleFilterChange = (sf: string) => {
    setStatusFilter(sf);
    setPage(1);
    fetchPage(1, pageSize, sf);
  };

  const handlePageSizeChange = (ps: number) => {
    setPageSize(ps);
    setPage(1);
    fetchPage(1, ps, statusFilter);
  };

  const handlePageChange = (p: number) => {
    setPage(p);
    fetchPage(p, pageSize, statusFilter);
  };

  const handleReanalyze = async (prId: string) => {
    setReanalyzeError(null);
    setReanalyzing((prev) => new Set(prev).add(prId));
    try {
      await post(`/api/v1/pull-requests/${prId}/reanalyze`);
      setPrs((prev) => prev.map((pr) => pr.id === prId ? { ...pr, status: 'Reviewing' } : pr));
    } catch (err) {
      setReanalyzeError(err instanceof Error ? err.message : 'Re-analyze failed.');
    } finally {
      setReanalyzing((prev) => { const next = new Set(prev); next.delete(prId); return next; });
    }
  };

  const exportCsv = () => {
    const headers = ['PR#', 'Title', 'Branch', 'Author', 'Status', 'Tech Debt Score', 'Updated'];
    const rows = table.getFilteredRowModel().rows.map((row) => {
      const pr = row.original;
      return [
        pr.gitHubPrNumber,
        `"${pr.title.replace(/"/g, '""')}"`,
        pr.headBranch,
        pr.authorLogin,
        pr.status,
        pr.latestTechDebtScore ?? '',
        new Date(pr.updatedAt).toLocaleDateString(),
      ].join(',');
    });
    const csv = [headers.join(','), ...rows].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a'); a.href = url; a.download = `pull-requests.csv`; a.click();
    URL.revokeObjectURL(url);
  };

  const columns = [
    columnHelper.accessor('gitHubPrNumber', {
      header: 'PR',
      cell: (info) => <span className="text-[11px] font-mono text-neutral-600">#{info.getValue()}</span>,
    }),
    columnHelper.accessor('title', {
      header: 'Title',
      cell: (info) => {
        const pr = info.row.original;
        return (
          <div>
            <span className="text-sm font-medium text-neutral-200">{info.getValue()}</span>
            <div className="mt-0.5 flex items-center gap-1 text-[11px] text-neutral-600">
              <GitBranch size={10} />
              <span className="font-mono">{pr.headBranch}</span>
              <ArrowRight size={9} />
              <span className="font-mono">{pr.baseBranch}</span>
              <span className="mx-1 text-neutral-700">·</span>
              <span>{pr.authorLogin}</span>
            </div>
          </div>
        );
      },
    }),
    columnHelper.accessor('latestTechDebtScore', {
      header: 'Debt Score',
      cell: (info) => {
        const score = info.getValue();
        const pr = info.row.original;
        if (score === null || ACTIVE_STATUSES.has(pr.status)) return <span className="text-neutral-700">—</span>;
        return <TechDebtGauge score={score} small />;
      },
    }),
    columnHelper.accessor('status', {
      header: 'Status',
      cell: (info) => {
        const pr = info.row.original;
        const isActive = ACTIVE_STATUSES.has(pr.status);
        if (isActive) return (
          <span className="inline-flex items-center gap-1.5 rounded-full border border-blue-500/20 bg-blue-500/10 px-2.5 py-0.5 text-xs font-medium text-blue-400">
            <span className="relative flex h-1.5 w-1.5">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75" />
              <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-blue-400" />
            </span>
            {pr.status === 'Pending' ? 'Queued' : 'Analyzing…'}
          </span>
        );
        return <PullRequestStatusBadge status={pr.status} />;
      },
    }),
    columnHelper.accessor('updatedAt', {
      header: 'Updated',
      cell: (info) => <span className="text-xs text-neutral-600">{new Date(info.getValue()).toLocaleDateString()}</span>,
    }),
    columnHelper.display({
      id: 'actions',
      header: '',
      cell: (info) => {
        const pr = info.row.original;
        const isReanalyzing = reanalyzing.has(pr.id);
        const canReanalyze = pr.status === 'Reviewed' || pr.status === 'Failed';
        const isActive = ACTIVE_STATUSES.has(pr.status);
        return (
          <div className="flex items-center gap-2 justify-end">
            {canReanalyze && (
              <button onClick={() => handleReanalyze(pr.id)} disabled={isReanalyzing}
                className="inline-flex items-center gap-1 text-xs text-neutral-500 hover:text-neutral-300 disabled:opacity-40 transition-colors"
                title="Re-analyze">
                <RefreshCw size={11} className={isReanalyzing ? 'animate-spin' : ''} />
                {isReanalyzing ? 'Queuing…' : 'Re-analyze'}
              </button>
            )}
            {pr.latestReviewId && !isActive && (
              <Link href={`/reviews/${pr.latestReviewId}`}
                className="inline-flex items-center gap-1 text-xs font-medium text-blue-400 hover:text-blue-300 transition-colors">
                View <ArrowRight size={11} />
              </Link>
            )}
          </div>
        );
      },
    }),
  ];

  const table = useReactTable({
    data: prs,
    columns,
    state: { sorting, globalFilter },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    manualPagination: true,
  });

  if (prs.length === 0 && !loading && !statusFilter && !globalFilter) return null;

  return (
    <div className="space-y-3">
      {reanalyzeError && (
        <div className="rounded-lg border border-red-500/20 bg-red-500/5 px-4 py-2.5">
          <p className="text-xs text-red-400">{reanalyzeError}</p>
        </div>
      )}

      {/* Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <input
            value={globalFilter}
            onChange={(e) => setGlobalFilter(e.target.value)}
            placeholder="Search PRs…"
            className="h-8 rounded-lg border border-neutral-800 bg-neutral-900 px-3 text-xs text-neutral-300 placeholder-neutral-600 outline-none focus:border-neutral-600 w-48"
          />
          <select
            value={statusFilter}
            onChange={(e) => handleFilterChange(e.target.value)}
            className="h-8 rounded-lg border border-neutral-800 bg-neutral-900 px-2 text-xs text-neutral-300 outline-none focus:border-neutral-600"
          >
            <option value="">All statuses</option>
            <option value="Pending">Pending</option>
            <option value="Reviewing">Reviewing</option>
            <option value="Reviewed">Reviewed</option>
            <option value="Failed">Failed</option>
          </select>
        </div>

        <div className="flex items-center gap-2">
          <span className="text-xs text-neutral-600">
            {totalCount} PR{totalCount !== 1 ? 's' : ''}
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
            {loading ? (
              <tr><td colSpan={columns.length} className="px-4 py-8 text-center text-xs text-neutral-600">Loading…</td></tr>
            ) : table.getRowModel().rows.length === 0 ? (
              <tr><td colSpan={columns.length} className="px-4 py-8 text-center text-xs text-neutral-600">No pull requests match.</td></tr>
            ) : (
              table.getRowModel().rows.map((row) => (
                <tr key={row.id} className="hover:bg-neutral-900/40 transition-colors">
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id} className="px-4 py-3">
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-xs text-neutral-600">
          <span>Rows per page:</span>
          {PAGE_SIZES.map((ps) => (
            <button key={ps} onClick={() => handlePageSizeChange(ps)}
              className={`px-2 py-0.5 rounded ${pageSize === ps ? 'bg-neutral-800 text-neutral-200' : 'hover:text-neutral-400'}`}>
              {ps}
            </button>
          ))}
        </div>

        <div className="flex items-center gap-3 text-xs text-neutral-500">
          <span>Page {page} of {totalPages}</span>
          <div className="flex items-center gap-1">
            <button onClick={() => handlePageChange(page - 1)} disabled={page === 1}
              className="p-1 rounded hover:bg-neutral-800 disabled:opacity-30 disabled:cursor-not-allowed">
              <ChevronLeft size={14} />
            </button>
            <button onClick={() => handlePageChange(page + 1)} disabled={page >= totalPages}
              className="p-1 rounded hover:bg-neutral-800 disabled:opacity-30 disabled:cursor-not-allowed">
              <ChevronRight size={14} />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
