using System;
using System.Collections.Generic;
using System.Text;
using RexSimulator.Hardware;
using RexSimulator.Hardware.Wramp;

namespace COMP200Marker.TestScripts
{
    class LabMarker_2_3 : LabMarker_2
    {
        public override string Version { get { return "19A.1.0"; } }

        public override bool Mark(RexBoard board)
        {
            Initialise(board);

            for (int c = 0; c < TEST_CASES.Length; c++)
            {
                if (!TryTestCase(board, TEST_CASES[c], false))
                {
                    return false;
                }
            }
            return true;
        }
    }
}