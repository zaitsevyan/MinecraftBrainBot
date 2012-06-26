using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;


namespace BrainBot
{
	public class Minecraft
	{
		public Player player{ get; private set;}
		private string server;
		private ushort port;
		private string connectHash;
		private string connectName;
		private Socket client;
		private bool isConnected;
		private List<byte> stream;
		public string levelType{ get; private set;}
		public ServerMode serverMode;
		public Dimension dimension;
		public Difficulty difficulty;
		public byte maxPlayers;
		public XYZ<int> spawnPosition;
		public List<string> chat;
		public Dictionary<int,Entity> entities;
		public long time;
		public List<Chunk> map;
		public Dictionary<string,short> playerList;
		public short worldHeight = 256;
		public bool isLogged;
		public List<PacketID> packetHistory;
		
		private long packetCountReceived = 0;
		private long packetCountSend = 0;
		private long packetSizeReceived = 0;
		private long packetSizeSend = 0;
		private long commandCountReceived = 0;
		private long commandCountSend = 0;
		public Minecraft (string server, ushort port)
		{
			this.server = server;
			this.port = port;
			this.connectName = "Player";
			this.connectHash = "-";
			isConnected = false;
			maxPlayers = 0;
			stream = new List<byte> ();
			chat = new List<string> ();
			entities = new Dictionary<int, Entity> ();
			map = new List<Chunk> ();
			time = 0;
			playerList = new Dictionary<string, short> ();
			packetHistory = new List<PacketID> ();
			isLogged = false;
		}
		public void Status ()
		{
			IPAddress ip = Dns.GetHostAddresses (this.server) [0];
			client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client.Connect (ip, this.port);
			isConnected = true;
			this.SendPacket (new object[]{(byte)PacketID.ServerListPing});
			PacketID packetID = (PacketID)readByte ();
			client.Close ();
			isConnected = false;
			if(packetID==PacketID.DisconnectKick)
			{
				string status = readString ();
				string[] parts = status.Split ('§');
				Console.WriteLine (
					"{0}:{1} {2} {3}/{4}",
					this.server,
					this.port,
					parts [0],
					parts [1],
					parts [2] == "0" ? "???" : parts [2]
				);
			}
		}
		public void Start (string name)
		{
			try {
				this.connectName = name;
				this.EnterGame ();	
			} catch {
				for (int i = packetHistory.Count-100; i<packetHistory.Count; i++) {
					if (i < 0)
						continue;
					Console.WriteLine ("Debug: {0}",Enum.GetName(typeof(PacketID),packetHistory[i]));
				}
			}
		}
		public void writeToChat (string message)
		{
			string part = "";
			while (message.Length>0) {
				part = message.Substring (0, Math.Min(message.Length,100));
				message = message.Remove (0, part.Length);
				this.SendPacket (new object[]{(byte)PacketID.ChatMessage,part});
			}
		}
		private string clearString (string text)
		{
			string result = "";
			for (int i =0; i<text.Length; i++)
				if (text [i] == '§')
					i++;
				else
					result += text [i];
			return result;
		}
		private void EnterGame ()
		{
			IPAddress ip = Dns.GetHostAddresses (this.server) [0];
			client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client.Connect (ip, this.port);
			isConnected = true;
			this.SendPacket (new object[] {
				(byte)PacketID.Handshake,
				this.connectName + ";" + this.server + ":" + this.port.ToString ()
			}
			);
			PacketID packetID = (PacketID)readByte ();
			if (packetID == PacketID.Handshake) {
				this.connectHash = readString ();
				this.SendPacket (new object[] {
					(byte)PacketID.LoginRequest,
					(int)29,
					this.connectName,
					"",
					(int)0,
					(int)0,
					(byte)0,
					(byte)0,
					(byte)0
				}
				);
				this.StartGame ();
			}
		}
		private void StartGame ()
		{
			PacketID packetID;
			while (isConnected) {
				packetID = (PacketID)readByte ();
				this.commandCountReceived++;
				packetHistory.Add (packetID);
				double stance;
				int entityID = 0;
				//Console.WriteLine (
				//	"[{0}:{1}:{2}] [{3}]: {4}",this.commandCountReceived,this.packetCountReceived,this.packetSizeReceived,DateTime.Now.ToLongTimeString(),Enum.GetName(typeof(PacketID),packetID));
				switch (packetID) {
				case PacketID.LoginRequest:
					this.player = new Player (this.connectName, this);
					this.player.entityID = readInt ();
					entities [this.player.entityID] = this.player;
					readString ();
					this.levelType = readString ();
					this.serverMode = (ServerMode)readInt ();
					this.dimension = (Dimension)readInt ();
					this.difficulty = (Difficulty)readByte ();
					readByte ();
					this.maxPlayers = readByte ();
					Console.WriteLine ("Entity ID: {0}", this.player.entityID);
					Console.WriteLine ("Level Type: {0}", this.levelType);
					Console.WriteLine ("Server mode: {0}", Enum.GetName (typeof(ServerMode), this.serverMode));
					Console.WriteLine ("Dimension: {0}", Enum.GetName (typeof(Dimension), this.dimension));
					Console.WriteLine ("Difficulty: {0}", Enum.GetName (typeof(Difficulty), this.difficulty));
					Console.WriteLine ("Max players: {0}", this.maxPlayers);
					break;
				case PacketID.DisconnectKick:
					string serverAnswer = readString ();
					Console.WriteLine ("Dissconnected: {0}", serverAnswer);
					isConnected = false;
					isLogged = false;
					break;
				case PacketID.KeepAlive:
					this.SendPacket (new object[]{(byte)PacketID.KeepAlive,readInt ()});
					break;
				case PacketID.PluginMessage:
					string channel = readString ();
					byte[] data = readBytes (readShort ());
					Console.WriteLine ("Plugins data for {0} size: {1}", channel, data.Length);
					break;
				case PacketID.SpawnPosition:
					this.spawnPosition = new XYZ<int> (readInt (), readInt (), readInt ());
					//Console.WriteLine ("Spawn position: X: {0} Y: {1} Z: {2}", X, Y, Z);
					break;
				case PacketID.PlayerAbilities:
					this.player.invulnerability = readBool ();
					this.player.isFlying = readBool ();
					this.player.canFly = readBool ();
					this.player.instantDestroy = readBool ();
					Console.WriteLine (this.player.invulnerability ? "Player cannot take damage" : "Player can take damage");
					Console.WriteLine (this.player.isFlying ? "Player is currently flying" : "Player is't currently flying");
					Console.WriteLine (this.player.canFly ? "Player is able to fly" : "Player is't able to fly");
					Console.WriteLine (this.player.instantDestroy ? "Player can destroy blocks instantly" : "Player can't destroy blocks instantly");
					break;
				case PacketID.TimeUpdate:
					this.time = readLong ();
				//Console.WriteLine ("Time: {0}", time);
					break;
				case PacketID.ChatMessage:
					string msg = readString ();
					if (clearString (msg).Length > 0) {
						this.chat.Add (clearString (msg));
						Console.WriteLine ("--|{0}", clearString (msg));
					}
					break;
				case PacketID.PlayerPositionLook:
					lock (this.player) {
						this.player.position.x = readDouble ();
						stance = readDouble ();
						this.player.position.y = readDouble ();
						this.player.position.z = readDouble ();
						this.player.endPosition = this.player.position;
						this.player.look.yaw = readFloat ();
						this.player.look.pitch = readFloat ();
						this.player.onGround = readBool ();
						this.player.height = stance - this.player.position.y;
					}
					Console.WriteLine (
						"Absolute position: X:{0:0.####} Y:{1:0.####} Z:{2:0.####} Height:{3:0.###!#}",
						this.player.position.x,
						this.player.position.y,
						this.player.position.z, this.player.height);
					Console.WriteLine ("Absolute rotation: X:{0} Y:{1}", this.player.look.yaw, this.player.look.pitch);
					Console.WriteLine ("On ground: {0}", this.player.onGround ? "yes" : "no");
					this.SendPacket (new object[] {
						(byte)PacketID.PlayerPositionLook,
						this.player.position.x,
						this.player.position.y,
						this.player.position.y + this.player.height,
						this.player.position.z,this.player.look.yaw,this.player.look.pitch,this.player.onGround}
					);
					isLogged = true;
					break;
				case PacketID.Player:
					this.player.onGround = readBool ();
					Console.WriteLine ("On ground: {0}", this.player.onGround ? "yes" : "no");
					break;
				case PacketID.PlayerPosition:
					lock (this.player) {
						this.player.position.x = readDouble ();
						this.player.position.y = readDouble ();
						stance = readDouble ();
						this.player.position.z = readDouble ();
						this.player.height = stance - this.player.position.y;
						this.player.endPosition = this.player.position;
					}
					Console.WriteLine (
						"Absolute position: X:{0} Y:{1} Z:{2} Height:{3}",
						this.player.position.x,
						this.player.position.y,
						this.player.position.z,
						this.player.height
					);
					break;
				case PacketID.SpawnPainting:
					Painting picture = new Painting ();
					picture.entityID = readInt ();
					picture.title = readString ();
					picture.centerPosition.x = readInt ();
					picture.centerPosition.y = readInt ();
					picture.centerPosition.z = readInt ();
					picture.direction = readInt ();
					this.entities [picture.entityID] = picture;
				//Console.WriteLine ("Spawn Painting: {4} X:{0} Y:{1} Z:{2} Direction:{3}", x2, y2, z2, direction, title);
					break;
				case PacketID.EntityHeadLook:
					entityID = readInt ();
					if (entities.ContainsKey (entityID))
						entities [entityID].headYaw = readByte ();
					else
						readByte ();
				//Console.WriteLine ("Head yaw: {0} steps", headYew);
					break;
				case PacketID.SpawnMob:
					Mob mob = new Mob ();
					mob.entityID = readInt ();
					mob.type = readByte ();
					mob.position.x = readInt ();
					mob.position.y = readInt ();
					mob.position.z = readInt ();
					mob.look.yaw = readByte ();
					mob.look.pitch = readByte ();
					mob.headYaw = readByte ();
					mob.metadata = readMetadata ();
					entities [mob.entityID] = mob;
				/*Console.WriteLine (
					"Spawn Mob: Type:{0} X:{1} Y:{2} Z:{3} Yaw:{4} Pitch:{5} Head Yaw:{6} Metadate size:{7}",
					type,
					x3,
					y3,
					z3,
					yaw2,
					pitch2,
					headYaw2,
					metadata
				);*/
					break;
				case PacketID.EntityVelocity:
					entityID = readInt ();
					if (entities.ContainsKey (entityID)) {
						entities [entityID].velocity.x = readShort ();
						entities [entityID].velocity.y = readShort ();
						entities [entityID].velocity.z = readShort ();
					} else {
						readShort ();
						readShort ();
						readShort ();
					}
				//Console.WriteLine ("Velocity: X:{0} Y:{1} Z:{1}", vX / 28800.0, vY / 28800.0, vZ / 28800.0);
					break;
				case PacketID.SpawnNamedEntity:
					{
						OtherPlayer p = new OtherPlayer ();
						p.entityID = readInt ();
						p.name = readString ();
						p.position.x = readInt ();
						p.position.y = readInt ();
						p.position.z = readInt ();
						p.look.yaw = readByte ();
						p.look.pitch = readByte ();
						p.currentItem = readShort ();
						entities [p.entityID] = p;
						Console.WriteLine (
						"Player spawned: {3} X:{0} Y:{1} Z:{2} yaw:{4} pitch:{5} withItem:{6}",
						p.position.x,
						p.position.y,
						p.position.z,
						p.name,
						p.look.yaw,
						p.look.pitch,
						p.currentItem
						);
					}
					break;
				case PacketID.EntityEquipment:
					entityID = readInt ();
					if (entities.ContainsKey (entityID) && entities [entityID].GetType () == typeof(OtherPlayer)) {
						OtherPlayer p = (OtherPlayer)entities [entityID];
						Armor armor = new Armor ();
						armor.slot = readShort ();
						armor.itemID = readShort ();
						armor.damage = readShort ();
						p.armor [armor.slot] = armor;
						Console.WriteLine ("Equipment: slot:{0} itemID:{1} damage:{2}", armor.slot, armor.itemID, armor.damage);
					} else {
						readShort ();
						readShort ();
						readShort ();
					}
					break;
				case PacketID.SpawnDroppedItem:
					DroppedItem item = new DroppedItem ();
					item.entityID = readInt ();
					//readInt ();
					item.item = readShort ();
					item.count = readByte ();
					item.data = readShort ();
					item.position.x = readInt ();
					item.position.z = readInt ();
					item.position.y = readInt ();
					item.rotation = readByte ();
					item.pitch = readByte ();
					item.roll = readByte ();
					entities [item.entityID] = item;
					Console.WriteLine (
						"Dropped item: item:{0}:{2} count:{1} x:{3} y:{4} z:{5} rotation:{6} pitch:{7} roll:{8}",
					item.item,
					item.count,
					item.data,
					item.position.x,
					item.position.y,
					item.position.z,
					item.rotation,
					item.pitch,
					item.roll
					);
					break;
				case PacketID.MapColumnAllocation:
					{
						Chunk chunk = new Chunk ();
						chunk.position.z = readInt ();
						chunk.position.y = readInt ();
						bool mode = readBool ();
						if (mode) {
							map.Add (chunk);
						} else {
							Chunk result = map.Find (delegate(Chunk ch) {
								return ch.position.Equals (chunk.position);
							}
							); 
							if (result != null) {
								map.Remove (result);
							}
						}
					}
				//Console.WriteLine ("Need to {0} the chunk X:{1} Y{2}", mode ? "initialize" : "unload", x6, y6);
					break;
				case PacketID.PlayerListItem:
					string name = clearString (readString ());
					bool online = readBool ();
					short ping = readShort ();
					if (online) {
						playerList [name] = ping;
					} else {
						playerList.Remove (name);
					}
					Console.WriteLine ("Player:{0} ping:{1} {2} game", name, ping, online ? "enter" : "exit");
					break;
				case PacketID.SetWindowItems:
					byte windowId = readByte ();
					short count2 = readShort ();
				//Console.Write ("Window items: count:{0}", count2);
					for (int i = 0; i<count2; i++) {
						short id = readShort ();
						if (id != -1) {
							byte itemCount = readByte ();
							short _data = readShort ();
							if (_data != -1)
								readBytes (_data);
							//Console.Write (" [{0}: id: {1} count:{2} dataSize:{3}]", i, id, itemCount, _data);
						}// else
						//Console.Write (" [{0};{1}]", i, "Empty");
					}
				//Console.WriteLine ();
					break;
				case PacketID.SetSlot:
					byte winID = readByte ();
					short _slot = readShort ();
					short _id = readShort ();
					if (_id != -1) {
						byte itemCount = readByte ();
						short _data = readShort ();
						if (_data != -1)
							readBytes (_data);
						/*Console.WriteLine (
						"Set slot: [window:{4} {0}: id: {1} count:{2} dataSize:{3}]",
						_slot,
						_id,
						itemCount,
						_data,
						winID
					);*/
					} //else
					//Console.WriteLine ("Set slot: [window:{2} {0};{1}]", _slot, "Empty", winID);
					break;
				case PacketID.SpawnExperienceOrb:
					readInt ();
					int x7 = readInt ();
					int y7 = readInt ();
					int z7 = readInt ();
					int count3 = readShort ();
					Console.WriteLine ("Spawn expirience orb: x:{0} y:{1} z:{2} count:{3}", x7, y7, z7, count3);
					break;
				case PacketID.EntityRelativeMove:
					readInt ();
					byte dx = readByte ();
					byte dy = readByte ();
					byte dz = readByte ();
				//Console.WriteLine ("Entity move: dx:{0} dy:{1} dz:{2}", dx, dy, dz);
					break;
				case PacketID.EntityLook:
					readInt ();
					byte yaw4 = readByte ();
					byte pitch5 = readByte ();
				//Console.WriteLine ("Entity rotate: yaw:{0} pitch:{1}", yaw4, pitch5);
					break;
				case PacketID.DestroyEntity:
					entities.Remove (readInt ());
				//Console.WriteLine ("Destroy entity");
					break;
				case PacketID.EntityTeleport:
					readInt ();
					int x8 = readInt ();
					int y8 = readInt ();
					int z8 = readInt ();
					byte yaw8 = readByte ();
					byte pitch8 = readByte ();
				//Console.WriteLine ("Entity teleport: X:{0} Y:{1} Z:{2} yaw:{3} pitch:{4}", x8, y8, z8, yaw8, pitch8);
					break;
				case PacketID.BlockAction:
					int x9 = readInt ();
					short y9 = readShort ();
					int z9 = readInt ();
					byte byte1 = readByte ();
					byte byte2 = readByte ();
				//Console.WriteLine ("Block action: X:{0} Y:{1} Z:{2} bytes:[{3},{4}]",x9,y9,z9,byte1,byte2);
					break;
				case PacketID.EntityLookandRelativeMove:
					readInt ();
					int x10 = readByte ();
					int y10 = readByte ();
					int z10 = readByte ();
					byte yaw9 = readByte ();
					byte pitch9 = readByte ();
				//Console.WriteLine ("Entity relative look/move: dX:{0} dY:{1} dZ:{2} Yaw:{3} Pitch:{4}", x10, y10, z10, yaw9, pitch9);
					break;
				case PacketID.EntityMetadata:
					readInt ();
					int msize = readMetadata ();
					break;
				case PacketID.Animation:
					readInt ();
					readByte ();
					break;
				case PacketID.SpawnObjectVehicle:
					/*for (int i =0; i<50 && i<stream.Count; i++) {
						Console.Write ("{0} ",Convert.ToString (stream[i],16));
					}*/
					readInt ();
					readByte ();
					readInt ();
					readInt ();
					readInt ();
					int fireball = readInt ();
					if(fireball>0)
					{
						readShort ();
						readShort ();
						readShort ();
					}
					break;
				case PacketID.EntityStatus:
					readInt ();
					readByte ();
					break;
				case PacketID.IncrementStatistic:
					int statisticID = readInt ();
					byte value = readByte ();
					Console.WriteLine ("Statistic {0} change to {1}", statisticID, value);
					break;
				case PacketID.UpdateHealth:
					this.player.health = readShort ();
					this.player.food = readShort ();
					this.player.saturation = readFloat ();
					Console.WriteLine (
						"Update health:{0} food:{1} saturation:{2}",
						this.player.health, this.player.food, this.player.saturation);
					if (this.player.health <= 0) {
						this.SendPacket (new object[]{PacketID.Respawn,(int)0,(byte)1,(byte)1,(short)256,"default"});
					}
					break;
				case PacketID.UseBed:
					readInt ();
					readByte ();
					readInt ();
					readByte ();
					readInt ();
					Console.WriteLine ("use bad");
					break;
				case PacketID.CollectItem:
					readInt ();
					readInt ();
					Console.WriteLine ("Someone pick up items");
					break;
				case PacketID.Entity:
					entities [readInt ()] = new Entity ();
					break;
				case PacketID.AttachEntity:
					readInt ();
					readInt ();
					Console.WriteLine ("Attach player to vehicle");
					break;
				case PacketID.EntityEffect:
					readInt ();
					readByte ();
					readByte ();
					readShort ();
					break;
				case PacketID.RemoveEntityEffect:
					readInt ();
					readByte ();
					break;
				case PacketID.SetExperience:
					readFloat ();
					readShort ();
					readShort ();
					break;
				case PacketID.MapChunks:
					readInt ();
					readInt ();
					readBool ();
					readShort ();
					readShort ();
					int size = readInt ();
					readInt ();
					readBytes (size);
					Console.WriteLine ("Chunk uploaded size:{0}", size);
					break;
				case PacketID.MultiBlockChange:
					readInt ();
					readInt ();
					readShort ();
					int _size = readInt ();
					readBytes (_size);
					Console.WriteLine ("Multi block changes size:{0}", _size);
					break;
				case PacketID.BlockChange:
					readInt ();
					readByte ();
					readInt ();
					readByte ();
					readByte ();
					break;
				case PacketID.Explosion:
					readDouble ();
					readDouble ();
					readDouble ();
					readFloat ();
					int size2 = readInt ();
					readBytes (size2 * 3);
					Console.WriteLine ("Explosion records:{0}", size2);
					break;
				case PacketID.SoundParticleEffect:
					readInt ();
					readInt ();
					readByte ();
					readInt ();
					readInt ();
					break;
				case PacketID.ChangeGameState:
					byte byte3 = readByte ();
					byte byte4 = readByte ();
					Console.WriteLine (@"Change game mod: reason: {0} game mod:{1}", byte3, byte4);
					break;
				case PacketID.Thunderbolt:
					readInt ();
					readBool ();
					readInt ();
					readInt ();
					readInt ();
					Console.WriteLine ("Thunderbolt!");
					break;
				case PacketID.OpenWindow:
					readByte ();
					readByte ();
					string winTitle = readString ();
					int slotnum = readByte ();
					Console.WriteLine ("Open window {0} slots:{1}", winTitle, slotnum);
					break;
				case PacketID.CloseWindow:
					readByte ();
					Console.WriteLine ("Close window");
					break;
				case PacketID.UpdateWindowProperty:
					readByte ();
					readShort ();
					readShort ();
					break;
				case PacketID.ItemData:
					readShort ();
					readShort ();
					int textLength = readByte ();
					byte[] text = readBytes (textLength);
					Console.WriteLine ("Item data: {0}", Encoding.ASCII.GetString (text));
					break;
				case PacketID.UpdateTileEntity:
					readInt ();
					readShort ();
					readInt ();
					readByte ();
					readInt ();
					readInt ();
					readInt ();
					break;
				case PacketID.Respawn:
					this.dimension = (Dimension)readInt ();
					this.difficulty = (Difficulty)readByte ();
					this.serverMode = (ServerMode)readByte ();
					this.worldHeight = readShort ();
					this.levelType = readString ();
					Console.WriteLine ("Level Type: {0}", this.levelType);
					Console.WriteLine ("Server mode: {0}", Enum.GetName (typeof(ServerMode), this.serverMode));
					Console.WriteLine ("Dimension: {0}", Enum.GetName (typeof(Dimension), this.dimension));
					Console.WriteLine ("Difficulty: {0}", Enum.GetName (typeof(Difficulty), this.difficulty));
					Console.WriteLine ("World height: {0}", this.worldHeight);
					break;
				case PacketID.ConfirmTransaction:
					readByte ();
					readShort ();
					readBool ();
					Console.WriteLine ("Confirm transaction");
					break;
				case PacketID.CreativeInventoryAction:
					readShort ();
					short _id2 = readShort ();
					if (_id2 != -1) {
						byte itemCount = readByte ();
						short _data = readShort ();
						if (_data != -1)
							readBytes (_data);
					}
					break;
				case PacketID.UpdateSign:
					readInt ();
					readShort ();
					readInt ();
					string line1 = readString ();
					string line2 = readString ();
					string line3 = readString ();
					string line4 = readString ();
					Console.WriteLine ("Update sign:{0} {1} {2} {3}", line1, line2, line3, line4);
					break;
				default:
					Console.WriteLine ("Unknown response: {0} ", Convert.ToString ((byte)packetID, 16));
					isConnected = false;
					isLogged = false;
					throw new Exception ();
					break;
				}
			}
		}
		private void ReadPacket ()
		{
			if (!isConnected)
				return;
			byte[] buffer = new byte[4096];
			int size = this.client.Receive (buffer);
			this.packetCountReceived++;
			this.packetSizeReceived += size;
			Array.Resize (ref buffer,size);
			this.stream.AddRange (buffer);
		}
		public void SendPacket (object[] commands)
		{
			if (!isConnected)
				return;
			lock (this.client) {
				List<byte> packet = new List<byte> ();
				foreach (object command in commands) {
					if (command.GetType () == typeof(byte))
						write (packet, (byte)command);
					else if (command.GetType () == typeof(int))
						write (packet, (int)command);
					else if (command.GetType () == typeof(short))
						write (packet, (short)command);
					else if (command.GetType () == typeof(string))
						write (packet, (string)command);
					else if (command.GetType () == typeof(double))
						write (packet, (double)command);
					else if (command.GetType () == typeof(float))
						write (packet, (float)command);
					else if (command.GetType () == typeof(bool))
						write (packet, (byte)(((bool)command) ? 0x01 : 0x00));
				}
				this.client.Send (packet.ToArray ());
			}
		}
		private void write (List<byte> result, byte value)
		{
			result.Add (value);
		}
		private void write (List<byte> result, short value)
		{
			byte[] number = BitConverter.GetBytes (value);
			Array.Reverse (number);
			result.AddRange (number);
		}
		private void write (List<byte> result, int value)
		{
			byte[] number = BitConverter.GetBytes (value);
			Array.Reverse (number);
			result.AddRange (number);
		}
		private void write (List<byte> result, double value)
		{
			byte[] number = BitConverter.GetBytes (value);
			Array.Reverse (number);
			result.AddRange (number);
		}
		private void write (List<byte> result, float value)
		{
			byte[] number = BitConverter.GetBytes (value);
			Array.Reverse (number);
			result.AddRange (number);
		}
		private void write (List<byte> result, string value)
		{
			write (result, (short)value.Length);
			result.AddRange (Encoding.BigEndianUnicode.GetBytes (value));
		}
		private byte readByte ()
		{
			needPacket (1);
			byte result = stream [0];
			stream.RemoveAt (0);
			return result;
		}
		private bool readBool ()
		{
			return readByte()==0x01;
		}
		private int readInt ()
		{
			needPacket (4);
			byte[] bytes = stream.GetRange (0, 4).ToArray ();
			Array.Reverse (bytes);
			int result = BitConverter.ToInt32 (bytes, 0);
			stream.RemoveRange (0, 4);
			return result;
		}
		private double readDouble ()
		{
			needPacket (8);
			byte[] bytes = stream.GetRange (0, 8).ToArray ();
			Array.Reverse (bytes);
			double result = BitConverter.ToDouble (bytes, 0);
			stream.RemoveRange (0, 8);
			return result;
		}
		private byte[] readBytes (int size)
		{
			needPacket (size);
			byte[] result = stream.GetRange (0, size).ToArray ();
			stream.RemoveRange (0, size);
			return result;
		}
		private int readMetadata ()
		{
			int size = 0;
			byte curByte = readByte();
			int ty = 0;
			while (curByte!=127) {
				ty = curByte >> 5;
				switch (ty) {
				case 0:
					readByte ();
					size += 1;
					break;
				case 1:
					readShort();
					size += 2;
					break;
				case 2:
					readInt();
					size += 4;
					break;
				case 3:
					readFloat();
					size += 4;
					break;
				case 4:
					string str = readString();
					size += str.Length * 2;
					break;
				case 5:
					readShort();
					readByte();
					readShort();
					size += 1;
					break;
				case 6:
					readInt();
					readInt();
					readInt();
					size += 1;
					break;
				}
				curByte = readByte();
			}
			return size;
		}
		private long readLong ()
		{
			needPacket (8);
			byte[] bytes = stream.GetRange (0, 8).ToArray ();
			Array.Reverse (bytes);
			long result = BitConverter.ToInt64 (bytes, 0);
			stream.RemoveRange (0, 8);
			return result;
		}
		private float readFloat ()
		{
			needPacket (4);
			byte[] bytes = stream.GetRange (0, 4).ToArray ();
			Array.Reverse (bytes);
			float result = BitConverter.ToSingle (bytes, 0);
			stream.RemoveRange (0, 4);
			return result;
		}
		private string readString ()
		{
			short size = readShort ();
			needPacket (size * 2);
			string result = Encoding.BigEndianUnicode.GetString (stream.GetRange (0, size * 2).ToArray ());
			stream.RemoveRange (0, size * 2);
			return result;
		}
		private short readShort ()
		{
			needPacket (2);
			byte[] bytes = stream.GetRange (0, 2).ToArray ();
			Array.Reverse (bytes);
			short result = BitConverter.ToInt16 (bytes,0);
			stream.RemoveRange (0, 2);
			return result;
		}
		private void needPacket (int size)
		{
			while (stream.Count < size) {
				this.ReadPacket ();
			}
		}
	}
}

