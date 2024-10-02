using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Function
{
    public class MathFunction
    {
        public delegate Interval ImpFunction(Interval X, Interval Y, double[] vars);
        public static ImpFunction GetFunc(string formula)
        {
            formula = formula.ToLower();
            if (formula.Length - formula.Replace("(", "").Length != formula.Length - formula.Replace(")", "").Length)
            {
                throw new Exception("左右括号数量不一致");
            }
            Stack<char> op = new Stack<char>();
            Stack<op> num = new Stack<op>();
            op.Push('#');
            bool lastisop = true;
            for (int i = 0; i < formula.Length;)
            {
                int opnum = GetOperationLevel(formula[i].ToString());
                if (opnum == 0)
                {
                    int index = GetCompleteValue(formula.Substring(i, formula.Length - i));
                    string n = formula.Substring(i, index);
                    if (formula[Math.Min(i + index, formula.Length - 1)] == '(')
                    {
                        bool isused = false;
                        for (int j = 0; j < functions.Count; j++)
                        {
                            if (n == functions[j].ToLower())
                            {
                                op.Push('(');
                                op.Push((char)(j + 128));
                                i += index + 1;
                                isused = true;
                                break;
                            }
                        }
                        if (!isused)
                        {
                            throw new Exception("函数未知'" + n + "'");
                        }
                    }
                    else if (n.ToUpper() != n.ToLower())
                    {
                        num.Push(new op("var", n, 0));
                        i += index;
                    }
                    else
                    {
                        num.Push(new op("num", "", double.Parse(n)));
                        i += index;
                    }
                    lastisop = false;
                    continue;
                }
                else
                {
                    if (lastisop && formula[i] != '(' && formula[Math.Max(0, i - 1)] != ')')
                    {
                        if (formula[i] == '-')
                        {
                            int index = GetCompleteValue(formula.Substring(i + 1, formula.Length - i - 1));
                            num.Push(new op("num", "", 0d));
                            string n = formula.Substring(i + 1, index);
                            if (n.ToUpper() != n.ToLower())
                            {
                                num.Push(new op("var", n, 0));
                            }
                            else
                            {
                                num.Push(new op("num", "", double.Parse(n)));
                            }
                            num.Push(new op("op", "-", 0));
                            lastisop = false;
                            i += index + 1;
                            continue;
                        }
                        else
                        {
                            throw new Exception("连续运算符'" + formula[i - 1] + formula[i] + "'");
                        }
                    }
                    else
                    {
                        if (formula[i] == '(')
                        {
                            if (i > 0)
                            {
                                if (formula[i - 1] == ')')
                                {
                                    op.Push('*');
                                }
                            }
                            op.Push('(');
                        }
                        else if (formula[i] == ')')
                        {
                            MoveOperator(op, num);
                        }
                        else
                        {
                            if (op.Peek() == '(')
                            {
                                op.Push(formula[i]);
                            }
                            else
                            {
                                JudgeOperator(op, num, formula[i]);
                            }
                        }
                        i++;
                        lastisop = true;
                    }
                }
            }
            if (op.Count != 0)
            {
                while (op.Count != 0 && op.Peek() != '#')
                {
                    num.Push(new op("op", op.Pop().ToString(), 0));
                }
            }
            IEnumerable<op> e = num.Reverse();

            DynamicMethod method = new DynamicMethod("ImpFunction", typeof(Interval), new Type[3] { typeof(Interval), typeof(Interval), typeof(double[]) });
            ILGenerator IL = method.GetILGenerator();
            foreach (var i in e)
            {
                switch (i.type)
                {
                    case "var":
                        switch (i.value_S)
                        {
                            case "x":
                                IL.Emit(OpCodes.Ldarg_0);
                                break;
                            case "y":
                                IL.Emit(OpCodes.Ldarg_1);
                                break;
                            case "e":
                                IL.Emit(OpCodes.Ldc_R8, Math.E);
                                IL.Emit(OpCodes.Ldc_R8, Math.E);
                                IL.Emit(OpCodes.Newobj, typeof(Interval).GetConstructor(new Type[] { typeof(double), typeof(double) }));
                                break;
                            case "pi":
                                IL.Emit(OpCodes.Ldc_R8, Math.PI);
                                IL.Emit(OpCodes.Ldc_R8, Math.PI);
                                IL.Emit(OpCodes.Newobj, typeof(Interval).GetConstructor(new Type[] { typeof(double), typeof(double) }));
                                break;
                            default:
                                if (i.value_S.Length == 1 && 'a' <= i.value_S[0] && i.value_S[0] <= 'z')
                                {
                                    IL.Emit(OpCodes.Ldarg_2);
                                    IL.Emit(OpCodes.Ldc_I4, (i.value_S[0] - 'a'));
                                    IL.Emit(OpCodes.Ldelem_R8);
                                    IL.Emit(OpCodes.Ldarg_2);
                                    IL.Emit(OpCodes.Ldc_I4, (i.value_S[0] - 'a'));
                                    IL.Emit(OpCodes.Ldelem_R8);
                                    IL.Emit(OpCodes.Newobj, typeof(Interval).GetConstructor(new Type[] { typeof(double), typeof(double) }));
                                    break;
                                }
                                else
                                {
                                    throw new Exception("变量未知 '" + i.value_S + "'");
                                }
                        }
                        break;
                    case "num":
                        IL.Emit(OpCodes.Ldc_R8, i.value_N);
                        IL.Emit(OpCodes.Ldc_R8, i.value_N);
                        IL.Emit(OpCodes.Newobj, typeof(Interval).GetConstructor(new Type[] { typeof(double), typeof(double) }));
                        break;
                    case "op":
                        if (i.value_S == "^")
                        {
                            IL.Emit(OpCodes.Call, typeof(IntervalMath).GetMethod("Pow"));
                        }
                        else if (i.value_S == "%")
                        {
                            IL.Emit(OpCodes.Call, typeof(IntervalMath).GetMethod("Mod"));
                        }
                        else if (128 <= i.value_S[0] && i.value_S[0] < 256)
                        {
                            IL.Emit(OpCodes.Call, typeof(IntervalMath).GetMethod(functions[i.value_S[0] - 128]));
                        }
                        else
                        {
                            IL.Emit(OpCodes.Call, typeof(IntervalMath).GetMethod(GetOpCodeFromOpChar(i.value_S)));
                        }
                        break;
                }
            }
            IL.Emit(OpCodes.Ret);
            var mf = (ImpFunction)method.CreateDelegate(typeof(ImpFunction));
            mf(new Interval(-1, 1), new Interval(-1, 1), new double[30]);
            try
            {

            }
            catch (Exception ex)
            {
                throw new Exception("算式错误");
            }
            return mf;
        }

        private static List<string> functions = new List<string>()
        {
"Abs",
"Acos",
"Asin",
"Atan",
"Ceiling",
"Cos",
"Cosh",
"Exp",
"Floor",
"Log",
"Log10",
"Round",
"Sin",
"Sinh",
"Sqrt",
"Tan",
"Tanh",
"Truncate",
"Sgn"
        };
        private static int GetOperationLevel(string c)
        {
            switch (c)
            {
                case "+": return 1;
                case "-": return 1;
                case "*": return 2;
                case "/": return 2;
                case "%": return 3;
                case "^": return 3;
                case "#": return -1;
                case "(": return -1;
                case ")": return -1;
                default: return 0;
            }
        }
        private static string GetOpCodeFromOpChar(string OpChar)
        {
            switch (OpChar)
            {
                case "+": return "Add";
                case "-": return "Sub";
                case "*": return "Mul";
                case "/": return "Div";
                default:
                    MessageBox.Show(OpChar);
                    throw new Exception();
            }
        }
        private static int GetOperationLevel(char c)
        {
            return GetOperationLevel(c.ToString());
        }
        private static int GetCompleteValue(string formula)
        {
            int index = formula.Length;
            for (int i = 0; i < formula.Length; i++)
            {
                int num = GetOperationLevel(formula[i].ToString());
                if (num != 0)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        private struct op
        {
            public string type;
            public string value_S;
            public double value_N;
            public op(string type, string value_S, double value_N)
            {
                this.type = type;
                this.value_S = value_S;
                this.value_N = value_N;
            }
        }
        private static void MoveOperator(Stack<char> opStack, Stack<op> numStack)
        {
            char s = opStack.Pop();
            if (s == '(')
            {
                return;
            }
            else
            {
                numStack.Push(new op("op", s.ToString(), 0));
                MoveOperator(opStack, numStack);
                return;
            }
        }
        private static void JudgeOperator(Stack<char> opStack, Stack<op> numStack, char x)
        {
            int xNum = GetOperationLevel(x);
            int opNum = GetOperationLevel(opStack.Peek());
            if (xNum > opNum || numStack.Peek().value_S == "(")
            {
                opStack.Push(x);
                return;
            }
            else
            {
                char opStr = opStack.Pop();
                numStack.Push(new op("op", opStr.ToString(), 0));
                JudgeOperator(opStack, numStack, x);
                return;
            }
        }
        public struct Interval
        {
            public double min;
            public double max;
            private static double nan = Double.NaN;
            public static Interval NULL = new Interval(nan, nan);

            public Interval(double s, double e)
            {
                if (s > e)
                {
                    double d = e;
                    e = s;
                    s = d;
                }
                min = s; max = e;
            }
            public bool Cross(double num)
            {
                return min <= num && num <= max;
            }
            public bool isNull
            {
                get
                {
                    return double.IsNaN(min) || double.IsNaN(max);
                }
            }
            public override string ToString()
            {
                return "[" + min + "," + max + "]";
            }

        }
        public enum IntervalIntersectionState
        {
            Nonexistent,
            Possible,
            Existent
        }
        public static class IntervalMath
        {
            private static double pi = Math.PI;
            public static Interval Add(Interval i1, Interval i2)
            {
                if (i1.isNull || i2.isNull)
                {
                    return i1;
                }
                return new Interval(i1.min + i2.min, i1.max + i2.max);
            }
            public static Interval Sub(Interval i1, Interval i2)
            {
                if (i1.isNull || i2.isNull)
                {
                    return i1;
                }
                return Add(i1, new Interval(-i2.max, -i2.min));
            }
            public static Interval Mul(Interval i1, Interval i2)
            {
                if (i1.isNull || i2.isNull)
                {
                    return i1;
                }
                double[] ds = MinMaxForDouble(i1.min * i2.min, i1.min * i2.max, i1.max * i2.min, i1.max * i2.max);
                return new Interval(
                    ds[0], ds[1]
                );
            }
            public static Interval Div(Interval i1, Interval i2)
            {
                if (i1.isNull || i2.isNull)
                {
                    return i1;
                }
                double s = double.MinValue, e = double.MaxValue;
                bool c = i2.Cross(0);
                List<double> results = new List<double> { i1.min / i2.min, i1.min / i2.max, i1.max / i2.min, i1.max / i2.max };
                foreach (var r in results.ToArray())
                {
                    if (c)
                    {
                        if (r > 0)
                            results.Add(double.PositiveInfinity);
                        if (r < 0)
                            results.Add(e = double.NegativeInfinity);
                    }
                }
                double[] ds = MinMaxForDouble(results.ToArray());
                return new Interval(ds[0], ds[1]);
            }
            public static Interval Sin(Interval i)
            {
                if (i.isNull)
                {
                    return i;
                }
                Interval result = new Interval(0, 0);
                Interval minmax = new Interval(0, 0);
                double a = i.min;
                double b = i.max;
                if (Math.Floor((a / pi - 0.5) / 2) < Math.Floor((b / pi - 0.5) / 2))
                {
                    minmax.max = 1;
                    result.max = 1;
                }
                if (Math.Floor((a / pi + 0.5) / 2) < Math.Floor((b / pi + 0.5) / 2))
                {
                    minmax.min = 1;
                    result.min = -1;
                }
                if (minmax.min == 0)
                {
                    result.min = Math.Min(Math.Sin(a), Math.Sin(b));
                }
                if (minmax.max == 0)
                {
                    result.max = Math.Max(Math.Sin(a), Math.Sin(b));
                }
                return result;
            }

            public static Interval Pow(Interval i1, Interval i2)
            {
                if (i2.min != i2.max)
                {
                    throw new ArgumentException("指数不为变量");
                }
                double num = i2.min;
                if (i1.max < 0 && num != (int)num)
                {
                    return Interval.NULL;
                }
                return new Interval(Math.Pow(i1.min, num), Math.Pow(i1.max, num));
            }
            public static Interval Sqrt(Interval i)
            {
                if (i.isNull)
                {
                    return i;
                }
                if (i.max < 0)
                {
                    return Interval.NULL;
                }
                else if (i.Cross(0))
                {
                    return new Interval(0, Math.Sqrt(i.max));
                }
                else
                {
                    return new Interval(Math.Sqrt(i.min), Math.Sqrt(i.max));
                }
            }
            public static Interval Cos(Interval i)
            {
                if (i.isNull)
                {
                    return i;
                }
                Interval result = new Interval(0, 0);
                Interval minmax = new Interval(0, 0);
                double a = i.min;
                double b = i.max;
                if (Math.Floor((a / pi) / 2) < Math.Floor((b / pi) / 2))
                {
                    minmax.max = 1;
                    result.max = 1;
                }
                if (Math.Floor((a / pi + 1) / 2) < Math.Floor((b / pi + 1) / 2))
                {
                    minmax.min = 1;
                    result.min = -1;
                }
                if (minmax.min == 0)
                {
                    result.min = Math.Min(Math.Cos(a), Math.Cos(b));
                }
                if (minmax.max == 0)
                {
                    result.max = Math.Max(Math.Cos(a), Math.Cos(b));
                }
                return result;
            }
            public static Interval Tan(Interval i)
            {
                if (i.isNull)
                {
                    return i;
                }
                return Div(Sin(i), Cos(i));
            }

            public static Interval Atan(Interval i)
            {
                if (i.isNull)
                {
                    return i;
                }
                return new Interval(Math.Atan(i.min), Math.Atan(i.max));
            }

            public static Interval Min(Interval i1, Interval i2)
            {
                return new Interval(Math.Min(i1.min, i2.min), Math.Min(i1.max, i2.max));
            }

            public static Interval Max(Interval i1, Interval i2)
            {
                return new Interval(Math.Max(i1.min, i2.min), Math.Max(i1.max, i2.max));
            }

            public static Interval Mod(Interval i1, Interval i2)
            {
                if (i1.isNull)
                {
                    return i1;
                }
                if (i2.min != i2.max)
                {
                    throw new ArgumentException();
                }
                if (Math.Ceiling(i1.min / i2.min) == Math.Ceiling(i1.max / i2.min))
                {
                    return new Interval(i1.min % i2.min, i1.max % i2.min);
                }
                else
                {
                    return new Interval(0, i2.min);
                }

            }

            public static IntervalIntersectionState Less(Interval i1, Interval i2)
            {
                if (i1.isNull || i2.isNull)
                {
                    return IntervalIntersectionState.Nonexistent;
                }
                if (i1.min > i2.max)
                {
                    return IntervalIntersectionState.Nonexistent;
                }
                else if (i1.max < i2.min)
                {
                    return IntervalIntersectionState.Existent;
                }
                else
                {
                    return IntervalIntersectionState.Possible;
                }
            }
            public static IntervalIntersectionState Greater(Interval i1, Interval i2)
            {
                if (i1.isNull || i2.isNull)
                {
                    return IntervalIntersectionState.Nonexistent;
                }
                if (i1.min > i2.max)
                {
                    return IntervalIntersectionState.Existent;
                }
                else if (i1.max < i2.min)
                {
                    return IntervalIntersectionState.Nonexistent;
                }
                else
                {
                    return IntervalIntersectionState.Possible;
                }
            }
            public static IntervalIntersectionState Equal(Interval i1, Interval i2)
            {
                if (i1.isNull || i2.isNull)
                {
                    return IntervalIntersectionState.Nonexistent;
                }
                if (i2.max < i1.min || i2.min > i1.max)
                {
                    return IntervalIntersectionState.Nonexistent;
                }
                else
                {
                    return IntervalIntersectionState.Possible;
                }
            }


            private static double MinForDouble(double n1, double n2, params double[] ns)
            {
                double minnum = Math.Min(n1, n2);
                foreach (var n in ns)
                {
                    minnum = Math.Min(minnum, n);
                }
                return minnum;
            }
            private static double MaxForDouble(double n1, double n2, params double[] ns)
            {
                double maxnum = Math.Max(n1, n2);
                foreach (var n in ns)
                {
                    maxnum = Math.Max(maxnum, n);
                }
                return maxnum;
            }
            private static double[] MinMaxForDouble(double n1, double n2, params double[] ns)
            {
                double minnum = Math.Min(n1, n2);
                double maxnum = Math.Max(n1, n2);
                foreach (var n in ns)
                {
                    minnum = Math.Min(minnum, n);
                    maxnum = Math.Max(maxnum, n);
                }
                return new double[] { minnum, maxnum };

            }
            private static double[] MinMaxForDouble(double[] ns)
            {
                double minnum = Math.Min(ns[1], ns[0]);
                double maxnum = Math.Max(ns[1], ns[0]);
                for (int i = 2; i < ns.Length; i++)
                {
                    minnum = Math.Min(minnum, ns[i]);
                    maxnum = Math.Max(maxnum, ns[i]);
                }
                return new double[] { minnum, maxnum };

            }
        }
    }
}
