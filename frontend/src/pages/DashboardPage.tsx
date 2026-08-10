import { BookOpen, CheckCircle2, Clock, Hourglass } from "lucide-react";
import { Link } from "react-router-dom";
import { useStudentDashboard } from "@/hooks/use-queries";
import { PageHeader } from "@/components/PageHeader";
import { ProgressBar } from "@/components/ProgressBar";
import { LoadingState, CardSkeleton } from "@/components/LoadingState";
import { EmptyState } from "@/components/EmptyState";
import { StatusBadge } from "@/components/SemesterProgress";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { formatDate } from "@/lib/utils";
import type { EnrollmentResponse } from "@/types/api";

export default function DashboardPage() {
  const { data: dashboard, isLoading, isError } = useStudentDashboard();

  if (isError) return <LoadingState />;
  if (isLoading || !dashboard) {
    return (
      <div className="space-y-6">
        <PageHeader title="Dashboard" description="Your academic overview" />
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <CardSkeleton key={i} />
          ))}
        </div>
      </div>
    );
  }

  const fullName = `${dashboard.firstName} ${dashboard.lastName}`;

  return (
    <div className="space-y-8">
      <PageHeader
        title="Dashboard"
        description={`Welcome back, ${dashboard.firstName}. Here is your academic overview.`}
      />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          icon={<Clock className="h-5 w-5" />}
          label="Current Credits"
          value={`${dashboard.currentSemesterCredits}`}
          sub={`of ${dashboard.maxSemesterCredits} max`}
        />
        <StatCard
          icon={<BookOpen className="h-5 w-5" />}
          label="Registered"
          value={`${dashboard.registeredCourses.length}`}
          sub="courses this term"
        />
        <StatCard
          icon={<Hourglass className="h-5 w-5" />}
          label="Waitlisted"
          value={`${dashboard.waitlistedCourses.length}`}
          sub="courses pending"
        />
        <StatCard
          icon={<CheckCircle2 className="h-5 w-5" />}
          label="Completed"
          value={`${dashboard.completedCourses.length}`}
          sub="courses finished"
        />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Credit Load</CardTitle>
        </CardHeader>
        <CardContent>
          <ProgressBar
            value={dashboard.currentSemesterCredits}
            max={dashboard.maxSemesterCredits}
            label="Semester credits"
          />
        </CardContent>
      </Card>

      <div className="grid gap-6 lg:grid-cols-2">
        <EnrollmentListCard
          title="Registered Courses"
          empty="You are not registered for any courses this semester."
          enrollments={dashboard.registeredCourses}
        />
        <EnrollmentListCard
          title="Waitlists"
          empty="You are not on any waitlists."
          enrollments={dashboard.waitlistedCourses}
        />
        <EnrollmentListCard
          title="Completed Courses"
          empty="You have not completed any courses yet."
          enrollments={dashboard.completedCourses}
          showGrade
          className="lg:col-span-2"
        />
      </div>
    </div>
  );
}

function StatCard({
  icon,
  label,
  value,
  sub,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
  sub: string;
}) {
  return (
    <Card>
      <CardContent className="pt-6">
        <div className="flex items-center justify-between">
          <span className="text-sm font-medium text-muted-foreground">{label}</span>
          <span className="flex h-9 w-9 items-center justify-center rounded-full bg-primary/10 text-primary">
            {icon}
          </span>
        </div>
        <div className="mt-3 text-3xl font-bold tracking-tight">{value}</div>
        <p className="text-xs text-muted-foreground">{sub}</p>
      </CardContent>
    </Card>
  );
}

function EnrollmentListCard({
  title,
  empty,
  enrollments,
  showGrade = false,
  className,
}: {
  title: string;
  empty: string;
  enrollments: EnrollmentResponse[];
  showGrade?: boolean;
  className?: string;
}) {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent>
        {enrollments.length === 0 ? (
          <EmptyState title="Nothing here" description={empty} className="border-none p-4" />
        ) : (
          <ul className="divide-y">
            {enrollments.map((e) => (
              <li key={e.id} className="flex items-center justify-between py-3">
                <div className="min-w-0">
                  <Link
                    to={`/courses/${e.courseId}`}
                    className="block truncate font-medium hover:text-primary"
                  >
                    <span className="text-muted-foreground">{e.courseCode}</span> · {e.courseName}
                  </Link>
                  <p className="text-xs text-muted-foreground">
                    {e.credits} credits · {e.semester} · registered {formatDate(e.registeredAt)}
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  {showGrade && e.finalGrade && (
                    <span className="font-semibold">{e.finalGrade}</span>
                  )}
                  <StatusBadge status={e.status} />
                </div>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  );
}
