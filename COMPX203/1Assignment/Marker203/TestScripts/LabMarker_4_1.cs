using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RexSimulator.Hardware.Wramp;
using RexSimulator.Hardware;

namespace COMP200Marker.TestScripts
{
    class LabMarker_4_1 : LabMarker_4
    {
        public override string Version { get { return "19A.1.1"; } }

        protected override uint ExpectedCCTRL
        {
            get { return 0xaa; }
        }

        protected override bool DoSubTest(RexBoard mBoard)
        {
            //Check that the SSDs are showing the right value
            if (mBoard.Parallel.SSD != 0x00)
            {
                mMessage += "Before pressing the user interrupt button, SSDs did not show '00'\r\n";
                return false;
            }

            //Press the user interrupt button 10 times, see where it gets you.
            for (int i = 1; i < 10; i++)
            {
                mBoard.InterruptButton.PressButton();
                Wag(mBoard);
                if ((mBoard.Parallel.LeftSSD & 0xf) * 10 + (mBoard.Parallel.RightSSD & 0xf) != i)
                {
                    mMessage += string.Format("SSDs showed \"{0:X2}\" after pressing the user interrupt button {1} times. Expected SSD to show \"{1:D2}\"\r\n", mBoard.Parallel.SSD, i);
                    return false;
                }
                if (mBoard.InterruptButton.InterruptAck != 0)
                {
                    mMessage += "User Interrupt Button was not acknowledged.";
                    return false;
                }
            }

            // Press the other buttons some, see what happens.
            for (int i = 10; i < 100; i++)
            {
                mBoard.Parallel.Buttons = (uint)(1 << (i % 3));
                Wag(mBoard);
                mBoard.Parallel.Buttons = 0;
                Wag(mBoard);
                if ((mBoard.Parallel.LeftSSD & 0xf) * 10 + (mBoard.Parallel.RightSSD & 0xf) != i)
                {
                    mMessage += string.Format("SSDs showed \"{0:X2}\" after pressing the parallel buttons {1} times. Expected SSD to show \"{2:D2}\"\r\n", mBoard.Parallel.SSD, i - 10, i);
                    return false;
                }
                if (mBoard.Parallel.InterruptAck != 0)
                {
                    mMessage += "Parallel Interrupts were not acknowledged.";
                    return false;
                }
            }
            
            return true;
        }
    }
}
