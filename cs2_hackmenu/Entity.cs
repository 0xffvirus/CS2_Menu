using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace cs2_hackmenu
{
    public class Entity
    {
        public IntPtr address { get; set; }
        public int Health { get; set; }
        public int teamNum { get; set; }
        public int jumpFlag {  get; set; }  
        public float magnitude { get; set; }
        public float angleDifference { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 viewOffset { get; set; }
        public Vector3 abs { get; set; }    
        public Vector2 ScreenPosition { get; set; }
        public Vector2 absScreenPosition { get; set; }


    }
}
