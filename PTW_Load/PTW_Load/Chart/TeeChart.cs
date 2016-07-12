using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Steema.TeeChart;

namespace PTW_Load.Chart
{
    public partial class TeeChart : UserControl
    {
        TeeChartPanel myParent;
        bool isPressed = false;

        public TChart Chart
        {
            get { return this.tChart; }
        }

        public TeeChart(TeeChartPanel _panel)
        {
            InitializeComponent();

            chartController.Visible = false;
            chartController.Chart = tChart;

            myParent = _panel;


        }

        private void tChart_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                foreach (ToolStripItem button in chartController.Items)
                {
                    if (button is ToolStripButton)
                    {
                        switch (button.Tag.ToString())
                        {
                            case "bNormal":
                                break;
                            case "bSeparator":
                                break;
                            case "bRotate":
                                break;
                            case "bMove":
                                break;
                            case "bZoom":
                                break;
                            case "bDepth":
                                break;
                            case "bView3D":
                                break;
                            case "bEdit":
                                button.PerformClick();
                                break;
                            case "bPrint":
                                break;
                            case "bCopy":
                                break;
                            case "bExport":
                                break;
                        }
                    }
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                isPressed = true;
                
            }
        }

        private void tChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPressed == true)
            {
                myParent.UpdateFrameCount();
            }
        }
        private void tChart_MouseUp(object sender, MouseEventArgs e)
        {
            isPressed = false;
        }

    }
}
