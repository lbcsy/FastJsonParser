// This file is based on JsonParser but only extract leaf node with path and its string value
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Sys.Text.Json
{
    public class JsonPather
    {
        private const string TypeTag1 = "__type";
        private const string TypeTag2 = "$type";

        private static readonly byte[] HEX = new byte[128];
        private static readonly bool[] HXD = new bool[128];
        private static readonly char[] ESC = new char[128];
        private static readonly bool[] IDF = new bool[128];
        private static readonly bool[] IDN = new bool[128];
        private const int EOF = char.MaxValue + 1;
        private const int ANY = 0;

        private readonly IDictionary<Type, int> rtti = new Dictionary<Type, int>();
        private readonly TypeInfo[] types;

        private readonly Func<int, object>[] parse = new Func<int, object>[128];
        private readonly StringBuilder lsb = new StringBuilder();
        private readonly char[] stc = new char[1];
        private readonly char[] lbf;
        private TextReader str;
        private Func<int, int> Char;
        private Action<int> Next;
        // Read to next character
        private Func<int> Read;
        // Function to skip space(s)
        private Func<int> SkipSpaces;
        private string txt;
        private readonly int lbs;
        private int len;
        private int lln;
        private int chr;
        private int at;

        internal class EnumInfo
        {
            internal string Name;
            internal object Value;
            internal int Len;
        }

        internal class ItemInfo
        {
            internal string Name;
            internal Action<object, JsonPather, int, int> Set;
            internal Type Type;
            internal int Outer;
            internal int Len;
            internal int Atm;
        }

        internal class TypeInfo
        {
            private static readonly HashSet<Type> WellKnown = new HashSet<Type>();

            internal Func<Type, object, object, int, Func<object, object>> Select;
            internal Func<JsonPather, int, object> Parse;
            internal Func<object> Ctor;
            internal EnumInfo[] Enums;
            internal ItemInfo[] Props;
#if FASTER_GETPROPINFO
            internal char[] Mlk;
            internal int Mnl;
#endif
            internal ItemInfo Dico;
            internal ItemInfo List;
            internal bool IsAnonymous;
            internal bool IsNullable;
            internal bool IsStruct;
            internal bool IsEnum;
            internal bool Closed;
            internal Type VType;
            internal Type EType;
            internal Type Type;
            internal int Inner;
            internal int Key;
            internal int T;

            static TypeInfo()
            {
                WellKnown.Add(typeof(bool));
                WellKnown.Add(typeof(char));
                WellKnown.Add(typeof(sbyte));
                WellKnown.Add(typeof(byte));
                WellKnown.Add(typeof(short));
                WellKnown.Add(typeof(ushort));
                WellKnown.Add(typeof(int));
                WellKnown.Add(typeof(uint));
                WellKnown.Add(typeof(long));
                WellKnown.Add(typeof(ulong));
                WellKnown.Add(typeof(float));
                WellKnown.Add(typeof(double));
                WellKnown.Add(typeof(decimal));
                WellKnown.Add(typeof(Guid));
                WellKnown.Add(typeof(DateTime));
                WellKnown.Add(typeof(DateTimeOffset));
                WellKnown.Add(typeof(string));
            }

            private static Func<object> GetCtor(Type clr, bool list)
            {
                var type = !list ? clr == typeof(object) ? typeof(Dictionary<string, object>) : clr : typeof(List<>).MakeGenericType(clr);
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, Type.EmptyTypes, null);
                if (ctor != null)
                {
                    var dyn = new System.Reflection.Emit.DynamicMethod("", typeof(object), null, typeof(string), true);
                    var il = dyn.GetILGenerator();
                    il.Emit(System.Reflection.Emit.OpCodes.Newobj, ctor);
                    il.Emit(System.Reflection.Emit.OpCodes.Ret);
                    return (Func<object>)dyn.CreateDelegate(typeof(Func<object>));
                }
                return null;
            }

            private static Func<object> GetCtor(Type clr, Type key, Type value)
            {
                var type = typeof(Dictionary<,>).MakeGenericType(key, value);
                var ctor = (type != clr && clr.IsClass ? clr : type).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, Type.EmptyTypes, null);
                var dyn = new System.Reflection.Emit.DynamicMethod("", typeof(object), null, typeof(string), true);
                var il = dyn.GetILGenerator();
                il.Emit(System.Reflection.Emit.OpCodes.Newobj, ctor);
                il.Emit(System.Reflection.Emit.OpCodes.Ret);
                return (Func<object>)dyn.CreateDelegate(typeof(Func<object>));
            }

            private static EnumInfo[] GetEnumInfos(Type type)
            {
                var actual = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? type.GetGenericArguments()[0] : type;
                var einfo = Enum.GetNames(actual).ToDictionary(name => name, name => new EnumInfo { Name = name, Value = Enum.Parse(actual, name), Len = name.Length });
                return einfo.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray();
            }

            private ItemInfo GetItemInfo(Type type, string name, MethodInfo setter)
            {
                var method = new System.Reflection.Emit.DynamicMethod("Set" + name, null, new[] { typeof(object), typeof(JsonPather), typeof(int), typeof(int) }, typeof(string), true);
                var nType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? new[] { type.GetGenericArguments()[0] } : null;
                var parse = GetParserParse(GetParseName(type));
                var il = method.GetILGenerator();
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, parse);
                if (type.IsValueType && parse.ReturnType == typeof(object))
                    il.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, type);
                if (parse.ReturnType.IsValueType && type == typeof(object))
                    il.Emit(System.Reflection.Emit.OpCodes.Box, parse.ReturnType);
                if (nType != null)
                {
                    var con = typeof(Nullable<>).MakeGenericType(nType).GetConstructor(nType);
                    if (con != null)
                        il.Emit(System.Reflection.Emit.OpCodes.Newobj, con);
                }
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, setter);
                il.Emit(System.Reflection.Emit.OpCodes.Ret);
                return new ItemInfo { Type = type, Name = name, Set = (Action<object, JsonPather, int, int>)method.CreateDelegate(typeof(Action<object, JsonPather, int, int>)), Len = name.Length };
            }

            private ItemInfo GetItemInfo(Type type, Type key, Type value, MethodInfo setter)
            {
                var method = new System.Reflection.Emit.DynamicMethod("Add", null, new[] { typeof(object), typeof(JsonPather), typeof(int), typeof(int) }, typeof(string), true);
                var sBrace = typeof(JsonPather).GetMethod("SBrace", BindingFlags.Instance | BindingFlags.NonPublic);
                var eBrace = typeof(JsonPather).GetMethod("EBrace", BindingFlags.Instance | BindingFlags.NonPublic);
                var kColon = typeof(JsonPather).GetMethod("KColon", BindingFlags.Instance | BindingFlags.NonPublic);
                var sComma = typeof(JsonPather).GetMethod("SComma", BindingFlags.Instance | BindingFlags.NonPublic);
                var vnType = value.IsGenericType && value.GetGenericTypeDefinition() == typeof(Nullable<>) ? new[] { value.GetGenericArguments()[0] } : null;
                var knType = key.IsGenericType && key.GetGenericTypeDefinition() == typeof(Nullable<>) ? new[] { key.GetGenericArguments()[0] } : null;
                var vParse = GetParserParse(GetParseName(value));
                var kParse = GetParserParse(GetParseName(key));
                var il = method.GetILGenerator();
                il.DeclareLocal(key);
                il.DeclareLocal(value);

                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, sBrace);

                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, kColon);

                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_3);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, kParse);
                if (key.IsValueType && kParse.ReturnType == typeof(object))
                    il.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, key);
                if (kParse.ReturnType.IsValueType && key == typeof(object))
                    il.Emit(System.Reflection.Emit.OpCodes.Box, kParse.ReturnType);
                if (knType != null)
                {
                    var con = typeof(Nullable<>).MakeGenericType(knType).GetConstructor(knType);
                    if (con != null)
                        il.Emit(System.Reflection.Emit.OpCodes.Newobj, con);
                }

                il.Emit(System.Reflection.Emit.OpCodes.Stloc_0);

                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, sComma);

                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, kColon);

                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, vParse);
                if (value.IsValueType && vParse.ReturnType == typeof(object))
                    il.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, value);
                if (vParse.ReturnType.IsValueType && value == typeof(object))
                    il.Emit(System.Reflection.Emit.OpCodes.Box, vParse.ReturnType);
                if (vnType != null)
                {
                    var con = typeof(Nullable<>).MakeGenericType(vnType).GetConstructor(vnType);
                    if (con != null)
                        il.Emit(System.Reflection.Emit.OpCodes.Newobj, con);
                }
                il.Emit(System.Reflection.Emit.OpCodes.Stloc_1);

                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, eBrace);

                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                il.Emit(System.Reflection.Emit.OpCodes.Ldloc_0);
                il.Emit(System.Reflection.Emit.OpCodes.Ldloc_1);
                il.Emit(System.Reflection.Emit.OpCodes.Callvirt, setter);
                il.Emit(System.Reflection.Emit.OpCodes.Ret);
                return new ItemInfo { Type = type, Name = string.Empty, Set = (Action<object, JsonPather, int, int>)method.CreateDelegate(typeof(Action<object, JsonPather, int, int>)) };
            }

            private static Type GetEnumUnderlyingType(Type enumType)
            {
                return enumType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0].FieldType;
            }

            protected string GetParseName(Type type)
            {
                var actual = type.IsGenericType && type.GetGenericTypeDefinition()
                    == typeof(Nullable<>) ? type.GetGenericArguments()[0] : type;
                var name = !WellKnown.Contains(actual) ? actual.IsEnum && WellKnown.Contains(GetEnumUnderlyingType(actual)) ? GetEnumUnderlyingType(actual).Name : null : actual.Name;
                return name != null ? string.Concat("Parse", name) : null;
            }

            protected MethodInfo GetParserParse(string pName)
            {
                return typeof(JsonPather).GetMethod(pName ?? "Val", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            protected TypeInfo(Type type, int self, Type eType, Type kType, Type vType)
            {
                var props = self > 2 ? type.GetProperties(BindingFlags.Instance | BindingFlags.Public) : new PropertyInfo[] { };
                var infos = new Dictionary<string, ItemInfo>();
                IsAnonymous = eType == null && type.Name[0] == '<' && type.IsSealed;
                IsStruct = type.IsValueType;
                IsNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                IsEnum = IsNullable ? (VType = type.GetGenericArguments()[0]).IsEnum : type.IsEnum;
                EType = eType;
                Type = type;
                T = self;
                if (!IsAnonymous)
                {
                    Ctor = kType != null && vType != null ? GetCtor(Type, kType, vType) : GetCtor(EType ?? Type, EType != null);
                    foreach (PropertyInfo property in props)
                    {
                        PropertyInfo pi;
                        MethodInfo set;
                        if ((pi = property).CanWrite && (set = pi.GetSetMethod()).GetParameters().Length == 1)
                            infos.Add(pi.Name, GetItemInfo(pi.PropertyType, pi.Name, set));
                    }
                    Dico = kType != null && vType != null ? GetItemInfo(Type, kType, vType, typeof(Dictionary<,>).MakeGenericType(kType, vType).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public)) : null;
                    List = EType != null ? GetItemInfo(EType, string.Empty, typeof(List<>).MakeGenericType(EType).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public)) : null;
                    Enums = IsEnum ? GetEnumInfos(Type) : null;
                }
                else
                {
                    var args = type.GetConstructors()[0].GetParameters();
                    for (var i = 0; i < args.Length; i++) infos.Add(args[i].Name, new ItemInfo { Type = args[i].ParameterType, Name = args[i].Name, Atm = i, Len = args[i].Name.Length });
                }
                Props = infos.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray();
#if FASTER_GETPROPINFO
                if (Props.Length > 0)
                {
                    Mnl = Props.Max(p => p.Name.Length) + 1;
                    Mlk = new char[Mnl * (Props.Length + 1)];
                    for (var i = 0; i < Props.Length; i++)
                    {
                        var p = Props[i]; var n = p.Name; var l = n.Length;
                        n.CopyTo(0, Mlk, Mnl * i, l);
                    }
                }
                else
                {
                    Mnl = 1;
                    Mlk = new char[1];
                }
#endif
            }
        }

        internal class TypeInfo<T> : TypeInfo
        {
            internal Func<JsonPather, int, T> Value;

            private Func<JsonPather, int, R> GetParseFunc<R>(string pName)
            {
                var parse = GetParserParse(pName ?? "Key");
                if (parse != null)
                {
                    var method = new System.Reflection.Emit.DynamicMethod(parse.Name, typeof(R), new[] { typeof(JsonPather), typeof(int) }, typeof(string), true);
                    var il = method.GetILGenerator();
                    il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                    il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                    il.Emit(System.Reflection.Emit.OpCodes.Callvirt, parse);
                    il.Emit(System.Reflection.Emit.OpCodes.Ret);
                    return (Func<JsonPather, int, R>)method.CreateDelegate(typeof(Func<JsonPather, int, R>));
                }
                return null;
            }

            internal TypeInfo(int self, Type eType, Type kType, Type vType)
                : base(typeof(T), self, eType, kType, vType)
            {
                var value = Value = GetParseFunc<T>(GetParseName(typeof(T)));
                Parse = (parser, outer) => value(parser, outer);
            }
        }

        static JsonPather()
        {
            for (char c = '0'; c <= '9'; c++) { HXD[c] = true; HEX[c] = (byte)(c - 48); }
            for (char c = 'A'; c <= 'F'; c++) { HXD[c] = HXD[c + 32] = true; HEX[c] = HEX[c + 32] = (byte)(c - 55); }
            ESC['/'] = '/'; ESC['\\'] = '\\';
            ESC['b'] = '\b'; ESC['f'] = '\f'; ESC['n'] = '\n'; ESC['r'] = '\r'; ESC['t'] = '\t'; ESC['u'] = 'u';
            for (int c = ANY; c < 128; c++) if (ESC[c] == ANY) ESC[c] = (char)c;
            for (int c = '0'; c <= '9'; c++) IDN[c] = true;
            IDF['_'] = IDN['_'] = true;
            for (int c = 'A'; c <= 'Z'; c++) IDF[c] = IDN[c] = IDF[c + 32] = IDN[c + 32] = true;
        }

        private Exception Error(string message) { return new Exception(string.Format("{0} at {1} (found: '{2}')", message, at, chr < EOF ? "\\" + chr : "EOF")); }
        private void Reset(Func<int> read, Action<int> next, Func<int, int> achar, Func<int> space)
        {
            at = -1; chr = ANY; Read = read; Next = next; Char = achar; SkipSpaces = space;
        }

        private int StreamSpace()
        {
            if (chr > ' ')
                return chr;
            while ((chr = str.Read(stc, 0, 1) > 0 ? stc[0] : EOF) <= ' ')
            {
            }
            return chr;
        }

        private int StreamRead()
        {
            return chr = str.Read(stc, 0, 1) > 0 ? stc[0] : EOF;
        }
        private void StreamNext(int ch)
        {
            if (chr != ch) throw Error("Unexpected character");
            chr = str.Read(stc, 0, 1) > 0 ? stc[0] : EOF;
        }
        private int StreamChar(int ch)
        {
            if (lln >= lbs)
            {
                if (lsb.Length == 0)
                    lsb.Append(new string(lbf, 0, lln));
                lsb.Append((char)ch);
            }
            else
                lbf[lln++] = (char)ch;
            return chr = str.Read(stc, 0, 1) > 0 ? stc[0] : EOF;
        }

        private int StringSpace()
        {
            if (chr > ' ') return chr;
            while (++at < len && (chr = txt[at]) <= ' ')
            {
            }
            return chr;
        }

        private int StringRead() { return chr = ++at < len ? txt[at] : EOF; }
        private void StringNext(int ch)
        {
            if (chr != ch) throw Error("Unexpected character");
            chr = ++at < len ? txt[at] : EOF;
        }
        private int StringChar(int ch)
        {
            if (lln >= lbs)
            {
                if (lsb.Length == 0)
                    lsb.Append(new string(lbf, 0, lln));
                lsb.Append((char)ch);
            }
            else
                lbf[lln++] = (char)ch;
            return chr = ++at < len ? txt[at] : EOF;
        }

        private void CharEsc(int ec)
        {
            int cp = 0, ic = -1, ch;
            if (ec == 'u')
            {
                while (++ic < 4 && (ch = Read()) <= 'f' && HXD[ch]) { cp *= 16; cp += HEX[ch]; }
                if (ic < 4) throw Error("Invalid Unicode character");
                ch = cp;
            }
            else
                ch = ESC[ec];
            Char(ch);
        }


        private string ParseString(int outer)
        {
            var ch = SkipSpaces();
            if (ch == '"')
            {
                Read();
                lsb.Length = 0;
                lln = 0;
                while (true)
                {
                    // closing quote for string
                    if ((ch = chr) == '"')
                    {
                        Read();
                        return lsb.Length > 0 ? lsb.ToString() : new string(lbf, 0, lln);
                    }
                    bool e = ch == '\\';
                    if (e)
                        ch = Read();
                    if (ch < EOF)
                    {
                        if (!e || ch >= 128)
                            Char(ch);
                        else
                            CharEsc(ch);
                    }
                    else
                        break;
                }
            }
            if (ch == 'n')
                return (string)Null(0);
            throw Error(outer >= 0 ? "Bad string" : "Bad key");
        }

        private object Error(int outer) { throw Error($"Bad value @outer={outer}"); }
        private object Null(int outer) { Read(); Next('u'); Next('l'); Next('l');
            return new Dictionary<string, string>() { { "", "false" } };
        }
        private Dictionary<string, string> False(int outer) { Read(); Next('a'); Next('l'); Next('s'); Next('e');
            return new Dictionary<string, string>() { { "", "false" } };
        }
        private Dictionary<string,string> True(int outer) { Read(); Next('r'); Next('u'); Next('e'); 
            return new Dictionary<string, string>() { { "", "true" } }; 
        }

        private Dictionary<string,string> Num(int outer)
        {
            var ch = chr;
            lsb.Length = 0; lln = 0;
            if (ch == '-') ch = Char(ch);
            var b = ch >= '0' && ch <= '9';
            if (b) while (ch >= '0' && ch <= '9') ch = Char(ch);
            if (ch == '.') { ch = Char(ch); while (ch >= '0' && ch <= '9') ch = Char(ch); }
            if (ch == 'e' || ch == 'E')
            {
                ch = Char(ch); if (ch == '-' || ch == '+') ch = Char(ch);
                while (ch >= '0' && ch <= '9') ch = Char(ch);
            }
            if (!b) throw Error("Bad number");
            return new Dictionary<string, string>() { { "", lsb.Length > 0 ? lsb.ToString() : new string(lbf, 0, lln) } };
            
        }

        private Dictionary<string,string> Str(int outer)
        {
            var s = ParseString(0);
            if (outer != 2 || s != null && s.Length == 1)
            {
                var dic = new Dictionary<string, string>
                {
                    { "", s }
                };
                return dic;
            }
            
            throw Error("Bad character");
        }

        private static object Cat(TypeInfo atinfo, object[] atargs)
        {
            foreach (var prop in atinfo.Props.Where(prop => prop.Type.IsValueType && atargs[prop.Atm] == null))
            {
                atargs[prop.Atm] = Activator.CreateInstance(prop.Type);
            }
            return Activator.CreateInstance(atinfo.Type, atargs);
        }

        private object Parse(int typed)
        {
            if (SkipSpaces() != 'n' || !types[typed].IsNullable)
                return types[typed].Type.IsValueType ?
                    types[typed].IsNullable ?
                    types[types[typed].Inner].Parse(this, types[typed].Inner) :
                    types[typed].Parse(this, typed)
                    : Val(typed);
            return Null(0);
        }

        private object Obj(int outer)
        {
            var cached = types[outer]; 
            var isAnon = cached.IsAnonymous; 
            var hash = types[cached.Key];
            var select = cached.Select; 
            var props = cached.Props; 
            var ctor = cached.Ctor;
            var atargs = isAnon ? new object[props.Length] : null;
            var mapper = null as Func<object, object>;
            var keyed = hash.T;
            var ch = chr;
            if (ch != '{') throw Error("Bad object");
            Read();
            ch = SkipSpaces();
            if (ch == '}')
            {
                Read();
                return isAnon ? Cat(cached, atargs) : ctor();
            }
            Dictionary<string,string> obj = null;
            while (ch < EOF)
            {
                ((Dictionary<string,string>) Parse(keyed)).TryGetValue("",out var slot );
                Func<object, object> read = null;
                SkipSpaces();
                Next(':');
                if (slot != null)
                {
                    if (@select == null || (read = @select(cached.Type, obj, slot, -1)) != null)
                    {
                        Dictionary<string,string> val =(Dictionary<string,string>) Parse(cached.Inner);
                        if (obj == null)
                        {
                            
                            obj = new Dictionary<string, string>();
                        }
                        foreach(var kv in val)
                        {
                            obj.Add($"{slot}"+
                                (
                                string.IsNullOrEmpty(kv.Key)?
                                "":
                                kv.Key[0]=='[' ?$"{kv.Key}":$".{kv.Key}"
                                ), kv.Value);
                        }                                                
                    }
                    else
                        Val(0);
                }
                else
                    Val(0);

                mapper = mapper ?? read;
                ch = SkipSpaces();
                if (ch == '}')
                {
                    mapper = mapper ?? Identity;
                    Read();
                    return mapper(isAnon ?
                        Cat(cached, atargs) :
                        
                        obj 
                        ?? ctor());
                }
                Next(',');
                ch = SkipSpaces();
            }
            throw Error("Bad object");
        }

        private Dictionary<string,string> Arr(int outer)
        {
            var cached = types[outer != 0 ? outer : 1]; 
            var select = cached.Select; var dico = cached.Dico != null;
            var mapper = null as Func<object, object>;
            var val = cached.Inner;
            var ch = chr;
            var i = -1;
            if (ch != '[') throw Error("Bad array");
            Read();
            ch = SkipSpaces();
            var obj = new Dictionary<string, string>();
            if (ch == ']')
            {
                Read();
                return obj;
            }
            while (ch < EOF)
            {
                Func<object, object> read = null;
                i++;
                if (ch == 'n' && types[val].IsNullable && !dico)
                {
                    Null(0);
                    obj.Add($"[{i}]", "null");
                }
                else if (dico || @select == null || (read = @select(cached.Type, obj, null, i)) != null)
                {
                    var value=(Dictionary<string,string>) Parse(cached.Inner);
                    foreach (var kv in value)
                    {
                        obj.Add($"[{i}]" + (string.IsNullOrEmpty(kv.Key) ? "" : $".{kv.Key}"), kv.Value);
                    }
                }
                else
                    Val(0);
                mapper = mapper ?? read;
                ch = SkipSpaces();
                if (ch == ']')
                {
                    mapper = mapper ?? Identity;
                    Read();
                    if (!cached.Type.IsArray) return obj;                  
                    return obj;
                }
                Next(',');
                ch = SkipSpaces();
            }
            throw Error("Bad array");
        }

        private object Val(int outer)
        {
            return parse[SkipSpaces() & 0x7f](outer);
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();
            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
                return type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
            return null;
        }

        private static Type Realizes(Type type, Type generic)
        {
            while (true)
            {
                var itfs = type.GetInterfaces();
                if (itfs.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == generic))
                {
                    return type;
                }
                if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
                    return type;
                if (type.BaseType == null) return null;
                type = type.BaseType;
            }
        }

        private static bool GetKeyValueTypes(Type type, out Type key, out Type value)
        {
            var generic = Realizes(type, typeof(Dictionary<,>)) ?? Realizes(type, typeof(IDictionary<,>));
            var kvPair = generic != null && generic.GetGenericArguments().Length == 2;
            value = kvPair ? generic.GetGenericArguments()[1] : null;
            key = kvPair ? generic.GetGenericArguments()[0] : null;
            return kvPair;
        }

        private int Closure(int outer)
        {
            if (types[outer].Closed) return outer;
            var prop = types[outer].Props;
            types[outer].Closed = true;
            foreach (ItemInfo p in prop)
                p.Outer = Entry(p.Type);
            return outer;
        }

        private int Entry(Type type)
        {
            int outer;
            if (!rtti.TryGetValue(type, out outer))
            {
                Type kt, vt;
                bool dico = GetKeyValueTypes(type, out kt, out vt);
                var et = !dico ? GetElementType(type) : null;
                outer = rtti.Count;
                types[outer] = (TypeInfo)Activator.CreateInstance(
                    typeof(TypeInfo<>).MakeGenericType(type),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null, 
                    new object[] { outer, et, kt, vt }, 
                    null);

                rtti.Add(type, outer);
                types[outer].Inner =
                    et != null ?
                        Entry(et) : 
                        dico ?
                            Entry(vt) :
                            types[outer].IsNullable ?
                                Entry(types[outer].VType) : 
                                0;
                if (dico) types[outer].Key = Entry(kt);
            }
            
            return Closure(outer);
        }

        private Dictionary<string,string> DoParse<T>(string input )
        {
            var outer = Entry(typeof(T));
            len = input.Length;
            txt = input;
            Reset(StringRead, StringNext, StringChar, StringSpace);
            return 
                (Dictionary<string,string>)Val(outer);
        }

        private T DoParse<T>(TextReader input )
        {
            var outer = Entry(typeof(T));
            str = input;
            Reset(StreamRead, StreamNext, StreamChar, StreamSpace);
            return typeof(T).IsValueType ? ((TypeInfo<T>)types[outer]).Value(this, outer) : (T)Val(outer);
        }

        public static object Identity(object obj) { return obj; }

        public static readonly Func<object, object> Skip = null;

        public JsonPather() : this(null) { }

        public JsonPather(JsonParserOptions options)
        {
            options = options ?? new JsonParserOptions
            {
                StringBufferLength = byte.MaxValue + 1,
                TypeCacheCapacity = byte.MaxValue + 1
            };

            if (!options.Validate()) throw new ArgumentException("Invalid JSON parser options", "options");

            lbf = new char[lbs = options.StringBufferLength];
            types = new TypeInfo[options.TypeCacheCapacity];
            parse['n'] = Null;
            parse['f'] = False;
            parse['t'] = True;
            parse['0'] = parse['1'] = parse['2'] = parse['3'] = parse['4'] = parse['5'] = parse['6'] = parse['7'] = parse['8'] = parse['9'] = parse['-'] = Num;
            parse['"'] = Str;
            parse['{'] = Obj;
            parse['['] = Arr;
            for (var input = 0; input < 128; input++)
            {
                parse[input] = parse[input] ?? Error;
            }
            Entry(typeof(object));
            Entry(typeof(List<object>));
            Entry(typeof(char));
        }

        public Dictionary<string,string> Parse(string input)
        {             
            return Parse<object>(input); 
        }

        public object Parse(TextReader input) { return Parse<object>(input); }

        public object Parse(Stream input) { return Parse<object>(input); }

        public object Parse(Stream input, Encoding encoding) { return Parse<object>(input, encoding); }

        public object Parse(Stream input, IDictionary<Type, Func<Type, object, object, int, Func<object, object>>> mappers) { return Parse<object>(input, mappers); }

        public object Parse(Stream input, Encoding encoding, IDictionary<Type, Func<Type, object, object, int, Func<object, object>>> mappers) { return Parse<object>(input, encoding, mappers); }

        public Dictionary<string,string> Parse<T>(string input) 
        {
            return Parse(default(T), input);
        }

        public Dictionary<string,string> Parse<T>(T prototype, string input)
        {
            if (input == null) throw new ArgumentNullException("input", "cannot be null");
            return DoParse<T>(input);
        }

        public T Parse<T>(TextReader input)
        { 
            return Parse(default(T), input);
        }

        public T Parse<T>(T prototype, TextReader input)
        {
            if (input == null) throw new ArgumentNullException("input", "cannot be null");
            return DoParse<T>(input);
        }

        public T Parse<T>(Stream input) { return Parse(default(T), input); }

        public T Parse<T>(T prototype, Stream input) { return Parse<T>(input, null as Encoding); }

        public T Parse<T>(Stream input, Encoding encoding) { return Parse(default(T), input, encoding); }

        public T Parse<T>(T prototype, Stream input, Encoding encoding) { return Parse<T>(input, encoding, null); }

        public T Parse<T>(Stream input, IDictionary<Type, Func<Type, object, object, int, Func<object, object>>> mappers) { return Parse(default(T), input, mappers); }

        public T Parse<T>(T prototype, Stream input, IDictionary<Type, Func<Type, object, object, int, Func<object, object>>> mappers) { return Parse<T>(input, null, mappers); }

        public T Parse<T>(Stream input, Encoding encoding, IDictionary<Type, Func<Type, object, object, int, Func<object, object>>> mappers) { return Parse(default(T), input, encoding, mappers); }

        public T Parse<T>(T prototype, Stream input, Encoding encoding, IDictionary<Type, Func<Type, object, object, int, Func<object, object>>> mappers)
        {
            if (input == null) throw new ArgumentNullException("input", "cannot be null");
            return DoParse<T>(encoding != null ? new StreamReader(input, encoding) : new StreamReader(input));
        }
    }
}