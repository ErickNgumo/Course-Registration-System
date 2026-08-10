import { apiClient } from "@/lib/api-client";
import type { CourseResponse } from "@/types/api";

export const coursesService = {
  async getAll(): Promise<CourseResponse[]> {
    const res = await apiClient.get<CourseResponse[]>("/courses");
    return res.data;
  },

  async getById(id: string): Promise<CourseResponse> {
    const res = await apiClient.get<CourseResponse>(`/courses/${id}`);
    return res.data;
  },
};