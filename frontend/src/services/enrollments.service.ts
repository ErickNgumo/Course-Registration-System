import { apiClient } from "@/lib/api-client";
import type {
  EnrollmentResponse,
  RegisterEnrollmentRequest,
  DashboardResponse,
} from "@/types/api";

export const enrollmentsService = {
  async register(data: RegisterEnrollmentRequest): Promise<EnrollmentResponse> {
    const res = await apiClient.post<EnrollmentResponse>("/enrollments", data);
    return res.data;
  },

  async drop(id: string): Promise<void> {
    await apiClient.delete(`/enrollments/${id}`);
  },

  async getMyEnrollments(): Promise<EnrollmentResponse[]> {
    const res = await apiClient.get<EnrollmentResponse[]>("/enrollments");
    return res.data;
  },

  async getDashboard(): Promise<DashboardResponse> {
    const res = await apiClient.get<DashboardResponse>("/dashboard");
    return res.data;
  },
};