import { useState, useMemo } from "react";
import { ArrowUp, ArrowDown, ArrowUpDown } from "lucide-react";
import { cn } from "@/lib/utils";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

export interface Column<T> {
  id: string;
  header: React.ReactNode;
  /** When provided and sortable, this key is sent to the backend sort param. */
  sortKey?: string;
  cell: (row: T) => React.ReactNode;
  className?: string;
  headerClassName?: string;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  rowKey: (row: T) => string;
  onRowClick?: (row: T) => void;
  sortable?: boolean;
  sortKey?: string;
  sortDir?: "asc" | "desc";
  onSort?: (sortKey: string) => void;
  loading?: boolean;
  emptyState?: React.ReactNode;
}

export function DataTable<T>({
  columns,
  data,
  rowKey,
  onRowClick,
  sortable = false,
  sortKey,
  sortDir,
  onSort,
  loading = false,
  emptyState,
}: DataTableProps<T>) {
  const [internalSort, setInternalSort] = useState<{ key: string; dir: "asc" | "desc" } | null>(
    null,
  );

  // When sorting is purely visual (no controlled backend sort), sort locally.
  const displayRows = useMemo(() => {
    if (sortable && onSort) return data; // controlled — leave to parent
    if (!sortable || !internalSort) return data;
    const col = columns.find((c) => c.sortKey === internalSort.key);
    if (!col) return data;
    const accessor = (row: T): unknown => {
      const node = col.cell(row);
      if (node == null || typeof node === "boolean" || typeof node === "object") return "";
      return node;
    };
    const copy = [...data];
    copy.sort((a, b) => {
      const av = accessor(a);
      const bv = accessor(b);
      if (av === bv) return 0;
      const cmp = String(av).localeCompare(String(bv), undefined, { numeric: true });
      return internalSort.dir === "asc" ? cmp : -cmp;
    });
    return copy;
  }, [data, columns, sortable, onSort, internalSort]);

  const activeSortKey = sortable && onSort ? sortKey : internalSort?.key;
  const activeSortDir = sortable && onSort ? sortDir : internalSort?.dir;

  const handleHeaderClick = (col: Column<T>) => {
    if (!sortable || !col.sortKey) return;
    if (onSort) {
      onSort(col.sortKey);
      return;
    }
    setInternalSort((prev) => {
      if (!prev || prev.key !== col.sortKey) return { key: col.sortKey!, dir: "asc" };
      return { key: col.sortKey!, dir: prev.dir === "asc" ? "desc" : "asc" };
    });
  };

  const SortIcon = ({ col }: { col: Column<T> }) => {
    if (!sortable || !col.sortKey) return null;
    const isActive = activeSortKey === col.sortKey;
    if (!isActive) return <ArrowUpDown className="ml-1 inline h-3 w-3 opacity-40" />;
    return activeSortDir === "asc" ? (
      <ArrowUp className="ml-1 inline h-3 w-3" />
    ) : (
      <ArrowDown className="ml-1 inline h-3 w-3" />
    );
  };

  if (loading) return null;

  if (data.length === 0) {
    return <>{emptyState}</>;
  }

  return (
    <div className="rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow>
            {columns.map((col) => (
              <TableHead
                key={col.id}
                className={cn(
                  col.headerClassName,
                  sortable && col.sortKey && "cursor-pointer select-none",
                )}
                onClick={() => handleHeaderClick(col)}
                aria-sort={
                  activeSortKey === col.sortKey
                    ? activeSortDir === "asc"
                      ? "ascending"
                      : "descending"
                    : col.sortKey
                      ? "none"
                      : undefined
                }
              >
                <span className="inline-flex items-center">
                  {col.header}
                  <SortIcon col={col} />
                </span>
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {displayRows.map((row) => (
            <TableRow
              key={rowKey(row)}
              className={cn(onRowClick && "cursor-pointer")}
              onClick={onRowClick ? () => onRowClick(row) : undefined}
            >
              {columns.map((col) => (
                <TableCell key={col.id} className={col.className}>
                  {col.cell(row)}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
