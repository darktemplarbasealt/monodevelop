using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Mono.Debugging.Client;
using MonoDevelop.Core;
using System.Linq;

namespace MonoDevelop.Debugger
{
	class BreakpointManager
	{
		private ITextBuffer textBuffer;
		private readonly ITextDocument textDocument;

		public BreakpointManager (ITextBuffer textBuffer)
		{
			this.textBuffer = textBuffer;
			if (textBuffer.Properties.TryGetProperty (typeof (ITextDocument), out textDocument) && textDocument.FilePath != null) {
				textDocument.FileActionOccurred += TextDocument_FileActionOccurred;
			} else {
				LoggingService.LogWarning ("Failed to get filename of textbuffer, breakpoints integration will not work.");
				return;
			}
			textBuffer.Changed += TextBuffer_Changed;
			DebuggingService.Breakpoints.Changed += OnBreakpointsChanged;
			DebuggingService.Breakpoints.BreakpointStatusChanged += OnBreakpointsChanged;
			DebuggingService.Breakpoints.BreakpointModified += OnBreakpointsChanged;
			OnBreakpointsChanged (null, null);
		}

		private void TextDocument_FileActionOccurred (object sender, TextDocumentFileActionEventArgs e)
		{
			if (e.FileActionType == FileActionTypes.DocumentRenamed)
				OnBreakpointsChanged (null, null);
		}

		void TextBuffer_Changed (object sender, TextContentChangedEventArgs e)
		{
			foreach (var breakpoint in breakpoints.Values) {
				var newSpan = breakpoint.TrackingSpan.GetSpan (e.After);
				if (newSpan.IsEmpty) {
					DebuggingService.Breakpoints.Remove (breakpoint.Breakpoint);
					continue;
				}
				var newLineNumber = e.After.GetLineFromPosition (newSpan.Start).LineNumber + 1;
				if (breakpoint.Breakpoint.Line != newLineNumber) {
					DebuggingService.Breakpoints.UpdateBreakpointLine (breakpoint.Breakpoint, newLineNumber);
				}
			}
		}

		class ManagerBreakpoint
		{
			public Breakpoint Breakpoint { get; set; }
			public ITrackingSpan TrackingSpan { get; set; }
			public Span Span { get; set; }
		}

		private Dictionary<Breakpoint, ManagerBreakpoint> breakpoints = new Dictionary<Breakpoint, ManagerBreakpoint> ();

		private void OnBreakpointsChanged (object sender, EventArgs eventArgs)
		{
			var snapshot = textBuffer.CurrentSnapshot;
			var newBreakpoints = new Dictionary<Breakpoint, ManagerBreakpoint> ();
			bool needsUpdate = false;
			foreach (var breakpoint in DebuggingService.Breakpoints.GetBreakpointsAtFile (textDocument.FilePath)) {
				if (breakpoint.Line > snapshot.LineCount)
					continue;
				if (eventArgs is BreakpointEventArgs breakpointEventArgs && breakpointEventArgs.Breakpoint == breakpoint)
					needsUpdate = true;
				var newSpan = snapshot.GetLineFromLineNumber (breakpoint.Line - 1).Extent;
				if (breakpoints.TryGetValue (breakpoint, out var existingBreakpoint)) {
					newBreakpoints.Add (breakpoint, existingBreakpoint);
					if (existingBreakpoint.Span != newSpan.Span) {
						// Update if anything was modifed
						needsUpdate = true;
						existingBreakpoint.Span = newSpan.Span;
					}
				} else {
					// Update if anything was added
					needsUpdate = true;
					newBreakpoints.Add (breakpoint, new ManagerBreakpoint () {
						Breakpoint = breakpoint,
						TrackingSpan = snapshot.CreateTrackingSpan (newSpan, SpanTrackingMode.EdgeExclusive),
						Span = newSpan.Span
					});
				}
			}
			// Update if anything was removed
			if (needsUpdate || breakpoints.Keys.Except (newBreakpoints.Keys).Any ())
				needsUpdate = true;
			breakpoints = newBreakpoints;
			if (needsUpdate)
				BreakpointsChanged?.Invoke (this, new SnapshotSpanEventArgs (new SnapshotSpan (snapshot, 0, snapshot.Length)));
		}
		public event EventHandler<SnapshotSpanEventArgs> BreakpointsChanged;

		public void Dispose ()
		{
			BreakpointsChanged = null;
			textBuffer.Changed -= TextBuffer_Changed;
			DebuggingService.Breakpoints.Changed -= OnBreakpointsChanged;
			DebuggingService.Breakpoints.BreakpointStatusChanged -= OnBreakpointsChanged;
			DebuggingService.Breakpoints.BreakpointModified -= OnBreakpointsChanged;
			if (textDocument != null)
				textDocument.FileActionOccurred -= TextDocument_FileActionOccurred;
		}

		public IEnumerable<BreakpointSpan> GetBreakpoints (ITextSnapshot snapshot)
		{
			foreach (var item in breakpoints.Values) {
				yield return new BreakpointSpan (item.Breakpoint, item.TrackingSpan.GetSpan (snapshot));
			}
		}
	}

	class BreakpointSpan
	{
		public Breakpoint Breakpoint { get; }
		public SnapshotSpan Span { get; }

		public BreakpointSpan (Breakpoint breakpoint, SnapshotSpan span)
		{
			Breakpoint = breakpoint;
			Span = span;
		}
	}
}
