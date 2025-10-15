﻿using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can specify the two constructor arguments used to initialize
	/// a plain old class object which then gets wrapped by a <see cref="Wrapper{TWrapped}"/> component.
	/// <para>
	/// The argument values can be assigned using the inspector and serialized as part of a scene or a prefab.
	/// </para>
	/// <para>
	/// The <typeparamref name="TWrapped">wrapped object</typeparamref> gets created and injected to
	/// the <typeparamref name="TWrapper">wrapper component</typeparamref> during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// After the object has been injected the <see cref="WrapperInitializer{,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TWrapper"> Type of the initialized wrapper component. </typeparam>
	/// <typeparam name="TWrapped"> Type of the object wrapped by the wrapper. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument passed to the wrapped object's constructor. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument passed to the wrapped object's constructor. </typeparam>
	public abstract class WrapperInitializer<TWrapper, TWrapped, TFirstArgument, TSecondArgument>
		: WrapperInitializerBase<TWrapper, TWrapped, TFirstArgument, TSecondArgument>
			where TWrapper : Wrapper<TWrapped>
	{
		[SerializeField]
		private Any<TFirstArgument> firstArgument = default;
		[SerializeField]
		private Any<TSecondArgument> secondArgument = default;

		/// <inheritdoc/>
		protected override TFirstArgument FirstArgument { get => firstArgument.GetValueOrDefault(this, Context.MainThread); set => firstArgument = value; }
		/// <inheritdoc/>
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValueOrDefault(this, Context.MainThread); set => secondArgument = value; }

		#if UNITY_EDITOR
		protected override bool HasNullArguments => IsNull(this, firstArgument) || IsNull(this, secondArgument);
		#endif
    }
}