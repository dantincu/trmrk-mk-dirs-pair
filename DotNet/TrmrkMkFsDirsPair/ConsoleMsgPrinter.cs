using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// Custom action delegate for the console message printer component to be executed while printing the message.
    /// </summary>
    /// <param name="msg">The input matrix object.</param>
    /// <param name="lineIdx">The current row index.</param>
    /// <param name="chunkIdx">The current chunk index</param>
    /// <returns>A boolean value for the algorithm to decide whether to continue its execution or stop.</returns>
    public delegate bool? ConsoleMsgPrinterAction(object[][] msg, int lineIdx, int chunkIdx);

    /// <summary>
    /// Stores values for expressions to be parsed from each console message string row.
    /// </summary>
    public class ConsoleMsgPrinterExprValues
    {
        /// <summary>
        /// Gets the Null expression value;
        /// </summary>
        public string Null { get; init; }

        /// <summary>
        /// Gets the NewLine expression value;
        /// </summary>
        public string NewLine { get; init; }

        /// <summary>
        /// The string used as splitter of expression parts.
        /// </summary>
        public string Splitter { get; init; }

        /// <summary>
        /// Gets the Black color expression value;
        /// </summary>
        public string Black { get; init; }

        /// <summary>
        /// Gets the DarkBlue color expression value;
        /// </summary>
        public string DarkBlue { get; init; }

        /// <summary>
        /// Gets the DarkGreen color expression value;
        /// </summary>
        public string DarkGreen { get; init; }

        /// <summary>
        /// Gets the DarkCyan color expression value;
        /// </summary>
        public string DarkCyan { get; init; }

        /// <summary>
        /// Gets the DarkRed color expression value;
        /// </summary>
        public string DarkRed { get; init; }

        /// <summary>
        /// Gets the DarkMagenta color expression value;
        /// </summary>
        public string DarkMagenta { get; init; }

        /// <summary>
        /// Gets the DarkYellow color expression value;
        /// </summary>
        public string DarkYellow { get; init; }

        /// <summary>
        /// Gets the Gray color expression value;
        /// </summary>
        public string Gray { get; init; }

        /// <summary>
        /// Gets the DarkGray color expression value;
        /// </summary>
        public string DarkGray { get; init; }

        /// <summary>
        /// Gets the Blue color expression value;
        /// </summary>
        public string Blue { get; init; }

        /// <summary>
        /// Gets the Green color expression value;
        /// </summary>
        public string Green { get; init; }

        /// <summary>
        /// Gets the Cyan color expression value;
        /// </summary>
        public string Cyan { get; init; }

        /// <summary>
        /// Gets the Red color expression value;
        /// </summary>
        public string Red { get; init; }

        /// <summary>
        /// Gets the Magenta color expression value;
        /// </summary>
        public string Magenta { get; init; }

        /// <summary>
        /// Gets the Yellow color expression value;
        /// </summary>
        public string Yellow { get; init; }

        /// <summary>
        /// Gets the White color expression value;
        /// </summary>
        public string White { get; init; }
    }

    /// <summary>
    /// The interface implemented by the console message printer.
    /// </summary>
    public interface IConsoleMsgPrinter
    {
        /// <summary>
        /// Returns the default expression values for console message printing.
        /// </summary>
        /// <param name="useNames">Nullable (3) state boolean value indicating what type of values to
        /// assign to expressions. If set to <c>true</c>, it provides full enum names for console colors
        /// and full name for new line expression. If set to <c>false</c>, it provides enum number values for
        /// console colors and short name for new line expression. If set to <c>null</c>, it provides short names
        /// for console colors and new line expression.</param>
        /// <returns>An object containing the default expression values for console message printing.</returns>
        ConsoleMsgPrinterExprValues GetDefaultExpressionValues(
            bool? useNames = null);

        /// <summary>
        /// Parses the console color from the provided expression value.
        /// </summary>
        /// <param name="exprValue">The input expression value.</param>
        /// <param name="exprValuesMap">The expression values object.</param>
        /// <returns>An object containing the expression values.</returns>
        ConsoleColor? ParseColor(
            string exprValue,
            ConsoleMsgPrinterExprValues exprValuesMap);

        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="rowsArr">The input rows.</param>
        /// <param name="expressionTextParserOpts">An object containing the options for expression tedxt parser.</param>
        /// <param name="useNamesForExprValues">Nullable (3) state boolean value indicating what type of values to
        /// assign to expressions. Used by method <see cref="GetDefaultExpressionValues" />.</param>
        /// <param name="resetConsoleAction">An optional callback that will be called instead of <see cref="Console.ResetColor" />
        /// method when the current input argument item (row cell) is a zero-length array of type
        /// <see cref="Nullable{ConsoleColor}" /> of <see cref="ConsoleColor" />.</param>
        void Print(
            string[] rowsArr,
            ExpressionTextParserOpts expressionTextParserOpts,
            bool? useNamesForExprValues,
            ConsoleMsgPrinterAction resetConsoleAction = null);

        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="rowsArr">The input rows.</param>
        /// <param name="exprValues">The expression values.</param>
        /// <param name="expressionTextParserOpts">An object containing the options for expression tedxt parser.</param>
        /// <param name="resetConsoleAction">An optional callback that will be called instead of <see cref="Console.ResetColor" />
        /// method when the current input argument item (row cell) is a zero-length array of type
        /// <see cref="Nullable{ConsoleColor}" /> of <see cref="ConsoleColor" />.</param>
        void Print(
            string[] rowsArr,
            ExpressionTextParserOpts expressionTextParserOpts,
            ConsoleMsgPrinterExprValues exprValues = null,
            ConsoleMsgPrinterAction resetConsoleAction = null);

        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="linesArr">The lines to be printed.</param>
        /// <param name="expressionTextParserOpts">An object containing the options for expression tedxt parser.</param>
        /// <param name="resetConsoleAction">An optional callback that will be called instead of <see cref="Console.ResetColor" />
        /// method when the current input argument item (row cell) is a zero-length array of type
        /// <see cref="Nullable{ConsoleColor}" /> of <see cref="ConsoleColor" />.</param>
        void Print(
            string[] linesArr,
            ExpressionTextParserOpts expressionTextParserOpts = null,
            ConsoleMsgPrinterAction resetConsoleAction = null);

        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="msg">The input argument containing rows of arrays of objects. It largely matches
        /// the text words to be printed, in that the rows correspond to new lines and the column array items represent
        /// words separated by the space character, although both of these conventions can be cancelled given the appropriate
        /// array item data type.</param>
        /// <param name="resetConsoleAction">An optional callback that will be called instead of <see cref="Console.ResetColor" />
        /// method when the current input argument item (row cell) is a zero-length array of type
        /// <see cref="Nullable{ConsoleColor}" /> of <see cref="ConsoleColor" />.</param>
        void Print(object[][] msg,
            ConsoleMsgPrinterAction resetConsoleAction = null);
    }

    /// <summary>
    /// Helper component for printing colored messages to command prompt.
    /// </summary>
    public class ConsoleMsgPrinter : IConsoleMsgPrinter
    {
        /// <summary>
        /// A dictionary that stores associations between each console color and their name.
        /// </summary>
        public static readonly ReadOnlyDictionary<ConsoleColor, string> ConsoleColorsMap;

        /// <summary>
        /// A dictionary that stores associations between each of type <see cref="ConsoleMsgPrinterExprValues"/>'s
        /// property and their name.
        /// </summary>
        public static readonly ReadOnlyDictionary<string, PropertyInfo> ExprValuesPropsMap;

        /// <summary>
        /// The expression text parser.
        /// </summary>
        private readonly IExpressionTextParser expressionTextParser;

        /// <summary>
        /// Static constructor where static members are initialized.
        /// </summary>
        static ConsoleMsgPrinter()
        {
            ConsoleColorsMap = EnumsH.GetValues<ConsoleColor>().ToDictionary(
                value => value, value => value.ToString()).RdnlD();

            ExprValuesPropsMap = typeof(ConsoleMsgPrinterExprValues).GetProperties().ToDictionary(
                propInfo => propInfo.Name, propInfo => propInfo).RdnlD();
        }

        /// <summary>
        /// The component's single constructor.
        /// </summary>
        /// <param name="expressionTextParser">The expression text parser.</param>
        /// <exception cref="ArgumentNullException">Gets thrown when
        /// either of the dependencies passed in is <c>null</c>.</exception>
        public ConsoleMsgPrinter(
            IExpressionTextParser expressionTextParser)
        {
            this.expressionTextParser = expressionTextParser ?? throw new ArgumentNullException(
                nameof(expressionTextParser));
        }

        /// <summary>
        /// Returns the default expression values for console message printing.
        /// </summary>
        /// <param name="useNames">Nullable (3) state boolean value indicating what type of values to
        /// assign to expressions. If set to <c>true</c>, it provides full enum names for console colors
        /// and full name for new line expression. If set to <c>false</c>, it provides enum number values for
        /// console colors and short name for new line expression. If set to <c>null</c>, it provides short names
        /// for console colors and new line expression.</param>
        /// <returns>An object containing the default expression values for console message printing.</returns>
        public ConsoleMsgPrinterExprValues GetDefaultExpressionValues(
            bool? useNames = null) => useNames switch
            {
                true => new ConsoleMsgPrinterExprValues
                {
                    NewLine = nameof(Environment.NewLine),
                    Splitter = "-",
                    Black = nameof(ConsoleColor.Black),
                    DarkBlue = nameof(ConsoleColor.DarkBlue),
                    DarkGreen = nameof(ConsoleColor.DarkGreen),
                    DarkCyan = nameof(ConsoleColor.DarkCyan),
                    DarkRed = nameof(ConsoleColor.DarkRed),
                    DarkMagenta = nameof(ConsoleColor.DarkMagenta),
                    DarkYellow = nameof(ConsoleColor.DarkYellow),
                    Gray = nameof(ConsoleColor.Gray),
                    DarkGray = nameof(ConsoleColor.DarkGray),
                    Blue = nameof(ConsoleColor.Blue),
                    Green = nameof(ConsoleColor.Green),
                    Cyan = nameof(ConsoleColor.Cyan),
                    Red = nameof(ConsoleColor.Red),
                    Magenta = nameof(ConsoleColor.Magenta),
                    Yellow = nameof(ConsoleColor.Yellow)
                },
                false => new ConsoleMsgPrinterExprValues
                {
                    NewLine = "NL",
                    Splitter = "-",
                    Black = ((int)ConsoleColor.Black).ToString(),
                    DarkBlue = ((int)ConsoleColor.DarkBlue).ToString(),
                    DarkGreen = ((int)ConsoleColor.DarkGreen).ToString(),
                    DarkCyan = ((int)ConsoleColor.DarkCyan).ToString(),
                    DarkRed = ((int)ConsoleColor.DarkRed).ToString(),
                    DarkMagenta = ((int)ConsoleColor.DarkMagenta).ToString(),
                    DarkYellow = ((int)ConsoleColor.DarkYellow).ToString(),
                    Gray = ((int)ConsoleColor.Gray).ToString(),
                    DarkGray = ((int)ConsoleColor.DarkGray).ToString(),
                    Blue = ((int)ConsoleColor.Blue).ToString(),
                    Green = ((int)ConsoleColor.Green).ToString(),
                    Cyan = ((int)ConsoleColor.Cyan).ToString(),
                    Red = ((int)ConsoleColor.Red).ToString(),
                    Magenta = ((int)ConsoleColor.Magenta).ToString(),
                    Yellow = ((int)ConsoleColor.Yellow).ToString()
                },
                _ => new ConsoleMsgPrinterExprValues
                {
                    NewLine = "NL",
                    Splitter = "-",
                    Black = "0",
                    DarkBlue = "0bu",
                    DarkGreen = "0ge",
                    DarkCyan = "0cy",
                    DarkRed = "0re",
                    DarkMagenta = "0ma",
                    DarkYellow = "0ye",
                    Gray = "1ga",
                    DarkGray = "0ga",
                    Blue = "1bu",
                    Green = "1ge",
                    Cyan = "1cy",
                    Red = "1re",
                    Magenta = "1ma",
                    Yellow = "1ye",
                    White = "1",
                }
            };

        /// <summary>
        /// Parses the console color from the provided expression value.
        /// </summary>
        /// <param name="exprValue">The input expression value.</param>
        /// <param name="exprValuesMap">The expression values object.</param>
        /// <returns>An object containing the expression values.</returns>
        public ConsoleColor? ParseColor(
            string exprValue,
            ConsoleMsgPrinterExprValues exprValuesMap)
        {
            ConsoleColor? color = null;

            KeyValuePair<string, PropertyInfo> match = default;

            foreach (var kvp in ExprValuesPropsMap)
            {
                var value = kvp.Value.GetValue(exprValuesMap) as string;

                if (value == exprValue)
                {
                    match = kvp;
                    break;
                }
            }

            if (match.Key != null && match.Key != nameof(ConsoleMsgPrinterExprValues.NewLine))
            {
                color = ConsoleColorsMap.Single(
                    kvp => kvp.Value == match.Key).Key;
            }

            return color;
        }

        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="rowsArr">The input rows.</param>
        /// <param name="exprValues">The expression values.</param>
        /// <param name="useNamesForExprValues">Nullable (3) state boolean value indicating what type of values to
        /// assign to expressions. Used by method <see cref="GetDefaultExpressionValues" />.</param>
        /// <param name="resetConsoleAction">An optional callback that will be called instead of <see cref="Console.ResetColor" />
        /// method when the current input argument item (row cell) is a zero-length array of type
        /// <see cref="Nullable{ConsoleColor}" /> of <see cref="ConsoleColor" />.</param>
        public void Print(
            string[] rowsArr,
            ExpressionTextParserOpts expressionTextParserOpts,
            bool? useNamesForExprValues,
            ConsoleMsgPrinterAction resetConsoleAction = null)
        {
            var exprValues = GetDefaultExpressionValues(useNamesForExprValues);
            Print(rowsArr, expressionTextParserOpts, exprValues, resetConsoleAction);
        }

        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="rowsArr">The input rows.</param>
        /// <param name="exprValues">The expression values.</param>
        /// <param name="expressionTextParserOpts">An object containing the options for expression tedxt parser.</param>
        /// <param name="resetConsoleAction">An optional callback that will be called instead of <see cref="Console.ResetColor" />
        /// method when the current input argument item (row cell) is a zero-length array of type
        /// <see cref="Nullable{ConsoleColor}" /> of <see cref="ConsoleColor" />.</param>
        public void Print(
            string[] rowsArr,
            ExpressionTextParserOpts expressionTextParserOpts,
            ConsoleMsgPrinterExprValues exprValues = null,
            ConsoleMsgPrinterAction resetConsoleAction = null)
        {
            exprValues ??= GetDefaultExpressionValues();

            expressionTextParserOpts = new ExpressionTextParserOpts(
                expressionTextParserOpts ?? new ExpressionTextParserOpts())
            {
                ExpressionParser = wka =>
                {
                    var exprStr = wka.TextExtractor(wka);
                    object? retObj;

                    if (exprStr == exprValues.NewLine)
                    {
                        retObj = 1;
                    }
                    else if (exprStr == exprValues.Null)
                    {
                        retObj = null;
                    }
                    else
                    {
                        var exprParts = exprStr.Split(
                            exprValues.Splitter);

                        if (exprParts.Length == 1)
                        {
                            retObj = ParseColor(
                                exprParts.Single(),
                                exprValues);
                        }
                        else
                        {
                            var retArr = exprParts.Select(
                                part => ParseColor(
                                    part,
                                    exprValues)).ToArray();

                            if (retArr.All(obj => obj == null))
                            {
                                retArr = [];
                            }

                            retObj = retArr;
                        }
                    }

                    return retObj;
                }
            };

            Print(rowsArr, expressionTextParserOpts, resetConsoleAction);
        }

        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="rowsArr">The input rows.</param>
        /// <param name="expressionTextParserOpts">An object containing the options for expression tedxt parser.</param>
        /// <param name="resetConsoleAction">An optional callback that will be called instead of <see cref="Console.ResetColor" />
        /// method when the current input argument item (row cell) is a zero-length array of type
        /// <see cref="Nullable{ConsoleColor}" /> of <see cref="ConsoleColor" />.</param>
        public void Print(
            string[] rowsArr,
            ExpressionTextParserOpts expressionTextParserOpts = null,
            ConsoleMsgPrinterAction resetConsoleAction = null)
        {
            expressionTextParserOpts.ExpressionObjectAppender = expressionTextParserOpts.ExpressionObjectAppender.FirstNotNull(
                (wka, obj) =>
                {
                    if (obj is object[] arr)
                    {
                        wka.OutputList.AddRange(arr);
                    }
                    else
                    {
                        wka.OutputList.Add(obj);
                    }
                });

            expressionTextParserOpts = expressionTextParser.NormalizeOpts(
                expressionTextParserOpts);

            object[][] msgMx = rowsArr.Select(
                line => expressionTextParser.Parse(new ExpressionTextParserOpts(
                    expressionTextParserOpts)
                    {
                        InputStr = line,
                    }, false)).ToArray();

            Print(msgMx, resetConsoleAction);
        }

        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="msg">The input argument containing rows of arrays of objects. It largely matches
        /// the text words to be printed, in that the rows correspond to new lines and the column array items represent
        /// words separated by the space character, although both of these conventions can be cancelled given the appropriate
        /// array item data type.</param>
        /// <param name="resetConsoleAction">An optional callback that will be called instead of <see cref="Console.ResetColor" />
        /// method when the current input argument item (row cell) is a zero-length array of type
        /// <see cref="Nullable{ConsoleColor}" /> of <see cref="ConsoleColor" />.</param>
        public void Print(
            object[][] msg,
            ConsoleMsgPrinterAction resetConsoleAction = null)
        {
            int rowIdx = 0;
            int chunkIdx = 0;
            bool stop = false;
            object? prevChunk = null;

            resetConsoleAction = resetConsoleAction.FirstNotNull(
                (msg, rwIdx, chIdx) =>
                {
                    Console.ResetColor();
                    return null;
                });

            foreach (var line in msg)
            {
                if (stop)
                {
                    break;
                }
                else if (rowIdx > 0)
                {
                    if (prevChunk is not int)
                    {
                        Console.WriteLine();
                    }
                }

                foreach (var chunk in line)
                {
                    if (chunk != null)
                    {
                        if (chunkIdx > 0)
                        {
                            if (chunk is string && prevChunk is string)
                            {
                                Console.Write(" ");
                            }
                        }

                        if (chunk is string strChunk)
                        {
                            Console.Write(strChunk);
                        }
                        else if (chunk is ConsoleColor color)
                        {
                            Console.ForegroundColor = color;
                        }
                        else if (chunk is ConsoleColor?[] colorsArr)
                        {
                            if (colorsArr.Length == 0)
                            {
                                resetConsoleAction(msg, rowIdx, chunkIdx);
                            }
                            else
                            {
                                var bgColorVal = colorsArr[0];

                                if (bgColorVal.HasValue)
                                {
                                    Console.BackgroundColor = bgColorVal.Value;
                                }

                                if (colorsArr.Length == 2)
                                {
                                    var fgColorVal = colorsArr[1];

                                    if (fgColorVal.HasValue)
                                    {
                                        Console.ForegroundColor = fgColorVal.Value;
                                    }
                                }
                                else if (colorsArr.Length > 2)
                                {
                                    throw new ArgumentException(
                                        nameof(colorsArr));
                                }
                            }
                        }
                        else if (chunk is int nlCount)
                        {
                            for (int i = 0; i < nlCount; i++)
                            {
                                Console.WriteLine();
                            }
                        }
                        else if (chunk is KeyValuePair<int, string> chunkKvp && chunkKvp.Key > 0)
                        {
                            string str = string.Concat(
                                Enumerable.Range(0, chunkKvp.Key).Select(
                                    i => chunkKvp.Value));

                            Console.Write(str);
                        }
                        else if (chunk is ConsoleMsgPrinterAction action)
                        {
                            bool? answer = action(msg, rowIdx, chunkIdx);

                            if (answer.HasValue)
                            {
                                if (!answer.Value)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                stop = true;
                            }
                        }
                    }

                    prevChunk = chunk;
                    chunkIdx++;
                }

                rowIdx++;
                chunkIdx = 0;
            }
        }
    }
}
