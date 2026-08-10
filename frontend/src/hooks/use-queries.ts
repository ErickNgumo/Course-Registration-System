import {
  useMutation,
  useQuery,
  useQueryClient,
  keepPreviousData,
} from "@tanstack/react-query";
import { coursesService } from "@/services/courses.service";
import { enrollmentsService } from "@/services/enrollments.service";
import { adminService } from "@/services/admin.service";
import { queryKeys } from "@/lib/query-keys";
import type {
  SaveCourseRequest,
  AssignGradeRequest,
} from "@/types/api";

/* ───────────────────────── Student ───────────────────────── */

export function useCourses() {
  return useQuery({
    queryKey: queryKeys.courses(),
    queryFn: () => coursesService.getAll(),
  });
}

export function useCourse(id: string | undefined) {
  return useQuery({
    queryKey: queryKeys.course(id ?? ""),
    queryFn: () => coursesService.getById(id!),
    enabled: !!id,
  });
}

export function useStudentDashboard() {
  return useQuery({
    queryKey: queryKeys.studentDashboard(),
    queryFn: () => enrollmentsService.getDashboard(),
  });
}

export function useMyEnrollments() {
  return useQuery({
    queryKey: queryKeys.myEnrollments(),
    queryFn: () => enrollmentsService.getMyEnrollments(),
  });
}

export function useRegisterCourse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (courseId: string) => enrollmentsService.register({ courseId }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.studentDashboard() });
      qc.invalidateQueries({ queryKey: queryKeys.myEnrollments() });
    },
  });
}

export function useDropCourse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => enrollmentsService.drop(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.studentDashboard() });
      qc.invalidateQueries({ queryKey: queryKeys.myEnrollments() });
    },
  });
}

/* ───────────────────────── Admin dashboard ───────────────────────── */

export function useAdminDashboard() {
  return useQuery({
    queryKey: queryKeys.adminDashboard(),
    queryFn: () => adminService.getDashboard(),
  });
}

/* ───────────────────────── Admin students ───────────────────────── */

export function useAdminStudents(params: {
  status?: string;
  search?: string;
  sortBy?: string;
  page: number;
  pageSize: number;
}) {
  return useQuery({
    queryKey: queryKeys.adminStudents(params),
    queryFn: () => adminService.getStudents(params),
    placeholderData: keepPreviousData,
  });
}

export function useAdminStudent(id: string | undefined) {
  return useQuery({
    queryKey: queryKeys.adminStudent(id ?? ""),
    queryFn: () => adminService.getStudent(id!),
    enabled: !!id,
  });
}

export function useChangeStudentStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) =>
      adminService.changeStudentStatus(id, status),
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ["admin", "students"] });
      qc.invalidateQueries({ queryKey: queryKeys.adminStudent(data.id) });
      qc.invalidateQueries({ queryKey: queryKeys.adminDashboard() });
    },
  });
}

/* ───────────────────────── Admin courses ───────────────────────── */

export function useAdminCourses(params: {
  search?: string;
  sortBy?: string;
  page: number;
  pageSize: number;
}) {
  return useQuery({
    queryKey: queryKeys.adminCourses(params),
    queryFn: () => adminService.getCourses(params),
    placeholderData: keepPreviousData,
  });
}

export function useCreateCourse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: SaveCourseRequest) => adminService.createCourse(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["admin", "courses"] }),
  });
}

export function useUpdateCourse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SaveCourseRequest }) =>
      adminService.updateCourse(id, data),
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ["admin", "courses"] });
      qc.invalidateQueries({ queryKey: queryKeys.adminCourse(data.id) });
    },
  });
}

export function useDeleteCourse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => adminService.deleteCourse(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "courses"] });
      qc.invalidateQueries({ queryKey: queryKeys.adminDashboard() });
    },
  });
}

export function useToggleCourseActivation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, activate }: { id: string; activate: boolean }) =>
      activate ? adminService.activateCourse(id) : adminService.deactivateCourse(id),
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ["admin", "courses"] });
      qc.invalidateQueries({ queryKey: queryKeys.adminCourse(data.id) });
    },
  });
}

/* ───────────────────────── Admin enrollments ───────────────────────── */

export function useAdminEnrollments(params: {
  status?: string;
  semester?: string;
  courseId?: string;
  studentId?: string;
  sortBy?: string;
  page: number;
  pageSize: number;
}) {
  return useQuery({
    queryKey: queryKeys.adminEnrollments(params),
    queryFn: () => adminService.getEnrollments(params),
    placeholderData: keepPreviousData,
  });
}

export function useDropEnrollmentAdmin() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => adminService.dropEnrollment(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "enrollments"] });
      qc.invalidateQueries({ queryKey: queryKeys.adminDashboard() });
    },
  });
}

export function usePromoteWaitlist() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (courseId: string) => adminService.promoteWaitlist(courseId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "enrollments"] });
      qc.invalidateQueries({ queryKey: queryKeys.waitlistReport() });
      qc.invalidateQueries({ queryKey: queryKeys.adminDashboard() });
    },
  });
}

export function useAssignGrade() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AssignGradeRequest }) =>
      adminService.assignGrade(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "enrollments"] });
      qc.invalidateQueries({ queryKey: queryKeys.adminDashboard() });
    },
  });
}

/* ───────────────────────── Admin reports ───────────────────────── */

export function useEnrollmentReport() {
  return useQuery({
    queryKey: queryKeys.enrollmentReport(),
    queryFn: () => adminService.getEnrollmentReport(),
  });
}

export function useStudentsReport() {
  return useQuery({
    queryKey: queryKeys.studentsReport(),
    queryFn: () => adminService.getStudentsReport(),
  });
}

export function useAvailableSeatsReport() {
  return useQuery({
    queryKey: queryKeys.availableSeatsReport(),
    queryFn: () => adminService.getAvailableSeatsReport(),
  });
}

export function useWaitlistReport() {
  return useQuery({
    queryKey: queryKeys.waitlistReport(),
    queryFn: () => adminService.getWaitlistReport(),
  });
}

/* ───────────────────────── Admin audit ───────────────────────── */

export function useAuditLogs(params: {
  entity?: string;
  action?: string;
  administratorId?: string;
  page: number;
  pageSize: number;
}) {
  return useQuery({
    queryKey: queryKeys.auditLogs(params),
    queryFn: () => adminService.getAuditLogs(params),
    placeholderData: keepPreviousData,
  });
}
