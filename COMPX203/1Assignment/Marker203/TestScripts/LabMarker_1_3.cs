using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RexSimulator.Hardware;

namespace COMP200Marker.TestScripts
{
    class LabMarker_1_3 : LabMarker_1
    {
        public override string Version { get { return "19A.0.0"; } }

        protected override uint GetExpected(uint i)
        {
            return Encod(CountBits(i));
        }
    }
}
