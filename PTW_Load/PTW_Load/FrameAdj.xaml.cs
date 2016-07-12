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
using System.Windows.Shapes;

namespace PTW_Load
{
    /// <summary>
    /// FrameAdj.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FrameAdj : Window
    {
        int limitCount;
        public bool isCancel = false;
        public bool isOkay = false;
        public int minCnt;
        public int maxCnt;

        public FrameAdj( String _fileName, int _frameCount, int _width, int _height, int _maxCount)
        {
            InitializeComponent();
            limitCount = _maxCount;
            this.Dispatcher.Invoke(new OnChangeValue(ChangeValue),new Object[] {_fileName, _frameCount, _width, _height, _maxCount});
            //this.Dispatcher.Invoke(new OnChangeLabelBar(ChangeLabelBar), new Object[] { 4, Min, Max });
        }
        

        public FrameAdj()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            minCnt = int.Parse(minFrameCount.Text);
            maxCnt = int.Parse(maxFrameCount.Text);

            if (minCnt >= maxCnt)
                MessageBox.Show("최소 frame값이 최대 frame값보다 큽니다. 값을 다시 조정하세요");
            else if (maxCnt - minCnt >= limitCount)
                MessageBox.Show("최대 frame값은 한계 frame을 초과할 수 없습니다. 값을 다시 조정하세요");
            else
            {
//                (VisualTreeHelper.GetParent(this) as MainWindow).minFrameCount = minCnt;
//                (VisualTreeHelper.GetParent(this) as MainWindow).maxFrameCount = maxCnt;
                isOkay = true;

                this.Close();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            isCancel = true;
            this.Close();
        }
        delegate void OnChangeValue(String _fileName, int _frameCount, int _width, int _height, int _maxCount);
        private void ChangeValue(String _fileName, int _frameCount, int _width, int _height, int _maxCount)
        {
             FileName.Text = _fileName;
            frameCount.Text = _frameCount.ToString();
            frameWidth.Text = _width.ToString();
            frameHeight.Text = _height.ToString();
            LimitFrame.Content = "(Maximum: " + _maxCount.ToString() + ")";

            minFrameCount.Text = "0";
            if (_frameCount > _maxCount)
                maxFrameCount.Text = _maxCount.ToString();
            else
                maxFrameCount.Text = _frameCount.ToString();

            
        }
    }
}
