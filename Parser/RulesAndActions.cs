///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.3                                                           //
// Language:    C#, 2018, .Net Framework 4.62                         //
// Platform:    Win10, visual studio                                 //
// Application:  project 3 Parser                                    //
// Author:       Ren Jie, computer science master student             //
//               (315) 289 0047, jren21@syr.edu                       // 
//                                                                    //
//               Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.5 :11/1 2018
 * add more rules and actions to build Typetable and run dependency analysis 
 * actions added includes writeTypeTable   addDependency   addUsingNamespace 
 * Rules added includes AddUsingNamespace DetectEnum DetectDelegate DetectAlias DetectDependency DetectUsingNamespace 
 * 
 * addm more building functions to build parsers for different purpose buildTypetableParser, BuildDependencyParser
 * 
 * 
 * ver 2.4 : 09 Oct 2018
 * - modified comments
 * - removed unnecessary definition from repository class
 * - moved local semi definition inside display test in PopStack action
 * ver 2.3 : 30 Sep 2014
 * - added scope-based complexity analysis
 *   Note: doesn't detect braceless scopes yet
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 */
using System;
using typeinfo;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lexer;


namespace CodeAnalysis
{
    ///////////////////////////////////////////////////////////////////
    // Repository class
    // - Specific to each application
    // - holds results of processing
    // - ScopeStack holds current state of scope processing
    // - List<Elem> holds start and end line numbers for each scope
    ///////////////////////////////////////////////////////////////////

    public class Repository
    {
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();
        TypeTable table_ = new TypeTable();                          // the repository holds a TypeTable
        Stack<String> namespaceStack_ = new Stack<string>();         // the repository holds a namespaceStack to track and determine the current namespace scope
        Dictionary<String, HashSet<String>> dependencyTable_ = new Dictionary<string, HashSet<string>>(); // the repository holds a dependency table
        public HashSet<String> usingList { get; set; }
        public HashSet<String> aliasList { get; set; }
        static Repository instance;

        public Repository()
        {
            if (instance != null)
                this.table = instance.table;
            if (instance != null)
                this.dependencyTable = instance.dependencyTable;
            instance = this;
        }
        public String filename { get; set; }                           // the repositoy holds a file name that is currently parsing
                                                                      

        public static Repository getInstance()                         //----< provides all code access to Repository >-------------------
        {
            return instance;
        }
        public static void changeFileName(String name)
        {
            instance.filename = name;
        }

        //----< provides all actions access to current semiExp >-----------

        public ITokenCollection semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount(); }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }

        //----< enables recursively tracking entry and exit from scopes >--

        public int scopeCount
        {
            get;
            set;
        }

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }

        // the locations table is the result returned by parser's actions
        // in this demo

        public List<Elem> locations
        {
            get { return locations_; }
            set { locations_ = value; }
        }
        public TypeTable table                  // the repository holds a TypeTable
        {

            get { return table_; }
            set { table_ = value; }
        }
        public Stack<String> namespaceStack {
            get { return namespaceStack_; }
            set { namespaceStack_ = value; }
        }
        public Dictionary<String, HashSet<String>> dependencyTable {
            get { return dependencyTable_; }
            set { dependencyTable_ = value; }
        }
        public static void clear()                             //clear the reporstoy
        {
            instance.table = new TypeTable();
            instance.dependencyTable = new Dictionary<string, HashSet<string>>();
            instance = new Repository();
        }

        public static void emptyUsingList()
        {
            instance.usingList = new HashSet<string>();
        }
        public static void resetAliasList()
        {
            instance.aliasList = new HashSet<string>();
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Define Actions
    ///////////////////////////////////////////////////////////////////
    // - PushStack
    // - PopStack
    // - PrintFunction
    // - PrintSemi
    // - SaveDeclar

    ///////////////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope
    // - pushes element with type and name onto stack
    // - records starting line number

    public class PushStack : AAction
  {
    public PushStack(Repository repo)
    {
      repo_ = repo;
    }

    public override void doAction(ITokenCollection semi)
    {
      ++repo_.scopeCount;
      Elem elem = new Elem();
      elem.type = semi[0];     // expects type, i.e., namespace, class, struct, ..
      elem.name = semi[1];     // expects name
      elem.beginLine = repo_.semi.lineCount() - 1;
      elem.endLine = 0;        // will be set by PopStack action
      elem.beginScopeCount = repo_.scopeCount;
      elem.endScopeCount = 0;  // will be set by PopStack action
      repo_.stack.push(elem);

            if (semi[0] == "namespace")                                // if the type is detected as a namespace, we push that namespace name in a stack to track the current namespace scope
                repo_.namespaceStack.Push(semi[1]);

      // display processing details if requested

      if (AAction.displayStack)
        repo_.stack.display();
      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
        Console.Write("entering ");
        string indent = new string(' ', 2 * repo_.stack.count);
        Console.Write("{0}", indent);
        this.display(semi); // defined in abstract action
      }

      // add starting location if namespace, type, or function

      if (elem.type == "control" || elem.name == "anonymous")
        return;
      repo_.locations.Add(elem);
    }
  }
  ///////////////////////////////////////////////////////////////////
  // pops scope info from stack when leaving scope
  // - records end line number and scope count

  public class PopStack : AAction
  {
    public PopStack(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(ITokenCollection semi)
    {
      Elem elem;
      try
      {
        // if stack is empty (shouldn't be) pop() will throw exception

        elem = repo_.stack.pop();

                // record ending line count and scope level
                if (elem.type == "namespace")                                  // if a end of namespace scope detected, we pop the namespaceStack
                    repo_.namespaceStack.Pop();                                  
        for (int i = 0; i < repo_.locations.Count; ++i )
        {
          Elem temp = repo_.locations[i];
          if (elem.type == temp.type)
          {
            if (elem.name == temp.name)
            {
              if ((repo_.locations[i]).endLine == 0)
              {
                (repo_.locations[i]).endLine = repo_.semi.lineCount();
                (repo_.locations[i]).endScopeCount = repo_.scopeCount;
                break;
              }
            }
          }
        }
      }
      catch
      {
        return;
      }
      
      if (AAction.displaySemi)
      {
        Lexer.ITokenCollection local = Factory.create();
        local.add(elem.type).add(elem.name);
        if (local[0] == "control")
          return;

        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount());
        Console.Write("leaving  ");
        string indent = new string(' ', 2 * (repo_.stack.count + 1));
        Console.Write("{0}", indent);
        this.display(local); // defined in abstract action
      }
    }
  }
  ///////////////////////////////////////////////////////////////////
  // action to print function signatures - not used in demo

  public class PrintFunction : AAction
  {
    public PrintFunction(Repository repo)
    {
      repo_ = repo;
    }
    public override void display(Lexer.ITokenCollection semi)
    {
      Console.Write("\n    line# {0}", repo_.semi.lineCount() - 1);
      Console.Write("\n    ");
      for (int i = 0; i < semi.size(); ++i)
      {
        if (semi[i] != "\n")
          Console.Write("{0} ", semi[i]);
      }
    }
    public override void doAction(ITokenCollection semi)
    {
      this.display(semi);
    }
  }
  ///////////////////////////////////////////////////////////////////
  // ITokenCollection printing action, useful for debugging

  public class PrintSemi : AAction
  {
    public PrintSemi(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(ITokenCollection semi)
    {
      Console.Write("\n  line# {0}", repo_.semi.lineCount() - 1);
      this.display(semi);
    }
  }
  ///////////////////////////////////////////////////////////////////
  // display public declaration

  public class SaveDeclar : AAction
  {
    public SaveDeclar(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(ITokenCollection semi)
    {
      Elem elem = new Elem();
      elem.type = semi[0];  // expects type
      elem.name = semi[1];  // expects name
      elem.beginLine = repo_.lineCount;
      elem.endLine = elem.beginLine;
      elem.beginScopeCount = repo_.scopeCount;
      elem.endScopeCount = elem.beginScopeCount;
      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.lineCount - 1);
        Console.Write("entering ");
        string indent = new string(' ', 2 * repo_.stack.count);
        Console.Write("{0}", indent);
        this.display(semi); // defined in abstract action
      }
      repo_.locations.Add(elem);
    }
  }

    // implement a new action to write the TypeTable
    public class writeTypeTable : AAction
    {
        public writeTypeTable(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            String file = repo_.filename;
            String namespace_ = repo_.namespaceStack.Peek();
            String type = semi[0];
            TypeTableEle e = new TypeTableEle(type, namespace_, file);
            repo_.table.add(semi[1],semi[0],namespace_,file);
        }

    }

    // This action is to add a detected dependency to the dependency table
    public class addDependency : AAction
    {
        public addDependency(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            String file1 = semi[0];                                 // file1 dependes on file2
            String file2 = semi[1];
            if (!repo_.dependencyTable[file1].Contains(file2))
            {
                repo_.dependencyTable[semi[0]].Add(semi[1]);
            }
            
        }
    }

    //This action is to add the currently using namespace to a list. 
    public class addUsingNamespace : AAction
    {
        public addUsingNamespace(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(ITokenCollection semi)
        {
            int index;       
            semi.find(";", out index);
            repo_.usingList.Add(semi[index - 1]);
        }
    }



    // updata the current
  ///////////////////////////////////////////////////////////////////
  // Define Rules
  ///////////////////////////////////////////////////////////////////
  // - DetectNamespace
  // - DetectClass
  // - DetectFunction
  // - DetectAnonymousScope
  // - DetectPublicDeclaration
  // - DetectLeavingScope

  ///////////////////////////////////////////////////////////////////
  // rule to detect namespace declarations

  public class DetectNamespace : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      int index;
      semi.find("namespace", out index);
      if (index != -1 && semi.size() > index + 1)
      {
        ITokenCollection local = Factory.create();
        // create local semiExp with tokens for type and name
        local.add(semi[index]).add(semi[index + 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // rule to dectect class definitions

  public class DetectClass : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      int indexCL;
      semi.find("class", out indexCL);
      int indexIF;
      semi.find("interface", out indexIF);
      int indexST;
      semi.find("struct", out indexST);

      int index = Math.Max(indexCL, indexIF);
      index = Math.Max(index, indexST);
      if (index != -1 && semi.size() > index + 1)
      {
     
        ITokenCollection local = Factory.create();
        // local semiExp with tokens for type and name
        local.add(semi[index]).add(semi[index + 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }
/// /////////////////////////
/// rule to detect enum definitions

        public class DetectEnum : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int index;
            semi.find("enum", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    public class DetectDelegate : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int index;
            semi.find("delegate", out index);
            if (index != -1 && semi.size() > index + 2){
                ITokenCollection local = Factory.create();
                local.add(semi[index]).add(semi[index + 2]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    public class DetectAlias : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int indexusing;
            int indexeq;
            semi.find("using", out indexusing);
            semi.find("=",out indexeq);
            if (indexeq != -1 && semi.size() > indexusing + 1&&indexusing!=-1)
            {
                ITokenCollection local = Factory.create();
                local.add("alias").add(semi[indexusing + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
  ///////////////////////////////////////////////////////////////////
  // rule to dectect function definitions

  public class DetectFunction : ARule
  {
    public static bool isSpecialToken(string token)
    {
      string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
      foreach (string stoken in SpecialToken)
        if (stoken == token)
          return true;
      return false;
    }
    public override bool test(ITokenCollection semi)
    {
      if (semi[semi.size() - 1] != "{")
        return false;

      int index;
      semi.find("(", out index);
      if (index > 0 && !isSpecialToken(semi[index - 1]))
      {
        ITokenCollection local = Factory.create();
        local.add("function").add(semi[index - 1]);
        doActions(local);
        return true;
      }
      return false;
    }
       
  }
  ///////////////////////////////////////////////////////////////////
  // detect entering anonymous scope
  // - expects namespace, class, and function scopes
  //   already handled, so put this rule after those

  public class DetectAnonymousScope : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      int index;
      semi.find("{", out index);
      if (index != -1)
      {
        ITokenCollection local = Factory.create();
        // create local semiExp with tokens for type and name
        local.add("control").add("anonymous");
        doActions(local);
        return true;
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // detect public declaration

  public class DetectPublicDeclar : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      int index;
      semi.find(";", out index);
      if (index != -1)
      {
        semi.find("public", out index);
        if (index == -1)
          return true;
        ITokenCollection local = Factory.create();
        // create local semiExp with tokens for type and name
        //local.displayNewLines = false;
        local.add("public "+semi[index+1]).add(semi[index+2]);

        semi.find("=", out index);
        if (index != -1)
        {
          doActions(local);
          return true;
        }
        semi.find("(", out index);
        if(index == -1)
        {
          doActions(local);
          return true;
        }
      }
      return false;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // detect leaving scope

  public class DetectLeavingScope : ARule
  {
    public override bool test(ITokenCollection semi)
    {
      int index;
      semi.find("}", out index);
      if (index != -1)
      {
        doActions(semi);
        return true;
      }
      return false;
    }
  }
    //detect dependency
    public class DetectDependency : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Repository rep = Repository.getInstance();
            if (semi[0][0] == '/')                                                      // comments will not cause any dependency
                return false;
            foreach (String token in semi)
            {
                if (token == "\n" || token == "}" ||token=="{")
                    continue;                                                            //ignore tokens that is impossible to cause a dependency
                if (rep.table.containsKey(token )  &&  isUsingType(semi,token))          //only when we make sure it is using a type can we determine it is using a type. not a instance name or function name
                {                                                                        // for example : we need to eliminate the cases like        int ITokenCollection =0; or       public void ITokenCollection()
                    List<TypeTableEle> list = rep.table.get(token);
                    ITokenCollection local = Factory.create();
                    if (list.Count == 1 && list[0].file_ != rep.filename)
                    {
                        local.add(rep.filename).add(list[0].file_);                  // if there is only 1 element in the TypeTable[key], dependency detected and add it to teh dependencyTable
                        doActions(local);
                    }
                    else if(list.Count > 1)
                    {
                        foreach(TypeTableEle ele in list)
                        {
                            if (rep.usingList.Contains(ele.namespace_))
                            {
                                local.add(rep.filename).add(ele.file_);              // if there are more than 1 element in the TypeTable[key], find the one with the namespace that is currently using and add it to dependency table
                                doActions(local);
                            }
                        }
                    }
                   
                }
            }
            return false;
            
        }
        public bool isUsingType(ITokenCollection semi, String token) 
        {
            int index;
            semi.find(token,out index);
            if ( semi.size()>index+1 && (semi[index + 1] == ":" ||semi[index+1]=="="||semi[index+1][0]=='('))        // if the token is a variable name or function name, return false
                 return false;
            return true;
        }
    }

    public class DetectUsingNamespace : ARule
    {
        public override bool test (ITokenCollection semi)
        {
            int index;
            if(semi.find("using",out index)&&index==0&&!semi.find("=",out index))
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }


  ///////////////////////////////////////////////////////////////////
  // BuildTypeTableParser class
  // it can generate a parser with rules and action to build a typetable
  ///////////////////////////////////////////////////////////////////

  public class BuildTpyeTablePareer
  {
    Repository repo = new Repository();

    public BuildTpyeTablePareer(Lexer.ITokenCollection semi)
    {
      repo.semi = semi;
    }
    public virtual Parser build()
    {
      Parser parser = new Parser();
      // decide what to show
      AAction.displaySemi = false;
      AAction.displayStack = false;  // false is default
      // action used for namespaces, classes, and functions
      PushStack push = new PushStack(repo);
            writeTypeTable writetable = new writeTypeTable(repo);                            //add write TypeTable Action to the rules
      // capture namespace info
      DetectNamespace detectNS = new DetectNamespace();
      detectNS.add(push);
            //detectNS.add(writetable);                                                        //add write TypeTable Action to the rules                                   
            parser.add(detectNS);
      // capture class info
      DetectClass detectCl = new DetectClass();
      detectCl.add(push);
            detectCl.add(writetable);
      parser.add(detectCl);
      // capture function info
      DetectFunction detectFN = new DetectFunction();
      detectFN.add(push);
            detectFN.add(writetable);                                                       //add write TypeTable Action to the rules
            parser.add(detectFN);     
            DetectDelegate detectD = new DetectDelegate();                                  //capture delegate info                                                                
            detectD.add(push);
            detectD.add(writetable);
            parser.add(detectD);
            DetectAlias detectAl = new DetectAlias();                                       //capture Alias info   
            detectAl.add(push);
            detectD.add(writetable);
            parser.add(detectAl);
            DetectEnum detectEn = new DetectEnum();                                          //capture Enum info   
            detectEn.add(push);
            detectEn.add(writetable);
            parser.add(detectEn);          
      // handle entering anonymous scopes, e.g., if, while, etc.
      DetectAnonymousScope anon = new DetectAnonymousScope();
      anon.add(push);
      parser.add(anon);
      // show public declarations
      DetectPublicDeclar pubDec = new DetectPublicDeclar();
      SaveDeclar print = new SaveDeclar(repo);
      pubDec.add(print);
      parser.add(pubDec);
      // handle leaving scopes
      DetectLeavingScope leave = new DetectLeavingScope();
      PopStack pop = new PopStack(repo);
      leave.add(pop);
      parser.add(leave);           // parser configured    
      return parser;
    }
  }

    ///////////////////////////////////////////////////////////////////
    // BuildDependencyParser class
    // it can generate a parser with rules and action to analyze the dependency between packages and build a denpendency table
    // it must be runned after the typetable is already built
    ///////////////////////////////////////////////////////////////////
    public class BuildDependencyParser
    {
        Repository repo = new Repository();
        public BuildDependencyParser()
        {

        }
        public Parser build()
        {
            Parser parser = new Parser();
            DetectUsingNamespace detectUsing = new DetectUsingNamespace();// add detectUsingNamespace Rule to the parser
            addUsingNamespace addUsing = new addUsingNamespace(repo);
            detectUsing.add(addUsing);
            parser.add(detectUsing);

            DetectDependency detectdep = new DetectDependency();         // add detectdependency Rule to the parser
            addDependency addDep = new addDependency(repo);
            detectdep.add(addDep);
            parser.add(detectdep);

            return parser;
         }      
    }
}


