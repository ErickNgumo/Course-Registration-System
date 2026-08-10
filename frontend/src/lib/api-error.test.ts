import { describe, it, expect } from "vitest";
import { AxiosError, AxiosHeaders } from "axios";
import {
  getApiErrorMessage,
  getApiStatus,
  isConflict,
  isNotFound,
  isUnprocessable,
} from "@/lib/api-error";

function axiosError(
  status: number,
  data?: unknown,
): AxiosError {
  return new AxiosError(
    "Request failed",
    "ERR_BAD_REQUEST",
    undefined,
    undefined,
    {
      status,
      data,
      statusText: "",
      headers: {},
      config: { headers: new AxiosHeaders() },
    },
  );
}

describe("getApiErrorMessage", () => {
  it("extracts the validation errors array from a ValidationProblemDetails body", () => {
    const err = axiosError(422, {
      type: "Validation",
      title: "Validation failed",
      errors: {
        email: ["Email is required"],
        password: ["Password is required"],
      },
    });
    expect(getApiErrorMessage(err)).toBe("Email is required Password is required");
  });

  it("prefers `detail` over `title` when present", () => {
    const err = axiosError(409, {
      title: "Conflict",
      detail: "Timetable conflict with CS-101 Lab.",
    });
    expect(getApiErrorMessage(err)).toBe("Timetable conflict with CS-101 Lab.");
  });

  it("falls back to `title` when there is no detail", () => {
    const err = axiosError(404, { title: "Course not found" });
    expect(getApiErrorMessage(err)).toBe("Course not found");
  });

  it("renders a friendly message for common status codes when body is empty", () => {
    expect(getApiErrorMessage(axiosError(401))).toBe("You need to sign in to continue.");
    expect(getApiErrorMessage(axiosError(403))).toBe("You do not have permission to do that.");
    expect(getApiErrorMessage(axiosError(404))).toBe("The requested resource was not found.");
    expect(getApiErrorMessage(axiosError(409))).toBe("The request conflicts with the current state.");
  });

  it("reports a network failure when no response was received", () => {
    // axios sets `request` (but leaves `response` undefined) when the request
    // was sent but no response came back — e.g. a network/CORS/DNS failure.
    const networkErr = new AxiosError("Network Error", "ERR_NETWORK", undefined, {});
    expect(getApiErrorMessage(networkErr)).toBe(
      "No response from the server. Please check your connection and try again.",
    );
  });

  it("falls back to the Error.message for non-axios errors", () => {
    expect(getApiErrorMessage(new Error("boom"))).toBe("boom");
  });
});

describe("status predicates", () => {
  it("getApiStatus returns the response status for axios errors", () => {
    expect(getApiStatus(axiosError(404))).toBe(404);
    expect(getApiStatus(new Error("nope"))).toBeUndefined();
  });

  it("isConflict / isNotFound / isUnprocessable detect specific status codes", () => {
    expect(isConflict(axiosError(409))).toBe(true);
    expect(isConflict(axiosError(200))).toBe(false);
    expect(isNotFound(axiosError(404))).toBe(true);
    expect(isUnprocessable(axiosError(422))).toBe(true);
  });
});
