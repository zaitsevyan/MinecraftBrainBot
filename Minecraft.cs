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
		public Minecraft (string server, ushort port)
		{
			this.server = server;
			this.port = port;
			this.connectName = "Player";
			this.connectHash = "-";
			isConnected = false;
			stream = new List<byte> ();		
		}
		public void Start (string name)
		{
			this.connectName = name;
			this.connect ();	
		}
		private void connect ()
		{
			IPAddress ip = Dns.GetHostAddresses (this.server) [0];
			client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client.Connect (ip, this.port);
			isConnected = true;
			this.SendPacket (new object[]{(byte)PacketID.ServerListPing});
			PacketID packetID = (PacketID)readByte ();
			client.Close ();
			isConnected = false;
			if (packetID == PacketID.DisconnectKick) {
				string status = readString ();
				string[] parts = status.Split ('§');
				Console.WriteLine ("{0}:{1} {2} {3}/{4}", this.server, this.port, parts [0], parts [1], parts [2] == "0" ? "???" : parts [2]);
				this.EnterGame ();
			}
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
				switch ((byte)packetID) {
				case 0x01:
					int EntityId = readInt ();
					readString ();
					string levelType = readString ();
					int serverMode = readInt ();
					int dimension = readInt ();
					byte difficulty = readByte ();
					readByte ();
					byte maxPlayers = readByte ();
					Console.WriteLine ("Entity ID: {0}", EntityId);
					Console.WriteLine ("Level Type: {0}", levelType);
					Console.WriteLine ("Server mode: {0}", serverMode == 0 ? "Survival" : "Creative");
					Console.WriteLine ("Dimension: {0}", dimension == -1 ? "Nether" : (dimension == 0 ? "Overworld" : "End"));
					Console.WriteLine (
					"Difficulty: {0}",
					difficulty == 0 ? "Peaceful" : (difficulty == 1 ? "Easy" : (difficulty == 2 ? "Normal" : "Hard"))
					);
					Console.WriteLine ("Max players: {0}", maxPlayers);
					break;
				case 0xFF:
					string serverAnswer = readString ();
					Console.WriteLine ("Dissconnected: {0}", serverAnswer);
					isConnected = false;
					break;
				case 0x00:
					this.SendPacket (new object[]{(byte)PacketID.KeepAlive,readInt ()});
					break;
				case 0xFA:
					string channel = readString ();
					byte[] data = readBytes (readShort ());
					Console.WriteLine ("Plugins data for {0} size: {1}", channel, data.Length);
					break;
				case 0x06:
					int X = readInt ();
					int Y = readInt ();
					int Z = readInt ();
				//Console.WriteLine ("Spawn position: X: {0} Y: {1} Z: {2}", X, Y, Z);
					break;
				case 0xCA:
					bool invulnerability = readBool ();
					bool isFlying = readBool ();
					bool canFly = readBool ();
					bool instantDestroy = readBool ();
					Console.WriteLine (invulnerability ? "Player cannot take damage" : "Player can take damage");
					Console.WriteLine (isFlying ? "Player is currently flying" : "Player is't currently flying");
					Console.WriteLine (canFly ? "Player is able to fly" : "Player is't able to fly");
					Console.WriteLine (instantDestroy ? "Player can destroy blocks instantly" : "Player can't destroy blocks instantly");
					break;
				case 0x04:
					long time = readLong ();
				//Console.WriteLine ("Time: {0}", time);
					break;
				case 0x03:
					string msg = readString ();
					Console.WriteLine ("Chat message: {0}", msg);
					break;
				case 0x0D:
					double x = readDouble ();
					double stance = readDouble ();
					double y = readDouble ();
					double z = readDouble ();
					float yaw = readFloat ();
					float pitch = readFloat ();
					bool onGround = readBool ();
					Console.WriteLine ("Absolute position: X:{0:0.##} Y:{1:0.##} Z:{2:0.##} Stance:{3:0.##}", x, y, z, stance);
					Console.WriteLine ("Absolute rotation: X:{0} Y:{1}", yaw, pitch);
					Console.WriteLine ("On ground: {0}", onGround ? "yes" : "no");
					this.SendPacket (new object[]{(byte)PacketID.PlayerPositionLook,x,y,stance,z,yaw,pitch,onGround});
					break;
				case 0x0A:
					bool onground = readBool ();
					Console.WriteLine ("On ground: {0}", onground ? "yes" : "no");
					break;
				case 0x0B:
					double x1 = readDouble ();
					double y1 = readDouble ();
					double stance1 = readDouble ();
					double z1 = readDouble ();
					Console.WriteLine ("Absolute position: X:{0} Y:{1} Z:{2} Stance:{3}", x1, y1, z1, stance1);
					break;
				case 0x19:
					readInt ();
					string title = readString ();
					int x2 = readInt ();
					int y2 = readInt ();
					int z2 = readInt ();
					int direction = readInt ();
				//Console.WriteLine ("Spawn Painting: {4} X:{0} Y:{1} Z:{2} Direction:{3}", x2, y2, z2, direction, title);
					break;
				case 0x23:
					readInt ();
					byte headYew = readByte ();
				//Console.WriteLine ("Head yaw: {0} steps", headYew);
					break;
				case 0x18:
					readInt ();
					byte type = readByte ();
					int x3 = readInt ();
					int y3 = readInt ();
					int z3 = readInt ();
					byte yaw2 = readByte ();
					byte pitch2 = readByte ();
					byte headYaw2 = readByte ();
					int metadata = readMetadata ();
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
				case 0x1C:
					readInt ();
					short vX = readShort ();
					short vY = readShort ();
					short vZ = readShort ();
				//Console.WriteLine ("Velocity: X:{0} Y:{1} Z:{1}", vX / 28800.0, vY / 28800.0, vZ / 28800.0);
					break;
				case 0x14:
					int playerID = readInt ();
					string playerName = readString ();
					int x4 = readInt ();
					int y4 = readInt ();
					int z4 = readInt ();
					byte yaw3 = readByte ();
					byte pitch3 = readByte ();
					short currentItem = readShort ();
					Console.WriteLine (
					"Player spawned: {3} X:{0} Y:{1} Z:{2} yaw:{4} pitch:{5} withItem:{6}",
					x4,
					y4,
					z4,
					playerName,
					yaw3,
					pitch3,
					currentItem
					);
					break;
				case 0x05:
					readInt ();
					short slot = readShort ();
					short itemID = readShort ();
					short damage = readShort ();
					Console.WriteLine ("Equipment: slot:{0} itemID:{1} damage:{2}", slot, itemID, damage);
					break;
				case 0x15:
					readInt ();
					short Item = readShort ();
					byte _count = readByte ();
					short _damage = readShort ();
					int x5 = readInt ();
					int y5 = readInt ();
					int z5 = readInt ();
					byte rotation = readByte ();
					byte pitch4 = readByte ();
					byte roll = readByte ();
					Console.WriteLine (
					"Dropped item: item:{0} count:{1} data:{2} x:{3} y:{4} z:{5} rotation:{6} pitch:{7} roll:{8}",
					Item,
					_count,
					_damage,
					x5,
					y5,
					z5,
					rotation,
					pitch4,
					roll
					);
					break;
				case 0x32:
					int x6 = readInt ();
					int y6 = readInt ();
					bool mode = readBool ();
				//Console.WriteLine ("Need to {0} the chunk X:{1} Y{2}", mode ? "initialize" : "unload", x6, y6);
					break;
				case 0xC9:
					string name = readString ();
					bool online = readBool ();
					short ping = readShort ();
					Console.WriteLine ("Player:{0} ping:{1} {2} game", name, ping, online ? "enter" : "exit");
					break;
				case 0x68:
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
				case 0x67:
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
				case 0x1A:
					readInt ();
					int x7 = readInt ();
					int y7 = readInt ();
					int z7 = readInt ();
					int count3 = readShort ();
					Console.WriteLine ("Spawn expirience orb: x:{0} y:{1} z:{2} count:{3}", x7, y7, z7, count3);
					break;
				case 0x1F:
					readInt ();
					byte dx = readByte ();
					byte dy = readByte ();
					byte dz = readByte ();
				//Console.WriteLine ("Entity move: dx:{0} dy:{1} dz:{2}", dx, dy, dz);
					break;
				case 0x20:
					readInt ();
					byte yaw4 = readByte ();
					byte pitch5 = readByte ();
				//Console.WriteLine ("Entity rotate: yaw:{0} pitch:{1}", yaw4, pitch5);
					break;
				case 0x1D:
					readInt ();
				//Console.WriteLine ("Destroy entity");
					break;
				case 0x22:
					readInt ();
					int x8 = readInt ();
					int y8 = readInt ();
					int z8 = readInt ();
					byte yaw8 = readByte ();
					byte pitch8 = readByte ();
				//Console.WriteLine ("Entity teleport: X:{0} Y:{1} Z:{2} yaw:{3} pitch:{4}", x8, y8, z8, yaw8, pitch8);
					break;
				case 0x36:
					int x9 = readInt ();
					short y9 = readShort ();
					int z9 = readInt ();
					byte byte1 = readByte ();
					byte byte2 = readByte ();
				//Console.WriteLine ("Block action: X:{0} Y:{1} Z:{2} bytes:[{3},{4}]",x9,y9,z9,byte1,byte2);
					break;
				case 0x21:
					readInt ();
					int x10 = readByte ();
					int y10 = readByte ();
					int z10 = readByte ();
					byte yaw9 = readByte ();
					byte pitch9 = readByte ();
				//Console.WriteLine ("Entity relative look/move: dX:{0} dY:{1} dZ:{2} Yaw:{3} Pitch:{4}", x10, y10, z10, yaw9, pitch9);
					break;
				case 0x28:
					readInt ();
					int msize = readMetadata ();
					break;
				case 0x12:
					readInt ();
					readByte ();
					break;
				case 0x17:
					readInt ();
					readByte ();
					readInt ();
					readInt ();
					readInt ();
					readInt ();
					readShort ();
					readShort ();
					readShort ();
					Console.WriteLine ("Spawn object/vehicle");
					break;
				case 0x26:
					readInt ();
					readByte ();
					break;
				case 0xc8:
					int statisticID = readInt ();
					byte value = readByte ();
					Console.WriteLine ("Statistic {0} change to {1}", statisticID, value);
					break;
				case 0x08:
					short health = readShort ();
					short food = readShort ();
					float saturation = readFloat ();
					Console.WriteLine ("Update health:{0} food:{1} saturation:{2}", health, food, saturation);
					if (health <= 0) {
						this.SendPacket (new object[]{PacketID.Respawn,(int)0,(byte)1,(byte)1,(short)256,"default"});
					}
					break;
				case 0x11:
					readInt ();
					readByte ();
					readInt ();
					readByte ();
					readInt ();
					Console.WriteLine ("use bad");
					break;
				case 0x16:
					readInt ();
					readInt ();
					Console.WriteLine ("Someone pick up items");
					break;
				case 0x1e:
					readInt ();
					break;
				case 0x27:
					readInt ();
					readInt ();
					Console.WriteLine ("Attach player to vehicle");
					break;
				case 0x29:
					readInt ();
					readByte ();
					readByte ();
					readShort ();
					break;
				case 0x2A:
					readInt ();
					readByte ();
					break;
				case 0x2B:
					readFloat ();
					readShort ();
					readShort ();
					break;
				case 0x33:
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
				case 0x34:
					readInt ();
					readInt ();
					readShort ();
					int _size = readInt ();
					readBytes (_size);
					Console.WriteLine ("Multi block changes size:{0}", _size);
					break;
				case 0x35:
					readInt ();
					readByte ();
					readInt ();
					readByte ();
					readByte ();
					break;
				case 0x3C:
					readDouble ();
					readDouble ();
					readDouble ();
					readFloat ();
					int size2 = readInt ();
					readBytes (size2 * 3);
					Console.WriteLine ("Explosion records:{0}", size2);
					break;
				case 0x3D:
					readInt ();
					readInt ();
					readByte ();
					readInt ();
					readInt ();
					break;
				case 0x46:
					byte byte3 = readByte ();
					byte byte4 = readByte ();
					Console.WriteLine (@"Change game mod: reason: {0} game mod:{1}", byte3, byte4);
					break;
				case 0x47:
					readInt ();
					readBool ();
					readInt ();
					readInt ();
					readInt ();
					Console.WriteLine ("Thunderbolt!");
					break;
				case 0x64:
					readByte ();
					readByte ();
					string winTitle = readString ();
					int slotnum = readByte ();
					Console.WriteLine ("Open window {0} slots:{1}", winTitle, slotnum);
					break;
				case 0x65:
					readByte ();
					Console.WriteLine ("Close window");
					break;
				case 0x69:
					readByte ();
					readShort ();
					readShort ();
					break;
				case 0x83:
					readShort ();
					readShort ();
					int textLength = readByte ();
					byte[] text = readBytes (textLength);
					Console.WriteLine ("Item data: {0}", Encoding.ASCII.GetString (text));
					break;
				case 0x84:
					readInt ();
					readShort ();
					readInt ();
					readByte ();
					readInt ();
					readInt ();
					readInt ();
					break;
				case 0x09:
					int dimension2 = readInt ();
					byte difficulty2 = readByte ();
					byte creativeMode = readByte ();
					short worldHeight = readShort ();
					string levelType2 = readString ();
					Console.WriteLine ("Level Type: {0}", levelType2);
					Console.WriteLine ("Dimension: {0}", dimension2 == -1 ? "Nether" : (dimension2 == 0 ? "Overworld" : "End"));
					Console.WriteLine (
					"Difficulty: {0}",
					difficulty2 == 0 ? "Peaceful" : (difficulty2 == 1 ? "Easy" : (difficulty2 == 2 ? "Normal" : "Hard"))
					);
					Console.WriteLine ("Creative: {0}", creativeMode);
					Console.WriteLine ("World height: {0}", worldHeight);
					break;
				case 0x6A:
					readByte ();
					readShort ();
					readBool ();
					Console.WriteLine ("Confirm transaction");
					break;
				case 0x6B:
					readShort ();
					short _id2 = readShort ();
					if (_id2 != -1) {
						byte itemCount = readByte ();
						short _data = readShort ();
						if (_data != -1)
							readBytes (_data);
					}
					break;
				case 0x82:
					readInt ();
					readShort ();
					readInt ();
					string line1 = readString ();
					string line2 = readString ();
					string line3 = readString ();
					string line4 = readString ();
					Console.WriteLine ("Update sign:{0}/{1}/{2}/{3}", line1, line2, line3, line4);
					break;
				default:
					Console.WriteLine ("Unknown response: {0} ", Convert.ToString ((byte)packetID, 16));
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
			Array.Resize (ref buffer,size);
			this.stream.AddRange (buffer);
		}
		private void SendPacket (object[] commands)
		{
			if (!isConnected)
				return;
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
				else if(command.GetType()==typeof(bool))
					write(packet,(byte)(((bool)command)?0x01:0x00));
			}
			this.client.Send (packet.ToArray());
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

