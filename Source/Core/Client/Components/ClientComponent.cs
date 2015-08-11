using System.Threading.Tasks;

namespace Bricklayer.Core.Client.Components
{
    /// <summary>
    /// Represents a component required for the client.
    /// </summary>
    public abstract class ClientComponent
    {
        /// <summary>
        /// Determines if the component is initialized.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// The client object controlling this component.
        /// </summary>
        protected internal Client Client { get; }

        internal ClientComponent(Client client)
        {
            Client = client;
        }

        /// <summary>
        /// Performs initialization login for the component
        /// </summary>
        #pragma warning disable 1998 // Ignore warning, as overriden Init methods may include asynchonous code.
        public virtual async Task Init()
        #pragma warning restore 1998
        {
            Initialized = true;
        }
    }
}