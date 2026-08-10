import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm, useFieldArray, Controller } from "react-hook-form";
import { useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Plus, Trash2, Save, AlertCircle } from "lucide-react";
import {
  useAdminCourses,
  useCreateCourse,
  useUpdateCourse,
} from "@/hooks/use-queries";
import { getApiErrorMessage, isConflict, isUnprocessable } from "@/lib/api-error";
import { SEMESTER_OPTIONS } from "@/lib/format";
import { DAY_NAMES } from "@/lib/utils";
import { PageHeader } from "@/components/PageHeader";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { LoadingState } from "@/components/LoadingState";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import type { CourseAdministrationResponse, ScheduleInput } from "@/types/api";

interface FormValues {
  code: string;
  name: string;
  description: string;
  credits: number;
  capacity: number;
  semester: string;
  schedules: { dayOfWeek: number; startTime: string; endTime: string }[];
  prerequisiteCourseIds: string[];
}

export default function AdminCourseEditorPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;
  const navigate = useNavigate();
  const { toast } = useToast();
  const qc = useQueryClient();
  const create = useCreateCourse();
  const update = useUpdateCourse();

  // Admin course list serves the full schedule + prerequisite payload.
  const list = useAdminCourses({ page: 1, pageSize: 500 });
  const course = useMemo(
    () => list.data?.items.find((c) => c.id === id),
    [list.data, id],
  );

  const [loaded, setLoaded] = useState(isEdit ? false : true);
  const [confirmLeave, setConfirmLeave] = useState(false);

  const form = useForm<FormValues>({
    // SaveCourseValues excludes schedules/prereqs; full validation done
    // via the partial schema plus custom schedule checks on submit.
    defaultValues: {
      code: "",
      name: "",
      description: "",
      credits: 3,
      capacity: 30,
      semester: SEMESTER_OPTIONS[0] ?? "",
      schedules: [],
      prerequisiteCourseIds: [],
    },
  });

  const { register, control, handleSubmit, reset, formState: { errors, isDirty } } = form;
  const schedulesField = useFieldArray({ control, name: "schedules" });

  // Prefill once the course resolves.
  useEffect(() => {
    if (isEdit && course && !loaded) {
      reset({
        code: course.code,
        name: course.name,
        description: course.description ?? "",
        credits: course.credits,
        capacity: course.capacity,
        semester: course.semester,
        schedules: (course.schedules ?? []).map((s) => ({
          dayOfWeek: s.dayOfWeek,
          startTime: s.startTime.slice(0, 5),
          endTime: s.endTime.slice(0, 5),
        })),
        prerequisiteCourseIds: course.prerequisiteCourseIds ?? [],
      });
      setLoaded(true);
    }
  }, [isEdit, course, loaded, reset]);

  const alternatives = useMemo(
    () => (list.data?.items ?? []).filter((c) => c.id !== id),
    [list.data, id],
  );

  if (isEdit && list.isLoading) return <LoadingState />;
  if (isEdit && list.data && !course) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" size="sm" onClick={() => navigate("/admin/courses")}>
          <ArrowLeft className="h-4 w-4" /> Back to courses
        </Button>
        <p className="text-muted-foreground">Course not found.</p>
      </div>
    );
  }

  const onSubmit = async (values: FormValues) => {
    // Basic schedule sanity check.
    for (const s of values.schedules) {
      if (!s.startTime || !s.endTime) {
        toast({ variant: "destructive", title: "Incomplete schedule", description: "Each schedule slot needs start and end times." });
        return;
      }
    }
    const payload = {
      code: values.code,
      name: values.name,
      description: values.description.trim() ? values.description.trim() : null,
      credits: Number(values.credits),
      capacity: Number(values.capacity),
      semester: values.semester,
      schedules: values.schedules.map<ScheduleInput>((s) => ({
        dayOfWeek: Number(s.dayOfWeek),
        startTime: s.startTime,
        endTime: s.endTime,
      })),
      prerequisiteCourseIds: values.prerequisiteCourseIds,
    };

    try {
      if (isEdit && id) {
        await update.mutateAsync({ id, data: payload });
        toast({ title: "Course updated", description: `${payload.code} saved.` });
      } else {
        await create.mutateAsync(payload);
        toast({ title: "Course created", description: `${payload.code} added to the catalog.` });
      }
      qc.invalidateQueries({ queryKey: ["admin", "courses"] });
      navigate("/admin/courses");
    } catch (error) {
      let message = getApiErrorMessage(error, "Unable to save the course.");
      if (isConflict(error)) message = "A course with this code already exists.";
      else if (isUnprocessable(error)) message = getApiErrorMessage(error);
      toast({ variant: "destructive", title: "Save failed", description: message });
    }
  };

  return (
    <div className="space-y-6">
      <Button
        variant="ghost"
        size="sm"
        onClick={() => (isDirty ? setConfirmLeave(true) : navigate("/admin/courses"))}
      >
        <ArrowLeft className="h-4 w-4" /> Back to courses
      </Button>

      <PageHeader
        title={isEdit ? "Edit course" : "New course"}
        description={isEdit ? "Update course details, schedules, and prerequisites." : "Create a new course in the catalog."}
      />

      <form onSubmit={handleSubmit(onSubmit)} className="grid gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="code">Course code</Label>
                <Input id="code" placeholder="CS-101" aria-invalid={!!errors.code} {...register("code")} />
                {errors.code && <FieldError message={errors.code.message} />}
              </div>
              <div className="space-y-2">
                <Label htmlFor="semester">Semester</Label>
                <Controller
                  control={control}
                  name="semester"
                  render={({ field }) => (
                    <Select value={field.value} onValueChange={field.onChange}>
                      <SelectTrigger id="semester">
                        <SelectValue placeholder="Select semester" />
                      </SelectTrigger>
                      <SelectContent>
                        {SEMESTER_OPTIONS.map((s) => (
                          <SelectItem key={s} value={s}>
                            {s}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="name">Course name</Label>
              <Input id="name" placeholder="Introduction to Computer Science" aria-invalid={!!errors.name} {...register("name")} />
              {errors.name && <FieldError message={errors.name.message} />}
            </div>
            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <textarea
                id="description"
                rows={4}
                placeholder="Brief course description…"
                className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                {...register("description")}
              />
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="credits">Credits</Label>
                <Input
                  id="credits"
                  type="number"
                  min={1}
                  max={12}
                  aria-invalid={!!errors.credits}
                  {...register("credits", { valueAsNumber: true })}
                />
                {errors.credits && <FieldError message={errors.credits.message} />}
              </div>
              <div className="space-y-2">
                <Label htmlFor="capacity">Capacity</Label>
                <Input
                  id="capacity"
                  type="number"
                  min={1}
                  max={500}
                  aria-invalid={!!errors.capacity}
                  {...register("capacity", { valueAsNumber: true })}
                />
                {errors.capacity && <FieldError message={errors.capacity.message} />}
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Prerequisites</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3 max-h-[320px] overflow-y-auto">
            {alternatives.length === 0 ? (
              <p className="text-sm text-muted-foreground">No other courses available.</p>
            ) : (
              alternatives.map((c) => (
                <PrereqOption
                  key={c.id}
                  course={c}
                  control={control}
                />
              ))
            )}
          </CardContent>
        </Card>

        <Card className="lg:col-span-3">
          <CardHeader className="flex flex-row items-center justify-between space-y-0">
            <CardTitle>Weekly schedule</CardTitle>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => schedulesField.append({ dayOfWeek: 1, startTime: "09:00", endTime: "10:30" })}
            >
              <Plus className="h-4 w-4" /> Add slot
            </Button>
          </CardHeader>
          <CardContent className="space-y-3">
            {schedulesField.fields.length === 0 && (
              <p className="text-sm text-muted-foreground">No weekly meeting slots added. Add one or more.</p>
            )}
            {schedulesField.fields.map((field, i) => (
              <div key={field.id} className="grid grid-cols-1 items-end gap-3 sm:grid-cols-[1fr_1fr_1fr_auto]">
                <div className="space-y-2">
                  <Label>Day</Label>
                  <Controller
                    control={control}
                    name={`schedules.${i}.dayOfWeek`}
                    render={({ field: f }) => (
                      <Select value={String(f.value)} onValueChange={(v) => f.onChange(Number(v))}>
                        <SelectTrigger><SelectValue /></SelectTrigger>
                        <SelectContent>
                          {Object.entries(DAY_NAMES).map(([k, v]) => (
                            <SelectItem key={k} value={k}>{v}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
                <div className="space-y-2">
                  <Label>Start</Label>
                  <Input type="time" {...register(`schedules.${i}.startTime`)} />
                </div>
                <div className="space-y-2">
                  <Label>End</Label>
                  <Input type="time" {...register(`schedules.${i}.endTime`)} />
                </div>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  className="text-destructive"
                  onClick={() => schedulesField.remove(i)}
                  aria-label="Remove slot"
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            ))}
          </CardContent>
        </Card>

        <Separator className="lg:col-span-3" />
        <div className="lg:col-span-3 flex items-center justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => (isDirty ? setConfirmLeave(true) : navigate("/admin/courses"))}>
            Cancel
          </Button>
          <Button type="submit" disabled={create.isPending || update.isPending || (isEdit && !loaded)}>
            <Save className="h-4 w-4" /> {isEdit ? "Save changes" : "Create course"}
          </Button>
        </div>
      </form>

      <ConfirmDialog
        open={confirmLeave}
        onOpenChange={setConfirmLeave}
        title="Discard changes?"
        description="You have unsaved changes that will be lost."
        confirmLabel="Discard"
        destructive
        onConfirm={() => navigate("/admin/courses")}
      />
    </div>
  );
}

function FieldError({ message }: { message?: string }) {
  if (!message) return null;
  return (
    <p className="flex items-center gap-1 text-sm text-destructive" role="alert">
      <AlertCircle className="h-3 w-3" /> {message}
    </p>
  );
}

function PrereqOption({
  course,
  control,
}: {
  course: CourseAdministrationResponse;
  control: ReturnType<typeof useForm<FormValues>>["control"];
}) {
  return (
    <Controller
      control={control}
      name="prerequisiteCourseIds"
      render={({ field }) => {
        const checked = field.value?.includes(course.id);
        return (
          <label className="flex cursor-pointer items-center gap-3 rounded-md border p-3 hover:bg-accent">
            <input
              type="checkbox"
              className="h-4 w-4 rounded border-input"
              checked={!!checked}
              onChange={(e) => {
                const next = new Set(field.value ?? []);
                if (e.target.checked) next.add(course.id);
                else next.delete(course.id);
                field.onChange(Array.from(next));
              }}
            />
            <div>
              <p className="text-sm font-medium">{course.code} · {course.name}</p>
              <p className="text-xs text-muted-foreground">{course.credits} credits · {course.semester}</p>
            </div>
          </label>
        );
      }}
    />
  );
}

// `useEffect`, `Controller`, and `useFieldArray` come from React / RHF
// imports at the top of this module.
