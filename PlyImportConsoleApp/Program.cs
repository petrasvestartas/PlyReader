using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileToVox;
using FileToVox.Converter;

namespace PlyImportConsoleApp {
    class Program {
        static void Main(string[] args) {

            string path = @"C:\Scans\CloudCompare\Poisson.ply";
            AbstractToSchematic converter = new PLYToSchematic(path, 1);
       


        }
    }
}
