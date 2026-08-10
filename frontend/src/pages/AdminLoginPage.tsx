import { useState } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Shield } from "lucide-react";
import { useAuth } from "@/hooks/useAuth";
import { authService } from "@/services/auth.service";
import { getApiErrorMessage } from "@/lib/api-error";
import { loginSchema, type LoginValues } from "@/lib/schemas";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useToast } from "@/hooks/use-toast";
import { AuthShell } from "@/pages/LoginPage";

export default function AdminLoginPage() {
  const { isAuthenticated, login, role } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [submitting, setSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const onSubmit = async (values: LoginValues) => {
    setSubmitting(true);
    try {
      const res = await authService.adminLogin(values);
      login(
        res.accessToken,
        "Administrator",
        res.administrator.id,
        `${res.administrator.firstName} ${res.administrator.lastName}`,
      );
      toast({ title: "Welcome back", description: `${res.administrator.firstName} ${res.administrator.lastName}` });
      navigate("/admin/dashboard", { replace: true });
    } catch (error) {
      const message = getApiErrorMessage(error, "Invalid administrator credentials.");
      setError("password", { message });
      toast({ variant: "destructive", title: "Sign-in failed", description: message });
    } finally {
      setSubmitting(false);
    }
  };

  if (isAuthenticated) {
    return <Navigate to={role === "Administrator" ? "/admin/dashboard" : "/dashboard"} replace />;
  }

  return (
    <AuthShell
      title="Administrator Sign In"
      description="Manage students, courses, and enrollments."
      icon={<Shield className="h-7 w-7 text-primary" />}
      footer={
        <p className="text-sm text-muted-foreground">
          Student?{" "}
          <Link to="/login" className="font-medium text-primary hover:underline">
            Sign in here
          </Link>
        </p>
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <div className="space-y-2">
          <Label htmlFor="admin-email">Email</Label>
          <Input
            id="admin-email"
            type="email"
            autoComplete="email"
            placeholder="admin@university.edu"
            aria-invalid={!!errors.email}
            aria-describedby={errors.email ? "admin-email-error" : undefined}
            {...register("email")}
          />
          {errors.email && (
            <p id="admin-email-error" className="text-sm text-destructive" role="alert">
              {errors.email.message}
            </p>
          )}
        </div>
        <div className="space-y-2">
          <Label htmlFor="admin-password">Password</Label>
          <Input
            id="admin-password"
            type="password"
            autoComplete="current-password"
            aria-invalid={!!errors.password}
            aria-describedby={errors.password ? "admin-password-error" : undefined}
            {...register("password")}
          />
          {errors.password && (
            <p id="admin-password-error" className="text-sm text-destructive" role="alert">
              {errors.password.message}
            </p>
          )}
        </div>
        <Button type="submit" className="w-full" disabled={submitting}>
          {submitting ? "Signing in…" : "Sign in to admin"}
        </Button>
      </form>
    </AuthShell>
  );
}
