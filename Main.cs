using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Timers;

namespace BrainBot
{
	class MainClass
	{ 
		private Socket client;
		// Адрес сервера
	    private IPAddress ip = Dns.GetHostAddresses("main.ttyh.ru")[0];
	    // Порт, по которому будем присоединяться
	    private int port = 25565;
	    // Статус клиента
		string connectHash = "-";
		string nickname = "HypnoToad";
		string consoleinput = "";
		double posX = 0;
		double posY = 0;
		double posZ = 0;
		int posIndex = 0;
		double speed = 0;
		double[] sx = new double[4]{102,103,104,105};
		double[] sy = new double[4]{135,136,135,136};
		double[] sz = new double[4]{99,99,99,99};
		bool isLogged = false;
		public static void Main (string[] args)
		{
			MainClass BrainBot = new MainClass ();
			while (true) {
				if (BrainBot.client == null) {
					BrainBot.Connect ();
					BrainBot.client.Close ();
					BrainBot.client = null;
				}
			}
		}
		void Connect ()
		{
			isLogged = false;
			try {
				client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				client.Connect (ip, port);
				if (!client.Connected) {
					Console.WriteLine ("Cannot connect to server");
					return;
				}
				//client.ReceiveTimeout = 2000;
				byte[] bytes = new byte[2];
				bytes [0] = 0xFE;
				Sender (bytes);
				byte[] result = Receiver ();
				if (result.Length == 0)
					Console.WriteLine ("Server dont response");
				else {
					if (result [0] == 0xFF) {
						string res = Encoding.BigEndianUnicode.GetString (result, 3, result.Length - 3);
						string[] ress = res.Split ('§');
						if (ress.Length == 3) {
							int currentOnline = int.Parse (ress [1]);
							int maxAvaliable = int.Parse (ress [2]);
							Console.WriteLine (
							"Server: {0} {1}/{2}",
							ress [0],
							currentOnline,
							(maxAvaliable > 0) ? maxAvaliable.ToString () : "???"
							);
							client.Close ();
							StartSession ();
						} else {
						
							Console.WriteLine ("Unknown response code");
						}
					} else {
						Console.WriteLine ("Unknown response code");
					}
				}
			} catch (Exception e) {
				Console.WriteLine ("Error: {0}",e.Message);
				if (client.Connected == false) {
					client.Close ();
					Connect ();
				}
			}
        }
		void processCommand ()
		{
			if (consoleinput == "")
				return;
			byte[] sendMessage = new byte[0];
			if (consoleinput [0] == '!') {
				string[] lines = consoleinput.Split (' ');
				if (lines [0] == "!moveTo") {
					double x = double.Parse (lines [1]);
					double y = double.Parse (lines [2]);
					double z = double.Parse (lines [3]);
					sendMessage = addByteToPacket (sendMessage, 0x0B);
					sendMessage = addDoubleToPacket (sendMessage, x);
					sendMessage = addDoubleToPacket (sendMessage, y);
					sendMessage = addDoubleToPacket (sendMessage, y + 1.62);
					sendMessage = addDoubleToPacket (sendMessage, z);
					sendMessage = addByteToPacket (sendMessage, 0x00);
					posX = x;
					posY = y;
					posZ = z;
				}
				if (lines [0] == "!player") {
					bool onground = lines [1] == "1";
					sendMessage = new byte [2];
					sendMessage [0] = 0x0A;
					sendMessage [1] = (byte)(onground ? 0x01 : 0x00);
				}
				if (lines [0] == "!look") {
					float yaw = float.Parse (lines [1]);
					float pitch = float.Parse (lines [2]);
					sendMessage = addByteToPacket (sendMessage, 0x0C);
					sendMessage = addFloatToPacket (sendMessage, yaw);
					sendMessage = addFloatToPacket (sendMessage, pitch);
					sendMessage = addByteToPacket (sendMessage, 0x00);
				}
			} else {
				sendMessage = addByteToPacket (sendMessage, 0x03);
				sendMessage = addStringToPacket (sendMessage, consoleinput);
			}
			Console.Write("User command: ");
			for (int i = 0; i< sendMessage.Length; i++) {
				Console.Write("{0}:",Convert.ToString(sendMessage[i],16).ToUpper());
			}
			Console.WriteLine ();
			SendPacket (sendMessage);
			consoleinput = "";
		}
		byte[] Receiver ()
		{
			byte[] bytes = new byte[4096];
			// Принимает данные от сервера в формате "X|Y"

			int result = 0;
			try {
				result = client.Receive (bytes);
			} catch (SocketException e) {
				Console.WriteLine (e.Message);
				return new byte[0];
			}
			if (result >= 0) {
				//string data = Encoding.UTF8.GetString(bytes);
				//string[] split_data = data.Split(new Char[] {'|'});
				// Передаем отпарсенные значения методу Draw на отрисовку
				byte[] res = new byte[result];
				for (int i = 0; i < result; i++) {
					//Console.Write ("{0} ", bytes [i]);
					res [i] = bytes [i];
				}
				//Console.WriteLine ();
				return res;			
			}
			if (result < 0) {
				Console.WriteLine ("Timeout...");
			}
			return new byte[0];
       }
		void Sender(byte[] bytes)
        {
            try
            {
                client.Send(bytes);
            }
            catch {}
        }
		byte[] addStringToPacket (byte[] packet, string value)
		{
			byte[] result = new byte[packet.Length + Encoding.BigEndianUnicode.GetByteCount (value) + 2];
			packet.CopyTo (result, 0);
			result [packet.Length] = (byte)(value.Length / 256);
			result [packet.Length + 1] = (byte)(value.Length % 256);
			Encoding.BigEndianUnicode.GetBytes (value).CopyTo(result,packet.Length+2);
			return result;
		}
		byte[] addIntToPacket (byte[] packet, int value)
		{
			byte[] result = new byte[packet.Length + 4];
			packet.CopyTo (result, 0);
			result [packet.Length] = (byte)(value / 256 / 256 / 256 % 256);
			result [packet.Length + 1] = (byte)(value / 256 / 256 % 256);
			result [packet.Length + 2] = (byte)(value / 256 % 256);
			result [packet.Length + 3] = (byte)(value % 256);
			return result;
		}byte[] addShortToPacket (byte[] packet, short value)
		{
			byte[] result = new byte[packet.Length + 2];
			packet.CopyTo (result, 0);
			result [packet.Length] = (byte)(value / 256  % 256);
			result [packet.Length + 1] = (byte)(value % 256);
			return result;
		}
		byte[] addByteToPacket (byte[] packet, byte value)
		{
			byte[] result = new byte[packet.Length+1];
			packet.CopyTo (result, 0);
			result [packet.Length] = value;
			return result;
		}
		byte[] addDoubleToPacket (byte[] packet, double value)
		{
			byte[] result = new byte[packet.Length + 8];
			packet.CopyTo (result, 0);
			byte[] doubleBit = BitConverter.GetBytes (value);
			for (int i = 0; i<8; i++) {
				result [packet.Length + i] = doubleBit [7-i];
			}
			return result;
		}
		byte[] addFloatToPacket (byte[] packet, float value)
		{
			byte[] result = new byte[packet.Length + 4];
			packet.CopyTo (result, 0);
			byte[] doubleBit = BitConverter.GetBytes (value);
			for (int i = 0; i<4; i++) {
				result [packet.Length + i] = doubleBit [3-i];
			}
			return result;
		}
		byte readByteFromPacket (ref byte[] packet)
		{
			needPacket (ref packet, 1);
			byte value = packet [0];
			byte[] result = new byte[packet.Length - 1];
			for (int i = 1; i<packet.Length; i++)
				result [i - 1] = packet [i];
			packet = result;
			return value;
		}
		bool readBoolFromPacket (ref byte[] packet)
		{
			bool value = readByteFromPacket(ref packet)==1;
			return value;
		}
		void needPacket (ref byte[] packet, int needSize)
		{
			if (packet.Length < needSize) {
				byte[] nextPart = new byte[4096];
				int count = client.Receive (nextPart);
				removeNullsFromPacket (ref nextPart, count);
				byte[] newPacket = new byte[packet.Length + count];
				packet.CopyTo (newPacket, 0);
				for (int i =0; i<count; i++) {
					newPacket [packet.Length + i] = nextPart [i];
				}
				packet = newPacket;
				needPacket (ref packet, needSize);
			}
		}
		int readIntFromPacket (ref byte[] packet)
		{
			needPacket (ref packet, 4);
			int value = packet [0];
			value = value * 256 + packet [1];
			value = value * 256 + packet [2];
			value = value * 256 + packet [3];
			byte[] result = new byte[packet.Length - 4];
			for (int i = 4; i<packet.Length; i++)
				result [i - 4] = packet [i];
			packet = result;
			return value;
		}
		double readDoubleFromPacket (ref byte[] packet)
		{
			needPacket (ref packet, 8);
			byte[] number = new byte[8];
			for (int i = 7; i>=0; i--)
				number [7 - i] = packet [i];
			double value = BitConverter.ToDouble (number, 0);
			byte[] result = new byte[packet.Length - 8];
			for (int i = 8; i<packet.Length; i++)
				result [i - 8] = packet [i];
			packet = result;
			return value;
		}
		float readFloatFromPacket (ref byte[] packet)
		{
			needPacket (ref packet, 4);
			byte[] number = new byte[4];
			for (int i = 3; i>=0; i--)
				number [3 - i] = packet [i];
			float value = BitConverter.ToSingle (number, 0);
			byte[] result = new byte[packet.Length - 4];
			for (int i = 4; i<packet.Length; i++)
				result [i - 4] = packet [i];
			packet = result;
			return value;
		}
		int readLongFromPacket (ref byte[] packet)
		{
			needPacket (ref packet, 8);
			int value = packet [0];
			value = value * 256 + packet [1];
			value = value * 256 + packet [2];
			value = value * 256 + packet [3];
			value = value * 256 + packet [4];
			value = value * 256 + packet [5];
			value = value * 256 + packet [6];
			value = value * 256 + packet [7];
			byte[] result = new byte[packet.Length - 8];
			for (int i = 8; i<packet.Length; i++)
				result [i - 8] = packet [i];
			packet = result;
			return value;
		}
		short readShortFromPacket (ref byte[] packet)
		{
			needPacket (ref packet, 2);
			short value = (short)(packet [0]*256 + packet[1]);
			byte[] result = new byte[packet.Length - 2];
			for (int i = 2; i<packet.Length; i++)
				result [i - 2] = packet [i];
			packet = result;
			return value;
		}
		string readStringFromPacket (ref byte[] packet)
		{
			int size = readShortFromPacket (ref packet);
			needPacket (ref packet, size*2);
			string value = Encoding.BigEndianUnicode.GetString (packet, 0, size * 2);
			byte[] result = new byte[packet.Length - size * 2];
			for (int i = size*2; i<packet.Length; i++)
				result [i - size * 2] = packet [i];
			packet = result;
			return value;
		}
		byte[] readBytesFromPacket (ref byte[] packet)
		{
			short size = readShortFromPacket (ref packet);
			needPacket (ref packet, size);
			byte[] value = new byte[size];
			byte[] result = new byte[packet.Length - size];
			for (int i = 0; i<size; i++)
				value [i] = packet [i];
			for (int i = size; i<packet.Length; i++)
				result [i  - size] = packet [i];
			packet = result;
			return value;
		}
		byte[] readBytesFromPacket (ref byte[] packet,int size)
		{
			needPacket (ref packet, size);
			byte[] value = new byte[size];
			byte[] result = new byte[packet.Length - size];
			for (int i = 0; i<size; i++)
				value [i] = packet [i];
			for (int i = size; i<packet.Length; i++)
				result [i  - size] = packet [i];
			packet = result;
			return value;
		}
		int readMetadataFromPacket (ref byte[] packet)
		{
			int size = 0;
			byte curByte = readByteFromPacket (ref packet);
			//int index = 0;
			int ty = 0;
			while (curByte!=127) {
				//index = curByte & 0x1F;
				ty = curByte >> 5;
				switch (ty) {
				case 0:
					readByteFromPacket (ref packet);
					size += 1;
					break;
				case 1:
					readShortFromPacket (ref packet);
					size += 2;
					break;
				case 2:
					readIntFromPacket (ref packet);
					size += 4;
					break;
				case 3:
					readFloatFromPacket (ref packet);
					size += 4;
					break;
				case 4:
					string str = readStringFromPacket (ref packet);
					size += str.Length * 2;
					break;
				case 5:
					readShortFromPacket (ref packet);
					readByteFromPacket (ref packet);
					readShortFromPacket (ref packet);
					size += 1;
					break;
				case 6:
					readIntFromPacket (ref packet);
					readIntFromPacket (ref packet);
					readIntFromPacket (ref packet);
					size += 1;
					break;
				}
				curByte = readByteFromPacket (ref packet);
			}
			return size;
		}
		void removeNullsFromPacket (ref byte[] packet, int count)
		{
			byte[] result = new byte[Math.Min (count, packet.Length)];
			for (int i = 0; i<packet.Length && i<count; i++)
				result [i] = packet [i];
			packet = result;
		}
		void SendPacket (byte[] request)
		{
			try {
				//Console.Write ("Request: ");
				/*for (int i = 0; i<request.Length; i++) {
					Console.Write ("{0} ", Convert.ToString (request [i],16).ToUpper());
				}*/
				//Console.WriteLine ();
				client.Send (request);
			} catch (SocketException e) {
				Console.WriteLine (e.Message);
			}
		}
		int ReceivePacket (byte[] response)
		{
			while (client.Available==0 && client.Connected) {
				if(Console.KeyAvailable==true)
				{
					char key = (char)Console.Read ();
					if (key == 13 || key == 0 || key == 10) {
						processCommand ();
					} else
						consoleinput += key;
				}
			}
			try {
				return client.Receive (response);
			} catch (SocketException e) {
				Console.WriteLine (e.Message);
				return 0;
			}
		}
		void StartSession ()
		{
			client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client.Connect (ip, port);
			if (!client.Connected) {
				Console.WriteLine ("Cannot connect to server");
				return;
			}
			byte[] packet = new byte[0];
			packet = addByteToPacket (packet, 0x02);
			packet = addStringToPacket (packet, nickname + ";main.ttyh.ru:25565");
			SendPacket (packet);
			byte[] response = new byte[4096];
			int result = ReceivePacket (response);
			bool next = processPacket (ref response, result);
			if (!next)
				return;
			packet = new byte[0];
			packet = addByteToPacket (packet, 0x01);
			packet = addIntToPacket (packet, 29);
			packet = addStringToPacket (packet, nickname);
			packet = addStringToPacket (packet, "");
			packet = addIntToPacket (packet, 0);
			packet = addIntToPacket (packet, 0);
			packet = addByteToPacket (packet, 0x00);
			packet = addByteToPacket (packet, 0x00);
			packet = addByteToPacket (packet, 0x00);			
			SendPacket (packet);
			System.Timers.Timer scenario = new System.Timers.Timer ();
			scenario.Elapsed += new ElapsedEventHandler (DisplayTimeEvent);
			scenario.Interval = 100;
			scenario.Start();
			while (next) {
				response = new byte[4096];
				result = ReceivePacket (response);
				//Console.WriteLine ("Packet size: {0}",result);
				next = processPacket (ref response, result);
				while (next && response.Length>0) {
					next = processPacket (ref response, response.Length);
				}
			}
		}
		bool processPacket (ref byte[] packet, int count)
		{
			removeNullsFromPacket (ref packet, count);
			byte packetID = readByteFromPacket (ref packet);
			switch (packetID) {
			case 0x02:
				connectHash = readStringFromPacket (ref packet);
				Console.WriteLine ("Connection hash: {0}", connectHash);
				return true;
			case 0x01:
				int EntityId = readIntFromPacket (ref packet);
				readStringFromPacket (ref packet);
				string levelType = readStringFromPacket (ref packet);
				int serverMode = readIntFromPacket (ref packet);
				int dimension = readIntFromPacket (ref packet);
				byte difficulty = readByteFromPacket (ref packet);
				readByteFromPacket (ref packet);
				byte maxPlayers = readByteFromPacket (ref packet);
				Console.WriteLine ("Entity ID: {0}", EntityId);
				Console.WriteLine ("Level Type: {0}", levelType);
				Console.WriteLine ("Server mode: {0}", serverMode == 0 ? "Survival" : "Creative");
				Console.WriteLine ("Dimension: {0}", dimension == -1 ? "Nether" : (dimension == 0 ? "Overworld" : "End"));
				Console.WriteLine (
					"Difficulty: {0}",
					difficulty == 0 ? "Peaceful" : (difficulty == 1 ? "Easy" : (difficulty == 2 ? "Normal" : "Hard"))
				);
				Console.WriteLine ("Max players: {0}", maxPlayers);
				return true;
			case 0xFF:
				string serverAnswer = readStringFromPacket (ref packet);
				Console.WriteLine ("Dissconnected: {0}", serverAnswer);
				return false;
			case 0x00:
				int keepAliveID = readIntFromPacket (ref packet);
				//Console.WriteLine ("Keep-alive");
				byte[] request = new byte[0];
				request = addByteToPacket (request, 0x00);
				request = addIntToPacket (request, keepAliveID);
				SendPacket (request);
				return true;
			case 0xFA:
				string channel = readStringFromPacket (ref packet);
				byte[] data = readBytesFromPacket (ref packet);
				Console.WriteLine ("Plugins data for {0} size: {1}", channel, data.Length);
				return true;
			case 0x06:
				int X = readIntFromPacket (ref packet);
				int Y = readIntFromPacket (ref packet);
				int Z = readIntFromPacket (ref packet);
				//Console.WriteLine ("Spawn position: X: {0} Y: {1} Z: {2}", X, Y, Z);
				return true;
			case 0xCA:
				bool invulnerability = readBoolFromPacket (ref packet);
				bool isFlying = readBoolFromPacket (ref packet);
				bool canFly = readBoolFromPacket (ref packet);
				bool instantDestroy = readBoolFromPacket (ref packet);
				Console.WriteLine (invulnerability ? "Player cannot take damage" : "Player can't take damage");
				Console.WriteLine (isFlying ? "Player is currently flying" : "Player is't currently flying");
				Console.WriteLine (canFly ? "Player is able to fly" : "Player is't able to fly");
				Console.WriteLine (instantDestroy ? "Player can destroy blocks instantly" : "Player can't destroy blocks instantly");
				return true;
			case 0x04:
				long time = readLongFromPacket (ref packet);
				//Console.WriteLine ("Time: {0}", time);
				return true;
			case 0x03:
				string msg = readStringFromPacket (ref packet);
				Console.WriteLine ("Chat message: {0}", msg);
				return true;
			case 0x0D:
				double x = readDoubleFromPacket (ref packet);
				double stance = readDoubleFromPacket (ref packet);
				double y = readDoubleFromPacket (ref packet);
				double z = readDoubleFromPacket (ref packet);
				float yaw = readFloatFromPacket (ref packet);
				float pitch = readFloatFromPacket (ref packet);
				bool onGround = readBoolFromPacket (ref packet);
				Console.WriteLine ("Absolute position: X:{0:0.##} Y:{1:0.##} Z:{2:0.##} Stance:{3:0.##}", x, y, z, stance);
				Console.WriteLine ("Absolute rotation: X:{0} Y:{1}", yaw, pitch);
				Console.WriteLine ("On ground: {0}", onGround ? "yes" : "no");
				posX = x;
				posY = y;
				posZ = z;
				byte[] res = new byte[0];
				res = addByteToPacket (res, 0x0D);
				res = addDoubleToPacket (res, x);
				res = addDoubleToPacket (res, y);
				res = addDoubleToPacket (res, stance);
				res = addDoubleToPacket (res, z);
				res = addFloatToPacket (res, yaw);
				res = addFloatToPacket (res, pitch);
				res = addByteToPacket (res, (byte)(onGround ? 0x01 : 0x00));
				SendPacket (res);
				isLogged = true;
				return true;
			case 0x0A:
				bool onground = readBoolFromPacket (ref packet);
				Console.WriteLine ("On ground: {0}", onground ? "yes" : "no");
				return true;
			case 0x0B:
				double x1 = readDoubleFromPacket (ref packet);
				double y1 = readDoubleFromPacket (ref packet);
				double stance1 = readDoubleFromPacket (ref packet);
				double z1 = readDoubleFromPacket (ref packet);
				Console.WriteLine ("Absolute position: X:{0} Y:{1} Z:{2} Stance:{3}", x1, y1, z1, stance1);
				return true;
			case 0x19:
				readIntFromPacket (ref packet);
				string title = readStringFromPacket (ref packet);
				int x2 = readIntFromPacket (ref packet);
				int y2 = readIntFromPacket (ref packet);
				int z2 = readIntFromPacket (ref packet);
				int direction = readIntFromPacket (ref packet);
				//Console.WriteLine ("Spawn Painting: {4} X:{0} Y:{1} Z:{2} Direction:{3}", x2, y2, z2, direction, title);
				return true;
			case 0x23:
				readIntFromPacket (ref packet);
				byte headYew = readByteFromPacket (ref packet);
				//Console.WriteLine ("Head yaw: {0} steps", headYew);
				return true;
			case 0x18:
				readIntFromPacket (ref packet);
				byte type = readByteFromPacket (ref packet);
				int x3 = readIntFromPacket (ref packet);
				int y3 = readIntFromPacket (ref packet);
				int z3 = readIntFromPacket (ref packet);
				byte yaw2 = readByteFromPacket (ref packet);
				byte pitch2 = readByteFromPacket (ref packet);
				byte headYaw2 = readByteFromPacket (ref packet);
				int metadata = readMetadataFromPacket (ref packet);
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
				return true;
			case 0x1C:
				readIntFromPacket (ref packet);
				short vX = readShortFromPacket (ref packet);
				short vY = readShortFromPacket (ref packet);
				short vZ = readShortFromPacket (ref packet);
				//Console.WriteLine ("Velocity: X:{0} Y:{1} Z:{1}", vX / 28800.0, vY / 28800.0, vZ / 28800.0);
				return true;
			case 0x14:
				int playerID = readIntFromPacket (ref packet);
				string playerName = readStringFromPacket (ref packet);
				int x4 = readIntFromPacket (ref packet);
				int y4 = readIntFromPacket (ref packet);
				int z4 = readIntFromPacket (ref packet);
				byte yaw3 = readByteFromPacket (ref packet);
				byte pitch3 = readByteFromPacket (ref packet);
				short currentItem = readShortFromPacket (ref packet);
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
				return true;
			case 0x05:
				readIntFromPacket (ref packet);
				short slot = readShortFromPacket (ref packet);
				short itemID = readShortFromPacket (ref packet);
				short damage = readShortFromPacket (ref packet);
				Console.WriteLine ("Equipment: slot:{0} itemID:{1} damage:{2}", slot, itemID, damage);
				return true;
			case 0x15:
				readIntFromPacket (ref packet);
				short Item = readShortFromPacket (ref packet);
				byte _count = readByteFromPacket (ref packet);
				short _damage = readShortFromPacket (ref packet);
				int x5 = readIntFromPacket (ref packet);
				int y5 = readIntFromPacket (ref packet);
				int z5 = readIntFromPacket (ref packet);
				byte rotation = readByteFromPacket (ref packet);
				byte pitch4 = readByteFromPacket (ref packet);
				byte roll = readByteFromPacket (ref packet);
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
				return true;
			case 0x32:
				int x6 = readIntFromPacket (ref packet);
				int y6 = readIntFromPacket (ref packet);
				bool mode = readBoolFromPacket (ref packet);
				//Console.WriteLine ("Need to {0} the chunk X:{1} Y{2}", mode ? "initialize" : "unload", x6, y6);
				return true;
			case 0xC9:
				string name = readStringFromPacket (ref packet);
				bool online = readBoolFromPacket (ref packet);
				short ping = readShortFromPacket (ref packet);
				Console.WriteLine ("Player:{0} ping:{1} {2} game", name, ping, online ? "enter" : "exit");
				return true;
			case 0x68:
				byte windowId = readByteFromPacket (ref packet);
				short count2 = readShortFromPacket (ref packet);
				//Console.Write ("Window items: count:{0}", count2);
				for (int i = 0; i<count2; i++) {
					short id = readShortFromPacket (ref packet);
					if (id != -1) {
						byte itemCount = readByteFromPacket (ref packet);
						short _data = readShortFromPacket (ref packet);
						if (_data != -1)
							readBytesFromPacket (ref packet, _data);
						//Console.Write (" [{0}: id: {1} count:{2} dataSize:{3}]", i, id, itemCount, _data);
					}// else
					//Console.Write (" [{0};{1}]", i, "Empty");
				}
				//Console.WriteLine ();
				return true;
			case 0x67:
				byte winID = readByteFromPacket (ref packet);
				short _slot = readShortFromPacket (ref packet);
				short _id = readShortFromPacket (ref packet);
				if (_id != -1) {
					byte itemCount = readByteFromPacket (ref packet);
					short _data = readShortFromPacket (ref packet);
					if (_data != -1)
						readBytesFromPacket (ref packet, _data);
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
				return true;
			case 0x1A:
				readIntFromPacket (ref packet);
				int x7 = readIntFromPacket (ref packet);
				int y7 = readIntFromPacket (ref packet);
				int z7 = readIntFromPacket (ref packet);
				int count3 = readShortFromPacket (ref packet);
				Console.WriteLine ("Spawn expirience orb: x:{0} y:{1} z:{2} count:{3}", x7, y7, z7, count3);
				return true;
			case 0x1F:
				readIntFromPacket (ref packet);
				byte dx = readByteFromPacket (ref packet);
				byte dy = readByteFromPacket (ref packet);
				byte dz = readByteFromPacket (ref packet);
				//Console.WriteLine ("Entity move: dx:{0} dy:{1} dz:{2}", dx, dy, dz);
				return true;
			case 0x20:
				readIntFromPacket (ref packet);
				byte yaw4 = readByteFromPacket (ref packet);
				byte pitch5 = readByteFromPacket (ref packet);
				//Console.WriteLine ("Entity rotate: yaw:{0} pitch:{1}", yaw4, pitch5);
				return true;
			case 0x1D:
				readIntFromPacket (ref packet);
				//Console.WriteLine ("Destroy entity");
				return true;
			case 0x22:
				readIntFromPacket (ref packet);
				int x8 = readIntFromPacket (ref packet);
				int y8 = readIntFromPacket (ref packet);
				int z8 = readIntFromPacket (ref packet);
				byte yaw8 = readByteFromPacket (ref packet);
				byte pitch8 = readByteFromPacket (ref packet);
				//Console.WriteLine ("Entity teleport: X:{0} Y:{1} Z:{2} yaw:{3} pitch:{4}", x8, y8, z8, yaw8, pitch8);
				return true;
			case 0x36:
				int x9 = readIntFromPacket (ref packet);
				short y9 = readShortFromPacket (ref packet);
				int z9 = readIntFromPacket (ref packet);
				byte byte1 = readByteFromPacket (ref packet);
				byte byte2 = readByteFromPacket (ref packet);
				//Console.WriteLine ("Block action: X:{0} Y:{1} Z:{2} bytes:[{3},{4}]",x9,y9,z9,byte1,byte2);
				return true;
			case 0x21:
				readIntFromPacket (ref packet);
				int x10 = readByteFromPacket (ref packet);
				int y10 = readByteFromPacket (ref packet);
				int z10 = readByteFromPacket (ref packet);
				byte yaw9 = readByteFromPacket (ref packet);
				byte pitch9 = readByteFromPacket (ref packet);
				//Console.WriteLine ("Entity relative look/move: dX:{0} dY:{1} dZ:{2} Yaw:{3} Pitch:{4}", x10, y10, z10, yaw9, pitch9);
				return true;
			case 0x28:
				readIntFromPacket (ref packet);
				int msize = readMetadataFromPacket (ref packet);
				return true;
			case 0x12:
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				return true;
			case 0x17:
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readShortFromPacket (ref packet);
				readShortFromPacket (ref packet);
				readShortFromPacket (ref packet);
				Console.WriteLine ("Spawn object/vehicle");
				return true;
			case 0x26:
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				return true;
			case 0xc8:
				int statisticID = readIntFromPacket (ref packet);
				byte value = readByteFromPacket (ref packet);
				Console.WriteLine ("Statistic {0} change to {1}", statisticID, value);
				return true;
			case 0x08:
				short health = readShortFromPacket (ref packet);
				short food = readShortFromPacket (ref packet);
				float saturation = readFloatFromPacket (ref packet);
				Console.WriteLine ("Update health:{0} food:{1} saturation:{2}", health, food, saturation);
				if (health < 0 ) {
					//isLogged = false;
					byte[] r = new byte[0];
					r = addByteToPacket (r, 0x09);
					r = addIntToPacket (r, 0);
					r = addByteToPacket (r, 1);
					r = addByteToPacket (r, 1);
					r = addShortToPacket (r, 256);
					r = addStringToPacket (r, "default");
					SendPacket (r);
				}
				return true;
			case 0x11:
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readIntFromPacket (ref packet);
				Console.WriteLine ("use bad");
				return true;
			case 0x16:
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				Console.WriteLine ("Someone pick up items");
				return true;
			case 0x1e:
				readIntFromPacket (ref packet);
				return true;
			case 0x27:
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				Console.WriteLine ("Attach player to vehicle");
				return true;
			case 0x29:
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readShortFromPacket (ref packet);
				return true;
			case 0x2A:
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				return true;
			case 0x2B:
				readFloatFromPacket (ref packet);
				readShortFromPacket (ref packet);
				readShortFromPacket (ref packet);
				return true;
			case 0x33:
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readBoolFromPacket (ref packet);
				readShortFromPacket (ref packet);
				readShortFromPacket (ref packet);
				int size = readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readBytesFromPacket (ref packet, size);
				Console.WriteLine ("Chunk uploaded size:{0}", size);
				return true;
			case 0x34:
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readShortFromPacket (ref packet);
				int _size = readIntFromPacket (ref packet);
				readBytesFromPacket (ref packet, _size);
				Console.WriteLine ("Multi block changes size:{0}", _size);
				return true;
			case 0x35:
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readByteFromPacket (ref packet);
				return true;
			case 0x3C:
				readDoubleFromPacket (ref packet);
				readDoubleFromPacket (ref packet);
				readDoubleFromPacket (ref packet);
				readFloatFromPacket (ref packet);
				int size2 = readIntFromPacket (ref packet);
				readBytesFromPacket (ref packet, size2 * 3);
				Console.WriteLine ("Explosion records:{0}", size2);
				return true;
			case 0x3D:
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				return true;
			case 0x46:
				byte byte3 = readByteFromPacket (ref packet);
				byte byte4 = readByteFromPacket (ref packet);
				Console.WriteLine (@"Change game mod: reason: {0} game mod:{1}", byte3, byte4);
				return true;
			case 0x47:
				readIntFromPacket (ref packet);
				readBoolFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				Console.WriteLine ("Thunderbolt!");
				return true;
			case 0x64:
				readByteFromPacket (ref packet);
				readByteFromPacket (ref packet);
				string winTitle = readStringFromPacket (ref packet);
				int slotnum = readByteFromPacket (ref packet);
				Console.WriteLine ("Open window {0} slots:{1}", winTitle, slotnum);
				return true;
			case 0x65:
				readByteFromPacket (ref packet);
				Console.WriteLine ("Close window");
				return true;
			case 0x69:
				readByteFromPacket (ref packet);
				readShortFromPacket (ref packet);
				readShortFromPacket (ref packet);
				return true;
			case 0x83:
				readShortFromPacket (ref packet);
				readShortFromPacket (ref packet);
				int textLength = readByteFromPacket (ref packet);
				byte[] text = readBytesFromPacket (ref packet, textLength);
				Console.WriteLine ("Item data: {0}", Encoding.ASCII.GetString (text));
				return true;
			case 0x84:
				readIntFromPacket (ref packet);
				readShortFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readByteFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				readIntFromPacket (ref packet);
				return true;
			case 0x09:
				int dimension2 = readIntFromPacket (ref packet);
				byte difficulty2 = readByteFromPacket (ref packet);
				byte creativeMode = readByteFromPacket (ref packet);
				short worldHeight = readShortFromPacket (ref packet);
				string levelType2 = readStringFromPacket (ref packet);
				Console.WriteLine ("Level Type: {0}", levelType2);
				Console.WriteLine ("Dimension: {0}", dimension2 == -1 ? "Nether" : (dimension2 == 0 ? "Overworld" : "End"));
				Console.WriteLine (
					"Difficulty: {0}",
					difficulty2 == 0 ? "Peaceful" : (difficulty2 == 1 ? "Easy" : (difficulty2 == 2 ? "Normal" : "Hard"))
				);
				Console.WriteLine ("Creative: {0}", creativeMode);
				Console.WriteLine ("World height: {0}", worldHeight);
				return true;
			case 0x6A:
				readByteFromPacket (ref packet);
				readShortFromPacket (ref packet);
				readBoolFromPacket (ref packet);
				Console.WriteLine ("Confirm transaction");
				return true;
			case 0x6B:
				readShortFromPacket (ref packet);
				short _id2 = readShortFromPacket (ref packet);
				if (_id2 != -1) {
					byte itemCount = readByteFromPacket (ref packet);
					short _data = readShortFromPacket (ref packet);
					if (_data != -1)
						readBytesFromPacket (ref packet, _data);
				}
				return true;
			case 0x82:
				readIntFromPacket (ref packet);
				readShortFromPacket (ref packet);
				readIntFromPacket (ref packet);
				string line1 = readStringFromPacket (ref packet);
				string line2 = readStringFromPacket (ref packet);
				string line3 = readStringFromPacket (ref packet);
				string line4 = readStringFromPacket (ref packet);
				Console.WriteLine ("Update sign:");
				Console.WriteLine (line1);
				Console.WriteLine (line2);
				Console.WriteLine (line3);
				Console.WriteLine (line4);
				return true;
			}
			Console.Write ("Unknown response: {0} ", Convert.ToString (packetID, 16));
			for (int i = 0; i<packet.Length; i++) {
				Console.Write ("{0} ", Convert.ToString (packet[i], 16).ToUpper());
			}
			Console.WriteLine ();
			return false;
		}
		public void DisplayTimeEvent (object source, ElapsedEventArgs e)
		{
			if (isLogged) {
				byte[] sendMessage = new byte[0];
				sendMessage = addByteToPacket (sendMessage, 0x0B);
				sendMessage = addDoubleToPacket (sendMessage, sx [posIndex]);
				sendMessage = addDoubleToPacket (sendMessage, sy [posIndex]);
				sendMessage = addDoubleToPacket (sendMessage, sy [posIndex] + 1.62);
				sendMessage = addDoubleToPacket (sendMessage, sz [posIndex]);
				sendMessage = addByteToPacket (sendMessage, 0x00);
				SendPacket (sendMessage);
				posIndex = (posIndex + 1) % sx.Length;
			}
	    }
	}
}
