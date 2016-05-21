using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaneCrash
{
    [Serializable]
    public class Message
    {
        public int CellToHit { get; set; }
        public bool Atacker { get; set; }

        public bool PlanesReady { get; set; }
    }
}
