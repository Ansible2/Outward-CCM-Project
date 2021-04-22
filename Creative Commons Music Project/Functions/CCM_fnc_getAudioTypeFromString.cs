/* ----------------------------------------------------------------------------
Function: CCM_fnc_getAudioTypeFromString

Description:
	Uses the extension of a filename as a means of determining its AudioType.

Parameters:
	0: _fileName <STRING> - The name of the file (including extension) for which to get the type

Returns:
	<AudioType> - The UnityEngine enum audio type

Examples:
    (begin example)
		AudioType _type = CCM_fnc_getAudioTypeFromString("mysong.ogg");
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
using UnityEngine;


namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        /// <summary>
        /// Uses the extension of a filename as a means of determining its AudioType.
        /// </summary>
        /// <param name="_fileName"></param>
        /// <returns>AudioType</returns>
        internal static AudioType CCM_fnc_getAudioTypeFromString(string _fileName)
        {
            AudioType _audioType = AudioType.UNKNOWN;

            if (_fileName != null)
            {
                _fileName = _fileName.ToLower();

                if (_fileName.Contains(@".ogg"))
                {
                    _audioType = AudioType.OGGVORBIS;
                } 
                else if (_fileName.Contains(@".mp3"))
                {
                    _audioType = AudioType.MPEG;
                }
                else if (_fileName.Contains(@".wav"))
                {
                    _audioType = AudioType.WAV;
                }
            }

            return _audioType;
        }
    }
}