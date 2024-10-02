using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using static Function.MathFunction;
using Function;


namespace Function
{
    public partial class Form1 : Form
    {
        private BufferedGraphics Buf;
        private Bitmap FunctionBitmap;
        private Graphics Graphics;
        private int width, height;
        private PointLong zero=new PointLong(250,250);
        private double unitlength = 40;
        private float unitlengthf = 40;
        private PointLong lastzero = new PointLong(250, 250);

        private List<Function> Funcs=new List<Function>();


        private bool loaded = false;


        private static Color white = Color.FromArgb(255, 255, 255);
        private static Color black = Color.FromArgb(0, 0, 0);
        private static Color whiteA = Color.FromArgb(128, 255, 255, 255);
        private static Color blackA = Color.FromArgb(128, 0, 0, 0);
        private static Pen wPen, bPen;
        private static Brush wBrush, bBrush;
        private static Font font = new Font("Consolas", 13);

        private double[] varlist = new double[26];

        private System.Windows.Forms.Timer DrawerTimer=new System.Windows.Forms.Timer();

        private object syncroot=new object();


        public Form1()
        {
            InitializeComponent();
            canvas = new Panel();
            canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;
            canvas.BackColor = Color.White;
            canvas.Location = new Point(0, 0);
            canvas.Size = ClientSize;
            canvas.Paint += new PaintEventHandler(canvas_Paint);
            canvas.SetControlStyle(ControlStyles.UserPaint,true);
            Controls.Add(canvas);
            canvas.SendToBack();
            panel3.BringToFront();
            变量输入.Location = new Point(104, 0);

            函数显示.SetControlStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            函数输入.SetControlStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            panel3.BackColor = Color.FromArgb(0, 0, 0, 0);

            Function.Init(this);



            wBrush = new SolidBrush(white);
            bBrush = new SolidBrush(black);
            wPen = new Pen(white);
            bPen = new Pen(black);
            InputLanguage.CurrentInputLanguage = InputLanguage.DefaultInputLanguage;
            CheckForIllegalCrossThreadCalls = false;
            DrawerTimer.Tick += DrawerTimer_Elapsed;
            DrawerTimer.Interval = 500;
            DrawerTimer.Enabled = false;
            /*Funcs.Add(new Function("x/sin(x)+y/sin(y)=x*y/sin(x*y)"));
            Funcs.Add(new Function("x/sin(x)+y/sin(y)=-x*y/sin(x*y)"));
            Funcs.Add(new Function("x/sin(x)-y/sin(y)=x*y/sin(x*y)"));
            Funcs.Add(new Function("x/sin(x)-y/sin(y)=-x*y/sin(x*y)"));*/


            Action canvasmouseaction = () =>
            {
                int mousedown_x = 0, mousedown_y = 0;
                long zero_x = 0, zero_y = 0;
                bool mousedownl = false;
                bool mousedownr = false;

                canvas.Cursor = Cursors.Cross;
                canvas.MouseDown += new MouseEventHandler((object sender, MouseEventArgs e) =>
                {
                    width=canvas.Width;
                    height=canvas.Height;
                    if (e.Button == MouseButtons.Left)
                    {
                        mousedownl = true;
                        mousedown_x = e.X;
                        mousedown_y = e.Y;
                        zero_x = zero.X;
                        zero_y = zero.Y;
                        //DrawerTimer.Start();
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        mousedownr = true;
                    }
                    lastzero = zero;
                });
                canvas.MouseMove += new MouseEventHandler((object sender, MouseEventArgs e) =>
                {
                    if (mousedownl)
                    {//移动零点
                        zero.X = (zero_x + e.X - mousedown_x);
                        zero.Y = (zero_y + e.Y - mousedown_y);
                        Graphics gb = Buf.Graphics;
                        gb.Clear(Color.White);  
                        DrawAxis(gb);
                        Bitmap bitmap = new Bitmap(width,height);
                        Graphics fg = Graphics.FromImage(bitmap);
                        fg.Clear(Color.White);
                        bitmap.MakeTransparent(Color.White);
                        fg = Graphics.FromImage(bitmap);
                        fg.DrawImage(FunctionBitmap,(int)(zero.X-lastzero.X), (int)(zero.Y - lastzero.Y));
                        foreach (var function in Funcs)
                        {
                            if (function.Visible)
                            {
                                if (zero.X < lastzero.X)
                                {
                                    DrawFunction(new Rectangle((int)(width - lastzero.X + zero.X), 0, (int)(lastzero.X - zero.X), height), function.LeftFunction, function.RightFunction, function.Operator, fg,new SolidBrush(function.Color));
                                    if (zero.Y < lastzero.Y)
                                    {
                                        DrawFunction(new Rectangle(0, (int)(height - lastzero.Y + zero.Y), (int)(width - lastzero.X + zero.X), (int)(lastzero.Y - zero.Y)), function.LeftFunction, function.RightFunction, function.Operator, fg, new SolidBrush(function.Color));

                                    }
                                    else if (zero.Y > lastzero.Y)
                                    {
                                        DrawFunction(new Rectangle(0, 0, (int)(width - lastzero.X + zero.X), (int)(zero.Y - lastzero.Y)), function.LeftFunction, function.RightFunction, function.Operator, fg, new SolidBrush(function.Color));
                                    }
                                }
                                else if (zero.X > lastzero.X)
                                {
                                    DrawFunction(new Rectangle(0, 0, (int)(zero.X - lastzero.X), height), function.LeftFunction, function.RightFunction, function.Operator, fg, new SolidBrush(function.Color));
                                    if (zero.Y < lastzero.Y)
                                    {
                                        DrawFunction(new Rectangle((int)(zero.X - lastzero.X), (int)(height - lastzero.Y + zero.Y), width - (int)(zero.X - lastzero.X), (int)(lastzero.Y - zero.Y)), function.LeftFunction, function.RightFunction, function.Operator, fg, new SolidBrush(function.Color));

                                    }
                                    else if (zero.Y > lastzero.Y)
                                    {
                                        DrawFunction(new Rectangle((int)(zero.X - lastzero.X), 0, width - (int)(zero.X - lastzero.X), (int)(zero.Y - lastzero.Y)), function.LeftFunction, function.RightFunction, function.Operator, fg, new SolidBrush(function.Color));
                                    }
                                }
                                else
                                {
                                    if (zero.Y < lastzero.Y)
                                    {
                                        DrawFunction(new Rectangle(0, (int)(height - lastzero.Y + zero.Y), width, (int)(lastzero.Y - zero.Y)), function.LeftFunction, function.RightFunction, function.Operator, fg, new SolidBrush(function.Color));

                                    }
                                    else if (zero.Y > lastzero.Y)
                                    {
                                        DrawFunction(new Rectangle(0, 0, width, (int)(zero.Y - lastzero.Y)), function.LeftFunction, function.RightFunction, function.Operator, fg, new SolidBrush(function.Color));
                                    }
                                }
                            }
                        }
                        gb.DrawImage(bitmap,0,0);
                        Buf.Render();
                        GC.Collect();
                        if (Math.Abs(zero.X - lastzero.X) > 20 || Math.Abs(zero.Y - lastzero.Y) > 20)
                        {
                            FunctionBitmap = bitmap;
                            lastzero = zero;
                        }
                    }
                    if (mousedownr)
                    {
                        Point p = e.Location;
                        string s = "(" + realtomathX(p.X) + "," + realtomathY(p.Y) + ")";
                        BufferedGraphics bg = BufferedGraphicsManager.Current.Allocate(Graphics, canvas.ClientRectangle);
                        Buf.Render(bg.Graphics);
                        bg.Graphics.FillRectangle(new SolidBrush(whiteA), e.Location.X, e.Location.Y, font.Height / 2 * s.Length, font.Height);
                        bg.Graphics.DrawString(s, font, bBrush, e.Location);
                        bg.Render();
                    }

                });

                canvas.MouseUp += new MouseEventHandler((object sender, MouseEventArgs e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        mousedownl = false;
                        DrawerTimer.Stop();
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        mousedownr = false;
                    }
                    Draw();
                    lastzero = zero;
                });
                canvas.MouseLeave += new EventHandler((object sender, EventArgs e) =>
                {
                    mousedownl = false;
                    mousedownr = false;
                    mousedown_x = 0;
                    mousedown_y = 0;
                });
                canvas.MouseWheel += new MouseEventHandler((object sender, MouseEventArgs e) =>
                {
                    double cursor_x = e.X, cursor_y = e.Y;
                    double times_x = (zero.X - cursor_x) / unitlength;
                    double times_y = (zero.Y - cursor_y) / unitlength;
                    if (e.Delta > 0)
                    {
                        unitlength *= 1.1;
                    }
                    else
                    {
                        unitlength /= 1.1;
                    }
                    unitlength = range(0.01, 1000000, unitlength);
                    unitlengthf = (float)unitlength;
                    zero = new PointLong(
                        (long)(times_x * unitlength + cursor_x),
                        (long)(times_y * unitlength + cursor_y)
                    );

                    Draw();
                });
            };
            canvasmouseaction();
            Action stretchmouseaction = () =>
            {
                bool mousedown = false;
                int mousedownx = 0;
                int spx = 0;
                panel3.MouseDown += new MouseEventHandler((object sender, MouseEventArgs e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        mousedown = true;
                        mousedownx = e.X;
                        spx = 函数显示.Width;
                        panel3.Width = 1;
                        panel3.BackColor = Color.Black;
                    }
                });
                panel3.MouseUp += new MouseEventHandler((object sender, MouseEventArgs e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        mousedown = false;
                        函数显示.Width = panel3.Location.X;
                        变量输入.Location = 函数输入.Location = new Point(函数显示.Width+4, 0);
                        变量输入.Size = 函数输入.Size = new Size(width - 函数显示.Width-4 , 30);
                        RefreshFuncListLocation();
                        panel3.BackColor = Color.White;
                        panel3.Width = 4;
                        函数显示.Refresh();
                        Buf.Render();
                        Draw();
                    }
                });
                panel3.MouseMove += new MouseEventHandler((object sender, MouseEventArgs e) =>
                {
                    if (mousedown)
                    {
                        panel3.Location = new Point((int)range(100f, canvas.Width / 2, PointToClient(Cursor.Position).X), 0);
                        panel3.Refresh();
                    }
                });

            };
            stretchmouseaction();
        }
        public void RefreshFuncListLocation()
        {
            panel2.Width = panel1.ClientSize.Width;
            panel2.Location = new Point(0, 0);
            for (int i = 0; i < panel2.Controls.Count; i++)
            {
                panel2.Controls[i].Location = new Point(0, i * 30);
            }
            panel2.Height = panel2.Controls.Count * 30;
        }

        private void DrawerTimer_Elapsed(object sender, EventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            if (canvas != null &&loaded)
            {
                Graphics = canvas.CreateGraphics();
                Buf = BufferedGraphicsManager.Current.Allocate(Graphics, ClientRectangle);
                FunctionBitmap = new Bitmap(width,height);
                Graphics fb= Graphics.FromImage(FunctionBitmap);
                Graphics gb = Buf.Graphics;
                Graphics.SmoothingMode = gb.SmoothingMode = SmoothingMode.AntiAlias;
                Graphics.InterpolationMode = gb.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Graphics.CompositingQuality = gb.CompositingQuality = CompositingQuality.HighQuality;
                gb.Clear(Color.White);
                DrawAxis(gb);
                DrawFunctions(fb);
                FunctionBitmap.MakeTransparent(Color.White);
                lastzero = zero;
                gb.DrawImage(FunctionBitmap,0,0);
                Buf.Render();
                GC.Collect();
            }
        }
        private void DrawAxis(Graphics gb)
        {
            Pen b0, b1, b2;
            b0 = new Pen(Color.FromArgb(190, 190, 190));
            b1 = new Pen(Color.FromArgb(128, 128, 128));
            if (range(0, width, zero.X) == zero.X)
            {
                gb.DrawLine(
                    bPen,
                    new PointF(zero.X, 0f),
                    new PointF(zero.X, height)
                );
            }
            if (range(0, height, zero.Y) == zero.Y)
            {
                gb.DrawLine(
                bPen,
                new PointF(0f, zero.Y),
                new PointF(width, zero.Y)
            );
            }

            //sign
            int zs = (int)Math.Floor(Math.Log((350 / unitlength), 10));
            double addnum = Math.Pow(10, zs);
            decimal addnumD = (decimal)Math.Pow(10, zs);
            double p = range(0, height - font.Height - 4, zero.Y);
            float fff = 1f / 4f * font.Height;
            double ii, iii;
            for(double i=Math.Min(zero.X-(addnum*unitlength), mathtorealX(round(realtomathX(width), -zs)));i>0;i-= (addnum * unitlength))
            {
                decimal num = RoundD(realtomathX(i),-zs);
                if (num%(10*addnumD) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    (float)i, 0,
                    (float)i, height
                );
                gb.DrawString(num.ToString(), font, bBrush, (float)(i - (num.ToString().Length) * fff - 2), (float)p);
            }
            for (double i = Math.Max(zero.X + (addnum * unitlength), mathtorealX(round(realtomathX(0), -zs))); i <width; i += (addnum * unitlength))
            {
                decimal num = RoundD(realtomathX(i), -zs);
                if (num % (10 * addnumD) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    (float)i, 0,
                    (float)i, height
                );
                gb.DrawString(num.ToString(), font, bBrush, (float)(i - (num.ToString().Length) * fff - 2), (float)p);
            }
            for (double i = Math.Min(zero.Y - (addnum * unitlength), mathtorealY(round(realtomathY(height), -zs))); i > 0; i -= (addnum * unitlength))
            {
                decimal num = RoundD(realtomathY(i),-zs);
                if (num % (10 * addnumD) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    0, (float)i,
                    width, (float)i
                );
                if (panel1.Width + 3 > zero.X)
                {
                    gb.DrawString(num.ToString(), font, bBrush, (float)panel1.Width + 3, (float)(i - font.Height / 2 - 2));
                }
                else if (zero.X + num.ToString().Length * font.Height / 2 > width - 3)
                {
                    gb.DrawString(num.ToString(), font, bBrush, (float)width - num.ToString().Length * font.Height / 2 - 5, (float)(i - font.Height / 2 - 2));
                }
                else
                {
                    gb.DrawString(num.ToString(), font, bBrush, (float)zero.X, (float)(i - font.Height / 2 - 2));
                }
            }
            for (double i = Math.Max(zero.Y + (addnum * unitlength), mathtorealY(round(realtomathY(0), -zs))); i <height; i += (addnum * unitlength))
            {
                decimal num = RoundD(realtomathY(i),-zs);
                if (num % (10 * addnumD) == 0) { b2 = b1; } else { b2 = b0; }
                gb.DrawLine(
                    b2,
                    0, (float)i,
                    width, (float)i
                );
                if (panel1.Width + 3 > zero.X)
                {
                    gb.DrawString(num.ToString(), font, bBrush, (float)panel1.Width + 3, (float)(i - font.Height / 2 - 2));
                }
                else if (zero.X + num.ToString().Length * font.Height / 2 > width - 3)
                {
                    gb.DrawString(num.ToString(), font, bBrush, (float)width - num.ToString().Length * font.Height / 2 - 5, (float)(i - font.Height / 2 - 2));
                }
                else
                {
                    gb.DrawString(num.ToString(), font, bBrush, (float)zero.X, (float)(i - font.Height / 2 - 2));
                }
            }
            gb.DrawString("0", font, bBrush, zero.X + 3, zero.Y);
        }
        private void DrawFunctions(Graphics gb)
        {
            Bitmap[] bitmaps = new Bitmap[Funcs.Count];
            Parallel.For(0, Funcs.Count, (index) =>
            {
                Function function=Funcs[index];
                if (!function.Visible)
                    return;
                Bitmap bmp = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.White);
                bmp.MakeTransparent(Color.White);
                g = Graphics.FromImage(bmp);
                DrawFunction(new Rectangle(0, 0, width, height), function.LeftFunction, function.RightFunction, function.Operator, g, new SolidBrush(function.Color));
                bitmaps[index] = bmp;
            });
            foreach(var bmp in bitmaps)
            {
                gb.DrawImage(bmp,0,0);
            }
        }
        private void DrawFunction(Rectangle rectangle, ImpFunction L, ImpFunction R,char Operator, Graphics gb,SolidBrush colorbrush)
        {
            int width = rectangle.Width;
            int height = rectangle.Height;
            Color bc = colorbrush.Color;
            switch (Operator)
            {
                case '=':
                    DrawFunctionGLE(rectangle, L, R, IntervalMath.Equal, gb, colorbrush);
                    break;
                case '<':
                    colorbrush.Color = Color.FromArgb(128,bc);
                    DrawFunctionGLE(rectangle,L,R, IntervalMath.Less,gb, colorbrush);
                    break;
                case '>':
                    colorbrush.Color = Color.FromArgb(128, bc);
                    DrawFunctionGLE(rectangle, L, R, IntervalMath.Greater, gb, colorbrush);
                    break;
                case '≥':
                    DrawFunctionGLE(rectangle, L, R, IntervalMath.Equal, gb, colorbrush);
                    colorbrush.Color = Color.FromArgb(128, bc);
                    DrawFunctionGLE(rectangle, L, R, IntervalMath.Greater, gb, colorbrush);
                    break;
                case '≤':
                    DrawFunctionGLE(rectangle, L, R, IntervalMath.Equal, gb, colorbrush);
                    colorbrush.Color = Color.FromArgb(128, bc);
                    DrawFunctionGLE(rectangle, L, R, IntervalMath.Less, gb, colorbrush);
                    break;
                default:
                    break;
            }
        }
        public bool drawwithbz=false;
        public void DrawFunctionGLE(Rectangle rectangle, ImpFunction L, ImpFunction R, Func<Interval, Interval, IntervalIntersectionState> EqGtLt, Graphics gb, Brush colorbrush)
        {
            RectangleDrawer rd = new RectangleDrawer(gb, colorbrush);
            List<Rectangle> rectangleFsCon = new List<Rectangle>() { rectangle };
            for (; ; )
            {
                Rectangle[] rs = rectangleFsCon.ToArray();
                rectangleFsCon.Clear();
                Parallel.ForEach<Rectangle>(rs, (rr) =>
                {
                    DrawFunctionInRect(rr, L, R, EqGtLt, gb, rd, rectangleFsCon);
                });
                rd.Draw();
                if (rectangleFsCon.Count == 0)
                {
                    break;
                }
                if (!drawwithbz)
                {
                    continue;
                }
            }
            drawwithbz = false;
        }
        public class RectangleDrawer
        {
            private List<Rectangle> Rects= new List<Rectangle>();
            private object synroot;
            public Graphics graphics;
            private Brush brush;
            public RectangleDrawer(Graphics graphics, Brush brush)
            {
                this.graphics = graphics;
                this.brush = brush;
                synroot = graphics;
            }
            public void Add(Rectangle r)
            {
                lock (synroot)
                {
                    Rects.Add(r);
                }
                if (Rects.Count > 500)
                    Draw();
            }
            public void Draw()
            {
                lock (synroot)
                {
                    if (Rects.Count != 0)
                    {
                        graphics.FillRectangles(brush, Rects.ToArray());
                        Rects.Clear();
                    }
                }

            }
        }
        private void DrawFunctionInRect(Rectangle r,ImpFunction L,ImpFunction R, Func<Interval, Interval, IntervalIntersectionState> EqGtLt, Graphics gb, RectangleDrawer rfs,List<Rectangle> con)
        {
            int xtimes=2, ytimes=2;
            if (r.Width > r.Height)
            {
                xtimes = 2;
                ytimes = 1;
            }
            else if(r.Width<r.Height)
            {
                xtimes = 1;
                ytimes = 2;
            }
            int dx = (int)Math.Ceiling((double)r.Width / xtimes);
            int dy = (int)Math.Ceiling((double)r.Height / ytimes);
            double mwidth = r.Width/unitlength;
            double mheight = -r.Height / unitlength;
            double left=realtomathX(r.Left);
            double top=realtomathY(r.Top);
            List<Rectangle> rf=new List<Rectangle>();
            for(int i = r.Left; i < r.Right; i+=dx)
            {
                Interval Xi = new Interval(realtomathX(i),realtomathX(Math.Min(i+dx,r.Right)));
                for(int j = r.Top; j<r.Bottom; j+=dy)
                {
                    Interval Yi = new Interval(realtomathY(j), realtomathY(Math.Min(j+dy, r.Bottom)));
                    switch (EqGtLt(
                        L(
                            Xi,Yi,
                            varlist
                        ),
                        R(
                            Xi, Yi,
                            varlist
                        )
                    ))
                    {
                        case IntervalIntersectionState.Existent:
                            rfs.Add(new Rectangle(i,j, Math.Min(i + dx, r.Right)-i,Math.Min(j + dy, r.Bottom)-j));
                            break;
                        case IntervalIntersectionState.Possible:
                            if (r.Width == 1 && r.Height==1 )
                            {
                                rfs.Add(new Rectangle(i, j, Math.Min(i + dx, r.Right) - i, Math.Min(j + dy, r.Bottom) - j));
                            }
                            else
                            {
                                rf.Add(new Rectangle(i, j, Math.Min(i + dx, r.Right) - i, Math.Min(j + dy, r.Bottom) - j));
                            }
                            break;
                        default:
                            if (!drawwithbz) { break; }
                            lock (rfs.graphics)
                            {
                                rfs.graphics.FillRectangle(new SolidBrush(Color.FromArgb(100,Color.Red)), new Rectangle(i, j, Math.Min(i + dx, r.Right) - i, Math.Min(j + dy, r.Bottom) - j));
                            }
                            break;

                    }
                }
            }
            lock ((con as ICollection).SyncRoot)
            {
                con.AddRange(rf);
            }
            rf.Clear();
        }
        
        private struct PointLong
        {
            public long X;
            public long Y;
            public PointLong(long X,long Y)
            {
                this.X= X; this.Y = Y;
            }
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            Buf.Render();
        }
        public float round(float num, int fix)
        {
            return (float)Math.Round(num, fix);
        }
        public double round(double num, int fix)
        {
            if (fix < 0)
            {
                num /= Math.Pow(10, -fix);
                num = Math.Round(num, 0);
                num *= Math.Pow(10, -fix);
                return num;
            }
            return Math.Round(num, fix);
        }
        public static double range(double min, double max, double num)
        {
            if (min > num) { return min; }
            if (max < num) { return max; }
            return num;
        }
        public double mathtorealX(double d)
        {
            return zero.X + d * unitlength;
        }
        public double mathtorealY(double d)
        {
            return zero.Y + -d * unitlength;
        }
        public double realtomathX(double d)
        {
            return (d - zero.X) / unitlength;
        }
        public double realtomathY(double d)
        {
            return -(d - zero.Y) / unitlength;
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            width = ClientSize.Width;
            height = ClientSize.Height;
        }

        public PointF mathtoreal(PointF f)
        {
            f.X = zero.X + f.X * unitlengthf;
            f.Y = zero.Y + -f.Y * unitlengthf;
            return f;
        }
        public PointF mathtoreal(float fx, float fy)
        {
            return mathtoreal(new PointF(fx, fy));
        }
        public PointF realtomath(PointF f)
        {
            f.X = (f.X - zero.X) / unitlengthf;
            f.Y = -(f.Y - zero.Y) / unitlengthf;
            return f;
        }
        public PointF realtomath(float fx, float fy)
        {
            return realtomath(new PointF(fx, fy));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            width = canvas.Width;
            height = canvas.Height;
            loaded = true;
            Draw();
        }

        public Point realtomath(Point f)
        {
            f.X = (int)(((float)f.X - zero.X) / unitlengthf);
            f.Y = (int)(-((float)f.Y - zero.Y) / unitlengthf);
            return f;
        }
        public decimal Min(decimal n1, decimal n2)
        {
            return n1 < n2 ? n1 : n2;
        }

        public decimal Max(decimal n1, decimal n2)
        {
            return n1 > n2 ? n1 : n2;
        }
        public decimal RoundD(double num,int fix)
        {
            decimal d = (decimal)num;
            d /= PowD(10, -fix);
            d = Math.Round(d, 0);
            d *= PowD(10, -fix);
            return d;
        }
        private decimal PowD(decimal num,int times)
        {
            decimal result = 1;
            if (times > 0)
            {
                for(int i = 0; i < times; i++)
                {
                    result *= num;
                }
                return result;
            }else if (times == 0)
            {
                return 1;
            }
            else
            {
                for (int i = 0; i > times; i--)
                {
                    result /= num;
                }
                return result;
            }
        }
        private struct Rectd { 
            public double x;
            public double y;
            public double width;
            public double height;
            public Rectd(double x,double y,double width,double height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
        }
        public void button2_Click(object sender, EventArgs e)
        {
            Button tb = (Button)sender;
            if (函数输入.Visible)
            {
                tb.Text = "常量";
            }
            else
            {
                tb.Text = "函数";
            }
            函数输入.Visible = !函数输入.Visible;
            变量输入.Visible = !变量输入.Visible;
        }
        public void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    try
                    {
                        if (textBox.Text != "")
                        {
                            Funcs.Add(new Function(textBox.Text));
                            textBox.Text = "";
                            Draw();
                        }
                    }
                    catch (Exception ex) {
                        switch (ex.Message)
                        {
                            case "格式错误":
                                if(!textBox.Text.Contains("y") && textBox.Text.Contains("x"))
                                {
                                    try
                                    {
                                        Funcs.Add(new Function("y=" + textBox.Text));
                                        textBox.Text = "";
                                        Draw();
                                    }
                                    catch (Exception ex2) {
                                        MessageBox.Show(ex2.Message);
                                    }
                                }else if (textBox.Text.Contains("y") && !textBox.Text.Contains("x"))
                                {
                                    try
                                    {
                                        Funcs.Add(new Function("x=" + textBox.Text));
                                        textBox.Text = "";
                                        Draw();
                                    }
                                    catch (Exception ex2)
                                    {
                                        MessageBox.Show(ex2.Message);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        Funcs.Add(new Function( textBox.Text+"=0"));
                                        textBox.Text = "";
                                        Draw();
                                    }
                                    catch (Exception ex2)
                                    {
                                        MessageBox.Show(ex2.Message);
                                    }
                                }
                                break;
                        }
                    }
                    finally { 
                    }
                    break;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            string[] s;
            do
            {
                s = Interaction.InputBox("").Split(' ');
            } while (s.Length <= 1 || !Double.TryParse(s[0], out double n1) || !Double.TryParse(s[1], out double n2));
            trackBar1.Minimum = (int)(Double.Parse(s[0]) * 1000);
            trackBar1.Maximum = (int)(Double.Parse(s[1]) * 1000);

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(
                (e.KeyChar == '\b') || (e.KeyChar == '.') ||
                ('0' <= e.KeyChar && e.KeyChar <= '9') ||
                ('a' <= e.KeyChar && e.KeyChar <= 'z') ||
                (e.KeyChar == '%') || (e.KeyChar == '^') || (e.KeyChar == '*') ||
                (e.KeyChar == '(') || (e.KeyChar == ')') || (e.KeyChar == '+') ||
                (e.KeyChar == '-') || (e.KeyChar == '/') || 
                (e.KeyChar == '=') || (e.KeyChar == '<') || (e.KeyChar == '>') ||
                (e.KeyChar == 3) ||
                (e.KeyChar == 22) ||
                (e.KeyChar == 24)

            ))
            {
                e.Handled = true;//ban
            }
        }
        private void toolstriptextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(
                (e.KeyChar == '\b') ||
                ('0' <= e.KeyChar && e.KeyChar <= '9')

            ))
            {
                e.Handled = true;//ban
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            trackBar1.Enabled = true;
            try
            {
                trackBar1.Value = (int)(varlist[comboBox1.SelectedIndex] * 1000);
            }
            catch (Exception ex)
            {
                int min = trackBar1.Minimum;
                int max = trackBar1.Maximum;
                int value = (int)(varlist[comboBox1.SelectedIndex] * 1000);
                trackBar1.Minimum = value - 1;
                trackBar1.Maximum = value + 1;
                trackBar1.Value = value;
                trackBar1.Minimum = min;
                trackBar1.Maximum = max;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            TrackBar bar = (TrackBar)sender;
            string varname = (string)comboBox1.Items[comboBox1.SelectedIndex];
            double variable = ((double)bar.Value) / 1000;
            if (Math.Abs(Math.Abs(variable) - Math.Round(Math.Abs(variable))) < (double)(bar.Maximum - bar.Minimum) / 330000d)
            {
                variable = Math.Sign(variable) * Math.Round(Math.Abs(variable));
            }
            label3.Text = (variable).ToString();
            varlist[varname[0] - 'a'] = variable;
            Draw();
        }
        class p : Panel
        {
            public p()
            {
            }
        }
    }
    public static class ExtentedMethod
    {
        private static MethodInfo mi = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
        public static void SetControlStyle(this Control con,ControlStyles styles,bool b)
        {
            mi.Invoke(con,new object[] {styles,b});
        }
    }
}
