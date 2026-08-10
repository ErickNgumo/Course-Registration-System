import { useAuth } from "@/hooks/useAuth";
import { ErrorPage } from "@/pages/ErrorPage";

export default function UnauthorizedPage() {
  const { isAdmin } = useAuth();
  return (
    <ErrorPage
      code="403"
      title="Access denied"
      message={
        isAdmin
          ? "You need to be signed in as an administrator to view this page."
          : "You need to be signed in as a student to view this page."
      }
    />
  );
}
