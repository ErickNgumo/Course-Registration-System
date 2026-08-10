import { useState } from "react";
import { ClipboardList, Users, Clock } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  useAdminEnrollments,
  useDropEnrollmentAdmin,
  usePromoteWaitlist,
  useAssignGrade,
  useAvailableSeatsReport,
} from "@/hooks/use-queries";
import { getApiErrorMessage, isConflict } from "@/lib/api-error";
import { buildSort, SEMESTER_OPTIONS } from "@/lib/format";
import { assignGradeSchema, type AssignGradeValues } from "@/lib/schemas";
import { PageHeader } from "@/components/PageHeader";
import { Pagination } from "@/components/Pagination";
import { TableSkeleton } from "@/components/LoadingState";
import { EmptyState } from "@/components/EmptyState";
import { DataTable, type Column } from "@/components/DataTable";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { StatusBadge } from "@/components/SemesterProgress";
import { cn, formatDate, gradeColor } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useToast } from "@/hooks/use-toast";
import type { EnrollmentAdministrationResponse } from "@/types/api";

const PAGE_SIZE = 10;

const STATUS_FILTERS = [
  "all",
  "Registered",
  "Waitlisted",
  "Completed",
  "Dropped",
] as const;

export default function AdminEnrollmentsPage() {
  const { toast } = useToast();
  const drop = useDropEnrollmentAdmin();
  const promote = usePromoteWaitlist();

  const [status, setStatus] = useState<string>("all");
  const [semester, setSemester] = useState<string>("all");
  const [studentQuery, setStudentQuery] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE);
  const [sortBy, setSortBy] = useState<string | undefined>(undefined);

  const [dropTarget, setDropTarget] = useState<EnrollmentAdministrationResponse | null>(null);
  const [gradeTarget, setGradeTarget] = useState<EnrollmentAdministrationResponse | null>(null);

  // Waitlist course options come from the available-seats report, which only
  // includes courses that genuinely have waitlisted students via the waitlist
  // report. We reuse it here to drive the “promote” waitlist dropdown.
  const { data: seats } = useAvailableSeatsReport();

  const { data, isLoading, isFetching } = useAdminEnrollments({
    page,
    pageSize,
    status: status === "all" ? undefined : status,
    semester: semester === "all" ? undefined : semester,
    studentId: studentQuery.trim() || undefined,
    sortBy,
  });

  const handleSort = (key: string) => {
    setSortBy((prev) => {
      if (!prev || !prev.startsWith(`${key}:`)) return buildSort(key, "asc");
      return buildSort(key, prev.endsWith(":desc") ? "asc" : "desc");
    });
  };

  const handleDrop = async () => {
    if (!dropTarget) return;
    try {
      await drop.mutateAsync(dropTarget.id);
      toast({
        title: "Enrollment dropped",
        description: `${dropTarget.courseCode} for ${dropTarget.studentEmail}.`,
      });
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Cannot drop enrollment",
        description: getApiErrorMessage(error),
      });
    }
    setDropTarget(null);
  };

  const handlePromote = async (courseId: string, courseCode: string) => {
    try {
      await promote.mutateAsync(courseId);
      toast({
        title: "Waitlist promoted",
        description: `The next student on the ${courseCode} waitlist was promoted to a seat.`,
      });
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Could not promote",
        description: isConflict(error)
          ? "No one is currently on the waitlist for this course."
          : getApiErrorMessage(error),
      });
    }
  };

  const columns: Column<EnrollmentAdministrationResponse>[] = [
    {
      id: "student",
      sortKey: "student",
      header: "Student",
      cell: (e) => (
        <span className="font-medium">{e.studentEmail}</span>
      ),
    },
    {
      id: "course",
      sortKey: "course",
      header: "Course",
      cell: (e) => (
        <div>
          <span className="font-medium">{e.courseCode}</span>
          <p className="text-sm text-muted-foreground">{e.courseName}</p>
        </div>
      ),
    },
    { id: "semester", sortKey: "semester", header: "Semester", cell: (e) => e.semester },
    {
      id: "status",
      sortKey: "status",
      header: "Status",
      cell: (e) => <StatusBadge status={e.status} />,
    },
    { id: "grade", sortKey: "grade", header: "Grade", cell: (e) => (
      <span className={cn("font-medium", gradeColor(e.finalGrade))}>
        {e.finalGrade ?? "—"}
      </span>
    )},
    { id: "registered", sortKey: "registeredat", header: "Registered", cell: (e) => formatDate(e.registeredAt) },
    {
      id: "actions",
      header: "",
      className: "text-right",
      cell: (e) => (
        <div className="flex justify-end gap-1">
          {(e.status === "Registered" || e.status === "Waitlisted") && (
            <Button
              variant="outline"
              size="sm"
              onClick={(ev) => { ev.stopPropagation(); setGradeTarget(e); }}
            >
              Grade
            </Button>
          )}
          {e.status === "Waitlisted" && (
            <Button
              variant="outline"
              size="sm"
              onClick={(ev) => { ev.stopPropagation(); void handlePromote(e.courseId, e.courseCode); }}
            >
              Promote
            </Button>
          )}
          {e.status !== "Dropped" && e.status !== "Completed" && (
            <Button
              variant="ghost"
              size="sm"
              className="text-destructive hover:bg-destructive/10"
              onClick={(ev) => { ev.stopPropagation(); setDropTarget(e); }}
            >
              Force drop
            </Button>
          )}
        </div>
      ),
    },
  ];

  // Courses that have waitlisted students available for promotion.
  const waitlistedCourses = (seats?.courses ?? []).filter((c) => c.waitlisted > 0);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Enrollments"
        description="Manage every enrollment across the system — assign grades, force drop, and promote waitlists."
        actions={
          waitlistedCourses.length > 0 ? (
            <PromoteWaitlistMenu
              courses={waitlistedCourses.map((c) => ({ id: c.courseId, code: c.code }))}
              onPromote={(code) => {
                const course = waitlistedCourses.find((c) => c.code === code);
                if (course) void handlePromote(course.courseId, course.code);
              }}
              loading={promote.isPending}
            />
          ) : null
        }
      />

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative flex-1">
          <Users className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Filter by student ID"
            className="pl-9"
            value={studentQuery}
            onChange={(e) => { setStudentQuery(e.target.value); setPage(1); }}
            aria-label="Filter by student ID"
          />
        </div>
        <Select value={status} onValueChange={(v) => { setStatus(v); setPage(1); }}>
          <SelectTrigger className="w-[170px]">
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            {STATUS_FILTERS.map((s) => (
              <SelectItem key={s} value={s}>
                {s === "all" ? "All statuses" : s}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={semester} onValueChange={(v) => { setSemester(v); setPage(1); }}>
          <SelectTrigger className="w-[170px]">
            <SelectValue placeholder="Semester" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All semesters</SelectItem>
            {SEMESTER_OPTIONS.map((s) => (
              <SelectItem key={s} value={s}>{s}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <TableSkeleton rows={6} />
      ) : data && data.items.length === 0 ? (
        <EmptyState
          icon={ClipboardList}
          title="No enrollments"
          description="No enrollments match the current filters."
        />
      ) : data ? (
        <DataTable
          columns={columns}
          data={data.items}
          rowKey={(e) => e.id}
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
        open={dropTarget !== null}
        onOpenChange={(o) => !o && setDropTarget(null)}
        title="Force drop this enrollment?"
        description={
          dropTarget
            ? `${dropTarget.studentEmail} will be dropped from ${dropTarget.courseCode}. This cannot be undone.`
            : ""
        }
        confirmLabel="Force drop"
        destructive
        loading={drop.isPending}
        onConfirm={handleDrop}
      />

      <GradeDialog
        target={gradeTarget}
        onClose={() => setGradeTarget(null)}
      />
    </div>
  );
}

/** Inline dropdown for promoting a waitlisted student to a seat. */
function PromoteWaitlistMenu({
  courses,
  onPromote,
  loading,
}: {
  courses: { id: string; code: string }[];
  onPromote: (code: string) => void;
  loading: boolean;
}) {
  return (
    <Select value="" onValueChange={(v) => v && onPromote(v)}>
      <SelectTrigger className="w-[200px]" disabled={loading}>
        <span className="flex items-center gap-2">
          <Clock className="h-4 w-4" />
          <SelectValue placeholder="Promote waitlist" />
        </span>
      </SelectTrigger>
      <SelectContent>
        {courses.map((c) => (
          <SelectItem key={c.id} value={c.code}>
            {c.code}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

/** Modal form for assigning or clearing a final grade on an enrollment. */
function GradeDialog({
  target,
  onClose,
}: {
  target: EnrollmentAdministrationResponse | null;
  onClose: () => void;
}) {
  const { toast } = useToast();
  const assignGrade = useAssignGrade();

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<AssignGradeValues>({
    resolver: zodResolver(assignGradeSchema),
    values: target ? { finalGrade: target.finalGrade ?? "" } : { finalGrade: "" },
  });

  const current = watch("finalGrade") ?? "";

  const onSubmit = async (values: AssignGradeValues) => {
    if (!target) return;
    try {
      await assignGrade.mutateAsync({ id: target.id, data: { finalGrade: values.finalGrade } });
      toast({
        title: "Grade saved",
        description: `${target.courseCode} → ${values.finalGrade ?? "no grade"}`,
      });
      onClose();
    } catch (error) {
      toast({ variant: "destructive", title: "Could not save grade", description: getApiErrorMessage(error) });
    }
  };

  return (
    <Dialog open={target !== null} onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Assign final grade</DialogTitle>
          {target && (
            <DialogDescription>
              {target.courseCode} — {target.studentEmail}
            </DialogDescription>
          )}
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="finalGrade">Grade</Label>
            <div className="flex gap-2">
              <Input
                id="finalGrade"
                placeholder="e.g. A, B+, C-"
                {...register("finalGrade")}
                aria-invalid={!!errors.finalGrade}
                aria-describedby={errors.finalGrade ? "finalGrade-error" : undefined}
              />
              <Button
                type="button"
                variant="outline"
                onClick={() => setValue("finalGrade", "", { shouldValidate: true })}
              >
                Clear
              </Button>
            </div>
            {errors.finalGrade && (
              <p id="finalGrade-error" role="alert" className="text-sm text-destructive">
                {errors.finalGrade.message}
              </p>
            )}
            <p className="text-xs text-muted-foreground">
              Leave blank to remove an existing grade. Current: {target?.finalGrade ?? "—"}
            </p>
            <div className="flex flex-wrap gap-1 pt-1">
              {["A", "A-", "B+", "B", "B-", "C+", "C", "D", "F"].map((g) => (
                <Button
                  key={g}
                  type="button"
                  size="sm"
                  variant={current.toUpperCase() === g ? "default" : "outline"}
                  onClick={() => setValue("finalGrade", g, { shouldValidate: true })}
                >
                  {g}
                </Button>
              ))}
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={assignGrade.isPending}>
              {assignGrade.isPending ? "Saving…" : "Save grade"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
