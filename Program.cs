using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace VesselB
{
    class Container
    {
        public string Name { get; private set; }

        public int Amount { get; set; }
        public int Capacity { get; private set; }

        public Container(string name, int amount, int capacity)
        {
            Name = name;
            Amount = amount;
            Capacity = capacity;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, Amount);
        }

        public Container Clone()
        {
            return new Container(Name, Amount, Capacity);
        }

        public static IEnumerable<Container> CloneList(IEnumerable<Container> containers)
        {
            return containers.Select(_ => _.Clone()).ToArray();
        }
    }

    class Program
    {
        static IEnumerable<Action> MakeActions(Container thisContainer, IEnumerable<Container> otherContainers)
        {
            Action action;
            action = new Dump(thisContainer);
            if (action.CanPerform()) yield return action;

            action = new FillUp(thisContainer);
            if (action.CanPerform()) yield return action;

            foreach (var container in otherContainers)
            {
                action = new Pour(thisContainer, container);
                if (action.CanPerform()) yield return action;
            }

            foreach (var container in otherContainers)
            {
                action = new Pour(container, thisContainer);
                if (action.CanPerform()) yield return action;
            }
        }

        static void VisualizeState(IEnumerable<Container> containers)
        {
            Console.Write("Current state: ");
            foreach(var container in containers)
            {
                Console.Write("{0} ", container);
            }
            Console.WriteLine();
        }

        static Dictionary<string, int> seenStates = new Dictionary<string, int>();

        static List<List<Action>> allSolutions = new List<List<Action>>();

        static string GetStateKey(IEnumerable<Container> containers)
        {
            return String.Join(",", containers.Select(_ => _.Amount.ToString()));
        }

        static void Traverse(List<Action> currentActionChain, int maxDepth, IEnumerable<Container> containers)
        {
            foreach (var container in containers)
            {
                // Make actions for this container
                var actions = MakeActions(container, containers.Where(_ => _ != container));
                foreach (var action in actions)
                {
                    var newState = action.Perform(containers);
                    var stateKey = GetStateKey(newState);
                    int seenDepth;
                    if (seenStates.TryGetValue(stateKey, out seenDepth))
                    {
                        // Alredy been in this state, skip it unless we see it at a more shallow depth
                        if (seenDepth <= currentActionChain.Count)
                            continue;
                    }
                    seenStates[stateKey] = currentActionChain.Count;

                    // Accept the action
                    currentActionChain.Add(action);

                    if (logging)
                    {
                        for (int i = 0; i < currentActionChain.Count; ++i) Console.Write("  ");
                        action.Visualize();
                        for (int i = 0; i < currentActionChain.Count; ++i) Console.Write("  ");
                        VisualizeState(newState);
                    }

                    if (newState.Where(_ => _.Amount == desiredAmount).Any())
                    {
                        if (logging) Console.WriteLine("Found a solution in {0} steps", currentActionChain.Count);
                        var newRecord = new Action[currentActionChain.Count];
                        currentActionChain.CopyTo(newRecord);
                        allSolutions.Add(newRecord.ToList());
                    }
                    else if (currentActionChain.Count < maxDepth)
                    {
                        Traverse(currentActionChain, maxDepth, newState);
                    }

                    // Pop off current action
                    currentActionChain.RemoveAt(currentActionChain.Count - 1);
                }
            }
        }

        const bool logging = false;
        static int desiredAmount = 3;

        static void Main(string[] args)
        {
#if false
            // Set up initial state
            var container1 = new Container("A", 0, 5);
            var container2 = new Container("B", 0, 9);

            var containers = new List<Container> { container1, container2 };
#else // interactive
            var containers = new List<Container>();

            while (true){
                Console.Write("Enter container name (empty to finish):");
                var containerName = Console.ReadLine();
                if (containerName.Trim() == "")
                    break;

                int capacity = 0;
                while (true)
                {
                    Console.Write("Enter capacity for container '{0}':", containerName);
                    var capacityStr = Console.ReadLine();
                    
                    if (!int.TryParse(capacityStr, out capacity) || capacity <= 0)
                    {
                        Console.WriteLine("Error! Must be a positive integer value!");
                        continue;
                    }
                    break;
                }
                containers.Add(new Container(containerName, 0, capacity));
            }

            while (true)
            {
                Console.Write("Enter desired amount in a container:");
                var amountStr = Console.ReadLine();

                if (!int.TryParse(amountStr, out desiredAmount) || desiredAmount <= 0)
                {
                    Console.WriteLine("Error! Must be a positive integer value!");
                    continue;
                }
                break;
            }

#endif
            seenStates.Add(GetStateKey(containers), 0);
            Traverse(new List<Action>(), 15, containers);

            var bestSolution = allSolutions.OrderBy(_ => _.Count);

            if (bestSolution.Any())
            {
                Console.WriteLine("Best solution in {0} steps:", bestSolution.First().Count);

                foreach (var action in bestSolution.First())
                {
                    action.Visualize();
                    containers = action.Perform(containers).ToList();
                    VisualizeState(containers);
                }
            }
            else
            {
                Console.WriteLine("No solution found!");
            }
        }
    }
}
