using System;
using System.Drawing;
using Glovebox.Graphics.Drivers;
using Glovebox.Graphics.Interfaces;
using Glovebox.Graphics.LedType;

namespace Glovebox.Graphics.Components
{
    public class SevenSegmentDisplay : SevenSegmentDisplayBase, ILedDriver
    {
        private ILedDriver _driver;

        public SevenSegmentDisplay(ILedDriver driver) : base("ssd", driver.PanelsPerFrame)
        {
            _driver = driver;
        }

        public int PanelsPerFrame
        {
            get
            {
                return _driver.PanelsPerFrame;
            }
        }

        public byte Brightness { set { _driver.Brightness = value; } }

        public LedDriver.BlinkRate Blink { set {} }

        public void SetFrameState(LedDriver.Display state)
        {
            _driver.SetFrameState(state);
        }

        public void Write(Color[] frame)
        {
            _driver.Write(frame);
        }

        protected override void FrameDraw(Color[] frame)
        {
            _driver.Write(frame);
        }
    }
}
