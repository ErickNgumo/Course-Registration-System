import { useMemo, useState } from "react";
import { BookOpen, Calendar, Search } from "lucide-react";
import { Link } from "react-router-dom";
import { useCourses } from "@/hooks/use-queries";
import { PageHeader } from "@/components/PageHeader";
import { EmptyState } from "@/components/EmptyState";
import { CardSkeleton } from "@/components/LoadingState";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Pagination } from "@/components/Pagination";
import { CardSkeletonGrid } from "@/components/CardSkeletonGrid";

const PAGE_SIZE = 9;

export default function CoursesPage() {
  const { data: courses, isLoading } = useCourses();
  const [query, setQuery] = useState("");
  const [semester, setSemester] = useState("all");
  const [page, setPage] = useState(1);

  const semesters = useMemo(() => {
    const set = new Set<string>();
    courses?.forEach((c) => set.add(c.semester));
    return Array.from(set).sort();
  }, [courses]);

  const filtered = useMemo(() => {
    if (!courses) return [];
    const q = query.trim().toLowerCase();
    return courses.filter((c) => {
      const matchesQuery =
        !q ||
        c.code.toLowerCase().includes(q) ||
        c.name.toLowerCase().includes(q) ||
        (c.description ?? "").toLowerCase().includes(q);
      const matchesSemester = semester === "all" || c.semester === semester;
      return matchesQuery && matchesSemester;
    });
  }, [courses, query, semester]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const currentPage = Math.min(page, totalPages);
  const paged = filtered.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);

  return (
    <div className="space-y-6">
      <PageHeader title="Course Catalog" description="Browse and register for available courses." />

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by name, code, or description"
            className="pl-9"
            value={query}
            onChange={(e) => {
              setQuery(e.target.value);
              setPage(1);
            }}
            aria-label="Search courses"
          />
        </div>
        <div className="flex items-center gap-2">
          <label htmlFor="semester-filter" className="text-sm text-muted-foreground">
            Semester
          </label>
          <select
            id="semester-filter"
            className="h-10 rounded-md border border-input bg-background px-3 text-sm"
            value={semester}
            onChange={(e) => {
              setSemester(e.target.value);
              setPage(1);
            }}
          >
            <option value="all">All</option>
            {semesters.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </div>
      </div>

      {isLoading ? (
        <CardSkeletonGrid count={6} />
      ) : paged.length === 0 ? (
        <EmptyState
          icon={BookOpen}
          title="No courses found"
          description="Try adjusting your search or semester filter."
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {paged.map((c) => (
            <Card key={c.id} className="flex flex-col">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <Badge variant="secondary">{c.code}</Badge>
                  <span className="flex items-center gap-1 text-xs text-muted-foreground">
                    <Calendar className="h-3 w-3" />
                    {c.semester}
                  </span>
                </div>
                <h3 className="mt-2 text-lg font-semibold leading-snug">{c.name}</h3>
              </CardHeader>
              <CardContent className="flex-1">
                <p className="line-clamp-3 text-sm text-muted-foreground">
                  {c.description ?? "No description available."}
                </p>
                <div className="mt-3 flex items-center gap-2 text-sm">
                  <Badge variant="outline">{c.credits} credits</Badge>
                  <Badge variant="outline">Cap {c.capacity}</Badge>
                </div>
              </CardContent>
              <CardFooter>
                <Button asChild variant="outline" className="w-full">
                  <Link to={`/courses/${c.id}`}>View details</Link>
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      )}

      {!isLoading && filtered.length > 0 && (
        <Pagination
          data={{
            items: paged,
            page: currentPage,
            pageSize: PAGE_SIZE,
            totalItems: filtered.length,
            totalPages,
            hasNext: currentPage < totalPages,
            hasPrevious: currentPage > 1,
          }}
          onPageChange={setPage}
          onPageSizeChange={() => {}}
        />
      )}
    </div>
  );
}
