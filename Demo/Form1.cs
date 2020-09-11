using GDIForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo
{
    public partial class Form1 : GDIForm.GDIForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        StringFormat centerF = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        public override void onInit(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            ShowFPS = true;

            using (Graphics gt = Graphics.FromImage(secondPan)) {
                gt.SmoothingMode = SmoothingMode.AntiAlias;
                gt.Clear(Color.Transparent);
                RectangleF region = new RectangleF(440, 240, 64, 22);
                gt.ResetTransform();

                gt.TranslateTransform(250, 250);
                for (int i = 0; i < 60; i++)
                {
                    gt.TranslateTransform(-250, -250);
                    gt.DrawString(fitTo4(numToChinese2(i) + "秒"), Font, white, region, centerF);
                    gt.TranslateTransform(250, 250);
                    gt.RotateTransform(6);
                }
                gt.ResetTransform();
            }

            using (Graphics gt = Graphics.FromImage(minutePan))
            {
                gt.SmoothingMode = SmoothingMode.AntiAlias;
                gt.Clear(Color.Transparent);
                RectangleF region = new RectangleF(380, 240, 64, 22);
                gt.ResetTransform();

                gt.TranslateTransform(250, 250);
                for (int i = 0; i < 60; i++)
                {
                    gt.TranslateTransform(-250, -250);
                    gt.DrawString(fitTo4(numToChinese2(i) + "分"), Font, white, region, centerF);
                    gt.TranslateTransform(250, 250);
                    gt.RotateTransform(6);
                }
                gt.ResetTransform();
            }

            using (Graphics gt = Graphics.FromImage(hourPan))
            {
                gt.SmoothingMode = SmoothingMode.AntiAlias;
                gt.Clear(Color.Transparent);
                RectangleF region = new RectangleF(320, 240, 64, 22);
                gt.ResetTransform();

                gt.TranslateTransform(250, 250);
                for (int i = 0; i < 24; i++)
                {
                    gt.TranslateTransform(-250, -250);
                    gt.DrawString(fitTo4(numToChinese(i) + "点"), Font, white, region, centerF);
                    gt.TranslateTransform(250, 250);
                    gt.RotateTransform(15);
                }
                gt.ResetTransform();
            }

            using (Graphics gt = Graphics.FromImage(mask))
            {
                gt.SmoothingMode = SmoothingMode.AntiAlias;
                gt.CompositingMode = CompositingMode.SourceCopy;
                gt.Clear(Color.FromArgb(127,Color.Black));
                Rectangle region = new Rectangle(330, 250, 182, 20);
                gt.FillRectangle(Brushes.Transparent, region);
                gt.DrawRectangle(thinPen, region);
                
            }
        }

        String fitTo4(String instr) {
            if (instr.Length == 2) {
                return instr[0] + "    " + instr[1];
            }
            if (instr.Length == 3)
            {
                return instr[0] + " " + instr[1] + " " + instr[2];
            }

            return instr;
        
        }

        Brush white = Brushes.White;
        string[] literals = { "", "一","二","三","四","五","六","七","八","九"};
        public string numToChinese(int i) {
            if (i == 0) { return "零"; }
            if (i < 10) { return "" + literals[i]; }
            if (i < 20) { return "十" + literals[i - 10]; }
            return literals[i / 10] + "十" + literals[i % 10];
        }

        public string numToChinese2(int i)
        {
            if (i == 0) { return "零"; }
            if (i < 10) { return "零" + literals[i]; }
            if (i < 20) { return "十" + literals[i-10]; }
            return literals[i / 10] + "十" + literals[i % 10];
        }

        Pen thinPen = new Pen(Brushes.White, 2)
        {
            EndCap = LineCap.Round,
            StartCap = LineCap.Round
        };

        PointF center = new PointF(260,260);

        Bitmap secondPan = new Bitmap(500,500);
        Bitmap minutePan = new Bitmap(500,500);
        Bitmap hourPan = new Bitmap(500,500);
        Bitmap mask = new Bitmap(520, 520);
        public override void onDraw(Graphics g)
        {
            g.Clear(Color.Black);
            DateTime now = DateTime.Now;
            int hour = now.Hour;
            int minute = now.Minute;
            int second = now.Second;
            float ms = now.Millisecond / 1000f;

            float hourangel = hour * 15f;
            if (minute == 59 && second == 59) { hourangel = hourangel + 15f * ms; }

            float minangel = minute * 6f;
            if (second == 59) { minangel = minangel + 6 * ms; }

            float secangel = second * 6f + 6 * ms;

            DrawUtils.drawRotateImg(g, secondPan, -secangel, 260, 260);
            DrawUtils.drawRotateImg(g, minutePan, -minangel, 260, 260);
            DrawUtils.drawRotateImg(g, hourPan, -hourangel, 260, 260);
            DrawUtils.drawRotateImg(g, mask, 0, 260, 260);
        }

        public PointF degreeTransform(PointF center, float length, float angelDeg) {
            double angelRad = (angelDeg-90) / 180 * Math.PI;
            return new PointF((float)(center.X + Math.Cos(angelRad) * length), (float)(center.Y + Math.Sin(angelRad) * length));
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            SoundPlayer sp =new SoundPlayer(Properties.Resources.unamed);
            sp.PlayLooping();
            while (!backgroundWorker1.CancellationPending) {
                Thread.Sleep(100);
            }
            sp.Stop();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }
    }
}
