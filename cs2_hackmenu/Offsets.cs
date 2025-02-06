using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cs2_cheat;

namespace cs2_hackmenu
{
    public class Offsets
    {

        //bases
        public int viewAngle = 0x19CB708;
        public int viewmatrix = 0x19BE600;
        public int localplayer = 0x17C9468;
        public int entitylist = 0x17D6B60;

        // features
        public int team = 0x3c3;
        public int jumpflag = 0x3cc;
        public int health = 0x324;
        public int origin = 0xD58;
    }
}
