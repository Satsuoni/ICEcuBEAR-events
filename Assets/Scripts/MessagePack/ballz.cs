#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Resolvers
{
    using System;
    using MessagePack;

    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(7)
            {
                {typeof(global::System.Collections.Generic.List<global::ballIntegratedData>), 0 },
                {typeof(global::System.Collections.Generic.List<global::SavedEventData>), 1 },
                {typeof(global::ballIntegratedData), 2 },
                {typeof(global::eventDesc), 3 },
                {typeof(global::fullEventData), 4 },
                {typeof(global::SavedEventData), 5 },
                {typeof(global::SavedEventsSettings), 6 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.ListFormatter<global::ballIntegratedData>();
                case 1: return new global::MessagePack.Formatters.ListFormatter<global::SavedEventData>();
                case 2: return new MessagePack.Formatters.ballIntegratedDataFormatter();
                case 3: return new MessagePack.Formatters.eventDescFormatter();
                case 4: return new MessagePack.Formatters.fullEventDataFormatter();
                case 5: return new MessagePack.Formatters.SavedEventDataFormatter();
                case 6: return new MessagePack.Formatters.SavedEventsSettingsFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612



#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace MessagePack.Formatters
{
    using System;
    using MessagePack;


    public sealed class ballIntegratedDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::ballIntegratedData>
    {

        public int Serialize(ref byte[] bytes, int offset, global::ballIntegratedData value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 6);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.affected);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.mintime);
            offset += MessagePackBinary.WriteSingle(ref bytes, offset, value.maxtime);
            offset += formatterResolver.GetFormatterWithVerify<double[]>().Serialize(ref bytes, offset, value.icharge, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.stId);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.domId);
            return offset - startOffset;
        }

        public global::ballIntegratedData Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __affected__ = default(bool);
            var __mintime__ = default(float);
            var __maxtime__ = default(float);
            var __icharge__ = default(double[]);
            var __stId__ = default(int);
            var __domId__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __affected__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 1:
                        __mintime__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 2:
                        __maxtime__ = MessagePackBinary.ReadSingle(bytes, offset, out readSize);
                        break;
                    case 3:
                        __icharge__ = formatterResolver.GetFormatterWithVerify<double[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __stId__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __domId__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::ballIntegratedData();
            ____result.affected = __affected__;
            ____result.mintime = __mintime__;
            ____result.maxtime = __maxtime__;
            ____result.icharge = __icharge__;
            ____result.stId = __stId__;
            ____result.domId = __domId__;
            return ____result;
        }
    }


    public sealed class eventDescFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::eventDesc>
    {

        public int Serialize(ref byte[] bytes, int offset, global::eventDesc value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 6);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.run);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.evn);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.baseDesc, formatterResolver);
            offset += MessagePackBinary.WriteDouble(ref bytes, offset, value.energy);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.eventDate, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.humName, formatterResolver);
            return offset - startOffset;
        }

        public global::eventDesc Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __run__ = default(long);
            var __evn__ = default(long);
            var __baseDesc__ = default(string);
            var __energy__ = default(double);
            var __eventDate__ = default(string);
            var __humName__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __run__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 1:
                        __evn__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 2:
                        __baseDesc__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __energy__ = MessagePackBinary.ReadDouble(bytes, offset, out readSize);
                        break;
                    case 4:
                        __eventDate__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __humName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::eventDesc();
            ____result.run = __run__;
            ____result.evn = __evn__;
            ____result.baseDesc = __baseDesc__;
            ____result.energy = __energy__;
            ____result.eventDate = __eventDate__;
            ____result.humName = __humName__;
            return ____result;
        }
    }


    public sealed class fullEventDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::fullEventData>
    {

        public int Serialize(ref byte[] bytes, int offset, global::fullEventData value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.eventName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::ballIntegratedData>>().Serialize(ref bytes, offset, value.ballData, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.isteps);
            offset += formatterResolver.GetFormatterWithVerify<global::eventDesc>().Serialize(ref bytes, offset, value.description, formatterResolver);
            return offset - startOffset;
        }

        public global::fullEventData Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __eventName__ = default(string);
            var __ballData__ = default(global::System.Collections.Generic.List<global::ballIntegratedData>);
            var __isteps__ = default(int);
            var __description__ = default(global::eventDesc);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __eventName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __ballData__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::ballIntegratedData>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __isteps__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __description__ = formatterResolver.GetFormatterWithVerify<global::eventDesc>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::fullEventData();
            ____result.eventName = __eventName__;
            ____result.ballData = __ballData__;
            ____result.isteps = __isteps__;
            ____result.description = __description__;
            return ____result;
        }
    }


    public sealed class SavedEventDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::SavedEventData>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SavedEventDataFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "description", 0},
                { "hashname", 1},
                { "csvName", 2},
                { "csvHash", 3},
                { "integrationSteps", 4},
                { "status", 5},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("description"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("hashname"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("csvName"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("csvHash"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("integrationSteps"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("status"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::SavedEventData value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 6);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += formatterResolver.GetFormatterWithVerify<global::eventDesc>().Serialize(ref bytes, offset, value.description, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.hashname, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[2]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.csvName, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[3]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.csvHash, formatterResolver);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[4]);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.integrationSteps);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[5]);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.status, formatterResolver);
            return offset - startOffset;
        }

        public global::SavedEventData Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __description__ = default(global::eventDesc);
            var __hashname__ = default(string);
            var __csvName__ = default(string);
            var __csvHash__ = default(string);
            var __integrationSteps__ = default(int);
            var __status__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __description__ = formatterResolver.GetFormatterWithVerify<global::eventDesc>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __hashname__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __csvName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __csvHash__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __integrationSteps__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __status__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::SavedEventData();
            ____result.description = __description__;
            ____result.hashname = __hashname__;
            ____result.csvName = __csvName__;
            ____result.csvHash = __csvHash__;
            ____result.integrationSteps = __integrationSteps__;
            ____result.status = __status__;
            return ____result;
        }
    }


    public sealed class SavedEventsSettingsFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::SavedEventsSettings>
    {

        readonly global::MessagePack.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public SavedEventsSettingsFormatter()
        {
            this.____keyMapping = new global::MessagePack.Internal.AutomataDictionary()
            {
                { "numberIntegrated", 0},
                { "numberKeptAsCsv", 1},
                { "eventData", 2},
            };

            this.____stringByteKeys = new byte[][]
            {
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("numberIntegrated"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("numberKeptAsCsv"),
                global::MessagePack.MessagePackBinary.GetEncodedStringBytes("eventData"),
                
            };
        }


        public int Serialize(ref byte[] bytes, int offset, global::SavedEventsSettings value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedMapHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[0]);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.numberIntegrated);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[1]);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.numberKeptAsCsv);
            offset += global::MessagePack.MessagePackBinary.WriteRaw(ref bytes, offset, this.____stringByteKeys[2]);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::SavedEventData>>().Serialize(ref bytes, offset, value.eventData, formatterResolver);
            return offset - startOffset;
        }

        public global::SavedEventsSettings Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadMapHeader(bytes, offset, out readSize);
            offset += readSize;

            var __numberIntegrated__ = default(uint);
            var __numberKeptAsCsv__ = default(uint);
            var __eventData__ = default(global::System.Collections.Generic.List<global::SavedEventData>);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.MessagePackBinary.ReadStringSegment(bytes, offset, out readSize);
                offset += readSize;
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __numberIntegrated__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __numberKeptAsCsv__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __eventData__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::SavedEventData>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                
                NEXT_LOOP:
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::SavedEventsSettings();
            ____result.numberIntegrated = __numberIntegrated__;
            ____result.numberKeptAsCsv = __numberKeptAsCsv__;
            ____result.eventData = __eventData__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
