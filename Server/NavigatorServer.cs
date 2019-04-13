////////////////////////////////////////////////////////////////////////////
// NavigatorServer.cs - File Server for WPF NavigatorClient Application   //
// ver 2.0                                                                //
// Jim Fawcett, Ren Jie CSE681 - Software Modeling and Analysis, Fall 2018 //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines a single NavigatorServer class that returns file
 * and directory information about its rootDirectory subtree.  It uses
 * a message dispatcher that handles processing of all incoming and outgoing
 * messages.
 * 
 * Maintanence History:
 * --------------------
 * ver 2.0 - 24 Oct 2017
 * - added message dispatcher which works very well - see below
 * - added these comments
 * ver 1.0 - 22 Oct 2017
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using depAnalysis;
using strongComponent;

namespace Navigator
{
  public class NavigatorServer
  {
    IFileMgr localFileMgr { get; set; } = null;
    Comm comm { get; set; } = null;
        DepAnalysis analyzer = new DepAnalysis();
  

    Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher = 
      new Dictionary<string, Func<CommMessage, CommMessage>>();

    /*----< initialize server processing >-------------------------*/

    public NavigatorServer()
    {
      initializeEnvironment();
      Console.Title = "Navigator Server";
      localFileMgr = FileMgrFactory.create(FileMgrType.Local);
    }
    /*----< set Environment properties needed by server >----------*/

    void initializeEnvironment()
    {
      Environment.root = ServerEnvironment.root;
      Environment.address = ServerEnvironment.address;
      Environment.port = ServerEnvironment.port;
      Environment.endPoint = ServerEnvironment.endPoint;
    }
    /*----< define how each message will be processed >------------*/
    

    void initializeDispatcher()
    {
            //get top files
      Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
      {
        localFileMgr.currentPath = "";
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "getTopFiles";
        reply.arguments = localFileMgr.getFiles().ToList<string>();
          localFileMgr.pathStack.Clear();
        return reply;
      };
      messageDispatcher["getTopFiles"] = getTopFiles;

        //get top dirs
      Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
      {
        localFileMgr.currentPath = "";
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "getTopDirs";
        reply.arguments = localFileMgr.getDirs().ToList<string>();
          localFileMgr.pathStack.Clear();
          reply.arguments.Add(localFileMgr.currentPath);
          return reply;
      };
      messageDispatcher["getTopDirs"] = getTopDirs;

            //move into folder and get files

      Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
      {
          if (msg.arguments.Count() == 1)
          {
         
              localFileMgr.currentPath = msg.arguments[0];
          }
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "moveIntoFolderFiles";
        reply.arguments = localFileMgr.getFiles().ToList<string>();
        return reply;
      };
      messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;

       //move into dir and get dirs

      Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
      {
          if (msg.arguments.Count() == 1)
          {
              
              localFileMgr.pathStack.Push(localFileMgr.currentPath);
              localFileMgr.currentPath = msg.arguments[0];
              foreach(string s in localFileMgr.pathStack)
              Console.WriteLine("the stack values are " + s);
          }
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "moveIntoFolderDirs";
        reply.arguments = localFileMgr.getDirs().ToList<string>();
          reply.arguments.Add(localFileMgr.currentPath);
          return reply;
      };
      messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;

            //move into upper dir and get files and dirs

            Func<CommMessage, CommMessage> moveIntoUpFiles = (CommMessage msg) =>
               {
                   CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                   reply.to = msg.from;
                   reply.from = msg.to;
                   reply.command = "moveIntoUpFiles";
                   

                   //localFileMgr.currentPath = localFileMgr.pathStack.Peek();
                 
                   reply.arguments = localFileMgr.getFiles().ToList<string>();
                   
                   return reply;
               };
            messageDispatcher["moveIntoUpFiles"] = moveIntoUpFiles;

            Func<CommMessage, CommMessage> moveIntoUpDirs = (CommMessage msg) =>
               {
                   CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                   reply.to = msg.from;
                   reply.from = msg.to;
                   reply.command = "moveIntoUpDirs";
                   foreach(string s in localFileMgr.pathStack)
                   Console.WriteLine("the stack values are " + s);
                   if (localFileMgr.pathStack.Count == 0)
                       localFileMgr.currentPath = "";
                   else
                   {
                       localFileMgr.currentPath = localFileMgr.pathStack.Peek();
                       localFileMgr.pathStack.Pop();
                   }
                   reply.arguments = localFileMgr.getDirs().ToList<string>();
                   reply.arguments.Add(localFileMgr.currentPath);
                   return reply;

               };
            messageDispatcher["moveIntoUpDirs"] = moveIntoUpDirs;

            Func<CommMessage, CommMessage> getRemoteFiles = delegate( CommMessage msg){
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getRemoteFiles";
               string path=ServerEnvironment.root+ msg.arguments[0];
                try
                {

                    string content = System.IO.File.ReadAllText(path);
                    reply.arguments.Add(content);
                    reply.arguments.Add(System.IO.Path.GetFileName(path));
                }
                catch
                {
                    Console.WriteLine(msg.arguments[0]);
                }
   
                return reply;

            };
            messageDispatcher["getRemoteFiles"] = getRemoteFiles;

            messageDispatcher["analyzeDependency"] = (CommMessage msg) =>
              {
                  CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                  reply.from = msg.to;
                  reply.to = msg.from;
                  reply.command = "analyzeDependency";
                  analyzer.setpath ( ServerEnvironment.root+ localFileMgr.currentPath);
                  analyzer.analyze();
                  reply.dependecy = analyzer.DependencyTable;
                  return reply;

              };

            messageDispatcher["strongComponent"] = (CommMessage msg) =>
              {
                  CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                  reply.from = msg.to;
                  reply.to = msg.from;
                  reply.command = "strongComponent";
                  StrongComponent strong = new StrongComponent();
                  strong.buildgraph(analyzer.DependencyTable);
                  reply.strongComponent = strong.tarjan();
                  return reply;
              };


     // define delegate and put them into the dispatcher dictionary finishe


    }
    /*----< Server processing >------------------------------------*/
    /*
     * - all server processing is implemented with the simple loop, below,
     *   and the message dispatcher lambdas defined above.
     */
    static void Main(string[] args)
    {
      TestUtilities.title("Starting Navigation Server", '=');
      try
      {
        NavigatorServer server = new NavigatorServer();
        server.initializeDispatcher();
        server.comm = new MessagePassingComm.Comm(ServerEnvironment.address, ServerEnvironment.port);
        
        while (true)
        {
          CommMessage msg = server.comm.getMessage();
          if (msg.type == CommMessage.MessageType.closeReceiver)
            break;
          msg.show();
          if (msg.command == null)
            continue;
          CommMessage reply = server.messageDispatcher[msg.command](msg);
          reply.show();
          server.comm.postMessage(reply);
        }
      }
      catch(Exception ex)
      {
        Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
      }
    }
  }
}
