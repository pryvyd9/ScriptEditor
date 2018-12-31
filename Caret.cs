using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ScriptEditor
{
    public class Caret : FrameworkElement
    {
        public LinkedListNode<char> Position { get; set; }

        public Point ContainerPosition;



        private TimeSpan AnimationDuration { get; } = TimeSpan.FromMilliseconds(60);





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

            drawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, Width, Height));
        }
    }
}
