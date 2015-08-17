using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bricklayer.Core.Common
{
    public delegate void EventHandler<in TArgsType>(TArgsType args) where TArgsType : BricklayerEventArgs;

    /// <summary>
    /// Event arguments for Bricklayer events.
    /// </summary>
    public class BricklayerEventArgs : EventArgs
    {
        /// <summary>
        /// True if the event has been cancelled and should not continue to execute any further.
        /// </summary>
        /// <remarks>
        /// Plugins can ignore this property by setting ignoreCancel to true when registering an event.
        /// </remarks>
        public bool Cancelled { get; set; }

        public BricklayerEventArgs()
        {
            Cancelled = false;
        }

        /// <summary>
        /// Cancel the event from progressing any further.
        /// </summary>
        public void Cancel()
        {
            Cancelled = true;
        }
    }

    /// <summary>
    /// Base class for all events.
    /// </summary>
    /// <remarks>
    /// This class is used to solve the issue of automatically removing events.
    /// All events are added a list. Each event's RemoveHandlers method is then called, which will remove all events
    /// registered to a certain plugin, based on the assembly name found when it registered the event.
    /// </remarks>
    public class BaseEvent
    {
        /// <summary>
        /// All events.
        /// </summary>
        internal static List<BaseEvent> Events = new List<BaseEvent>(); 

        internal virtual void RemoveHandlers(string mainTypeName)
        {
            
        }
    }

    /// <summary>
    /// Represents a collection of event handlers for a certain game event.
    /// </summary>
    /// <typeparam name="TArgs">The type of arguments for the handler.</typeparam>
    public class Event<TArgs> : BaseEvent where TArgs : BricklayerEventArgs
    {
        private readonly List<PrioritizedEventHandler<TArgs>> handlers;

        /// <summary>
        /// Creates a new event.
        /// </summary>
        public Event()
        {
            handlers = new List<PrioritizedEventHandler<TArgs>>();
            Events.Add(this);
        }

        /// <summary>
        /// Adds a handler to this event.
        /// </summary>
        /// <param name="handler">Delegate to be invoked when event is ran.</param>
        /// <param name="ignoreCancel">If true, the handler will be executed even if a previous handler cancelled the event.</param>
        /// <remarks>
        /// For information on the priority ordering system, see <c>Priority</c>.
        /// </remarks>
        public void AddHandler(EventHandler<TArgs> handler, bool ignoreCancel = false)
        {
            var asm = Assembly.GetCallingAssembly();
            addHandler(asm, handler, EventPriority.Normal, ignoreCancel);
        }

        /// <summary>
        /// Adds a handler to this event.
        /// </summary>
        /// <param name="handler">Delegate to be invoked when event is ran.</param>
        /// <param name="priority">
        /// The priority order of this handler. LOWER priorities are called FIRST, and higher priorities are
        /// called last. The default priority is 'Normal'.
        /// </param>
        /// <param name="ignoreCancel">If true, the handler will be executed even if a previous handler cancelled the event.</param>
        /// <remarks>
        /// For information on the priority ordering system, see <c>Priority</c>.
        /// </remarks>
        public void AddHandler(EventHandler<TArgs> handler, EventPriority priority, bool ignoreCancel = false)
        {
            var asm = Assembly.GetCallingAssembly();
            addHandler(asm, handler, priority, ignoreCancel);
        }

        private void addHandler(Assembly asm, EventHandler<TArgs> handler, EventPriority priority, bool ignoreCancel)
        {
            // Attempt to find the calling plugin. 
            // This is used when a plugin is unloaded, to automatically remove any events.
            // TODO: Only issue is, the event must be registered from the main assembly (plugin.dll)
            // I (Cyral) have tried reflection and a bunch of ideas, but decided that it is best to try
            // and remove the events we can. In certain cases the author will manually have to remove events however.
            var callerTypeName = string.Empty;
            if (asm.FullName.Split(',')[0].Equals("plugin"))
            {
                // If found, find the main type name that inherits from the plugin class.
                var types = asm.GetTypes();
                var pluginType = typeof(Plugin);
                var mainType = types.FirstOrDefault(type => pluginType.IsAssignableFrom(type));
                if (mainType != null)
                    callerTypeName = mainType.FullName;
            }

            handlers.Add(new PrioritizedEventHandler<TArgs>(handler, priority, ignoreCancel, callerTypeName));
            handlers.Sort((a, b) => ((int)a.Priority).CompareTo(b.Priority)); // Sort by priority
        }

        /// <summary>
        /// Calls each handler in order of priority.
        /// </summary>
        /// <param name="args"></param>
        public void Invoke(TArgs args)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < handlers.Count; i++)
            {
                if (!args.Cancelled || handlers[i].IgnoreCancel)
                    handlers[i].Event(args);
            }
        }

        /// <summary>
        /// Removes a handler from this event.
        /// </summary>
        public void RemoveHandler(EventHandler<TArgs> handler)
        {
            handlers.RemoveAll(e => e.Event.Equals(handler));
        }

        /// <summary>
        /// Removes all handlers that were created by the specified type name.
        /// </summary>
        internal override void RemoveHandlers(string mainTypeName)
        {
            for (var i = handlers.Count - 1; i >= 0; i--)
            {
                if (handlers[i].CallerTypeName.Equals(mainTypeName))
                {
                    handlers.RemoveAt(i);
                    i--;
                }
            }
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
        private class PrioritizedEventHandler<TArgsType> where TArgsType : BricklayerEventArgs
        {
            /// <summary>
            /// The delegate/action associated with this event handler.
            /// </summary>
            public EventHandler<TArgsType> Event { get; }

            /// <summary>
            /// The priority order of this handler. LOWER priorities are called FIRST, and higher priorities are called last.
            /// </summary>
            public EventPriority Priority { get; }

            /// <summary>
            /// If true, the handler will be executed even if a previous handler cancelled the event.
            /// </summary>
            public bool IgnoreCancel { get; }

            /// <summary>
            /// The name of the caller who created the event. Used for automatic unsubscription when a plugin is unloaded.
            /// </summary>
            public string CallerTypeName { get; }

            public PrioritizedEventHandler(EventHandler<TArgsType> handler, EventPriority priority, bool ignoreCancel = false, string callerTypeName = "")
            {
                Event = handler;
                Priority = priority;
                IgnoreCancel = ignoreCancel;
                CallerTypeName = callerTypeName;
            }
        }

        #endregion
    }

    #region Priority enum

    /// <summary>
    /// Represents the order priority for an event. The lowest priority events are called FIRST, and the highest, LAST.
    /// (This allows high priority events to have the "final" say on what happens)
    /// </summary>
    public class EventPriority
    {
        /// <summary>
        /// The level of priority from 0 to 100.
        /// </summary>
        internal int Priority { get; private set; }

        /// <summary>
        /// This priority level is only accessible to the core game, and is called before <c>Initial</c>.
        /// </summary>
        internal static readonly EventPriority InternalInitial = 0;

        /// <summary>
        /// The initial priority is called FIRST, and should cancel the event if needed.
        /// It is encouraged NOT to use the value of the event, only modify it at this stage, as there are many more stages after
        /// this.
        /// </summary>
        /// <example>
        /// A protection plugin could cancel an block place event using the initial priority, so it is not passed to other plugins.
        /// </example>
        public static readonly EventPriority Initial = 1;

        /// <summary>
        /// Lowest priority is after <c>Initial</c>, and should be used when an event needs to know the state from <c>Initial</c>
        /// events, but may still change the state for higher events.
        /// </summary>
        public static readonly EventPriority Lowest = 15;
        public static readonly EventPriority Low = 35;

        /// <summary>
        /// This is the default priority level, and should be used unless another priority is needed.
        /// Events that must be called first, such as block protection plugins, will have already been called, later events, such
        /// as logging, will not have been called.
        /// </summary>
        public static readonly EventPriority Normal = 50;

        public static readonly EventPriority High = 65;

        /// <summary>
        /// This is the last event called before <c>Final</c>, it has the final say on an event, and should take into the
        /// consideration the state of the event from prior priorities.
        /// </summary>
        public static readonly EventPriority Highest = 85;

        /// <summary>
        /// The final priority is called LAST, and should use the state of the event to perform an action.
        /// It encouraged NOT to modify or change the outcome of an event at this stage. Please use it purely for monitoring the
        /// final outcome of an event.
        /// </summary>
        /// <example>
        /// A logging plugin could read the final state of an event (whether it is cancelled, who sent it, etc), and log it, as the
        /// event itself it should not be modified in this stage.
        /// </example>
        public static readonly EventPriority Final = 99;

        /// <summary>
        /// This priority level is only accessible to the core game, and is called before <c>Initial</c>.
        /// </summary>
        internal static readonly EventPriority InternalFinal = 100;

        private EventPriority(int priority)
        {
            Priority = priority;
        }

        // The following operators are used to make this class behave like an enum (castable to int)
        static public EventPriority operator <=(EventPriority value1, EventPriority value2)
        {
            return value1 <= value2;
        }

        static public EventPriority operator >=(EventPriority value1, EventPriority value2)
        {
            return value1 >= value2;
        }

        static public implicit operator EventPriority(int value)
        {
            return new EventPriority(value);
        }

        static public implicit operator int(EventPriority priority)
        {
            return priority.Priority;
        }
    }

    #endregion
}