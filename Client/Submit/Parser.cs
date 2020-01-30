using LanguageExt;
using LanguageExt.Parsec;

using static LanguageExt.Prelude;
using static LanguageExt.List;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Token;

using System.Collections.Generic;
using System;

namespace WorkNet.Client.Submit
{
    abstract class Argument { }
    class RangeArgument<T> : Argument
    {
        public T Start;
        public T End;
        public T Step;
        public bool Close;
    }

    class IterArgument : Argument
    {
        public Seq<string> Iter;
    }
    class PlainArgument : Argument
    {
        public string Arg;
        public override string ToString()
        {
            return Arg;
        }
    }
    enum ArgumentType
    {
        Plain,
        Argument,
        File
    }
    static class ArgumentParser
    {
        static Parser<Unit> spaces1 = skipMany1(space);

        static Parser<Argument> plainArgument =
            from value in asString(many1(
                either(noneOf("[]:\\"),
                (from _ in ch('\\') from c in oneOf("[]:\\%?") select c)
                )))
            select new PlainArgument() { Arg = value } as Argument;
        static Parser<Argument> iterArgument =
            from _0 in str("?[")
            from _01 in spaces
            from arguments in sepEndBy1(asString(many1(noneOf(" ]"))), spaces1)
            from _00 in spaces
            from _1 in ch(']')
            select new IterArgument() { Iter = arguments } as Argument;
        static Parser<int> intLit =
                   from d in asString(many1(digit))
                   select Int32.Parse(d);
        static Parser<(float lit, float step)> floatLit =
           from d in asString(many1(digit))
           from _ in ch('.')
           from c in asString(many1(digit))
           select (Single.Parse(d + '.' + c), MathF.Pow(10.0f, -1 * c.Length));
        static Parser<Argument> floatRangeArgument =
            from _0 in str("%[")
            from start in floatLit
            from _1 in str("..")
            from close in optional(ch('='))
            from end in floatLit
            from step in optional(
                from _2 in ch(',')
                from s in floatLit
                select s
            )
            from _2 in ch(']')
            select new RangeArgument<float>()
            {
                Start = start.lit,
                End = end.lit,
                Close = close.IsSome,
                Step = step.IsSome ? step.Head().lit : MathF.Min(start.step, end.step)
            } as Argument;
        static Parser<Argument> intRangeArgument =
            from _0 in str("%[")
            from start in intLit
            from _1 in str("..")
            from close in optional(ch('='))
            from end in intLit
            from step in optional(
                from _2 in ch(',')
                from s in intLit
                select s
            )
            from _2 in ch(']')
            select new RangeArgument<int>()
            {
                Start = start,
                End = end,
                Close = close.IsSome,
                Step = step.IsSome ? step.Head() : 1
            } as Argument;
        static Parser<Argument> argument =
            choice(attempt(intRangeArgument), attempt(floatRangeArgument), attempt(iterArgument), plainArgument);
        static Parser<string> key =
            from first in choice(letter, ch('_'), ch('~'))
            from left in asString(many1(choice(letter, digit, ch('_'))))
            from _ in ch(':')
            select first + left;


        static Parser<(string, Argument)> keyvalue =
            from key in optional(key)
            from arg in key.Match(x => x.StartsWith("~") ? plainArgument : argument, argument)
            select (key.IfNone(""), arg);


        public static (string key, Argument arg) ParseArguments(string input)
         => keyvalue.Parse(input).Reply.Result;


    }
}