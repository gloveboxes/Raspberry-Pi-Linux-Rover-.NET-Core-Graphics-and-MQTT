using Glovebox.Graphics.Drivers;
using Glovebox.Graphics.Font;
using Glovebox.Graphics.Interfaces;
using Glovebox.Graphics.LedType;
using System;
using System.Drawing;

namespace Glovebox.Graphics.Components
{
    public class LED8x8Matrix : Grid8x8, ILedDriver
    {

        private readonly ILedDriver _driver;


        public LED8x8Matrix(ILedDriver driver, IFont font) : base("matrix", font, driver.PanelsPerFrame)
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

        public byte Brightness
        {
            set { _driver.Brightness = value; }
        }

        public LedDriver.BlinkRate Blink
        {
            set
            {
                _driver.Blink = value;
            }
        }

        public void SetFrameState(LedDriver.Display state)
        {
            _driver.SetFrameState(state);
        }

        public void Write(Color[] frame)
        {
            // never called - implementation is overridden
            throw new NotImplementedException();
        }

        protected override void FrameDraw(Color[] frame)
        {
            _driver.Write(frame);
        }
    }
}
