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
            internal static class withTime
            {
                internal static void error(object myMessage)
                {
                    CCM_core.CCM_logSource.Log(LogLevel.Error, System.DateTime.UtcNow + "--: " + myMessage);
                }
                internal static void debug(object myMessage)
                {
                    CCM_core.CCM_logSource.Log(LogLevel.Debug, System.DateTime.UtcNow + "--: " + myMessage);
                }
                internal static void fatal(object myMessage)
                {
                    CCM_core.CCM_logSource.Log(LogLevel.Fatal, System.DateTime.UtcNow + "--: " + myMessage);
                }
                internal static void info(object myMessage)
                {
                    CCM_core.CCM_logSource.Log(LogLevel.Info, System.DateTime.UtcNow + "--: " + myMessage);
                }
                internal static void message(object myMessage)
                {
                    CCM_core.CCM_logSource.Log(LogLevel.Message, System.DateTime.UtcNow + "--: " + myMessage);
                }
                internal static void warning(object myMessage)
                {
                    CCM_core.CCM_logSource.Log(LogLevel.Warning, System.DateTime.UtcNow + "--: " + myMessage);
                }
                internal static void none(object myMessage)
                {
                    CCM_core.CCM_logSource.Log(LogLevel.None, System.DateTime.UtcNow + "--: " + myMessage);
                }
            }



            internal static void error(object myMessage)
            {
                CCM_core.CCM_logSource.Log(LogLevel.Error, myMessage);
            }
            internal static void debug(object myMessage)
            {
                CCM_core.CCM_logSource.Log(LogLevel.Debug, myMessage);
            }
            internal static void fatal(object myMessage)
            {
                CCM_core.CCM_logSource.Log(LogLevel.Fatal, myMessage);
            }
            internal static void info(object myMessage)
            {
                CCM_core.CCM_logSource.Log(LogLevel.Info, myMessage);
            }
            internal static void message(object myMessage)
            {
                CCM_core.CCM_logSource.Log(LogLevel.Message, myMessage);
            }
            internal static void warning(object myMessage)
            {
                CCM_core.CCM_logSource.Log(LogLevel.Warning, myMessage);
            }
            internal static void none(object myMessage)
            {
                CCM_core.CCM_logSource.Log(LogLevel.None, myMessage);
            }
        }


    }
}