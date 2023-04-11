using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfFireworks.Utilities;

namespace WpfFireworks.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfFireworks.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfFireworks.Controls;assembly=WpfFireworks.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:GameElement/>
    ///
    /// </summary>
    public class FireworkElement : Control
    {
        static FireworkElement()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FireworkElement), new FrameworkPropertyMetadata(typeof(FireworkElement)));
        }

        public FireworkElement()
        {
            GameTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(1),
                DispatcherPriority.Render,
                MainLoopAction,
                Dispatcher);
        }

        private readonly Stopwatch watch = 
            new Stopwatch();

        private double lastTime;
        private List<FireworkPoint> fireworkPoints = 
            new List<FireworkPoint>();

        private void MainLoopAction(object? sender, EventArgs e)
        {
            watch.Start();
            double nowTime = watch.Elapsed.TotalSeconds;
            double deltaTime = nowTime - lastTime;

            if (deltaTime > 1)
                deltaTime = 0;

            List<FireworkPoint> pointsToRemove =
                new List<FireworkPoint>();

            foreach (var fireworkPoint in fireworkPoints)
            {
                if (fireworkPoint.LifeTime < 0)
                    pointsToRemove.Add(fireworkPoint);

                // 运动
                fireworkPoint.Position +=
                    fireworkPoint.Velocity * deltaTime;

                // 减速
                fireworkPoint.Velocity -=
                    fireworkPoint.Velocity * Drag * deltaTime;

                // 重力
                fireworkPoint.Velocity +=
                    new Vector(0, Gravity) * deltaTime;

                // 生命时长
                fireworkPoint.LifeTime -= deltaTime;
            }

            foreach (var pointToRemove in pointsToRemove)
                fireworkPoints.Remove(pointToRemove);

            InvalidateVisual();

            lastTime = nowTime;
        }

        public DispatcherTimer GameTimer { get; }



        public double Gravity
        {
            get { return (double)GetValue(GravityProperty); }
            set { SetValue(GravityProperty, value); }
        }

        public double Drag
        {
            get { return (double)GetValue(DragProperty); }
            set { SetValue(DragProperty, value); }
        }

        public bool GameEnabled
        {
            get { return (bool)GetValue(GameEnabledProperty); }
            set { SetValue(GameEnabledProperty, value); }
        }




        // Using a DependencyProperty as the backing store for Gravity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GravityProperty =
            DependencyProperty.Register(nameof(Gravity), typeof(double), typeof(FireworkElement), new PropertyMetadata(100.0));

        // Using a DependencyProperty as the backing store for Drag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragProperty =
            DependencyProperty.Register(nameof(Drag), typeof(double), typeof(FireworkElement), new PropertyMetadata(.8));

        // Using a DependencyProperty as the backing store for GameEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GameEnabledProperty =
            DependencyProperty.Register(nameof(GameEnabled), typeof(bool), typeof(FireworkElement), new PropertyMetadata(false, GameEnabledChanged));

        private static void GameEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FireworkElement element)
                return;
            if (e.NewValue is not bool value)
                return;

            if (value)
                element.GameTimer.Start();
            else
                element.GameTimer.Stop();
        }

        public void NewFirework(Point point, double radius)
        {
            Color color = ColorBand.GetColor(Random.Shared.NextDouble());

            for (int i = 0; i < 300; i++)
            {
                double angle =
                    Math.PI * 2 * Random.Shared.NextDouble();

                double distance =
                    radius * Random.Shared.NextDouble();

                double x = Math.Cos(angle) * distance;
                double y = Math.Sin(angle) * distance;

                fireworkPoints.Add(new FireworkPoint()
                {
                    Color = color,
                    Position = point,
                    Velocity = new Vector(x, y) * 3
                });
            }
        }

        private static readonly Dictionary<Color, SolidColorBrush> solidColorBrushes =
            new Dictionary<Color, SolidColorBrush>();
        private static readonly Dictionary<Color, Pen> solidColorPens =
            new Dictionary<Color, Pen>();

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            foreach (var fireworkPoint in fireworkPoints)
            {
                Color color = fireworkPoint.Color;

                if (fireworkPoint.LifeTime < 0)
                    continue;
                if (fireworkPoint.LifeTime < 1)
                    color.A = (byte)(fireworkPoint.LifeTime * 255);

                if (!solidColorBrushes.TryGetValue(color, out SolidColorBrush? brush))
                    brush = solidColorBrushes[color] = new SolidColorBrush(color);
                if (!solidColorPens.TryGetValue(color, out Pen? pen))
                    pen = solidColorPens[color] = new Pen(brush, 1);

                double radius =
                    fireworkPoint.Velocity.Length * .02;

                drawingContext.DrawEllipse(brush, null, fireworkPoint.Position, 2, 2);
                drawingContext.DrawLine(pen, fireworkPoint.Position, fireworkPoint.Position - fireworkPoint.Velocity * 0.1);
            }
        }

        public record class FireworkPoint
        {
            public double LifeTime { get; set; } = 1;
            public Color Color { get; set; }

            public Point Position { get; set; }
            public Vector Velocity { get; set; }
        }
    }
}
