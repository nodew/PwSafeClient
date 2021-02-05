using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PwSafeClient.Console
{
    public static class ConsoleHelper
    {
        public static SecureString ReadPassword()
        {
            var pwd = new SecureString();
            System.Console.Write("Enter your password: ");
            while (true)
            {
                ConsoleKeyInfo i = System.Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        System.Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000')
                {
                    pwd.AppendChar(i.KeyChar);
                    System.Console.Write("*");
                }
            }

            return pwd;
        }
    }
}
