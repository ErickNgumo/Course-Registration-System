import { Mail, User2, Hash, CalendarRange } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { authService } from "@/services/auth.service";
import { queryKeys } from "@/lib/query-keys";
import { useStudentDashboard } from "@/hooks/use-queries";
import { PageHeader } from "@/components/PageHeader";
import { LoadingState } from "@/components/LoadingState";
import { StatusBadge } from "@/components/SemesterProgress";
import { ProgressBar } from "@/components/ProgressBar";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { getInitials, formatDate } from "@/lib/utils";

export default function ProfilePage() {
  const { data: me } = useQuery({
    queryKey: queryKeys.me(),
    queryFn: () => authService.getMe(),
  });
  const { data: dashboard } = useStudentDashboard();

  const name = me ? `${me.firstName} ${me.lastName}` : "Student";

  return (
    <div className="space-y-6">
      <PageHeader title="Profile" description="Your student account details." />
      <LoadingState className={me ? "hidden" : "block"} />
      <div className={me ? "block" : "hidden"}>
        <Card>
          <CardContent className="pt-6">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
              <Avatar className="h-16 w-16">
                <AvatarFallback className="bg-primary/10 text-lg font-semibold text-primary">
                  {me ? getInitials(me.firstName, me.lastName) : "?"}
                </AvatarFallback>
              </Avatar>
              <div>
                <h2 className="text-xl font-bold">{name}</h2>
                <p className="text-sm text-muted-foreground">{me?.email}</p>
              </div>
            </div>
            <Separator className="my-6" />
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <InfoRow icon={<User2 className="h-4 w-4" />} label="Full name" value={name} />
              <InfoRow icon={<Mail className="h-4 w-4" />} label="Email" value={me?.email ?? "—"} />
              <InfoRow icon={<Hash className="h-4 w-4" />} label="Student number" value={me?.studentNumber ?? "—"} />
            </div>
          </CardContent>
        </Card>

        <Card className="mt-6">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <CalendarRange className="h-5 w-5" /> Semester summary
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {dashboard ? (
              <>
                <ProgressBar
                  value={dashboard.currentSemesterCredits}
                  max={dashboard.maxSemesterCredits}
                  label="Credit load"
                />
                <div className="grid gap-3 sm:grid-cols-3">
                  <MiniStat label="Registered" value={dashboard.registeredCourses.length} />
                  <MiniStat label="Waitlisted" value={dashboard.waitlistedCourses.length} />
                  <MiniStat label="Completed" value={dashboard.completedCourses.length} />
                </div>
              </>
            ) : (
              <p className="text-sm text-muted-foreground">Loading summary…</p>
            )}
          </CardContent>
        </Card>

        {dashboard && (
          <Card className="mt-6">
            <CardHeader>
              <CardTitle>Recent activity</CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="divide-y">
                {[...dashboard.registeredCourses, ...dashboard.waitlistedCourses].slice(0, 5).map((e) => (
                  <li key={e.id} className="flex items-center justify-between py-3">
                    <div>
                      <p className="font-medium">{e.courseCode} · {e.courseName}</p>
                      <p className="text-xs text-muted-foreground">
                        Registered {formatDate(e.registeredAt)}
                      </p>
                    </div>
                    <StatusBadge status={e.status} />
                  </li>
                ))}
                {[...dashboard.registeredCourses, ...dashboard.waitlistedCourses].length === 0 && (
                  <li className="py-3 text-sm text-muted-foreground">No recent enrollments.</li>
                )}
              </ul>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}

function InfoRow({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
  return (
    <div className="flex items-start gap-3">
      <span className="mt-0.5 flex h-8 w-8 items-center justify-center rounded-full bg-muted text-muted-foreground">
        {icon}
      </span>
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="font-medium">{value}</p>
      </div>
    </div>
  );
}

function MiniStat({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-lg border p-3 text-center">
      <p className="text-2xl font-bold">{value}</p>
      <p className="text-xs text-muted-foreground">{label}</p>
    </div>
  );
}
