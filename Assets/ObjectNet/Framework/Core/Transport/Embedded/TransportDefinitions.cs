using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Static class containing definitions and configurations for transport mechanisms.
    /// </summary>
    public static class TransportDefinitions {

        /// <summary>
        // Define the default address family for the transport layer.
        // Currently set to IPv4.
        /// <summary>
        public static TransportAddressFamily AddressFamily = TransportAddressFamily.Ipv4;

        /// <summary>
        // Define the default server binding type for the transport layer.
        // Currently set to use any available address for binding.
        /// <summary>
        public static TransportServerBind ServerBindingType = TransportServerBind.UseAnyAddress;

        /// <summary>
        /// The size of the buffer for unreliable messages. When the buffer size is reached,
        /// the oldest messages will be discarded.
        /// </summary>
        /// <remarks>
        /// TODO: Implement an automatic system to adjust the buffer size based on the number of objects in the scene.
        /// </remarks>
        public static int UnreliabeBufferSize = 10000000; // Initial buffer size for unreliable messages.

        /// <summary>
        /// The maximun of clients accepted by server
        /// </summary>
        public static int MaximunOfClients = -1; // MNegqative means that has no limits

        // Minimum queue size per object in the scene.
        private const int MIN_QUEUE_SIZE_PEER_OBJECT = 5;

        // Maximum queue size per object, calculated as a multiple of MIN_QUEUE_SIZE_PEER_OBJECT.
        private const int MAX_QUEUE_SIZE_PEER_OBJECT = (1000 * MIN_QUEUE_SIZE_PEER_OBJECT);

        /// <summary>
        /// Adjusts the buffer size for unreliable messages based on the number of objects in the scene.
        /// </summary>
        /// <param name="objectsCounter">The number of objects currently in the scene.</param>
        public static void AdjustBufferSize(int objectsCounter) {
            // Clamp the buffer size to ensure it's within the allowed range.
            // Note: Currently, this calculation does not change the buffer size as it clamps
            // the value to MAX_QUEUE_SIZE_PEER_OBJECT regardless of the objectsCounter.
            // This might be an area to review for correct functionality.
            TransportDefinitions.UnreliabeBufferSize = Math.Clamp(objectsCounter * MAX_QUEUE_SIZE_PEER_OBJECT, MAX_QUEUE_SIZE_PEER_OBJECT, MAX_QUEUE_SIZE_PEER_OBJECT);
        }
    }

}