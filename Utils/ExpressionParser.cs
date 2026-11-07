using System;
using System.Collections.Generic;
using System.Globalization;

namespace ScienceLab.Utils;

/// <summary>
/// 数学表达式解析器：支持四则运算、幂、括号、常用函数、常量 pi/e、变量 x。
/// 采用递归下降（recursive descent）实现。
/// </summary>
public sealed class ExpressionParser
{
    private readonly string _text;
    private int _pos;

    public ExpressionParser(string text) => _text = text ?? "";

    /// <summary>编译表达式为可反复调用的委托，抛异常则说明语法错误。</summary>
    public static Func<double, double> Compile(string expr)
    {
        var parser = new ExpressionParser(expr);
        var root = parser.ParseExpression();
        parser.SkipWhitespace();
        if (parser._pos < parser._text.Length)
            throw new FormatException($"无法识别的部分：'{parser._text.Substring(parser._pos)}'");
        return x => root.Eval(x);
    }

    public static bool TryCompile(string expr, out Func<double, double>? fn, out string? error)
    {
        try { fn = Compile(expr); error = null; return true; }
        catch (Exception ex) { fn = null; error = ex.Message; return false; }
    }

    private Node ParseExpression()
    {
        var left = ParseTerm();
        while (true)
        {
            SkipWhitespace();
            if (Match('+')) left = new BinOp(left, '+', ParseTerm());
            else if (Match('-')) left = new BinOp(left, '-', ParseTerm());
            else break;
        }
        return left;
    }

    private Node ParseTerm()
    {
        var left = ParsePower();
        while (true)
        {
            SkipWhitespace();
            if (Match('*')) left = new BinOp(left, '*', ParsePower());
            else if (Match('/')) left = new BinOp(left, '/', ParsePower());
            else break;
        }
        return left;
    }

    private Node ParsePower()
    {
        var left = ParseUnary();
        SkipWhitespace();
        if (Match('^')) return new BinOp(left, '^', ParsePower()); // 右结合
        return left;
    }

    private Node ParseUnary()
    {
        SkipWhitespace();
        if (Match('-')) return new Neg(ParseUnary());
        if (Match('+')) return ParseUnary();
        return ParsePrimary();
    }

    private Node ParsePrimary()
    {
        SkipWhitespace();
        if (_pos >= _text.Length) throw new FormatException("表达式意外结束");

        if (Match('('))
        {
            var node = ParseExpression();
            SkipWhitespace();
            if (!Match(')')) throw new FormatException("缺少右括号 ')'");
            return node;
        }

        // 数字
        if (char.IsDigit(_text[_pos]) || _text[_pos] == '.')
        {
            int start = _pos;
            while (_pos < _text.Length && (char.IsDigit(_text[_pos]) || _text[_pos] == '.')) _pos++;
            if (double.TryParse(_text.Substring(start, _pos - start), NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
                return new Const(val);
            throw new FormatException($"无效数字：'{_text.Substring(start, _pos - start)}'");
        }

        // 标识符（变量或函数）
        if (char.IsLetter(_text[_pos]))
        {
            int start = _pos;
            while (_pos < _text.Length && char.IsLetterOrDigit(_text[_pos])) _pos++;
            var id = _text.Substring(start, _pos - start).ToLowerInvariant();
            SkipWhitespace();

            // 函数调用
            if (_pos < _text.Length && _text[_pos] == '(')
            {
                _pos++; // 吃掉 '('
                var arg = ParseExpression();
                SkipWhitespace();
                if (!Match(')')) throw new FormatException($"函数 {id} 缺少右括号");
                return new FuncNode(id, arg);
            }

            // 变量 / 常量
            return id switch
            {
                "x" => new Var(),
                "pi" => new Const(Math.PI),
                "e" => new Const(Math.E),
                _ => throw new FormatException($"未知的标识符：'{id}'"),
            };
        }

        throw new FormatException($"意外的字符：'{_text[_pos]}'");
    }

    private void SkipWhitespace()
    {
        while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos])) _pos++;
    }

    private bool Match(char c)
    {
        if (_pos < _text.Length && _text[_pos] == c) { _pos++; return true; }
        return false;
    }

    // ---- AST 节点 ----
    private abstract class Node { public abstract double Eval(double x); }
    private sealed class Const : Node { private readonly double _v; public Const(double v) => _v = v; public override double Eval(double x) => _v; }
    private sealed class Var : Node { public override double Eval(double x) => x; }
    private sealed class Neg : Node { private readonly Node _n; public Neg(Node n) => _n = n; public override double Eval(double x) => -_n.Eval(x); }
    private sealed class BinOp : Node
    {
        private readonly Node _l, _r; private readonly char _op;
        public BinOp(Node l, char op, Node r) { _l = l; _r = r; _op = op; }
        public override double Eval(double x) => _op switch
        {
            '+' => _l.Eval(x) + _r.Eval(x),
            '-' => _l.Eval(x) - _r.Eval(x),
            '*' => _l.Eval(x) * _r.Eval(x),
            '/' => _l.Eval(x) / _r.Eval(x),
            '^' => Math.Pow(_l.Eval(x), _r.Eval(x)),
            _ => 0,
        };
    }
    private sealed class FuncNode : Node
    {
        private readonly string _name; private readonly Node _arg;
        public FuncNode(string name, Node arg) { _name = name; _arg = arg; }
        public override double Eval(double x)
        {
            double a = _arg.Eval(x);
            return _name switch
            {
                "sin" => Math.Sin(a), "cos" => Math.Cos(a), "tan" => Math.Tan(a),
                "asin" => Math.Asin(a), "acos" => Math.Acos(a), "atan" => Math.Atan(a),
                "sqrt" => Math.Sqrt(a), "abs" => Math.Abs(a),
                "ln" => Math.Log(a), "log" => Math.Log10(a), "log2" => Math.Log(a, 2),
                "exp" => Math.Exp(a),
                "sinh" => Math.Sinh(a), "cosh" => Math.Cosh(a), "tanh" => Math.Tanh(a),
                _ => throw new FormatException($"未知函数：'{_name}'"),
            };
        }
    }
}
