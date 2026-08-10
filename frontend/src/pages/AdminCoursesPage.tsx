import { useState } from "react";
import { Link } from "react-router-dom";
import { PlusCircle, Search, BookOpen } from "lucide-react";
import {
  useAdminCourses,
  useDeleteCourse,
  useToggleCourseActivation,
} from "@/hooks/use-queries";
import { getApiErrorMessage, isConflict } from "@/lib/api-error";
import { buildSort } from "@/lib/format";
import { PageHeader } from "@/components/PageHeader";
import { Pagination } from "@/components/Pagination";
import { TableSkeleton } from "@/components/LoadingState";
import { EmptyState } from "@/components/EmptyState";
import { DataTable, type Column } from "@/components/DataTable";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { ActiveBadge } from "@/components/SemesterProgress";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/hooks/use-toast";
import { useDebounce } from "@/hooks/use-debounce";
import type { CourseAdministrationResponse } from "@/types/api";

const PAGE_SIZE = 10;

export default function AdminCoursesPage() {
  const { toast } = useToast();
  const del = useDeleteCourse();
  const toggle = useToggleCourseActivation();

  const [query, setQuery] = useState("");
  const debouncedQuery = useDebounce(query, 350);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE);
  const [sortBy, setSortBy] = useState<string | undefined>(undefined);
  const [deleteTarget, setDeleteTarget] = useState<CourseAdministrationResponse | null>(null);

  const { data, isLoading, isFetching } = useAdminCourses({
    page,
    pageSize,
    search: debouncedQuery || undefined,
    sortBy,
  });

  const handleSort = (key: string) => {
    setSortBy((prev) => {
      if (!prev || !prev.startsWith(`${key}:`)) return buildSort(key, "asc");
      return buildSort(key, prev.endsWith(":desc") ? "asc" : "desc");
    });
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await del.mutateAsync(deleteTarget.id);
      toast({ title: "Course deleted", description: `${deleteTarget.code} was removed.` });
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Cannot delete course",
        description: isConflict(error)
          ? "The course still has students enrolled (or administrable enrollments)."
          : getApiErrorMessage(error),
      });
    }
    setDeleteTarget(null);
  };

  const handleToggle = async (course: CourseAdministrationResponse) => {
    try {
      await toggle.mutateAsync({ id: course.id, activate: !course.isActive });
      toast({ title: course.isActive ? "Course deactivated" : "Course activated", description: course.name });
    } catch (error) {
      toast({ variant: "destructive", title: "Update failed", description: getApiErrorMessage(error) });
    }
  };

  const columns: Column<CourseAdministrationResponse>[] = [
    { id: "code", sortKey: "code", header: "Code", cell: (c) => <Badge variant="secondary">{c.code}</Badge> },
    { id: "name", sortKey: "name", header: "Name", cell: (c) => (
      <Link to={`/admin/courses/${c.id}/edit`} className="font-medium hover:text-primary">
        {c.name}
      </Link>
    )},
    { id: "semester", sortKey: "semester", header: "Semester", cell: (c) => c.semester },
    { id: "credits", sortKey: "credits", header: "Credits", cell: (c) => c.credits },
    { id: "capacity", sortKey: "capacity", header: "Capacity", cell: (c) => c.capacity },
    { id: "enrolled", header: "Enrolled", cell: (c) => c.activeEnrollmentCount },
    { id: "active", sortKey: "active", header: "Status", cell: (c) => <ActiveBadge active={c.isActive} /> },
    { id: "actions", header: "", className: "text-right", cell: (c) => (
      <div className="flex justify-end gap-1">
        <Button variant="outline" size="sm" onClick={(e) => { e.stopPropagation(); void handleToggle(c); }}>
          {c.isActive ? "Deactivate" : "Activate"}
        </Button>
        <Button variant="ghost" size="sm" asChild>
          <Link to={`/admin/courses/${c.id}/edit`}>Edit</Link>
        </Button>
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:bg-destructive/10"
          onClick={(e) => { e.stopPropagation(); setDeleteTarget(c); }}
        >
          Delete
        </Button>
      </div>
    )},
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title="Courses"
        description="Create and manage the course catalog."
        actions={
          <Button asChild>
            <Link to="/admin/courses/new">
              <PlusCircle className="h-4 w-4" /> New course
            </Link>
          </Button>
        }
      />

      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Search by code or name"
          className="pl-9"
          value={query}
          onChange={(e) => { setQuery(e.target.value); setPage(1); }}
          aria-label="Search courses"
        />
      </div>

      {isLoading ? (
        <TableSkeleton rows={6} />
      ) : data && data.items.length === 0 ? (
        <EmptyState icon={BookOpen} title="No courses" description="Create your first course to get started." />
      ) : data ? (
        <DataTable
          columns={columns}
          data={data.items}
          rowKey={(c) => c.id}
          sortable
          sortKey={sortBy?.split(":")[0]}
          sortDir={sortBy?.endsWith(":desc") ? "desc" : "asc"}
          onSort={handleSort}
          loading={isFetching}
        />
      ) : null}

      {data && data.totalItems > 0 && (
        <Pagination data={data} onPageChange={setPage} onPageSizeChange={(s) => { setPageSize(s); setPage(1); }} />
      )}

      <ConfirmDialog
        open={deleteTarget !== null}
        onOpenChange={(o) => !o && setDeleteTarget(null)}
        title="Delete this course?"
        description={deleteTarget?.code ? `${deleteTarget.code} — ${deleteTarget.name} will be permanently removed.` : ""}
        confirmLabel="Delete"
        destructive
        loading={del.isPending}
        onConfirm={handleDelete}
      />
    </div>
  );
}
