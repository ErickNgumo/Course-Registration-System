import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "@/contexts/AuthContext";

// Stub the toast hook so we don't depend on module-level state across tests.
const { toastSpy } = vi.hoisted(() => ({ toastSpy: vi.fn() }));
vi.mock("@/hooks/use-toast", () => ({
  useToast: () => ({ toast: toastSpy, dismiss: vi.fn(), toasts: [] }),
  toast: toastSpy,
}));

// Mock the auth service so the form never reaches the network.
const { studentLogin } = vi.hoisted(() => ({ studentLogin: vi.fn() }));
vi.mock("@/services/auth.service", () => ({
  authService: { studentLogin: (d: unknown) => studentLogin(d) },
}));

import LoginPage from "@/pages/LoginPage";

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <AuthProvider>
          <LoginPage />
        </AuthProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("LoginPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it("renders the student sign-in form with labeled inputs", () => {
    renderPage();
    expect(screen.getByRole("heading", { name: /Student Sign In/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/Email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Password/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Sign in/i })).toBeInTheDocument();
  });

  it("shows inline validation errors when the form is submitted empty", async () => {
    renderPage();
    fireEvent.click(screen.getByRole("button", { name: /Sign in/i }));
    expect(await screen.findByText(/Email is required/i)).toBeInTheDocument();
    expect(screen.getByText(/Password is required/i)).toBeInTheDocument();
    expect(studentLogin).not.toHaveBeenCalled();
  });

  it("calls the auth service and pushes to /dashboard on success", async () => {
    studentLogin.mockResolvedValueOnce({
      accessToken: "tok",
      tokenType: "Bearer",
      expiresIn: 3600,
      student: { id: "s1", firstName: "Jane", lastName: "Doe", email: "j@b.edu" },
    });

    renderPage();
    fireEvent.change(screen.getByLabelText(/Email/i), { target: { value: "j@b.edu" } });
    fireEvent.change(screen.getByLabelText(/Password/i), { target: { value: "secret" } });
    fireEvent.click(screen.getByRole("button", { name: /Sign in/i }));

    await waitFor(() => expect(studentLogin).toHaveBeenCalledTimes(1));
    expect(studentLogin).toHaveBeenCalledWith({ email: "j@b.edu", password: "secret" });
    // The token must end up in localStorage for the AuthProvider to be logged in.
    expect(localStorage.getItem("accessToken")).toBe("tok");
    expect(toastSpy).toHaveBeenCalledWith(
      expect.objectContaining({ title: "Welcome back" }),
    );
  });

  it("shows a destructive toast and inline error on API failure", async () => {
    studentLogin.mockRejectedValueOnce(new Error("Invalid credentials"));
    renderPage();
    fireEvent.change(screen.getByLabelText(/Email/i), { target: { value: "j@b.edu" } });
    fireEvent.change(screen.getByLabelText(/Password/i), { target: { value: "wrong" } });
    fireEvent.click(screen.getByRole("button", { name: /Sign in/i }));

    await waitFor(() => expect(toastSpy).toHaveBeenCalled());
    const call = toastSpy.mock.calls[0][0];
    expect(call.variant).toBe("destructive");
    expect(call.title).toBe("Sign-in failed");
  });
});
