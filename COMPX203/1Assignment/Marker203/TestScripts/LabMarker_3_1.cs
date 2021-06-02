using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COMP200Marker.TestScripts
{
    class LabMarker_3_1 : LabMarker_3
    {
        public override string Version { get { return "19A.0.0"; } }

        public override bool Mark(RexSimulator.Hardware.RexBoard mBoard)
        {
            base.Mark(mBoard);
            return RunSerialTestCase("", "", "", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", mBoard);
        }
    }
}
