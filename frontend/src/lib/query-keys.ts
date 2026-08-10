/**
 * Centralised TanStack Query keys so cache invalidation and selective
 * refetching stay consistent across every page. Each factory returns an
 * array (optionally with params) to match React Query's hierarchical keys.
 */
export const queryKeys = {
  auth: ["auth"] as const,
  me: () => [...queryKeys.auth, "me"] as const,

  // Student
  studentDashboard: () => ["student", "dashboard"] as const,
  myEnrollments: () => ["student", "enrollments"] as const,
  courses: () => ["courses"] as const,
  course: (id: string) => ["courses", id] as const,

  // Admin dashboard
  adminDashboard: () => ["admin", "dashboard"] as const,

  // Admin students
  adminStudents: (params: Record<string, unknown>) => ["admin", "students", params] as const,
  adminStudent: (id: string) => ["admin", "students", id] as const,

  // Admin courses
  adminCourses: (params: Record<string, unknown>) => ["admin", "courses", params] as const,
  adminCourse: (id: string) => ["admin", "courses", id] as const,

  // Admin enrollments
  adminEnrollments: (params: Record<string, unknown>) =>
    ["admin", "enrollments", params] as const,

  // Admin reports
  enrollmentReport: () => ["admin", "reports", "enrollment"] as const,
  studentsReport: () => ["admin", "reports", "students"] as const,
  availableSeatsReport: () => ["admin", "reports", "courses"] as const,
  waitlistReport: () => ["admin", "reports", "waitlist"] as const,

  // Admin audit
  auditLogs: (params: Record<string, unknown>) => ["admin", "audit", params] as const,
};
