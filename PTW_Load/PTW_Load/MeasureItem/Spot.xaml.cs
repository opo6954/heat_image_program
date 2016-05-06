using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PTW_Load.MeasureItem
{
    /// <summary>
    /// Interaction logic for Spot.xaml
    /// </summary>
    public partial class Spot : UserControl
    {
        private Point prePosition = new Point();
        private bool IsPress = false;

        private readonly int ImageWidth = 640;
        private readonly int ImageHeight = 512;

        public String ID
        {
            get
            {
                return label.Content.ToString();
            }
            set
            {
                label.Content = value;
            }
        }

        public int X
        {
            set;
            get;
        }

        public int Y
        {
            set;
            get;
        }

        public double RealWidth
        {
            set;
            get;
        }

        public double RealHeight
        {
            set;
            get;
        }

        public Spot()
        {
            InitializeComponent();

            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;

            
        }

        public bool MouseDownEvent(double x, double y)
        {
            if (Visibility != Visibility.Visible)
                return false;

            if (Margin.Left <= x && Margin.Left + RenderSize.Width >= x)
            {
                if (Margin.Top <= y && Margin.Top + RenderSize.Height >= y)
                {
                    IsPress = true;
                    prePosition.X = x;
                    prePosition.Y = y;

                    return true;
                }
            }

            return false;
        }

        public bool MouseMoveEvent(double x, double y)
        {
            if (Visibility != Visibility.Visible)
                return false;

            if (IsPress)
            {
                double moveX = x - prePosition.X;
                double moveY = y - prePosition.Y;

                Thickness margin = Margin;
                margin.Left += moveX;
                margin.Top += moveY;
                Margin = margin;

                X = (int)((margin.Left + 16) * ImageWidth / RealWidth);
                Y = (int)((margin.Top + 16) * ImageHeight / RealHeight);

                prePosition.X = x;
                prePosition.Y = y;

                return true;
            }
            else
            {
                return CheckMouseCursor(x, y);
            }
        }

        public void MouseUpEvent(double x, double y)
        {
            IsPress = false;
        }

        private bool CheckMouseCursor(double x, double y)
        {
            if (Margin.Left <= x && Margin.Left + RenderSize.Width >= x)
            {
                if (Margin.Top <= y && Margin.Top + RenderSize.Height >= y)
                {
                    Mouse.OverrideCursor = Cursors.SizeAll;
                    return true;
                }
            }

            return false;
        }

        public void Position()
        {
            Thickness margin = Margin;
            margin.Left = (X * RealWidth / ImageWidth) - 16;
            margin.Top = (Y * RealHeight / ImageHeight) - 16;
            Margin = margin;
        }
    }
}
