///////////////////////////////////////////////////////////////////////////////
// StrongComponent.cs      Find the strong connected component in a graph    //                               
// version                 1.0                                               // 
// Language                C#,                                               // 
// Platform                .Net Framework 4.6.1 WIN10, VS2017 Community      // 
// Application            Strong component implement tarjun's algriothim    //
//                                                                           //
// Author                  Ren Jie , Master in Computer Science              //
//                         Syracuse University                               //
//                         (315) 289 0047 jren21@syr.edu                     //
///////////////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * ======================
 * Find the strong connected component in a graph
 * 
 * Public Interface
 * ======================
 * StrongComponent finder StrongComponnet()   //constructor
 * finder.buildGraph (TypeTable table);
 * List<String> res =finder.tarjun();
 * finder.display();
 *
 *   
 * Maintenance History
 * ======================
 * ver1.0 : 11/2
 * - first release
 * 
 * Planned Modifications:
 * ----------------------
 * - 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsGraph;
using depAnalysis;


namespace strongComponent
{
    public class StrongComponent
    {
        public  CsGraph<String, String> graph { get; set; }
        CsNode<String, String> prev { get; set; }
        public StrongComponent()
        {
            graph = new CsGraph<string, string>("mygraph");
        }

        // build a graph with the file dependency table
        public void buildgraph(Dictionary<String, HashSet<String>> depTable)
        {
            foreach (KeyValuePair<String, HashSet<String>> entry in depTable)
            {
                graph.addNode(new CsNode<string, string>(entry.Key));
            }
            foreach (KeyValuePair<String, HashSet<String>> entry in depTable)

            {
                CsNode<String, String> node1 = find(entry.Key);
                foreach (String file in entry.Value)
                {
                    node1.addChild(find(file), "null");
                }
            }

        }

        public CsNode<String, String> find(String filename)
        {
            foreach (CsNode<String, String> node in graph.adjList)
            {
                if (filename == node.name)
                    return node;
            }
            throw new NullReferenceException();
        }
        public List<List<String>> tarjan()                                 // tarjan function must be runned after buildgragh
        {
            List<List<String>> res = new List<List<string>>();
            int i = 0;
            foreach (CsNode<String, String> node in graph.adjList)
            {
                node.visited = false;
                node.ID = i;                                               // unmark all the node and assign each node a id
                i++;
            }


            foreach (CsNode<String, String> node in graph.adjList)
            {
                tarjan(node);                                             // notice DFS here
            }
            Dictionary<int, List<String>> dic = new Dictionary<int, List<string>>();
            foreach (CsNode<String, String> node in graph.adjList)
            {
                if (dic.ContainsKey(node.low_num))                                        // collect all the node with the same low_nubmer in to a dictionary, it will be easier to collect them into list later. 
                    dic[node.low_num].Add(node.name);
                else
                {
                    List<String> list = new List<string>();
                    list.Add(node.name);
                    dic.Add(node.low_num, list);
                }
            }

            foreach (KeyValuePair<int, List<String>> entry in dic)
            {
                res.Add(entry.Value);
            }

            return res;
        }

        private void tarjan(CsNode<String, String> node)                             // tarjan's algorithm 
        {
            if (node.visited)
            {
                prev = node;
                return;
            }
            node.low_num = node.ID;                                               //
            graph.seen.Push(node);
            node.visited = true;
            foreach (CsEdge<String, String> child in node.children)
            {
                tarjan(child.targetNode);
                //Console.WriteLine(graph.seen.Contains(prev));
                if (graph.seen.Contains(prev))
                {
                    node.low_num = Math.Min(node.low_num, prev.low_num);
                }
            }
            if (node.low_num == node.ID)
                while (graph.seen.Pop() != node) {
                }
            prev = node;
            return;

        }

        public void display()
        {
            foreach (CsNode<String, String> node in graph.adjList)
            {

                Console.WriteLine(node.name + " depends on");
                foreach (CsEdge<String, String> edge in node.children)
                {
                    Console.WriteLine("           " + edge.targetNode.name);
                }
                Console.WriteLine("--------------------------------------------------");
            }
        }

         #if testscc
        public class test
        {
            static void Main(string[] args)
            {
                DepAnalysis analyzer = new DepAnalysis();
                analyzer.setpath("../../../");
                analyzer.analyze();
                Dictionary<String, HashSet<String>> dep = analyzer.DependencyTable;
                StrongComponent scc = new StrongComponent();
                scc.buildgraph(dep);
                // scc.display();
                List<List<String>> res = scc.tarjan();
                foreach (List<String> list in res)
                {
                    Console.Write("{");
                    foreach (String file in list)
                    {
                        Console.Write(" [" + file + "] ");
                    }
                    Console.WriteLine("}");
                }

                foreach (CsNode<String, String> node in scc.graph.adjList)
                {
                    Console.WriteLine(node.low_num);
                }


            }
        }
           #endif
    }
   
}
