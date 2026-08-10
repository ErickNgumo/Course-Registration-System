import { Home, RotateCcw } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

interface ErrorPageProps {
  code?: string;
  title: string;
  message?: string;
  details?: string;
  onRetry?: () => void;
  retryLabel?: string;
}

/** Reusable full-page error UI used by the ErrorBoundary, QueryErrorBoundary,
 *  and the 404 / 403 / 500 route pages. */
export function ErrorPage({ code = "Error", title, message, details, onRetry, retryLabel }: ErrorPageProps) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background px-6 text-center">
      <div className="mx-auto max-w-md space-y-6">
        <div className="text-7xl font-extrabold tracking-tighter text-primary/80">{code}</div>
        <div className="space-y-2">
          <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
          {message && <p className="text-muted-foreground">{message}</p>}
          {details && (
            <pre className="mx-auto mt-2 max-w-md overflow-auto rounded-md bg-muted p-3 text-left text-xs text-muted-foreground">
              {details}
            </pre>
          )}
        </div>
        <div className="flex items-center justify-center gap-3">
          {onRetry && (
            <Button onClick={onRetry}>
              <RotateCcw className="h-4 w-4" />
              {retryLabel ?? "Try again"}
            </Button>
          )}
          <Button variant="outline" asChild>
            <Link to="/">
              <Home className="h-4 w-4" />
              Back to home
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
}
