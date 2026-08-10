import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { Pagination } from "@/components/Pagination";
import type { PagedResponse } from "@/types/api";

const data: PagedResponse<unknown> = {
  items: [],
  page: 2,
  pageSize: 10,
  totalItems: 45,
  totalPages: 5,
  hasNext: true,
  hasPrevious: true,
};

const nop = () => {};

describe("Pagination", () => {
  it("shows the range of visible items and total count", () => {
    render(<Pagination data={data} onPageChange={nop} onPageSizeChange={nop} />);
    // (page-1)*pageSize+1 .. page*pageSize = 11-20 of 45
    expect(screen.getByText(/11/)).toBeInTheDocument();
    expect(screen.getByText(/20/)).toBeInTheDocument();
    expect(screen.getByText(/45/)).toBeInTheDocument();
  });

  it("shows the current page and total pages", () => {
    render(<Pagination data={data} onPageChange={nop} onPageSizeChange={nop} />);
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("5")).toBeInTheDocument();
  });

  it("calls onPageChange with the right page for next/prev/first/last", () => {
    const onPageChange = vi.fn();
    render(<Pagination data={data} onPageChange={onPageChange} onPageSizeChange={nop} />);
    fireEvent.click(screen.getByRole("button", { name: "Next page" }));
    expect(onPageChange).toHaveBeenLastCalledWith(3);
    fireEvent.click(screen.getByRole("button", { name: "Previous page" }));
    expect(onPageChange).toHaveBeenLastCalledWith(1);
    fireEvent.click(screen.getByRole("button", { name: "First page" }));
    expect(onPageChange).toHaveBeenLastCalledWith(1);
    fireEvent.click(screen.getByRole("button", { name: "Last page" }));
    expect(onPageChange).toHaveBeenLastCalledWith(5);
  });

  it("disables prev/first on the first page", () => {
    const first: PagedResponse<unknown> = { ...data, page: 1, hasPrevious: false };
    render(<Pagination data={first} onPageChange={nop} onPageSizeChange={nop} />);
    expect(screen.getByRole("button", { name: "Previous page" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "First page" })).toBeDisabled();
  });
});
