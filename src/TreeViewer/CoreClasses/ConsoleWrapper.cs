/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class ConsoleWrapper
    {
        public const char Whitespace = ' ';

        public static event EventHandler<ConsoleWindowResizedEventArgs> ConsoleWindowResized;

        private static TextWriter currentWriter = Console.Out;
        public static bool ConsoleEnabled => currentWriter != null;

        public static bool ColourEnabled { get; set; } = true;

        public static bool TextWrapEnabled { get; set; } = true;

        public static int ConsoleSizePollInterval { get; set; } = 1;

        public enum OutputModes { Out, Error, None }

        public static bool TreatControlCAsInput
        {
            get
            {
                if (!Console.IsInputRedirected)
                {
                    return Console.TreatControlCAsInput;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (!Console.IsInputRedirected)
                {
                    Console.TreatControlCAsInput = value;
                }
            }
        }

        static ConsoleWrapper()
        {
            lock (consoleSizeLock)
            {
                lastConsoleWidth = WindowWidth;
                lastConsoleHeight = WindowHeight;
            }

            StartConsoleSizeWatcher();
        }

        static private int lastConsoleWidth;
        static private int lastConsoleHeight;
        static private object consoleSizeLock = new object();

        static async void StartConsoleSizeWatcher()
        {
            while (true)
            {
                int winWidth = WindowWidth;
                int winHeight = WindowHeight;

                if (winWidth != lastConsoleWidth || winHeight != lastConsoleHeight)
                {
                    lock (consoleSizeLock)
                    {
                        lastConsoleWidth = winWidth;
                        lastConsoleHeight = winHeight;
                    }

                    ConsoleWindowResized?.Invoke(null, new ConsoleWindowResizedEventArgs(lastConsoleWidth, lastConsoleHeight, winWidth, winHeight));
                }

                await Task.Delay(ConsoleSizePollInterval);
            }
        }

        public static void SetOutputMode(OutputModes mode)
        {
            switch (mode)
            {
                case OutputModes.Out:
                    currentWriter = Console.Out;
                    break;
                case OutputModes.Error:
                    currentWriter = Console.Error;
                    break;
                case OutputModes.None:
                    currentWriter = null;
                    break;
            }
        }

        public static void Write(string value)
        {
            if (ConsoleEnabled)
            {
                currentWriter.Write(value);
            }
        }

        public static void Write(char value)
        {
            if (ConsoleEnabled)
            {
                currentWriter.Write(value);
            }
        }

        public static void Write(string format, params object[] args)
        {
            if (ConsoleEnabled)
            {
                currentWriter.Write(format, args);
            }
        }

        public static void Write(IEnumerable<ConsoleTextSpan> formattedText)
        {
            if (ConsoleEnabled)
            {
                ConsoleColor foreground = Console.ForegroundColor;
                ConsoleColor background = Console.BackgroundColor;

                foreach (ConsoleTextSpan span in formattedText)
                {
                    InternalWrite(span);
                }

                ForegroundColor = foreground;
                BackgroundColor = background;
            }
        }

        public static void Write(ConsoleTextSpan formattedText)
        {
            if (ConsoleEnabled)
            {
                ConsoleColor foreground = Console.ForegroundColor;
                ConsoleColor background = Console.BackgroundColor;

                InternalWrite(formattedText);

                ForegroundColor = foreground;
                BackgroundColor = background;
            }
        }

        private static void InternalWrite(ConsoleTextSpan formattedText)
        {
            ForegroundColor = formattedText.Foreground;
            BackgroundColor = formattedText.Background;

            if (!TextWrapEnabled || Console.IsOutputRedirected)
            {
                Write(formattedText.Text);
            }
            else
            {
                int availableSpace = WindowWidth - CursorLeft;
                string text = formattedText.Text;

                while (text.Length > 0)
                {
                    if (text.Length <= availableSpace)
                    {
                        Write(text);
                        text = "";
                    }
                    else
                    {
                        int space = text.LastIndexOfAny(new char[] { ' ', '\t', '\n' }, availableSpace, availableSpace);
                        if (space >= 0)
                        {
                            int prevTop = CursorTop;
                            string substr = text.Substring(0, space);
                            Write(substr);
                            if (CursorTop == prevTop + CountOccurrences(substr, '\n'))
                            {
                                WriteLine();
                            }

                            if (text[space] != '\n')
                            {
                                Write(new string(Whitespace, formattedText.WrapIndent));
                            }

                            text = text.Substring(space + 1);
                        }
                        else if (CursorLeft > formattedText.WrapIndent)
                        {
                            WriteLine();
                            Write(new string(Whitespace, formattedText.WrapIndent));
                        }
                        else
                        {
                            Write(text);
                            text = "";
                        }
                    }

                    availableSpace = WindowWidth - CursorLeft;
                }
            }

        }

        public static void WriteLine()
        {
            if (ConsoleEnabled)
            {
                currentWriter.WriteLine();
            }
        }

        public static void WriteLine(string value)
        {
            if (ConsoleEnabled)
            {
                currentWriter.WriteLine(value);
            }
        }

        public static void WriteLine(string format, params object[] args)
        {
            if (ConsoleEnabled)
            {
                currentWriter.WriteLine(format, args);
            }
        }

        public static void WriteLine(IEnumerable<ConsoleTextSpan> formattedText)
        {
            Write(formattedText);
            WriteLine();
        }

        public static void WriteLine(ConsoleTextSpan formattedText)
        {
            Write(formattedText);
            WriteLine();
        }

        public static void WriteList(IEnumerable<string> list, string indent = "")
        {
            List<string> items = new List<string>(list);

            int maxWidth = (from el in items select el.Length).Max() + 4;

            int itemsPerLine = (WindowWidth - indent.Length) / maxWidth;

            for (int i = 0; i < items.Count; i++)
            {
                if (i % itemsPerLine == 0)
                {
                    Write(indent);
                }

                Write(items[i]);

                Write(new string(Whitespace, maxWidth - items[i].Length));

                if ((i + 1) % itemsPerLine == 0 || i == items.Count - 1)
                {
                    WriteLine();
                }
                else
                {
                    Write(new string(Whitespace, 4));
                }
            }
        }

        public static void WriteList(IEnumerable<ConsoleTextSpan[]> list, string indent = "")
        {
            List<ConsoleTextSpan[]> items = new List<ConsoleTextSpan[]>(list);

            int maxWidth = (from el in items select el.Unformat().Length).Max() + 4;

            int itemsPerLine = Math.Max(1, (WindowWidth - indent.Length) / maxWidth);

            for (int i = 0; i < items.Count; i++)
            {
                if (i % itemsPerLine == 0)
                {
                    Write(indent);
                }

                Write(items[i]);

                Write(new string(Whitespace, maxWidth - items[i].Unformat().Length));

                if ((i + 1) % itemsPerLine == 0 || i == items.Count - 1)
                {
                    WriteLine();
                }
            }
        }

        public static void SetCursorPosition(int left, int top)
        {
            if (ConsoleEnabled && !Console.IsOutputRedirected)
            {
                if (left >= 0 && left <= Console.BufferWidth)
                {
                    if (top >= 0 && top < Console.BufferHeight)
                    {
                        Console.SetCursorPosition(left, top);
                    }
                    else if (top >= 0)
                    {
                        int delta = top - Console.BufferHeight + 1;
                        Console.Write(new string('\n', delta));
                        top -= delta;
                        Console.SetCursorPosition(left, top);
                        ConsoleWindowResized?.Invoke(null, new ConsoleWindowResizedEventArgs(lastConsoleWidth, lastConsoleHeight, Console.WindowWidth, Console.WindowHeight));
                    }
                }
            }
        }

        public static int CursorTop { get { if (ConsoleEnabled && !Console.IsOutputRedirected) { return Console.CursorTop; } else { return 0; } } set { if (ConsoleEnabled && !Console.IsOutputRedirected) { Console.CursorTop = value; } } }

        public static int CursorLeft { get { if (ConsoleEnabled && !Console.IsOutputRedirected) { return Console.CursorLeft; } else { return 0; } } set { if (ConsoleEnabled && !Console.IsOutputRedirected) { Console.CursorLeft = value; } } }

        public static int WindowWidth { get { if (ConsoleEnabled && !Console.IsOutputRedirected) { return Console.WindowWidth; } else { return 120; } } }
        public static int WindowHeight { get { if (ConsoleEnabled && !Console.IsOutputRedirected) { return Console.WindowHeight; } else { return 30; } } }

        public static int BufferWidth { get { if (ConsoleEnabled && !Console.IsOutputRedirected) { return Console.BufferWidth; } else { return 0; } } }
        public static int BufferHeight { get { if (ConsoleEnabled && !Console.IsOutputRedirected) { return Console.BufferHeight; } else { return 0; } } }

        public static void Clear()
        {
            if (ConsoleEnabled && !Console.IsOutputRedirected)
            {
                Console.Clear();
            }
        }

        public static void MoveBufferAndClear()
        {
            if (ConsoleEnabled && !Console.IsOutputRedirected)
            {
                int shift = 0;

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) && ConsoleWrapper.CursorTop - ConsoleWrapper.WindowHeight >= 0)
                {
                        Console.MoveBufferArea(0, ConsoleWrapper.CursorTop - ConsoleWrapper.WindowHeight, ConsoleWrapper.BufferWidth, ConsoleWrapper.WindowHeight, 0, 0);
                }
                else
                {
                    shift = ConsoleWrapper.CursorTop - ConsoleWrapper.WindowHeight + 1;
                }

                ConsoleWrapper.SetCursorPosition(ConsoleWrapper.CursorLeft, ConsoleWrapper.WindowHeight - 1 + shift);

                if (ConsoleWrapper.BufferHeight >= 3 * ConsoleWrapper.WindowHeight)
                {
                    int heightToClear = ConsoleWrapper.BufferHeight - ConsoleWrapper.WindowHeight * 2;
                    int bottom = ConsoleWrapper.BufferHeight - ConsoleWrapper.WindowHeight;

                    while (bottom > 2 * ConsoleWrapper.WindowHeight)
                    {
                        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) && bottom - heightToClear / 2 >= 0)
                        {
                            Console.MoveBufferArea(0, bottom - heightToClear / 2, ConsoleWrapper.BufferWidth, heightToClear / 2, 0, ConsoleWrapper.WindowHeight);
                        }

                        heightToClear /= 2;
                        bottom -= heightToClear;
                    }

                    ConsoleWrapper.SetCursorPosition(ConsoleWrapper.CursorLeft, ConsoleWrapper.WindowHeight - 1);
                    for (int i = 0; i < ConsoleWrapper.WindowHeight; i++)
                    {
                        ConsoleWrapper.WriteLine(new string(Whitespace, ConsoleWrapper.BufferWidth));
                    }
                }
                else
                {
                    for (int i = 0; i < ConsoleWrapper.BufferHeight - ConsoleWrapper.WindowHeight * 2; i++)
                    {
                        ConsoleWrapper.WriteLine(new string(Whitespace, ConsoleWrapper.BufferWidth));
                    }
                }

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    Console.WindowTop = 0;
                }

                ConsoleWrapper.SetCursorPosition(ConsoleWrapper.CursorLeft, ConsoleWrapper.WindowHeight - 1 + shift);
            }
        }

        public static bool CursorVisible { set { if (ConsoleEnabled && !Console.IsOutputRedirected) { try { Console.CursorVisible = value; } catch { } } } }

        public static ConsoleColor ForegroundColor { get { if (ConsoleEnabled && ColourEnabled && !Console.IsOutputRedirected) { return Console.ForegroundColor; } else { return ConsoleColor.Gray; } } set { if (ConsoleEnabled && ColourEnabled && !Console.IsOutputRedirected) { Console.ForegroundColor = value; } } }
        public static ConsoleColor BackgroundColor { get { if (ConsoleEnabled && ColourEnabled && !Console.IsOutputRedirected) { return Console.BackgroundColor; } else { return ConsoleColor.Black; } } set { if (ConsoleEnabled && ColourEnabled && !Console.IsOutputRedirected) { Console.BackgroundColor = value; } } }

        public static ConsoleKeyInfo ReadKey()
        {
            if (!Console.IsInputRedirected)
            {
                return Console.ReadKey();
            }
            else
            {
                int keyChar = Console.Read();
                ConsoleKey key = ConsoleKey.NoName;

                bool shift = false;
                bool ctrl = false;
                bool alt = false;

                if (keyChar < 0)
                {
                    keyChar = 'd';
                    key = ConsoleKey.D;
                    ctrl = true;
                }
                else if ((char)keyChar == '\n')
                {
                    key = ConsoleKey.Enter;
                }
                else if ((char)keyChar == '\t')
                {
                    key = ConsoleKey.Tab;
                }

                return new ConsoleKeyInfo((char)keyChar, key, shift, alt, ctrl);
            }
        }

        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (!Console.IsInputRedirected)
            {
                return Console.ReadKey(intercept);
            }
            else
            {
                int keyChar = Console.Read();
                ConsoleKey key = ConsoleKey.NoName;

                bool shift = false;
                bool ctrl = false;
                bool alt = false;

                if (keyChar < 0)
                {
                    keyChar = 'd';
                    key = ConsoleKey.D;
                    ctrl = true;
                }
                else if ((char)keyChar == '\n')
                {
                    key = ConsoleKey.Enter;
                }
                else if ((char)keyChar == '\t')
                {
                    key = ConsoleKey.Tab;
                }

                return new ConsoleKeyInfo((char)keyChar, key, shift, alt, ctrl);
            }
        }

        public static string ReadLine()
        {
            //return Console.ReadLine();

            int commandStart = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;

            int lastCursorPos = commandStart;

            StringBuilder command = new StringBuilder();

            bool finishedCommand = false;

            if (!Console.IsOutputRedirected && !Console.IsInputRedirected)
            {
                ConsoleKeyInfo ki;

                while (!finishedCommand)
                {
                    ki = ConsoleWrapper.ReadKey(true);

                    if (ki.Key == ConsoleKey.Backspace)
                    {
                        int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                        if (cursorPos > commandStart)
                        {
                            command.Remove(cursorPos - commandStart - 1, 1);

                            ConsoleWrapper.CursorVisible = false;
                            ConsoleWrapper.Write('\b');
                            ConsoleWrapper.Write(command.ToString(cursorPos - commandStart - 1, command.Length - cursorPos + commandStart + 1));
                            ConsoleWrapper.Write(ConsoleWrapper.Whitespace);

                            int newPos = Math.Max(cursorPos - 1, commandStart);
                            ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                            ConsoleWrapper.CursorVisible = true;
                        }
                    }
                    else if (ki.Key == ConsoleKey.Delete)
                    {
                        int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                        if (cursorPos - commandStart < command.Length)
                        {
                            command.Remove(cursorPos - commandStart, 1);

                            ConsoleWrapper.CursorVisible = false;
                            ConsoleWrapper.Write(command.ToString(cursorPos - commandStart, command.Length - cursorPos + commandStart));
                            ConsoleWrapper.Write(ConsoleWrapper.Whitespace);

                            int newPos = Math.Max(cursorPos, commandStart);
                            ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                            ConsoleWrapper.CursorVisible = true;
                        }
                    }
                    else if (ki.Key == ConsoleKey.Home)
                    {
                        ConsoleWrapper.SetCursorPosition(commandStart % ConsoleWrapper.WindowWidth, commandStart / ConsoleWrapper.WindowWidth);
                    }
                    else if (ki.Key == ConsoleKey.End)
                    {
                        ConsoleWrapper.SetCursorPosition((commandStart + command.Length) % ConsoleWrapper.WindowWidth, (commandStart + command.Length) / ConsoleWrapper.WindowWidth);

                    }
                    else if (ki.Key == ConsoleKey.LeftArrow)
                    {
                        int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                        int newPos = Math.Max(cursorPos - 1, commandStart);
                        ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                    }
                    else if (ki.Key == ConsoleKey.RightArrow)
                    {
                        int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                        int newPos = Math.Min(cursorPos + 1, commandStart + command.Length);
                        ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                    }
                    else if (ki.Key == ConsoleKey.C && ki.Modifiers == ConsoleModifiers.Control)
                    {
                        ConsoleWrapper.WriteLine("^C");
                        return null;
                    }
                    else
                    {
                        if (ki.Key == ConsoleKey.Enter)
                        {
                            ConsoleWrapper.WriteLine();
                            finishedCommand = true;
                        }
                        else if (ConsoleWrapper.IsPrintable(ki.KeyChar))
                        {
                            int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                            command.Insert(cursorPos - commandStart, ki.KeyChar);

                            ConsoleWrapper.CursorVisible = false;
                            ConsoleWrapper.Write(command.ToString(cursorPos - commandStart, command.Length - cursorPos + commandStart));

                            int newPos = Math.Min(cursorPos + 1, commandStart + command.Length);
                            ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                            ConsoleWrapper.CursorVisible = true;
                        }
                    }
                    lastCursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                }
            }
            else
            {
                ConsoleKeyInfo ki;

                while (!finishedCommand)
                {
                    ki = ConsoleWrapper.ReadKey(true);

                    if (ki.Key == ConsoleKey.C && ki.Modifiers == ConsoleModifiers.Control)
                    {
                        ConsoleWrapper.WriteLine("^C");
                        return null;
                    }
                    else
                    {
                        if (ki.Key == ConsoleKey.Enter)
                        {
                            ConsoleWrapper.WriteLine();
                            finishedCommand = true;
                        }
                        else if (ConsoleWrapper.IsPrintable(ki.KeyChar))
                        {
                            command.Append(ki.KeyChar);

                            ConsoleWrapper.Write(ki.KeyChar);
                        }
                    }
                }
            }

            return command.ToString();
        }

        public static int Read()
        {
            return Console.Read();
        }

        //Based on https://stackoverflow.com/questions/3253247/how-do-i-detect-non-printable-characters-in-net
        public static bool IsPrintable(char c)
        {
            if (char.IsWhiteSpace(c))
            {
                return true;
            }
            else
            {
                UnicodeCategory cat = char.GetUnicodeCategory(c);
                return cat != UnicodeCategory.Control && cat != UnicodeCategory.OtherNotAssigned && cat != UnicodeCategory.Surrogate;
            }
        }

        private static int CountOccurrences(string sr, char needle)
        {
            int count = 0;

            for (int i = 0; i < sr.Length; i++)
            {
                if (sr[i] == needle)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public class ConsoleWindowResizedEventArgs : EventArgs
    {
        public int PreviousWidth { get; }
        public int PreviousHeight { get; }
        public int CurrentWidth { get; }
        public int CurrentHeight { get; }

        public ConsoleWindowResizedEventArgs(int previousWidth, int previousHeight, int currentWidth, int currentHeight)
        {
            PreviousWidth = previousWidth;
            PreviousHeight = previousHeight;
            CurrentWidth = currentWidth;
            CurrentHeight = currentHeight;
        }
    }

    public class ConsoleTextSpan
    {
        public string Text { get; set; }
        public ConsoleColor Foreground { get; set; } = ConsoleColor.Gray;
        public ConsoleColor Background { get; set; } = ConsoleColor.Black;
        public int WrapIndent { get; set; } = 0;

        public ConsoleTextSpan() { }

        public ConsoleTextSpan(string text, ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black)
        {
            this.Text = text;
            this.Foreground = foreground;
            this.Background = background;
        }

        public ConsoleTextSpan(string text, int wrapIndent, ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black)
        {
            this.Text = text;
            this.WrapIndent = wrapIndent;
            this.Foreground = foreground;
            this.Background = background;
        }


        public override string ToString()
        {
            return this.Text;
        }
    }

    public static class ConsoleTextSpanExtensions
    {
        public static string Unformat(this IEnumerable<ConsoleTextSpan> formattedText)
        {
            return formattedText.Aggregate("", (a, b) => a + b.ToString());
        }
    }
}
