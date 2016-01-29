using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkNotes;
using System.Windows.Forms;
using System.IO;
using Microsoft.CSharp.RuntimeBinder;
namespace jni_client_lib
{
    public class JNIClientLib
    {
        dynamic java = null;

/*
private static EngineManager engineManager;
private static ClientInteractor clientInteractor;
*/
        dynamic engine = null;
        dynamic engineManager = null;
        dynamic clientInteractor = null;//= java.si.SOMEInterface.components.ClientInteractor.getClientInteractor();// .EngineManager.@new(); ;

        public void start()
        {
            try
            {





                string pathseperator = ";";



                var jarsdir = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]); // maybe put jars in other dir
                string jarsfileList =
                                     new DirectoryInfo(jarsdir).GetFiles("*.jar", SearchOption.TopDirectoryOnly)
                                        .Select(x => x.FullName) // map  - converts objects to string
                                        .Aggregate((s, sx) => s + pathseperator + sx)  // reduce  - does array.join(":")

                                    + pathseperator +

                                     new DirectoryInfo(jarsdir + "\\Lib").GetFiles("*.jar", SearchOption.AllDirectories)
                                        .Select(x => x.FullName) // map  - converts objects to string
                                        .Aggregate((s, sx) => s + pathseperator + sx); // reduce  - does array.join(":")
    


                if(java==null)
                    java = DarkJava.CreateVm(DarkNotes.JavaFinder.findbestjvm(), true,
                        new[] { JavaOption.DefineProperty("java.class.path", jarsfileList)/*,JavaOption.DefineProperty(""java.library.path", "path to dlls to load from java")*/ }
                        );

                // note: "A first chance exception of type 'DarkNotes.JavaException' occurred in DarkNotes.dll" are OK, it tries various class names until it finds the correct class name

            java.ImportPackage("java.lang");   
            java.ImportPackage("System");
            //MessageBox.Show("pathseperator=" + (string)java.System.getProperty("path.separator") ); // = ";" // could not load jvm then set class path so i need this in advance, so hard codede above
            //MessageBox.Show((string)java.System.getProperty("java.class.path")); // was empty
            
            
            // return;

            /*
import si.SOMEEngine.EngineManager;
import si.SOMEEngine.SOMEEngine;
import si.SOMEEngine.EngineManagerIfc.EngineType;
import si.SOMEInterface.SOMEContext;
import si.SOMEInterface.components.ClientInteractor;
            */
                

            java.ImportPackage("si.SOMEEngine.EngineManager");
            java.ImportPackage("si.SOMEEngine.SOMEEngine");
            java.ImportPackage("si.SOMEEngine.EngineManagerIfc.EngineType");
            java.ImportPackage("si.SOMEInterface.SOMEContext");
            java.ImportPackage("si.SOMEInterface.components.ClientInteractor");
            java.ImportPackage("si.core");
            //java.ImportPackage("si.SOMEEngine.EngineManagerIfc$EngineType");
            
                /*
	private void initEnv(String titleName, String propFileName) {
                 */

            dynamic titleName = java.String.@new("mytitle");
            dynamic propFileName = java.String.@new("some.properties");
/*
		String initMsg = SOMEContext.initServer(false, propFileName); // false ==> no client server setting
 */

            // check if class name exists:
            //Console.Out.WriteLine(" got name " + ((string)java.si.SOMEEngine.EngineManagerIfc.@class.getName())); // classnames need to be full
            //return;

            //see subclassess
            //var classes = java.si.SOMEEngine.EngineManagerIfc.@class.getDeclaredClasses();
            //for (int i = 0; i < classes.Length; i++)
            //    Console.Out.WriteLine("Class = " + classes[i].getName());
            //return;

                
            var EngineType = java.si.SOMEEngine.EngineManagerIfc.@class.getDeclaredClasses()[0];
            //Console.Out.WriteLine(EngineType.getEnumConstants()[0]);
            //return;


            //dynamic sTitlesToLoad = (string)java.SOMEContext.@class.getName(); // si.SOMEInterface.SOMEContext


            string initMsg = (string)java.si.SOMEInterface.SOMEContext.initServer(false, propFileName); // false ==> no client server setting
            //dynamic initMsg = java.si.SOMEInterface.SOMEContext.@class.initServer(false, propFileName); // false ==> no client server setting
           
            //Console.Out.WriteLine(" APPL_IDE = " + ((string)java.Environment.APPL_IDE));
            //Console.Out.WriteLine(" java.cNAMED_IDE_SOME_CONTEXT = " + (java.cNAMED_IDE_SOME_CONTEXT==null?"n":"a" )); 
                
                /*
		if (initMsg == null) {
			initMsg = Initializer.load(Environment.APPL_IDE, titleName);
		}*/
            if (initMsg == null)
            {
                initMsg = (string)java.Initializer.load(java.Environment.APPL_IDE, titleName);
            }
                 
                /*
                
		if (initMsg != null) {
			GUIUtils.showError("Initialization problem: \n" + initMsg);
			System.exit(1);
		}*/
            if (initMsg != null)
            {
                MessageBox.Show("Initialization problem: \n" + initMsg);
                return;
            }

            /*

		// initialize ClientInteractor, based on SOME context
		clientInteractor = ClientInteractor.getClientInteractor();
                 */
            clientInteractor = java.si.SOMEInterface.components.ClientInteractor.getClientInteractor();

           /*
		try {
			clientInteractor.initialize(cNAMED_IDE_SOME_CONTEXT, null, false, true, false, true);
		} catch (Exception e) {
			e.printStackTrace();
		}
           */
            Console.Out.WriteLine("step22");
            try
            {
                clientInteractor.initialize(java.si.ide.IDEApplication.cNAMED_IDE_SOME_CONTEXT, null, false, true, false, true);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("clientInteractor.initialize Exception " + e.Message);
                if (e.InnerException != null)
                    Console.Out.WriteLine(e.InnerException.Message);
            }
            Console.Out.WriteLine("step23");
            /* 
            
    engineManager.getEngine(si.SOMEEngine.EngineManagerIfc.EngineType.ET_SINGLE_TEXT, titleName);
             */

            //var EngineType1 = java.si.SOMEEngine.EngineManagerIfc.@class.getDeclaredClasses()[0];
            engineManager = java.si.SOMEEngine.EngineManager.@new();
            engine = engineManager.getEngine(EngineType.getEnumConstants()[0], titleName); // si.SOMEEngine.SOMEEngine
            Console.Out.WriteLine("step24");
                /*
	}

                 * 
                 */


            }
            catch (JavaException e)
            {

                Console.Out.WriteLine("JavaException1: "+ e.Message);
                Console.Out.WriteLine("JavaException1JavaStackTrace: " + e.JavaStackTrace.Aggregate((s, sx) => s + "\n" + sx));
                Console.Out.WriteLine("Exception1StackTrace: " + e.StackTrace);
                if (e.InnerException != null)
                    Console.Out.WriteLine("JavaException1InnerException: " + e.InnerException.Message);
            }
            catch (RuntimeBinderException e)
            {
                Console.Out.WriteLine("RuntimeBinderException1: " + e.Message);
                Console.Out.WriteLine("RuntimeBinderException1StackTrace: " + e.StackTrace);
                if (e.InnerException != null)
                    Console.Out.WriteLine("RuntimeBinderException1InnerException: " + e.InnerException.Message);
            }
            catch (Exception e)
            {

                Console.Out.WriteLine("Exception1: " + e.Message);
                if (e.InnerException != null)
                    Console.Out.WriteLine("Exception1InnerException: " + e.InnerException.Message);
            }
            //MessageBox.Show((string)s.toLowerCase());
        }

        public void run()
        {
        }

        public void stop()
        {
            
        }
    }
}
