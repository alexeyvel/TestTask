﻿namespace TestTask
{
    /// <summary>
    /// Интерфейс для работы с файлом в сильно урезаном виде.
    /// Умеет всего 2 вещи: прочитать символ, и перемотать стрим на начало.
    /// </summary>
    internal interface IReadOnlyStream
    {
        // TODO : Необходимо доработать данный интерфейс для обеспечения гарантированного закрытия файла, по окончанию работы с таковым!
        void Dispose();
        char ReadNextChar();

        void ResetPositionToStart();

        bool IsEof { get; }
        long LenghtFile { get; } //добавил на всякий случай, для того что бы при апкасте иметь возможность вытянуть свойство где бы лежал размер считанного файла
    }
}
