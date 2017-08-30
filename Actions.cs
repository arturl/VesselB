using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace VesselB
{
    abstract class Action
    {
        public abstract bool CanPerform();
        public abstract IEnumerable<Container> Perform(IEnumerable<Container> oldState);
        public abstract void Visualize();
    }

    class Dump : Action
    {
        Container container;
        public Dump(Container container)
        {
            this.container = container;
        }

        public override bool CanPerform()
        {
            return container.Amount != 0;
        }

        public override IEnumerable<Container> Perform(IEnumerable<Container> oldState)
        {
            var newState = Container.CloneList(oldState);
            // Find the corresponding container that will be mutated
            var containerTwin = newState.First(_ => _.Name == container.Name);
            containerTwin.Amount = 0;
            return newState;
        }

        public override void Visualize()
        {
            Console.WriteLine("Action: Empty container {0}", container.Name);
        }
    }

    class FillUp : Action
    {
        Container container;
        public FillUp(Container container)
        {
            this.container = container;
        }

        public override bool CanPerform()
        {
            return container.Amount != container.Capacity;
        }

        public override IEnumerable<Container> Perform(IEnumerable<Container> oldState)
        {
            var newState = Container.CloneList(oldState);
            // Find the corresponding container that will be mutated
            var containerTwin = newState.First(_ => _.Name == container.Name);
            containerTwin.Amount = containerTwin.Capacity;
            return newState;
        }

        public override void Visualize()
        {
            Console.WriteLine("Action: Fill up container {0}", container.Name);
        }
    }

    class Pour : Action
    {
        Container from;
        Container to;

        public Pour(Container from, Container to)
        {
            this.from = from;
            this.to = to;
        }

        public override bool CanPerform()
        {
            if (from.Amount == 0) return false;
            if (to.Amount == to.Capacity) return false;
            return true;
        }

        public override IEnumerable<Container> Perform(IEnumerable<Container> oldState)
        {
            var newState = Container.CloneList(oldState);
            // Find the corresponding container that will be mutated
            var toTwin = newState.First(_ => _.Name == to.Name);
            var fromTwin = newState.First(_ => _.Name == from.Name);

            int capacityLeft = toTwin.Capacity - toTwin.Amount;
            if (capacityLeft >= fromTwin.Amount)
            {
                // Pour all liquid from 'from' to 'to', leaving 'from' empty
                toTwin.Amount += fromTwin.Amount;
                fromTwin.Amount = 0;
                Debug.Assert(toTwin.Amount <= toTwin.Capacity);
            }
            else
            {
                toTwin.Amount = toTwin.Capacity;
                fromTwin.Amount -= capacityLeft;
            }

            return newState;
        }

        public override void Visualize()
        {
            Console.WriteLine("Action: Pour from container {0} to container {1}", from.Name, to.Name);
        }
    }
}
