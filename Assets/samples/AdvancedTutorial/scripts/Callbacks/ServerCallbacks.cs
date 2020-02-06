using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UdpKit;

namespace Bolt.AdvancedTutorial
{
	[BoltGlobalBehaviour(BoltNetworkModes.Server, "Level1")]
	public class ServerCallbacks : Bolt.GlobalEventListener
	{
		public static bool ListenServer = true;

		public override bool PersistBetweenStartupAndShutdown()
		{
			return base.PersistBetweenStartupAndShutdown();
		}

		void Awake()
		{
			if (ListenServer)
			{
				Player.CreateServerPlayer();
				Player.serverPlayer.name = "SERVER";
			}
		}


		public override void ConnectRequest(UdpKit.UdpEndPoint endpoint, Bolt.IProtocolToken token)
		{
			BoltConsole.Write("ConnectRequest", Color.red);

			if (token != null)
			{
				BoltConsole.Write("Token Received", Color.red);
			}

			BoltNetwork.Accept(endpoint);
		}

		public override void ConnectAttempt(UdpEndPoint endpoint, IProtocolToken token)
		{
			BoltConsole.Write("ConnectAttempt", Color.red);
			base.ConnectAttempt(endpoint, token);
		}

		public override void Disconnected(BoltConnection connection)
		{
			BoltConsole.Write("Disconnected", Color.red);
			base.Disconnected(connection);
		}

		public override void ConnectRefused(UdpEndPoint endpoint, IProtocolToken token)
		{
			BoltConsole.Write("ConnectRefused", Color.red);
			base.ConnectRefused(endpoint, token);
		}

		public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
		{
			BoltConsole.Write("ConnectFailed", Color.red);
			base.ConnectFailed(endpoint, token);
		}

		public override void Connected(BoltConnection connection)
		{
			BoltConsole.Write("Connected", Color.red);

			connection.UserData = new Player();
			connection.GetPlayer().connection = connection;
			connection.GetPlayer().name = "CLIENT:" + connection.RemoteEndPoint.Port;

			connection.SetStreamBandwidth(1024 * 1024);
		}


		public override void SceneLoadLocalBegin(string scene)
		{
			foreach (Player p in Player.allPlayers)
			{
				p.entity = null;
			}
		}
	}
}
