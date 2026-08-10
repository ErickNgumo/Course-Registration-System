import { describe, it, expect, beforeEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useToast, toast } from "@/hooks/use-toast";

beforeEach(() => {
  // Dismiss any lingering toasts between tests.
  const { result } = renderHook(() => useToast());
  act(() => result.current.dismiss());
});

describe("useToast", () => {
  it("adds a toast to the global queue", () => {
    const { result } = renderHook(() => useToast());
    act(() => {
      toast({ title: "Hello", description: "World" });
    });
    const added = result.current.toasts.find((t) => t.title === "Hello");
    expect(added).toBeDefined();
    expect(added?.description).toBe("World");
  });

  it("dismisses a toast by id", () => {
    const { result } = renderHook(() => useToast());
    let id = "";
    act(() => {
      const { id: created } = toast({ title: "Bye" });
      id = created;
    });
    act(() => result.current.dismiss(id));
    const target = result.current.toasts.find((t) => t.id === id);
    expect(target?.open).toBe(false);
  });

  it("limits the queue to TOAST_LIMIT (4) toasts", () => {
    const { result } = renderHook(() => useToast());
    act(() => {
      for (let i = 0; i < 10; i++) toast({ title: `t${i}` });
    });
    expect(result.current.toasts.length).toBeLessThanOrEqual(4);
  });
});
