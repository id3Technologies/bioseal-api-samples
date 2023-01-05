using System;

namespace BioSealWSCodeSamples
{
    class ByteTools
    {
        public static bool AreBytesArrayEqual(byte[] array1, byte[] array2)
        {
            // handle null cases
            if (array1 == null && array2 == null)
                return true;
            if (array1 == null || array2 == null)
                return false;

            // check length
            if (array1.Length != array2.Length)
                return false;

            // byte-per-byte comparison
            for (int i = 0; i < array1.Length; i++)
                if (array1[i] != array2[i])
                    return false;

            return true;
        }

        public static byte[] TrimEndByteArray(byte[] array, bool addLastZero = false)
        {
            int lastZeroIndex = Array.FindLastIndex(array, b => b != 0);

            if (lastZeroIndex >= array.Length - 1)
                addLastZero = false;

            if (addLastZero)
                Array.Resize(ref array, lastZeroIndex + 2); // include last zero
            else
                Array.Resize(ref array, lastZeroIndex + 1);

            return array;
        }

        public static byte[] TrimByteArrayToIndex(byte[] array, int index)
        {
            Array.Resize(ref array, index);

            return array;
        }
    }
}
