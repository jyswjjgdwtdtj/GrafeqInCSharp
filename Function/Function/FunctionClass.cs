using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Function.MathFunction;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Function
{
    public partial class Form1
    {
        private class Function
        {
            private static Array colors = Enum.GetValues(typeof(KnownColor));
            private static Random rnd = new Random();
            public static Form1 Form;
            private static Panel FunctionPanel;

            public string LeftFormula, RightFormula;
            public ImpFunction LeftFunction, RightFunction;
            public Color Color;
            public bool Visible = true;
            public char Operator;

            private Panel panel;
            private System.Windows.Forms.Label label;
            private Button button;

            public static void Init(Form1 form)
            {
                Form = form;
                FunctionPanel = Form.panel2;
            }
            public Function(string Formula)
            {
                Formula = Formula.ToLower();
                Formula = Formula.Replace("<=", "≤").Replace(">=", "≥");
                Regex regex = new Regex(@"^(.*)(=|<|>|≤|≥)(.*)$");
                var f = regex.Matches(Formula);
                if (f.Count == 0) { throw new Exception("格式错误"); }
                LeftFormula = f[0].Groups[1].Value;
                RightFormula = f[0].Groups[3].Value;
                if (LeftFormula == "" || RightFormula == "")
                {
                    throw new Exception("算式错误");
                }
                Operator = f[0].Groups[2].Value[0];
                LeftFunction = GetFunc(LeftFormula);
                RightFunction = GetFunc(RightFormula);
                do
                {
                    Color = Color.FromKnownColor((KnownColor)colors.GetValue(rnd.Next(colors.Length - 27) + 27));
                } while (Color.GetBrightness() > 0.7f);

                FunctionPanel.Width = FunctionPanel.Parent.ClientSize.Width;
                panel = new Panel();
                label = new System.Windows.Forms.Label();
                label.ForeColor = Color.Black;
                label.Text = Formula;
                label.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                label.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Location = new Point(0, 0);
                label.Size = new Size(1000, 30);
                label.AutoSize = false;
                label.Click += DoubleClick;
                label.ContextMenuStrip = Form.contextMenuStrip1;
                label.MouseEnter += MouseEnter;
                label.BackColor = Color;

                button = new Button();
                button.Text = "✕";
                button.FlatStyle = FlatStyle.System;
                button.TextAlign = ContentAlignment.MiddleCenter;
                button.Anchor = AnchorStyles.Right;
                button.Location = new Point(FunctionPanel.Width - 30, 0);
                button.Size = new Size(30, 30);
                button.BackColor = Color.White;
                button.Click += btnclick;

                panel.BackColor = Color;
                panel.Location = new Point(0, FunctionPanel.Controls.Count * 30);
                panel.Size = new Size(FunctionPanel.Width, 30);
                panel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                panel.Controls.Add(label);
                panel.Controls.Add(button);
                button.BringToFront();
                FunctionPanel.Controls.Add(panel);
                Form.RefreshFuncListLocation();
                Form.RefreshFuncListLocation();
            }
            private void DoubleClick(object sender, EventArgs e)
            {
            }
            private void MouseEnter(object sender, EventArgs e)
            {
                //form.rightbuttonon = this;
                //form.toolStripTextBox1.Text = this.stokewidth.ToString();
            }
            private void btnclick(object sender, EventArgs e)
            {
                Button btn = (Button)sender;
                FunctionPanel.Controls.Remove(btn.Parent);
                Form.Funcs.Remove(this);
                /*if (form.functionrevising)
                {
                    if (form.revisingfunc == this)
                    {
                        form.functionrevising = false;
                        form.textBox1.Text = "";
                    }
                }*/
                LeftFunction = RightFunction = null;
                label.Dispose();
                label = null;
                button.Dispose();
                button = null;
                panel.Dispose();
                panel = null;
                Form.RefreshFuncListLocation();
                Form.Draw();
            }
        }
    }
    
}
