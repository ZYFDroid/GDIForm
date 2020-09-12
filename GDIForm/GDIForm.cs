using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GDIForm
{
    /// <summary>
    /// GDI window base class
    /// </summary>
    public partial class GDIForm : Form
    {
        Form GdiLayer = null;

        private Size _resolution = new Size(854, 480);
        /// <summary>
        /// Get or set the Render resolution
        /// the resolution should only be change in designer or constructor method
        /// </summary>
        public Size Resolution {
            get
            {
                this.Size = SizeFromClientSize(_resolution);
                return _resolution;
            }
            set {
                if (!canChangeResolution) {
                    throw new InvalidOperationException();
                }
                _resolution = value;
                this.Size = SizeFromClientSize(_resolution);
                if (GdiLayer == null) { return; }
                GdiLayer.Size = value;
            }
        }

        

        bool canChangeResolution = true;

        /// <summary>
        /// Create a gdi window
        /// </summary>
        public GDIForm()
        {
            InitializeComponent();
            this.Size = SizeFromClientSize(_resolution);
        }

        private void GDIForm_Move(object sender, EventArgs e)
        {
            if (null == GdiLayer) { return; }
            GdiLayer.Location = PointToScreen(DrawingBase.DisplayRectangle.Location);
            GdiLayer.Size = DrawingBase.DisplayRectangle.Size;
        }

        private void GDIForm_SizeChanged(object sender, EventArgs e)
        {
            if (null == GdiLayer) { return; }
            GdiLayer.Visible = (this.WindowState != FormWindowState.Minimized);
            RenderTimer.Enabled = (this.WindowState != FormWindowState.Minimized);
            GdiLayer.Location = PointToScreen(DrawingBase.DisplayRectangle.Location);
            GdiLayer.Size = DrawingBase.DisplayRectangle.Size;
        }

        /// <summary>
        /// Show or hide fps counter
        /// </summary>
        public bool ShowFPS {
            get;
            set;
        }
        private void GDIForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GdiLayer?.Close();
            GdiLayer?.Dispose();
        }

        GdiSystem gdi;

        private void GDIForm_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                beforeInit();
                GdiLayer = new Form()
                {
                    Location = PointToScreen(DrawingBase.DisplayRectangle.Location),
                    Size = _resolution,
                    FormBorderStyle = FormBorderStyle.None,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual,
                    BackColor = Color.Aqua
                };
            }
            if (!DesignMode)
            {
                
                GdiLayer.Visible = false;
                GdiLayer.Show(this);
                gdi = new GdiSystem(GdiLayer);
                canChangeResolution = false;
                gdi.Graphics.Clear(Color.Transparent);
                gdi.UpdateWindow();
                GdiLayer.Visible = (this.WindowState != FormWindowState.Minimized);
                RenderTimer.Enabled = (this.WindowState != FormWindowState.Minimized);
                GdiLayer.Location = PointToScreen(DrawingBase.DisplayRectangle.Location);
                GdiLayer.Size = DrawingBase.DisplayRectangle.Size;
                DrawingBase.BackColor = Color.FromArgb(255, 1, 0, 0);
                this.TransparencyKey = DrawingBase.BackColor;
                onInit(gdi.Graphics);
            }
            else {
                RenderTimer.Enabled = false;
            }
        }

        /// <summary>
        /// before GDI object is created
        /// you can set the resolution here
        /// </summary>
        public virtual void beforeInit()
        {

        }
        /// <summary>
        /// Initialization method
        /// will be call once before onDraw is called
        /// </summary>
        /// <param name="g"></param>
        public virtual void onInit(Graphics g) { 
            
        }
        /// <summary>
        /// Main drawing method
        /// Will be call 60 times per second
        /// </summary>
        /// <param name="g"></param>
        public virtual void onDraw(Graphics g) {
            g.Clear(Color.Black);
        }

        int frames = 0;
        int second = 0;
        int fps = 0;

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            if (gdi != null) {
                onDraw(gdi.Graphics);

                frames++;
                int sec = DateTime.Now.Second;
                if (second != sec) {
                    second = sec;
                    fps = frames;
                    frames = 0;
                }
                if (ShowFPS) {
                    String fpsstr = "FPS:" + fps;
                    SizeF sizef = gdi.Graphics.MeasureString(fpsstr, this.Font);
                    gdi.Graphics.FillRectangle(fpsBackground,0,0,sizef.Width,sizef.Height);
                    gdi.Graphics.DrawString(fpsstr, Font, fpsForeground, 0, 0);
                }
                gdi.UpdateWindow();
            }
        }

        Brush fpsBackground = new SolidBrush(Color.FromArgb(192, Color.Black));
        Brush fpsForeground = new SolidBrush(Color.Lime);

        private void DrawingBase_Paint(object sender, PaintEventArgs e)
        {

        }
    }
    /// <summary>
    /// Layered Window operates
    /// </summary>
    public class GdiSystem : IDisposable
    {
        Form thisWindow;

        /// <summary>
        /// Convert a form to LayeredWindow
        /// call this in Load event
        /// </summary>
        /// <param name="attachForm"></param>
        ///  <param name="operateable">indicates whether the window will process input events.</param>
        public GdiSystem(Form attachForm,bool operateable = false)
        {
            thisWindow = attachForm;
            if (attachForm.Handle != IntPtr.Zero)
            {
                Win32.SetWindowLong(attachForm.Handle, Win32.GWL_EXSTYLE, Win32.GetWindowLong(attachForm.Handle, Win32.GWL_EXSTYLE)|Win32.WS_EX_LAYERED);
                if (!operateable)
                {
                    Win32.SetWindowLong(attachForm.Handle, Win32.GWL_EXSTYLE, Win32.GetWindowLong(attachForm.Handle, Win32.GWL_EXSTYLE) | Win32.WS_EX_TRANSPARENT);
                }
            }
            else
            {
                throw new AccessViolationException("Window not initialized.");
            }
            oldBits = IntPtr.Zero;
            screenDC = Win32.GetDC(IntPtr.Zero);
            hBitmap = IntPtr.Zero;
            memDc = Win32.CreateCompatibleDC(screenDC);
            blendFunc.BlendOp = Win32.AC_SRC_OVER;
            blendFunc.SourceConstantAlpha = 255;
            blendFunc.AlphaFormat = Win32.AC_SRC_ALPHA;
            blendFunc.BlendFlags = 0;

            initBitmaps();

        }

        private void initBitmaps()
        {
            thisBitmap = new Bitmap(thisWindow.Width, thisWindow.Height);
            thisGraphics = Graphics.FromImage(thisBitmap);
            bitMapSize = new Win32.Size(thisBitmap.Width, thisBitmap.Height);

        }
        /// <summary>
        /// Get a Graphic object.
        /// </summary>
        public Graphics Graphics
        {
            get
            {
                return thisGraphics;
            }
        }
        /// <summary>
        /// Apply content after paint on the Graphic
        /// </summary>
        public void UpdateWindow()
        {
            SetBits(thisBitmap);
        }
        IntPtr oldBits;
        IntPtr screenDC;
        IntPtr hBitmap;
        IntPtr memDc;
        Win32.BLENDFUNCTION blendFunc = new Win32.BLENDFUNCTION();

        Win32.Point topLoc = new Win32.Point(0, 0);
        Win32.Size bitMapSize;
        Win32.Point srcLoc = new Win32.Point(0, 0);


        IntPtr graphicDC;
        private void SetBits(Bitmap bitmap)
        {
            if (!Bitmap.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat))
                throw new ApplicationException("The picture must be 32bit picture with alpha channel.");
            try
            {
                topLoc.x = thisWindow.Left;
                topLoc.y = thisWindow.Top;
                hBitmap = thisBitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = Win32.SelectObject(memDc, hBitmap);
                Win32.BitBlt(memDc, 0, 0, bitMapSize.cx, bitMapSize.cy, graphicDC, 0, 0, 0x00CC0020);
                Win32.UpdateLayeredWindow(thisWindow.Handle, screenDC, ref topLoc, ref bitMapSize, memDc, ref srcLoc, 0, ref blendFunc, Win32.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBits);
                    Win32.DeleteObject(hBitmap);
                }
            }
        }

        public void Dispose()
        {
            Win32.ReleaseDC(IntPtr.Zero, screenDC);
            Win32.DeleteDC(memDc);

        }

        private Bitmap thisBitmap;
        private Graphics thisGraphics;


        class Win32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Size
            {
                public Int32 cx;
                public Int32 cy;

                public Size(Int32 x, Int32 y)
                {
                    cx = x;
                    cy = y;
                }
            }

            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern int BitBlt(
                IntPtr hdcDest,     // handle to destination DC (device context)
                int nXDest,         // x-coord of destination upper-left corner
                int nYDest,         // y-coord of destination upper-left corner
                int nWidth,         // width of destination rectangle
                int nHeight,        // height of destination rectangle
                IntPtr hdcSrc,      // handle to source DC
                int nXSrc,          // x-coordinate of source upper-left corner
                int nYSrc,          // y-coordinate of source upper-left corner
                System.Int32 dwRop  // raster operation code
            );

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct BLENDFUNCTION
            {
                public byte BlendOp;
                public byte BlendFlags;
                public byte SourceConstantAlpha;
                public byte AlphaFormat;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Point
            {
                public Int32 x;
                public Int32 y;

                public Point(Int32 x, Int32 y)
                {
                    this.x = x;
                    this.y = y;
                }
            }

            public const byte AC_SRC_OVER = 0;
            public const Int32 ULW_ALPHA = 2;
            public const byte AC_SRC_ALPHA = 1;
            public const int GWL_EXSTYLE = -20;
            public const int WS_EX_TRANSPARENT = 0x20;
            public const int WS_EX_LAYERED = 0x80000;

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern int DeleteDC(IntPtr hDC);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern int DeleteObject(IntPtr hObj);

            [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr ExtCreateRegion(IntPtr lpXform, uint nCount, IntPtr rgnData);

            [DllImport("user32.dll", EntryPoint = "GetWindowLongA")]
            public static extern int GetWindowLong(IntPtr hwnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongA")]
            public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

        }
    }

    /// <summary>
    /// Some method to draw images
    /// </summary>
    public class DrawUtils
    {
        #region drawAlphaImage
        /// <summary>
        /// Draw a image on the specified graphic with specified alpha
        /// </summary>
        /// <param name="g"></param>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="alpha"></param>
        public static void drawAlphaImage(Graphics g, Image image, float x, float y, float w, float h, float alpha)
        {
            if (alpha >= 0.99)
            {
                g.DrawImage(image, x, y, w, h);
                return;
            }
            g.DrawImage(image, new Rectangle((int)x, (int)y, (int)w, (int)h), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, alphaImage(alpha));
        }
        private static ImageAttributes alphaAttrs = new ImageAttributes();
        private static ColorMatrix cmx = new ColorMatrix(new float[][]{
                new float[5]{ 1,0,0,0,0 },
                new float[5]{ 0,1,0,0,0 },
                new float[5]{ 0,0,1,0,0 },
                new float[5]{ 0,0,0,0.5f,0 },
                new float[5]{ 0,0,0,0,0 }
            });
        private static ImageAttributes alphaImage(float alpha)
        {
            cmx.Matrix33 = alpha;
            alphaAttrs.SetColorMatrix(cmx, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            return alphaAttrs;
        }
        #endregion

        #region drawRotateImg
        /// <summary>
        /// Draw image rotated from the center
        /// </summary>
        /// <param name="g"></param>
        /// <param name="img"></param>
        /// <param name="angle"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        public static void drawRotateImg(Graphics g, Image img, float angle, float centerX, float centerY)
        {
            drawRotateImg(g, img, angle, centerX, centerY, img.Width, img.Height);
        }
        /// <summary>
        /// Draw image rotated from the center, with specified size
        /// </summary>
        /// <param name="g"></param>
        /// <param name="img"></param>
        /// <param name="angle"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="imgW"></param>
        /// <param name="imgH"></param>
        public static void drawRotateImg(Graphics g, Image img, float angle, float centerX, float centerY, float imgW, float imgH)
        {
            float width = imgW;
            float height = imgH;
            Matrix mtrx = new Matrix();
            mtrx.RotateAt(angle, new PointF((width / 2), (height / 2)), MatrixOrder.Append);
            //得到旋转后的矩形
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(new RectangleF(0f, 0f, width, height));
            RectangleF rct = path.GetBounds(mtrx);
            Point Offset = new Point((int)(rct.Width - width) / 2, (int)(rct.Height - height) / 2);
            //构造图像显示区域：让图像的中心与窗口的中心点一致
            RectangleF rect = new RectangleF(-width / 2 + centerX, -height / 2 + centerY, (int)width, (int)height);
            PointF center = new PointF((int)(rect.X + rect.Width / 2), (int)(rect.Y + rect.Height / 2));
            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(angle);
            //恢复图像在水平和垂直方向的平移
            g.TranslateTransform(-center.X, -center.Y);
            g.DrawImage(img, rect);
            //重至绘图的所有变换
            g.ResetTransform();
        }

        #endregion
    }

    /// <summary>
    /// Use pointer to process Bitmap pixel
    /// </summary>
    public class FastBitmap
    {
        Bitmap source = null;
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;

        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public FastBitmap(Bitmap source)
        {
            this.source = source;
        }

        public void LockBits()
        {
            try
            {
                // Get width and height of bitmap
                Width = source.Width;
                Height = source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                             source.PixelFormat);

                //得到首地址
                unsafe
                {
                    Iptr = bitmapData.Scan0;
                    //二维图像循环

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UnlockBits()
        {
            try
            {
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Color GetPixel(int x, int y)
        {
            unsafe
            {
                byte* ptr = (byte*)Iptr;
                ptr = ptr + bitmapData.Stride * y;
                ptr += Depth * x / 8;
                Color c = Color.Empty;
                if (Depth == 32)
                {
                    int a = ptr[3];
                    int r = ptr[2];
                    int g = ptr[1];
                    int b = ptr[0];
                    c = Color.FromArgb(a, r, g, b);
                }
                else if (Depth == 24)
                {
                    int r = ptr[2];
                    int g = ptr[1];
                    int b = ptr[0];
                    c = Color.FromArgb(r, g, b);
                }
                else if (Depth == 8)
                {
                    int r = ptr[0];
                    c = Color.FromArgb(r, r, r);
                }
                return c;
            }
        }

        public void SetPixel(int x, int y, Color c)
        {
            unsafe
            {
                byte* ptr = (byte*)Iptr;
                ptr = ptr + bitmapData.Stride * y;
                ptr += Depth * x / 8;
                if (Depth == 32)
                {
                    ptr[3] = c.A;
                    ptr[2] = c.R;
                    ptr[1] = c.G;
                    ptr[0] = c.B;
                }
                else if (Depth == 24)
                {
                    ptr[2] = c.R;
                    ptr[1] = c.G;
                    ptr[0] = c.B;
                }
                else if (Depth == 8)
                {
                    ptr[2] = c.R;
                    ptr[1] = c.G;
                    ptr[0] = c.B;
                }
            }
        }
    }
    /**/
}
