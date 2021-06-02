using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COMP200Marker.TestScripts
{
    class LabMarker_3_4 : LabMarker_3
    {
        public override string Version { get { return "19A.0.0"; } }

        public override bool Mark(RexSimulator.Hardware.RexBoard mBoard)
        {
            //initialise
            base.Mark(mBoard);

            //Allow some setup code to run
            for (int i = 0; i < 1e6; i++)
                mBoard.Tick();

            uint[] testCases = { 
                0x0000, 0xffff, 0x5a5a, 0xa5a5, 0x1f1f, 0xf4f4,
                0x000F, 0x00F0, 0x0F00, 0xF000, 0x00FF, 0xFF00,
                0x0000, 0xFFFF, 0x0F0F, 0xF0F0, 0xA5A5, 0x5A5A,
                0xdead, 0xbeef, 0xf00d, 0x1234, 0xF080, 0x80F0,
            };

            // First third
            for (int i = 0; i < 8; i++)
            {
                if (!RunParallelTestCase(mBoard, testCases[i]))
                    return false;
            }

            if (!RunSerialTestCase("abc123ABCXYZ@[ ]!$<>CoMp200cOmPx203", "", "abc******************o*p***c*m*x***", "", mBoard))
                return false;

            // Second third
            for (int i = 8; i < 16; i++)
            {
                if (!RunParallelTestCase(mBoard, testCases[i]))
                    return false;
            }

            if (!RunSerialTestCase("COMPX203TestSerial", "", "*********est*erial", "", mBoard))
                return false;

            // Last third
            for (int i = 16; i < 24; i++)
            {
                if (!RunParallelTestCase(mBoard, testCases[i]))
                    return false;
            }

            //Exit button
            mBoard.Parallel.Buttons = 4;
            Tick(mBoard);
            if (mBoard.CPU.PC < 0x80000)
            {
                mMessage += "Program did not properly terminate when button 3 was pressed.\r\n";
                return false;
            }

            return true;
        }
    }
}
