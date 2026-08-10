import { z } from "zod";

export const loginSchema = z.object({
  email: z.string().min(1, "Email is required").email("Enter a valid email address"),
  password: z.string().min(1, "Password is required"),
});
export type LoginValues = z.infer<typeof loginSchema>;

export const saveCourseSchema = z.object({
  code: z
    .string()
    .min(1, "Course code is required")
    .max(20, "Course code must be 20 characters or fewer")
    .regex(/^[A-Za-z0-9-]+$/, "Use only letters, numbers, and dashes"),
  name: z
    .string()
    .min(1, "Course name is required")
    .max(200, "Course name must be 200 characters or fewer"),
  description: z.string().max(2000, "Description is too long").nullable().optional(),
  credits: z.coerce.number().int().min(1, "Credits must be at least 1").max(12, "Credits must be 12 or fewer"),
  capacity: z.coerce.number().int().min(1, "Capacity must be at least 1").max(500, "Capacity must be 500 or fewer"),
  semester: z
    .string()
    .min(1, "Semester is required")
    .max(50, "Semester is too long")
    .regex(/^[A-Za-z0-9\s-]+$/, "Use letters, numbers, spaces, and dashes"),
});
export type SaveCourseValues = z.infer<typeof saveCourseSchema>;

export const assignGradeSchema = z.object({
  finalGrade: z
    .string()
    .trim()
    .refine(
      (v) => v === "" || /^[A-F][+-]?$/.test(v.toUpperCase()),
      "Grade must be a letter grade like A, B+, or C-",
    )
    .transform((v) => (v === "" ? null : v.toUpperCase())),
});
export type AssignGradeValues = z.infer<typeof assignGradeSchema>;
