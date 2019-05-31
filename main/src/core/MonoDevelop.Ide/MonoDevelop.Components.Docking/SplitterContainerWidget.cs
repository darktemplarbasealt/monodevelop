//
//
// Author:
//   Jose Medrano
//

//
// Copyright (C) 2019 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using AppKit;
using Gdk;

namespace MonoDevelop.Components.Docking
{
	class SplitterContainerWidget : NativeContainerWidget, ISplitterWidget
	{
		Mac.DragCaptureEventMonitor monitor;

		public SplitterContainerWidget (IntPtr raw) : base (raw)
		{
		}

		protected SplitterContainerWidget ()
		{
		}

		protected SplitterContainerWidget (GLib.GType gtype) : base (gtype)
		{
		}

		public void Init (DockGroup grp, int index)
		{
			splitter?.Init (grp, index);
		}

		public void SetSize (Rectangle rect)
		{
			OnSizeAllocated (rect);
		}

		MacSplitterWidget splitter;

		protected override void OnSetNativeContent (NSView widget)
		{
			monitor = new Mac.DragCaptureEventMonitor ();

			if (widget is MacSplitterWidget view) {
				this.splitter = view;
				this.splitter.DragStarted += Widget_DragStarted;
				this.splitter.DragEnd += Widget_DragEnd;
			} 
		}

		private void Widget_DragEnd (object sender, EventArgs e)
		{
			monitor?.Stop ();
		}

		private void Widget_DragStarted (object sender, EventArgs e)
		{
			if (this.splitter != null && monitor != null) {
				monitor.Start (this.splitter);
			}
		}

		bool disposed;
		public override void Dispose ()
		{
			if (!disposed) {
				disposed = true;
				if (this.splitter != null) {
					this.splitter.DragStarted -= Widget_DragStarted;
					this.splitter.DragEnd -= Widget_DragEnd;
					splitter = null;
				}
				if (monitor != null) {
					monitor.Dispose ();
					monitor = null;
				}
			}
			base.Dispose ();
		}
	}
}
