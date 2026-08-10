import axios from "axios";
import { jwtDecode } from "@/lib/jwt";

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "/api/v1";

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const token = localStorage.getItem("accessToken");
      if (!token) return Promise.reject(error);

      const uid = getTokenUserId(token);
      const role = getTokenUserRole(token);

      localStorage.removeItem("accessToken");
      localStorage.removeItem("userRole");
      localStorage.removeItem("userId");
      localStorage.removeItem("userName");

      if (role === "Administrator") {
        window.location.href = "/admin/login";
      } else {
        window.location.href = "/login";
      }
    }
    return Promise.reject(error);
  },
);

export function setAuthToken(token: string, role: string, userId: string, userName: string) {
  localStorage.setItem("accessToken", token);
  localStorage.setItem("userRole", role);
  localStorage.setItem("userId", userId);
  localStorage.setItem("userName", userName);
}

export function clearAuth() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("userRole");
  localStorage.removeItem("userId");
  localStorage.removeItem("userName");
}

function getTokenUserId(token: string): string | null {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.sub ?? null;
  } catch {
    return null;
  }
}

function getTokenUserRole(token: string): string | null {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload.role ?? null;
  } catch {
    return null;
  }
}

export { getTokenUserId, getTokenUserRole };