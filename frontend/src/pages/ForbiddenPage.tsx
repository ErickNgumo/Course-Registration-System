import { ErrorPage } from "@/pages/ErrorPage";

export default function ServerErrorPage() {
  return (
    <ErrorPage
      code="500"
      title="Server error"
      message="Something went wrong on our end. Please try again in a moment."
    />
  );
}
