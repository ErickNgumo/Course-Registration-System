import { describe, it, expect } from "vitest";
import { queryKeys } from "@/lib/query-keys";

describe("queryKeys", () => {
  it("returns stable hierarchical key arrays", () => {
    expect(queryKeys.auth).toEqual(["auth"]);
    expect(queryKeys.me()).toEqual(["auth", "me"]);
    expect(queryKeys.studentDashboard()).toEqual(["student", "dashboard"]);
  });

  it("uses the given id in the course/student keys", () => {
    expect(queryKeys.course("abc")).toEqual(["courses", "abc"]);
    expect(queryKeys.adminStudent("xyz")).toEqual(["admin", "students", "xyz"]);
  });

  it("embeds params into admin collection keys for cache partitioning", () => {
    const k = queryKeys.adminStudents({ page: 1, search: "jane" });
    expect(k[0]).toBe("admin");
    expect(k[1]).toBe("students");
    expect(k[2]).toEqual({ page: 1, search: "jane" });
  });

  it("separates report keys by report kind", () => {
    expect(queryKeys.enrollmentReport()).toEqual(["admin", "reports", "enrollment"]);
    expect(queryKeys.waitlistReport()).toEqual(["admin", "reports", "waitlist"]);
  });
});
