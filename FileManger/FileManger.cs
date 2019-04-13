/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// TypeTable.cs  This package is to provide a tool to generate all the .cs file path in the dir and sub dirtory//
// version 1.0                                                                                                 //
// Language: C#    Platform: .net 4.6.2  IDE visual studio                                                     //
// Application: Dependency analysis                                                                            //
// Author: Ren Jie  email:jren21@str.edu   Syrcause University    computer science master student              //
//                  phone: 315-289-0047                                                                        //
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/* model operation:
 * ************************
 * The TypeTable provide a useful tool to generate all the .cs file path in the dir and sub dirtory, 
 * 
 * the out put would be a list of string that contains all the .cs file path in the dirtory
 * 
 * Public interfaces:
 * ***********************
 * list<string> files =FileManger.ProcessDirtory(String dirpath)  
 * 
 * list<string> files = FileManger.ProcessDirtory(String[] dirpath)   // this will take dirpath[0] as the dirpath
 *
 * Maintenance History: 
 * ///////////////////////
 * alpha test 1.0, first release 11/01/2018
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace fileManger
{
    public class FileManger
    {
        public static List<String> ProcessDirtory(String dirpath)                             // this static function  reconize the files that specified by the commdLine arguments
        {
            String[] dirpath_ = { dirpath };
            return ProcessDirtory(dirpath_);
        }
        public static List<string> ProcessDirtory(string[] args)                                
            {
                List<string> files = new List<string>();
                /*
                if (args.Length < 2)
                {
                    Console.Write("\n  Please enter path and file(s) to analyze\n\n");
                    return files;
                }
                */
                string path = args[0];

                if (!Directory.Exists(path))
                {
                    Console.Write("\n  invalid path \"{0}\"", System.IO.Path.GetFullPath(path));
                    return files;
                }

                //path = Path.GetFullPath(path);

                String[] filespace = Directory.GetFiles(path, "*.cs");
                foreach (String file in filespace)
                {
                    files.Add(file);
                }
                String[] subdirectorys = Directory.GetDirectories(path);
                recursiveProcessDictory(subdirectorys, files);

                return files;
            }

            static List<string> recursiveProcessDictory(String[] directorys, List<string> files)
            {
                if (directorys.Length == 0)
                    return files;
                foreach (String path in directorys)                                                  // recursively call the collecting path function in every subdirectory
                {
                    String[] filespace = Directory.GetFiles(path, "*.cs");
                    foreach (String file in filespace)                                              //add all the file paths in subdirectory
                    {
                    if (file.Length > 50)
                        continue;
                        files.Add(file);
                    }
                    String[] subdictory = Directory.GetDirectories(path);                           // find all the subdirectory of the subdirectory
                    recursiveProcessDictory(subdictory, files);
                }
                return files;
            }

            static void ShowCommandLine(string[] args)                                                            // this static function shows the input path
            {
                Console.Write("\n  Commandline args are:\n  ");
                foreach (string arg in args)
                {
                    Console.Write("  {0}", arg);
                }
                Console.Write("\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
                Console.Write("\n");
            }

    }
#if testFileManger
    class test
    {
        public static void Main(String[] args)
        {
            string path = "../../../";
            List<string> files = FileManger.ProcessDirtory(path);
            foreach(String file in files)
            {
                Console.WriteLine(file);
            }
        }

    }
#endif

}
