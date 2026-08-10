import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Search, Users } from "lucide-react";
import { useAdminStudents, useChangeStudentStatus } from "@/hooks/use-queries";
import { getApiErrorMessage } from "@/lib/api-error";
import { buildSort } from "@/lib/format";
import { PageHeader } from "@/components/PageHeader";
import { Pagination } from "@/components/Pagination";
import { TableSkeleton } from "@/components/LoadingState";
import { EmptyState } from "@/components/EmptyState";
import { DataTable, type Column } from "@/components/DataTable";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { StudentStatusBadge } from "@/components/SemesterProgress";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useDebounce } from "@/hooks/use-debounce";
import { useToast } from "@/hooks/use-toast";
import type { StudentAdministrationResponse } from "@/types/api";

const PAGE_SIZE = 10;

export default function AdminStudentsPage() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const changeStatus = useChangeStudentStatus();

  const [query, setQuery] = useState("");
  const debouncedQuery = useDebounce(query, 350);
  const [status, setStatus] = useState("all");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE);
  const [sortBy, setSortBy] = useState<string | undefined>(undefined);
  const [confirm, setConfirm] = useState<{ id: string; status: string; name: string } | null>(null);

  const { data, isLoading, isFetching } = useAdminStudents({
    page,
    pageSize,
    status: status === "all" ? undefined : status,
    search: debouncedQuery || undefined,
    sortBy,
  });

  const handleSort = (key: string) => {
    setSortBy((prev) => {
      if (!prev || !prev.startsWith(`${key}:`)) return buildSort(key, "asc");
      const dir = prev.endsWith(":desc") ? "asc" : "desc";
      return buildSort(key, dir);
    });
  };

  const columns: Column<StudentAdministrationResponse>[] = [
    { id: "name", sortKey: "name", header: "Name", cell: (s) => (
      <span className="font-medium">{s.firstName} {s.lastName}</span>
    )},
    { id: "number", sortKey: "studentnumber", header: "Student #", cell: (s) => s.studentNumber },
    { id: "email", sortKey: "email", header: "Email", cell: (s) => (
      <span className="text-muted-foreground">{s.email}</span>
    )},
    { id: "status", sortKey: "status", header: "Status", cell: (s) => <StudentStatusBadge status={s.status} /> },
    { id: "actions", header: "", className: "text-right", cell: (s) => {
      const isSuspend = s.status !== "Suspended";
      return (
        <div className="flex justify-end gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={(e) => {
              e.stopPropagation();
              setConfirm({
                id: s.id,
                status: isSuspend ? "Suspended" : "Active",
                name: `${s.firstName} ${s.lastName}`,
              });
            }}
          >
            {isSuspend ? "Suspend" : "Reactivate"}
          </Button>
        </div>
      );
    }},
  ];

  const handleConfirm = async () => {
    if (!confirm) return;
    try {
      await changeStatus.mutateAsync({ id: confirm.id, status: confirm.status });
      toast({
        title: confirm.status === "Suspended" ? "Student suspended" : "Student reactivated",
        description: `${confirm.name} is now ${confirm.status.toLowerCase()}.`,
      });
    } catch (error) {
      toast({ variant: "destructive", title: "Update failed", description: getApiErrorMessage(error) });
    }
    setConfirm(null);
  };

  return (
    <div className="space-y-6">
      <PageHeader title="Students" description="Search, filter, and manage student accounts." />

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by name, student number, or email"
            className="pl-9"
            value={query}
            onChange={(e) => {
              setQuery(e.target.value);
              setPage(1);
            }}
            aria-label="Search students"
          />
        </div>
        <Select value={status} onValueChange={(v) => { setStatus(v); setPage(1); }}>
          <SelectTrigger className="w-[170px]">
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All statuses</SelectItem>
            <SelectItem value="Active">Active</SelectItem>
            <SelectItem value="Suspended">Suspended</SelectItem>
            <SelectItem value="Inactive">Inactive</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <TableSkeleton rows={6} />
      ) : data && data.items.length === 0 ? (
        <EmptyState icon={Users} title="No students found" description="Try a different search or filter." />
      ) : data ? (
        <DataTable
          columns={columns}
          data={data.items}
          rowKey={(s) => s.id}
          onRowClick={(s) => navigate(`/admin/students/${s.id}`)}
          sortable
          sortKey={sortBy?.split(":")[0]}
          sortDir={sortBy?.endsWith(":desc") ? "desc" : "asc"}
          onSort={handleSort}
          loading={isFetching}
        />
      ) : null}

      {data && data.totalItems > 0 && (
        <Pagination
          data={data}
          onPageChange={setPage}
          onPageSizeChange={(s) => { setPageSize(s); setPage(1); }}
        />
      )}

      <ConfirmDialog
        open={confirm !== null}
        onOpenChange={(o) => !o && setConfirm(null)}
        title={confirm?.status === "Suspended" ? "Suspend student?" : "Reactivate student?"}
        description={
          confirm?.status === "Suspended"
            ? `${confirm.name} will lose access until reactivated.`
            : `${confirm?.name} will regain portal access.`
        }
        confirmLabel={confirm?.status === "Suspended" ? "Suspend" : "Reactivate"}
        destructive={confirm?.status === "Suspended"}
        loading={changeStatus.isPending}
        onConfirm={handleConfirm}
      />
    </div>
  );
}
