import { Component, type ErrorInfo, type ReactNode } from "react";
import { ErrorPage } from "@/pages/ErrorPage";

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

/** Catches render-time errors and shows a friendly fallback. */
export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    // eslint-disable-next-line no-console
    console.error("Uncaught render error:", error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <ErrorPage
          code="500"
          title="Something broke"
          message="An unexpected error occurred while rendering this page."
          details={this.state.error?.message}
        />
      );
    }
    return this.props.children;
  }
}
