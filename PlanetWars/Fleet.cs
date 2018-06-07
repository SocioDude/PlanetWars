using System;

namespace PlanetWars
{
    public class Fleet
    {
        public int Owner { get; }
        public int Ships { get; }
        public int Source { get; }
        public int Destination { get; }
        public int TotalTurns { get; }
        public int RemainingTurns { get; set; }

        public Fleet(string fleet)
        {
            string[] components = fleet.Split(' ');
            if (components.Length != 7 || components[0] != "F")
            {
                throw new Exception("Bad fleet line format.  Line = " + fleet);
            }

            int owner;
            if (!int.TryParse(components[1], out owner))
            {
                throw new Exception("Bad fleet line format.  Line = " + fleet);
            }
            Owner = owner;

            int ships;
            if (!int.TryParse(components[2], out ships))
            {
                throw new Exception("Bad fleet line format.  Line = " + fleet);
            }
            Ships = ships;

            int source;
            if (!int.TryParse(components[3], out source))
            {
                throw new Exception("Bad fleet line format.  Line = " + fleet);
            }
            Source = source;

            int destination;
            if (!int.TryParse(components[4], out destination))
            {
                throw new Exception("Bad fleet line format.  Line = " + fleet);
            }
            Destination = destination;

            int totalTurns;
            if (!int.TryParse(components[5], out totalTurns))
            {
                throw new Exception("Bad fleet line format.  Line = " + fleet);
            }
            TotalTurns = totalTurns;

            int remainingTurns;
            if (!int.TryParse(components[6], out remainingTurns))
            {
                throw new Exception("Bad fleet line format.  Line = " + fleet);
            }
            RemainingTurns = remainingTurns;
        }

        public Fleet(int owner, int ships, int source, int destination, int totalTurns, int remainingTurns)
        {
            Owner = owner;
            Ships = ships;
            Source = source;
            Destination = destination;
            TotalTurns = totalTurns;
            RemainingTurns = remainingTurns;
        }

        public string ToPlayerTwoString()
        {
            int transformedOwner = Owner == 0 ? 0 : Owner == 1 ? 2 : 1;
            return $"F {transformedOwner} {Ships} {Source} {Destination} {TotalTurns} {RemainingTurns}";
        }

        public override string ToString()
        {
            return $"F {Owner} {Ships} {Source} {Destination} {TotalTurns} {RemainingTurns}";
        }
    }
}
