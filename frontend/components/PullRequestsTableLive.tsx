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
import {
  ArrowRight, GitBranch, RefreshCw,
  ChevronUp, ChevronDown, ChevronsUpDown,
  Download, ChevronLeft, ChevronRight,
} from 'lucide-react';
import * as XLSX from 'xlsx';
import { PullRequestSummaryDto, PagedResult } from '@/types/api';
import PullRequestStatusBadge from './PullRequestStatusBadge';
import TechDebtGauge from './TechDebtGauge';
import { useApiClient } from '@/hooks/useApiClient';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';

const ACTIVE_STATUSES = new Set(['Pending', 'Reviewing']);
const POLL_INTERVAL_MS = 3000;
const PAGE_SIZES = [10, 20, 50];
const ALL = '__all__';

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
  const [statusFilter, setStatusFilter] = useState(ALL);
  const [sorting, setSorting] = useState<SortingState>([]);
  const [globalFilter, setGlobalFilter] = useState('');
  const [reanalyzing, setReanalyzing] = useState<Set<string>>(new Set());
  const [reanalyzeError, setReanalyzeError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const { get, post } = useApiClient();
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // Only poll for PRs that are genuinely in-flight (active AND recently updated).
  // Stuck PRs (active > 10 min) are excluded — they won't self-resolve.
  const hasActive = prs.some(
    (pr) =>
      ACTIVE_STATUSES.has(pr.status) &&
      Date.now() - new Date(pr.updatedAt).getTime() < 10 * 60_000,
  );
  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  const fetchPage = useCallback(async (p: number, ps: number, sf: string) => {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(p), pageSize: String(ps) });
      if (sf && sf !== ALL) params.set('status', sf);
      const data = await get<PagedResult<PullRequestSummaryDto>>(
        `/api/v1/repositories/${repoId}/pull-requests?${params}`
      );
      setPrs(data.items);
      setTotalCount(data.totalCount);
    } catch {
      // silent — keep showing existing data
    } finally {
      setLoading(false);
    }
  }, [repoId]); // eslint-disable-line react-hooks/exhaustive-deps

  // Poll while active reviews exist
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
      // Optimistically set status to Reviewing so polling re-activates
      setPrs((prev) =>
        prev.map((pr) =>
          pr.id === prId
            ? { ...pr, status: 'Reviewing', updatedAt: new Date().toISOString() }
            : pr,
        ),
      );
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Re-analyze failed.';
      // "already in progress" on a stuck PR just means the API hasn't restarted yet
      const isStuckConflict = msg.toLowerCase().includes('already in progress');
      if (!isStuckConflict) {
        setReanalyzeError(msg);
      }
    } finally {
      setReanalyzing((prev) => { const next = new Set(prev); next.delete(prId); return next; });
    }
  };

  const exportExcel = () => {
    const rows = prs.map((pr) => ({
      'PR #': pr.gitHubPrNumber,
      Title: pr.title,
      'Head Branch': pr.headBranch,
      'Base Branch': pr.baseBranch,
      Author: pr.authorLogin,
      Status: pr.status,
      'Tech Debt Score': pr.latestTechDebtScore ?? '',
      Updated: new Date(pr.updatedAt).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' }),
    }));
    const ws = XLSX.utils.json_to_sheet(rows);
    ws['!cols'] = [
      { wch: 6 }, { wch: 50 }, { wch: 25 }, { wch: 20 },
      { wch: 20 }, { wch: 12 }, { wch: 16 }, { wch: 14 },
    ];
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Pull Requests');
    XLSX.writeFile(wb, 'pull-requests.xlsx');
  };

  const columns = [
    columnHelper.accessor('gitHubPrNumber', {
      header: 'PR',
      size: 60,
      cell: (info) => (
        <span className="text-[11px] font-mono text-neutral-600">#{info.getValue()}</span>
      ),
    }),
    columnHelper.accessor('title', {
      header: 'Title',
      size: 320,
      cell: (info) => {
        const pr = info.row.original;
        return (
          <div className="min-w-0">
            <p className="text-sm font-medium text-neutral-200 truncate" title={info.getValue()}>
              {info.getValue()}
            </p>
            <div className="mt-0.5 flex items-center gap-1 text-[11px] text-neutral-600">
              <GitBranch size={10} />
              <span className="font-mono truncate max-w-30">{pr.headBranch}</span>
              <ArrowRight size={9} className="shrink-0" />
              <span className="font-mono truncate max-w-20">{pr.baseBranch}</span>
              <span className="mx-1 text-neutral-700 shrink-0">·</span>
              <span className="truncate">{pr.authorLogin}</span>
            </div>
          </div>
        );
      },
    }),
    columnHelper.accessor('latestTechDebtScore', {
      header: 'Debt Score',
      size: 110,
      cell: (info) => {
        const score = info.getValue();
        const pr = info.row.original;
        if (score === null || ACTIVE_STATUSES.has(pr.status)) {
          return <span className="text-neutral-700">—</span>;
        }
        return <TechDebtGauge score={score} small />;
      },
    }),
    columnHelper.accessor('status', {
      header: 'Status',
      size: 140,
      cell: (info) => {
        const pr = info.row.original;
        if (ACTIVE_STATUSES.has(pr.status)) {
          const ageMinutes = (Date.now() - new Date(pr.updatedAt).getTime()) / 60_000;
          const isStuck = ageMinutes > 10;

          if (isStuck) {
            return (
              <span className="inline-flex items-center gap-1.5 rounded-full border border-amber-500/20 bg-amber-500/10 px-2.5 py-0.5 text-xs font-medium text-amber-400 whitespace-nowrap">
                <span className="h-1.5 w-1.5 rounded-full bg-amber-400 shrink-0" />
                Stuck — Re-analyze
              </span>
            );
          }

          return (
            <span className="inline-flex items-center gap-1.5 rounded-full border border-blue-500/20 bg-blue-500/10 px-2.5 py-0.5 text-xs font-medium text-blue-400 whitespace-nowrap">
              <span className="relative flex h-1.5 w-1.5 shrink-0">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75" />
                <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-blue-400" />
              </span>
              {pr.status === 'Pending' ? 'Queued' : 'Analyzing…'}
            </span>
          );
        }
        return <PullRequestStatusBadge status={pr.status} />;
      },
    }),
    columnHelper.accessor('updatedAt', {
      header: 'Updated',
      size: 90,
      cell: (info) => (
        <span className="text-xs text-neutral-600 whitespace-nowrap">
          {new Date(info.getValue()).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}
        </span>
      ),
    }),
    columnHelper.display({
      id: 'actions',
      header: '',
      size: 120,
      cell: (info) => {
        const pr = info.row.original;
        const isReanalyzing = reanalyzing.has(pr.id);
        const isActive = ACTIVE_STATUSES.has(pr.status);
        // Allow re-analyze on completed PRs, failed PRs, or active PRs stuck > 10 min
        const ageMinutes = (Date.now() - new Date(pr.updatedAt).getTime()) / 60_000;
        const isStuck = isActive && ageMinutes > 10;
        const canReanalyze = pr.status === 'Reviewed' || pr.status === 'Failed' || isStuck;
        return (
          <div className="flex items-center gap-2 justify-end">
            {canReanalyze && (
              <button
                onClick={() => handleReanalyze(pr.id)}
                disabled={isReanalyzing}
                className="inline-flex items-center gap-1 text-xs text-neutral-500 hover:text-neutral-300 disabled:opacity-40 transition-colors whitespace-nowrap"
              >
                <RefreshCw size={11} className={isReanalyzing ? 'animate-spin' : ''} />
                {isReanalyzing ? 'Queuing…' : 'Re-analyze'}
              </button>
            )}
            {pr.latestReviewId && !isActive && (
              <Link
                href={`/reviews/${pr.latestReviewId}`}
                className="inline-flex items-center gap-1 text-xs font-medium text-blue-400 hover:text-blue-300 transition-colors whitespace-nowrap"
              >
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

  if (prs.length === 0 && !loading && statusFilter === ALL && !globalFilter) return null;

  return (
    <div className="space-y-3">
      {reanalyzeError && (
        <div className="rounded-lg border border-red-500/20 bg-red-500/5 px-4 py-2.5">
          <p className="text-xs text-red-400">{reanalyzeError}</p>
        </div>
      )}

      {/* Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2">
          <Input
            value={globalFilter}
            onChange={(e) => setGlobalFilter(e.target.value)}
            placeholder="Search PRs…"
            className="h-8 w-44 text-xs bg-neutral-900 border-neutral-800 focus-visible:ring-neutral-700"
          />
          <Select value={statusFilter} onValueChange={handleFilterChange}>
            <SelectTrigger className="h-8 w-36 text-xs bg-neutral-900 border-neutral-800">
              <SelectValue placeholder="All statuses" />
            </SelectTrigger>
            <SelectContent className="bg-neutral-900 border-neutral-800 text-neutral-200">
              <SelectItem value={ALL} className="text-xs focus:bg-neutral-800">All statuses</SelectItem>
              <SelectItem value="Pending" className="text-xs focus:bg-neutral-800">Pending</SelectItem>
              <SelectItem value="Reviewing" className="text-xs focus:bg-neutral-800">Reviewing</SelectItem>
              <SelectItem value="Reviewed" className="text-xs focus:bg-neutral-800">Reviewed</SelectItem>
              <SelectItem value="Failed" className="text-xs focus:bg-neutral-800">Failed</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="flex items-center gap-2">
          <span className="text-xs text-neutral-600">
            {totalCount} PR{totalCount !== 1 ? 's' : ''}
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={exportExcel}
            className="h-8 text-xs bg-neutral-900 border-neutral-800 hover:bg-neutral-800 hover:text-neutral-200 text-neutral-400 gap-1.5"
          >
            <Download size={12} /> Export Excel
          </Button>
        </div>
      </div>

      {/* Table */}
      <div className="rounded-xl border border-neutral-800 overflow-hidden">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((hg) => (
              <TableRow key={hg.id} className="border-b border-neutral-800 bg-neutral-900/50 hover:bg-neutral-900/50">
                {hg.headers.map((header) => (
                  <TableHead
                    key={header.id}
                    onClick={header.column.getToggleSortingHandler()}
                    style={{ width: header.column.getSize(), cursor: header.column.getCanSort() ? 'pointer' : 'default' }}
                    className="text-[11px] font-semibold text-neutral-500 uppercase tracking-widest px-4 py-3"
                  >
                    <span className="inline-flex items-center gap-1 select-none">
                      {flexRender(header.column.columnDef.header, header.getContext())}
                      {header.column.getCanSort() && (
                        header.column.getIsSorted() === 'asc' ? <ChevronUp size={11} /> :
                        header.column.getIsSorted() === 'desc' ? <ChevronDown size={11} /> :
                        <ChevronsUpDown size={11} className="opacity-30" />
                      )}
                    </span>
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody className="divide-y divide-neutral-800/60">
            {loading ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="px-4 py-8 text-center text-xs text-neutral-600">
                  Loading…
                </TableCell>
              </TableRow>
            ) : table.getRowModel().rows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="px-4 py-8 text-center text-xs text-neutral-600">
                  No pull requests match.
                </TableCell>
              </TableRow>
            ) : (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id} className="border-b-0 hover:bg-neutral-900/40 transition-colors">
                  {row.getVisibleCells().map((cell) => (
                    <TableCell
                      key={cell.id}
                      style={{ width: cell.column.getSize() }}
                      className="px-4 py-3 overflow-hidden"
                    >
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      <div className="flex flex-wrap items-center justify-between gap-3 text-xs text-neutral-500">
        <div className="flex items-center gap-2">
          <span className="text-neutral-600">Rows per page:</span>
          <Select value={String(pageSize)} onValueChange={(v) => handlePageSizeChange(Number(v))}>
            <SelectTrigger className="h-7 w-16 text-xs bg-neutral-900 border-neutral-700 text-neutral-300 focus:ring-1 focus:ring-neutral-600 px-2">
              <SelectValue />
            </SelectTrigger>
            <SelectContent className="bg-neutral-900 border-neutral-700 min-w-16">
              {PAGE_SIZES.map((ps) => (
                <SelectItem key={ps} value={String(ps)} className="text-xs text-neutral-300 focus:bg-neutral-800 focus:text-neutral-100">
                  {ps}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <span className="text-neutral-600">
            Showing {totalCount === 0 ? 0 : (page - 1) * pageSize + 1}–{Math.min(page * pageSize, totalCount)} of {totalCount}
          </span>
        </div>

        <div className="flex items-center gap-2">
          <span>Page {page} of {totalPages}</span>
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="icon"
              className="h-7 w-7 text-neutral-500 hover:text-neutral-200 hover:bg-neutral-800 disabled:opacity-30"
              onClick={() => handlePageChange(page - 1)}
              disabled={page === 1}
            >
              <ChevronLeft size={14} />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="h-7 w-7 text-neutral-500 hover:text-neutral-200 hover:bg-neutral-800 disabled:opacity-30"
              onClick={() => handlePageChange(page + 1)}
              disabled={page >= totalPages}
            >
              <ChevronRight size={14} />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
