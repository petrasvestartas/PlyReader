using MoreLinq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlyImportConsoleApp {
    public class PLYToSchematic : AbstractToSchematic {
        private readonly List<Block> _blocks = new List<Block>();

        #region Internal data structure

        enum DataProperty {
            Invalid,
            R8, G8, B8, A8,
            R16, G16, B16, A16,
            SingleX, SingleY, SingleZ,
            DoubleX, DoubleY, DoubleZ,
            Data8, Data16, Data32, Data64
        }

        static int GetPropertySize(DataProperty p) {
            switch (p) {
                case DataProperty.R8:
                return 1;
                case DataProperty.G8:
                return 1;
                case DataProperty.B8:
                return 1;
                case DataProperty.A8:
                return 1;
                case DataProperty.R16:
                return 2;
                case DataProperty.G16:
                return 2;
                case DataProperty.B16:
                return 2;
                case DataProperty.A16:
                return 2;
                case DataProperty.SingleX:
                return 4;
                case DataProperty.SingleY:
                return 4;
                case DataProperty.SingleZ:
                return 4;
                case DataProperty.DoubleX:
                return 8;
                case DataProperty.DoubleY:
                return 8;
                case DataProperty.DoubleZ:
                return 8;
                case DataProperty.Data8:
                return 1;
                case DataProperty.Data16:
                return 2;
                case DataProperty.Data32:
                return 4;
                case DataProperty.Data64:
                return 8;
            }
            return 0;
        }

        class DataHeader {
            public List<DataProperty> properties = new List<DataProperty>();
            public int vertexCount = -1;
            public int faceCount = -1;
            public bool binary = true;
        }

        class DataBody {
            public List<Vector3> vertices;
            public List<Color> colors;
            public List<int[]> faces;

            public DataBody(int vertexCount, int faceCount) {
                vertices = new List<Vector3>(vertexCount);
                colors = new List<Color>(vertexCount);
                faces = new List<int[]>(faceCount);
            }

            public void AddPoint(
                float x, float y, float z,
                byte r, byte g, byte b
            //int fa, int fb, int fc
            ) {
                vertices.Add(new Vector3(x, y, z));
                colors.Add(Color.FromArgb(r, g, b));

            }

            public void AddFace(int[] vertexIndices) {
                faces.Add(vertexIndices);
            }



        }

        #endregion

        #region Static Methods
        private static DataHeader ReadDataHeader(StreamReader reader) {
            DataHeader data = new DataHeader();
            int readCount = 0;

            // Magic number line ("ply")
            string line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "ply")
                throw new ArgumentException("Magic number ('ply') mismatch.");

            // Data format: check if it's binary/little endian.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            data.binary = line == "format binary_little_endian 1.0";

            // Read header contents.
            for (bool skip = false; ;) {
                // Read a line and split it with white space.

                

                line = reader.ReadLine();

                Console.WriteLine(line);

                readCount += line.Length + 1;
                if (line == "end_header")
                    break;
                string[] col = line.Split();

                // Element declaration (unskippable)
                if (col[0] == "element") {
                    if (col[1] == "vertex") {
                        data.vertexCount = Convert.ToInt32(col[2]);
                        skip = false;
                    } else if (col[1] == "face") {
                        data.faceCount = Convert.ToInt32(col[2]);
                        skip = false;
                    } else {
                        // Don't read elements other than vertices.
                        skip = true;
                    }
                }

                if (skip)
                    continue;

           

                // Property declaration line
                if (col[0] == "property") {
                    DataProperty prop = DataProperty.Invalid;

                    // Parse the property name entry.
                    switch (col[2]) {
                        case "red":
                        prop = DataProperty.R8;//1
                        break;
                        case "green":
                        prop = DataProperty.G8;//2
                        break;
                        case "blue":
                        prop = DataProperty.B8;//3
                        break;
                        case "alpha":
                        prop = DataProperty.A8;//4
                        break;
                        case "x":
                        prop = DataProperty.SingleX;//9
                        break;
                        case "y":
                        prop = DataProperty.SingleY;//10
                        break;
                        case "z":
                        prop = DataProperty.SingleZ;//11
                        break;
                        //case: "int":
                        //prop = 
                        //case "list":
                        //Console.WriteLine(col[2]);
                        //Console.WriteLine(col[3]);
                        //Console.WriteLine(col[4]);
                        break;

                    }





                    //Console.WriteLine(col[1]);
                    switch (col[1]) {

  
                        // Check the property type.
                        case "list":
                        Console.WriteLine("there is a list " + col[2] + " " + col[3] + " " + col[4]);
                        break;
                 


                        case "char":
                        case "uchar":
                        case "int8":
                        case "uint8": {
                                if (prop == DataProperty.Invalid)
                                    prop = DataProperty.Data8;
                                else if (GetPropertySize(prop) != 1)
                                    throw new ArgumentException("Invalid property type ('" + line + "').");
                                break;
                            }
                        case "short":
                        case "ushort":
                        case "int16":
                        case "uint16": {
                                switch (prop) {
                                    case DataProperty.Invalid:
                                    prop = DataProperty.Data16;
                                    break;
                                    case DataProperty.R8:
                                    prop = DataProperty.R16;
                                    break;
                                    case DataProperty.G8:
                                    prop = DataProperty.G16;
                                    break;
                                    case DataProperty.B8:
                                    prop = DataProperty.B16;
                                    break;
                                    case DataProperty.A8:
                                    prop = DataProperty.A16;
                                    break;
                                }
                                if (GetPropertySize(prop) != 2)
                                    throw new ArgumentException("Invalid property type ('" + line + "').");
                                break;
                            }
                        case "int":
                        case "uint":
                        case "float":
                        case "int32":
                        case "uint32":
                        case "float32": {
                                if (prop == DataProperty.Invalid)
                                    prop = DataProperty.Data32;
                                else if (GetPropertySize(prop) != 4)
                                    throw new ArgumentException("Invalid property type ('" + line + "').");
                                break;
                            }
                        case "int64":
                        case "uint64":
                        case "double":
                        case "float64": {
                                switch (prop) {
                                    case DataProperty.Invalid:
                                    prop = DataProperty.Data64;
                                    break;
                                    case DataProperty.SingleX:
                                    prop = DataProperty.DoubleX;
                                    break;
                                    case DataProperty.SingleY:
                                    prop = DataProperty.DoubleY;
                                    break;
                                    case DataProperty.SingleZ:
                                    prop = DataProperty.DoubleZ;
                                    break;
                                }
                                if (GetPropertySize(prop) != 8)
                                    throw new ArgumentException("Invalid property type ('" + line + "').");
                                break;
                            }

                        default:
                        throw new ArgumentException("Unsupported property type ('" + line + "').");
                    }

                    data.properties.Add(prop);
                }
            }
            Console.WriteLine("N of properties"+ data.properties.Count().ToString());
            // Rewind the stream back to the exact position of the reader.
            reader.BaseStream.Position = readCount;

            return data;
        }


        /// <summary>
        /// Poisson format
        /// </summary>
        /// <param name="header"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static DataBody ReadDataBodyBinary(DataHeader header, BinaryReader reader) {
            DataBody data = new DataBody(header.vertexCount,header.faceCount);

            float x = 0, y = 0, z = 0;
            byte r = 255, g = 255, b = 255, a = 255;
            int f0 = 0, f1 = 0, f2 = 0;

            //for(int i = 0; i < header.vertexCount; i++) {
            //    int m = i % 6;
            //    switch (m) {
            //        case (0):
            //        x = reader.ReadSingle();
            //        break;
            //        case (1):
            //        y = reader.ReadSingle();
            //        break;
            //        case (2):
            //        z = reader.ReadSingle();
            //        break;
            //        case (3):
            //        r = reader.ReadByte();
            //        break;
            //        case (4):
            //        r = reader.ReadByte();
            //        break;
            //        case (5):
            //            r = reader.ReadByte();
            //        break;
            //    }
            //    data.AddPoint(x, y, z, r, g, b);
            //}


            //reader.BaseStream.Position = readCount;
            //Console.WriteLine("Number of properties:   " + header.properties.Count().ToString());
            for (int i = 0; i < header.vertexCount; i++) {//header.vertexCount
                foreach (DataProperty prop in header.properties) {//iterate six properties
                    //Console.WriteLine(prop.ToString());
                    
                    switch (prop) {
                        case DataProperty.R8:
                        r = reader.ReadByte();
                        Console.WriteLine("R8"+r.ToString());
                        break;
                        case DataProperty.G8:
                        g = reader.ReadByte();
                        Console.WriteLine("G8" + g.ToString());
                        break;
                        case DataProperty.B8:
                        b = reader.ReadByte();
                        Console.WriteLine("B8" + b.ToString());
                        break;
                        case DataProperty.A8:
                        a = reader.ReadByte();
                        Console.WriteLine("A8" + a.ToString());
                        break;

                        case DataProperty.R16:
                        r = (byte)(reader.ReadUInt16() >> 8);
                        Console.WriteLine("R16" + r.ToString());
                        break;
                        case DataProperty.G16:
                        g = (byte)(reader.ReadUInt16() >> 8);
                        Console.WriteLine("G16" + g.ToString());
                        break;
                        case DataProperty.B16:
                        b = (byte)(reader.ReadUInt16() >> 8);
                        Console.WriteLine("B16" + b.ToString());
                        break;
                        case DataProperty.A16:
                        a = (byte)(reader.ReadUInt16() >> 8);
                        Console.WriteLine("A16" + a.ToString());
                        break;

                        case DataProperty.SingleX:
                        x = reader.ReadSingle();
                        Console.WriteLine("SingleX" + x.ToString());
                        break;
                        case DataProperty.SingleY:
                        y = reader.ReadSingle();
                        Console.WriteLine("SingleY" + y.ToString());
                        break;
                        case DataProperty.SingleZ:
                        z = reader.ReadSingle();
                        Console.WriteLine("SingleZ" + z.ToString());
                        break;

                        case DataProperty.DoubleX:
                        x = (float)reader.ReadDouble();
                        Console.WriteLine("DoubleX" + x.ToString());
                        break;
                        case DataProperty.DoubleY:
                        y = (float)reader.ReadDouble();
                        Console.WriteLine("DoubleY" + y.ToString());
                        break;
                        case DataProperty.DoubleZ:
                        z = (float)reader.ReadDouble();
                        Console.WriteLine("DoubleZ" + z.ToString());
                        break;

                        case DataProperty.Data8:
                        Console.WriteLine("reader.ReadByte();" );
                        reader.ReadByte();
                        break;
                        case DataProperty.Data16:
                        Console.WriteLine("reader.BaseStream.Position ");
                        reader.BaseStream.Position += 2;
                        break;
                        case DataProperty.Data32:
                        reader.BaseStream.Position += 4;
                        Console.WriteLine("reader.BaseStream.Position ");
                        break;
                        case DataProperty.Data64:
                        reader.BaseStream.Position += 8;
                        Console.WriteLine("reader.BaseStream.Position ");
                        break;

                    }
                }

                data.AddPoint(x, y, z, r, g, b);
            }

            //Header
            //Vertex List
            //Face List
            //(lists of other elements)
            // Console.WriteLine("Hiiiiiiiii");
            int[] f = new int[0];
            int n = 0;
            int counter = 0;
            bool flag = true;
            for (int i = 0; i < header.faceCount; i++) {

                if (flag) {

                    n = reader.ReadInt32();
                    //Console.WriteLine("number of indices " + n);
                    f = new int[n];
                    flag = false;


                } else {
                    int v = reader.ReadInt32();
                    f[counter] = v;
                    //Console.WriteLine("id " + v);
                    counter++;


                    if (counter == n) {
                        data.AddFace(f);
                        counter = 0;
                        flag = true;
                    }



                }


            }
            Console.WriteLine("Hiiiiiiiii");

            return data;
        }

        private static DataBody ReadDataBodyAscii(DataHeader header, StreamReader reader) {
            DataBody data = new DataBody(header.vertexCount, header.faceCount);
            
            string line;
            while ((line = reader.ReadLine()) != null) {
                string[] strings = line.Split(' ');
                if (strings.Length > 6) {
                    try {
                        float x = float.Parse(strings[0], CultureInfo.InvariantCulture);
                        float y = float.Parse(strings[1], CultureInfo.InvariantCulture);
                        float z = float.Parse(strings[2], CultureInfo.InvariantCulture);
                        byte r = byte.Parse(strings[6], CultureInfo.InvariantCulture);
                        byte g = byte.Parse(strings[7], CultureInfo.InvariantCulture);
                        byte b = byte.Parse(strings[8], CultureInfo.InvariantCulture);

                        data.AddPoint(x, y, z, r, g, b);
                    } catch (Exception e) {
                        Console.WriteLine("[ERROR] Line not well formated : " + line + " " + e.Message);
                    }
                }
            }

            return data;
        }
        #endregion

        public PLYToSchematic(string path, int scale) : base(path) {
            FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            DataHeader header = ReadDataHeader(new StreamReader(stream));
            DataBody body;
            body = header.binary ? ReadDataBodyBinary(header, new BinaryReader(stream)) : ReadDataBodyAscii(header, new StreamReader(stream));

            List<Vector3> bodyVertices = body.vertices;
            List<int[]> bodyFaces = body.faces;
            List<Color> bodyColors = body.colors;

            Vector3 minX = bodyVertices.MinBy(t => t.X);
            Vector3 minY = bodyVertices.MinBy(t => t.Y);
            Vector3 minZ = bodyVertices.MinBy(t => t.Z);

            float min = Math.Abs(Math.Min(minX.X, Math.Min(minY.Y, minZ.Z)));
            for (int i = 0; i < bodyVertices.Count; i++) {
                bodyVertices[i] += new Vector3(min, min, min);
                bodyVertices[i] = new Vector3(
                    (float)Math.Truncate(bodyVertices[i].X * scale), 
                    (float)Math.Truncate(bodyVertices[i].Y * scale), 
                    (float)Math.Truncate(bodyVertices[i].Z * scale));
            }

            HashSet<Vector3> set = new HashSet<Vector3>();
            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();

            using (ProgressBar progressbar = new ProgressBar()) {
                for (int i = 0; i < bodyVertices.Count; i++) {
                    if (!set.Contains(bodyVertices[i])) {
                        set.Add(bodyVertices[i]);
                        vertices.Add(bodyVertices[i]);
                        colors.Add(bodyColors[i]);
                    }
                    progressbar.Report(i / (float)bodyVertices.Count);
                }
            }

            minX = vertices.MinBy(t => t.X);
            minY = vertices.MinBy(t => t.Y);
            minZ = vertices.MinBy(t => t.Z);

            min = Math.Min(minX.X, Math.Min(minY.Y, minZ.Z));
            for (int i = 0; i < vertices.Count; i++) {
                float max = Math.Max(vertices[i].X, Math.Max(vertices[i].Y, vertices[i].Z));
                if (/*max - min < 8000 && */max - min >= 0) {
                    vertices[i] -= new Vector3(min, min, min);
                    uint col = (uint)((colors[i].A << 24) | (colors[i].R << 16) | (colors[i].G << 8) | (colors[i].B << 0));
                    _blocks.Add(new Block((ushort)vertices[i].X, (ushort)vertices[i].Y, (ushort)vertices[i].Z, col));
                }
            }

            for (int i = 0; i < 5; i++) {
                Console.WriteLine(vertices[i].ToString());
                Console.WriteLine(colors[i].ToString());
            }

        }



        public override Schematic WriteSchematic() {
            float minX = _blocks.MinBy(t => t.X).X;
            float minY = _blocks.MinBy(t => t.Y).Y;
            float minZ = _blocks.MinBy(t => t.Z).Z;

            float maxX = _blocks.MaxBy(t => t.X).X;
            float maxY = _blocks.MaxBy(t => t.Y).Y;
            float maxZ = _blocks.MaxBy(t => t.Z).Z;

            Schematic schematic = new Schematic() {
                Length = (ushort)(Math.Abs(maxZ - minZ)),
                Width = (ushort)(Math.Abs(maxX - minX)),
                Heigth = (ushort)(Math.Abs(maxY - minY)),
                Blocks = new HashSet<Block>()
            };

            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.WidthSchematic = schematic.Width;
            LoadedSchematic.HeightSchematic = schematic.Heigth;
            /*
            List<Block> list = Quantization.ApplyQuantization(_blocks);
            list.ApplyOffset(new Vector3(minX, minY, minZ))

            foreach (Block t in list) {
                schematic.Blocks.Add(t);
            }
            */

            return schematic;
        }
  

    }
}
