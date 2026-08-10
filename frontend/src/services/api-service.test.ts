import { describe, it, expect, vi, beforeEach } from "vitest";

// Mock the shared axios instance so services never make a real network call.
const { apiClient } = vi.hoisted(() => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}));
vi.mock("@/lib/api-client", () => ({ apiClient }));

import { coursesService } from "@/services/courses.service";
import { enrollmentsService } from "@/services/enrollments.service";
import { authService } from "@/services/auth.service";

beforeEach(() => {
  vi.clearAllMocks();
});

describe("coursesService", () => {
  it("GETs /courses and returns the array", async () => {
    apiClient.get.mockResolvedValueOnce({ data: [{ id: "1" }] });
    const out = await coursesService.getAll();
    expect(apiClient.get).toHaveBeenCalledWith("/courses");
    expect(out).toEqual([{ id: "1" }]);
  });

  it("GETs a course by id", async () => {
    apiClient.get.mockResolvedValueOnce({ data: { id: "42", code: "CS-101" } });
    const out = await coursesService.getById("42");
    expect(apiClient.get).toHaveBeenCalledWith("/courses/42");
    expect(out.code).toBe("CS-101");
  });
});

describe("enrollmentsService", () => {
  it("POSTs a registration and returns the enrollment", async () => {
    apiClient.post.mockResolvedValueOnce({ data: { id: "e1", status: "Registered" } });
    const out = await enrollmentsService.register({ courseId: "course-1" });
    expect(apiClient.post).toHaveBeenCalledWith("/enrollments", { courseId: "course-1" });
    expect(out.status).toBe("Registered");
  });

  it("DELETEs an enrollment by id", async () => {
    apiClient.delete.mockResolvedValueOnce({ data: undefined });
    await enrollmentsService.drop("e1");
    expect(apiClient.delete).toHaveBeenCalledWith("/enrollments/e1");
  });

  it("GETs the student dashboard", async () => {
    apiClient.get.mockResolvedValueOnce({ data: { studentId: "s1", registeredCourses: [] } });
    const out = await enrollmentsService.getDashboard();
    expect(apiClient.get).toHaveBeenCalledWith("/dashboard");
    expect(out.studentId).toBe("s1");
  });
});

describe("authService", () => {
  it("POSTs student credentials to /auth/login", async () => {
    apiClient.post.mockResolvedValueOnce({
      data: { accessToken: "tok", student: { id: "s1" } },
    });
    const out = await authService.studentLogin({ email: "a@b.edu", password: "x" });
    expect(apiClient.post).toHaveBeenCalledWith("/auth/login", { email: "a@b.edu", password: "x" });
    expect(out.accessToken).toBe("tok");
  });

  it("POSTs admin credentials to /admin/auth/login", async () => {
    apiClient.post.mockResolvedValueOnce({
      data: { accessToken: "adm", administrator: { id: "a1" } },
    });
    const out = await authService.adminLogin({ email: "admin@b.edu", password: "x" });
    expect(apiClient.post).toHaveBeenCalledWith("/admin/auth/login", {
      email: "admin@b.edu",
      password: "x",
    });
    expect(out.accessToken).toBe("adm");
  });
});
