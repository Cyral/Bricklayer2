using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Bricklayer.Core.Common
{
    public delegate void EventHandler<in TArgsType>(TArgsType args) where TArgsType : EventArgs;

    /// <summary>
    /// Represents a collection of event handlers for a certain game event.
    /// </summary>
    /// <typeparam name="THandler">The type of delegate each handler must be.</typeparam>
    /// <typeparam name="TArgs">The type of arguments for the handler.</typeparam>
    public class Event<THandler, TArgs> where TArgs : EventArgs
    {
        private readonly List<PrioritizedEventHandler<TArgs>> handlers;

        /// <summary>
        /// Creates a new event.
        /// </summary>
        public Event()
        {
            handlers = new List<PrioritizedEventHandler<TArgs>>();
        }

        /// <summary>
        /// Adds a handler to this event.
        /// </summary>
        /// <param name="handler">Delegate to be invoked when event is ran.</param>
        /// <param name="priority">
        /// The priority order of this handler. LOWER priorities are called FIRST, and higher priorities are
        /// called last.
        /// </param>
        /// <remarks>
        /// For information on the priority ordering system, see <c>Priority</c>.
        /// </remarks>
        public void AddHandler(EventHandler<TArgs> handler, Priority priority = Priority.Normal)
        {
            handlers.Add(new PrioritizedEventHandler<TArgs>(handler, priority));
            handlers.Sort((a, b) => a.Priority.CompareTo(b)); //Sort by priority
        }

        /// <summary>
        /// Calls each handler in order of priority.
        /// </summary>
        /// <param name="args"></param>
        public void Invoke(TArgs args)
        {
            for (var i = 0; i < handlers.Count; i++)
            {
                if (i < handlers.Count - 1)
                    Debug.Assert(handlers[i].Priority <= handlers[i + 1].Priority);

                handlers[i].Event(args);
            }
        }

        /// <summary>
        /// Removes a handler from this event.
        /// </summary>
        public void RemoveHandler(THandler handler)
        {
            if (!(handler is Delegate))
                throw new ArgumentException($"{nameof(handler)} must be an delegate.");
            handlers.Remove(handlers.FirstOrDefault(e => Equals(e.Event, handler)));
        }

        /// <summary>
        /// Clears all handlers from this event.
        /// </summary>
        internal void ClearHandlers()
        {
            handlers.Clear();
        }

        #region Nested type: Class

        /// <summary>
        /// Represents an event handler and priority.
        /// </summary>
        private class PrioritizedEventHandler<TArgsType> where TArgsType : EventArgs
        {
            /// <summary>
            /// The delegate/action associated with this event handler.
            /// </summary>
            public EventHandler<TArgsType> Event { get; }

            /// <summary>
            /// The priority order of this handler. LOWER priorities are called FIRST, and higher priorities are called last.
            /// </summary>
            public Priority Priority { get; }

            public PrioritizedEventHandler(EventHandler<TArgsType> handler, Priority priority)
            {
                Event = handler;
                Priority = priority;
            }
        }

        #endregion

        #region Priority enum

        /// <summary>
        /// Represents the order priority for an event. The lowest priority events are called FIRST, and the highest, LAST.
        /// (This allows high priority events to have the "final" say on what happens)
        /// </summary>
        public enum Priority
        {
            /// <summary>
            /// The initial priority is called FIRST, and should cancel the event if needed.
            /// It is encouraged NOT to use the value of the event, only modify it at this stage, as there are many more stages after
            /// this.
            /// </summary>
            /// <example>
            /// A protection plugin could cancel an block place event using the initial priority, so it is not passed to other plugins.
            /// </example>
            Initial = 0,

            /// <summary>
            /// Lowest priority is after <c>Initial</c>, and should be used when an event needs to know the state from <c>Initial</c>
            /// events, but may still change the state for higher events.
            /// </summary>
            Lowest = 15,
            Low = 35,

            /// <summary>
            /// This is the default priority level, and should be used unless another priority is needed.
            /// Events that must be called first, such as block protection plugins, will have already been called, later events, such
            /// as logging, will not have been called.
            /// </summary>
            Normal = 50,
            High = 65,

            /// <summary>
            /// This is the last event called before <c>Final</c>, it has the final say on an event, and should take into the
            /// consideration the state of the event from prior priorities.
            /// </summary>
            Highest = 85,

            /// <summary>
            /// The final priority is called LAST, and should use the state of the event to perform an action.
            /// It encouraged NOT to modify or change the outcome of an event at this stage. Please use it purely for monitoring the
            /// final outcome of an event.
            /// </summary>
            /// <example>
            /// A logging plugin could read the final state of an event (whether it is cancelled, who sent it, etc), and log it, as the
            /// event itself it should not be modified in this stage.
            /// </example>
            Final = 100
        }

        #endregion
    }
}