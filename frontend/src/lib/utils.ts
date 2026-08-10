import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function getInitials(firstName: string, lastName: string): string {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
}

export const DAY_NAMES: Record<number, string> = {
  0: "Sunday",
  1: "Monday",
  2: "Tuesday",
  3: "Wednesday",
  4: "Thursday",
  5: "Friday",
  6: "Saturday",
};

export function enrollmentStatusColor(status: string): string {
  switch (status) {
    case "Registered":
      return "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400";
    case "Waitlisted":
      return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400";
    case "Dropped":
      return "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400";
    case "Completed":
      return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400";
    default:
      return "bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400";
  }
}

export function studentStatusColor(status: string): string {
  switch (status) {
    case "Active":
      return "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400";
    case "Suspended":
      return "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400";
    case "Inactive":
      return "bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400";
    default:
      return "bg-gray-100 text-gray-800 dark:bg-gray-900/30 dark:text-gray-400";
  }
}

export function gradeColor(grade: string | null): string {
  if (!grade) return "";
  const g = grade.toUpperCase();
  if (g.startsWith("A")) return "text-green-600 dark:text-green-400 font-semibold";
  if (g.startsWith("B")) return "text-blue-600 dark:text-blue-400 font-semibold";
  if (g.startsWith("C")) return "text-amber-600 dark:text-amber-400 font-semibold";
  if (g.startsWith("D") || g.startsWith("F")) return "text-red-600 dark:text-red-400 font-semibold";
  return "";
}