﻿#pragma warning disable CS0414

using System;
using JetBrains.Annotations;
#if UNITY_EDITOR
using Sisus.Init.EditorOnly;
#endif
using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;
using static Sisus.NullExtensions;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the constructor argument used to initialize
	/// a plain old class object which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The argument values can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the object has been injected the <see cref="WrapperInitializer{,,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// <para>
	/// When you derive your Initializer class from <see cref="WrapperInitializerBase{,,}"/>
	/// you are responsible for implementing the argument properties and serializing their value.
	/// This means you will need to write a little bit more code, but it also grants you more options
	/// in how to handle the serialization, making it possible to support types that Unity can't serialize
	/// automatically. If you derive from <see cref="WrapperInitializer{,,}"/> instead, then these things will be handled for you.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TArgument"> Type of the argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializerBase<TWrapper, TWrapped, TArgument> : MonoBehaviour
		, IInitializer<TWrapped, TArgument>, IValueProvider<TWrapped>
		#if UNITY_EDITOR
		, IInitializerEditorOnly
		#endif
		where TWrapper : Wrapper<TWrapped>
	{
		[SerializeField, HideInInspector, Tooltip(TargetTooltip)]
		protected TWrapper target = null;

		[SerializeField, HideInInspector, Tooltip(NullArgumentGuardTooltip)]
		private NullArgumentGuard nullArgumentGuard = NullArgumentGuard.EditModeWarning | NullArgumentGuard.RuntimeException;

		/// <inheritdoc/>
		TWrapped IValueProvider<TWrapped>.Value => target != null ? target.WrappedObject : default;

		/// <inheritdoc/>
		object IValueProvider.Value => target != null ? target.WrappedObject : default(TWrapped);

		/// <inheritdoc/>
		Object IInitializer.Target { get => target; set => target = (TWrapper)value; }

		/// <inheritdoc/>
		bool IInitializer.TargetIsAssignableOrConvertibleToType(Type type) => type.IsAssignableFrom(typeof(TWrapper)) || type.IsAssignableFrom(typeof(TWrapped));

		/// <inheritdoc/>
		object IInitializer.InitTarget() => InitTarget();

		/// <summary>
		/// The argument used to initialize the wrapped object.
		/// </summary>
		protected abstract TArgument Argument { get; set; }

		#if UNITY_EDITOR
		bool IInitializerEditorOnly.ShowNullArgumentGuard => true;
		NullArgumentGuard IInitializerEditorOnly.NullArgumentGuard { get => nullArgumentGuard; set => nullArgumentGuard = value; }
		string IInitializerEditorOnly.NullGuardFailedMessage { get => nullGuardFailedMessage; set => nullGuardFailedMessage = value; }
		bool IInitializerEditorOnly.HasNullArguments => HasNullArguments;
		protected virtual bool HasNullArguments => IsNull(Argument);
		[HideInInspector, NonSerialized] private string nullGuardFailedMessage = "";
		bool IInitializerEditorOnly.MultipleInitializersPerTargetAllowed => false;
		#endif

		/// <inheritdoc/>
		public TWrapped InitTarget()
		{
			if(this == null)
			{
				return target;
			}

			// Handle instance first creation method, which supports cyclical dependencies (A requires B, and B requires A).
			if(GetOrCreateUnitializedWrappedObject() is var wrappedObject && wrappedObject is IInitializable<TArgument> initializable)
			{
				target = InitWrapper(wrappedObject);

				var argument = Argument;
				OnAfterUnitializedWrappedObjectArgumentRetrieved(this, ref argument);

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
				{
					if(argument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TArgument));
				}
				#endif

				initializable.Init(argument);
			}
			// Handle arguments first creation method, which supports constructor injection.
			else
			{
				var argument = Argument;

				#if DEBUG || INIT_ARGS_SAFE_MODE
				if(nullArgumentGuard.IsEnabled(NullArgumentGuard.RuntimeException))
				{
					if(argument == Null) throw GetMissingInitArgumentsException(GetType(), typeof(TWrapper), typeof(TArgument));
				}
				#endif

				wrappedObject = CreateWrappedObject(argument);
				target = InitWrapper(wrappedObject);
			}

			Updater.InvokeAtEndOfFrame(DestroySelf);
			return target;
		}

		protected virtual void OnReset(ref TArgument argument) { }

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> using the default constructor
		/// or retrieves an existing instance of it contained in <see cref="TWrapper"/>.
		/// <para>
		/// By default this method returns <see langword="null"/>. When this is the case then
		/// the <see cref="CreateWrappedObject"/> overload will be used to create the
		/// <see cref="TWrapped"/> instance during initialization.
		/// </para>
		/// <para>
		/// If <see cref="TWrapped"/> is a serializable class, or this method is overridden to return
		/// a non-null value, and <see cref="TWrapped"/> implements <see cref="IInitializable{TArgument}"/>,
		/// then this overload will be used to create the instance during initialization instead
		/// of <see cref="CreateWrappedObject"/>.
		/// The instance will be created and injected to the <see cref="TWrapper"/>
		/// component first, and only then will all the initialization arguments be retrieved and injected
		/// to the Wrapped object through its <see cref="IInitializable{}.Init"/> function.
		/// </para>
		/// <para>
		/// The main benefit with this form of two-part initialization (first create and inject the instance,
		/// then retrieve the arguments and inject them to the instance), is that it makes it possible to
		/// have cyclical dependencies between your objects. Normally if A requires B during its initialization,
		/// and B requires A during its initialization, both will fail to initialize as the cyclical dependency
		/// is unresolvable. With two-part initialization it is possible to initialize both objects, because A
		/// can be created without its dependencies injected at first, then B can be created and initialized with A,
		/// and finally B can be injected to A.
		/// is that 
		/// </para>
		/// </summary>
		/// <returns> Instance of the <see cref="TWrapped"/> class or <see langword="null"/>. </returns>
		[CanBeNull]
		protected virtual TWrapped GetOrCreateUnitializedWrappedObject() => target != null && target.gameObject == gameObject ? target.wrapped : default;

		/// <summary>
		/// Creates a new instance of <see cref="TWrapped"/> initialized using the provided argument and returns it.
		/// <para>
		/// Note: If you need support circular dependencies between your objects then you need to also override
		/// <see cref="GetOrCreateUnitializedWrappedObject()"/>.
		/// </para>
		/// </summary>
		/// <param name="argument"> The argument used to initialize the wrapped object. </param>
		/// <returns> Instance of the <see cref="TWrapped"/> class. </returns>
		[NotNull]
		protected abstract TWrapped CreateWrappedObject(TArgument argument);

		/// <summary>
		/// Initializes the existing <see cref="target"/> or new instance of type <see cref="TWrapper"/> using the provided <paramref name="wrappedObject">wrapped object</paramref>.
		/// </summary>
		/// <param name="wrappedObject"> The <see cref="TWrapped">wrapped object</see> to pass to the <typeparamref name="TWrapper">wrapper</typeparamref>'s Init function. </param>
		/// <returns> The existing <see cref="target"/> or new instance of type <see cref="TWrapper"/>. </returns>
		[NotNull]
		protected virtual TWrapper InitWrapper(TWrapped wrappedObject)
        {
            if(target == null)
            {
                return gameObject.AddComponent<TWrapper, TWrapped>(wrappedObject);
            }

            if(target.gameObject != gameObject)
            {
                return target.Instantiate(wrappedObject);
            }

            (target as IInitializable<TWrapped>).Init(wrappedObject);

			return target;
        }

		#if UNITY_EDITOR
        private void Reset()
		{
			var set = HandleReset(this, ref target, Argument, OnReset);
			if(!AreEqual(Argument, set)) Argument = set;
		}

		private void OnValidate() => OnMainThread(Validate);
		#endif

		protected virtual void Validate()
		{
			#if UNITY_EDITOR
			ValidateOnMainThread(this);
			#endif
		}

		private void Awake() => InitTarget();

		private void DestroySelf()
		{
			if(this != null)
			{
				Destroy(this);
			}
		}
    }
}