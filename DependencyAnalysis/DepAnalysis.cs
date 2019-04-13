/////////////////////////////////////////////////////////////////////////////////
// depAnalyser.cs  This package is to analysis the dependency between packages //
// version 1.0                                                                 //
// Language: C#    Platform: .net 4.6.2  IDE visual studio                     //
// Application: Dependency analysis                                            //
// Author: Ren Jie  email:jren21@str.edu   Syrcause University                 //
//                  phone: 315-289-0047                                        //
////////////////////////////////////////////////////////////////////////////

/* model operation:
 * ************************
 *  The DepAnalysis package provided a facility to analyze the dependency between all the
 *  files in the given path , include subdictorys
 *  
 *
 * 
 * Public interfaces:
 * ***********************
 *  DepAnalysis analyzer=new DepAnalysis;        //constructor
 *  analyzer.setpath(String path);               // set path
 *  analyzer.buildTypeTable();                   // must be called after path is set
 *  analyzer analyze();                          // must be called after TypeTable is built
 *  Dictionary depndency = analyzer.DependencyTablt;   // get the dependency
 *  analyzer.display(0                                 // print the result to the console
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
using CodeAnalysis;
using CsGraph;
using typeinfo;
using typeanalysis;
using fileManger;
using Lexer;
namespace depAnalysis
{
    public class DepAnalysis
    {
        public CsGraph<String, String> depGraph { get; set; }
        public String dirpath { get; set; }
        public TypeTable table { get; set; }
        public Dictionary<String,HashSet<String>> DependencyTable {get;set;}
       
        public DepAnalysis()
        {
            DependencyTable = new Dictionary<string, HashSet<string>>();
        }
       
        public void setpath(String path)
        {
            dirpath = path;
        }

        public void buildTypeTable()
        {
            String[] dirpath_ = { dirpath };
            table = TypeAnalysis.buildTypeTable(dirpath_);
        }
        public Parser buildDepAnalser()                                     // this function builds a parser with certain Rules and Actions to parse the files again and compare with the TypeTable
        {
            Parser depParser = new Parser();
            return depParser;
        }
        public void analyze()
        {
            if (dirpath == null)
            {
                Console.Write("dirpath not set");
                return;
            }
            buildTypeTable();
            BuildDependencyParser builder = new BuildDependencyParser();
            Parser parser = builder.build();
            List<String> files = FileManger.ProcessDirtory(dirpath);

            foreach(String file in files)
            {
               // Console.Write("\n  Dependency Analyzsis, Processing file {0}\n", System.IO.Path.GetFileName(file));                   //print on the console which file is being processed
                ITokenCollection semi = Factory.create();
                Repository.changeFileName(System.IO.Path.GetFileName(file));                                                          //update the current file name
                Repository.emptyUsingList();                                                                                          // empty the using namespace list whenever start parsing a new file
                Repository.resetAliasList();
                if(!Repository.getInstance().dependencyTable.ContainsKey(System.IO.Path.GetFileName(file)))
                Repository.getInstance().dependencyTable.Add(System.IO.Path.GetFileName(file), new HashSet<string>());                // create a dependency node for each file
                //semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    continue;
                }

                // Console.Write("\n  Type and Function Analysis");
                // Console.Write("\n ----------------------------");
               
                try
                {
                    while (semi.get().Count > 0)
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }

                semi.close();
            }
            Console.Write("\n\n");
            DependencyTable = Repository.getInstance().dependencyTable;
            Repository.clear();

        }
        public void display()
        {
            if (DependencyTable == null)
            {
                Console.WriteLine("no dependency table ");
                return;
            }
            Console.WriteLine("Now displaying dependency list");
            Console.WriteLine("/////////////////////////////////////////");
            foreach(KeyValuePair<string,HashSet<String>> entry in DependencyTable)
            {
                if (entry.Value.Count != 0)
                    Console.WriteLine("----------------------------------------------------");
                foreach(String file in entry.Value)
                {
                    Console.WriteLine("{0,20} {1,25} {2,25}" ,    entry.Key , "  depends on  " , file);
                }
                
            }
                

        }

    }

    public class test
    {
        public static void Main(String[] args)
        {
            DepAnalysis test = new DepAnalysis();
            test.setpath("../../../");
            test.analyze();
            Repository.getInstance().table.display();
            test.display();
        }
    }


}


