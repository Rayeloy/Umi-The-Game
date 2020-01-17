using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UE = UnityEngine;

namespace Bolt.AdvancedTutorial
{

	public partial class Player : IDisposable
	{
		public const byte TEAM_RED = 1;
		public const byte TEAM_BLUE = 2;

		public string name;
		public BoltEntity entity;
		public BoltConnection connection;

		public IPlayerState state
		{
			get { return entity.GetState<IPlayerState>(); }
		}

		public bool isServer
		{
			get { return connection == null; }
		}

		public Player()
		{
			players.Add(this);
		}



		public void Dispose()
		{
			players.Remove(this);

			// destroy
			if (entity)
			{
				BoltNetwork.Destroy(entity.gameObject);
			}

			// while we have a team difference of more then 1 player

		}

		public void InstantiateEntity()
		{
			entity = BoltNetwork.Instantiate(BoltPrefabs.Player, new TestToken(), RandomSpawn(), Quaternion.identity);

			state.name = name;

			if (isServer)
			{
				entity.TakeControl(new TestToken());
			}
			else
			{
				entity.AssignControl(connection, new TestToken());
			}
		}
	}

	partial class Player
	{
		static List<Player> players = new List<Player>();


		public static IEnumerable<Player> allPlayers
		{
			get { return players; }
		}

		public static bool serverIsPlaying
		{
			get { return serverPlayer != null; }
		}

		public static Player serverPlayer
		{
			get;
			private set;
		}

		public static void CreateServerPlayer()
		{
			serverPlayer = new Player();
		}

		static Vector3 RandomSpawn()
		{
			float x = UE.Random.Range(-32f, +32f);
			float z = UE.Random.Range(-32f, +32f);
			return new Vector3(x, 32f, z);
		}

	}

}