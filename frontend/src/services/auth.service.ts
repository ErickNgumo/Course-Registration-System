import { apiClient } from "@/lib/api-client";
import type {
  StudentLoginRequest,
  StudentLoginResponse,
  StudentDto,
  AdministratorLoginRequest,
  AdministratorLoginResponse,
} from "@/types/api";

export const authService = {
  async studentLogin(data: StudentLoginRequest): Promise<StudentLoginResponse> {
    const res = await apiClient.post<StudentLoginResponse>("/auth/login", data);
    return res.data;
  },

  async getMe(): Promise<StudentDto> {
    const res = await apiClient.get<StudentDto>("/auth/me");
    return res.data;
  },

  async adminLogin(data: AdministratorLoginRequest): Promise<AdministratorLoginResponse> {
    const res = await apiClient.post<AdministratorLoginResponse>("/admin/auth/login", data);
    return res.data;
  },
};