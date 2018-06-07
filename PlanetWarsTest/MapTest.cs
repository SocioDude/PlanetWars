using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanetWars;

namespace PlanetWarsTest
{
    [TestClass]
    public class MapTest
    {
        [TestMethod]
        public void TestOrderFleet()
        {
            // Order a fleet to go from one planet to another. (P1 and P2)
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 1 6" };
            List<string> playerTwoOrders = new List<string> { "1 0 7" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 9 5",
                "P 8 8 2 8 5",
                "F 1 6 0 1 12 11",
                "F 2 7 1 0 12 11"
            };
            CollectionAssert.AreEqual(expected, state);
        }

        [TestMethod]
        public void TestNoOrders()
        {
            // Send no orders. (P1 and P2)
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string>();
            List<string> playerTwoOrders = new List<string>();
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 15 5",
                "P 8 8 2 15 5"
            };
            CollectionAssert.AreEqual(expected, state);
        }

        [TestMethod]
        public void TestMultiFleet()
        {
            // Send multiple fleets from the same planet. (P1 and P2)
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5",
                "P 4 4 0 15 10"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 1 6", "0 2 4" };
            List<string> playerTwoOrders = new List<string> { "1 0 7", "1 2 2" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 5 5",
                "P 8 8 2 6 5",
                "P 4 4 0 15 10",
                "F 1 6 0 1 12 11",
                "F 1 4 0 2 6 5",
                "F 2 7 1 0 12 11",
                "F 2 2 1 2 6 5"
            };
            CollectionAssert.AreEqual(expected, state);
        }

        [TestMethod]
        public void TestSendTooMany()
        {
            // Send more than the number of ships that are on a planet. (P1 and P2)
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 1 11" };
            List<string> playerTwoOrders = new List<string> { "1 0 50" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            CollectionAssert.AreEqual(expected, state);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestSendTooManyMultiFleet()
        {
            // Send a valid number of ships in one fleet, and a "valid" number in the second that depletes the
            // planet. (P1 and P2)
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5",
                "P 4 4 0 15 10"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 1 6", "0 2 5" };
            List<string> playerTwoOrders = new List<string> { "1 0 7", "1 2 9" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestSendNeutralFleet()
        {
            // Order a fleet from a neutral planet. (P1 and P2)
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5",
                "P 4 4 0 15 10"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "2 1 6" };
            List<string> playerTwoOrders = new List<string> { "2 0 7" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestSendOpponnetFleet()
        {
            // Order an opponent's fleet. (P1 and P2)
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5",
                "P 4 4 0 15 10"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "1 1 6" };
            List<string> playerTwoOrders = new List<string> { "0 0 7" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestViewport()
        {
            // Ensure that the world looks like the player is player one. (P1 and P2)
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 1 6" };
            List<string> playerTwoOrders = new List<string> { "1 0 7" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 9 5",
                "P 8 8 2 8 5",
                "F 1 6 0 1 12 11",
                "F 2 7 1 0 12 11"
            };
            CollectionAssert.AreEqual(expected, state);
            List<string> twoExpected = new List<string>
            {
                "P 0 0 2 9 5",
                "P 8 8 1 8 5",
                "F 2 6 0 1 12 11",
                "F 1 7 1 0 12 11"
            };
            CollectionAssert.AreEqual(twoExpected, testMap.GetPlayerTwoState());
        }

        [TestMethod]
        public void TestMalformedOrder()
        {
            // Send a completely malformed order string.
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "cow" };
            List<string> playerTwoOrders = new List<string> { "1 0 moo" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            CollectionAssert.AreEqual(expected, state);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestFromNonPlanet()
        {
            // Send an order to move a fleet from a planet that doesn't exist.
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "19 1 5" };
            List<string> playerTwoOrders = new List<string> { "3 0 5" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            CollectionAssert.AreEqual(expected, state);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestToNonPlanet()
        {
            // Send an order to move a fleet to a planet that doesn't exist.
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 15 5" };
            List<string> playerTwoOrders = new List<string> { "1 3 5" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            CollectionAssert.AreEqual(expected, state);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestSendToSource()
        {
            // Send an order to move a fleet from a planet to the same planet.
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 0 5" };
            List<string> playerTwoOrders = new List<string> { "1 1 5" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            CollectionAssert.AreEqual(expected, state);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestSendNoShips()
        {
            // Send an order to move 0 ships.
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 1 0" };
            List<string> playerTwoOrders = new List<string> { "1 0 0" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            CollectionAssert.AreEqual(expected, state);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestNegativeNumbers()
        {
            // Try to send an order with negative numbers.
            List<string> mapLines = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            Map testMap = new Map(mapLines);
            List<string> playerOneOrders = new List<string> { "0 -1 0" };
            List<string> playerTwoOrders = new List<string> { "1 0 -3" };
            testMap.ApplyOrders(playerOneOrders, playerTwoOrders);
            List<string> state = testMap.GetPlayerOneState();
            List<string> expected = new List<string>
            {
                "P 0 0 1 10 5",
                "P 8 8 2 10 5"
            };
            CollectionAssert.AreEqual(expected, state);
            Assert.AreNotEqual("", testMap.PlayerOneError);
            Assert.AreNotEqual("", testMap.PlayerTwoError);
            Assert.AreEqual(0, testMap.GetWinner());
        }

        [TestMethod]
        public void TestBattles()
        {
            // Probably multiple test cases here.  Want to set up conditions where we test all combinations of fleets
            // arriving.
        }

        [TestMethod]
        public void TestGameEndsAtTurnLimit()
        {
            // Ensure the game ends at the turn limit.
        }

        [TestMethod]
        public void TestGameEndsWhenPlayerWins()
        {
            // Ensure the game ends when a player wins (multiple conditions for this?)
        }

        [TestMethod]
        public void TestGameEndsWithTie()
        {
            // I don't understand how this can happen, but it's in the rules.
        }

        [TestMethod]
        public void TestRunAfterEnded()
        {
            // Try to continue simulating the match after it's ended.  Expect exception.
        }
    }
}
