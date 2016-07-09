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
        

        private int ImageWidth;
        private int ImageHeight;

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
        public void setSpotValue(double spotValue)
        {
            value.Text = spotValue.ToString();           
        }

        public void setImageSize(int _imageWidth, int _imageHeight)
        {
            ImageWidth = _imageWidth;
            ImageHeight = _imageHeight;
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

        public Spot(int _imageWidth, int _imageHeight, String unitName)
        {
            InitializeComponent();

            ImageWidth = _imageWidth;
            ImageHeight = _imageHeight;

            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;

            unit.Content = unitName;

            FormattedText t = new FormattedText(unit.Content.ToString(), System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface(unit.FontFamily, unit.FontStyle, unit.FontWeight, unit.FontStretch), unit.FontSize, Brushes.Black);

            unit.Width = t.Width + 5;
            unit.Height = t.Height;



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

        public void setStressValue(double val)
        {
            value.Text = val.ToString();
        }

        public void setStressValue()
        {
            
            MainWindow myWin = (MainWindow)System.Windows.Application.Current.MainWindow;

            if (myWin.StressImage != null)
                value.Text = ((myWin.StressImage[Y * ImageHeight + X])).ToString();
        }

        public void MouseUpEvent(double x, double y)
        {
            IsPress = false;

            setStressValue();

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
