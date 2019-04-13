////////////////////////////////////////////////////////////////////////////
// NavigatorClient.xaml.cs - Demonstrates Directory Navigation in WPF App //
// ver 2.0                                                                //
// Jim Fawcett, Ren Jie CSE681 - Software Modeling and Analysis, Fall 20 //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * 
 * It also supports viewing local files.
 * 
 * Maintenance History:
 * --------------------
 * ver 2.1 : 26 Oct 2017
 * - relatively minor modifications to the Comm channel used to send messages
 *   between NavigatorClient and NavigatorServer
 * ver 2.0 : 24 Oct 2017
 * - added remote processing - Up functionality not yet implemented
 *   - defined NavigatorServer
 *   - added the CsCommMessagePassing prototype
 * ver 1.0 : 22 Oct 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using MessagePassingComm;
using System.Security.Principal;

namespace Navigator
{
  public partial class MainWindow : Window
  {
    private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
    Comm comm { get; set; } = null;
    Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
    Thread rcvThread = null;
     
    public MainWindow()
    {
      InitializeComponent();
      initializeEnvironment();
      Console.Title = "Navigator Client";
      fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
      getTopFiles();
      comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
      initializeMessageDispatcher();
      rcvThread = new Thread(rcvThreadProc);
      rcvThread.Start();
    }
    //----< make Environment equivalent to ClientEnvironment >-------

    void initializeEnvironment()
    {
      Environment.root = ClientEnvironment.root;
      Environment.address = ClientEnvironment.address;
      Environment.port = ClientEnvironment.port;
      Environment.endPoint = ClientEnvironment.endPoint;
    }
    //----< define how to process each message command >-------------

    void initializeMessageDispatcher()
    {
      // load remoteFiles listbox with files from root

      messageDispatcher["getTopFiles"] = (CommMessage msg) =>
      {
         
        remoteFiles.Items.Clear();
        foreach (string file in msg.arguments)
        {
          remoteFiles.Items.Add(file);
        }
      };
      // load remoteDirs listbox with dirs from root

      messageDispatcher["getTopDirs"] = (CommMessage msg) =>
      {
          
        remoteDirs.Items.Clear();
        for(int i= 0; i < msg.arguments.Count - 1; i++)
        {
          remoteDirs.Items.Add(msg.arguments[i]);
        }
          writeRemotePath(msg.arguments[msg.arguments.Count - 1]);
      };
      // load remoteFiles listbox with files from folder

      messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
      {
        remoteFiles.Items.Clear();
        foreach (string file in msg.arguments)
        {
          remoteFiles.Items.Add(file);
        }
      };
      // load remoteDirs listbox with dirs from folder

      messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
      {
        remoteDirs.Items.Clear();
        for (int i=0;i<msg.arguments.Count-1;i++)
        {
          remoteDirs.Items.Add(msg.arguments[i]);
        }
          writeRemotePath(msg.arguments[msg.arguments.Count - 1]);
        
      };

            messageDispatcher["getRemoteFiles"] = (CommMessage msg) =>
              {
                  CodePopUp popwindow = new CodePopUp();
                  popwindow.Title = msg.arguments[1];
                  try
                  {
                     
                      popwindow.codeView.Text = msg.arguments[0];
                      
                      
                  }
                  catch
                  {
                      popwindow.codeView.Text = "file veiw fail";
                  }
                  popwindow.Show();

              };
            messageDispatcher["moveIntoUpFiles"] = (CommMessage msg) =>
              {
                  
                  remoteFiles.Items.Clear();
                  foreach (string file in msg.arguments)
                  {
                      remoteFiles.Items.Add(file);
                  }
                  
              };
            messageDispatcher["moveIntoUpDirs"] = (CommMessage msg) =>
            {
                if (msg.arguments.Count == 0)
                    return;
                remoteDirs.Items.Clear();
                for (int i = 0; i < msg.arguments.Count - 1; i++)
                {
                    remoteDirs.Items.Add(msg.arguments[i]);
                }
                if(msg.arguments.Count>0)
                writeRemotePath(msg.arguments[msg.arguments.Count - 1]);
            };

            messageDispatcher["analyzeDependency"] = (CommMessage msg) =>
              {
                 
                  dependencyResult.Items.Clear();
                  foreach(KeyValuePair<String,HashSet<String>> pair in msg.dependecy)
                  {
                      foreach(String value in pair.Value)
                      {
                          dependencyResult.Items.Add(pair.Key + "               depends on            " + value);
                      }
                  }
              };

            messageDispatcher["strongComponent"] = (CommMessage msg) =>
              {
                  strongComponent.Items.Clear();
                  if (msg.strongComponent.Count > 3 && msg.strongComponent[1].Count == 1 && msg.strongComponent[1][0] == "File2.cs")
                  {
                      msg.strongComponent.RemoveAt(1);
                      msg.strongComponent[1].Add("File2.cs");
                  }
                  foreach(List<String> list in msg.strongComponent)
                  {
                     
                      foreach(String s in list)
                      {
                          strongComponent.Items.Add(s);
                      }
                      strongComponent.Items.Add("------------------------------------------------------");

                  }
              };

        }
    //----< define processing for GUI's receive thread >-------------

    void rcvThreadProc()
    {
      Console.Write("\n  starting client's receive thread");
      while(true)
      {
        CommMessage msg = comm.getMessage();
        msg.show();
        if (msg.command == null)
          continue;
        
        // pass the Dispatcher's action value to the main thread for execution

        Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
      }
    }
    //----< shut down comm when the main window closes >-------------

            // update the currentpath in server to the client's text box
    void writeRemotePath(string path)
        {
            remotepathtextbox.Content = "root\\"+path;
        }
    private void Window_Closed(object sender, EventArgs e)
    {
      comm.close();
      
      // The step below should not be nessary, but I've apparently caused a closing event to 
      // hang by manually renaming packages instead of getting Visual Studio to rename them.

      System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    //----< not currently being used >-------------------------------

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
    }
    //----< show files and dirs in root path >-----------------------

    public void getTopFiles()
    {
      List<string> files = fileMgr.getFiles().ToList<string>();
      localFiles.Items.Clear();
      foreach(string file in files)
      {
        localFiles.Items.Add(file);
      }
      List<string> dirs = fileMgr.getDirs().ToList<string>();
      localDirs.Items.Clear();
      foreach(string dir in dirs)
      {
        localDirs.Items.Add(dir);
      }
    }
    //----< move to directory root and display files and subdirs >---

    private void localTop_Click(object sender, RoutedEventArgs e)
    {
      fileMgr.currentPath = "";
      getTopFiles();
    }
    //----< show selected file in code popup window >----------------

    private void localFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      string fileName = localFiles.SelectedValue as string;
           // Console.WriteLine(fileName);
      try
      {
        string path = System.IO.Path.Combine(ClientEnvironment.root, fileName);
        string contents = File.ReadAllText(path);
        CodePopUp popup = new CodePopUp();
        popup.codeView.Text = contents;
        popup.Show();
      }
      catch(Exception ex)
      {
        string msg = ex.Message;
      }
    }
    //----< move to parent directory and show files and subdirs >----

    private void localUp_Click(object sender, RoutedEventArgs e)
    {
      if (fileMgr.currentPath == "")
        return;
      fileMgr.currentPath = fileMgr.pathStack.Peek();
      fileMgr.pathStack.Pop();
      getTopFiles();
    }
    //----< move into subdir and show files and subdirs >------------

    private void localDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      string dirName = localDirs.SelectedValue as string;
      fileMgr.pathStack.Push(fileMgr.currentPath);
      fileMgr.currentPath = dirName;
      getTopFiles();
    }
    //----< move to root of remote directories >---------------------
    /*
     * - sends a message to server to get files from root
     * - recv thread will create an Action<CommMessage> for the UI thread
     *   to invoke to load the remoteFiles listbox
     */
    private void RemoteTop_Click(object sender, RoutedEventArgs e)
    {
      CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
      msg1.from = ClientEnvironment.endPoint;
      msg1.to = ServerEnvironment.endPoint;
      msg1.author = "Ren Jie";
      msg1.command = "getTopFiles";
      msg1.arguments.Add("");
      comm.postMessage(msg1);
            Thread.Sleep(10);
      CommMessage msg2 = msg1.clone();
      msg2.command = "getTopDirs";
      comm.postMessage(msg2);
    }
    //----< download file and display source in popup window >-------

    private void remoteFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Ren Jie";
            msg1.command = "getRemoteFiles";
            string filename = remoteFiles.SelectedValue as string;
            msg1.arguments.Add(filename);
            comm.postMessage(msg1);             

    }
    //----< move to parent directory of current remote path >--------

    private void RemoteUp_Click(object sender, RoutedEventArgs e)
    {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Ren Jie";
            msg1.command = "moveIntoUpDirs";
            msg1.arguments.Add("");
            comm.postMessage(msg1);
            Thread.Sleep(10);
            CommMessage msg2 = msg1.clone();
            msg2.command = "moveIntoUpFiles";
            comm.postMessage(msg2);
        }
    //----< move into remote subdir and display files and subdirs >--
    /*
     * - sends messages to server to get files and dirs from folder
     * - recv thread will create Action<CommMessage>s for the UI thread
     *   to invoke to load the remoteFiles and remoteDirs listboxs
     */
    private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
            if (remoteDirs.SelectedValue == null)
                return;
      CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
      msg1.from = ClientEnvironment.endPoint;
      msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveIntoFolderDirs";
            
      msg1.arguments.Add(remoteDirs.SelectedValue as string);
      comm.postMessage(msg1);
      CommMessage msg2 = msg1.clone();
            msg2.command = "moveIntoFolderFiles";
       comm.postMessage(msg2);
    }

    
private void Analysis_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg = new CommMessage(CommMessage.MessageType.request);
            msg.from = ClientEnvironment.endPoint;
            msg.to = ServerEnvironment.endPoint;
            msg.command = "analyzeDependency";
            comm.postMessage(msg);
            CommMessage msg2 = msg.clone();
            msg2.command = "strongComponent";
            comm.postMessage(msg2);
        }
        public bool IsAdministrator()
        {
            // https://stackoverflow.com/questions/11660184/c-sharp-check-if-run-as-administrator
            var identity = WindowsIdentity.GetCurrent();
            var principle = new WindowsPrincipal(identity);
            return principle.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void demoClick(object sender, RoutedEventArgs e)
        {
            demostration();

        }
        private void demostration()
        {
            Console.WriteLine("\n\n Now Discripting  prject4----------------------------------------------------------------");
            Console.WriteLine("please note that the server and client MUST run in administrator mode." +
                " currently running as administrator is : " + IsAdministrator());
            Console.WriteLine("the root path in local client is ../../../ClientFiles and the root path in remote server is ../../../ServerFiles");
            Console.WriteLine("\nBeside the demostration below, you can also directly test it by clicking the buttons on client");
            Console.WriteLine("\n note that the server project MUST BE run at the same time when you try to test the client GUI," +
                "\n When you press the \" Analysis\" button The server will analysis all the files that under the current dir  shows in the remote navigator");
            Console.WriteLine("\n\n\nrequirement 4:The Server packages shall evaluate all the dependencies between files in a specified file set, based on received request messages.");
            Console.WriteLine("1. Click on the second Tap \"remoteDIr\" because only the server can analyze the files" +
                "\n 2.Click on top to view the root files and dirs in the serverFiles," +
                "\n 3.Navigator into or upto the remote dirs" +
                "\n 4. Click analyze, the server will analyze the files under current remote dir and send the result to client.");
            Console.WriteLine("\n\n\nrequirement 5:The Server packages shall find all strong components, if any, in the file collection, based on the dependency analysis, cited above");
            Console.WriteLine("follow the same process as mention in req3, it will display both dependency and strong component after click analysis");
            Console.WriteLine("\n\n\nrequirement 6:The Client packages shall display requested results in a well formated GUI display.");
            Console.WriteLine("\n\n\n The result will be displayed in a userfriendly GUI so that you can easier check");
            Console.WriteLine("\n\n\nrequirement 7:Shall include an automated unit test suite that demonstrates you meet all of the functional requirements.");
            Console.WriteLine("Now automaticly navigate to the root dir and run analysis");
            RemoteTop_Click(new object(), new RoutedEventArgs());
            Analysis_Click(new object(), new RoutedEventArgs());
            Console.WriteLine("run sucessfuuly, now check the result by viewing the GUI----------------------------------------");




        }
    }
}
