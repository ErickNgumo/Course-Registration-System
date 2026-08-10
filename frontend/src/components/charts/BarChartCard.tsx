import { Bar } from "react-chartjs-2";
import type { ChartOptions } from "chart.js";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { baseChartOptions, CHART_COLORS } from "@/components/charts/chart-init";

export interface BarDatum {
  label: string;
  value: number;
}

interface BarChartCardProps {
  title: string;
  data: BarDatum[];
  color?: keyof typeof CHART_COLORS;
  height?: number;
  valueFormatter?: (v: number) => string;
}

export function BarChartCard({
  title,
  data,
  color = "primary",
  height = 260,
  valueFormatter = (v) => String(v),
}: BarChartCardProps) {
  const chartData = {
    labels: data.map((d) => d.label),
    datasets: [
      {
        data: data.map((d) => d.value),
        backgroundColor: CHART_COLORS[color],
        borderRadius: 6,
        maxBarThickness: 48,
      },
    ],
  };

  const options: ChartOptions<"bar"> = {
    ...baseChartOptions,
    plugins: {
      ...baseChartOptions.plugins,
      tooltip: {
        callbacks: { label: (ctx) => `${ctx.dataset.label ?? ""} ${valueFormatter(Number(ctx.raw))}` },
      },
    },
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div style={{ height }}>
          {data.length === 0 ? (
            <div className="flex h-full items-center justify-center text-sm text-muted-foreground">
              No data available
            </div>
          ) : (
            <Bar data={chartData} options={options} />
          )}
        </div>
      </CardContent>
    </Card>
  );
}
