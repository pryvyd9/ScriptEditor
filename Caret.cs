using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Timers;

namespace ScriptEditor
{
    public class Caret : FrameworkElement
    {
        public LinkedListNode<char> Position { get; set; }

        public Point ContainerPosition;

        private TimeSpan AnimationDuration { get; } = TimeSpan.FromMilliseconds(60);

        private readonly Timer timer;

        private const double timerSpan = 800;

        public Brush Brush
        {
            get => (Brush)GetValue(BrushProperty);
            set
            {
                SetValue(BrushProperty, value);
                timer.Stop();
                timer.Start();
            }
        }

        public static readonly DependencyProperty BrushProperty;


        static Caret()
        {
            BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(Caret),
                new FrameworkPropertyMetadata(
                    Brushes.Black,
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.AffectsRender
                )
            );
        }




        public Caret()
        {
            timer = new Timer(timerSpan) { AutoReset = true };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            SnapsToDevicePixels = true;

        }

        public void SetColor(bool transparent)
        {
            if(transparent)
                Brush = Brushes.Transparent;
            else
                Brush = Brushes.Black;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (Brush == Brushes.Black)
                    Brush = Brushes.Transparent;
                else
                    Brush = Brushes.Black;
            });
        }

        public void MoveTo(Point destPoint)
        {
            TranslateTransform translateTransform = new TranslateTransform();
            RenderTransform = translateTransform;

            DoubleAnimation xAnim = new DoubleAnimation(ContainerPosition.X, destPoint.X, AnimationDuration);
            DoubleAnimation yAnim = new DoubleAnimation(ContainerPosition.Y, destPoint.Y, AnimationDuration);

            translateTransform.BeginAnimation(TranslateTransform.XProperty, xAnim);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, yAnim);

            ContainerPosition = destPoint;
        }




        protected override void OnRender(DrawingContext drawingContext)
        {
            
            base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Brush, null, new Rect(0, 0, Width, Height));
        }
    }
}
