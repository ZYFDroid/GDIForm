namespace GDIForm
{
    partial class GDIForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.DrawingBase = new System.Windows.Forms.Panel();
            this.RenderTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // DrawingBase
            // 
            this.DrawingBase.BackColor = System.Drawing.Color.Black;
            this.DrawingBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrawingBase.Location = new System.Drawing.Point(0, 0);
            this.DrawingBase.Name = "DrawingBase";
            this.DrawingBase.Size = new System.Drawing.Size(713, 527);
            this.DrawingBase.TabIndex = 0;
            this.DrawingBase.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawingBase_Paint);
            // 
            // RenderTimer
            // 
            this.RenderTimer.Enabled = true;
            this.RenderTimer.Interval = 1;
            this.RenderTimer.Tick += new System.EventHandler(this.RenderTimer_Tick);
            // 
            // GDIForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(713, 527);
            this.Controls.Add(this.DrawingBase);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "GDIForm";
            this.Text = "GDIWindow";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GDIForm_FormClosed);
            this.Load += new System.EventHandler(this.GDIForm_Load);
            this.SizeChanged += new System.EventHandler(this.GDIForm_SizeChanged);
            this.Move += new System.EventHandler(this.GDIForm_Move);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel DrawingBase;
        private System.Windows.Forms.Timer RenderTimer;
    }
}

