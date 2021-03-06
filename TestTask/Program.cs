﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TestTask
{

    public class Program
    {
        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            Console.ResetColor();
            while (true)
            {
                var timeExecute = new Stopwatch();
                var messageBank = new Title();
                args = new string[2];
                bool failureRead = true;
                IReadOnlyStream inputStream1 = null;
                IReadOnlyStream inputStream2 = null;

                timeExecute.Start();
                while (failureRead)
                {
                    Console.Write(Title.FirstPathFileMessage);
                    failureRead = ReadConsoleInput(args[0], ref inputStream1);
                }
                failureRead = true;

                while (failureRead)
                {
                    Console.Write(Title.SecondPathFileMessage);
                    failureRead = ReadConsoleInput(args[1], ref inputStream2);
                }

                IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

                Console.WriteLine("\nРегистрозависимая статистика вхождения каждой буквы:");
                PrintStatistic(singleLetterStats);
                Console.WriteLine("\nНе регистрозависимая статистика вхождения парных букв:");
                PrintStatistic(doubleLetterStats);

                RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
                RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

                Console.WriteLine("\nСстатистика вхождения гласных букв:");
                PrintStatistic(singleLetterStats);
                Console.WriteLine("\nСстатистика вхождения согласных парных букв:");
                PrintStatistic(doubleLetterStats);

                inputStream1.Dispose();
                inputStream2.Dispose();

                timeExecute.Stop();
                Console.WriteLine($"\nВремя выполнения: {timeExecute.ElapsedMilliseconds} мсек");
                Console.WriteLine($"Размер файла №1: {inputStream1.LenghtFile} байт");
                Console.WriteLine($"Размер файла №2: {inputStream2.LenghtFile} байт");
                // TODO : Необжодимо дождаться нажатия клавиши, прежде чем завершать выполнение программы.
                Console.WriteLine("\nПрограмма выполнена. Что бы начать заново - нажмите любую клавишу");
                Console.ReadKey();
                Console.Clear();
            }
        }

        /// <summary>
        /// Метод считывает ввод с консоли (путь к файлу) и возвращает false в случае если указанный путь к файлу верен, в противном случае true.
        /// </summary>
        /// <param name="args">Путь до первого файла</param>
        /// <param name="inputStream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Флаг успешного пути к файлу.</returns>
        private static bool ReadConsoleInput(string args, ref IReadOnlyStream inputStream)
        {
            args = Console.ReadLine();
            try
            {
                inputStream = GetInputStream(args);
                return false;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                return true;
            }
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }


        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {

            string pattern = @"\p{L}";
            Regex regex = new Regex(pattern);
            stream.ResetPositionToStart();
            List<LetterStats> letterStats = new List<LetterStats>();
            try
            {
                while (!stream.IsEof)
                {
                    char c = stream.ReadNextChar();
                    if (regex.IsMatch(c.ToString()))
                    {
                        LetterStats stats = new LetterStats();
                        stats.Letter = c.ToString();
                        IncStatistic(stats);
                        letterStats.Add(stats);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - регистрозависимый.
            finally
            {
                for (int i = 0; i < letterStats.Count; i++)
                {
                    for (int j = i + 1; j < letterStats.Count; j++)
                    {
                        if (letterStats[i].Letter == letterStats[j].Letter)
                        {

                            IncStatistic(letterStats[i]);
                            letterStats.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
            return letterStats;
        }


        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            List<char> tempCharArray = new List<char>();
            string pattern = @"\p{L}";
            Regex regex = new Regex(pattern);
            stream.ResetPositionToStart();
            List<LetterStats> letterStats = new List<LetterStats>();
            try
            {
                while (!stream.IsEof)
                {
                    char c = stream.ReadNextChar();
                    tempCharArray.Add(c);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - регистрозависимый.
            finally
            {
                for (int i = 0; i < tempCharArray.Count - 1; i++)
                {
                    if (char.ToUpper(tempCharArray[i]) == char.ToUpper(tempCharArray[i + 1]) && regex.IsMatch(tempCharArray[i].ToString()))
                    {
                        LetterStats stats = new LetterStats();
                        stats.Letter = string.Concat(tempCharArray[i], tempCharArray[i + 1]).ToUpper();
                        IncStatistic(stats);
                        letterStats.Add(stats);
                    }
                }
                for (int i = 0; i < letterStats.Count; i++)
                {
                    for (int j = i + 1; j < letterStats.Count; j++)
                    {
                        if (letterStats[i].Letter == letterStats[j].Letter)
                        {
                            IncStatistic(letterStats[i]);
                            letterStats.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
            return letterStats;
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            // TODO : Удалить статистику по запрошенному типу букв.          
            string pattern;
            switch (charType)
            {
                case CharType.Consonants:
                    pattern = @"[aeiouAEIOUауоыиэяюёеАУОЫИЭЯЮЁЕ]";
                    RemoveCharStatsByPattern(letters, pattern);
                    break;

                case CharType.Vowel:
                    pattern = @"[^aeiouAEIOUауоыиэяюёеАУОЫИЭЯЮЁЕ]";
                    RemoveCharStatsByPattern(letters, pattern);
                    break;
            }
        }

        /// <summary>
        /// Метод перебирает все найденные символы, удовлетворяющие шаблону переданного регулярного выражения.
        /// (Тип символов для перебора определяется параметром pattern типа string)
        /// Все найденные символы соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="pattern">Шаблон регулярного выражения для анализа</param>

        // Метод RemoveCharStatsByPattern(string pattern, IList<LetterStats> letters) является более универсальной реализацией
        // и позволяет искать не только вхождение глассных/согласных, но и производить анализ текста на основе множества регулярных 
        // выражений которые можно создать под различные задчи сортировки и поиска.
        // Колличество регулярных выражений может легко масштабироваться без лишнего кода и блока switch, достаточно создать массив или список 
        // ругулярных выражений, который дополнять по мере необходимости новыми комбинациями.
        private static void RemoveCharStatsByPattern(IList<LetterStats> letters, string pattern)
        {
            int i = 0;
            Regex regex = new Regex(pattern);
            while (i < letters.Count)
            {
                if (regex.IsMatch(letters[i].Letter))
                    letters.Remove(letters[i]);
                else i++;
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            // TODO : Выводить на экран статистику. Выводить предварительно отсортировав по алфавиту!
            int totalLettersValue = 0;
            List<LetterStats> tempList = letters as List<LetterStats>;
            tempList.Sort(delegate (LetterStats l1, LetterStats l2) { return l1.Letter.CompareTo(l2.Letter); });
            foreach (var let in letters)
            {
                totalLettersValue += let.Count;
                Console.WriteLine($"{let.Letter} : {let.Count}");
            }
            Console.WriteLine($"Итого: {totalLettersValue} букв/пар в тексте");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(LetterStats letterStats)
        {
            letterStats.Count++;
        }


    }
}
