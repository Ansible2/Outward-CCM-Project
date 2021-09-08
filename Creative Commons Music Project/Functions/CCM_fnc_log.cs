/* ----------------------------------------------------------------------------
Function: CCM_fnc_log

Description:
	

Parameters:
	0:  <> - 

Returns:
	NOTHING

Examples:
    (begin example)
		
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using BepInEx.Logging;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static ManualLogSource CCM_logSource = BepInEx.Logging.Logger.CreateLogSource("CCM_project");

        /// <summary>
        /// Creates a BepinEx log message but with a UTC date/time stamp
        /// </summary>
        /// <param name="myMessage"></param>
        internal static class CCM_fnc_log
        {
            internal static class WithTime
            {
                internal static void fatal(object myMessage)
                {
                    printWithTime(LogLevel.Fatal, myMessage);
                }
                internal static void error(object myMessage)
                {
                    printWithTime(LogLevel.Error, myMessage);
                }
                internal static void warning(object myMessage)
                {
                    printWithTime(LogLevel.Warning, myMessage);
                }
                internal static void debug(object myMessage)
                {
                    printWithTime(LogLevel.Debug, myMessage);
                }
                internal static void info(object myMessage)
                {
                    printWithTime(LogLevel.Info, myMessage);
                }
                internal static void message(object myMessage)
                {
                    printWithTime(LogLevel.Message, myMessage);
                }
                internal static void none(object myMessage)
                {
                    printWithTime(LogLevel.None, myMessage);
                }


                private static void printWithTime(BepInEx.Logging.LogLevel logLevel, object message)
                {
                    printLog(logLevel, message, true);
                }
               
            }



            internal static void error(object myMessage)
            {
                printLog(LogLevel.Error, myMessage);
            }
            internal static void debug(object myMessage)
            {
                printLog(LogLevel.Debug, myMessage);
            }
            internal static void fatal(object myMessage)
            {
                printLog(LogLevel.Fatal, myMessage);
            }
            internal static void info(object myMessage)
            {
                printLog(LogLevel.Info, myMessage);
            }
            internal static void message(object myMessage)
            {
                printLog(LogLevel.Message, myMessage);
            }
            internal static void warning(object myMessage)
            {
                printLog(LogLevel.Warning, myMessage);
            }
            internal static void none(object myMessage)
            {
                printLog(LogLevel.None, myMessage);
            }

            static void printLog(BepInEx.Logging.LogLevel logLevel, object message, bool withTime = false)
            {
                if (CCM_core.CCM_doLog)
                {
                    if (withTime)
                    {
                        message = System.DateTime.UtcNow + "--: " + message;
                    }

                    CCM_core.CCM_logSource.Log(logLevel, message);
                }
                
            }
        }


    }
}