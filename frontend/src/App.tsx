import { lazy, Suspense } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "@/contexts/AuthContext";
import { Toaster } from "@/components/ui/Toaster";
import { ThemeProvider } from "@/components/ThemeProvider";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { StudentLayout } from "@/layouts/StudentLayout";
import { AdminLayout } from "@/layouts/AdminLayout";
import { ErrorBoundary } from "@/components/ErrorBoundary";
import { QueryErrorBoundary } from "@/components/QueryErrorBoundary";
import { LoadingState } from "@/components/LoadingState";
import LoginPage from "@/pages/LoginPage";
import AdminLoginPage from "@/pages/AdminLoginPage";

// Code-split every route so initial bundle stays small. Public auth pages
// load eagerly (they are the gateway); everything else loads on demand.
const DashboardPage = lazy(() => import("@/pages/DashboardPage"));
const CoursesPage = lazy(() => import("@/pages/CoursesPage"));
const CourseDetailPage = lazy(() => import("@/pages/CourseDetailPage"));
const EnrollmentsPage = lazy(() => import("@/pages/EnrollmentsPage"));
const ProfilePage = lazy(() => import("@/pages/ProfilePage"));

const AdminDashboardPage = lazy(() => import("@/pages/AdminDashboardPage"));
const AdminStudentsPage = lazy(() => import("@/pages/AdminStudentsPage"));
const AdminStudentDetailPage = lazy(() => import("@/pages/AdminStudentDetailPage"));
const AdminCoursesPage = lazy(() => import("@/pages/AdminCoursesPage"));
const AdminCourseEditorPage = lazy(() => import("@/pages/AdminCourseEditorPage"));
const AdminEnrollmentsPage = lazy(() => import("@/pages/AdminEnrollmentsPage"));
const AdminReportsPage = lazy(() => import("@/pages/AdminReportsPage"));
const AdminAuditPage = lazy(() => import("@/pages/AdminAuditPage"));

const NotFoundPage = lazy(() => import("@/pages/NotFoundPage"));
const UnauthorizedPage = lazy(() => import("@/pages/UnauthorizedPage"));
const ServerErrorPage = lazy(() => import("@/pages/ForbiddenPage"));

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
});

function RouteFallback() {
  return <LoadingState className="min-h-[60vh]" />;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider defaultTheme="system" storageKey="course-reg-theme">
        <BrowserRouter>
          <AuthProvider>
            <ErrorBoundary>
              <QueryErrorBoundary>
                <Suspense fallback={<RouteFallback />}>
                  <Routes>
                    {/* ── Public ───────────────────────────── */}
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/admin/login" element={<AdminLoginPage />} />

                    {/* ── Student-protected ────────────────── */}
                    <Route element={<ProtectedRoute requiredRole="Student" />}>
                      <Route element={<StudentLayout />}>
                        <Route path="/dashboard" element={<DashboardPage />} />
                        <Route path="/courses" element={<CoursesPage />} />
                        <Route path="/courses/:id" element={<CourseDetailPage />} />
                        <Route path="/enrollments" element={<EnrollmentsPage />} />
                        <Route path="/profile" element={<ProfilePage />} />
                      </Route>
                    </Route>

                    {/* ── Admin-protected ───────────────────── */}
                    <Route element={<ProtectedRoute requiredRole="Administrator" />}>
                      <Route element={<AdminLayout />}>
                        <Route path="/admin/dashboard" element={<AdminDashboardPage />} />
                        <Route path="/admin/students" element={<AdminStudentsPage />} />
                        <Route path="/admin/students/:id" element={<AdminStudentDetailPage />} />
                        <Route path="/admin/courses" element={<AdminCoursesPage />} />
                        <Route path="/admin/courses/new" element={<AdminCourseEditorPage />} />
                        <Route path="/admin/courses/:id/edit" element={<AdminCourseEditorPage />} />
                        <Route path="/admin/enrollments" element={<AdminEnrollmentsPage />} />
                        <Route path="/admin/reports" element={<AdminReportsPage />} />
                        <Route path="/admin/audit" element={<AdminAuditPage />} />
                      </Route>
                    </Route>

                    {/* ── Status pages ─────────────────────── */}
                    <Route path="/unauthorized" element={<UnauthorizedPage />} />
                    <Route path="/server-error" element={<ServerErrorPage />} />
                    <Route path="/" element={<Navigate to="/login" replace />} />
                    <Route path="*" element={<NotFoundPage />} />
                  </Routes>
                </Suspense>
              </QueryErrorBoundary>
            </ErrorBoundary>
            <Toaster />
          </AuthProvider>
        </BrowserRouter>
      </ThemeProvider>
    </QueryClientProvider>
  );
}
