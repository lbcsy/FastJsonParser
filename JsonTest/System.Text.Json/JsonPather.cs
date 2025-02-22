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
        private static readonly byte[] HEX = new byte[128];
        private static readonly bool[] HXD = new bool[128];
        private static readonly char[] ESC = new char[128];
        private static readonly bool[] IDF = new bool[128];
        private static readonly bool[] IDN = new bool[128];
        private const int EOF = char.MaxValue + 1;
        private const int ANY = 0;

        private readonly Func<object>[] valueLexer = new Func<object>[128];
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

        private Exception Error(string message) 
        { 
            return new Exception(string.Format("{0} at {1} (found: '{2}')", message, at, chr < EOF ? "\\" + chr : "EOF")); }
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

        private string ParseString()
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
            {
                Null();
                return "null"; 
            }
            throw Error("Bad string" );
        }

        private object Error() { throw Error($"Bad value"); }
        private Dictionary<string,string> Null() { Read(); Next('u'); Next('l'); Next('l');
            return new Dictionary<string, string>() { { "", "null" } };
        }
        private Dictionary<string, string> False() { Read(); Next('a'); Next('l'); Next('s'); Next('e');
            return new Dictionary<string, string>() { { "", "false" } };
        }
        private Dictionary<string,string> True() { Read(); Next('r'); Next('u'); Next('e'); 
            return new Dictionary<string, string>() { { "", "true" } }; 
        }

        private Dictionary<string,string> Num()
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

        private Dictionary<string,string> Str()
        {
            var s = ParseString();
            
            var dic = new Dictionary<string, string>
            {
                { "", s }
            };
            return dic;           
        }

        private object Obj()
        {
            
            var ch = chr;
            if (ch != '{') throw Error("Bad object");
            Read();
            ch = SkipSpaces();
            if (ch == '}')
            {
                Read();
                // empty object
                return new Dictionary<string, string>();
            }
            Dictionary<string,string> obj = null;
            while (ch < EOF)
            {
                ((Dictionary<string,string>) Parse()).TryGetValue("",out var slot );
                SkipSpaces();
                Next(':');
                if (slot != null)
                {                    
                    Dictionary<string,string> val =(Dictionary<string,string>) Parse();
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
                    GetValueByTypeIdx();

                ch = SkipSpaces();
                if (ch == '}')
                {
                    Read();
                    return                         
                        obj 
                        ;
                }
                Next(',');
                ch = SkipSpaces();
            }
            throw Error("Bad object");
        }

        private Dictionary<string,string> Arr()
        {
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
                i++;
                if (ch == 'n' )
                {
                    Null();
                    obj.Add($"[{i}]", "null");
                }
                else 
                {
                    var value=(Dictionary<string,string>) Parse();
                    foreach (var kv in value)
                    {
                        obj.Add($"[{i}]" + (string.IsNullOrEmpty(kv.Key) ? "" : $".{kv.Key}"), kv.Value);
                    }
                }

                ch = SkipSpaces();
                if (ch == ']')
                {
                    Read();
                                    
                    return obj;
                }
                Next(',');
                ch = SkipSpaces();
            }
            throw Error("Bad array");
        }

        private object GetValueByTypeIdx()
        {
            return valueLexer[SkipSpaces() & 0x7f]();
        }

        private Dictionary<string,string> DoParse(string input )
        {
            len = input.Length;
            txt = input;
            Reset(StringRead, StringNext, StringChar, StringSpace);
            return 
                (Dictionary<string,string>)GetValueByTypeIdx();
        }

        private object DoParse(TextReader input )
        {
            str = input;
            Reset(StreamRead, StreamNext, StreamChar, StreamSpace);
            return  GetValueByTypeIdx();
        }

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
            valueLexer['n'] = Null;
            valueLexer['f'] = False;
            valueLexer['t'] = True;
            valueLexer['0'] = valueLexer['1'] = valueLexer['2'] = valueLexer['3'] = valueLexer['4'] = valueLexer['5'] = valueLexer['6'] = valueLexer['7'] = valueLexer['8'] = valueLexer['9'] = valueLexer['-'] = Num;
            valueLexer['"'] = Str;
            valueLexer['{'] = Obj;
            valueLexer['['] = Arr;
            for (var input = 0; input < 128; input++)
            {
                valueLexer[input] = valueLexer[input] ?? Error;
            }           
        }
        private object Parse()
        {
            return GetValueByTypeIdx();
        }
        public object Parse(Stream input, Encoding encoding)
        {
            if (input == null) throw new ArgumentNullException("input", "cannot be null");
            return DoParse(encoding != null ? new StreamReader(input, encoding) : new StreamReader(input));
        }
        
        public Dictionary<string,string> Parse( string input)
        {
            if (input == null) throw new ArgumentNullException("input", "cannot be null");
            return DoParse(input);
        }

        public object Parse( TextReader input)
        {
            if (input == null) throw new ArgumentNullException("input", "cannot be null");
            return DoParse(input);
        }
    }
}