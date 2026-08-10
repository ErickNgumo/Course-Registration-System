import { describe, it, expect } from "vitest";
import {
  cn,
  formatDate,
  formatDateTime,
  getInitials,
  DAY_NAMES,
  enrollmentStatusColor,
  studentStatusColor,
  gradeColor,
} from "@/lib/utils";

describe("cn", () => {
  it("merges class names and de-duplicates tailwind conflicts", () => {
    expect(cn("px-2", "px-4")).toBe("px-4");
    expect(cn("text-red-500", false && "hidden", "bg-white")).toBe("text-red-500 bg-white");
  });
});

describe("formatDate / formatDateTime", () => {
  it("formats an ISO date into a readable short date", () => {
    const out = formatDate("2025-08-07T10:30:00Z");
    expect(out).toMatch(/2025$/);
    expect(out.length).toBeGreaterThan(0);
  });

  it("formats date-time including time of day", () => {
    const out = formatDateTime("2025-08-07T10:30:00Z");
    expect(out.length).toBeGreaterThan(0);
  });
});

describe("getInitials", () => {
  it("returns uppercase initials from first and last names", () => {
    expect(getInitials("Erick", "Ngumo")).toBe("EN");
    expect(getInitials("ada", "lovelace")).toBe("AL");
  });
});

describe("DAY_NAMES", () => {
  it("maps 0–6 to Sunday–Saturday", () => {
    expect(DAY_NAMES[0]).toBe("Sunday");
    expect(DAY_NAMES[3]).toBe("Wednesday");
    expect(DAY_NAMES[6]).toBe("Saturday");
  });
});

describe("status colour helpers", () => {
  it("returns a class string for each enrollment status", () => {
    const statuses = ["Registered", "Waitlisted", "Dropped", "Completed", "Unknown"];
    for (const s of statuses) {
      expect(enrollmentStatusColor(s).length).toBeGreaterThan(0);
    }
  });

  it("returns a class string for each student status", () => {
    for (const s of ["Active", "Suspended", "Inactive", "Unknown"]) {
      expect(studentStatusColor(s).length).toBeGreaterThan(0);
    }
  });
});

describe("gradeColor", () => {
  it("tints letter grades and leaves unknowns empty", () => {
    expect(gradeColor("A")).toContain("text-green");
    expect(gradeColor("B+")).toContain("text-blue");
    expect(gradeColor("C")).toContain("text-amber");
    expect(gradeColor("F")).toContain("text-red");
    expect(gradeColor(null)).toBe("");
  });
});
