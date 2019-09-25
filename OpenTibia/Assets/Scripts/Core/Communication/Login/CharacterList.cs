using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Login
{
    public class CharacterList
    {
        public struct World
        {
            public int _id;
            public string Name;
            public string HostName;
            public int Port;
            public bool Preview;
        }

        public struct Character
        {
            public int WorldId;
            public string Name;
        }

        public List<World> Worlds { get; private set; } = new List<World>();
        public List<Character> Characters { get; private set; } = new List<Character>();

        public int AccountState { get; private set; }
        public bool IsPremium { get; private set; }
        public bool InfinitePremium { get; private set; }

        public ushort PremiumDays { get; private set; }
        public uint PremiumTimeStamp { get; private set; }

        public void Parse(Internal.CommunicationStream message) {
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1010) {
                byte worlds = message.ReadUnsignedByte();
                for (int i = 0; i < worlds; i++) {
                    var world = new World {
                        _id = message.ReadUnsignedByte(),
                        Name = message.ReadString(),
                        HostName = message.ReadString(),
                        Port = message.ReadUnsignedShort(),
                        Preview = message.ReadBoolean()
                    };
                    Worlds.Add(world);
                }

                byte characters = message.ReadUnsignedByte();
                for (int i = 0; i < characters; i++) {
                    Character character = new Character {
                        WorldId = message.ReadUnsignedByte(),
                        Name = message.ReadString()
                    };
                    Characters.Add(character);
                }
            } else {
                byte characters = message.ReadUnsignedByte();
                for (int i = 0; i < characters; i++) {
                    var characterName = message.ReadString();
                    var worldName = message.ReadString();
                    var worldIpLong = message.ReadUnsignedInt();
                    var worldPort = message.ReadUnsignedShort();
                    
                    var world = GetOrCreateWorld(worldName, worldIpLong, worldPort);
                    var character = new Character {
                        Name = characterName,
                        WorldId = world._id
                    };
                    Characters.Add(character);
                }
            }

            uint now = (uint)System.DateTime.Now.Second;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1077) {
                AccountState = message.ReadUnsignedByte();
                IsPremium = message.ReadBoolean();
                PremiumTimeStamp = message.ReadUnsignedInt();
                if (PremiumTimeStamp > now)
                    PremiumDays = (ushort)((PremiumTimeStamp - now) / 86400U);
                else
                    PremiumDays = 0;

                InfinitePremium = (IsPremium && PremiumTimeStamp == 0);
            } else {
                AccountState = 0;
                PremiumDays = message.ReadUnsignedShort();
                if (PremiumDays > 0)
                    PremiumTimeStamp = now + PremiumDays * 86400U;
                else
                    PremiumTimeStamp = 0;

                IsPremium = PremiumDays > 0;
                InfinitePremium = PremiumDays == 65535;
            }
        }

        private World GetOrCreateWorld(string name, uint ip, ushort port) {
            string ipAddress = new System.Net.IPAddress(ip).ToString();
            foreach (var world in Worlds) {
                if (world.Name == name && world.HostName == ipAddress && world.Port == port)
                    return world;
            }

            World newWorld = new World {
                _id = Worlds.Count,
                Name = name,
                HostName = ipAddress,
                Port = port,
                Preview = false
            };

            Worlds.Add(newWorld);
            return newWorld;
        }

        public World FindWorld(int id) {
            return Worlds.Find((x) => x._id == id);
        }

        public World FindWorld(string name) {
            return Worlds.Find((x) => x.Name == name);
        }
    }
}
