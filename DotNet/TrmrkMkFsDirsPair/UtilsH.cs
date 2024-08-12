using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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

        /// <summary>
        /// Retrieves the first item from the provided array (preceded by the first argument) that is not null.
        /// </summary>
        /// <typeparam name="T">The items type.</typeparam>
        /// <param name="first">The first item to be checked against null.</param>
        /// <param name="nextArr">The array containing the next items tobe checked against null.</param>
        /// <returns>The first item from the provided array (preceded by the first argument) that is not null.</returns>
        public static T FirstNotNull<T>(
            this T first,
            params T[] nextArr) => first ?? nextArr.FirstOrDefault(
                item => item != null)!;

        /// <summary>
        /// Retrieves the first item from the provided array (preceded by the first argument) that is not equal to the type's default value.
        /// </summary>
        /// <typeparam name="T">The items type.</typeparam>
        /// <param name="first">The first item to be checked against null.</param>
        /// <param name="nextArr">The array containing the next items tobe checked against null.</param>
        /// <param name="eqCompr">An optional equality comparer delegate.</param>
        /// <returns>The first item from the provided array (preceded by the first argument) that is not equal to the type's default value.</returns>
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
        /// Takes the input value, converts it to an output value using the provided output value factory and returns the output value.
        /// It's a convenience method for using the return value of a method or expression without explicitly assigning its
        /// value to a local variable in the parent scope. Results in shorter and more concise code,
        /// but overuse might lead to ninja-code.
        /// </summary>
        /// <typeparam name="TIn">The input value type</typeparam>
        /// <typeparam name="TOut">The output value type.</typeparam>
        /// <param name="inVal">The provided input value to be passed in to the output value factory.</param>
        /// <param name="outValFactory">The provided output value delegate.</param>
        /// <returns>The value returned by the provided output value factory delegate.</returns>
        public static TOut With<TIn, TOut>(
            this TIn inVal,
            Func<TIn, TOut> outValFactory) => outValFactory(inVal);

        /// <summary>
        /// Takes the input value, passes it to the provided action delegate, and then returns it to the caller.
        /// It's a convenience method for using the return value of a method or expression without explicitly assigning its
        /// value to a local variable in the parent scope. Results in shorter and more concise code,
        /// but overuse might lead to ninja-code.
        /// </summary>
        /// <typeparam name="T">The input value type.</typeparam>
        /// <param name="val">The provided input value.</param>
        /// <param name="action">The provided action delegate.</param>
        /// <returns>The input value.</returns>
        public static T ActWith<T>(
            this T val,
            Action<T> action)
        {
            action(val);
            return val;
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

        /// <summary>
        /// Normalizes the provided path by combining it with the provided base path if the input path is not a rooted path.
        /// Both input path and the base path are replace with the current working directory path if they are null.
        /// </summary>
        /// <param name="path">The provided input path.</param>
        /// <param name="dfBasePath">The default base path for the input path.</param>
        /// <returns>An instance of type <see cref="string" /> representing the normalizes path.</returns>
        public static string NormalizePath(
            this string? path,
            string? dfBasePath = null)
        {
            path = path?.Trim().Nullify();
            path ??= Directory.GetCurrentDirectory();

            if (!Path.IsPathRooted(path))
            {
                dfBasePath ??= Directory.GetCurrentDirectory();
                path = Path.Combine(dfBasePath, path);
            }

            return path;
        }

        /// <summary>
        /// Converts the provided enumberable to an enumerable that is sorted by the provided property expression.
        /// </summary>
        /// <typeparam name="T">The type of the provided enumerable.</typeparam>
        /// <typeparam name="TProp">The type of the provided sort expression property</typeparam>
        /// <param name="nmrbl">The provided enumerable</param>
        /// <param name="orderByExpr">The provided property expression</param>
        /// <param name="useAscendingSortOrder">A boolean value indicating whether the returned enumerable
        /// should be sorted with ascending or descending order</param>
        /// <returns>The ordered enumerable</returns>
        public static IEnumerable<T> OrderByExpr<T, TProp>(
            this IEnumerable<T> nmrbl,
            Func<T, TProp> orderByExpr,
            bool useAscendingSortOrder)
        {
            if (useAscendingSortOrder)
            {
                nmrbl = nmrbl.OrderBy(orderByExpr);
            }
            else
            {
                nmrbl = nmrbl.OrderByDescending(orderByExpr);
            }

            return nmrbl;
        }

        /// <summary>
        /// Gets an array containing the specified file system entries.
        /// </summary>
        /// <param name="dirPath">The folder path from where to get the entries</param>
        /// <param name="getFolders">A nullable boolean flag indicating whether to get only folders, only files or all entries.</param>
        /// <returns></returns>
        public static string[] GetFsEntries(
            string dirPath,
            bool? getFolders = null) => getFolders switch
            {
                true => Directory.GetDirectories(dirPath),
                false => Directory.GetFiles(dirPath),
                _ => Directory.GetFileSystemEntries(dirPath)
            };

        /// <summary>
        /// Gets the file system entries from the provided dir path and converts them to instances of type <see cref="FsEntry" />.
        /// </summary>
        /// <param name="dirPath">The folder path from where to get the entries</param>
        /// <returns>A list containing the converted file system entries.</returns>
        public static FsEntry[] GetFileSystemEntries(
            string dirPath)
        {
            var fsEntriesList = new List<FsEntry>();

            AddFileSystemEntries(fsEntriesList, dirPath, true);
            AddFileSystemEntries(fsEntriesList, dirPath, false);

            return fsEntriesList.ToArray();
        }

        /// <summary>
        /// Gets the file system entries from the provided dir path, converts them to instances of type <see cref="FsEntry" />
        /// and adds them to the provided list.
        /// </summary>
        /// <param name="fsEntriesList">The list of converted file system entries.</param>
        /// <param name="dirPath">The folder path from where to get the entries</param>
        /// <param name="addFolders">A nullable boolean flag indicating whether to get only folders, only files or all entries.</param>
        public static void AddFileSystemEntries(
            List<FsEntry> fsEntriesList,
            string dirPath,
            bool? addFolders)
        {
            var fsEntriesArr = GetFsEntries(
                dirPath, addFolders).Select(
                    fsEntry => new FsEntry
                    {
                        FullPath = fsEntry,
                        Name = Path.GetFileName(fsEntry),
                        IsFolder = addFolders
                    });

            fsEntriesList.AddRange(fsEntriesArr);
        }
    }
}
