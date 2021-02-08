/* ----------------------------------------------------------------------------
Function: CCM_fnc_buildFilePath

Description:
	Combines a folder path and filename string.
    Can also add "file://" to the front of the file.

Parameters:
	0: _folderPath <STRING> - The absolute path to the folder the file is in
    1: _fileName <STRING> - The filename with extension
    2: _addFileExtension <BOOL> - Add the "file://" to the back of the string

Returns:
	<STRING> - The combined file path

Examples:
    (begin example)
		var _filePath = CCM_fnc_buildFilePath(_folder,_file);
    (end)

Author(s):
	Ansible2
---------------------------------------------------------------------------- */
namespace creativeCommonsMusicProject
{
    partial class CCM_core
    {
        internal static string CCM_fnc_buildFilePath(string _folderPath, string _fileName, bool _addFileExtension = false)
        {
            string _filePath = _folderPath + @"\" + _fileName;

            if (_addFileExtension)
            {
                _filePath = CCM_filePathStart + _filePath;
            }

            return _filePath;
        }
    }
}