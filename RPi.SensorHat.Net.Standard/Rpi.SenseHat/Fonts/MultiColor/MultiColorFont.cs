////////////////////////////////////////////////////////////////////////////
//
//  This file is part of Rpi.SenseHat
//
//  Copyright (c) 2017, Mattias Larsson
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to use,
//  copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
//  Software, and to permit persons to whom the Software is furnished to do so,
//  subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
//  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Emmellsoft.IoT.Rpi.SenseHat.Fonts.MultiColor
{
	public class MultiColorFont : Font<MultiColorCharacter>
	{
		public MultiColorFont(IEnumerable<MultiColorCharacter> chars)
			: base(chars)
		{
		}

		//TODO DGLOVER
		// public static async Task<MultiColorFont> LoadFromImage(
		// 	Uri fontImageUri,
		// 	string symbols,
		// 	Color? transparencyColor = null)
		// {
		// 	Color[,] pixels = await PixelSupport.GetPixels(fontImageUri).ConfigureAwait(false);

		// 	return LoadFromImage(pixels, symbols, transparencyColor);
		// }

		public static MultiColorFont LoadFromImage(
			Image image,
			string symbols,
			Color? transparencyColor = null)
		{
			if (image.Height > 9)
			{
				throw new ArgumentException("The image must not be taller than 9 pixels high!");
			}

			var chars = new List<MultiColorCharacter>();

			int symbolIndex = 0;

			int bitmapX = 0;
			char currentSymbol = ' ';
			int charStartX = 0;

			int charHeight = image.Height - 1;

			while (bitmapX < image.Width)
			{
				bool isBeginningOfChar = (image[bitmapX, 0].R < 128);
				bool isLastX = (bitmapX == image.Width - 1);

				if (isBeginningOfChar || isLastX)
				{
					if ((bitmapX > 0) || isLastX)
					{
						int charWidth = bitmapX - charStartX;

						if (isLastX)
						{
							charWidth++;
						}

						Image charPixels = new Image(charWidth, charHeight);
						for (int y = 0; y < charHeight; y++)
						{
							for (int x = 0; x < charWidth; x++)
							{
								charPixels[x, y] = image[charStartX + x, 1 + y];
							}
						}

						var c = new MultiColorCharacter(currentSymbol, charPixels, transparencyColor);
						chars.Add(c);
					}

					if (symbolIndex < symbols.Length)
					{
						currentSymbol = symbols[symbolIndex++];
						charStartX = bitmapX;
					}
					else if (bitmapX < image.Width - 1)
					{
						throw new ArgumentException("Too few chars in the symbols-string!");
					}
				}

				bitmapX++;
			}

			return new MultiColorFont(chars);
		}
	}
}
