using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <returns></returns>
    public delegate bool? ConsoleMsgPrinterAction(object[][] msg, int lineIdx, int chunkIdx);

    /// <summary>
    /// The interface implemented by the console message printer.
    /// </summary>
    public interface IConsoleMsgPrinter
    {
        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="msg">The input argument containing rows of arrays of objects. It largely matches
        /// the text words to be printed, in that the rows correspond to new lines and the column array items represent
        /// words separated by the space character, although both of these conventions can be cancelled given the appropriate
        /// array item data type.</param>
        void Print(object[][] msg);
    }

    /// <summary>
    /// Helper component for printing colored messages to command prompt.
    /// </summary>
    public class ConsoleMsgPrinter : IConsoleMsgPrinter
    {
        /// <summary>
        /// The print method that does the job of printing the message to command prompt.
        /// </summary>
        /// <param name="msg">The input argument containing rows of arrays of objects. It largely matches
        /// the text words to be printed, in that the rows correspond to new lines and the column array items represent
        /// words separated by the space character, although both of these conventions can be cancelled given the appropriate
        /// array item data type.</param>
        public void Print(object[][] msg)
        {
            int rowIdx = 0;
            int chunkIdx = 0;
            bool stop = false;
            object? prevChunk = null;

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
                                Console.ResetColor();
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
            }
        }
    }
}
