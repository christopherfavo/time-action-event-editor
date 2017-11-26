using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DS_TAE_Editor
{
    public class TAE
    {
        public class Tae
        {
            public Header header;
            public Data data;

            public string err;
        }

        public class Header
        {
            public byte[] signature; // [4],TAE.
            public uint unk1;
            public uint unk2;
            public uint fileSize;
            public byte[] unk3; // [64]

            public uint id;

            public uint idCount;
            public uint idsOffset;
            public uint groupsOffset;

            public uint unk4;

            public uint dataCount;
            public uint dataOffset;
            public byte[] unk5; // [40]
            public uint filenamesOffset;
        }

        public class Data
        {
            public ulong unk;
            public NameStruct skeletonHkxName;
            public NameStruct SibName;
            public List<IdStruct> ids;

            public uint groupCount;
            public uint groupDataOffset;
            public List<GroupStruct> groups;

            public List<AnimDataStruct> animDatas;
        }

        public class NameStruct
        {
            public uint offset;
            public string name;
        }

        public class IdStruct
        {
            public uint id;
            public uint offset;
        }

        public class GroupStruct
        {
            public uint firstId;
            public uint lastId;
            public uint firstIdOffset;
        }

        public class AnimDataStruct
        {
            public uint eventCount;
            public uint eventHeaderOffset;

            public ulong unk;

            public uint timeConstantsCount;
            public uint timeConstantsOffset;

            public uint animFileOffset;

            public List<EventStruct> events;
            public AnimFileStruct animFile;
            public List<float> timeConstants;
        }

        public class EventStruct
        {
            public uint startTimeOffset;
            public float startTime;

            public uint endTimeOffset;
            public float endTime;

            public uint eventbodyOffset;

            public uint eventType;
            public uint eventParamOffset;

            public List<byte[]> parameters;
        }

        public class AnimFileStruct
        {
            public uint type;
            public uint dataOffset;
            public NameStruct name;
            public uint nextFileOffset;
            public uint linkedAnimId;
            public uint unk1;
            public uint unk2;
            public uint nulla;
        }

        public static Tae ReadTae(string path)
        {
            Tae tae = new Tae();

            BinaryReader file = new BinaryReader(File.Open(path, FileMode.Open));

            tae.header = new Header();
            tae.data = new Data();

            tae.header.signature = file.ReadBytes(4);

            if (Encoding.ASCII.GetString(tae.header.signature) == "TAE ")
            {
                tae.header.unk1 = file.ReadUInt32();
                tae.header.unk2 = file.ReadUInt32();

                tae.header.fileSize = file.ReadUInt32();
                tae.header.unk3 = file.ReadBytes(64);
                tae.header.id = file.ReadUInt32();

                tae.header.idCount = file.ReadUInt32();
                tae.header.idsOffset = file.ReadUInt32();
                tae.header.groupsOffset = file.ReadUInt32();

                tae.header.unk4 = file.ReadUInt32();

                tae.header.dataCount = file.ReadUInt32();
                tae.header.dataOffset = file.ReadUInt32();

                tae.header.unk5 = file.ReadBytes(40);
                tae.header.filenamesOffset = file.ReadUInt32();

                tae.data.skeletonHkxName = new NameStruct();
                tae.data.SibName = new NameStruct();

                file.BaseStream.Seek(tae.header.filenamesOffset, SeekOrigin.Begin);
                tae.data.skeletonHkxName.offset = file.ReadUInt32();
                tae.data.SibName.offset = file.ReadUInt32();

                tae.data.unk = file.ReadUInt64();

                file.BaseStream.Seek(tae.data.skeletonHkxName.offset, SeekOrigin.Begin);
                tae.data.skeletonHkxName.name = MyReadChars(file);

                file.BaseStream.Seek(tae.data.SibName.offset, SeekOrigin.Begin);
                tae.data.SibName.name = MyReadChars(file);

                file.BaseStream.Seek(tae.header.idsOffset, SeekOrigin.Begin);

                tae.data.ids = new List<IdStruct> { };

                for (int i = 0; i < tae.header.idCount; i++)
                {
                    IdStruct id = new IdStruct();

                    id.id = file.ReadUInt32();
                    id.offset = file.ReadUInt32();

                    tae.data.ids.Add(id);
                }

                file.BaseStream.Seek(tae.header.groupsOffset, SeekOrigin.Begin);

                tae.data.groupCount = file.ReadUInt32();
                tae.data.groupDataOffset = file.ReadUInt32();

                file.BaseStream.Seek(tae.data.groupDataOffset, SeekOrigin.Begin);

                tae.data.groups = new List<GroupStruct> { };

                for (int i = 0; i < tae.data.groupCount; i++)
                {
                    GroupStruct group = new GroupStruct();

                    group.firstId = file.ReadUInt32();
                    group.lastId = file.ReadUInt32();
                    group.firstIdOffset = file.ReadUInt32();

                    tae.data.groups.Add(group);
                }

                file.BaseStream.Seek(tae.header.dataOffset, SeekOrigin.Begin);

                tae.data.animDatas = new List<AnimDataStruct> { };

                long currpos = 0;

                for (int i = 0; i < tae.header.dataCount; i++)
                {
                    AnimDataStruct animData = new AnimDataStruct();

                    animData.eventCount = file.ReadUInt32();
                    animData.eventHeaderOffset = file.ReadUInt32();
                    animData.unk = file.ReadUInt64();
                    animData.timeConstantsCount = file.ReadUInt32();
                    animData.timeConstantsOffset = file.ReadUInt32();
                    animData.animFileOffset = file.ReadUInt32();

                    currpos = file.BaseStream.Position;

                    file.BaseStream.Seek(animData.animFileOffset, SeekOrigin.Begin);

                    animData.animFile = new AnimFileStruct();
                    animData.animFile.name = new NameStruct();

                    animData.animFile.type = file.ReadUInt32();
                    animData.animFile.dataOffset = file.ReadUInt32();
                    
                    if (animData.animFile.type == 0)
                    {
                        animData.animFile.name.offset = file.ReadUInt32();
                    }
                    else
                    {
                        animData.animFile.nextFileOffset = file.ReadUInt32();
                        animData.animFile.linkedAnimId = file.ReadUInt32();
                    }

                    animData.animFile.unk1 = file.ReadUInt32();
                    animData.animFile.unk2 = file.ReadUInt32();
                    animData.animFile.nulla = file.ReadUInt32();

                    if (animData.animFile.type == 0)
                    {
                        file.BaseStream.Seek(animData.animFile.name.offset, SeekOrigin.Begin);
                        animData.animFile.name.name = MyReadChars(file);
                    }

                    animData.events = new List<EventStruct> { };

                    for (int j = 0; j < animData.eventCount; j++)
                    {
                        EventStruct Event = new EventStruct();

                        file.BaseStream.Seek(animData.eventHeaderOffset + j * 12, SeekOrigin.Begin);

                        Event.startTimeOffset = file.ReadUInt32();
                        Event.endTimeOffset = file.ReadUInt32();
                        Event.eventbodyOffset = file.ReadUInt32();

                        file.BaseStream.Seek(Event.startTimeOffset, SeekOrigin.Begin);
                        Event.startTime = file.ReadSingle();
                        Event.startTimeOffset -= animData.timeConstantsOffset;

                        file.BaseStream.Seek(Event.endTimeOffset, SeekOrigin.Begin);
                        Event.endTime = file.ReadSingle();
                        Event.endTimeOffset -= animData.timeConstantsOffset;

                        file.BaseStream.Seek(Event.eventbodyOffset, SeekOrigin.Begin);

                        Event.eventType = file.ReadUInt32();
                        Event.eventParamOffset = file.ReadUInt32();

                        file.BaseStream.Seek(Event.eventParamOffset, SeekOrigin.Begin);

                        Event.parameters = ReadParams(Event.eventType, file);

                        if (Event.parameters[0] == null)
                        {
                            tae.err = "Unrecognized event type: " + BitConverter.ToInt32(Event.parameters[1],0) + "\n\nPosition in the file: " + file.BaseStream.Position;
                            file.Close();
                            return tae;
                        }

                        animData.events.Add(Event);
                    }

                    file.BaseStream.Seek(animData.timeConstantsOffset, SeekOrigin.Begin);

                    animData.timeConstants = new List<float> { };

                    for (int j = 0; j < animData.timeConstantsCount; j++)
                    {
                        animData.timeConstants.Add(file.ReadSingle());
                    }

                    file.BaseStream.Seek((uint)currpos, SeekOrigin.Begin);

                    tae.data.animDatas.Add(animData);
                }
            }
            else tae.err = "Specified file is not a TAE file.";

            file.Close();
            
            return tae;
        }

        public static void WriteTae(Tae tae, string path)
        {
            BinaryWriter file = new BinaryWriter(File.Open(path, FileMode.Create));

            int c = 0;

            while (c < 2)
            {
                file.Write(tae.header.signature);
                file.Write(tae.header.unk1);
                file.Write(tae.header.unk2);
                file.Write(tae.header.fileSize);
                file.Write(tae.header.unk3);
                file.Write(tae.header.id);
                file.Write(tae.header.idCount);
                file.Write(tae.header.idsOffset);
                file.Write(tae.header.groupsOffset);
                file.Write(tae.header.unk4);
                file.Write(tae.header.dataCount);
                file.Write(tae.header.dataOffset);
                file.Write(tae.header.unk5);
                file.Write(tae.header.filenamesOffset);

                tae.header.filenamesOffset = (uint)file.BaseStream.Position;

                file.Write(tae.data.skeletonHkxName.offset);
                file.Write(tae.data.SibName.offset);

                file.Write(tae.data.unk);

                tae.data.skeletonHkxName.offset = (uint)file.BaseStream.Position;
                file.Write(Encoding.UTF8.GetBytes(tae.data.skeletonHkxName.name));

                tae.data.SibName.offset = (uint)file.BaseStream.Position;
                file.Write(Encoding.UTF8.GetBytes(tae.data.SibName.name));

                tae.header.idsOffset = (uint)file.BaseStream.Position;

                foreach (IdStruct id in tae.data.ids)
                {
                    for (int i = 0; i < tae.data.groupCount; i++)
                    {
                        if (id.id == tae.data.groups[i].firstId) tae.data.groups[i].firstIdOffset = (uint)file.BaseStream.Position;
                    }
                    file.Write(id.id);
                    file.Write(id.offset);
                }

                tae.header.groupsOffset = (uint)file.BaseStream.Position;

                file.Write(tae.data.groupCount);
                file.Write((uint)file.BaseStream.Position + 4);

                foreach (GroupStruct group in tae.data.groups)
                {
                    file.Write(group.firstId);
                    file.Write(group.lastId);
                    file.Write(group.firstIdOffset);
                }

                tae.header.dataOffset = (uint)file.BaseStream.Position;

                for (int i = 0; i < tae.data.animDatas.Count; i++)
                {
                    tae.data.ids[i].offset = (uint)file.BaseStream.Position;
                    file.Write(tae.data.animDatas[i].eventCount);
                    file.Write(tae.data.animDatas[i].eventHeaderOffset);
                    file.Write(tae.data.animDatas[i].unk);
                    file.Write(tae.data.animDatas[i].timeConstantsCount);
                    file.Write(tae.data.animDatas[i].timeConstantsOffset);
                    file.Write(tae.data.animDatas[i].animFileOffset);
                }

                for (int i = 0; i < tae.data.animDatas.Count; i++)
                {
                    tae.data.animDatas[i].animFileOffset = (uint)file.BaseStream.Position;

                    file.Write(tae.data.animDatas[i].animFile.type);
                    file.Write(tae.data.animDatas[i].animFile.dataOffset);

                    tae.data.animDatas[i].animFile.dataOffset = (uint)file.BaseStream.Position;

                    if (tae.data.animDatas[i].animFile.type == 0)
                    {
                        file.Write(tae.data.animDatas[i].animFile.name.offset);
                    }
                    else
                    {
                        file.Write(tae.data.animDatas[i].animFile.nextFileOffset);
                        file.Write(tae.data.animDatas[i].animFile.linkedAnimId);
                    }

                    file.Write(tae.data.animDatas[i].animFile.unk1);
                    file.Write(tae.data.animDatas[i].animFile.unk2);
                    file.Write(tae.data.animDatas[i].animFile.nulla);

                    if (tae.data.animDatas[i].animFile.type == 0) //&& tae.data.animDatas[i].animFile.name.name != Encoding.UTF8.GetString(new byte[2]))
                    {
                        tae.data.animDatas[i].animFile.name.offset = (uint)file.BaseStream.Position;

                        file.Write(Encoding.UTF8.GetBytes(tae.data.animDatas[i].animFile.name.name));
                    }

                    if (tae.data.animDatas[i].animFile.type == 1)
                    {
                        tae.data.animDatas[i].animFile.nextFileOffset = (uint)file.BaseStream.Position;
                    }

                    tae.data.animDatas[i].timeConstantsOffset = (uint)file.BaseStream.Position;

                    foreach (float timeConstant in tae.data.animDatas[i].timeConstants)
                    {
                        file.Write(timeConstant);
                    }

                    tae.data.animDatas[i].eventHeaderOffset = (uint)file.BaseStream.Position;

                    foreach (EventStruct Event in tae.data.animDatas[i].events)
                    {
                        uint offset = tae.data.animDatas[i].timeConstantsOffset;
                        file.Write(offset + Event.startTimeOffset);
                        file.Write(offset + Event.endTimeOffset);
                        file.Write(Event.eventbodyOffset);
                    }

                    for (int j = 0; j < tae.data.animDatas[i].events.Count; j++)
                    {
                        tae.data.animDatas[i].events[j].eventbodyOffset = (uint)file.BaseStream.Position;

                        file.Write(tae.data.animDatas[i].events[j].eventType);
                        file.Write(tae.data.animDatas[i].events[j].eventParamOffset);

                        tae.data.animDatas[i].events[j].eventParamOffset = (uint)file.BaseStream.Position;

                        file.Write(ConcatParams(tae.data.animDatas[i].events[j].parameters));
                    }

                }

                c++;

                file.BaseStream.Seek(0, SeekOrigin.End);
                tae.header.fileSize = (uint)file.BaseStream.Position;

                file.BaseStream.Seek(0, SeekOrigin.Begin);

            }
                        
            file.Close();

        }

        private static string MyReadChars(BinaryReader file)
        {
            List<byte> chars = new List<byte> { };

            byte c1 = new byte();
            byte c2 = new byte();

            do
            {
                c1 = file.ReadByte();
                c2 = file.ReadByte();
                chars.Add(c1);
                chars.Add(c2);
            }
            while (c1 != 0x00 || c2 != 0x00);

            return Encoding.UTF8.GetString(chars.ToArray());
        }

        private static List<byte[]> ReadParams(uint eventType, BinaryReader file)
        {
            List<byte[]> parameters = new List<byte[]> { };

            if (Helper.ok)
            {
                bool found = false;
                int index = 0;

                while(!found && index < Helper.helpers.Count)
                {
                    if (Helper.helpers[index].id == eventType)
                    {
                        foreach (string parameterType in Helper.helpers[index].parameterTypes)
                        {
                            switch (parameterType)
                            {
                                case "byte":
                                case "ubyte":
                                    parameters.Add(new byte[1]);
                                    break;

                                case "short":
                                case "ushort":
                                    parameters.Add(new byte[2]);
                                    break;

                                case "int":
                                case "uint":
                                case "float":
                                    parameters.Add(new byte[4]);
                                    break;

                                case "long":
                                case "ulong":
                                case "double":
                                    parameters.Add(new byte[8]);
                                    break;

                                default:
                                    break;
                            }
                        }
                        found = true;
                    }
                    else
                    {
                        index++;
                    }
                }

                if (!found)
                {
                    return new List<byte[]> { null, BitConverter.GetBytes(eventType) };
                }
            }
            else
            {
                switch (eventType)
                {
                    //Parameter count: 1
                    case 32:
                    case 33:
                    case 65:
                    case 66:
                    case 67:
                    case 101:
                    case 110:
                    case 145:
                    case 224:
                    case 225:
                    case 226:
                    case 229:
                    case 231:
                    case 232:
                    case 301:
                    case 302:
                    case 308:
                    case 401:
                        parameters = new List<byte[]> { new byte[4] };
                        break;

                    //Parameter count: 2
                    case 5:
                    case 64:
                    case 112:
                    case 121:
                    case 128:
                    case 193:
                    case 233:
                    case 304:
                        parameters = new List<byte[]> { new byte[4], new byte[4] };
                        break;

                    //Parameter count: 3
                    case 0:
                    case 1:
                    case 96:
                    case 100:
                    case 104:
                    case 109:
                    case 114:
                    case 115:
                    case 116:
                    case 118:
                    case 144:
                    case 228:
                    case 236:
                    case 307:
                        parameters = new List<byte[]> { new byte[4], new byte[4], new byte[4] };
                        break;

                    //Parameter count: 4
                    case 2:
                    case 16:
                    case 24:
                    case 130:
                    case 300:
                        parameters = new List<byte[]> { new byte[4], new byte[4], new byte[4], new byte[4] };
                        break;

                    //Parameter count: 6
                    case 120:
                        parameters = new List<byte[]> { new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], };
                        break;

                    //Parameter count: 12
                    case 8:
                        parameters = new List<byte[]> { new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], new byte[4], new byte[4] };
                        break;

                    default:
                        return new List<byte[]>{ null, BitConverter.GetBytes(eventType)};                        
                }
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i] = file.ReadBytes(parameters[i].Length);
            }

            return parameters;
        }

        private static byte[] ConcatParams(List<byte[]> parameters)
        {
            int length = 0;

            foreach (byte[] parameter in parameters)
            {
                length += parameter.Length;
            }

            byte[] result = new byte[length];

            int offset = 0;

            foreach (byte[] parameter in parameters)
            {
                Array.Copy(parameter, 0, result, offset, parameter.Length);
                offset += parameter.Length;
            }

            return result;
        }

        public static AnimDataStruct AddEvent(AnimDataStruct animData)
        {
            TAE.EventStruct newEvent = new TAE.EventStruct();

            newEvent.parameters = new List<byte[]> { };
            newEvent.parameters.Add(new byte[4] { 0x0, 0x0, 0x0, 0x0 });
            newEvent.parameters.Add(new byte[4] { 0x0, 0x0, 0x0, 0x0 });
            newEvent.parameters.Add(new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF });

            animData.events.Add(newEvent);
            animData.eventCount++;

            return animData;
        }
    }
}
