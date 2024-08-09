using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// Utility class for helper methods.
    /// </summary>
    public static class UtilsH
    {
        /// <summary>
        /// The path where the executing assembly is located. That is the path where the <c>.exe</c> file and <c>.dll</c> files,
        /// along with the <c>.json</c> config file for this program reside.
        /// </summary>
        public static readonly string ExecutingAssemmblyPath = AppDomain.CurrentDomain.BaseDirectory;

        public static ReadOnlyDictionary<TKey, TValue> RdnlD<TKey, TValue>(
            this Dictionary<TKey, TValue> dictnr) => new ReadOnlyDictionary<TKey, TValue>(dictnr);

        /// <summary>
        /// Searches the provided enumerable for and returns the first item that satisfies the condition specified by the provided
        /// predicate or a default value of no suitable item is found.
        /// </summary>
        /// <typeparam name="T">The type of items that the provided enumerable contains.</typeparam>
        /// <param name="nmrbl">The enumerable containing the items to search in.</param>
        /// <param name="predicate">The predicate specifying the condition that the suitable item must satisfy.</param>
        /// <returns>A value of type <see cref="KeyValuePair{int, T}" /> containing the index of the first item
        /// that satisfies the condition specified by the predicate and the value of the actual item, if one
        /// has been found. If no suitable item has been found, it returns the value <c>null</c> for the item and
        /// <c>-1</c> for the item index.</returns>
        public static KeyValuePair<int, T> FirstKvp<T>(
            this IEnumerable<T> nmrbl,
            Func<T, int, bool> predicate)
        {
            KeyValuePair<int, T> retKvp = new KeyValuePair<int, T>(-1, default);
            int idx = 0;

            foreach (T item in nmrbl)
            {
                if (predicate(item, idx))
                {
                    retKvp = new KeyValuePair<int, T>(idx, item);
                    break;
                }
                else
                {
                    idx++;
                }
            }

            return retKvp;
        }

        /// <summary>
        /// Creates an array of the specified type with the specified items.
        /// </summary>
        /// <typeparam name="T">The type of array items</typeparam>
        /// <param name="firstItem">The item that will sit on the first position of the array</param>
        /// <param name="nextItemsArr">The rest of items that will fill the array</param>
        /// <returns>An instance of type <see cref="{T}[]" /> containing the specified items.</returns>
        public static T[] ToArr<T>(
            this T firstItem,
            params T[] nextItemsArr) => nextItemsArr.Prepend(
                firstItem).ToArray();

        /// <summary>
        /// Searches the provided enumerable for and returns the first item that satisfies the condition specified by the provided
        /// predicate or a default value of no suitable item is found.
        /// </summary>
        /// <typeparam name="T">The type of items that the provided enumerable contains.</typeparam>
        /// <param name="nmrbl">The enumerable containing the items to search in.</param>
        /// <param name="predicate">The predicate specifying the condition that the suitable item must satisfy.</param>
        /// <returns>A value of type <see cref="KeyValuePair{int, T}" /> containing the index of the first item
        /// that satisfies the condition specified by the predicate and the value of the actual item, if one
        /// has been found. If no suitable item has been found, it returns the value <c>null</c> for the item and
        /// <c>-1</c> for the item index.</returns>
        public static KeyValuePair<int, T> FirstKvp<T>(
            this IEnumerable<T> nmrbl,
            Func<T, bool> predicate) => nmrbl.FirstKvp(
                (item, idx) => predicate(item));

        public static T FirstNotNull<T>(
            this T first,
            params T[] nextArr) => first ?? nextArr.FirstOrDefault(
                item => item != null)!;

        public static T FirstNotDefault<T>(
            this T first,
            T[] nextArr,
            Func<T, T, bool> eqCompr = null)
        {
            eqCompr ??= EqualityComparer<T>.Default.Equals;

            T retVal = first;

            if (eqCompr(retVal, default))
            {
                foreach (var item in nextArr)
                {
                    retVal = item;

                    if (!eqCompr(retVal, default))
                    {
                        break;
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Returns the provided input string if it is not null or empty or the <c>null</c> value otherwise.
        /// </summary>
        /// <param name="str">The input string</param>
        /// <returns>The provided input string if it is not null or empty or the <c>null</c> value otherwise.</returns>
        public static string? Nullify(
            this string? str) => string.IsNullOrEmpty(
                str) ? null : str;

        /// <summary>
        /// Returns the provided input value boxed into a nullable int if the input value is different than <c>0</c>
        /// or the <c>null</c> value if the input value is equal to <c>0</c>.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>The provided input value boxed into a nullable int if the input value is different than <c>0</c>
        /// or the <c>null</c> value if the input value is equal to <c>0</c>.</returns>
        public static int? Nullify(
            this int value)
        {
            int? retVal = null;

            if (value != 0)
            {
                retVal = value;
            }

            return retVal;
        }

        /// <summary>
        /// Extension method that calls the <see cref="string.Join(string?, string?[])" /> method.
        /// Because it is an extension method, it is easier to call than the CLR provided method.
        /// </summary>
        /// <param name="strArr">An array containing the string values to join</param>
        /// <param name="joinStr">A string to be used as a join parameter (i.e. inserted between each 2 adjacent strings).</param>
        /// <returns>The joined string.</returns>
        public static string JoinStr(
            this string[] strArr,
            string joinStr = null) => string.Join(
                joinStr ?? string.Empty, strArr);

        /// <summary>
        /// If the provided path is not null, it launches a Windows Explorer Process with this path
        /// as argument. If the provided path points to an existing file, this causes the default program for that file
        /// extension to open the file.
        /// </summary>
        /// <param name="path"></param>
        public static void OpenWithDefaultProgramIfNotNull(string path)
        {
            if (path != null)
            {
                using Process fileopener = new Process();

                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + path + "\"";
                fileopener.Start();
            }
        }

        /// <summary>
        /// Executes the provided delegate by wrapping its execution in a try/catch block. If executing the delegate
        /// results in an exception being thrown, the exception is being printed to the command prompt before that
        /// program exists.
        /// </summary>
        /// <param name="program"></param>
        public static void ExecuteProgram(
            Action program)
        {
            // Console.WriteLine();

            try
            {
                program();

                Console.ResetColor();
            }
            catch (Exception exc)
            {
                Console.WriteLine();
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;

                Console.WriteLine("AN UNHANDLED EXCEPTION WAS THROWN: ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine();
                Console.WriteLine(exc);
                Console.ResetColor();
            }

            // Console.WriteLine();
        }

        /// <summary>
        /// Returns the input string with the first letter changed to the uppercase variant if the provided was a lowercase variant.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <returns>The input instance of type <see cref="string"/> having its first letter converted to
        /// its uppercase variant</returns>
        public static string CapitalizeFirstLetter(
            this string str)
        {
            if (str.Any())
            {
                str = char.ToUpperInvariant(str[0]) + str[1..];
            }

            return str;
        }
    }
}
