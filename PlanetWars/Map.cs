using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * TODOs:
 *   * Keep replay data internally, rather than the outer program needing to assemble it.
 *     - Ensure that we don't keep the replay of the final turn.  It may have partially processed.
 *   * Unit tests.
 *   * Figure out source control solution.
 */

namespace PlanetWars
{
    public class Map
    {
        public static readonly int MAX_TURNS = 200;
        public int TurnNumber = 0;

        public string PlayerOneError = "";
        public string PlayerTwoError = "";

        public List<Planet> Planets { get; }
        public List<Fleet> Fleets { get; }

        public Map(List<string> map)
        {
            Planets = new List<Planet>();
            Fleets = new List<Fleet>();
            for (int i = 0; i < map.Count; i++)
            {
                string line = map[i];
                if (line[0] == 'F')
                {
                    Fleets.Add(new Fleet(line));
                }
                else if (line[0] == 'P')
                {
                    Planets.Add(new Planet(line, i));
                }
                else
                {
                    throw new Exception("Bad map line format.  Line = " + line);
                }
            }
        }

        public List<string> GetPlayerTwoState()
        {
            List<string> results = Planets.Select(x => x.ToPlayerTwoString()).ToList();
            results.AddRange(Fleets.Select(x => x.ToPlayerTwoString()));
            return results;
        }

        public List<string> GetPlayerOneState()
        {
            List<string> results = Planets.Select(x => x.ToString()).ToList();
            results.AddRange(Fleets.Select(x => x.ToString()));
            return results;
        }

        public override string ToString()
        {
            List<string> results = Planets.Select(x => x.ToString()).ToList();
            results.AddRange(Fleets.Select(x => x.ToString()));
            return string.Join("\n", GetPlayerOneState());
        }

        private Order ConstructAndValidateOrder(string input, int playerId)
        {
            string error;
            Order order = Order.TryParse(input, playerId, out error);
            if (order == null)
            {
                SetErrorMessage(playerId, error);
                return null;
            }

            Planet sourcePlanet = Planets.FirstOrDefault(x => x.Identifier == order.Source);
            Planet destinationPlanet = Planets.FirstOrDefault(x => x.Identifier == order.Destination);

            if (sourcePlanet == null || destinationPlanet == null)
            {
                SetErrorMessage(playerId, $"Sending ships to or from a nonexistent planet.  Order = {input}");
            }

            if (order.Source == order.Destination)
            {
                SetErrorMessage(playerId, $"Sending ships from one planet to identical planet.  Order = {input}");
                return null;
            }
            
            if (sourcePlanet == null || destinationPlanet == null)
            {
                SetErrorMessage(playerId, $"Tried to reference an unknown planet.  Order = {input}");
                return null;
            }

            if (sourcePlanet.Ships < order.Ships)
            {
                SetErrorMessage(playerId, "Tried to send more ships than planet had.  " +
                    $"Order = {input}. Planet ship count = {sourcePlanet.Ships}");
                return null;
            }

            if (order.Ships <= 0)
            {
                SetErrorMessage(playerId, $"Tried to send 0 (or fewer) ships in a fleet. Order = {input}");
            }

            if (sourcePlanet.Owner != playerId)
            {
                SetErrorMessage(playerId, $"Tried to send ships from another player's planet! Order = {input}");
                return null;
            }

            return order;
        }

        private void SetErrorMessage(int playerId, string message)
        {
            if (playerId == 1)
            {
                PlayerOneError = message;
            }
            else if (playerId == 2)
            {
                PlayerTwoError = message;
            }
            else
            {
                throw new InvalidOperationException($"Tried to set player {playerId} error message.");
            }
        }

        public void ApplyOrders(List<string> playerOneOrders, List<string> playerTwoOrders)
        {
            List<Order> orders = playerOneOrders.Select(x => ConstructAndValidateOrder(x, 1)).ToList();
            orders.AddRange(playerTwoOrders.Select(x => ConstructAndValidateOrder(x, 2)));
            if (PlayerOneError != "" || PlayerTwoError != "")
            {
                return;
            }

            ApplyDepartures(orders);
            ApplyAdvancement();
            ApplyArrivals();

            TurnNumber++;
        }

        public int GetWinner()
        {
            // Someone crashed.
            if (PlayerOneError != "" || PlayerTwoError != "")
            {
                return PlayerOneError != "" && PlayerTwoError != "" ? 0 : PlayerOneError != "" ? 2 : 1;
            }

            int playerOneShips =
                Planets
                    .Where(x => x.Owner == 1)
                    .Aggregate(0, (total, next) => next.Ships + total) +
                Fleets
                    .Where(x => x.Owner == 1)
                    .Aggregate(0, (total, next) => next.Ships + total);
            int playerTwoShips =
                Planets
                    .Where(x => x.Owner == 2)
                    .Aggregate(0, (total, next) => next.Ships + total) +
                Fleets
                    .Where(x => x.Owner == 2)
                    .Aggregate(0, (total, next) => next.Ships + total);

            // Turn limit
            if (TurnNumber == MAX_TURNS)
            {
                return playerOneShips > playerTwoShips ? 1 : playerOneShips == playerTwoShips ? 0 : 2;
            }

            int playerOnePlanets = Planets.Where(x => x.Owner == 1).Count();
            int playerTwoPlanets = Planets.Where(x => x.Owner == 2).Count();

            // Draw, because both players have nothing.
            if (playerOnePlanets == 0 && playerOneShips == 0 &&
                playerTwoPlanets == 0 && playerTwoShips == 0)
            {
                return 0;
            }

            // Player one loses, because only they have zero planets and fleets.
            if (playerOneShips == 0 && playerOnePlanets == 0)
            {
                return 2;
            }

            // Player two loses, because only they have zero planets and fleets.
            if (playerTwoShips == 0 && playerTwoPlanets == 0)
            {
                return 1;
            }

            // Game is still going!
            return -1;
        }

        private void ApplyDepartures(List<Order> orders)
        {
            foreach (Order o in orders)
            {
                Planet sourcePlanet = Planets.First(x => x.Identifier == o.Source);
                if (sourcePlanet == null)
                {
                    throw new Exception("No source planet.  Order validation went wrong.");
                }

                Planet destinationPlanet = Planets.First(x => x.Identifier == o.Destination);
                if (destinationPlanet == null)
                {
                    throw new Exception("No destination planet.  Order validation went wrong.");
                }

                if (sourcePlanet.Ships < o.Ships)
                {
                    SetErrorMessage(o.Player, $"Tried to send too many ships from planet {o.Source}");
                }

                int totalTurns = 
                    (int)Math.Ceiling(
                            Math.Sqrt(Math.Pow(destinationPlanet.X - sourcePlanet.X, 2) +
                                      Math.Pow(destinationPlanet.Y - sourcePlanet.Y, 2)));
                Fleets.Add(new Fleet(o.Player, o.Ships, o.Source, o.Destination, totalTurns, totalTurns));
                sourcePlanet.Ships -= o.Ships;
            }
        }

        private void ApplyAdvancement()
        {
            foreach (Planet p in Planets)
            {
                if (p.Owner != 0)
                {
                    p.Ships += p.Growth;
                }
            }

            foreach (Fleet f in Fleets)
            {
                f.RemainingTurns--;
            }
        }

        private void ApplyArrivals()
        {
            Dictionary<int, List<Fleet>> arrivalLocations = new Dictionary<int, List<Fleet>>();
            List<Fleet> toRemove = new List<Fleet>();
            foreach (Fleet f in Fleets)
            {
                if (f.RemainingTurns == 0)
                {
                    if (!arrivalLocations.ContainsKey(f.Destination))
                    {
                        arrivalLocations.Add(f.Destination, new List<Fleet>());
                    }

                    arrivalLocations[f.Destination].Add(f);
                    toRemove.Add(f);
                }
            }

            toRemove.ForEach(x => Fleets.Remove(x));

            foreach (var arrival in arrivalLocations)
            {
                Planet planet = Planets.First(x => x.Identifier == arrival.Key);
                if (planet == null)
                {
                    throw new Exception("Wha?  There's a fleet en route to a non-existant planet?");
                }

                ApplySpecificArrival(planet, arrival.Value);
            }
        }

        private void ApplySpecificArrival(Planet planet, List<Fleet> incoming)
        {
            // Each kvp is a force - (owner, shipcount)
            Dictionary<int, int> forces = new Dictionary<int, int>();
            if (planet.Ships > 0)
            {
                forces.Add(planet.Owner, planet.Ships);
            }

            foreach (Fleet f in incoming)
            {
                if (forces.ContainsKey(f.Owner))
                {
                    forces[f.Owner] += f.Ships;
                }
                else
                {
                    forces.Add(f.Owner, f.Ships);
                }
            }

            List<KeyValuePair<int, int>> orderedForces = forces.OrderByDescending(x => x.Value).ToList();
            if (orderedForces.Count == 1)
            {
                // This wasn't even a fight - someone was reinforcing themselves.
                planet.Ships += orderedForces[0].Value;
                planet.Owner = orderedForces[0].Key;
            }
            else
            {
                if (orderedForces[0].Value == orderedForces[1].Value)
                {
                    // Tie - planet has no more ships, but belongs to its original owner.
                    planet.Ships = 0;
                }
                else
                {
                    // Battle was joined, and the biggest force won.
                    planet.Ships = orderedForces[0].Value - orderedForces[1].Value;
                    planet.Owner = orderedForces[0].Key;
                }
            }
        }
    }
}
