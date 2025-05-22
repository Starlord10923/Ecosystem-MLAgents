using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace DarkTonic.PoolBoss
{
	/// <summary>
	/// This class is used to configure a Timed Despawner
	/// </summary>
	// ReSharper disable once CheckNamespace
	[AddComponentMenu("Dark Tonic/Pool Boss/Timed Despawner")]
	public class TimedDespawner : MonoBehaviour
	{
		/*! \cond PRIVATE */
		private float _lifeSeconds = 5;
		public float LifeSeconds
		{
			get { return _lifeSeconds; }
			set
			{
				_lifeSeconds = value;
				RestartTimer();
			}
		}

		private CoroutineHandle _despawnCoroutine;

		public bool StartTimerOnSpawn = true;
		/*! \endcond */

		private Transform _trans;

		// ReSharper disable once UnusedMember.Local
		void Awake()
		{
			_trans = transform;
			AwakeOrSpawn();
		}

		// ReSharper disable once UnusedMember.Local
		void OnSpawned()
		{ // used by Core GameKit Pooling & also Pool Manager Pooling!
			AwakeOrSpawn();
		}

		void AwakeOrSpawn()
		{
			if (StartTimerOnSpawn)
			{
				StartTimer();
			}
		}

		/// <summary>
		/// Call this method to start the Timer if it's not set to start automatically.
		/// </summary>
		public void StartTimer()
		{
			if (_despawnCoroutine != null)
			{
				Timing.KillCoroutines(_despawnCoroutine);
			}
			_despawnCoroutine = Timing.RunCoroutine(WaitUntilTimeUp());
		}

		private void RestartTimer()
		{
			if (StartTimerOnSpawn)
			{
				StartTimer();
			}
		}

		private IEnumerator<float> WaitUntilTimeUp()
		{
			yield return Timing.WaitForSeconds(LifeSeconds);

			bool hasLogged = false;
			while (!PoolBoss.IsReady)
			{
				if (!hasLogged)
				{
					Debug.LogWarning("PoolBoss is not Ready for Despawning: " + (_trans != null ? _trans.name : "Null"));
					hasLogged = true;
				}

				yield return Timing.WaitForOneFrame;
			}

			if (hasLogged)
				Debug.LogWarning("After waiting for PoolBoss Ready, Despawned item : " + (_trans != null ? _trans.name : "Null"));

			PoolBoss.Despawn(_trans);
		}

		private void OnDestroy()
		{
			Timing.KillCoroutines(_despawnCoroutine);
		}
	}
}
