﻿/*<copyright>
Mo+ Solution Builder is a model oriented programming language and IDE, used for building models and generating code and other documents in a model driven development process.

Copyright (C) 2013 Dave Clemmer, Intelligent Coding Solutions, LLC

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.
</copyright>*/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace MoPlus.Data
{
	///--------------------------------------------------------------------------------
	/// <summary>This class is used to get strongly typed data values from various
	/// objects.</summary>
	///--------------------------------------------------------------------------------
	public static class DataHelper
	{
		#region "Fields and Properties"
		// the seed for getting a random integer
		private static Random randomInt = new Random((int)DateTime.Now.Ticks);
		#endregion "Fields and Properties"

		#region "Methods"
		///--------------------------------------------------------------------------------
		/// <summary>Get a random integer value.</summary>
		/// 
		/// <returns>A random int.</returns>
		///--------------------------------------------------------------------------------
		public static int GetRandomInt()
		{
			return randomInt.Next();
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a random integer value.</summary>
		/// 
		/// <returns>A random int.</returns>
		///--------------------------------------------------------------------------------
		public static int GetRandomInt(int min, int max)
		{
			randomInt.Next();
			int rand = randomInt.Next(min, max + 1);
			if (rand > max) rand = max;
			return rand;
		}

		///--------------------------------------------------------------------------------
		/// <summary>This method pads the input string (left or right justified).</summary>
		/// 
		/// <param name="inputString">The input string to pad.</param>
		/// <param name="outputLength">The length the output string needs to b.</param>
		/// <param name="isLeftJustified">If true, left justify, else right justify.</param>
		/// 
		/// <returns>A padded and right or left justified string.</returns>
		///--------------------------------------------------------------------------------
		public static string PadStringToLength(this string inputString, int outputLength, bool isLeftJustified)
		{
			StringBuilder output = new StringBuilder();
			StringBuilder pad = new StringBuilder();

			for (int i = 0; i < outputLength - inputString.Length; i++)
			{
				pad.Append(" ");
			}
			if (isLeftJustified)
			{
				output.Append(inputString).Append(pad);
			}
			else
			{
				output.Append(pad).Append(inputString);
			}

			return output.ToString();
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get an object value from a public property of the input object, else return
		/// null.</summary>
		///
		///
		/// <param name="item">The item to get an object value from.</param>
		/// <param name="propertyName">The name of the property to get an object value from.</param>
		/// 
		/// <returns>A property value, if found.</returns>
		///--------------------------------------------------------------------------------
		public static bool SetPropertyValue(this object item, string propertyName, object propertyValue)
		{
		    try
		    {
		        if (item != null && item.GetPropertyInfo(propertyName) != null)
		        {
		            object value = null;
		            value = GetValueFromSystemType(propertyValue, item.GetPropertySystemType(propertyName).ToString(), false);
		            item.GetPropertyInfo(propertyName).SetValue(item, value, null);
		            return true;
		        }
		    }
		    catch (Exception E)
		    {
                throw new Exception(String.Format("Error setting property '{0}'!", propertyName), E);
		    }
		    return false;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a PropertyInfo from a public property of the input object, else return
		/// null.</summary>
		///
		/// <param name="item">The item to get property info from.</param>
		/// <param name="propertyName">The name of the property to get property info from.</param>
		/// 
		/// <returns>PropertyInfo, if found.</returns>
		///--------------------------------------------------------------------------------
		public static PropertyInfo GetPropertyInfo(this object item, string propertyName)
		{
			if (item != null)
			{
				string typeName = item.GetType().FullName;
				if (item is IDataObject && !(DataTransformHelper.EntityPropertyFieldInfoCache.GenericEntityCache[typeName] is GenericEntity))
				{
					DataTransformHelper.EntityPropertyFieldInfoCache.AddNewEntityWithPropertyFieldInfo(item as IDataObject);
				}

				// utilize data transform cache to get info
				if (DataTransformHelper.EntityPropertyFieldInfoCache.PropertyInfoCache[typeName + "." + propertyName] is PropertyInfo)
				{
					return DataTransformHelper.EntityPropertyFieldInfoCache.PropertyInfoCache[typeName + "." + propertyName] as PropertyInfo;
				}

				// rely on reflection to get info
				Type itemType = item.GetType();
				if (itemType != null)
				{
					PropertyInfo itemInfo = itemType.GetProperty(propertyName);
					if (itemInfo != null)
					{
						// add to raw property info cache (not part of generic entity)
						DataTransformHelper.EntityPropertyFieldInfoCache.PropertyInfoCache[typeName + "." + propertyName] = itemInfo;

						return itemInfo;
					}
				}
			}
			return null;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a FieldInfo from a public property of the input object, else return
		/// null.</summary>
		///
		/// <param name="item">The item to get field info from.</param>
		/// <param name="fieldName">The name of the field to get field info from.</param>
		/// 
		/// <returns>FieldInfo, if found.</returns>
		///--------------------------------------------------------------------------------
		public static FieldInfo GetFieldInfo(this object item, string fieldName)
		{
			if (item != null)
			{
				string typeName = item.GetType().FullName;
				if (item is IDataObject && !(DataTransformHelper.EntityPropertyFieldInfoCache.GenericEntityCache[typeName] is GenericEntity))
				{
					DataTransformHelper.EntityPropertyFieldInfoCache.AddNewEntityWithPropertyFieldInfo(item as IDataObject);
				}

				// utilize data transform cache to get info
				if (DataTransformHelper.EntityPropertyFieldInfoCache.FieldInfoCache[typeName + "." + fieldName] is FieldInfo)
				{
					return DataTransformHelper.EntityPropertyFieldInfoCache.FieldInfoCache[typeName + "." + fieldName] as FieldInfo;
				}

				// rely on reflection to get info
				if (item != null && item.GetType() != null && item.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance) != null)
				{
					FieldInfo info = item.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

					// add to raw field info cache (not part of generic entity)
					DataTransformHelper.EntityPropertyFieldInfoCache.FieldInfoCache[typeName + "." + fieldName] = info;

					return info;
				}
			}
			return null;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a system type of a public property of the input object, else return
		/// null.</summary>
		///
		/// <param name="item">The item to get system type from.</param>
		/// <param name="propertyName">The name of the property to get system type from.</param>
		/// 
		/// <returns>A property value, if found.</returns>
		///--------------------------------------------------------------------------------
		public static System.Type GetPropertySystemType(this object item, string propertyName)
		{
			if (item != null)
			{
				PropertyInfo itemInfo = item.GetPropertyInfo(propertyName);
				if (itemInfo != null)
				{
					return itemInfo.PropertyType;
				}
			}
			return null;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a string value from a public property of the input object, else return
		/// null.</summary>
		///
		/// <param name="item">The item to get a string value from.</param>
		/// <param name="propertyName">The name of the property to get a string value from.</param>
		/// 
		/// <returns>A property value, if found.</returns>
		///--------------------------------------------------------------------------------
		public static string GetPropertyValueString(this object item, string propertyName)
		{
			if (item != null)
			{
				PropertyInfo itemInfo = item.GetPropertyInfo(propertyName);
				if (itemInfo != null)
				{
					object value = itemInfo.GetValue(item, new object[] { });
					if (value == null)
					{
						return null;
					}
					return GetString(value);
				}
			}
			return null;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get an object value from a public property of the input object, else return
		/// null.</summary>
		///
		///
		/// <param name="item">The item to get an object value from.</param>
		/// <param name="propertyName">The name of the property to get an object value from.</param>
		/// 
		/// <returns>A property value, if found.</returns>
		///--------------------------------------------------------------------------------
		public static object GetPropertyValue(this object item, string propertyName)
		{
			if (item != null)
			{
				PropertyInfo itemInfo = item.GetPropertyInfo(propertyName);
				if (itemInfo != null)
				{
					object value = itemInfo.GetValue(item, new object[] { });
					if (value == null)
					{
						return null;
					}
					return value;
				}
			}
			return null;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a string value from a field of the input object, else return
		/// null.</summary>
		///
		/// <param name="item">The item to get a string value from.</param>
		/// <param name="propertyName">The name of the property to get a string value from.</param>
		/// 
		/// <returns>A field value, if found.</returns>
		///--------------------------------------------------------------------------------
		public static string GetFieldValueString(this object item, string fieldName)
		{
			if (item != null && item.GetFieldInfo(fieldName) != null)
			{
				object value = item.GetFieldInfo(fieldName).GetValue(item);
				if (value == null)
				{
					return null;
				}
				return GetString(value);
			}
			return null;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Determine whether the input value matches the value of the specified
		/// property's value.</summary>
		///
		/// <param name="item">The item to get a matchint value from.</param>
		/// <param name="propertyName">The name of the property to get a matching value from.</param>
		/// <param name="value">The value to compare for a match.</param>
		/// 
		/// <returns>True if a value match was found, false otherwise.</returns>
		///--------------------------------------------------------------------------------
		public static bool IsPropertyValueMatch(this object item, string propertyName, object value)
		{
			string itemValue = GetPropertyValueString(item, propertyName);
			if (itemValue == null && value == null)
			{
				return true;
			}
			if (itemValue == null || value == null)
			{
				return false;
			}
			return GetString(itemValue) == GetString(value);
		}

		///--------------------------------------------------------------------------------
		/// <summary>Populate the input collection with values from public properties from
		/// the input object.</summary>
		///
		/// <param name="collectionToPopulate">The collection to populate with public properties.</param>
		/// <param name="item">The object to get public properties from.</param>
		///--------------------------------------------------------------------------------
		public static void PopulateCollectionWithPublicPropertyValues(this NameObjectCollection collectionToPopulate, object item)
		{
			foreach (PropertyInfo loopInfo in item.GetType().GetProperties())
			{
				collectionToPopulate[loopInfo.Name] = loopInfo.GetValue(item, null);
			}
		}

		///--------------------------------------------------------------------------------
		/// <summary>Populate the input collection with values from public properties with
		/// the DataElement attribute from the input object.</summary>
		///
		/// <param name="collectionToPopulate">The collection to populate with public properties.</param>
		/// <param name="item">The object to get public properties from.</param>
		///--------------------------------------------------------------------------------
		public static void PopulateCollectionWithDataElementPropertyValues(this NameObjectCollection collectionToPopulate, object item)
		{
			foreach (PropertyInfo loopInfo in item.GetType().GetProperties())
			{
				foreach (object loopAttribute in loopInfo.GetCustomAttributes(true))
				{
					if (loopAttribute is DataElementAttribute)
					{
						// get name of field to transform
						string transformName = ((DataElementAttribute)loopAttribute).ElementName.GetString().Trim().ToLower();
						if (transformName == String.Empty)
						{
							transformName = loopInfo.Name;
						}
						collectionToPopulate[transformName] = loopInfo.GetValue(item, null);
					}
				}
			}
		}

		///--------------------------------------------------------------------------------
		/// <summary>Populate the input collection with string values from public properties from
		/// the input object.</summary>
		///
		/// <param name="collectionToPopulate">The collection to populate with public properties.</param>
		/// <param name="item">The object to get public properties from.</param>
		///--------------------------------------------------------------------------------
		public static void PopulateCollectionWithPublicPropertyStringValues(this NameObjectCollection collectionToPopulate, object item)
		{
			foreach (PropertyInfo loopInfo in item.GetType().GetProperties())
			{
				collectionToPopulate[loopInfo.Name] = GetString(loopInfo.GetValue(item, null));
			}
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a value from the input object, converted to the specified system
		/// type.</summary>
		///
		/// <param name="value">The input value.</param>
		/// <param name="systemType">The underlying system type (CTS type).</param>
		/// <param name="getNonDefaultValuesOnly">If true, only return a value if not null and not a default value.</param>
		/// 
		/// <returns>A value converted to the system type, if allowed.</returns>
		///--------------------------------------------------------------------------------
		public static object GetValueFromSystemType(this object value, string systemType, bool getNonDefaultValuesOnly)
		{
			return GetValueFromSystemType(value, systemType, systemType.ToLower().Contains("nullable") == true, getNonDefaultValuesOnly);
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a value from the input object, converted to the specified system
		/// type.</summary>
		///
		/// <param name="value">The input value.</param>
		/// <param name="systemType">The underlying system type (CTS type).</param>
		/// <param name="isNullable">Flag indicating whether value returned can be null.</param>
		/// <param name="getNonDefaultValuesOnly">If true, only return a value if not null and not a default value.</param>
		/// 
		/// <returns>A value converted to the system type, if allowed.</returns>
		///--------------------------------------------------------------------------------
		public static object GetValueFromSystemType(this object value, string systemType, bool isNullable, bool getNonDefaultValuesOnly)
		{
			// handle array types
			// TODO: complete
			if (systemType.ToLower().Contains("byte[]"))
			{
				return value.GetByteArray();
			}
			if (isNullable == true)
			{
				// get values for nullable types
				if (systemType.Contains("SByte"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableSByte() == DefaultValue.SByte)
					{
						return null;
					}
					return value.GetNullableSByte();
				}
				if (systemType.Contains("Int16"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableShort() == DefaultValue.Short)
					{
						return null;
					}
					return value.GetNullableShort();
				}
				if (systemType.Contains("Int32"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableInt() == DefaultValue.Int)
					{
						return null;
					}
					return value.GetNullableInt();
				}
				if (systemType.Contains("Int64"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableLong() == DefaultValue.Long)
					{
						return null;
					}
					return value.GetNullableLong();
				}
				if (systemType.Contains("Byte"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableByte() == DefaultValue.Byte)
					{
						return null;
					}
					return value.GetNullableByte();
				}
				if (systemType.Contains("UInt16"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableUShort() == DefaultValue.UShort)
					{
						return null;
					}
					return value.GetNullableUShort();
				}
				if (systemType.Contains("UInt32"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableUInt() == DefaultValue.UInt)
					{
						return null;
					}
					return value.GetNullableUInt();
				}
				if (systemType.Contains("UInt64"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableULong() == DefaultValue.ULong)
					{
						return null;
					}
					return value.GetNullableULong();
				}
				if (systemType.Contains("Single"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableFloat() == DefaultValue.Float)
					{
						return null;
					}
					return value.GetNullableFloat();
				}
				if (systemType.Contains("Double"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableDouble() == DefaultValue.Double)
					{
						return null;
					}
					return value.GetNullableDouble();
				}
				if (systemType.Contains("Decimal"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableDecimal() == DefaultValue.Decimal)
					{
						return null;
					}
					return value.GetNullableDecimal();
				}
				if (systemType.Contains("Boolean"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableBool() == DefaultValue.Bool)
					{
						return null;
					}
					return value.GetNullableBool();
				}
				if (systemType.Contains("Char"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableChar() == DefaultValue.Char)
					{
						return null;
					}
					return value.GetNullableChar();
				}
				if (systemType.Contains("Guid"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableGuid() == DefaultValue.Guid)
					{
						return null;
					}
					return value.GetNullableGuid();
				}
				if (systemType.Contains("DateTime"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableDateTime() == DefaultValue.DateTime)
					{
						return null;
					}
					return value.GetNullableDateTime();
				}
				if (systemType.Contains("String"))
				{
					if (getNonDefaultValuesOnly == true && value.GetNullableString() == DefaultValue.String)
					{
						return null;
					}
					return value.GetNullableString();
				}
			}
			else
			{
				// get values for non nullable types
				if (systemType.Contains("SByte"))
				{
					if (getNonDefaultValuesOnly == true && value.GetSByte() == DefaultValue.SByte)
					{
						return null;
					}
					return value.GetSByte();
				}
				if (systemType.Contains("Int16"))
				{
					if (getNonDefaultValuesOnly == true && value.GetShort() == DefaultValue.Short)
					{
						return null;
					}
					return value.GetShort();
				}
				if (systemType.Contains("Int32"))
				{
					if (getNonDefaultValuesOnly == true && value.GetInt() == DefaultValue.Int)
					{
						return null;
					}
					return value.GetInt();
				}
				if (systemType.Contains("Int64"))
				{
					if (getNonDefaultValuesOnly == true && value.GetLong() == DefaultValue.Long)
					{
						return null;
					}
					return value.GetLong();
				}
				if (systemType.Contains("Byte"))
				{
					if (getNonDefaultValuesOnly == true && value.GetByte() == DefaultValue.Byte)
					{
						return null;
					}
					return value.GetByte();
				}
				if (systemType.Contains("UInt16"))
				{
					if (getNonDefaultValuesOnly == true && value.GetUShort() == DefaultValue.UShort)
					{
						return null;
					}
					return value.GetUShort();
				}
				if (systemType.Contains("UInt32"))
				{
					if (getNonDefaultValuesOnly == true && value.GetUInt() == DefaultValue.UInt)
					{
						return null;
					}
					return value.GetUInt();
				}
				if (systemType.Contains("UInt64"))
				{
					if (getNonDefaultValuesOnly == true && value.GetULong() == DefaultValue.ULong)
					{
						return null;
					}
					return value.GetULong();
				}
				if (systemType.Contains("Single"))
				{
					if (getNonDefaultValuesOnly == true && value.GetFloat() == DefaultValue.Float)
					{
						return null;
					}
					return value.GetFloat();
				}
				if (systemType.Contains("Double"))
				{
					if (getNonDefaultValuesOnly == true && value.GetDouble() == DefaultValue.Double)
					{
						return null;
					}
					return value.GetDouble();
				}
				if (systemType.Contains("Decimal"))
				{
					if (getNonDefaultValuesOnly == true && value.GetDecimal() == DefaultValue.Decimal)
					{
						return null;
					}
					return value.GetDecimal();
				}
				if (systemType.Contains("Boolean"))
				{
					if (getNonDefaultValuesOnly == true && value.GetBool() == DefaultValue.Bool)
					{
						return null;
					}
					return value.GetBool();
				}
				if (systemType.Contains("Char"))
				{
					if (getNonDefaultValuesOnly == true && value.GetChar() == DefaultValue.Char)
					{
						return null;
					}
					return value.GetChar();
				}
				if (systemType.Contains("Guid"))
				{
					if (getNonDefaultValuesOnly == true && value.GetGuid() == DefaultValue.Guid)
					{
						return null;
					}
					return value.GetGuid();
				}
				if (systemType.Contains("DateTime"))
				{
					if (getNonDefaultValuesOnly == true && value.GetDateTime() == DefaultValue.DateTime)
					{
						return null;
					}
					return value.GetDateTime();
				}
				if (systemType.Contains("String"))
				{
					if (getNonDefaultValuesOnly == true && value.GetString() == DefaultValue.String)
					{
						return null;
					}
					return value.GetString();
				}
			}
			return value;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Determine if a system type represents data that should be left
		/// justified.</summary>
		///
		/// <param name="systemType">The underlying system type (CTS type).</param>
		/// 
		/// <returns>True if data should be left justified, false otherwise.</returns>
		///--------------------------------------------------------------------------------
		public static bool IsLeftJustifiedSystemType(string systemType)
		{
			// return false (right justified) for numeric types
			switch (systemType)
			{
				case "SByte":
				case "Int16":
				case "Int32":
				case "Int64":
				case "Byte":
				case "UInt16":
				case "UInt32":
				case "UInt64":
				case "Single":
				case "Double":
				case "Decimal":
					return false;
				default:
					break;
			}
			return true;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Return whether or not a Guid is null or empty.</summary>
		///--------------------------------------------------------------------------------
		public static bool IsNullOrEmpty(this Guid? item)
		{
			if (item == null || item == Guid.Empty)
			{
				return true;
			}
			return false;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Return whether or not a Guid is empty.</summary>
		///--------------------------------------------------------------------------------
		public static bool IsNullOrEmpty(this Guid item)
		{
			if (item == Guid.Empty)
			{
				return true;
			}
			return false;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a string from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static string GetStringNoTrim(this object item)
		{
			if (item == null || item.ToString() == String.Empty)
			{
				return DefaultValue.String;
			}
			return item.ToString();
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a string from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static string GetString(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.String;
			}
			if (item is bool)
			{
				// return lowercase true and false
				return item.ToString().ToLower();
			}
			return item.ToString().Trim();
		}

		///--------------------------------------------------------------------------------
		/// <summary>This method returns underscore case for the input name.</summary>
		///
		///	<param name="inputName">The input name.</param>
		///--------------------------------------------------------------------------------
		public static string UnderscoreCase(this string inputName)
		{
			// return modified name
			return inputName.Length > 1 ? "_" + inputName.Substring(0, 1).ToLower() + inputName.Substring(1) : inputName.Length > 0 ? "_" + inputName.ToLower() : "_" + inputName;
		}

		///--------------------------------------------------------------------------------
		/// <summary>This method returns camel case for the input name.</summary>
		///
		///	<param name="inputName">The input name.</param>
		///--------------------------------------------------------------------------------
		public static string CamelCase(this string inputName)
		{
			// return modified name
			return inputName.Length > 1 ? inputName.Substring(0, 1).ToLower() + inputName.Substring(1) : inputName.Length > 0 ? inputName.ToLower() : inputName;
		}

		///--------------------------------------------------------------------------------
		/// <summary>This method returns capital case for the input name.</summary>
		///
		///	<param name="inputName">The input name.</param>
		///--------------------------------------------------------------------------------
		public static string CapitalCase(this string inputName)
		{
			// return modified name
			StringBuilder capitalName = new StringBuilder();
			bool changeToCap = true;
			for (int i = 0; i < inputName.Length; i++)
			{
				if (changeToCap == true)
				{
					capitalName.Append(inputName[i].ToString().ToUpper());
					changeToCap = false;
				}
				else
				{
					capitalName.Append(inputName[i]);
				}
				if (inputName[i] == '_' || inputName[i] == ' ')
				{
					changeToCap = true;
				}
			}
			return capitalName.ToString();
		}

		///--------------------------------------------------------------------------------
		/// <summary>This method returns capital word case for the input name.</summary>
		///
		///	<param name="inputName">The input name.</param>
		///--------------------------------------------------------------------------------
		public static string CapitalWordCase(this string inputName)
		{
			// return modified name
			StringBuilder capitalName = new StringBuilder();
			bool changeToCap = true;
			bool isCap = false;
			bool isSpace = false;
			for (int i = 0; i < inputName.Length; i++)
			{
				if (!((inputName[i] == '_' || Char.IsWhiteSpace(inputName[i]) == true) && isSpace == true))
				{
					if (inputName[i] == '_' || Char.IsWhiteSpace(inputName[i]) == true)
					{
						isSpace = true;
					}
					else
					{
						isSpace = false;
					}
					if (i > 0 && isCap == false && Char.IsUpper(inputName[i]))
					{
						capitalName.Append(" ");
						isCap = true;
					}
					if (Char.IsUpper(inputName[i]) == false)
					{
						isCap = false;
					}
					if (changeToCap == true && isSpace == false)
					{
						capitalName.Append(inputName[i].ToString().ToUpper());
						changeToCap = false;
					}
					else
					{
						if (isSpace == true)
						{
							capitalName.Append(" ");
							changeToCap = true;
						}
						else
						{
							capitalName.Append(inputName[i]);
						}
					}
				}
			}
			return capitalName.ToString().Trim().Replace("  ", " ");
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a DateTime from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static DateTime GetDateTime(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.DateTime;
			}
			return DateTime.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a DateTime from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static Guid GetGuid(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return Guid.Empty;
			}
			return new Guid(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get an sbyte from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static sbyte GetSByte(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.SByte;
			}
			return sbyte.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a short from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static short GetShort(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Short;
			}
			return short.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get an int from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static int GetInt(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Int;
			}
			return int.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a long from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static long GetLong(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Long;
			}
			return long.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a byte from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static byte GetByte(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Byte;
			}
			return byte.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a byte array from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static byte[] GetByteArray(this object item)
		{
			// TODO: handle other conversions
			if (item is byte[])
			{
				return item as byte[];
			}
			return null;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a ushort from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static ushort GetUShort(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.UShort;
			}
			return ushort.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a uint from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static uint GetUInt(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.UInt;
			}
			return uint.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a ulong from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static ulong GetULong(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.ULong;
			}
			return ulong.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a float from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static float GetFloat(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Float;
			}
			return float.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a double from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static double GetDouble(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Double;
			}
			return double.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a decimal from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static decimal GetDecimal(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Decimal;
			}
			return decimal.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a bool from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static bool GetBool(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Bool;
			}
			switch (item.ToString().Trim().ToLower())
			{
				case "0":
				case "false":
				case "no":
					return false;
				case "1":
				case "true":
				case "yes":
					return true;
				default:
					break;
			}
			return bool.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a char from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static char GetChar(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return DefaultValue.Char;
			}
			return char.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a string from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static string GetNullableString(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return item.ToString().Trim();
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable DateTime from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static DateTime? GetNullableDateTime(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return DateTime.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a DateTime from the input object, else return the default value.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static Guid? GetNullableGuid(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return new Guid(item.ToString().Trim()); ;
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable sbyte from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static sbyte? GetNullableSByte(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return sbyte.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable short from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static short? GetNullableShort(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return short.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable int from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static int? GetNullableInt(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return int.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable long from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static long? GetNullableLong(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return long.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable byte from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static byte? GetNullableByte(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return byte.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable ushort from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static ushort? GetNullableUShort(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return ushort.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable uint from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static uint? GetNullableUInt(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return uint.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable ulong from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static ulong? GetNullableULong(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return ulong.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable float from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static float? GetNullableFloat(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return float.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable double from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static double? GetNullableDouble(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return double.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable decimal from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static decimal? GetNullableDecimal(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return decimal.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable bool from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static bool? GetNullableBool(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return bool.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a nullable char from the input object.</summary>
		///
		/// <param name="item">The item to get the value from.</param>
		///--------------------------------------------------------------------------------
		public static char? GetNullableChar(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			return char.Parse(item.ToString().Trim());
		}

		///--------------------------------------------------------------------------------
		/// <summary>Get a list of words from the input object.</summary>
		/// 
		/// <remarks>Alphanumeric and underscores are allowed as part of each word.</remarks>
		///
		/// <param name="item">The item to get the strings from from.</param>
		///--------------------------------------------------------------------------------
		public static List<string> GetWords(this object item)
		{
			if (item == null || item.ToString().Trim() == String.Empty)
			{
				return null;
			}
			string text = item.ToString();
			List<string> strings = new List<string>();
			StringBuilder word = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetterOrDigit(text[i]) || text[i] == '_')
				{
					word.Append(text[i]);
				}
				else
				{
					if (word.Length > 0)
					{
						strings.Add(word.ToString());
						word.Clear();
					}
				}
			}
			return strings;
		}

		#endregion "Methods"
	}
}
