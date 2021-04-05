using System;
using System.Text;

namespace MSM.Functions
{
    public static class Generate
    {
        static Generate()
        {
            Int32 itterations = StaticRandom.Next(100, 1000);
            for (Int32 i = 0; i < itterations; i++)
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                StaticRandom.Next(1, 10);
            }
        }

        public static readonly Random StaticRandom = new(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
        public static String RandomPassword(UInt32 length)
        {
            StringBuilder builder = new();
            for (UInt32 i = 0; i < length; i++)
            {
                builder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor(26 * StaticRandom.NextDouble() + 65))));
            }
            return builder.ToString();
        }
        public static String RandomUniqueID(UInt32 maxLength)
        {
            String uniqueID = RandomUniqueID();
            if (uniqueID.Length > maxLength)
            {
                uniqueID = uniqueID.Substring(0, (Int32)maxLength);
            }
            return uniqueID;
        }
        public static String RandomUniqueID()
        {
            return Guid.NewGuid().ToString("N");
        }
        public static Guid RandomGuid()
        {
            return Guid.NewGuid();
        }
    }
}