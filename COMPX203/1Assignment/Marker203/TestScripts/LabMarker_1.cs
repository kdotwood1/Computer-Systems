using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RexSimulator.Hardware;
using RexSimulator.Hardware.Wramp;

namespace COMP200Marker.TestScripts
{
    abstract class LabMarker_1 : LabMarker
    {
        public override string Version { get { return "19A.0.0"; } }

        private readonly int CLOCK_CYCLE_LIMIT = 50000;

        protected uint Encod(uint i)
        {
            return new uint[] { 0xA3u, 0x22u, 0x6Bu, 0x0Du, 0x49u, 0xC0u, 0x7Fu, 0xB8u, 0x31u }[i];
        }

        protected uint CountBits(uint i)
        {
            uint count = 0;
            while (i != 0)
            {
                count += i & 1u;
                i >>= 1;
            }
            return count;
        }

        protected bool Test(uint switchCombination, uint expected, RexBoard board)
        {
            board.Parallel.Switches = switchCombination;

            for (int i = 0; i < CLOCK_CYCLE_LIMIT; i++)
            {
                board.Tick();
            }

            
            //HACK: If they've turned off Hex decoding, modify the expected value
            if ((board.Parallel.Control & 1u) == 0u)
            {
                uint[] bitPattern = { 0x772F, 0x5B5B, 0x7D7C, 0x3F5E, 0x666F, 0x392F, 0x0771, 0x7C7F, 0x2F06 };
                uint leftExpected = (bitPattern[CountBits(switchCombination)] >> 8) & 0xFF;
                uint rightExpected = bitPattern[CountBits(switchCombination)] & 0xFF;

                if (board.Parallel.LeftSSDOut != leftExpected || board.Parallel.RightSSDOut != rightExpected)
                {
                    mMessage += string.Format("Incorrect SSD segments lit (Switches {0:X2}).\r\nWarning: SSD Hex decoding is disabled; it is recommended that you turn this on instead of setting segments on/off manually.\r\n", board.Parallel.Switches);
                    return false;
                }
            }
            else
            {
                uint result = board.Parallel.SSD;
                if (result != expected)
                {
                    mMessage += string.Format("SSD incorrect: was {0:X2}, expected {1:X2} (Switches {2:X2})\r\n", result, expected, board.Parallel.Switches);
                    return false;
                }
            }

            return true;
        }

        public override bool Mark(RexSimulator.Hardware.RexBoard mBoard)
        {
            //Allow some setup code to run
            for (int i = 0; i < 1e6; i++)
                mBoard.Tick();

            bool passed = true;
            for (uint i = 0; i <= 0xff; i++)
            {
                uint expected = GetExpected(i);
                passed &= Test(i, expected, mBoard);
            }
            return passed;
        }

        protected abstract uint GetExpected(uint i);
    }
}
