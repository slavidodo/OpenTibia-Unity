using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Login
{
    internal class CharacterList
    {
        internal struct World
        {
            public int ID;
            public string Name;
            public string HostName;
            public int Port;
            public bool Preview;
        }

        internal struct Character
        {
            public int WorldID;
            public string Name;
        }

        internal List<World> Worlds { get; private set; } = new List<World>();
        internal List<Character> Characters { get; private set; } = new List<Character>();

        internal int AccountState { get; private set; }
        internal bool IsPremium { get; private set; }
        internal bool InfinitePremium { get; private set; }

        internal ushort PremiumDays { get; private set; }
        internal uint PremiumTimeStamp { get; private set; }

        internal void Parse(Internal.ByteArray message) {
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1010) {
                byte worlds = message.ReadUnsignedByte();
                for (int i = 0; i < worlds; i++) {
                    var world = new World {
                        ID = message.ReadUnsignedByte(),
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
                        WorldID = message.ReadUnsignedByte(),
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
                        WorldID = world.ID
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
                ID = Worlds.Count,
                Name = name,
                HostName = ipAddress,
                Port = port,
                Preview = false
            };

            Worlds.Add(newWorld);
            return newWorld;
        }

        internal World FindWorld(int id) {
            return Worlds.Find((x) => x.ID == id);
        }

        internal World FindWorld(string name) {
            return Worlds.Find((x) => x.Name == name);
        }
    }
}
