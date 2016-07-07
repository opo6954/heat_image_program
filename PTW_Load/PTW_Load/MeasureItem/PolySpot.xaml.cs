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
    public partial class PolySpot : UserControl
    {
        private Point prePosition = new Point();
        
        private bool isPress = false;
        
        private int imageWidth;
        private int imageHeight;

        public int ptID;

        public bool isShown = true;

        

        public PolySpotAll polyLayer;

        List<Point> polyPoint = new List<Point>();

        public PolySpot(int x, int y, int _ptId, PolySpotAll _polyLayer, int width, int height)
        {
            InitializeComponent();

            imageWidth = width;
            imageHeight = height;

            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;


            ptID = _ptId;
            polyLayer = _polyLayer;
            X = x;
            Y = y;

        }

        public PolySpot()
        {
            InitializeComponent();

            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;

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
        public double realWidth
        {
            set;
            get;
        }
        public double realHeight
        {
            set;
            get;
        }




        
        //눌렀을땡
        public bool MouseDownEvent(double x, double y)
        {
            if (Visibility != Visibility.Visible)
                return false;

            if (Margin.Left <= x && Margin.Left + RenderSize.Width >= x)
            {
                if (Margin.Top <= y && Margin.Top + RenderSize.Height >= y)
                {
                    //여기서 윗단을 불러야되는데

                    isPress = true;
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

            if (isPress)
            {
                
                double moveX = x - prePosition.X;
                double moveY = y - prePosition.Y;

                

                Thickness margin = Margin;
                margin.Left += moveX;
                margin.Top += moveY;

                if (margin.Left > 0 && margin.Top > 0)
                {

                    Margin = margin;



                    X = (int)((margin.Left + 16) * imageWidth / realWidth);
                    Y = (int)((margin.Top + 16) * imageHeight / realHeight);


                    polyLayer.drawLabel();
                    polyLayer.drawLine((int)margin.Left + 7, (int)margin.Top+7, ptID);

                    prePosition.X = x;
                    prePosition.Y = y;

                    return true;
                }
                else
                {
                    return true;
                }
            }
            else
                return CheckMouseCursor(x, y);
        }

        public void MouseUpEvent(double x, double y)
        {
            isPress = false;
            polyLayer.updatePolySpotValue();
            

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
            margin.Left = (X * realWidth / imageWidth)-16;
            margin.Top = (Y * realHeight / imageHeight)-16;
            Margin = margin;

        }
    }


    


}
