import { Doughnut } from "react-chartjs-2";
import type { ChartOptions } from "chart.js";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CHART_COLORS } from "@/components/charts/chart-init";

export interface DonutDatum {
  label: string;
  value: number;
  color?: keyof typeof CHART_COLORS;
}

interface DonutChartCardProps {
  title: string;
  data: DonutDatum[];
  height?: number;
  centerLabel?: string;
}

const PALETTE: (keyof typeof CHART_COLORS)[] = ["primary", "amber", "green", "red", "blue", "purple", "gray"];

export function DonutChartCard({ title, data, height = 260, centerLabel }: DonutChartCardProps) {
  const chartData = {
    labels: data.map((d) => d.label),
    datasets: [
      {
        data: data.map((d) => d.value),
        backgroundColor: data.map((d, i) => CHART_COLORS[d.color ?? PALETTE[i % PALETTE.length]]),
        borderWidth: 2,
        borderColor: "rgba(0,0,0,0)",
      },
    ],
  };

  const options: ChartOptions<"doughnut"> = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: "62%",
    plugins: {
      legend: { position: "bottom", labels: { usePointStyle: true, boxWidth: 8 } },
      tooltip: {
        callbacks: {
          label: (ctx) => ` ${ctx.label}: ${Number(ctx.raw)}`,
        },
      },
    },
  };

  const total = data.reduce((sum, d) => sum + d.value, 0);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="relative" style={{ height }}>
          {data.length === 0 || total === 0 ? (
            <div className="flex h-full items-center justify-center text-sm text-muted-foreground">
              No data available
            </div>
          ) : (
            <>
              <Doughnut data={chartData} options={options} />
              {centerLabel && (
                <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
                  <span className="text-2xl font-bold">{total}</span>
                  {centerLabel && <span className="text-xs text-muted-foreground">{centerLabel}</span>}
                </div>
              )}
            </>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
