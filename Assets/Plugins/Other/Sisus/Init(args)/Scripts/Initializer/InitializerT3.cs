﻿using UnityEngine;
using static Sisus.Init.Internal.InitializerUtility;

namespace Sisus.Init
{
	/// <summary>
	/// A base class for a component that can can specify the three arguments used to
	/// initialize a component that implements
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}"/>.
	/// <para>
	/// The arguments can be assigned using the inspector and are serialized as part of the client's scene or prefab asset.
	/// </para>
	/// <para>
	/// The arguments get injected to the <typeparamref name="TClient">client</typeparamref>
	/// during the <see cref="Awake"/> event.
	/// </para>
	/// <para>
	/// The client receives the arguments via the
	/// <see cref="IInitializable{TFirstArgument, TSecondArgument, TThirdArgument}.Init">Init</see>
	/// method where they can be assigned to member fields or properties.
	/// </para>
	/// <para>
	/// After the arguments have been injected the <see cref="Initializer{,,,}"/> is removed from the
	/// <see cref="GameObject"/> that holds it.
	/// </para>
	/// </summary>
	/// <typeparam name="TClient"> Type of the initialized client component. </typeparam>
	/// <typeparam name="TFirstArgument"> Type of the first argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TSecondArgument"> Type of the second argument to pass to the client component's Init function. </typeparam>
	/// <typeparam name="TThirdArgument"> Type of the third argument to pass to the client component's Init function. </typeparam>
	public abstract class Initializer<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
		: InitializerBase<TClient, TFirstArgument, TSecondArgument, TThirdArgument>
			where TClient : MonoBehaviour, IInitializable<TFirstArgument, TSecondArgument, TThirdArgument>
	{
		[SerializeField]
		private Any<TFirstArgument> firstArgument = default;
		[SerializeField]
		private Any<TSecondArgument> secondArgument = default;
		[SerializeField]
		private Any<TThirdArgument> thirdArgument = default;

		/// <inheritdoc/>
		protected override TFirstArgument FirstArgument { get => firstArgument.GetValueOrDefault(this, Context.MainThread); set => firstArgument = value; }
		/// <inheritdoc/>
		protected override TSecondArgument SecondArgument { get => secondArgument.GetValueOrDefault(this, Context.MainThread); set => secondArgument = value; }
		/// <inheritdoc/>
		protected override TThirdArgument ThirdArgument { get => thirdArgument.GetValueOrDefault(this, Context.MainThread); set => thirdArgument = value; }

		#if UNITY_EDITOR
		protected override bool HasNullArguments => IsNull(this, firstArgument) || IsNull(this, secondArgument) || IsNull(this, thirdArgument);
		#endif
	}
}