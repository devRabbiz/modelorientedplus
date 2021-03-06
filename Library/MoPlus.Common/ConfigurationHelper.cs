﻿/*<copyright>
Mo+ Solution Builder is a model oriented programming language and IDE, used for building models and generating code and other documents in a model driven development process.

Copyright (C) 2013 Dave Clemmer, Intelligent Coding Solutions, LLC

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.
</copyright>*/
using System;
using System.Configuration;

namespace MoPlus.Common
{
	///--------------------------------------------------------------------------------
	/// <summary>The methods in this class are to be used to help configuration related operations.</summary>
	///--------------------------------------------------------------------------------
	public static class ConfigurationHelper
	{
		#region "Fields and Properties"
		#endregion "Fields and Properties"

		#region "Methods"
		///--------------------------------------------------------------------------------
		/// <summary>This method gets an application setting as a string.</summary>
		///
		/// <param name="inputAppSettingName">The application setting name.</param>
		/// 
		/// <returns>An application setting as a string.</returns>
		///--------------------------------------------------------------------------------
		public static string GetAppSettingString(string inputAppSettingName)
		{
			// get application setting
			if (inputAppSettingName.Trim() != String.Empty)
			{
				return ConfigurationManager.AppSettings[inputAppSettingName.Trim()];
			}
			return String.Empty;
		}

		///--------------------------------------------------------------------------------
		/// <summary>This method gets an application setting as an int.</summary>
		///
		/// <param name="inputAppSettingName">The application setting name.</param>
		/// 
		/// <returns>An application setting as an int.</returns>
		///--------------------------------------------------------------------------------
		public static int GetAppSettingInt(string inputAppSettingName)
		{
			// get application setting
			int appSettingInt = 0;
			if (inputAppSettingName.Trim() != String.Empty)
			{
				int.TryParse(ConfigurationManager.AppSettings[inputAppSettingName.Trim()], out appSettingInt);
			}
			return appSettingInt;
		}
		#endregion "Methods"
	}
}
