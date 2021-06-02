using System;
using System.Collections.Generic;
using System.Text;
using RexSimulator.Hardware;
using RexSimulator.Hardware.Wramp;

namespace COMP200Marker.TestScripts
{
    class LabMarker_2_1 : LabMarker_2
    {
        public override string Version { get { return "19A.1.0"; } }

        public override bool Mark(RexBoard mBoard)
        {
            Console.WriteLine("This might take a while...");
            Initialise(mBoard);
            for (uint i = 0; i <= 0xffff; i++)
            {
                ResetBoard(mBoard);

                // Flip some switches
                mBoard.Parallel.Switches = i;

                // Give a nice progress indicator
                if (i % (0xffff / 100) == 0)
                {
                    Console.Write($"\r{i / (0xffff / 100)}%");
                }

                // Let the program run
                bool passed = RunSerialTestCase("", "", $"{i:D5}", "", mBoard);
                if (!passed) return false;
            }
            Console.WriteLine();
            return true;
        }
    }
}
