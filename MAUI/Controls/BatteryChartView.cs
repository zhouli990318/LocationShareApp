using System.Collections.Specialized;
using System.Collections.ObjectModel;
using LocationShareApp.Services;
using LocationShareApp.Helpers;

namespace LocationShareApp.Controls
{
    public class BatteryChartView : GraphicsView
    {
        private INotifyCollectionChanged? _currentCollection;
        public static readonly BindableProperty BatteryDataProperty =
            BindableProperty.Create(nameof(BatteryData), typeof(IEnumerable<BatteryRecord>), typeof(BatteryChartView), null, propertyChanged: OnBatteryDataChanged);

        public IEnumerable<BatteryRecord>? BatteryData
        {
            get => (IEnumerable<BatteryRecord>?)GetValue(BatteryDataProperty);
            set => SetValue(BatteryDataProperty, value);
        }

        public BatteryChartView()
        {
            Drawable = new BatteryChartDrawable();
            HeightRequest = 200;
        }

        private static void OnBatteryDataChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BatteryChartView chartView)
            {
                // 取消旧集合的事件监听
                if (chartView._currentCollection != null)
                {
                    chartView._currentCollection.CollectionChanged -= chartView.OnCollectionChanged;
                }

                // 设置新数据并预处理
                if (chartView.Drawable is BatteryChartDrawable drawable)
                {
                    drawable.SetBatteryData(newValue as IEnumerable<BatteryRecord>);
                }

                // 监听新集合的变化
                if (newValue is INotifyCollectionChanged newCollection)
                {
                    chartView._currentCollection = newCollection;
                    newCollection.CollectionChanged += chartView.OnCollectionChanged;
                }

                chartView.Invalidate();
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Drawable is BatteryChartDrawable drawable)
            {
                drawable.InvalidateCache();
            }
            Invalidate();
        }
    }

    // 时间段数据结构，用于缓存计算结果
    public struct TimeSlotData
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public float AverageBattery { get; set; }
        public bool IsCharging { get; set; }
        public bool HasData { get; set; }
    }

    public class BatteryChartDrawable : IDrawable
    {
        private IEnumerable<BatteryRecord>? _batteryData;
        private List<BatteryRecord>? _sortedData;
        private List<TimeSlotData>? _timeSlotData;
        private BatteryRecord? _latestData;
        private bool _cacheValid = false;
        
        // 缓存的布局参数
        private struct LayoutParams
        {
            public RectF ChartRect { get; set; }
            public float SlotWidth { get; set; }
            public float GapWidth { get; set; }
            public float BarWidth { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public DateTime DisplayEndTime { get; set; }
            public int TimeSlots { get; set; }
        }
        
        private LayoutParams? _cachedLayout;

        // 预定义的颜色，避免重复创建
        private static readonly Color GreenColor = Color.FromArgb("#52C41A");
        private static readonly Color RedColor = Color.FromArgb("#FF4D4F");
        private static readonly Color OrangeColor = Color.FromArgb("#FFA940");
        private static readonly Color GridColor = Color.FromArgb("#F0F0F0");
        private static readonly Color DashColor = Color.FromArgb("#E0E0E0");
        private static readonly Color NoDataColor = Color.FromArgb("#FDE6CE");
        private static readonly Color TextColor = Color.FromArgb("#999999");
        private static readonly Color LegendColor = Color.FromArgb("#666666");

        public void SetBatteryData(IEnumerable<BatteryRecord>? data)
        {
            _batteryData = data;
            InvalidateCache();
        }

        public void InvalidateCache()
        {
            _cacheValid = false;
            _sortedData = null;
            _timeSlotData = null;
            _latestData = null;
            _cachedLayout = null;
        }

        private void EnsureCacheValid()
        {
            if (_cacheValid || _batteryData == null) return;

            // 预处理和缓存数据
            _sortedData = _batteryData.OrderBy(b => b.Timestamp.ToLocalTime()).ToList();
            _latestData = _sortedData.LastOrDefault();
            
            // 预计算时间段数据
            PrecomputeTimeSlotData();
            
            _cacheValid = true;
        }

        private void PrecomputeTimeSlotData()
        {
            if (_sortedData == null) return;

            var now = DateTime.Now;
            var endTimeRounded = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var startTimeRounded = endTimeRounded.AddHours(-24);
            var displayEndTime = now.Minute >= 30 ? endTimeRounded.AddMinutes(30) : endTimeRounded;
            var totalMinutes = (displayEndTime - startTimeRounded).TotalMinutes;
            var timeSlots = (int)(totalMinutes / 30);

            _timeSlotData = new List<TimeSlotData>(timeSlots);

            for (int i = 0; i < timeSlots; i++)
            {
                var slotTime = startTimeRounded.AddMinutes(i * 30);
                var slotEndTime = slotTime.AddMinutes(30);
                
                var slotData = _sortedData.Where(d => 
                    d.Timestamp.ToLocalTime() >= slotTime && 
                    d.Timestamp.ToLocalTime() < slotEndTime).ToList();
                
                if (slotData.Count > 0)
                {
                    _timeSlotData.Add(new TimeSlotData
                    {
                        StartTime = slotTime,
                        EndTime = slotEndTime,
                        AverageBattery = (float)slotData.Average(d => d.BatteryLevel),
                        IsCharging = slotData.Any(d => d.IsCharging),
                        HasData = true
                    });
                }
                else
                {
                    _timeSlotData.Add(new TimeSlotData
                    {
                        StartTime = slotTime,
                        EndTime = slotEndTime,
                        HasData = false
                    });
                }
            }
        }

        private LayoutParams CalculateLayout(RectF dirtyRect)
        {
            if (_cachedLayout.HasValue) return _cachedLayout.Value;

            var leftPadding = 5f;
            var rightPadding = 35f;
            var topPadding = 10f;
            var bottomPadding = 35f;
            
            var chartRect = new RectF(leftPadding, topPadding, 
                dirtyRect.Width - leftPadding - rightPadding, 
                dirtyRect.Height - topPadding - bottomPadding);

            var now = DateTime.Now;
            var endTimeRounded = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var startTimeRounded = endTimeRounded.AddHours(-24);
            var displayEndTime = now.Minute >= 30 ? endTimeRounded.AddMinutes(30) : endTimeRounded;
            var totalMinutes = (displayEndTime - startTimeRounded).TotalMinutes;
            var timeSlots = (int)(totalMinutes / 30);

            var slotWidth = chartRect.Width / 48f;
            var gapWidth = slotWidth * 0.1f;
            var barWidth = slotWidth * 0.8f;

            var layout = new LayoutParams
            {
                ChartRect = chartRect,
                SlotWidth = slotWidth,
                GapWidth = gapWidth,
                BarWidth = barWidth,
                StartTime = startTimeRounded,
                EndTime = endTimeRounded,
                DisplayEndTime = displayEndTime,
                TimeSlots = timeSlots
            };

            _cachedLayout = layout;
            return layout;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            EnsureCacheValid();

            if (_batteryData == null || _sortedData?.Count == 0)
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
            var layout = CalculateLayout(dirtyRect);

            // 绘制白色背景
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(dirtyRect);

            // 绘制背景网格
            DrawGrid(canvas, layout);

            // 绘制电量柱状图
            DrawBatteryBars(canvas, layout);

            // 绘制最新电量柱状图
            DrawLatestBatteryBar(canvas, layout, dirtyRect);

            // 绘制坐标轴标签
            DrawLabels(canvas, layout, dirtyRect);

            // 绘制图例
            DrawLegend(canvas, dirtyRect);
        }

        private void DrawGrid(ICanvas canvas, LayoutParams layout)
        {
            // 绘制水平网格线
            canvas.StrokeColor = GridColor;
            canvas.StrokeSize = 0.5f;
            
            for (int i = 1; i <= 4; i++)
            {
                var y = layout.ChartRect.Bottom - (layout.ChartRect.Height / 5) * i;
                canvas.DrawLine(layout.ChartRect.Left, y, layout.ChartRect.Right, y);
            }

            // 绘制竖虚线
            canvas.StrokeColor = DashColor;
            canvas.StrokeSize = 0.5f;
            
            for (int hour = 1; hour < 24; hour++)
            {
                var x = layout.ChartRect.Left + (layout.ChartRect.Width / 24f) * hour;
                DrawDashedLine(canvas, x, layout.ChartRect.Top, x, layout.ChartRect.Bottom);
            }
        }
        
        private void DrawBatteryBars(ICanvas canvas, LayoutParams layout)
        {
            if (_timeSlotData == null) return;

            for (int i = 0; i < _timeSlotData.Count; i++)
            {
                var slotData = _timeSlotData[i];
                
                // 计算柱子位置
                var hoursFromStart = (slotData.StartTime - layout.StartTime).TotalHours;
                var slotX = layout.ChartRect.Left + (layout.ChartRect.Width / 24f) * (float)hoursFromStart;
                var x = slotX + layout.GapWidth / 2;
                
                if (slotData.HasData)
                {
                    var barHeight = (slotData.AverageBattery / 100f) * layout.ChartRect.Height;
                    var y = layout.ChartRect.Bottom - barHeight;

                    // 选择颜色
                    var barColor = slotData.AverageBattery >= 20 ? GreenColor : RedColor;
                    canvas.FillColor = barColor;
                    
                    // 绘制柱状图
                    var cornerRadius = Math.Min(layout.BarWidth / 4, 3f);
                    var barRect = new RectF(x, y, layout.BarWidth, barHeight);
                    canvas.FillRoundedRectangle(barRect, cornerRadius);

                    // 充电图标
                    if (slotData.IsCharging)
                    {
                        DrawChargingIcon(canvas, x + layout.BarWidth / 2, y - 8);
                    }
                }
                else
                {
                    // 无数据斜纹
                    var noDataRect = new RectF(x, layout.ChartRect.Top, layout.BarWidth, layout.ChartRect.Height);
                    DrawNoDataStripes(canvas, noDataRect);
                }
            }
        }

        private void DrawChargingIcon(ICanvas canvas, float x, float y)
        {
            canvas.FillColor = OrangeColor;
            
            var lightningPath = new PathF();
            lightningPath.MoveTo(x - 4, y);
            lightningPath.LineTo(x + 2, y - 6);
            lightningPath.LineTo(x - 1, y - 6);
            lightningPath.LineTo(x + 4, y - 12);
            lightningPath.LineTo(x - 2, y - 6);
            lightningPath.LineTo(x + 1, y - 6);
            lightningPath.Close();
            
            canvas.FillPath(lightningPath);
        }

        private void DrawNoDataStripes(ICanvas canvas, RectF rect)
        {
            canvas.StrokeColor = NoDataColor;
            canvas.StrokeSize = 1;
            
            for (float x = rect.Left; x < rect.Right + rect.Height; x += 4)
            {
                var startX = x;
                var startY = rect.Bottom;
                var endX = x - rect.Height;
                var endY = rect.Top;
                
                if (startX > rect.Right)
                {
                    var offset = startX - rect.Right;
                    startX = rect.Right;
                    startY = rect.Bottom - offset;
                }
                if (endX < rect.Left)
                {
                    var offset = rect.Left - endX;
                    endX = rect.Left;
                    endY = rect.Top + offset;
                }
                
                if (startY >= rect.Top && endY <= rect.Bottom)
                {
                    canvas.DrawLine(startX, startY, endX, endY);
                }
            }
        }

        private void DrawLatestBatteryBar(ICanvas canvas, LayoutParams layout, RectF dirtyRect)
        {
            if (_latestData == null) return;

            var x = layout.ChartRect.Right - layout.BarWidth - layout.GapWidth;
            var barHeight = (_latestData.BatteryLevel / 100f) * layout.ChartRect.Height;
            var y = layout.ChartRect.Bottom - barHeight;

            var barColor = _latestData.BatteryLevel >= 20 ? GreenColor : RedColor;
            canvas.FillColor = barColor;
            
            var cornerRadius = Math.Min(layout.BarWidth / 4, 3f);
            var barRect = new RectF(x, y, layout.BarWidth, barHeight);
            canvas.FillRoundedRectangle(barRect, cornerRadius);

            if (_latestData.IsCharging)
            {
                DrawChargingIcon(canvas, x + layout.BarWidth / 2, y - 8);
            }
        }

        private void DrawDashedLine(ICanvas canvas, float x1, float y1, float x2, float y2)
        {
            var dashLength = 3f;
            var gapLength = 2f;
            var totalLength = Math.Abs(y2 - y1);
            var dashCount = (int)(totalLength / (dashLength + gapLength));
            
            for (int i = 0; i < dashCount; i++)
            {
                var startY = y1 + i * (dashLength + gapLength);
                var endY = Math.Min(startY + dashLength, y2);
                canvas.DrawLine(x1, startY, x2, endY);
            }
        }

        private void DrawLabels(ICanvas canvas, LayoutParams layout, RectF dirtyRect)
        {
            canvas.FontColor = TextColor;
            canvas.FontSize = 10;

            // Y轴标签
            for (int i = 0; i <= 5; i++)
            {
                var y = layout.ChartRect.Bottom - (layout.ChartRect.Height / 5) * i;
                var percentage = (i * 20).ToString() + "%";
                var labelRect = new RectF(layout.ChartRect.Right + 2f, y - 6, 30, 12);
                canvas.DrawString(percentage, labelRect, HorizontalAlignment.Left, VerticalAlignment.Center);
            }

            // X轴标签
            for (int i = 0; i <= 12; i++)
            {
                var timePoint = layout.StartTime.AddHours(i * 2);
                var x = layout.ChartRect.Left + (layout.ChartRect.Width / 24f) * (i * 2);
                var timeLabel = timePoint.ToString("HH");
                var labelRect = new RectF(x - 8, layout.ChartRect.Bottom + 2, 16, 12);
                canvas.DrawString(timeLabel, labelRect, HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }

        private void DrawLegend(ICanvas canvas, RectF dirtyRect)
        {
            var legendY = dirtyRect.Height - 12;
            
            // 充电图例
            canvas.FillColor = OrangeColor;
            canvas.FillCircle(8, legendY, 2);
            canvas.FontColor = LegendColor;
            canvas.FontSize = 9;
            var chargingRect = new RectF(14, legendY - 5, 60, 10);
            canvas.DrawString("⚡ 充电中", chargingRect, HorizontalAlignment.Left, VerticalAlignment.Center);
            
            // 无数据图例
            canvas.StrokeColor = NoDataColor;
            canvas.StrokeSize = 1;
            canvas.DrawLine(80, legendY - 2, 90, legendY + 2);
            canvas.DrawLine(82, legendY - 2, 92, legendY + 2);
            var noDataRect = new RectF(95, legendY - 5, 60, 10);
            canvas.DrawString("无数据", noDataRect, HorizontalAlignment.Left, VerticalAlignment.Center);
        }
    }
}