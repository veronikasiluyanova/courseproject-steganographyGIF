using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Steganography_GIF
{
	public class GIFEncryptorByLSBMethod
	{
		protected static byte[] checkSequence = new byte[] { 0, 1, 0, 1, 0, 1, 0, 1 };
		protected static int firstLSBit = 0;
		protected static int secondLSBit = 1;
		public static byte[] Encrypt(string inputPath, string text)
		{
			// read bytes from input file
			byte[] bytes = File.ReadAllBytes(inputPath);

			// check format
			if (!(Encoding.ASCII.GetString(bytes, 0, 6)).Equals("GIF89a"))
			{
				throw new Exception("Input file has wrong GIF format");
			}

			// read palette size property from first three bits in the 10-th byte from the file
			BitArray bytes10 = new BitArray(new byte[] { bytes[10] });
			BitArray bytes10_3 = new BitArray(3);
			bytes10_3[0] = bytes10[0]; bytes10_3[1] = bytes10[1]; bytes10_3[2] = bytes10[2];
			long paletteSize = Encryptor.GetIntFromBitArray(bytes10_3);

			// calculate color count and possible message length
			int bOrigColorCount = (int)Math.Pow(2, paletteSize + 1);
			int possibleMessageLength = bOrigColorCount * 3 / 4;
			int possibleTextLength = possibleMessageLength - 2;
			if (possibleTextLength < text.Length)
			{
				throw new Exception("Text is too big");
			}

			int n = 13;

			// write check sequence
			for (int i = 0; i < checkSequence.Length / 2; i++)
			{
				byte[] bytes_n = BitConverter.GetBytes(bytes[n]);
				bytes_n[firstLSBit] = checkSequence[2 * i];
				bytes_n[secondLSBit] = checkSequence[2 * i + 1];
				bytes[n] = (byte)Encryptor.GetIntFromBitArray(new BitArray(bytes_n));
				n++;
			}

			// write text length
			BitArray bitTextLength = new BitArray(new byte[] { (byte)text.Length });
			for (int i = 0; i < bitTextLength.Length / 2; i++)
			{
				BitArray bytes_n = new BitArray(new byte[] { bytes[n] });
				bytes_n[firstLSBit] = bitTextLength[2 * i];
				bytes_n[secondLSBit] = bitTextLength[2 * i + 1];
				bytes[n] = (byte)Encryptor.GetIntFromBitArray(bytes_n);
				n++;
			}

			// write message
			byte[] bytesText = Encoding.Default.GetBytes(text);
			for (int i = 0; i < bytesText.Length; i++)
			{
				byte[] bytesChar = BitConverter.GetBytes(bytesText[i]);
				for (int ci = 0; ci < bytesChar.Length / 2; ci++)
				{
					byte[] bytes_n = BitConverter.GetBytes(bytes[n]);
					bytes_n[firstLSBit] = bytesChar[2 * ci];
					bytes_n[secondLSBit] = bytesChar[2 * ci + 1];
					bytes[n] = (byte)Encryptor.GetIntFromBitArray(new BitArray(bytes_n));
					n++;
				}
			}

			return bytes;
		}

		public static byte[] Decrypt(string inputPath)
		{
			// read bytes from input file
			byte[] bytes = File.ReadAllBytes(inputPath);

			// check format
			if (!(Encoding.ASCII.GetString(bytes, 0, 6)).Equals("GIF89a"))
			{
				throw new Exception("Input file has wrong GIF format");
			}

			// read palette size property from first three bits in the 10-th byte from the file
			BitArray b10 = new BitArray(new byte[] { bytes[10] });
			BitArray b10_3 = new BitArray(3);
			b10_3[0] = b10[0]; b10_3[1] = b10[1]; b10_3[2] = b10[2];
			long bsize = Encryptor.GetIntFromBitArray(b10_3);

			// calculate color count and possible message length
			int bOrigColorCount = (int)Math.Pow(2, bsize + 1);
			int possibleMessageLength = bOrigColorCount * 3 / 4;
			int possibleTextLength = possibleMessageLength - 2; // one byte for check and one byte for message length

			int n = 13;

			// read check sequence
			BitArray csBytes = new BitArray(checkSequence);
			for (int i = 0; i < 4; i++)
			{
				BitArray bit = new BitArray(new byte[] { bytes[n] });
				csBytes[2 * i] = bit[firstLSBit];
				csBytes[2 * i + 1] = bit[secondLSBit];
				n++;
			}
			byte cs = (byte)Encryptor.GetIntFromBitArray(csBytes);

			if (cs != (byte)Encryptor.GetIntFromBitArray(new BitArray(checkSequence)))
			{
				return new byte[] { 0 };
			}

			// read text length
			BitArray bytesTextLength = new BitArray(new byte[8]);
			for (int i = 0; i < 4; i++)
			{
				BitArray bytes_n = new BitArray(new byte[] { bytes[n] });
				bytesTextLength[2 * i] = bytes_n[firstLSBit];
				bytesTextLength[2 * i + 1] = bytes_n[secondLSBit];
				n++;
			}
			byte textLength = (byte)Encryptor.GetIntFromBitArray(new BitArray(bytesTextLength));

			if (textLength < 0)
			{
				throw new Exception("Decoded text length is less than 0");
			}
			if (possibleTextLength < textLength)
			{
				throw new Exception("There is no messages (Decoded message length (" + textLength + ")" +
					" is less than Possible message length (" + possibleTextLength + "))");
			}

			// read text bits and make text bytes
			byte[] bytesText = new byte[textLength];
			for (int i = 0; i < bytesText.Length; i++)
			{
				byte[] bytesChar = BitConverter.GetBytes(bytesText[i]);
				for (int bci = 0; bci < bytesChar.Length / 2; bci++)
				{
					byte[] bytes_n = BitConverter.GetBytes(bytes[n]);
					bytesChar[2 * bci] = bytes_n[firstLSBit];
					bytesChar[2 * bci + 1] = bytes_n[secondLSBit];
					n++;
				}
				bytesText[i] = (byte)Encryptor.GetIntFromBitArray(new BitArray(bytesChar));
			}

			return bytesText;
		}
	}

}
