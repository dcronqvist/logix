using LogiX.Circuits.Drawables;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Simulation
{
    interface IUpdateable
    {
        public void Update(Simulator sim, Vector2 mousePosInWorld);
    }
}
