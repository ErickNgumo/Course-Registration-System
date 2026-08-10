import { describe, it, expect, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { type ReactNode } from "react";
import { AuthProvider } from "@/contexts/AuthContext";
import { useAuth } from "@/hooks/useAuth";

function wrapper({ children }: { children: ReactNode }) {
  return <AuthProvider>{children}</AuthProvider>;
}

beforeEach(() => {
  localStorage.clear();
});

describe("useAuth", () => {
  it("starts unauthenticated with no token in storage", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.isStudent).toBe(false);
    expect(result.current.isAdmin).toBe(false);
    expect(result.current.userId).toBeNull();
  });

  it("hydrates auth state from a stored (non-expired) token", () => {
    // A jwt with a far-future `exp` — payload is base64url of {"sub":"u1","role":"Student","exp":9999999999}
    const payload = btoa(JSON.stringify({ sub: "u1", role: "Student", exp: 9999999999 }));
    const token = `header.${payload}.sig`;
    localStorage.setItem("accessToken", token);
    localStorage.setItem("userRole", "Student");
    localStorage.setItem("userId", "u1");
    localStorage.setItem("userName", "Jane Doe");

    const { result } = renderHook(() => useAuth(), { wrapper });
    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.isStudent).toBe(true);
    expect(result.current.userId).toBe("u1");
  });

  it("login() stores auth and updates the context", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });
    act(() => {
      result.current.login("tok", "Administrator", "a1", "Ada Admin");
    });
    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.isAdmin).toBe(true);
    expect(result.current.userName).toBe("Ada Admin");
    expect(localStorage.getItem("accessToken")).toBe("tok");
    expect(localStorage.getItem("userRole")).toBe("Administrator");
  });

  it("logout() clears auth and storage", () => {
    const { result } = renderHook(() => useAuth(), { wrapper });
    act(() => result.current.login("tok", "Student", "s1", "Sue"));
    act(() => result.current.logout());
    expect(result.current.isAuthenticated).toBe(false);
    expect(localStorage.getItem("accessToken")).toBeNull();
  });

  it("throws when used outside the provider", () => {
    expect(() => renderHook(() => useAuth())).toThrow(/useAuth must be used within an AuthProvider/);
  });
});
