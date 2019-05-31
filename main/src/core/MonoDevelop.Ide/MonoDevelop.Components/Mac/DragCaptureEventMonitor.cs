//
// CocoaLocalEventMonitor.cs
//
// Author:
//       jmedrano <josmed@microsoft.com>
//
// Copyright (c) 2019 
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
#if MAC

using System;

using AppKit;
using Foundation;
using Gdk;

namespace MonoDevelop.Components.Mac
{
	interface IDragCapturedView
	{
		void MouseDragUp (NSEvent theEvent);
		void MouseDrag (NSEvent theEvent);
	}

	sealed class DragCaptureEventMonitor : IDisposable
	{
		IDragCapturedView mouseDragView;
		NSObject localEventMonitor;

		void AddRemoveFilter (bool enable)
		{
			if (enable)
				Gdk.Window.AddFilterForAll (Filter);
			else
				Gdk.Window.RemoveFilterForAll (Filter);
		}
		static FilterReturn Filter (IntPtr xevent, Event evnt) => FilterReturn.Remove;

		public void Start (IDragCapturedView view)
		{
			Stop ();
			localEventMonitor = NSEvent.AddLocalMonitorForEventsMatchingMask (NSEventMask.LeftMouseUp | NSEventMask.LeftMouseDragged, HandleLocalEvent);

			if (mouseDragView == null) {
				mouseDragView = view;
				AddRemoveFilter (true);
			}
		}

		public void Stop ()
		{
			if (localEventMonitor != null) {
				NSEvent.RemoveMonitor (localEventMonitor);
				localEventMonitor.Dispose ();
				localEventMonitor = null;
			}

			if (mouseDragView != null) {
				mouseDragView = null;
				AddRemoveFilter (false);
			}
		}

		NSEvent HandleLocalEvent (NSEvent theEvent)
		{
			if (mouseDragView == null)
				return theEvent;

			switch (theEvent.Type) {
			case NSEventType.LeftMouseUp:
				mouseDragView.MouseDragUp (theEvent);
				return null;
		
			case NSEventType.LeftMouseDragged:
				mouseDragView.MouseDrag (theEvent);
				return null;
			}

			return theEvent;
		}

		public void Dispose ()
		{
			Stop ();
		}
	}
}

#endif
