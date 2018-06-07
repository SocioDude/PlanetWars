using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using System.Text;
using System.Threading.Tasks;

/**
 * TODOs:
 *   * Add reasonable error messages if commandline args aren't good.
 *   * Show help message if called for.
 */

namespace PlanetWars
{
    class Program
    {
        static void Main(string[] args)
        {
            string mapPath = "";
            string firstAIPath = "";
            string secondAIPath = "";
            string replayPath = "";
            bool useTimeout = true;

            var p = new OptionSet() {
                { "m|map=", "(required) the path to the map", v => mapPath = v },
                { "ai1|firstAI=", "(required) path to the first AI", v => firstAIPath = v },
                { "ai2|secondAI=", "(required) path to the first AI", v => secondAIPath = v },
                { "r|replayPath=", "(optional) path to where replay should be written.", v => replayPath = v },
                { "notimeout", "(optional) set if no timeout should be used (usually to attach debugger).", v => useTimeout = false }
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("planetwars: ");
                Console.WriteLine(e.Message);
                return;
            }

            if (mapPath == "" || firstAIPath == "" || secondAIPath == "")
            {
                WriteUsage(p);
                return;
            }

            string[] lines = File.ReadAllLines(mapPath);
            Map map = new Map(lines.ToList());

            List<string> replayData = new List<string>();
            replayData.Add(map.ToString());
            ManagedAI first = new ManagedAI(firstAIPath);

            // There's probably a less stupid language construct like some try-with-resources thing that could make
            // sure we kill these processes, but I don't want to look it up.
            try
            {
                ManagedAI second = new ManagedAI(secondAIPath);

                try
                {
                    while (map.GetWinner() == -1)
                    {
                        List<string> playerOneOrders = null;
                        if (useTimeout)
                        {
                            var task = Task.Run(() => first.GetOrders(map.GetPlayerOneState()));
                            if (task.Wait(TimeSpan.FromSeconds(1)))
                            {
                                playerOneOrders = task.Result;
                            }
                            else
                            {
                                map.PlayerOneError = "Timed out.";
                            }
                        }
                        else
                        {
                            playerOneOrders = first.GetOrders(map.GetPlayerOneState());
                        }

                        List<string> playerTwoOrders = null;
                        if (useTimeout)
                        {
                            var task = Task.Run(() => second.GetOrders(map.GetPlayerTwoState()));
                            if (task.Wait(TimeSpan.FromSeconds(1)))
                            {
                                playerTwoOrders = task.Result;
                            }
                            else
                            {
                                map.PlayerTwoError = "Timed out.";
                            }
                        }
                        else
                        {
                            playerTwoOrders = second.GetOrders(map.GetPlayerTwoState());
                        }

                        if (map.PlayerOneError != "" || map.PlayerTwoError != "")
                        {
                            // At least one AI timed out.
                            break;
                        }

                        List<string> playerOneDebug = new List<string>();
                        List<string> playerOneFilteredOrders = FilterOrders(playerOneOrders, playerOneDebug, 1);
                        List<string> playerTwoDebug = new List<string>();
                        List<string> playerTwoFilteredOrders = FilterOrders(playerTwoOrders, playerTwoDebug, 2);
                        map.ApplyOrders(playerOneFilteredOrders, playerTwoFilteredOrders);
                        replayData.Add(map.ToString());
                        replayData.AddRange(playerOneDebug);
                        replayData.AddRange(playerTwoDebug);
                        replayData.Add("go\n");
                    }

                    replayData.Add(map.ToString());
                    replayData.Add("go\n");
                    replayData.Add("GAME OVER");
                    replayData.Add($"LENGTH: {map.TurnNumber}");
                    replayData.Add($"P1ERROR: {map.PlayerOneError}");
                    replayData.Add($"P2ERROR: {map.PlayerTwoError}");
                    replayData.Add($"WINNER: {map.GetWinner()}");
                    replayData.ForEach(x => Console.WriteLine(x));

                    if (replayPath != "")
                    {
                        try
                        {
                            File.WriteAllLines(replayPath, replayData);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Couldn't write replay at {replayPath}");
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    second.EndProcess();
                    throw e;
                }

                second.EndProcess();
            }
            catch (Exception e)
            {
                first.EndProcess();
                throw e;
            }

            first.EndProcess();
        }

        private static List<string> FilterOrders(List<string> rawOrders, List<string> debugOutput, int playerNumber)
        {
            List<string> orders = new List<string>();
            foreach (string order in rawOrders)
            {
                if (order.StartsWith("#"))
                {
                    if (debugOutput.Count < 10)
                    {
                        debugOutput.Add(playerNumber + order);
                    }
                }
                else
                {
                    orders.Add(order);
                }
            }

            return orders;
        }

        private static void WriteUsage(OptionSet p)
        {
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
