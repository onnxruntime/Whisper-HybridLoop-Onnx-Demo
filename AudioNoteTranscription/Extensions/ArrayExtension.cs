using System;
using System.Collections.Generic;

namespace AudioNoteTranscription.Extensions
{
    public static class ArrayExtension
    {
        public static IEnumerable<Memory<T>> ChunkViaMemory<T>(this T[] source, int size) where T : struct
        {
            var chunks = source.Length / size;
            for (int i = 0; i < chunks; i++)
            {
                yield return source.AsMemory(i * size, size);
            }
            var leftOver = source.Length % size;
            if (leftOver > 0)
            {
                yield return source.AsMemory(chunks * size, leftOver);
            }
        }
    }
}
