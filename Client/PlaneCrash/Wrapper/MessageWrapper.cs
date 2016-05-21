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
            SENDIP,
            ACKNOWLEDGE,
            ATACK,
            HIT,
            LOSE
        }
        public int CellToHit { get; set; }
        public bool PlanesReady { get; set; }
        public string ActivePlayer { get; set; }
        public Phases Phase { get; set; }
        public string YourName { get; set; }

        public int[] Points = new int[8];

        public bool IsPlaneHit { get; set; }
    }
}
