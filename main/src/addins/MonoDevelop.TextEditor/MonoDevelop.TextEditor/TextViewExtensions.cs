//
// Copyright (c) Microsoft Corp. (https://www.microsoft.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using MonoDevelop.Ide.Gui.Documents;

namespace MonoDevelop.TextEditor
{
	public static class TextViewExtensions
	{
		/// <summary>
		/// Gets the parent <see cref="Ide.Gui.Document"/> from an <see cref="ITextView"/>
		/// </summary>
		public static Ide.Gui.Document TryGetParentDocument (this ITextView view)
		{
			if (view.Properties.TryGetProperty (typeof (DocumentController), out DocumentController documentController)) {
				return documentController.Document;
			}
			return null;
		}

		public static string GetFilePathOrNull (this ITextBuffer textBuffer)
		{
			if (textBuffer.Properties.TryGetProperty (typeof (ITextDocument), out ITextDocument textDocument)) {
				return textDocument.FilePath;
			}

			return null;
		}
	}
}
