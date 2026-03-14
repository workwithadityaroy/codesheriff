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
import { useState, useMemo } from 'react';
import {
  ChevronUp, ChevronDown, ChevronsUpDown,
  Download, ChevronLeft, ChevronRight, Maximize2,
} from 'lucide-react';
import * as XLSX from 'xlsx';
import { ReviewIssueDto } from '@/types/api';
import ReviewSeverityBadge from './ReviewSeverityBadge';
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
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

const SEVERITIES = ['Critical', 'Error', 'Warning', 'Info'];
const PAGE_SIZES = [10, 20, 50];
const ALL = '__all__';

const columnHelper = createColumnHelper<ReviewIssueDto>();

// ─── Issue Detail Modal ───────────────────────────────────────────────────────

interface IssueDetailModalProps {
  issue: ReviewIssueDto | null;
  onClose: () => void;
}

function IssueDetailModal({ issue, onClose }: IssueDetailModalProps) {
  return (
    <Dialog open={!!issue} onOpenChange={(open) => !open && onClose()}>
      <DialogContent
        className="
          max-w-2xl w-full
          bg-neutral-950 border border-neutral-800 text-neutral-200
          max-h-[85vh] flex flex-col
          overflow-hidden
        "
      >
        {/* Fixed header */}
        <DialogHeader className="shrink-0 pb-3 border-b border-neutral-800">
          <DialogTitle className="flex items-center gap-2 text-sm font-semibold text-neutral-200">
            {issue && <ReviewSeverityBadge severity={issue.severity} />}
            <span className="text-neutral-300 font-medium">{issue?.category}</span>
          </DialogTitle>
        </DialogHeader>

        {/* Scrollable body */}
        <div className="flex-1 overflow-y-auto pr-1 space-y-5 pt-4">
          {issue?.filePath && (
            <div className="rounded-lg bg-neutral-900 border border-neutral-800 px-4 py-3">
              <p className="text-[10px] font-semibold uppercase tracking-widest text-neutral-600 mb-1">
                Location
              </p>
              <p className="text-xs font-mono text-neutral-400 break-all">
                {issue.filePath}{issue.lineNumber !== null ? `:${issue.lineNumber}` : ''}
              </p>
            </div>
          )}

          {issue?.description && (
            <div>
              <p className="text-[10px] font-semibold uppercase tracking-widest text-neutral-600 mb-2">
                Description
              </p>
              <p className="text-sm text-neutral-200 leading-relaxed">{issue.description}</p>
            </div>
          )}

          {issue?.suggestion && (
            <div>
              <p className="text-[10px] font-semibold uppercase tracking-widest text-neutral-600 mb-2">
                Suggestion
              </p>
              <p className="text-sm text-neutral-400 leading-relaxed">{issue.suggestion}</p>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}

// ─── Main Table ───────────────────────────────────────────────────────────────

interface Props {
  issues: ReviewIssueDto[];
}

export default function IssuesTable({ issues }: Props) {
  const [sorting, setSorting] = useState<SortingState>([{ id: 'severity', desc: false }]);
  const [globalFilter, setGlobalFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState(ALL);
  const [categoryFilter, setCategoryFilter] = useState(ALL);
  const [pageSize, setPageSize] = useState(20);
  const [selectedIssue, setSelectedIssue] = useState<ReviewIssueDto | null>(null);

  const categories = useMemo(
    () => Array.from(new Set(issues.map((i) => i.category))).sort(),
    [issues],
  );

  const filteredData = useMemo(
    () =>
      issues.filter((issue) => {
        if (severityFilter !== ALL && issue.severity !== severityFilter) return false;
        if (categoryFilter !== ALL && issue.category !== categoryFilter) return false;
        return true;
      }),
    [issues, severityFilter, categoryFilter],
  );

  const columns = useMemo(
    () => [
      columnHelper.accessor('severity', {
        header: 'Severity',
        size: 110,
        cell: (info) => <ReviewSeverityBadge severity={info.getValue()} />,
        sortingFn: (a, b) =>
          SEVERITIES.indexOf(a.original.severity) - SEVERITIES.indexOf(b.original.severity),
      }),
      columnHelper.accessor('category', {
        header: 'Category',
        size: 130,
        cell: (info) => (
          <span className="inline-block text-xs text-neutral-300 bg-neutral-800 border border-neutral-700 rounded px-2 py-0.5 whitespace-nowrap">
            {info.getValue()}
          </span>
        ),
      }),
      columnHelper.accessor('filePath', {
        header: 'File',
        size: 220,
        cell: (info) => {
          const issue = info.row.original;
          const path = info.getValue() || '—';
          const display = path.length > 40 ? `…${path.slice(-39)}` : path;
          return (
            <span className="text-xs font-mono text-neutral-500" title={path}>
              {display}
              {issue.lineNumber !== null ? `:${issue.lineNumber}` : ''}
            </span>
          );
        },
      }),
      columnHelper.accessor('description', {
        header: 'Description',
        size: 300,
        enableGlobalFilter: true,
        cell: (info) => {
          const text = info.getValue();
          const truncated = text.length > 90;
          return (
            <span className="text-xs text-neutral-300 leading-relaxed">
              {truncated ? `${text.slice(0, 90)}…` : text}
            </span>
          );
        },
      }),
      columnHelper.accessor('suggestion', {
        header: 'Suggestion',
        size: 280,
        cell: (info) => {
          const text = info.getValue();
          const truncated = text.length > 90;
          return (
            <div className="flex items-start justify-between gap-2">
              <span className="text-xs text-neutral-500 leading-relaxed">
                {truncated ? `${text.slice(0, 90)}…` : text}
              </span>
              <button
                onClick={(e) => { e.stopPropagation(); setSelectedIssue(info.row.original); }}
                className="shrink-0 p-0.5 text-neutral-700 hover:text-blue-400 transition-colors"
                title="View full detail"
              >
                <Maximize2 size={12} />
              </button>
            </div>
          );
        },
      }),
    ],
    [],
  );

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
    initialState: { pagination: { pageSize } },
  });

  // Keep table in sync when pageSize state changes
  const handlePageSizeChange = (ps: string) => {
    const val = Number(ps);
    setPageSize(val);
    table.setPageSize(val);
    table.setPageIndex(0);
  };

  const exportExcel = () => {
    const rows = table.getFilteredRowModel().rows.map((row) => {
      const i = row.original;
      return {
        Severity: i.severity,
        Category: i.category,
        File: i.filePath || '',
        Line: i.lineNumber ?? '',
        Description: i.description,
        Suggestion: i.suggestion,
      };
    });

    const ws = XLSX.utils.json_to_sheet(rows);

    // Column widths
    ws['!cols'] = [
      { wch: 12 },  // Severity
      { wch: 16 },  // Category
      { wch: 55 },  // File
      { wch: 6 },   // Line
      { wch: 60 },  // Description
      { wch: 60 },  // Suggestion
    ];

    // Bold header row
    const headerRange = XLSX.utils.decode_range(ws['!ref'] ?? 'A1');
    for (let c = headerRange.s.c; c <= headerRange.e.c; c++) {
      const addr = XLSX.utils.encode_cell({ r: 0, c });
      if (ws[addr]) ws[addr].s = { font: { bold: true } };
    }

    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Issues');
    XLSX.writeFile(wb, 'code-review-issues.xlsx');
  };

  if (issues.length === 0) {
    return <p className="text-sm text-neutral-600">No issues found.</p>;
  }

  const { pageIndex } = table.getState().pagination;
  const totalPages = table.getPageCount();
  const filteredCount = table.getFilteredRowModel().rows.length;
  const start = pageIndex * pageSize + 1;
  const end = Math.min((pageIndex + 1) * pageSize, filteredCount);

  return (
    <>
      <IssueDetailModal issue={selectedIssue} onClose={() => setSelectedIssue(null)} />

      <div className="space-y-3 w-full">
        {/* Toolbar */}
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Input
              value={globalFilter}
              onChange={(e) => setGlobalFilter(e.target.value)}
              placeholder="Search issues…"
              className="h-8 w-44 text-xs bg-neutral-900 border-neutral-700 text-neutral-200 placeholder:text-neutral-600 focus-visible:ring-1 focus-visible:ring-neutral-600"
            />
            <Select value={severityFilter} onValueChange={(v) => { setSeverityFilter(v); table.setPageIndex(0); }}>
              <SelectTrigger className="h-8 w-36 text-xs bg-neutral-900 border-neutral-700 text-neutral-300 focus:ring-1 focus:ring-neutral-600">
                <SelectValue />
              </SelectTrigger>
              <SelectContent className="bg-neutral-900 border-neutral-700">
                <SelectItem value={ALL} className="text-xs text-neutral-300 focus:bg-neutral-800 focus:text-neutral-100">All severities</SelectItem>
                {SEVERITIES.map((s) => (
                  <SelectItem key={s} value={s} className="text-xs text-neutral-300 focus:bg-neutral-800 focus:text-neutral-100">{s}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={categoryFilter} onValueChange={(v) => { setCategoryFilter(v); table.setPageIndex(0); }}>
              <SelectTrigger className="h-8 w-36 text-xs bg-neutral-900 border-neutral-700 text-neutral-300 focus:ring-1 focus:ring-neutral-600">
                <SelectValue />
              </SelectTrigger>
              <SelectContent className="bg-neutral-900 border-neutral-700">
                <SelectItem value={ALL} className="text-xs text-neutral-300 focus:bg-neutral-800 focus:text-neutral-100">All categories</SelectItem>
                {categories.map((c) => (
                  <SelectItem key={c} value={c} className="text-xs text-neutral-300 focus:bg-neutral-800 focus:text-neutral-100">{c}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex items-center gap-2">
            <span className="text-xs text-neutral-500">
              {filteredCount} issue{filteredCount !== 1 ? 's' : ''}
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={exportExcel}
              className="h-8 text-xs bg-neutral-900 border-neutral-700 text-neutral-400 hover:bg-neutral-800 hover:text-neutral-100 hover:border-neutral-600 gap-1.5"
            >
              <Download size={12} /> Export Excel
            </Button>
          </div>
        </div>

        {/* Table */}
        <div className="w-full rounded-xl border border-neutral-800 overflow-x-auto">
          <Table className="min-w-225">
            <TableHeader>
              {table.getHeaderGroups().map((hg) => (
                <TableRow key={hg.id} className="border-b border-neutral-800 bg-neutral-900/60 hover:bg-neutral-900/60">
                  {hg.headers.map((header) => (
                    <TableHead
                      key={header.id}
                      onClick={header.column.getToggleSortingHandler()}
                      style={{
                        width: header.column.getSize(),
                        cursor: header.column.getCanSort() ? 'pointer' : 'default',
                      }}
                      className="px-4 py-3 text-[11px] font-semibold text-neutral-500 uppercase tracking-widest whitespace-nowrap"
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
            <TableBody>
              {table.getRowModel().rows.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={columns.length} className="px-4 py-8 text-center text-xs text-neutral-600">
                    No issues match the filter.
                  </TableCell>
                </TableRow>
              ) : (
                table.getRowModel().rows.map((row) => (
                  <TableRow
                    key={row.id}
                    onClick={() => setSelectedIssue(row.original)}
                    className="border-b border-neutral-800/60 hover:bg-neutral-900/50 cursor-pointer transition-colors"
                  >
                    {row.getVisibleCells().map((cell) => (
                      <TableCell
                        key={cell.id}
                        style={{ width: cell.column.getSize() }}
                        className="px-4 py-3 align-top"
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
          {/* Rows per page */}
          <div className="flex items-center gap-2">
            <span className="text-neutral-600">Rows per page:</span>
            <Select value={String(pageSize)} onValueChange={handlePageSizeChange}>
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
              Showing {filteredCount === 0 ? 0 : start}–{end} of {filteredCount}
            </span>
          </div>

          {/* Page nav */}
          <div className="flex items-center gap-2">
            <span>Page {pageIndex + 1} of {totalPages}</span>
            <div className="flex items-center gap-1">
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-neutral-500 hover:text-neutral-200 hover:bg-neutral-800 disabled:opacity-30"
                onClick={() => table.previousPage()}
                disabled={!table.getCanPreviousPage()}
              >
                <ChevronLeft size={14} />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7 text-neutral-500 hover:text-neutral-200 hover:bg-neutral-800 disabled:opacity-30"
                onClick={() => table.nextPage()}
                disabled={!table.getCanNextPage()}
              >
                <ChevronRight size={14} />
              </Button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
