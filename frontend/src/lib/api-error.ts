import { AxiosError } from "axios";

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

/**
 * Extracts a human-readable message from any error returned by the API.
 * The ASP.NET backend responds with RFC 7807 ProblemDetails
 * ({ type?, title, status, detail, instance?, errors? }) for both
 * thrown application exceptions and model-validation failures.
 */
export function getApiErrorMessage(error: unknown, fallback = "Something went wrong"): string {
  if (error instanceof AxiosError) {
    const data = error.response?.data as ProblemDetails | undefined;

    // AspNetCore ValidationProblemDetails carries an `errors` dictionary.
    if (data?.errors && typeof data.errors === "object") {
      const messages = Object.values(data.errors).flat();
      if (messages.length > 0) return messages.join(" ");
    }

    if (data?.detail && typeof data.detail === "string" && data.detail.trim()) {
      return data.detail;
    }
    if (data?.title && typeof data.title === "string" && data.title.trim()) {
      return data.title;
    }

    if (error.response) {
      const { status } = error.response;
      if (status === 401) return "You need to sign in to continue.";
      if (status === 403) return "You do not have permission to do that.";
      if (status === 404) return "The requested resource was not found.";
      if (status === 409) return "The request conflicts with the current state.";
      if (status === 422) return "The request could not be processed.";
      return `Request failed with status ${status}.`;
    }

    if (error.request) {
      return "No response from the server. Please check your connection and try again.";
    }
  }

  if (error instanceof Error) return error.message;
  if (typeof error === "string") return error;
  return fallback;
}

export function getApiStatus(error: unknown): number | undefined {
  if (error instanceof AxiosError) return error.response?.status;
  return undefined;
}

export function isConflict(error: unknown): boolean {
  return getApiStatus(error) === 409;
}

export function isUnprocessable(error: unknown): boolean {
  return getApiStatus(error) === 422;
}

export function isNotFound(error: unknown): boolean {
  return getApiStatus(error) === 404;
}
