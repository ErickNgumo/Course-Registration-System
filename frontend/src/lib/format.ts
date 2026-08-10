/** Suggested semester labels for the course editor dropdown and filters. */
export const SEMESTER_OPTIONS: string[] = [
  "Fall 2025",
  "Spring 2026",
  "Summer 2026",
  "Fall 2026",
  "Spring 2027",
];

export const SORT_DIRECTIONS = ["asc", "desc"] as const;
export type SortDirection = (typeof SORT_DIRECTIONS)[number];

/** Builds a "<key>:<dir>" sort string for the admin `sortBy` query param. */
export function buildSort(key: string, dir: SortDirection = "asc"): string {
  return `${key}:${dir}`;
}

export function parseSort(sortBy?: string): { key: string; dir: SortDirection } {
  if (!sortBy) return { key: "", dir: "asc" };
  const [key, dirRaw] = sortBy.split(":");
  const dir = dirRaw === "desc" ? "desc" : "asc";
  return { key: key ?? "", dir };
}
