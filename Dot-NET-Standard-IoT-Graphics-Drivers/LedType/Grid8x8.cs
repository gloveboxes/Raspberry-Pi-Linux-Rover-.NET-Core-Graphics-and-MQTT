using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Glovebox.Graphics;
using Glovebox.Graphics.Font;

namespace Glovebox.Graphics.LedType
{
    public class Grid8x8 : GridBase
    {

        public IFont Font { get; set; }

        public Grid8x8(string name, IFont font, int panels = 1)
            : base(8, 8, panels)
        {
            Font = font;
        }

        #region Scroll string primitives

        public void ScrollStringInFromRight(string characters, int delayMilliseconds = 100)
        {
            ScrollStringInFromRight(characters, delayMilliseconds, new Color[] { Color.White });
        }

        public void ScrollStringInFromLeft(string characters, int delayMilliseconds = 100)
        {
            ScrollStringInFromLeft(characters, delayMilliseconds, new Color[] { Color.White });
        }

        public void ScrollStringInFromRight(string characters, int delayMilliseconds, Color[] Color)
        {
            ushort cycleColor = 0;

            // loop through each character
            for (int ch = 0; ch < characters.Length; ch++)
            {
                char character = characters.Substring(ch, 1)[0];
                ScrollBitmapInFromRight(Font[character], delayMilliseconds, Color[cycleColor % Color.Length]);
                cycleColor++;
            }
        }

        public void ScrollStringInFromLeft(string characters, int delayMilliseconds, Color[] Color)
        {
            ushort cycleColor = 0;

            // loop through each character
            for (int ch = characters.Length - 1; ch >= 0; ch--)
            {

                char character = characters.Substring(ch, 1)[0];
                if (character >= ' ' && character <= 'z')
                {
                    ScrollBitmapInFromLeft(Font[character], delayMilliseconds, Color[cycleColor % Color.Length]);
                    cycleColor++;
                }
            }
        }

        #endregion

        #region Scroll Character primitives

        public void ScrollCharacterFromRight(char character, int delayMilliseconds)
        {
            ScrollCharacterFromRight(character, delayMilliseconds, Color.White);
        }


        public void ScrollCharacterFromRight(char character, int delayMilliseconds, Color Color)
        {
            ScrollBitmapInFromRight(Font[character], delayMilliseconds, Color);
        }

        public void ScrollCharacterFromLeft(char character, int delayMilliseconds)
        {
            ScrollCharacterFromLeft(character, delayMilliseconds, Color.White);
        }

        public void ScrollCharacterFromLeft(char character, int delayMilliseconds, Color Color)
        {
            if (character >= ' ' && character <= 'z')
            {
                ScrollBitmapInFromLeft(Font[character], delayMilliseconds, Color);
            }
        }

        #endregion


        #region Scroll Bitmaps left and right

        public virtual void ScrollBitmapInFromRight(IReadOnlyList<byte> charBytes, int delayMilliseconds, Color Color)
        {
            int pos = 0;
            bool pixelFound = false;

            for (int col = 0; col < charBytes.Count; col++)
            {
                if (charBytes[col] == 0)
                {
                    DrawShiftLeft(delayMilliseconds);
                }
                else
                {
                    for (int row = 0; row < RowsPerPanel; row++)
                    {
                        pos = ColumnsPerPanel + (row * ColumnsPerPanel) - 1;

                        if ((charBytes[col] & (byte)(1 << row)) != 0)
                        {
                            FrameSet(Color, (int)pos, (int)(NumberOfPanels - 1));
                            pixelFound = true;
                        }
                    }
                    DrawShiftLeft(delayMilliseconds);
                }
            }
            if (pixelFound)   //add space between characters
            {
                DrawShiftLeft(delayMilliseconds);
            }
        }

        private void DrawShiftLeft(int delayMilliseconds)
        {
            if (delayMilliseconds > 0)
            {
                FrameDraw();
                Thread.Sleep(delayMilliseconds);
                // Util.Delay(delayMilliseconds);
            }
            FrameShiftLeft();
        }

        public virtual void ScrollBitmapInFromLeft(IReadOnlyList<byte> charBytes, int delayMilliseconds, Color Color)
        {
            int pos = 0;
            bool pixelFound = false;

            for (int col = (int)charBytes.Count - 1; col >= 0; col--)
            {
                if (charBytes[col] == 0)
                {
                    DrawShiftRight(delayMilliseconds);
                }
                else
                {
                    {
                        for (int row = 0; row < RowsPerPanel; row++)
                        {
                            pos = (int)ColumnsPerPanel * row;
                            if ((charBytes[col] & (byte)(1 << row)) != 0)
                            {
                                FrameSet(Color, pos, 0);
                                pixelFound = true;
                            }
                        }
                        DrawShiftRight(delayMilliseconds);
                    }
                }

            }
            if (pixelFound)   //add space between characters
            {
                DrawShiftRight(delayMilliseconds);
            }
        }

        private void DrawShiftRight(int delayMilliseconds)
        {
            if (delayMilliseconds > 0)
            {
                FrameDraw();
                Thread.Sleep(delayMilliseconds);
                // Util.Delay(delayMilliseconds);
            }
            FrameShiftRight();
        }

        #endregion

        #region Draw Primitives


        public void DrawString(string characters, int delayMilliseconds, int panel = 0)
        {
            DrawString(characters, new Color[] { Color.White }, delayMilliseconds, panel);
        }

        public void DrawString(string characters, Color[] Color, int delayMilliseconds, int panel = 0)
        {
            ushort cycleColor = 0;
            char c;
            for (int i = 0; i < characters.Length; i++)
            {
                c = characters.Substring(i, 1)[0];
                if (c >= ' ' && c <= 'z')
                {
                    DrawLetter(c, Color[cycleColor % Color.Length], panel);
                    FrameDraw();
                    // Util.Delay(delayMilliseconds);
                    Thread.Sleep(delayMilliseconds);
                    cycleColor++;
                }
            }
        }

        public void DrawLetter(char character, int panel = 0)
        {
            DrawLetter(character, Color.White, panel);
        }

        public void DrawLetter(char character, Color Color, int panel = 0)
        {
            // ulong letter = 0;
            IReadOnlyList<byte> charBytes;
            charBytes = Font[character];
            DrawBitmap(charBytes, Color, panel);
        }

        private void DrawBitmap(IReadOnlyList<byte> charBytes, Color color, int panel)
        {
            if (panel < 0 || panel >= NumberOfPanels) { return; }

            for (int col = 0; col < charBytes.Count; col++)
            {
                for (int row = 0; row < 8; row++)
                {
                    FrameSet((charBytes[col] & (byte)(1 << row)) == 0 ? Color.Black : color, row * 8 + col, panel);
                }
            }
        }
        #endregion
    }
}
