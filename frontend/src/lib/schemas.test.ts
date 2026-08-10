import { describe, it, expect } from "vitest";
import { loginSchema, assignGradeSchema, saveCourseSchema } from "@/lib/schemas";

describe("loginSchema", () => {
  it("accepts a valid email + password", () => {
    const r = loginSchema.safeParse({ email: "a@b.edu", password: "secret" });
    expect(r.success).toBe(true);
  });

  it("rejects an empty email and a non-email string", () => {
    expect(loginSchema.safeParse({ email: "", password: "x" }).success).toBe(false);
    expect(loginSchema.safeParse({ email: "not-an-email", password: "x" }).success).toBe(false);
  });

  it("rejects an empty password", () => {
    expect(loginSchema.safeParse({ email: "a@b.edu", password: "" }).success).toBe(false);
  });
});

describe("assignGradeSchema", () => {
  it("accepts letter grades with optional +/- and uppercases them", () => {
    const a = assignGradeSchema.safeParse({ finalGrade: "b+" });
    expect(a.success).toBe(true);
    if (a.success) expect(a.data.finalGrade).toBe("B+");
  });

  it("normalises an empty string to null", () => {
    const a = assignGradeSchema.safeParse({ finalGrade: "" });
    expect(a.success).toBe(true);
    if (a.success) expect(a.data.finalGrade).toBeNull();
  });

  it("rejects invalid grades like 'Z' or 'AB'", () => {
    expect(assignGradeSchema.safeParse({ finalGrade: "Z" }).success).toBe(false);
    expect(assignGradeSchema.safeParse({ finalGrade: "AB" }).success).toBe(false);
  });
});

describe("saveCourseSchema", () => {
  const valid = {
    code: "CS-101",
    name: "Intro to CS",
    description: null,
    credits: 3,
    capacity: 30,
    semester: "Fall 2025",
  };

  it("accepts a valid course payload", () => {
    expect(saveCourseSchema.safeParse(valid).success).toBe(true);
  });

  it("coerces numeric-string credits and capacity", () => {
    const r = saveCourseSchema.safeParse({ ...valid, credits: "3", capacity: "30" });
    expect(r.success).toBe(true);
  });

  it("rejects illegal characters in the code", () => {
    expect(saveCourseSchema.safeParse({ ...valid, code: "CS 101!" }).success).toBe(false);
  });

  it("rejects credits outside the 1–12 range", () => {
    expect(saveCourseSchema.safeParse({ ...valid, credits: 0 }).success).toBe(false);
    expect(saveCourseSchema.safeParse({ ...valid, credits: 13 }).success).toBe(false);
  });
});
