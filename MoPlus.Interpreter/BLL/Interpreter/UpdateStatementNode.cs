﻿/*<copyright>
Mo+ Solution Builder is a model oriented programming language and IDE, used for building models and generating code and other documents in a model driven development process.

Copyright (C) 2013 Dave Clemmer, Intelligent Coding Solutions, LLC

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.
</copyright>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter.Ast;
using MoPlus.Interpreter.Resources;
using MoPlus.Interpreter.BLL.Solutions;
using System.IO;
using MoPlus.IO;

namespace MoPlus.Interpreter.BLL.Interpreter
{
	///--------------------------------------------------------------------------------
	/// <summary>This class implements necessary members for interpretation of update statements.</summary>
	///
	/// <CreatedByUserName>DAVE\JavaDave</CreatedByUserName>
	/// <CreatedDate>9/29/2011</CreatedDate>
	/// <Status>Customized</Status>
	///--------------------------------------------------------------------------------
	public class UpdateStatementNode : NonLeafGrammarNode, IStatementNode
	{
		///--------------------------------------------------------------------------------
		/// <summary>This property gets the DisplayName, for display purposes, etc.</summary>
		///--------------------------------------------------------------------------------
		public override string DisplayName
		{
			get
			{
				return LineNumber.ToString() + ": " + DisplayValues.TemplateNode_UpdateStatement;
			}
		}

		public ParameterNode FilePath { get; set; }

		public ParameterNode CustomText { get; set; }

		///--------------------------------------------------------------------------------
		/// <summary>This method sets up the AST node and its children in the AST.</summary>
		/// 
		/// <param name="context">The parsing context.</param>
		/// <param name="treeNode">The corresponding node in the parse tree.</param>
		///--------------------------------------------------------------------------------
		public override void Init(ParsingContext context, ParseTreeNode treeNode)
		{
			try
			{
				base.Init(context, treeNode);
				foreach (ParseTreeNode node in treeNode.ChildNodes)
				{
					if (node.AstNode is ParameterNode)
					{
						if (FilePath == null)
						{
							FilePath = node.AstNode as ParameterNode;
						}
						else
						{
							CustomText = node.AstNode as ParameterNode;
						}
						ChildNodes.Add(node.AstNode as ParameterNode);
					}
				}
			}
			catch (ApplicationAbortException)
			{
				throw;
			}
			catch (System.Exception ex)
			{
				ThrowASTException(ex, true);
			}
		}

		///--------------------------------------------------------------------------------
		/// <summary>Interpret this node to produce code, output, or model data..</summary>
		/// 
		/// <param name="interpreterType">The type of interpretation to perform.</param>
		/// <param name="solutionContext">The associated solution.</param>
		/// <param name="templateContext">The associated template.</param>
		/// <param name="modelContext">The associated model context.</param>
		///--------------------------------------------------------------------------------
		public void InterpretNode(InterpreterTypeCode interpreterType, Solution solutionContext, ITemplate templateContext, IDomainEnterpriseObject modelContext)
		{
			try
			{
				string text;
				if (CustomText != null)
				{
					text = CustomText.GetStringValue(solutionContext, templateContext, modelContext, interpreterType);
				}
				else
				{
					text = templateContext.ContentCodeBuilder.ToString();
				}
				string path = FilePath.GetStringValue(solutionContext, templateContext, modelContext, interpreterType);
				if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(path))
				{
					if (solutionContext.IsSampleMode == true)
					{
						// don't perform replacement of output files, just indicate it would be output
						templateContext.MessageBuilder.Append("\r\n> ");
						templateContext.MessageBuilder.Append(path);
					}
					else if (solutionContext.LoggedErrors.Count == 0)
					{
						if (File.Exists(path) == true)
						{
							if (solutionContext.UseProtectedAreas == true && String.IsNullOrEmpty(solutionContext.ProtectedAreaStart) == false && String.IsNullOrEmpty(solutionContext.ProtectedAreaEnd) == false)
							{
								// preserve protected areas in output file
								string originalText = FileHelper.GetText(path);
								if (originalText.Contains(solutionContext.ProtectedAreaStart) && originalText.Contains(solutionContext.ProtectedAreaEnd))
								{
									int currentTextIndex = 0;
									int currentOriginalTextIndex = 0;
									bool hasProtectedText = false;
									StringBuilder updatedText = new StringBuilder();
									while (currentTextIndex < (text.Length - 1))
									{
										if (text.IndexOf(solutionContext.ProtectedAreaStart, currentTextIndex) >= 0 && text.IndexOf(solutionContext.ProtectedAreaEnd, text.IndexOf(solutionContext.ProtectedAreaStart, currentTextIndex)) >= 0)
										{
											// append text to next protected block
											updatedText.Append(text.Substring(currentTextIndex, text.IndexOf(solutionContext.ProtectedAreaStart, currentTextIndex) - currentTextIndex));
											currentTextIndex = text.IndexOf(solutionContext.ProtectedAreaStart, currentTextIndex);

											if (originalText.IndexOf(solutionContext.ProtectedAreaStart, currentOriginalTextIndex) >= 0 && originalText.IndexOf(solutionContext.ProtectedAreaEnd, originalText.IndexOf(solutionContext.ProtectedAreaStart, currentOriginalTextIndex)) >= 0)
											{
												hasProtectedText = true;
												currentOriginalTextIndex = originalText.IndexOf(solutionContext.ProtectedAreaStart, currentOriginalTextIndex);
												if (String.IsNullOrWhiteSpace(originalText.Substring(currentOriginalTextIndex + solutionContext.ProtectedAreaStart.Length, originalText.IndexOf(solutionContext.ProtectedAreaEnd, currentOriginalTextIndex) - currentOriginalTextIndex - solutionContext.ProtectedAreaStart.Length)))
												{
													hasProtectedText = false;
												}
												if (hasProtectedText == true)
												{
													// append protected block in original text
													updatedText.Append(originalText.Substring(currentOriginalTextIndex, originalText.IndexOf(solutionContext.ProtectedAreaEnd, currentOriginalTextIndex) - currentOriginalTextIndex + solutionContext.ProtectedAreaEnd.Length));
												}
												else
												{
													// append protected block in generated text
													updatedText.Append(text.Substring(currentTextIndex, text.IndexOf(solutionContext.ProtectedAreaEnd, currentTextIndex) - currentTextIndex + solutionContext.ProtectedAreaEnd.Length));
												}
												currentOriginalTextIndex = originalText.IndexOf(solutionContext.ProtectedAreaEnd, currentOriginalTextIndex) + solutionContext.ProtectedAreaEnd.Length;

												// skip protected block in text
												currentTextIndex = text.IndexOf(solutionContext.ProtectedAreaEnd, currentTextIndex) + solutionContext.ProtectedAreaEnd.Length;
											}
											else
											{
												// append remainder of text to updated text
												updatedText.Append(text.Substring(currentTextIndex));
												currentTextIndex = text.Length - 1;
											}
										}
										else
										{
											// append remainder of text to updated text
											updatedText.Append(text.Substring(currentTextIndex));
											currentTextIndex = text.Length - 1;

											// append remainder of protected blocks in original text (if any, user will have to move/replace these blocks)
											while (originalText.IndexOf(solutionContext.ProtectedAreaStart, currentOriginalTextIndex) >= 0 && originalText.IndexOf(solutionContext.ProtectedAreaEnd, originalText.IndexOf(solutionContext.ProtectedAreaStart, currentOriginalTextIndex)) >= 0)
											{
												hasProtectedText = true;
												currentOriginalTextIndex = originalText.IndexOf(solutionContext.ProtectedAreaStart, currentOriginalTextIndex);
												if (String.IsNullOrWhiteSpace(originalText.Substring(currentOriginalTextIndex + solutionContext.ProtectedAreaStart.Length, originalText.IndexOf(solutionContext.ProtectedAreaEnd, currentOriginalTextIndex) - currentOriginalTextIndex - solutionContext.ProtectedAreaStart.Length)))
												{
													hasProtectedText = false;
												}
												if (hasProtectedText == true)
												{
													updatedText.Append(originalText.Substring(currentOriginalTextIndex, originalText.IndexOf(solutionContext.ProtectedAreaEnd, currentOriginalTextIndex) - currentOriginalTextIndex + solutionContext.ProtectedAreaEnd.Length));
												}
												currentOriginalTextIndex = originalText.IndexOf(solutionContext.ProtectedAreaEnd, currentOriginalTextIndex) + solutionContext.ProtectedAreaEnd.Length;
											}
										}
									}
									FileHelper.ReplaceFile(path, updatedText.ToString());
								}
								else
								{
									// replace file if protected tags not found
									FileHelper.ReplaceFile(path, text);
								}
							}
							else
							{
								// replace file wholesale
								FileHelper.ReplaceFile(path, text);
							}
						}
						else
						{
							string directory = Directory.GetParent(path).FullName;
							if (!Directory.Exists(directory))
							{
								Directory.CreateDirectory(directory);
							}
							FileHelper.CreateFile(path, text);
						}
					}
				}
			}
			catch (ApplicationAbortException)
			{
				throw;
			}
			catch (System.Exception ex)
			{
				LogException(solutionContext, templateContext, modelContext, ex, interpreterType);
			}
		}
	}
}
