using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Steganography_GIF
{
	public class Encryptor
	{
		public static int GetPossibleTextLength(string file)
		{
			byte[] bytes = File.ReadAllBytes(file);

			if (!(Encoding.ASCII.GetString(bytes, 0, 6)).Equals("GIF89a"))
			{
				throw new Exception("Input file has wrong GIF format");
			}

			BitArray b10 = new BitArray(new byte[] { bytes[10] });
			BitArray b10_3 = new BitArray(3);
			b10_3[0] = b10[0]; b10_3[1] = b10[1]; b10_3[2] = b10[2];
			long bsize = GetIntFromBitArray(b10_3);

			int possibleMessageLength = (int)(256 - Math.Pow(2, bsize)) * 3;
			int possibleTextLength = possibleMessageLength - 3;

			return possibleTextLength;
		}
		public static long GetPaletteSize(string file)
		{
			byte[] bytes = File.ReadAllBytes(file);

			if (!(Encoding.ASCII.GetString(bytes, 0, 6)).Equals("GIF89a"))
			{
				throw new Exception("Input file has wrong GIF format");
			}

			BitArray b10 = new BitArray(new byte[] { bytes[10] });
			BitArray b10_3 = new BitArray(3);
			b10_3[0] = b10[0]; b10_3[1] = b10[1]; b10_3[2] = b10[2];
			long paletteSize = GetIntFromBitArray(b10_3);

			return paletteSize;
		}
		public static long GetIntFromBitArray(BitArray bitArray)
		{
			long value = 0;

			for (int i = 0; i < bitArray.Count; i++)
			{
				if (bitArray[i])
					value += Convert.ToInt64(Math.Pow(2, i));
			}

			return value;
		}
	}

}
