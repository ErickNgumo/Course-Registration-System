import { useParams } from "react-router-dom";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, User2 } from "lucide-react";
import { useAdminStudent, useChangeStudentStatus } from "@/hooks/use-queries";
import { getApiErrorMessage } from "@/lib/api-error";
import { PageHeader } from "@/components/PageHeader";
import { LoadingState } from "@/components/LoadingState";
import { EmptyState } from "@/components/EmptyState";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { StudentStatusBadge, StatusBadge } from "@/components/SemesterProgress";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { useToast } from "@/hooks/use-toast";
import { getInitials, formatDate } from "@/lib/utils";
import type { EnrollmentAdministrationResponse } from "@/types/api";

export default function AdminStudentDetailPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const { toast } = useToast();
  const { data: profile, isLoading, isError } = useAdminStudent(id);
  const changeStatus = useChangeStudentStatus();
  const [confirm, setConfirm] = useState<string | null>(null);

  const isSuspended = profile?.student.status === "Suspended";

  const handleStatusToggle = async () => {
    if (!profile) return;
    const newStatus = isSuspended ? "Active" : "Suspended";
    try {
      await changeStatus.mutateAsync({ id: profile.student.id, status: newStatus });
      toast({ title: `Student ${newStatus === "Suspended" ? "suspended" : "reactivated"}` });
    } catch (error) {
      toast({ variant: "destructive", title: "Update failed", description: getApiErrorMessage(error) });
    }
    setConfirm(null);
  };

  if (isLoading) return <LoadingState />;
  if (isError || !profile) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" size="sm" onClick={() => navigate("/admin/students")}>
          <ArrowLeft className="h-4 w-4" /> Back to students
        </Button>
        <EmptyState icon={User2} title="Student not found" />
      </div>
    );
  }

  const s = profile.student;

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" onClick={() => navigate("/admin/students")}>
        <ArrowLeft className="h-4 w-4" /> Back to students
      </Button>

      <PageHeader
        title={`${s.firstName} ${s.lastName}`}
        description={`${s.studentNumber} · ${s.email}`}
        actions={
          <Button
            variant={isSuspended ? "default" : "destructive"}
            onClick={() => setConfirm(s.id)}
          >
            {isSuspended ? "Reactivate" : "Suspend"}
          </Button>
        }
      />

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
            <Avatar className="h-16 w-16">
              <AvatarFallback className="bg-primary/10 text-lg font-semibold text-primary">
                {getInitials(s.firstName, s.lastName)}
              </AvatarFallback>
            </Avatar>
            <div className="grid gap-3 sm:grid-cols-3">
              <Field label="Status" value={<StudentStatusBadge status={s.status} />} />
              <Field label="Email" value={s.email} />
              <Field label="Student number" value={s.studentNumber} />
            </div>
          </div>
        </CardContent>
      </Card>

      <Tabs defaultValue="registered">
        <TabsList>
          <TabsTrigger value="registered">Current ({profile.currentRegistrations.length})</TabsTrigger>
          <TabsTrigger value="completed">Completed ({profile.completedCourses.length})</TabsTrigger>
          <TabsTrigger value="waitlist">Waitlist ({profile.waitlists.length})</TabsTrigger>
          <TabsTrigger value="history">History ({profile.history.length})</TabsTrigger>
        </TabsList>
        <TabsContent value="registered">
          <EnrollmentsTable rows={profile.currentRegistrations} empty="No current registrations." />
        </TabsContent>
        <TabsContent value="completed">
          <EnrollmentsTable rows={profile.completedCourses} empty="No completed courses." showGrade />
        </TabsContent>
        <TabsContent value="waitlist">
          <EnrollmentsTable rows={profile.waitlists} empty="No waitlists." />
        </TabsContent>
        <TabsContent value="history">
          <EnrollmentsTable rows={profile.history} empty="No enrollment history." showGrade />
        </TabsContent>
      </Tabs>

      <ConfirmDialog
        open={confirm !== null}
        onOpenChange={(o) => !o && setConfirm(null)}
        title={isSuspended ? "Reactivate this student?" : "Suspend this student?"}
        description={
          isSuspended
            ? `${s.firstName} will regain portal access.`
            : `${s.firstName} will lose portal access until reactivated.`
        }
        confirmLabel={isSuspended ? "Reactivate" : "Suspend"}
        destructive={!isSuspended}
        loading={changeStatus.isPending}
        onConfirm={handleStatusToggle}
      />
    </div>
  );
}

function Field({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className="font-medium">{value}</span>
    </div>
  );
}

function EnrollmentsTable({
  rows,
  empty,
  showGrade = false,
}: {
  rows: EnrollmentAdministrationResponse[];
  empty: string;
  showGrade?: boolean;
}) {
  if (rows.length === 0) return <EmptyState title="Empty" description={empty} />;
  return (
    <div className="rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Course</TableHead>
            <TableHead>Semester</TableHead>
            <TableHead>Registered</TableHead>
            <TableHead>Status</TableHead>
            {showGrade && <TableHead>Grade</TableHead>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((e) => (
            <TableRow key={e.id}>
              <TableCell>
                <span className="font-medium">{e.courseCode}</span>
                <p className="text-sm text-muted-foreground">{e.courseName}</p>
              </TableCell>
              <TableCell>{e.semester}</TableCell>
              <TableCell>{formatDate(e.registeredAt)}</TableCell>
              <TableCell>
                <StatusBadge status={e.status} />
              </TableCell>
              {showGrade && <TableCell>{e.finalGrade ?? "—"}</TableCell>}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
