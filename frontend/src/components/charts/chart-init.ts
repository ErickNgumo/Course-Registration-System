import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler,
  type ChartOptions,
} from "chart.js";

// Register every controller/element used across the reports once.
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler,
);

export const baseChartOptions: ChartOptions<"bar"> = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: { mode: "index", intersect: false },
  },
  scales: {
    x: { grid: { display: false } },
    y: { beginAtZero: true, ticks: { precision: 0 } },
  },
};

// Brand palette aligned to the blue/white university theme + status colors.
export const CHART_COLORS = {
  primary: "rgba(37, 99, 235, 0.85)", // blue-600
  primarySoft: "rgba(37, 99, 235, 0.2)",
  amber: "rgba(217, 119, 6, 0.85)",
  green: "rgba(22, 163, 74, 0.85)",
  red: "rgba(220, 38, 38, 0.85)",
  gray: "rgba(107, 114, 128, 0.85)",
  blue: "rgba(59, 130, 246, 0.85)",
  purple: "rgba(147, 51, 234, 0.85)",
};
