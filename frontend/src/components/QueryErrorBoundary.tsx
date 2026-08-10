import { useQueryErrorResetBoundary } from "@tanstack/react-query";
import { Component, type ErrorInfo, type ReactNode } from "react";
import { ErrorPage } from "@/pages/ErrorPage";

interface Props {
  children: ReactNode;
  message?: string;
}

/** Wraps a subtree so that any thrown React Query mutation/query error
 *  surfaces a recoverable error UI instead of an unhandled exception. */
export function QueryErrorBoundary({ children, message }: Props) {
  const { reset } = useQueryErrorResetBoundary();
  return (
    <ErrorBoundaryCatcher reset={reset} message={message}>
      {children}
    </ErrorBoundaryCatcher>
  );
}

interface CatcherProps {
  children: ReactNode;
  reset: () => void;
  message?: string;
}

interface CatcherState {
  errored: boolean;
}

class ErrorBoundaryCatcher extends Component<CatcherProps, CatcherState> {
  state: CatcherState = { errored: false };

  static getDerivedStateFromError(): CatcherState {
    return { errored: true };
  }

  componentDidCatch(_error: Error, _info: ErrorInfo) {
    // No-op: TanStack owns the reset token passed in via props.
  }

  handleRetry = () => {
    this.props.reset();
    this.setState({ errored: false });
  };

  render() {
    if (this.state.errored) {
      return (
        <ErrorPage
          code="500"
          title="Load failure"
          message={this.props.message ?? "We couldn't load this content."}
          onRetry={this.handleRetry}
          retryLabel="Try again"
        />
      );
    }
    return this.props.children;
  }
}
