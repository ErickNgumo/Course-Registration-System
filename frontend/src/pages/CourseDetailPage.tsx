import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ArrowLeft, BookOpen } from "lucide-react";
import { useCourse, useMyEnrollments, useRegisterCourse, useStudentDashboard } from "@/hooks/use-queries";
import { getApiErrorMessage, isConflict, isUnprocessable } from "@/lib/api-error";
import { PageHeader } from "@/components/PageHeader";
import { EmptyState } from "@/components/EmptyState";
import { StatusBadge } from "@/components/SemesterProgress";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { useToast } from "@/hooks/use-toast";

export default function CourseDetailPage() {
  const { id = "" } = useParams();
  const { data: course, isLoading, isError } = useCourse(id);
  const { data: enrollments } = useMyEnrollments();
  const { data: dashboard } = useStudentDashboard();

  // Skip: dashboard registration + drop handled in EnrollmentsPage
  void dashboard;

  const register = useRegisterCourse();
  const { toast } = useToast();
  const [pending, setPending] = useState(false);

  const existing = enrollments?.find(
    (e) => e.courseId === id && (e.status === "Registered" || e.status === "Waitlisted"),
  );

  const remainingCredits =
    (dashboard?.maxSemesterCredits ?? 0) - (dashboard?.currentSemesterCredits ?? 0);

  const handleRegister = async () => {
    setPending(true);
    try {
      const res = await register.mutateAsync(id);
      if (res.status === "Waitlisted") {
        toast({
          variant: "warning",
          title: "Added to waitlist",
          description: `${res.courseName} is full — you are on the waitlist.`,
        });
      } else {
        toast({ title: "Registered", description: `You are now registered for ${res.courseName}.` });
      }
    } catch (error) {
      let message = getApiErrorMessage(error, "Unable to register for this course.");
      if (isConflict(error)) message = "You are already registered or waitlisted for this course.";
      else if (isUnprocessable(error)) {
        // Server returns the prerequisite / cap / timetable conflict reason.
        message = getApiErrorMessage(error);
      }
      toast({ variant: "destructive", title: "Registration failed", description: message });
    } finally {
      setPending(false);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Button asChild variant="ghost" size="sm">
          <Link to="/courses">
            <ArrowLeft className="h-4 w-4" /> Back to catalog
          </Link>
        </Button>
        <div className="h-64 animate-pulse rounded-lg bg-muted" />
      </div>
    );
  }

  if (isError || !course) {
    return (
      <div className="space-y-6">
        <Button asChild variant="ghost" size="sm">
          <Link to="/courses">
            <ArrowLeft className="h-4 w-4" /> Back to catalog
          </Link>
        </Button>
        <EmptyState
          icon={BookOpen}
          title="Course not found"
          description="This course may have been deactivated or removed."
        />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Button asChild variant="ghost" size="sm">
        <Link to="/courses">
          <ArrowLeft className="h-4 w-4" /> Back to catalog
        </Link>
      </Button>

      <PageHeader
        title={course.name}
        description={`${course.code} · ${course.semester}`}
      />

      <div className="grid gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Course details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex flex-wrap gap-2">
              <Badge variant="secondary">{course.code}</Badge>
              <Badge variant="outline">{course.credits} credits</Badge>
              <Badge variant="outline">Capacity {course.capacity}</Badge>
              <Badge variant="outline">{course.semester}</Badge>
            </div>
            <Separator />
            <div>
              <h3 className="mb-1 text-sm font-medium text-muted-foreground">Description</h3>
              <p className="text-sm leading-relaxed">
                {course.description ?? "No description provided for this course."}
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Registration</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {existing ? (
              <div className="space-y-3">
                <p className="text-sm text-muted-foreground">Your enrollment status:</p>
                <div className="flex items-center justify-between rounded-md border p-3">
                  <span className="font-medium">{existing.courseName}</span>
                  <StatusBadge status={existing.status} />
                </div>
                <Button asChild className="w-full" variant="outline">
                  <Link to="/enrollments">Manage enrollments</Link>
                </Button>
              </div>
            ) : (
              <>
                <p className="text-sm text-muted-foreground">
                  Remaining credits this semester:{" "}
                  <strong className="text-foreground">
                    {remainingCredits >= 0 ? remainingCredits : 0}
                  </strong>
                </p>
                <RegisterButtonWrapper
                  onConfirm={handleRegister}
                  pending={pending}
                  courseName={course.name}
                  remainingCredits={remainingCredits}
                />
              </>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function RegisterButtonWrapper({
  onConfirm,
  pending,
  courseName,
  remainingCredits,
}: {
  onConfirm: () => void;
  pending: boolean;
  courseName: string;
  remainingCredits: number;
}) {
  const [confirmOpen, setConfirmOpen] = useState(false);
  return (
    <>
      <Button className="w-full" onClick={() => setConfirmOpen(true)} disabled={pending}>
        {pending ? "Registering…" : "Register for this course"}
      </Button>
      <ConfirmDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title="Register for course?"
        description={`You are about to register for ${courseName}.`}
        confirmLabel="Register"
        loading={pending}
        onConfirm={() => {
          onConfirm();
          setConfirmOpen(false);
        }}
      />
    </>
  );
}
