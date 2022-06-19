using System;
using System.IO;

namespace DDSUnswizzle
{
	public static class Unswizzler
	{
		/* Pixel Formats
		BC1/DXT1 = 71
		BC2/DXT3 = 74
		BC3/DXT5 = 77
		BC4 = 80
		BC5 = 83
		BC5U = 83
		BC5S = 84
		BC6 = 95
		BC7 = 98
		*/

		private static readonly int[] FormatBits = new int[] {
		0, 128, 128, 128, 128, 96, 96, 96, 96,
		64, 64, 64, 64, 64, 64, 64, 64, 64, 64,
		64, 64, 64, 64, 32, 32, 32, 32, 32, 32,
		32, 32, 32, 32, 32, 32, 32, 32, 32, 32,
		32, 32, 32, 32, 32, 32, 32, 32, 32, 16,
		16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
		16, 8, 8, 8, 8, 8, 8, 1, 32, 32, 32, 4,
		4, 4, 8, 8, 8, 8, 8, 8, 4, 4, 4, 8, 8,
		8, 16, 16, 32, 32, 32, 32, 32, 32, 32,
		8, 8, 8, 8, 8, 8, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 16
		};

		private static int GetBlockPos(int x, int y, int BlockCount, int BlockSize)
		{
			int Val = 1;
			for (int i = 0; i < 3; i++)
            {
				Val = Val * 2;
				int Val1 = (x / Val) + (y / (Val / 2)) * (BlockCount / Val);
				int Val2 = (Val1 % (2 * (BlockCount / Val))) / 2 + (Val1 % (2 * (BlockCount / Val))) % 2 * (BlockCount / Val);
				int Val3 = Val1 / (2 * (BlockCount / Val)) * (2 * (BlockCount / Val)) + Val2;
				x = (Val3 % (BlockCount / Val)) * Val + (x % Val);
				y = (Val3 / (BlockCount / Val)) * (Val / 2) + (y % (Val / 2));
			}

			return BlockSize * (y * BlockCount + x);
		}

		public static int Unswizzle(byte[] input, out byte[] output, int Format, int Width, int Height)
		{
			try
            {
				using (MemoryStream stream = new(input))
				{
					using (BinaryReader br = new(stream))
					{
						int Length = input.Length;
						int BlockSize = FormatBits[Format] * 2;
						int FinalLength = Width * Height * FormatBits[Format] / 8;

						output = new byte[FinalLength];
						int VerticalBlockCount = Height / 4;
						int HorizontalBlockCount = Width / 4;
						for (int i = 0; i < VerticalBlockCount; i++)
						{
							for (int ii = 0; ii < HorizontalBlockCount; ii++)
							{
								Array.Copy(br.ReadBytes(BlockSize), 0, output, GetBlockPos(ii, i, HorizontalBlockCount, BlockSize), BlockSize);
							}
						}
					}
				}

				return 1;
			}
			catch
            {
				output = new byte[0];
				return 0;
            }
		}
	}
}
