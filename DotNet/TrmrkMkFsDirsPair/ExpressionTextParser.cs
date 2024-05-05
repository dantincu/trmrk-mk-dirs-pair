using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// The state of the parsing algorithm.
    /// </summary>
    public enum ExpressionTextParserState
    {
        /// <summary>
        /// The default state where the <see cref="ExpressionTextParserOpts.ExpressionStartChr" />
        /// and <see cref="ExpressionTextParserOpts.EscapeChr" /> are interpreted using their
        /// usual meaning as part of the formatted input text.
        /// </summary>
        Default = 0,

        /// <summary>
        /// The state of being inside an expression as part of the formatted input text.
        /// </summary>
        InsideExpression = 1,

        /// <summary>
        /// The state of being inside a free text section as part of the formatted input text.
        /// </summary>
        InsideFreeTextSection = 2
    }

    public interface IExpressionTextParser
    {
        /// <summary>
        /// Parses the input text according to the options passed in.
        /// </summary>
        /// <param name="opts">An object containing the parsing options.</param>
        /// <param name="normalizeOpts">A flag parameter indicating whether the provided options
        /// should be normalized before running the algorithm.</param>
        /// <returns>An array of containing the objects that have been parsed from the input text.</returns>
        object[] Parse(
            ExpressionTextParserOpts opts,
            bool normalizeOpts = true);

        /// <summary>
        /// Normalizes the input options by assigning default values to properties whose values are <c>null</c>.
        /// </summary>
        /// <param name="opts">An object containing the parsing options.</param>
        /// <returns>The same object that was passed in, now having all its properties set to values
        /// other than <c>null</c>.</returns>
        ExpressionTextParserOpts NormalizeOpts(
            ExpressionTextParserOpts opts);
    }

    /// <summary>
    /// DTO for storing options for the <see cref="ExpressionTextParser" /> component.
    /// </summary>
    public class ExpressionTextParserOpts
    {
        /// <summary>
        /// Parameterless constructor for creating object from scratch.
        /// </summary>
        public ExpressionTextParserOpts()
        {
        }

        /// <summary>
        /// Cloning constructor that copies all properties from the source to this instance.
        /// </summary>
        /// <param name="src">The source object.</param>
        public ExpressionTextParserOpts(
            ExpressionTextParserOpts src)
        {
            InputStr = src.InputStr;
            ExpressionStartChr = src.ExpressionStartChr;
            ExpressionEndChr = src.ExpressionEndChr;
            EscapeChr = src.EscapeChr;
            FreeTextStartChr = src.FreeTextStartChr;
            FreeTextEndChr = src.FreeTextEndChr;
            ExpressionParser = src.ExpressionParser;
            ExpressionObjectAppender = src.ExpressionObjectAppender;
            TextExtractor = src.TextExtractor;
            InsideExpressionCallback = src.InsideExpressionCallback;
            AfterParseCallback = src.AfterParseCallback;
        }

        /// <summary>
        /// Gets or sets the formatted input text to be parsed.
        /// </summary>
        public string InputStr { get; set; }

        /// <summary>
        /// Gets or sets the char marking the start of an expression.
        /// </summary>
        public char ExpressionStartChr { get; set; }

        /// <summary>
        /// Gets or sets the char marking the end of an expression.
        /// </summary>
        public char ExpressionEndChr { get; set; }

        /// <summary>
        /// Gets or sets the escape char for <see cref="ExpressionStartChr" />.
        /// </summary>
        public char EscapeChr { get; set; }

        /// <summary>
        /// Gets or sets the char that, when following the the <see cref="EscapeChr" />, all occurrences of
        /// either <see cref="ExpressionStartChr" />, <see cref="ExpressionEndChr" /> or <see cref="EscapeChr"/>
        /// are interpreted as regular text until the first sequence consisting of the
        /// <see cref="FreeTextEndChr" /> followed by the <see cref="EscapeChr" /> is found.
        /// </summary>
        public char FreeTextStartChr { get; set; }

        /// <summary>
        /// Gets or sets the char that, when followed by the <see cref="EscapeChr" />, the free text section
        /// ends and from then on all occurrences of
        /// <see cref="ExpressionStartChr" />, <see cref="ExpressionEndChr" /> and <see cref="EscapeChr"/>
        /// are interpreted using their usual meaning as part of the formatted text.
        /// </summary>
        public char FreeTextEndChr { get; set; }

        /// <summary>
        /// Gets or sets the expression parser that will be called when the algorithm 
        /// changes its state while currently being in that of
        /// <see cref="ExpressionTextParserState.InsideExpression"/>.
        /// </summary>
        public Func<ExpressionTextParserWorkArgs, object> ExpressionParser { get; set; }

        /// <summary>
        /// Gets or sets an optional callback to be called instead of the default way the parsed
        /// object is added to the output list.
        /// </summary>
        public Action<ExpressionTextParserWorkArgs, object> ExpressionObjectAppender { get; set; }

        /// <summary>
        /// Gets or sets the text extractor that will be called when the algorithm changes its state
        /// while currently being in that of either <see cref="ExpressionTextParserState.Default" /> or
        /// <see cref="ExpressionTextParserState.InsideFreeTextSection" />.
        /// </summary>
        public Func<ExpressionTextParserWorkArgs, string> TextExtractor { get; set; }

        /// <summary>
        /// Gets or sets a callback to be called instead of the default way the algorithm interprets characters
        /// inside an expression, which is just to look for the closing sequence. This is usefull
        /// for creating more complicated expression parsers, that is expressions that do not end
        /// upon encountering the default closing sequence.
        /// </summary>
        public Action<ExpressionTextParserWorkArgs> InsideExpressionCallback { get; set; }

        /// <summary>
        /// Gets or sets an optional callback to be called after the parsing algorithm has finished its execution.
        /// </summary>
        public Action<ExpressionTextParserWorkArgs> AfterParseCallback { get; set; }
    }

    /// <summary>
    /// Class used internally by the <see cref="ExpressionTextParser" /> component's algorithm execution.
    /// </summary>
    public class ExpressionTextParserWorkArgs
    {
        /// <summary>
        /// Gets or sets the normalized options passed in by the dependent component.
        /// </summary>
        public ExpressionTextParserOpts Opts { get; set; }

        /// <summary>
        /// Gets or sets the text extractor that will be called when the algorithm changes its state
        /// while currently being in that of either <see cref="ExpressionTextParserState.Default" /> or
        /// <see cref="ExpressionTextParserState.InsideFreeTextSection" />.
        /// </summary>
        public Func<ExpressionTextParserWorkArgs, string> TextExtractor { get; set; }

        /// <summary>
        /// Gets or sets an optional callback to be called instead of the default way the parsed
        /// object is added to the output list.
        /// </summary>
        public Action<ExpressionTextParserWorkArgs, object> ExpressionObjectAppender { get; set; }

        /// <summary>
        /// Gets or sets the formatted input text to be parsed.
        /// </summary>
        public string InputStr { get; set; }

        /// <summary>
        /// Gets or sets the current state of the algorithm's execution.
        /// </summary>
        public ExpressionTextParserState State { get; set; }

        /// <summary>
        /// Gets or sets the list containing the parsed objects returned to the dependent component.
        /// </summary>
        public List<object> OutputList { get; set; }

        /// <summary>
        /// The maximum value that <see cref="Idx" /> can have during the execution of the algorithm.
        /// </summary>
        public int MaxIdx { get; set; }

        /// <summary>
        /// Gets or sets the index of the current character that is being interpreted from the input text.
        /// </summary>
        public int Idx { get; set; }

        /// <summary>
        /// Gets or sets the current character that is being interpreted from the input text.
        /// </summary>
        public char Char { get; set; }

        /// <summary>
        /// Gets or sets the previous character that was interpreted from the input text.
        /// </summary>
        public char PrevChar { get; set; }

        /// <summary>
        /// Gets or sets the next character that will be interpreted from the input text.
        /// </summary>
        public char NextChar { get; set; }

        /// <summary>
        /// Gets or sets a list containing the characters digested from the input text since the
        /// last time the state of the algorithm has changed.
        /// </summary>
        public List<char> CurrentChars { get; set; }

        /// <summary>
        /// The remaining unparsed string in case of invalid expression closing or free text closing synthax.
        /// </summary>
        public string RemainingInvalidStr { get; set; }
    }

    /// <summary>
    /// Component that parses the formatted text for the use of <see cref="ConsoleMsgPrinter" /> component.
    /// </summary>
    public class ExpressionTextParser : IExpressionTextParser
    {
        /// <summary>
        /// The default value for <see cref="ExpressionTextParserOpts.ExpressionStartChr" />.
        /// </summary>
        public const char EXPRESSION_START_CHR = '{';

        /// <summary>
        /// The default value for <see cref="ExpressionTextParserOpts.ExpressionEndChr" />.
        /// </summary>
        public const char EXPRESSION_END_CHR = '}';

        /// <summary>
        /// The default value for <see cref="ExpressionTextParserOpts.EscapeChr" />.
        /// </summary>
        public const char ESCAPE_CHR = '\\';

        /// <summary>
        /// The default value for <see cref="ExpressionTextParserOpts.FreeTextStartChr" />.
        /// </summary>
        public const char FREE_TEXT_START_CHR = '@';

        /// <summary>
        /// The default value for <see cref="ExpressionTextParserOpts.FreeTextEndChr" />.
        /// </summary>
        public const char FREE_TEXT_END_CHR = '@';

        /// <summary>
        /// Parses the input text according to the options passed in.
        /// </summary>
        /// <param name="opts">An object containing the parsing options.</param>
        /// <param name="normalizeOpts">A flag parameter indicating whether the provided options
        /// should be normalized before running the algorithm.</param>
        /// <returns>An array of containing the objects that have been parsed from the input text.</returns>
        public object[] Parse(
            ExpressionTextParserOpts opts,
            bool normalizeOpts = true)
        {
            if (normalizeOpts)
            {
                opts = NormalizeOpts(opts);
            }

            if (string.IsNullOrWhiteSpace(opts.InputStr))
            {
                throw new ArgumentException(
                    opts.InputStr);
            }

            var wka = new ExpressionTextParserWorkArgs
            {
                Opts = opts,
                TextExtractor = opts.TextExtractor,
                ExpressionObjectAppender = opts.ExpressionObjectAppender,
                InputStr = opts.InputStr,
                State = ExpressionTextParserState.Default,
                MaxIdx = opts.InputStr.Length - 1,
                Idx = -1,
                OutputList = new List<object>(),
                CurrentChars = new List<char>()
            };

            IncrementIdx(wka);
            Parse(wka);

            opts.AfterParseCallback?.Invoke(wka);
            return wka.OutputList.ToArray();
        }

        /// <summary>
        /// Normalizes the input options by assigning default values to properties whose values are <c>null</c>.
        /// </summary>
        /// <param name="opts">An object containing the parsing options.</param>
        /// <returns>The same object that was passed in, now having all its properties set to values
        /// other than <c>null</c>.</returns>
        public ExpressionTextParserOpts NormalizeOpts(
            ExpressionTextParserOpts opts)
        {
            opts.ExpressionStartChr = opts.ExpressionStartChr.FirstNotDefault(
                [EXPRESSION_START_CHR]);

            opts.ExpressionEndChr = opts.ExpressionEndChr.FirstNotDefault(
                [EXPRESSION_END_CHR]);

            opts.EscapeChr = opts.EscapeChr.FirstNotDefault(
                [ESCAPE_CHR]);

            opts.FreeTextStartChr = opts.FreeTextStartChr.FirstNotDefault(
                [FREE_TEXT_START_CHR]);

            opts.FreeTextEndChr = opts.FreeTextEndChr.FirstNotDefault(
                [FREE_TEXT_END_CHR]);

            opts.ExpressionParser = opts.ExpressionParser.FirstNotNull(
                wka => wka.Opts.TextExtractor(wka));

            opts.TextExtractor = opts.TextExtractor.FirstNotNull(
                wka => new string(wka.CurrentChars.ToArray()));

            opts.ExpressionObjectAppender = opts.ExpressionObjectAppender.FirstNotNull(
                (wka, obj) => wka.OutputList.Add(obj));

            opts.InsideExpressionCallback = opts.InsideExpressionCallback.FirstNotNull(
                wka =>
                {
                    if (wka.Char == opts.ExpressionEndChr)
                    {
                        OnNewState(wka, ExpressionTextParserState.Default, false);
                        IncrementIdx(wka, true);
                    }
                    else
                    {
                        AddCurrentChar(wka);
                        IncrementIdx(wka);
                    }
                });

            return opts;
        }

        /// <summary>
        /// The core method that implements the parsing algorithm.
        /// </summary>
        /// <param name="wka">The work args containing all the necessary data for the algorighm to work propperly.</param>
        private void Parse(
            ExpressionTextParserWorkArgs wka)
        {
            var opts = wka.Opts;
            var inputStr = opts.InputStr;
            var output = wka.OutputList;
            int maxIdx = inputStr.Length - 1;

            while (wka.Idx <= maxIdx)
            {
                switch (wka.State)
                {
                    case ExpressionTextParserState.Default:
                        if (wka.Char == opts.ExpressionStartChr)
                        {
                            if (wka.NextChar == opts.ExpressionStartChr)
                            {
                                AddCurrentChar(wka);
                                IncrementIdx(wka);
                                IncrementIdx(wka, true);
                            }
                            else
                            {
                                OnNewState(wka, ExpressionTextParserState.InsideExpression);
                                IncrementIdx(wka, true);
                            }
                        }
                        else if (wka.Char == opts.ExpressionEndChr)
                        {
                            if (wka.NextChar == opts.ExpressionEndChr)
                            {
                                AddCurrentChar(wka);
                                IncrementIdx(wka);
                                IncrementIdx(wka, true);
                            }
                            else
                            {
                                AddCurrentChar(wka);
                                IncrementIdx(wka);
                            }
                        }
                        else if (wka.Char == opts.FreeTextStartChr)
                        {
                            if (wka.PrevChar == opts.EscapeChr)
                            {
                                OnNewState(wka, ExpressionTextParserState.InsideFreeTextSection);
                                IncrementIdx(wka, true);
                            }
                            else
                            {
                                AddCurrentChar(wka);
                                IncrementIdx(wka);
                            }
                        }
                        else if (wka.Char == opts.EscapeChr)
                        {
                            if (wka.PrevChar == opts.EscapeChr)
                            {
                                IncrementIdx(wka, true);
                            }
                            else if (wka.NextChar != opts.FreeTextStartChr)
                            {
                                AddCurrentChar(wka);
                                IncrementIdx(wka);
                            }
                            else
                            {
                                IncrementIdx(wka);
                            }
                        }
                        else
                        {
                            AddCurrentChar(wka);
                            IncrementIdx(wka);
                        }
                        break;
                    case ExpressionTextParserState.InsideExpression:
                        wka.Opts.InsideExpressionCallback(wka);
                        break;
                    case ExpressionTextParserState.InsideFreeTextSection:
                        if (wka.Char == opts.FreeTextStartChr)
                        {
                            if (wka.NextChar == opts.EscapeChr)
                            {
                                OnNewState(wka, ExpressionTextParserState.Default);
                                IncrementIdx(wka);
                                IncrementIdx(wka, true);
                            }
                            else
                            {
                                AddCurrentChar(wka);
                                IncrementIdx(wka);
                            }
                        }
                        else
                        {
                            AddCurrentChar(wka);
                            IncrementIdx(wka);
                        }
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Invalid expression text parser state: {wka.State}");
                }
            }

            if (wka.State == ExpressionTextParserState.Default)
            {
                OnNewState(wka, ExpressionTextParserState.Default);
            }
            else
            {
                wka.RemainingInvalidStr = wka.TextExtractor(wka);
            }
        }

        /// <summary>
        /// Increments the index and updates the value of the current char being interpretted by the algorithm,
        /// along with the values for the previous and next char.
        /// </summary>
        /// <param name="wka">The work args containing all the necessary data for the algorighm to work propperly.</param>
        /// <param name="setPrevCharToDefault">Flag parameter indicating whether to set the previous char to
        /// the empty character or to the previous value of the current character.</param>
        private void IncrementIdx(
            ExpressionTextParserWorkArgs wka,
            bool setPrevCharToDefault = false)
        {
            if (setPrevCharToDefault)
            {
                wka.PrevChar = default;
            }
            else
            {
                wka.PrevChar = wka.Char;
            }

            wka.Idx++;

            if (wka.Idx <= wka.MaxIdx)
            {
                wka.Char = wka.Opts.InputStr[wka.Idx];

                if (wka.Idx < wka.MaxIdx)
                {
                    wka.NextChar = wka.InputStr[wka.Idx + 1];
                }
                else
                {
                    wka.NextChar = default;
                }
            }
            else
            {
                wka.Char = default;
            }
        }


        /// <summary>
        /// Called when the executing algorithm has its state changed.
        /// </summary>
        /// <param name="wka">The work args containing all the necessary data for the algorighm to work propperly.</param>
        /// <param name="newState">The new state of the algorithm to switch to from the current state.</param>
        /// <param name="wasFreeText">Flag parameter indicating whether the text that has just been digested
        /// was free or regular text, or an expression to pe interpretted.</param>
        /// <param name="setPrevCharToDefault">Flag parameter indicating whether to set the previous char to
        /// the empty character or to the previous value of the current character.</param>
        private void OnNewState(
            ExpressionTextParserWorkArgs wka,
            ExpressionTextParserState newState = ExpressionTextParserState.Default,
            bool wasFreeText = true,
            bool setPrevCharToDefault = true)
        {
            var objFactory = wasFreeText switch
            {
                true => wka.Opts.TextExtractor,
                false => wka.Opts.ExpressionParser,
            };

            var obj = objFactory(wka);
            wka.ExpressionObjectAppender(wka, obj);
            wka.CurrentChars.Clear();
            wka.State = newState;

            if (setPrevCharToDefault)
            {
                wka.PrevChar = default;
            }
        }

        /// <summary>
        /// Adds the provided (or current) char to the current list of chars.
        /// </summary>
        /// <param name="wka">The work args containing all the necessary data for the algorighm to work propperly.</param>
        /// <param name="chr">The character to add or null to add the current character.</param>
        private void AddCurrentChar(
            ExpressionTextParserWorkArgs wka,
            char? chr = null)
        {
            chr ??= wka.Char;

            if (chr.Value != default)
            {
                wka.CurrentChars.Add(chr.Value);
            }
        }
    }
}
