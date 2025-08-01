namespace LocationShareApp.Controls
{
    public class BatteryChartView : GraphicsView
    {
        public static readonly BindableProperty BatteryDataProperty =
            BindableProperty.Create(nameof(BatteryData), typeof(List<BatteryRecord>), typeof(BatteryChartView), null, propertyChanged: OnBatteryDataChanged);

        public List<BatteryRecord>? BatteryData
        {
            get => (List<BatteryRecord>?)GetValue(BatteryDataProperty);
            set => SetValue(BatteryDataProperty, value);
        }

        public BatteryChartView()
        {
            Drawable = new BatteryChartDrawable();
            HeightRequest = 200;
        }

        private static void OnBatteryDataChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BatteryChartView chartView && chartView.Drawable is BatteryChartDrawable drawable)
            {
                drawable.BatteryData = newValue as List<BatteryRecord>;
                chartView.Invalidate();
            }
        }
    }

    public class BatteryChartDrawable : IDrawable
    {
        public List<BatteryRecord>? BatteryData { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (BatteryData == null || !BatteryData.Any())
            {
                DrawEmptyState(canvas, dirtyRect);
                return;
            }

            DrawChart(canvas, dirtyRect);
        }

        private void DrawEmptyState(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontColor = Colors.Gray;
            canvas.FontSize = 16;
            canvas.DrawString("暂无电量数据", dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        private void DrawChart(ICanvas canvas, RectF dirtyRect)
        {
            var padding = 20f;
            var chartRect = new RectF(padding, padding, dirtyRect.Width - 2 * padding, dirtyRect.Height - 2 * padding);

            // 绘制背景网格
            DrawGrid(canvas, chartRect);

            // 绘制电量曲线
            DrawBatteryLine(canvas, chartRect);

            // 绘制充电时段
            DrawChargingPeriods(canvas, chartRect);

            // 绘制坐标轴标签
            DrawLabels(canvas, chartRect);
        }

        private void DrawGrid(ICanvas canvas, RectF chartRect)
        {
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 1;

            // 绘制水平网格线 (电量百分比)
            for (int i = 0; i <= 4; i++)
            {
                var y = chartRect.Top + (chartRect.Height / 4) * i;
                canvas.DrawLine(chartRect.Left, y, chartRect.Right, y);
            }

            // 绘制垂直网格线 (时间)
            for (int i = 0; i <= 6; i++)
            {
                var x = chartRect.Left + (chartRect.Width / 6) * i;
                canvas.DrawLine(x, chartRect.Top, x, chartRect.Bottom);
            }
        }

        private void DrawBatteryLine(ICanvas canvas, RectF chartRect)
        {
            if (BatteryData == null || BatteryData.Count < 2) return;

            var sortedData = BatteryData.OrderBy(b => b.Timestamp).ToList();
            var minTime = sortedData.First().Timestamp;
            var maxTime = sortedData.Last().Timestamp;
            var timeSpan = maxTime - minTime;

            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 3;

            var path = new PathF();
            bool isFirstPoint = true;

            foreach (var battery in sortedData)
            {
                var x = chartRect.Left + (float)((battery.Timestamp - minTime).TotalMilliseconds / timeSpan.TotalMilliseconds) * chartRect.Width;
                var y = chartRect.Bottom - (battery.BatteryLevel / 100f) * chartRect.Height;

                if (isFirstPoint)
                {
                    path.MoveTo(x, y);
                    isFirstPoint = false;
                }
                else
                {
                    path.LineTo(x, y);
                }
            }

            canvas.DrawPath(path);
        }

        private void DrawChargingPeriods(ICanvas canvas, RectF chartRect)
        {
            if (BatteryData == null) return;

            var sortedData = BatteryData.OrderBy(b => b.Timestamp).ToList();
            var minTime = sortedData.First().Timestamp;
            var maxTime = sortedData.Last().Timestamp;
            var timeSpan = maxTime - minTime;

            canvas.FillColor = Colors.Green.WithAlpha(0.3f);

            for (int i = 0; i < sortedData.Count - 1; i++)
            {
                if (sortedData[i].IsCharging)
                {
                    var startX = chartRect.Left + (float)((sortedData[i].Timestamp - minTime).TotalMilliseconds / timeSpan.TotalMilliseconds) * chartRect.Width;
                    var endX = chartRect.Left + (float)((sortedData[i + 1].Timestamp - minTime).TotalMilliseconds / timeSpan.TotalMilliseconds) * chartRect.Width;

                    canvas.FillRectangle(startX, chartRect.Top, endX - startX, chartRect.Height);
                }
            }
        }

        private void DrawLabels(ICanvas canvas, RectF chartRect)
        {
            canvas.FontColor = Colors.Gray;
            canvas.FontSize = 12;

            // Y轴标签 (电量百分比)
            for (int i = 0; i <= 4; i++)
            {
                var y = chartRect.Bottom - (chartRect.Height / 4) * i;
                var percentage = (i * 25).ToString() + "%";
                canvas.DrawString(percentage, chartRect.Left - 15, y,chartRect.Width,chartRect.Height, HorizontalAlignment.Right, VerticalAlignment.Center);
            }

            // X轴标签 (时间)
            if (BatteryData != null && BatteryData.Any())
            {
                var sortedData = BatteryData.OrderBy(b => b.Timestamp).ToList();
                var minTime = sortedData.First().Timestamp;
                var maxTime = sortedData.Last().Timestamp;

                for (int i = 0; i <= 6; i++)
                {
                    var x = chartRect.Left + (chartRect.Width / 6) * i;
                    var time = minTime.AddTicks((maxTime - minTime).Ticks / 6 * i);
                    var timeLabel = time.ToString("HH:mm");
                    canvas.DrawString(timeLabel, x, chartRect.Bottom + 15,chartRect.Width,chartRect.Height, HorizontalAlignment.Center, VerticalAlignment.Top);
                }
            }
        }
    }

    public class BatteryRecord
    {
        public int BatteryLevel { get; set; }
        public bool IsCharging { get; set; }
        public DateTime Timestamp { get; set; }
    }
}