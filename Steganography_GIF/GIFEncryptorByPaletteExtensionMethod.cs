using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Steganography_GIF
{
	public class GIFEncryptorByPaletteExtensionMethod
	{
		static protected byte[] checkSequence = new byte[] { 0, 1, 0, 1, 0, 1, 0, 1 };
		public static byte[] Encrypt(string input, string text)
		{
			// read bytes from file
			byte[] bytes = File.ReadAllBytes(input);

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

			// calculate original color count
			int bOrigColorCount = (int)Math.Pow(2, paletteSize + 1);

			// calculate new palette size to contain all message
			long newPaletteSize = paletteSize;
			int possibleMessageLength = (int)(Math.Pow(2, newPaletteSize + 1) - bOrigColorCount) * 3;
			int possibleTextLength = possibleMessageLength - 3;
			while ((newPaletteSize < 7) && (possibleTextLength < text.Length))
			{
				newPaletteSize++;
				possibleMessageLength = (int)(Math.Pow(2, newPaletteSize + 1) - bOrigColorCount) * 3;
				possibleTextLength = possibleMessageLength - 3;
			}

			if (possibleTextLength < text.Length)
			{
				throw new Exception("Text is too big. Max text lenght for this image is " + possibleTextLength);
			}

			// set new palette property to the 10-th byte in the file
			BitArray newBsizeBits = new BitArray(new byte[] { (byte)newPaletteSize });
			bytes10[0] = newBsizeBits[0];
			bytes10[1] = newBsizeBits[1];
			bytes10[2] = newBsizeBits[2];
			bytes[10] = (byte)Encryptor.GetIntFromBitArray(bytes10);

			// create message array
			byte[] messageArray = new byte[possibleMessageLength];

			// create bit array from text length value and divide it into two arrays
			BitArray lengthBytes = new BitArray(new byte[] { (byte)text.Length });
			BitArray lowTextLengthByte = new BitArray(4);
			for (int i = 0; i < 4; i++)
			{
				lowTextLengthByte[i] = lengthBytes[i];
			}
			BitArray highTextLengthByte = new BitArray(4);
			for (int i = 0; i < 4; i++)
			{
				highTextLengthByte[i] = lengthBytes[i + 4];
			}

			// write bytes of check sequence and of two parts of message length
			messageArray[possibleMessageLength - 1] = (byte)Encryptor.GetIntFromBitArray(new BitArray(checkSequence));
			messageArray[possibleMessageLength - 2] = (byte)Encryptor.GetIntFromBitArray(new BitArray(highTextLengthByte));
			messageArray[possibleMessageLength - 3] = (byte)Encryptor.GetIntFromBitArray(new BitArray(lowTextLengthByte));

			// write text bytes
			byte[] textBytes = Encoding.Default.GetBytes(text);
			for (int i = 0; i < text.Length; i++)
			{
				messageArray[messageArray.Length - i - 4] = textBytes[i];
			}

			// write output file
			byte[] resultBytes = new byte[bytes.Length + messageArray.Length];
			int j = 0;
			for (int i = 0; i < 13 + 3 * bOrigColorCount; i++)
			{
				resultBytes[j] = bytes[i];
				j++;
			}
			for (int i = 0; i < messageArray.Length; i++)
			{
				resultBytes[j] = messageArray[i];
				j++;
			}
			for (int i = 13 + 3 * bOrigColorCount; i < bytes.Length; i++)
			{
				resultBytes[j] = bytes[i];
				j++;
			}

			return resultBytes;
		}

		public static byte[] Decrypt(string input)
		{
			// read bytes from input file
			byte[] bytes = File.ReadAllBytes(input);

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
			int possibleMessageLength = (int)Math.Pow(2, bsize + 1) * 3;
			int possibleTextLength = possibleMessageLength - 3;

			int n = 13;

			// read check sequence
			byte cs = bytes[n + bOrigColorCount * 3 - 1];
			if (cs != (byte)Encryptor.GetIntFromBitArray(new BitArray(checkSequence)))
			{
				return new byte[] { 0 };
			}

			// read two text length bytes and split them to one
			BitArray highTextLengthByte = new BitArray(new byte[] { bytes[n + bOrigColorCount * 3 - 2] });
			BitArray lowTextLengthByte = new BitArray(new byte[] { bytes[n + bOrigColorCount * 3 - 3] });
			BitArray textLengthByte = new BitArray(8);
			for (int i = 0; i < 4; i++)
			{
				textLengthByte[i] = lowTextLengthByte[i];
				textLengthByte[i + 4] = highTextLengthByte[i];
			}
			int textLength = (int)Encryptor.GetIntFromBitArray(new BitArray(textLengthByte));
			if (textLength < 0)
			{
				throw new Exception("Decoded text length is less than 0");
			}
			if (possibleTextLength < textLength)
			{
				throw new Exception("There is no encrypted message (Decoded message length (" + textLength + ") is less than Possible message length (" + possibleTextLength + "))");
			}

			// read text bytes
			byte[] bt = new byte[textLength];
			for (int i = 0; i < bt.Length; i++)
			{
				bt[i] = bytes[n + bOrigColorCount * 3 - 1 - i - 3];
			}

			return bt;
		}
	}

}
