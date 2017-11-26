using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DS_TAE_Editor
{
    public class Helper
    {
        public static List<HelperStruct> helpers = new List<HelperStruct> { };

        public static bool ok = true;

        public class HelperStruct
        {
            public uint id;
            public string description;
            public uint bytes;
            public List<string> parameterTypes;
        }

        public static void ReadHelper()
        {
            string[] lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "helper.txt");

            int i = 1;

            while(ok && i < lines.Length)
            {
                HelperStruct helper = new HelperStruct();

                string[] cells = lines[i].Split('\t');

                if (cells.Length <= 3)
                {
                    ok = false;
                }
                else
                {
                    ok &= uint.TryParse(cells[0], out helper.id);
                    ok &= uint.TryParse(cells[2], out helper.bytes);

                    helper.description = cells[1];
                }
                
                helper.parameterTypes = new List<string> { };

                int bytesLeft = (int)helper.bytes;
                int index = 3;

                while (ok && bytesLeft > 0)
                {
                    switch (cells[index])
                    {
                        case "byte":
                        case "ubyte":
                            bytesLeft -= 1;
                            break;

                        case "short":
                        case "ushort":
                            bytesLeft -= 2;
                            break;

                        case "int":
                        case "uint":
                        case "float":
                            bytesLeft -= 4;
                            break;

                        case "long":
                        case "ulong":
                        case "double":
                            bytesLeft -= 8;
                            break;
                        
                        default:
                            ok = false;
                            break;
                    }

                    helper.parameterTypes.Add(cells[index]);

                    index++;                    
                }

                if (bytesLeft < 0) ok = false;

                helpers.Add(helper);

                i++;
            }
        }
    }
}
