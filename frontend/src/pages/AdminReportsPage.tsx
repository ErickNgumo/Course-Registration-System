import { BarChart3, TrendingUp, Users, Clock, BookOpen } from "lucide-react";
import {
  useEnrollmentReport,
  useStudentsReport,
  useAvailableSeatsReport,
  useWaitlistReport,
} from "@/hooks/use-queries";
import { PageHeader } from "@/components/PageHeader";
import { LoadingState } from "@/components/LoadingState";
import { EmptyState } from "@/components/EmptyState";
import { BarChartCard, type BarDatum } from "@/components/charts/BarChartCard";
import { DonutChartCard } from "@/components/charts/DonutChartCard";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ProgressBar } from "@/components/ProgressBar";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

export default function AdminReportsPage() {
  const enrollment = useEnrollmentReport();
  const students = useStudentsReport();
  const seats = useAvailableSeatsReport();
  const waitlists = useWaitlistReport();

  const loading =
    enrollment.isLoading || students.isLoading || seats.isLoading || waitlists.isLoading;

  if (loading) return <LoadingState className="min-h-[60vh]" />;

  const courses = enrollment.data?.courses ?? [];
  const statusCounts = students.data?.statuses ?? [];
  const waitlistRows = waitlists.data?.waitlists ?? [];

  const topByEnrollment: BarDatum[] = [...courses]
    .sort((a, b) => b.registered - a.registered)
    .slice(0, 8)
    .map((c) => ({ label: c.code, value: c.registered }));

  const capacityData = [...courses]
    .sort((a, b) => b.utilizationPercent - a.utilizationPercent)
    .slice(0, 8)
    .map((c) => ({ label: c.code, value: Math.round(c.utilizationPercent) }));

  // Credit distribution buckets the active catalog by credit value.
  const creditHistogram = Bucket.from(
    courses.map((c) => c.credits),
  ).map((b) => ({ label: `${b.bucket} cr`, value: b.count }));

  return (
    <div className="space-y-8">
      <PageHeader
        title="Reports"
        description="Enrollment trends, capacity utilization, and student composition across the institution."
      />

      <div className="grid gap-6 lg:grid-cols-3">
        <DonutChartCard
          title="Students by status"
          centerLabel="students"
          data={statusCounts.map((s, i) => ({
            label: s.status,
            value: s.count,
            color: statusColor(s.status, i),
          }))}
        />
        <BarChartCard
          title="Top courses by enrollment"
          data={topByEnrollment}
          color="primary"
        />
        <BarChartCard
          title="Course credit distribution"
          data={creditHistogram}
          color="blue"
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <BarChartCard
          title="Capacity utilization (%)"
          data={capacityData}
          color="green"
          valueFormatter={(v) => `${v}%`}
        />

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <BookOpen className="h-5 w-5 text-primary" /> Capacity utilization
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {courses.length === 0 ? (
              <p className="py-8 text-center text-sm text-muted-foreground">
                No course data available.
              </p>
            ) : (
              [...courses]
                .sort((a, b) => b.utilizationPercent - a.utilizationPercent)
                .slice(0, 6)
                .map((c) => (
                  <div key={c.courseId}>
                    <ProgressBar value={c.registered} max={c.capacity} label={c.code} />
                    <p className="mt-0.5 text-right text-xs text-muted-foreground">
                      {Math.round(c.utilizationPercent)}% utilized
                    </p>
                  </div>
                ))
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <Clock className="h-5 w-5 text-primary" /> Waitlist counts by course
          </CardTitle>
        </CardHeader>
        <CardContent>
          {waitlistRows.length === 0 ? (
            <EmptyState icon={BarChart3} title="No active waitlists" description="No students are currently on a waitlist." />
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Code</TableHead>
                    <TableHead>Course</TableHead>
                    <TableHead>Semester</TableHead>
                    <TableHead className="text-right">Waitlisted</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {waitlistRows
                    .slice()
                    .sort((a, b) => b.waitlisted - a.waitlisted)
                    .map((w) => (
                      <TableRow key={w.courseId}>
                        <TableCell className="font-medium">{w.code}</TableCell>
                        <TableCell>{w.name}</TableCell>
                        <TableCell>{w.semester}</TableCell>
                        <TableCell className="text-right font-semibold">{w.waitlisted}</TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Full enrollment report table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <TrendingUp className="h-5 w-5 text-primary" /> Course enrollment report
          </CardTitle>
        </CardHeader>
        <CardContent>
          {courses.length === 0 ? (
            <EmptyState icon={Users} title="No enrollment data" description="Enrollment statistics will appear once courses have registrations." />
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Code</TableHead>
                    <TableHead>Course</TableHead>
                    <TableHead>Semester</TableHead>
                    <TableHead className="text-right">Capacity</TableHead>
                    <TableHead className="text-right">Registered</TableHead>
                    <TableHead className="text-right">Waitlisted</TableHead>
                    <TableHead className="text-right">Available</TableHead>
                    <TableHead className="text-right">Utilization</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {courses.map((c) => (
                    <TableRow key={c.courseId}>
                      <TableCell className="font-medium">{c.code}</TableCell>
                      <TableCell>{c.name}</TableCell>
                      <TableCell>{c.semester}</TableCell>
                      <TableCell className="text-right">{c.capacity}</TableCell>
                      <TableCell className="text-right">{c.registered}</TableCell>
                      <TableCell className="text-right">{c.waitlisted}</TableCell>
                      <TableCell className="text-right">{c.availableSeats}</TableCell>
                      <TableCell className="text-right font-medium">
                        {Math.round(c.utilizationPercent)}%
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

/** Picks a chart palette color for a student status, falling back by index. */
function statusColor(status: string, i: number): "green" | "red" | "gray" | "amber" | "primary" | "blue" | "purple" {
  switch (status) {
    case "Active": return "green";
    case "Suspended": return "red";
    case "Inactive": return "gray";
    default: return (["primary", "amber", "blue", "purple", "gray"] as const)[i % 5];
  }
}

/** Lightweight numeric histogram helper used for the credit distribution chart. */
namespace Bucket {
  export interface Hit { bucket: number; count: number; }
  export function from(values: number[]): Hit[] {
    const map = new Map<number, number>();
    for (const v of values) map.set(v, (map.get(v) ?? 0) + 1);
    const buckets = [...map.keys()].sort((a, b) => a - b);
    return buckets.map((b) => ({ bucket: b, count: map.get(b) ?? 0 }));
  }
}
