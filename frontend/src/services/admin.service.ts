import { apiClient } from "@/lib/api-client";
import type {
  AdminDashboardResponse,
  PagedResponse,
  StudentAdministrationResponse,
  StudentProfileResponse,
  CourseAdministrationResponse,
  SaveCourseRequest,
  EnrollmentAdministrationResponse,
  AssignGradeRequest,
  CourseEnrollmentReport,
  StudentsByStatusReport,
  WaitlistReport,
  AvailableSeatsReport,
  AuditLogResponse,
} from "@/types/api";

const ADMIN = "/admin";

function q(params: Record<string, string | number | undefined>): string {
  const search = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== "") {
      search.set(k, String(v));
    }
  });
  const s = search.toString();
  return s ? `?${s}` : "";
}

export const adminService = {
  // Dashboard
  async getDashboard(): Promise<AdminDashboardResponse> {
    const res = await apiClient.get<AdminDashboardResponse>(`${ADMIN}/dashboard`);
    return res.data;
  },

  // Students
  async getStudents(params: {
    status?: string;
    search?: string;
    sortBy?: string;
    page: number;
    pageSize: number;
  }): Promise<PagedResponse<StudentAdministrationResponse>> {
    const res = await apiClient.get<PagedResponse<StudentAdministrationResponse>>(
      `${ADMIN}/students${q(params)}`,
    );
    return res.data;
  },

  async getStudent(id: string): Promise<StudentProfileResponse> {
    const res = await apiClient.get<StudentProfileResponse>(`${ADMIN}/students/${id}`);
    return res.data;
  },

  async changeStudentStatus(id: string, status: string): Promise<StudentAdministrationResponse> {
    const res = await apiClient.patch<StudentAdministrationResponse>(
      `${ADMIN}/students/${id}/status`,
      { status },
    );
    return res.data;
  },

  // Courses
  async getCourses(params: {
    search?: string;
    sortBy?: string;
    page: number;
    pageSize: number;
  }): Promise<PagedResponse<CourseAdministrationResponse>> {
    const res = await apiClient.get<PagedResponse<CourseAdministrationResponse>>(
      `${ADMIN}/courses${q(params)}`,
    );
    return res.data;
  },

  async createCourse(data: SaveCourseRequest): Promise<CourseAdministrationResponse> {
    const res = await apiClient.post<CourseAdministrationResponse>(`${ADMIN}/courses`, data);
    return res.data;
  },

  async updateCourse(id: string, data: SaveCourseRequest): Promise<CourseAdministrationResponse> {
    const res = await apiClient.put<CourseAdministrationResponse>(`${ADMIN}/courses/${id}`, data);
    return res.data;
  },

  async deleteCourse(id: string): Promise<void> {
    await apiClient.delete(`${ADMIN}/courses/${id}`);
  },

  async activateCourse(id: string): Promise<CourseAdministrationResponse> {
    const res = await apiClient.patch<CourseAdministrationResponse>(
      `${ADMIN}/courses/${id}/activate`,
    );
    return res.data;
  },

  async deactivateCourse(id: string): Promise<CourseAdministrationResponse> {
    const res = await apiClient.patch<CourseAdministrationResponse>(
      `${ADMIN}/courses/${id}/deactivate`,
    );
    return res.data;
  },

  // Enrollments
  async getEnrollments(params: {
    status?: string;
    semester?: string;
    courseId?: string;
    studentId?: string;
    sortBy?: string;
    page: number;
    pageSize: number;
  }): Promise<PagedResponse<EnrollmentAdministrationResponse>> {
    const res = await apiClient.get<PagedResponse<EnrollmentAdministrationResponse>>(
      `${ADMIN}/enrollments${q(params)}`,
    );
    return res.data;
  },

  async dropEnrollment(id: string): Promise<void> {
    await apiClient.delete(`${ADMIN}/enrollments/${id}`);
  },

  async promoteWaitlist(courseId: string): Promise<EnrollmentAdministrationResponse> {
    const res = await apiClient.post<EnrollmentAdministrationResponse>(
      `${ADMIN}/enrollments/waitlist/${courseId}/promote`,
    );
    return res.data;
  },

  async assignGrade(id: string, data: AssignGradeRequest): Promise<EnrollmentAdministrationResponse> {
    const res = await apiClient.patch<EnrollmentAdministrationResponse>(
      `${ADMIN}/enrollments/${id}/grade`,
      data,
    );
    return res.data;
  },

  // Reports
  async getEnrollmentReport(): Promise<CourseEnrollmentReport> {
    const res = await apiClient.get<CourseEnrollmentReport>(`${ADMIN}/reports/enrollment`);
    return res.data;
  },

  async getStudentsReport(): Promise<StudentsByStatusReport> {
    const res = await apiClient.get<StudentsByStatusReport>(`${ADMIN}/reports/students`);
    return res.data;
  },

  async getAvailableSeatsReport(): Promise<AvailableSeatsReport> {
    const res = await apiClient.get<AvailableSeatsReport>(`${ADMIN}/reports/courses`);
    return res.data;
  },

  async getWaitlistReport(): Promise<WaitlistReport> {
    const res = await apiClient.get<WaitlistReport>(`${ADMIN}/reports/waitlist`);
    return res.data;
  },

  // Audit
  async getAuditLogs(params: {
    entity?: string;
    action?: string;
    administratorId?: string;
    page: number;
    pageSize: number;
  }): Promise<PagedResponse<AuditLogResponse>> {
    const res = await apiClient.get<PagedResponse<AuditLogResponse>>(`${ADMIN}/audit${q(params)}`);
    return res.data;
  },
};