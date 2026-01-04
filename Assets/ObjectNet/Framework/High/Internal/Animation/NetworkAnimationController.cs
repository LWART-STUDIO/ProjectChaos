using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Controls networked animations by synchronizing animation states across different clients.
    /// </summary>
    public class NetworkAnimationController : INetworkAnimation {

        // Reference to the network element that this controller will manage.
        private INetworkElement networkElement;

        // Animator component used to control animations.
        private Animator animator;

        const int DEFAULT_LAYER_ANIMATION = -1;

        /// <summary>
        /// Constructor for the NetworkAnimationController class.
        /// </summary>
        /// <param name="networkElement">The network element that contains the animations.</param>
        public NetworkAnimationController(INetworkElement networkElement) {
            this.networkElement = networkElement;
        }

        /// <summary>
        /// Initializes the NetworkAnimationController by setting up the animator and registering network events.
        /// </summary>
        public void Intialize() {
            // Check if the network element has an AnimationNetwork behavior and get the animator from it.
            if (this.networkElement.HasBehavior<AnimationNetwork>()) {
                this.animator = this.networkElement.GetBehavior<AnimationNetwork>().getAnimator();
            } else {
                // If not, find the Animator component in the children of the GameObject.
                this.animator = this.networkElement.GetGameObject().GetComponentInChildren<Animator>();
            }
            // Register a network event to handle animation play requests from other clients.
            if (this.networkElement.HasEvent(InternalGameEvents.AnimationPlay) == false) {
                this.networkElement.RegisterEvent(InternalGameEvents.AnimationPlay, OnReceiveAnimationPlay);
            }
            if (this.networkElement.HasEvent(InternalGameEvents.AnimationFade) == false) {
                this.networkElement.RegisterEvent(InternalGameEvents.AnimationFade, OnReceiveAnimationFade);
            }
            
        }

        /// <summary>
        /// Plays the specified animation state on the given layer.
        /// </summary>
        /// <param name="stateName">The name of the animation state to play.</param>
        /// <param name="layerIndex">The layer on which to play the animation.</param>
        public void Play(string stateName, int layerIndex) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Intialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Play the animation locally.
                this.animator.Play(stateName, layerIndex);
                // Send a message to other clients to play the same animation.
                using (DataStream writer = new DataStream()) {
                    writer.Write(stateName);
                    writer.Write(layerIndex);
                    this.networkElement.Send(InternalGameEvents.AnimationPlay, writer, DeliveryMode.Reliable);
                }
            } else if (this.networkElement.IsPassive()) {
                // If the element is passive, just play the animation without sending it to others.
                this.animator.Play(stateName, layerIndex);
            }
        }

        /// <summary>
        /// Overloaded Play method to play the specified animation state on the base layer.
        /// </summary>
        /// <param name="stateName">The name of the animation state to play.</param>
        public void Play(string stateName) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Intialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Play the animation locally.
                this.animator.Play(stateName);
                // Send a message to other clients to play the same animation on the base layer.
                using (DataStream writer = new DataStream()) {
                    writer.Write(stateName);
                    writer.Write(DEFAULT_LAYER_ANIMATION); // Zero is the base layer in Unity.
                    this.networkElement.Send(InternalGameEvents.AnimationPlay, writer, DeliveryMode.Reliable);
                }
            } else if (this.networkElement.IsPassive()) {
                // If the element is passive, just play the animation without sending it to others.
                this.animator.Play(stateName);
            }
        }

        /// <summary>
        /// CrossFade the specified animation state on the given layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="layerIndex">The layer on which to play the animation.</param>
        /// <param name="normalizedTransitionDuration"></param>
        /// <param name="normalizedTimeOffset"></param>
        /// <param name="normalizedTransitionTime"></param>
        public void CrossFade(string stateName, int layerIndex, float normalizedTransitionDuration, float normalizedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Intialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Play the animation locally.
                this.animator.Play(stateName);
                // Send a message to other clients to play the same animation on the base layer.
                using (DataStream writer = new DataStream()) {
                    writer.Write(stateName);
                    writer.Write(layerIndex);
                    writer.Write(normalizedTransitionDuration);
                    writer.Write(normalizedTimeOffset);
                    writer.Write(normalizedTransitionTime);
                    this.networkElement.Send(InternalGameEvents.AnimationFade, writer, DeliveryMode.Reliable);
                }
            } else if (this.networkElement.IsPassive()) {
                // If the element is passive, just play the animation without sending it to others.
                this.animator.CrossFade(stateName, normalizedTransitionDuration, layerIndex, normalizedTimeOffset, normalizedTransitionTime);
            }
        }

        /// <summary>
        /// Overloaded CrossFade method to fade the specified animation state on the base layer.
        /// </summary>
        /// <param name="stateName">The layer on which to play the animation</param>
        /// <param name="normalizedTransitionDuration"></param>
        /// <param name="normalizedTimeOffset"></param>
        /// <param name="normalizedTransitionTime"></param>
        public void CrossFade(string stateName, float normalizedTransitionDuration, float normalizedTimeOffset = float.NegativeInfinity, float normalizedTransitionTime = 0.0f) {
            // Initialize animation if has
            if (this.networkElement != null) {
                if (this.animator == null) {
                    this.Intialize();
                }
            }
            if (this.networkElement.IsActive()) {
                // Play the animation locally.
                this.animator.Play(stateName);
                // Send a message to other clients to play the same animation on the base layer.
                using (DataStream writer = new DataStream()) {
                    writer.Write(stateName);
                    writer.Write(DEFAULT_LAYER_ANIMATION); // Zero is the base layer in Unity.
                    writer.Write(normalizedTransitionDuration);
                    writer.Write(normalizedTimeOffset);
                    writer.Write(normalizedTransitionTime);
                    this.networkElement.Send(InternalGameEvents.AnimationFade, writer, DeliveryMode.Reliable);
                }
            } else if (this.networkElement.IsPassive()) {
                // If the element is passive, just play the animation without sending it to others.
                this.animator.CrossFade(stateName, normalizedTransitionDuration, 0, normalizedTimeOffset, normalizedTransitionTime);
            }
        }

        /// <summary>
        /// Event handler that is triggered when a passive client receives an animation play request.
        /// </summary>
        /// <param name="reader">The data stream containing the animation state and layer information.</param>
        private void OnReceiveAnimationPlay(IDataStream reader) {
            string animation = reader.Read<string>();
            int layerToPlay = reader.Read<int>();
            // Play the received animation on the specified layer.
            this.Play(animation, layerToPlay);
        }

        /// <summary>
        /// Event handler that is triggered when a passive client receives an animation crossfade request.
        /// </summary>
        /// <param name="reader">The data stream containing the animation state and layer information.</param>
        private void OnReceiveAnimationFade(IDataStream reader) {
            string animation = reader.Read<string>();
            int layerToPlay = reader.Read<int>();
            float normalizedTransitionDuration = reader.Read<float>();
            float normalizedTimeOffset = reader.Read<float>();
            float normalizedTransitionTime = reader.Read<float>();
            // Fade animation on the specified layer.
            this.CrossFade(animation, layerToPlay, normalizedTransitionDuration, normalizedTimeOffset, normalizedTransitionTime);
        }
    }

}