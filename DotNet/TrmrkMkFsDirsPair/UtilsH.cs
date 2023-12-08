using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    public static class UtilsH
    {
        public static readonly string ExecutingAssemmblyPath = AppDomain.CurrentDomain.BaseDirectory;

        public static string? Nullify(
            this string? str) => string.IsNullOrEmpty(
                str) ? null : str;

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

        public static string JoinStr(
            this string[] strArr,
            string joinStr = null) => string.Join(
                joinStr ?? string.Empty, strArr);

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

        public static void ExecuteProgram(
            Action program)
        {
            Console.WriteLine();

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

            Console.WriteLine();
        }
    }
}
