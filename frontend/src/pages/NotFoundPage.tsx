import { useNavigate } from "react-router-dom";
import { ErrorPage } from "@/pages/ErrorPage";

export default function NotFoundPage() {
  const navigate = useNavigate();
  return (
    <ErrorPage
      code="404"
      title="Page not found"
      message="The page you were looking for doesn't exist or has moved."
      onRetry={() => navigate(-1)}
      retryLabel="Go back"
    />
  );
}
