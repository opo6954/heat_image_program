namespace PTW_Load.Chart
{
    partial class TeeChart
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tChart = new Steema.TeeChart.TChart();
            this.chartController = new Steema.TeeChart.ChartController();
            this.SuspendLayout();
            // 
            // tChart
            // 
            this.tChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tChart.Location = new System.Drawing.Point(0, 0);
            this.tChart.Margin = new System.Windows.Forms.Padding(0);
            this.tChart.Name = "tChart";
            this.tChart.Size = new System.Drawing.Size(150, 150);
            this.tChart.TabIndex = 0;
            this.tChart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tChart_MouseDown);
            this.tChart.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tChart_MouseMove);
            this.tChart.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tChart_MouseUp);
            
            // 
            // chartController
            // 
            this.chartController.ButtonSize = Steema.TeeChart.ControllerButtonSize.x16;
            this.chartController.LabelValues = true;
            this.chartController.Location = new System.Drawing.Point(0, 0);
            this.chartController.Name = "chartController";
            this.chartController.Size = new System.Drawing.Size(150, 25);
            this.chartController.TabIndex = 1;
            this.chartController.Text = "chartController1";
            // 
            // TeeChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chartController);
            this.Controls.Add(this.tChart);
            this.Name = "TeeChart";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Steema.TeeChart.TChart tChart;
        private Steema.TeeChart.ChartController chartController;
    }
}
