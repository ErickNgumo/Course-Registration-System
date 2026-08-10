import { Badge } from "@/components/ui/badge";
import { cn, enrollmentStatusColor } from "@/lib/utils";
import type { EnrollmentStatus } from "@/types/api";

interface StatusBadgeProps {
  status: string;
  className?: string;
}

/** Renders a colored pill for an enrollment status. */
export function StatusBadge({ status, className }: StatusBadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
        enrollmentStatusColor(status),
        className,
      )}
    >
      {status}
    </span>
  );
}

const STUDENT_VARIANT: Record<string, string> = {
  Active: "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400",
  Suspended: "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400",
  Inactive: "bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400",
};

export function StudentStatusBadge({ status, className }: StatusBadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
        STUDENT_VARIANT[status] ?? STUDENT_VARIANT.Inactive,
        className,
      )}
    >
      {status}
    </span>
  );
}

export function EnrollmentStatusBadge({ status }: { status: EnrollmentStatus }) {
  return <StatusBadge status={status} />;
}

export function ActiveBadge({ active, label = "Active" }: { active: boolean; label?: string }) {
  return active ? <Badge variant="success">{label}</Badge> : <Badge variant="muted">Inactive</Badge>;
}
