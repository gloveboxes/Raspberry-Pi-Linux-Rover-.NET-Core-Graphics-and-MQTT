// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Spi;
using System.Drawing;
using Glovebox.Graphics.Interfaces;

namespace Glovebox.Graphics.Drivers.NeoPixel
{
    public class Ws2812b : Ws28xx, ILedDriver
    {
        BitmapImage _image;
        private int _width;
        private int _height;

        public int PanelsPerFrame { get; set; }

        public Ws2812b(SpiDevice spiDevice, int width, int height = 1, int panelsPerFrame = 1)
            : base(spiDevice, width, height)
        {
            Image = new BitmapImageNeo3(width, height, panelsPerFrame);
            _image = Image;
            _width = width;
            _height = height;
            PanelsPerFrame = panelsPerFrame;
        }

        // public int PanelsPerFrame => throw new System.NotImplementedException();

        public LedDriver.BlinkRate Blink { set => throw new System.NotImplementedException(); }
        public byte Brightness { set => throw new System.NotImplementedException(); }

        public void SetFrameState(LedDriver.Display state)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Write(Color[] frame)
        {
            for (int col = 0; col < _width * PanelsPerFrame; col++)
            {
                for (int row = 0; row < _height; row++)
                {
                    int pos = col * 8 + row;
                    _image.SetPixel(row, col, frame[pos]);
                }                
            }
            Update();
        }
    }
}