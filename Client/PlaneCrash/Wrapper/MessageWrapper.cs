using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrapper
{
    [Serializable]
    public class MessageWrapper
    {
        public enum Phases
        {
            ACKNOWLEDGE,
            ATACK,
            HIT,
            LOSE
        }
        public int CellToHit { get; set; }
        public bool Atacker { get; set; }

        public bool PlanesReady { get; set; }
        public string ActivePlayer { get; set; }
        public Phases Phase { get; set; }
    }
}
