using System.Drawing;
using System.Threading;

namespace Glovebox.Graphics
{


    /// <summary>
    /// Frame primitives - generic across Rings, Stips and Grids
    /// </summary>
    public class FrameBase
    {

        private readonly int ColorCount;

        public int Length
        {
            get { return ColorCount; }
        }

        public Color[] Frame { get; set; }

        private Color[] blinkFrame;
        public FrameBase(int _ColorCount)
        {
            ColorCount = _ColorCount;
            Frame = new Color[ColorCount];

            //lazy initialize in the black/blank in the blink method
            blinkFrame = new Color[ColorCount];

            // init frame to all black - specifically not null
            FrameClear();
        }

        #region Primitive Frame Manipulation Methods
        public void FrameClear()
        {
            FrameSet(Color.Black);
        }

        /// <summary>
        /// Fill entire frame with one colour
        /// </summary>
        /// <param name="Color"></param>
        public void FrameSet(Color Color)
        {
            for (int i = 0; i < Frame.Length; i++)
            {
                Frame[i] = Color;
            }
        }

        /// <summary>
        /// Fill entire frame with one colour
        /// </summary>
        /// <param name="Color"></param>
        public virtual void FrameSet(Color Color, int position)
        {
            if (position < 0) { return; }

            Frame[position % Length] = Color;
        }

        /// <summary>
        /// set specific frame Colors a colour - useful for letters on grids, patterns etc
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="ColorPos"></param>
        public void FrameSet(Color colour, int[] ColorPos)
        {
            for (int i = 0; i < ColorPos.Length; i++)
            {
                if (ColorPos[i] < 0 || ColorPos[i] >= Frame.Length) { continue; }
                Frame[ColorPos[i]] = colour;
            }
        }

        /// <summary>
        /// set specific frame Colors from a rolling palette of colours
        /// </summary>
        /// <param name="ColorPos"></param>
        /// <param name="palette"></param>
        public void FrameSet(Color[] palette, int[] ColorPos)
        {
            for (int i = 0; i < ColorPos.Length; i++)
            {
                if (ColorPos[i] < 0 || ColorPos[i] >= Frame.Length) { continue; }
                Frame[ColorPos[i]] = palette[i % palette.Length];
            }
        }

        /// <summary>
        /// fill frame Colors from a specified position and repeat 
        /// </summary>
        /// <param name="Color"></param>
        /// <param name="startPos"></param>
        /// <param name="repeat"></param>
        public void FrameSet(Color Color, int startPos, int repeat = 1)
        {
            if (startPos < 0 | repeat < 0) { return; }

            for (int i = startPos, r = 0; r < repeat; i++, r++)
            {
                Frame[i % Frame.Length] = Color;
            }
        }

        /// <summary>
        /// fill frame Colors from a specified position and repeat from a palette of colours
        /// </summary>
        /// <param name="Color"></param>
        /// <param name="startPos"></param>
        /// <param name="repeat"></param>
        public void FrameSet(Color[] Color, int startPos, int repeat = 1)
        {
            if (startPos < 0 | repeat < 0) { return; }

            for (int i = startPos, r = 0; r < repeat; i++, r++)
            {
                Frame[i % Frame.Length] = Color[i % Color.Length];
            }
        }

        /// <summary>
        /// fill frame from a rolling pallet
        /// </summary>
        /// <param name="palette"></param>
        public void FrameSet(Color[] palette)
        {
            for (int i = 0; i < Frame.Length; i++)
            {
                Frame[i] = palette[i % palette.Length];
            }
        }

        /// <summary>
        /// fill frame with blocks of colour from a palette
        /// </summary>
        /// <param name="palette"></param>
        public void FrameSetBlocks(Color[] palette)
        {
            if (palette == null || palette.Length == 0)
            {
                FrameClear();
            }
            else if (palette.Length >= ColorCount)
            {
                FrameSet(palette);
            }
            else
            {
                var leftovers = ColorCount % palette.Length;
                int leftoversUsed = 0;
                int thisColor = 0;
                uint baseBlockSize = (uint)(ColorCount / palette.Length);
                for (int i = 0; i < palette.Length; i++)
                {
                    for (int j = 0; j < baseBlockSize; j++)
                    {
                        Frame[thisColor] = palette[i];
                        thisColor++;
                    }
                    if (leftoversUsed < leftovers)
                    {
                        Frame[thisColor] = palette[i];
                        thisColor++;
                        leftoversUsed++;
                    }
                }
            }
        }


        /// <summary>
        /// Swap specified Colors with wrap
        /// </summary>
        /// <param name="Color1"></param>
        /// <param name="Color2"></param>
        public void FrameColorSwap(int Color1, int Color2)
        {
            if (Color1 < 0 | Color2 < 0) { return; }

            Color temp = Frame[Color2 % ColorCount];
            Frame[Color2 % ColorCount] = Frame[Color1 % ColorCount];
            Frame[Color1 % ColorCount] = temp;
        }

        public void FrameColorForward(int ColorIndex, int stepSize = 1)
        {
            if (ColorIndex < 0 | stepSize < 0) { return; }

            if (ColorIndex >= Frame.Length) { return; }

            int length = Frame.Length;
            int newIndex = (ColorIndex + stepSize) % length;

            Color p = Frame[newIndex];
            Frame[newIndex] = Frame[ColorIndex];
            Frame[ColorIndex] = p;
        }


        /// <summary>
        /// Shift wrap forward a block of Colors by specified amount
        /// </summary>
        /// <param name="blockSize"></param>
        public void FrameShiftForward(int blockSize = 1)
        {
            if (blockSize < 0) { return; }

            blockSize = blockSize % Length;

            int i;
            Color[] temp = new Color[blockSize];

            for (i = 0; i < blockSize; i++)
            {
                temp[i] = Frame[Frame.Length - blockSize + i];
            }

            for (i = Frame.Length - 1; i >= blockSize; i--)
            {
                Frame[i] = Frame[i - blockSize];
            }

            for (i = 0; i < blockSize; i++)
            {
                Frame[i] = temp[i];
            }
        }

        /// <summary>
        /// Shift wrap forward a block of Colors by specified amount
        /// </summary>
        /// <param name="blockSize"></param>
        public void FrameShiftBack(int blockSize = 1)
        {
            if (blockSize < 0) { return; }

            blockSize = blockSize % Length;

            int i;
            Color[] temp = new Color[blockSize];

            for (i = 0; i < blockSize; i++)
            {
                temp[i] = Frame[i];
            }

            for (i = blockSize; i < Frame.Length; i++)
            {
                Frame[i - blockSize] = Frame[i];
            }

            for (i = 0; i < blockSize; i++)
            {
                int p = Frame.Length - blockSize + i;
                Frame[p] = temp[i];
            }
        }


        /// <summary>
        /// cycle the Colors moving them up by increment Colors
        /// </summary>
        /// <param name="increment">number of positions to shift. Negative numbers backwards. If this is more than the number of LEDs, the result wraps</param>
        public void FrameShift(int increment = 1)
        {
            if (increment > 0) { FrameShiftForward(increment); }
            else if (increment < 0) { FrameShiftBack(System.Math.Abs(increment)); }
        }

        /// <summary>
        /// Forces an update with the current contents of currentDisplay
        /// </summary>
        public void FrameDraw()
        {
            FrameDraw(Frame);
        }

        protected virtual void FrameDraw(Color[] frame)
        {
        }

        #endregion

        #region Higher Level Display Methods

        /// <summary>
        /// move a singel Color around (or along) the ring (or strip) - always starts at position 0
        /// </summary>
        /// <param name="ColorColour">Colour of the Color to show</param>
        /// <param name="cycles">Number of whole cycles to rotate</param>
        /// <param name="stepDelay">Delay between steps (ms)</param>
        public void SpinColour(Color ColorColour, int cycles = 1, int stepDelay = 250)
        {
            SpinColourOnBackground(ColorColour, Color.Black, cycles, stepDelay);
        }

        public void SpinColourOnBackground(Color ColorColour, Color backgroundColour, int cycles = 1, int stepDelay = 250)
        {
            if (cycles < 0 || stepDelay < 0) { return; }

            FrameSet(backgroundColour);
            FrameSet(ColorColour, new int[] { 0 });

            FrameDraw();

            for (int i = 0; i < cycles; i++)
            {
                for (int j = 0; j < ColorCount; j++)
                {
                    FrameShift();
                    FrameDraw();
                    // Util.Delay(stepDelay);
                    Thread.Sleep(stepDelay);
                }
            }
        }

        protected void FrameBlink(int blinkDelay, int repeat)
        {
            if (blinkDelay < 0 || repeat < 0) { return; }

            if (blinkFrame[0] == null)
            {
                for (int i = 0; i < blinkFrame.Length; i++)
                {
                    blinkFrame[i] = Color.Black;
                }
            }

            for (int i = 0; i < repeat; i++)
            {
                // Util.Delay(blinkDelay);
                Thread.Sleep(blinkDelay);
                FrameDraw(blinkFrame);
                Thread.Sleep(blinkDelay);
                // Util.Delay(blinkDelay);
                FrameDraw();
            }
        }
        #endregion
    }
}
