import { BookOpen, GraduationCap, Hourglass, Layers, TrendingUp, UserCheck } from "lucide-react";
import { useAdminDashboard } from "@/hooks/use-queries";
import { PageHeader } from "@/components/PageHeader";
import { LoadingState } from "@/components/LoadingState";
import { DonutChartCard } from "@/components/charts/DonutChartCard";
import { BarChartCard } from "@/components/charts/BarChartCard";
import { Card, CardContent } from "@/components/ui/card";

export default function AdminDashboardPage() {
  const { data: dashboard, isLoading } = useAdminDashboard();

  if (isLoading || !dashboard) return <LoadingState className="min-h-[60vh]" />;

  return (
    <div className="space-y-8">
      <PageHeader title="Admin Dashboard" description="System-wide enrollment and course overview." />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Stat
          icon={<GraduationCap className="h-5 w-5" />}
          label="Total students"
          value={dashboard.totalStudents}
          hint={`${dashboard.activeStudents} active · ${dashboard.suspendedStudents} suspended`}
        />
        <Stat
          icon={<BookOpen className="h-5 w-5" />}
          label="Total courses"
          value={dashboard.totalCourses}
          hint={`${dashboard.activeCourses} active`}
        />
        <Stat
          icon={<TrendingUp className="h-5 w-5" />}
          label="Total registrations"
          value={dashboard.totalRegistrations}
          hint={`${dashboard.registeredStudents} registered`}
        />
        <Stat
          icon={<Hourglass className="h-5 w-5" />}
          label="Waitlisted students"
          value={dashboard.waitlistedStudents}
          hint={`${dashboard.availableSeats} seats available`}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <DonutChartCard
          title="Students by status"
          centerLabel="students"
          data={[
            { label: "Active", value: dashboard.activeStudents },
            { label: "Suspended", value: dashboard.suspendedStudents, color: "red" },
            {
              label: "Inactive",
              value: Math.max(dashboard.totalStudents - dashboard.activeStudents - dashboard.suspendedStudents, 0),
              color: "gray",
            },
          ]}
        />
        <BarChartCard
          title="Registration outcomes by semester"
          data={dashboard.semesterStatistics.map((s) => ({ label: s.semester, value: s.registered }))}
        />
        <DonutChartCard
          title="Seats availability"
          centerLabel="seats"
          data={[
            {
              label: "Registered",
              value: dashboard.registeredStudents,
            },
            {
              label: "Waitlisted",
              value: dashboard.waitlistedStudents,
              color: "amber",
            },
            {
              label: "Available",
              value: dashboard.availableSeats,
              color: "green",
            },
          ]}
        />
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="mb-4 flex items-center gap-2">
            <Layers className="h-5 w-5 text-primary" />
            <h2 className="font-semibold">Semester breakdown</h2>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b text-left text-muted-foreground">
                  <th className="py-2 pr-4">Semester</th>
                  <th className="py-2 pr-4">Registered</th>
                  <th className="py-2 pr-4">Waitlisted</th>
                  <th className="py-2 pr-4">Completed</th>
                  <th className="py-2 pr-4">Dropped</th>
                </tr>
              </thead>
              <tbody>
                {dashboard.semesterStatistics.map((s) => (
                  <tr key={s.semester} className="border-b last:border-0">
                    <td className="py-2 pr-4 font-medium">{s.semester}</td>
                    <td className="py-2 pr-4">{s.registered}</td>
                    <td className="py-2 pr-4">{s.waitlisted}</td>
                    <td className="py-2 pr-4">{s.completed}</td>
                    <td className="py-2 pr-4">{s.dropped}</td>
                  </tr>
                ))}
                {dashboard.semesterStatistics.length === 0 && (
                  <tr>
                    <td colSpan={5} className="py-8 text-center text-muted-foreground">
                      No semester data yet.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

function Stat({
  icon,
  label,
  value,
  hint,
}: {
  icon: React.ReactNode;
  label: string;
  value: number;
  hint: string;
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
        <div className="mt-3 text-3xl font-bold tracking-tight">{value.toLocaleString()}</div>
        <p className="flex items-center gap-1 text-xs text-muted-foreground">
          <UserCheck className="h-3 w-3" />
          {hint}
        </p>
      </CardContent>
    </Card>
  );
}
