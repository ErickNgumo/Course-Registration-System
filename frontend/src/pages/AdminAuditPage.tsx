import { useState } from "react";
import { ScrollText, Shield, ChevronRight } from "lucide-react";
import { useAuditLogs } from "@/hooks/use-queries";
import { PageHeader } from "@/components/PageHeader";
import { Pagination } from "@/components/Pagination";
import { TableSkeleton } from "@/components/LoadingState";
import { EmptyState } from "@/components/EmptyState";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatDateTime } from "@/lib/utils";
import type { AuditLogResponse } from "@/types/api";

const PAGE_SIZE = 15;

// Common entity/action values surfaced as quick filters. The backend tolerates
// any string, so these are conveniences rather than an exhaustive whitelist.
const ENTITY_FILTERS = ["all", "Course", "Enrollment", "Student"] as const;
const ACTION_FILTERS = [
  "all",
  "Created",
  "Updated",
  "Deleted",
  "Activated",
  "Deactivated",
  "Suspended",
  "Reactivated",
  "GradeAssigned",
  "Promoted",
  "Dropped",
] as const;

export default function AdminAuditPage() {
  const [entity, setEntity] = useState<string>("all");
  const [action, setAction] = useState<string>("all");
  const [adminQuery, setAdminQuery] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE);

  const [expanded, setExpanded] = useState<string | null>(null);

  const { data, isLoading, isFetching } = useAuditLogs({
    page,
    pageSize,
    entity: entity === "all" ? undefined : entity,
    action: action === "all" ? undefined : action,
    administratorId: adminQuery.trim() || undefined,
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Audit Logs"
        description="A complete, tamper-evident record of administrative actions across the system."
      />

      <div className="flex flex-col gap-3 lg:flex-row lg:items-center">
        <Select value={entity} onValueChange={(v) => { setEntity(v); setPage(1); }}>
          <SelectTrigger className="w-full lg:w-[180px]">
            <SelectValue placeholder="Entity" />
          </SelectTrigger>
          <SelectContent>
            {ENTITY_FILTERS.map((e) => (
              <SelectItem key={e} value={e}>
                {e === "all" ? "All entities" : e}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={action} onValueChange={(v) => { setAction(v); setPage(1); }}>
          <SelectTrigger className="w-full lg:w-[180px]">
            <SelectValue placeholder="Action" />
          </SelectTrigger>
          <SelectContent>
            {ACTION_FILTERS.map((a) => (
              <SelectItem key={a} value={a}>
                {a === "all" ? "All actions" : a}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <input
          type="search"
          placeholder="Administrator ID"
          className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring lg:w-[220px]"
          value={adminQuery}
          onChange={(e) => { setAdminQuery(e.target.value); setPage(1); }}
          aria-label="Filter by administrator ID"
        />
      </div>

      {isLoading ? (
        <TableSkeleton rows={8} />
      ) : data && data.items.length === 0 ? (
        <EmptyState
          icon={ScrollText}
          title="No audit entries"
          description="No administrative actions match the current filters."
        />
      ) : data ? (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Shield className="h-5 w-5 text-primary" />
              {data.totalItems} {data.totalItems === 1 ? "entry" : "entries"}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto scrollbar-thin">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-[40px]" />
                    <TableHead>Timestamp</TableHead>
                    <TableHead>Action</TableHead>
                    <TableHead>Entity</TableHead>
                    <TableHead>Entity ID</TableHead>
                    <TableHead className="text-muted-foreground">Admin</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.items.map((log) => (
                    <AuditRow
                      key={log.id}
                      log={log}
                      open={expanded === log.id}
                      onToggle={() => setExpanded((p) => (p === log.id ? null : log.id))}
                      loading={isFetching}
                    />
                  ))}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>
      ) : null}

      {data && data.totalItems > 0 && (
        <Pagination
          data={data}
          onPageChange={setPage}
          onPageSizeChange={(s) => { setPageSize(s); setPage(1); }}
          pageSizeOptions={[15, 25, 50]}
        />
      )}
    </div>
  );
}

function AuditRow({
  log,
  open,
  onToggle,
}: {
  log: AuditLogResponse;
  open: boolean;
  onToggle: () => void;
  loading?: boolean;
}) {
  const hasChanges = Boolean(log.oldValues || log.newValues);
  return (
    <>
      <TableRow className="cursor-pointer hover:bg-muted/50" onClick={onToggle}>
        <TableCell>
          {hasChanges && (
            <ChevronRight
              className={`h-4 w-4 text-muted-foreground transition-transform ${open ? "rotate-90" : ""}`}
            />
          )}
        </TableCell>
        <TableCell className="whitespace-nowrap text-muted-foreground">
          {formatDateTime(log.timestamp)}
        </TableCell>
        <TableCell>
          <Badge variant="secondary">{log.action}</Badge>
        </TableCell>
        <TableCell className="font-medium">{log.entity}</TableCell>
        <TableCell className="font-mono text-xs text-muted-foreground">{log.entityId}</TableCell>
        <TableCell className="font-mono text-xs text-muted-foreground">{log.administratorId}</TableCell>
      </TableRow>
      {open && hasChanges && (
        <TableRow className="bg-muted/30 hover:bg-muted/30">
          <TableCell colSpan={6} className="p-4">
            <div className="grid gap-4 md:grid-cols-2">
              <ValueDiff title="Old values" payload={log.oldValues} />
              <ValueDiff title="New values" payload={log.newValues} />
            </div>
          </TableCell>
        </TableRow>
      )}
    </>
  );
}

/** Pretty-prints a JSON payload stored as a string in the audit log. */
function ValueDiff({ title, payload }: { title: string; payload: string | null }) {
  return (
    <div className="rounded-md border bg-background p-3">
      <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
        {title}
      </p>
      {payload ? (
        <pre className="max-h-48 overflow-auto scrollbar-thin whitespace-pre-wrap break-words font-mono text-xs text-foreground">
          {prettyJson(payload)}
        </pre>
      ) : (
        <p className="text-xs text-muted-foreground">—</p>
      )}
    </div>
  );
}

function prettyJson(raw: string): string {
  try {
    return JSON.stringify(JSON.parse(raw), null, 2);
  } catch {
    return raw;
  }
}
