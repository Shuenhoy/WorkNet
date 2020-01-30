using System;
using System.Collections.Generic;
using LanguageExt;
using static LanguageExt.Prelude;

namespace WorkNet.Client.Submit
{
    internal static class ArgumentsGenerator
    {
        public static Seq<long> GenList(long start, long end, long step, bool eq)
        {
            var ret = new List<long>();
            if (eq)
            {
                for (; start <= end; start += step)
                {
                    ret.Add(start);
                }
                return ret.ToSeq();
            }
            else
            {
                for (; start < end; start += step)
                {
                    ret.Add(start);
                }
                return ret.ToSeq();
            }
        }
        public static Seq<double> GenList(double start, double end, double step, bool eq)
        {
            var ret = new List<double>();
            if (eq)
            {
                for (; start <= end; start += step)
                {
                    ret.Add(start);
                }
                return ret.ToSeq();
            }
            else
            {
                for (; start < end; start += step)
                {
                    ret.Add(start);
                }
                return ret.ToSeq();
            }
        }
        public static Seq<Seq<string>> GenerateArguments(List<Argument> arguments, int pos)
        {

            var arg = arguments[pos];
            if (pos == arguments.Count - 1)
            {

                return arg switch
                {
                    PlainArgument p => Seq1(Seq1(p.Arg)),
                    IterArgument i => i.Iter.Count > 0 && i.Iter.First() == "@" ?
                        i.Iter.Skip(1).Map(x => Seq1(("@" + x)))
                        : i.Iter.Map(x => Seq1(x)),
                    RangeArgument<int> r => GenList(r.Start, r.End, r.Step, r.Close).Map(x => Seq1(x.ToString())),
                    RangeArgument<float> r => GenList(r.Start, r.End, r.Step, r.Close).Map(x => Seq1(x.ToString())),
                    _ => throw new NotSupportedException()

                };
            }
            else
            {
                var next = GenerateArguments(arguments, pos + 1);
                return arg switch
                {
                    PlainArgument p => next.Map(x => x.Add(p.Arg)),
                    IterArgument i => i.Iter.Count > 0 && i.Iter.First() == "@"
                        ? next.Map(x => i.Iter.Skip(1).Map(y => x.Add("@" + y))).Flatten()
                        : next.Map(x => i.Iter.Map(y => x.Add(y))).Flatten(),
                    RangeArgument<int> r => GenList(r.Start, r.End, r.Step, r.Close).Map(x => next.Map(y => y.Add(x.ToString()))).Flatten(),
                    RangeArgument<float> r => GenList(r.Start, r.End, r.Step, r.Close).Map(x => next.Map(y => y.Add(x.ToString()))).Flatten(),
                    _ => throw new NotSupportedException()
                };
            }
        }
    }
}