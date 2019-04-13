////////////////////////////////////////////////////////////////////////////
// TypeTable.cs  This package is to provide a tool for dependency analysis//
// version 1.0                                                            //
// Language: C#    Platform: .net 4.6.2  IDE visual studio                //
// Application: Dependency analysis                                       //
// Author: Ren Jie  email:jren21@str.edu   Syrcause University            //
//                  phone: 315-289-0047                                   //
////////////////////////////////////////////////////////////////////////////

/* model operation:
 * ************************
 * The TypeTable provide a useful tool to store the information of tpye names, 
 * their namespaces and files.
 * 
 * The parser should stores those information while parsing all the target project
 * files.
 * 
 * Public interfaces:
 * ***********************
 * TypeTable mytable=new TypeTable();                                        // constructor
 * void add(String name ,String type, String namespace,String file);         // add the name, namespace it belongs and file it belongs
 * bool containsKey(String)                                                  // the key should be the type 
 * void display()                                                            // print the whole type table to the console
 *
 * Maintenance History: 
 * ///////////////////////
 * alpha test 1.0, first release 10/03/2018
 * */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace typeinfo
{
    public class TypeTable
    {
        private Dictionary<String, List<TypeTableEle>> table;

        public TypeTable()
        {
            table = new Dictionary<string, List<TypeTableEle>>();
        }

        public Boolean containsKey(String key)
        {
            return table.ContainsKey(key);
        }

        public void add(String name, String type, String namespace_, String file_)
        {
            TypeTableEle element = new TypeTableEle( type, namespace_, file_);

            if (!containsKey(name))
            {
                List<TypeTableEle> list = new List<TypeTableEle>();
                list.Add(element);
                table.Add(name, list);
            }
            else
            {
                table[name].Add(element);
            }
        }
        public List<TypeTableEle> get(String key)
        {
            return table[key];
        }
     
        public void display()
        {
            Console.WriteLine("Now displaying the TypeTable/////////////////////////////////////////////////");
            Console.WriteLine("{0,10}{1,20}{2,20}{3,20}", "Name", "type","namespace", "file");
            Console.WriteLine("{0,10}{1,20}{2,20}{3,20}", "--------", "----","---------", "----");
            foreach (KeyValuePair<String, List<TypeTableEle>> entry in table)
            {
                Console.WriteLine("--------------------------------------------------------------------------");
                Console.WriteLine("{0,10}", entry.Key);
                foreach (TypeTableEle element in entry.Value)
                {
                    Console.WriteLine("{0,10} {1,20} {2,20} {3,20}", "" ,"["+ element.type_,element.namespace_, element.file_+"]");
                }
                Console.Write("\n\n");
            }

        }






    }
    public class TypeTableEle
    {
        public String namespace_ { get; set; }
        public String file_ { get; set; }
        public String type_ { get; set; }
        //public String type { get; set; }
        public TypeTableEle(String type, String namespace_, String file_)
        {
            this.namespace_ = namespace_;
            this.file_ = file_;
            this.type_ = type;
        }
        


    }



#if testtypetable
    class typetableTest
    {
        public static void Main(String[] args)
        {
            
            TypeTable table = new TypeTable();
            table.display();
            Console.WriteLine("Now add information to the table");
            table.add("TypeTable","class", "typeinfo0", "TypeTable");
            table.add("Tokernizer","class" ,"Lexer", "Toker");
            table.add("TypeTable", "interface","typeinfo1", "anotherFile");
            table.display();
        }
    }
#endif
}