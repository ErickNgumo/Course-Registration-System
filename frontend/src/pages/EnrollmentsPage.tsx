import { useMemo, useState } from "react";
import { ClipboardList, XCircle } from "lucide-react";
import { Link } from "react-router-dom";
import { useMyEnrollments, useDropCourse } from "@/hooks/use-queries";
import { PageHeader } from "@/components/PageHeader";
import { EmptyState } from "@/components/EmptyState";
import { TableSkeleton } from "@/components/LoadingState";
import { StatusBadge } from "@/components/SemesterProgress";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useToast } from "@/hooks/use-toast";
import { getApiErrorMessage, isConflict } from "@/lib/api-error";
import { formatDate } from "@/lib/utils";
import type { EnrollmentResponse } from "@/types/api";

type TabKey = "registered" | "waitlisted" | "completed" | "all";

export default function EnrollmentsPage() {
  const { data: enrollments, isLoading } = useMyEnrollments();
  const drop = useDropCourse();
  const { toast } = useToast();
  const [tab, setTab] = useState<TabKey>("registered");
  const [dropId, setDropId] = useState<string | null>(null);

  const byStatus = useMemo(() => {
    const groups: Record<TabKey, EnrollmentResponse[]> = {
      registered: [],
      waitlisted: [],
      completed: [],
      all: enrollments ?? [],
    };
    (enrollments ?? []).forEach((e) => {
      if (e.status === "Registered") groups.registered.push(e);
      else if (e.status === "Waitlisted") groups.waitlisted.push(e);
      else if (e.status === "Completed") groups.completed.push(e);
    });
    return groups;
  }, [enrollments]);

  const handleDrop = async (id: string) => {
    try {
      await drop.mutateAsync(id);
      toast({ title: "Course dropped", description: "Your registration was withdrawn." });
    } catch (error) {
      const message = isConflict(error)
        ? "This enrollment cannot be dropped at this time."
        : getApiErrorMessage(error, "Unable to drop this course.");
      toast({ variant: "destructive", title: "Drop failed", description: message });
    }
  };

  if (isLoading) return <TableSkeleton rows={5} />;

  return (
    <div className="space-y-6">
      <PageHeader
        title="My Courses"
        description="Manage your registrations, waitlists, and completed courses."
        actions={
          <Button asChild variant="outline">
            <Link to="/courses">Browse courses</Link>
          </Button>
        }
      />

      <Tabs value={tab} onValueChange={(v) => setTab(v as TabKey)}>
        <TabsList>
          <TabsTrigger value="registered">
            Registered ({byStatus.registered.length})
          </TabsTrigger>
          <TabsTrigger value="waitlisted">
            Waitlisted ({byStatus.waitlisted.length})
          </TabsTrigger>
          <TabsTrigger value="completed">
            Completed ({byStatus.completed.length})
          </TabsTrigger>
          <TabsTrigger value="all">All ({byStatus.all.length})</TabsTrigger>
        </TabsList>

        {(["registered", "waitlisted", "completed", "all"] as TabKey[]).map((key) => (
          <TabsContent key={key} value={key}>
            <EnrollmentTable
              rows={byStatus[key]}
              tab={key}
              onDrop={(id) => setDropId(id)}
            />
          </TabsContent>
        ))}
      </Tabs>

      <ConfirmDialog
        open={dropId !== null}
        onOpenChange={(o) => !o && setDropId(null)}
        title="Drop this course?"
        description="You will be removed from the course. If a waitlist exists, the next student may be promoted."
        confirmLabel="Drop course"
        destructive
        loading={drop.isPending}
        onConfirm={() => {
          if (dropId) void handleDrop(dropId);
          setDropId(null);
        }}
      />
    </div>
  );
}

function EnrollmentTable({
  rows,
  tab,
  onDrop,
}: {
  rows: EnrollmentResponse[];
  tab: TabKey;
  onDrop: (id: string) => void;
}) {
  if (rows.length === 0) {
    return (
      <EmptyState
        icon={ClipboardList}
        title="No courses here"
        description={
          tab === "registered"
            ? "You are not currently registered for any courses."
            : tab === "waitlisted"
              ? "You are not on any waitlists."
              : tab === "completed"
                ? "You have not completed any courses yet."
                : "You have no enrollments yet."
        }
      />
    );
  }

  return (
    <div className="rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Course</TableHead>
            <TableHead>Semester</TableHead>
            <TableHead>Credits</TableHead>
            <TableHead>Grade</TableHead>
            <TableHead>Registered</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Action</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((e) => (
            <TableRow key={e.id}>
              <TableCell>
                <Link to={`/courses/${e.courseId}`} className="font-medium hover:text-primary">
                  {e.courseCode}
                </Link>
                <p className="text-sm text-muted-foreground">{e.courseName}</p>
              </TableCell>
              <TableCell>{e.semester}</TableCell>
              <TableCell>{e.credits}</TableCell>
              <TableCell>
                {e.finalGrade ? <span className="font-semibold">{e.finalGrade}</span> : "—"}
              </TableCell>
              <TableCell>{formatDate(e.registeredAt)}</TableCell>
              <TableCell>
                <StatusBadge status={e.status} />
              </TableCell>
              <TableCell className="text-right">
                {(e.status === "Registered" || e.status === "Waitlisted") && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-destructive hover:bg-destructive/10"
                    onClick={() => onDrop(e.id)}
                  >
                    <XCircle className="h-4 w-4" /> Drop
                  </Button>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
