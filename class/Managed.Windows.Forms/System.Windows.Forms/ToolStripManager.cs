//
// ToolStripManager.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections.Generic;

namespace System.Windows.Forms
{
	public sealed class ToolStripManager
	{
		private static ToolStripRenderer renderer;
		private static ToolStripManagerRenderMode render_mode;
		private static bool visual_styles_enabled;
		private static List<ToolStrip> toolstrips;
		private static List<ToolStripMenuItem> menu_items;
		
		#region Static Constructor
		static ToolStripManager ()
		{
			toolstrips = new List<ToolStrip> ();
			menu_items = new List<ToolStripMenuItem> ();
			ToolStripManager.renderer = new ToolStripProfessionalRenderer ();
			ToolStripManager.render_mode = ToolStripManagerRenderMode.Professional;
			ToolStripManager.visual_styles_enabled = Application.RenderWithVisualStyles;
		}

		private ToolStripManager ()
		{
		}
		#endregion

		#region Public Properties
		public static ToolStripRenderer Renderer {
			get { return ToolStripManager.renderer; }
			set {
				if (ToolStripManager.Renderer != value) {
					ToolStripManager.renderer = value;
					ToolStripManager.OnRendererChanged (EventArgs.Empty);
				}
			}
		}

		public static ToolStripManagerRenderMode RenderMode {
			get { return ToolStripManager.render_mode; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripManagerRenderMode), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripManagerRenderMode", value));

				if (ToolStripManager.render_mode != value) {
					ToolStripManager.render_mode = value;

					switch (value) {
						case ToolStripManagerRenderMode.Custom:
							throw new NotSupportedException ();
						case ToolStripManagerRenderMode.System:
							ToolStripManager.Renderer = new ToolStripProfessionalRenderer ();
							break;
						case ToolStripManagerRenderMode.Professional:
							ToolStripManager.Renderer = new ToolStripProfessionalRenderer ();
							break;
					}
				}
			}
		}

		public static bool VisualStylesEnabled {
			get { return ToolStripManager.visual_styles_enabled; }
			set {
				if (ToolStripManager.visual_styles_enabled != value) {
					ToolStripManager.visual_styles_enabled = value;

					if (ToolStripManager.render_mode == ToolStripManagerRenderMode.Professional) {
						(ToolStripManager.renderer as ToolStripProfessionalRenderer).ColorTable.UseSystemColors = !value;
						ToolStripManager.OnRendererChanged (EventArgs.Empty);
					}
				}
			}
		}
		#endregion

		#region Public Methods
		public static ToolStrip FindToolStrip (string toolStripName)
		{
			lock (toolstrips)
				foreach (ToolStrip ts in toolstrips)
					if (ts.Name == toolStripName)
						return ts;
						
			return null;
		}
		
		public static bool IsShortcutDefined (Keys shortcut)
		{
			lock (menu_items)
				foreach (ToolStripMenuItem tsmi in menu_items)
					if (tsmi.ShortcutKeys == shortcut)
						return true;

			return false;
		}
		
		public static bool IsValidShortcut (Keys shortcut)
		{
			// Anything with an F1 - F12 is a shortcut
			if ((shortcut & Keys.F1) == Keys.F1)
				return true;
			else if ((shortcut & Keys.F2) == Keys.F2)
				return true;
			else if ((shortcut & Keys.F3) == Keys.F3)
				return true;
			else if ((shortcut & Keys.F4) == Keys.F4)
				return true;
			else if ((shortcut & Keys.F5) == Keys.F5)
				return true;
			else if ((shortcut & Keys.F6) == Keys.F6)
				return true;
			else if ((shortcut & Keys.F7) == Keys.F7)
				return true;
			else if ((shortcut & Keys.F8) == Keys.F8)
				return true;
			else if ((shortcut & Keys.F9) == Keys.F9)
				return true;
			else if ((shortcut & Keys.F10) == Keys.F10)
				return true;
			else if ((shortcut & Keys.F11) == Keys.F11)
				return true;
			else if ((shortcut & Keys.F12) == Keys.F12)
				return true;
				
			// Modifier keys alone are not shortcuts
			switch (shortcut) {
				case Keys.Alt:
				case Keys.Control:
				case Keys.Shift:
				case Keys.Alt | Keys.Control:
				case Keys.Alt | Keys.Shift:
				case Keys.Control | Keys.Shift:
				case Keys.Alt | Keys.Control | Keys.Shift:
					return false;
			}
	
			// Anything else with a modifier key is a shortcut
			if ((shortcut & Keys.Alt) == Keys.Alt)
				return true;
			else if ((shortcut & Keys.Control) == Keys.Control)
				return true;
			else if ((shortcut & Keys.Shift) == Keys.Shift)
				return true;

			// Anything else is not a shortcut
			return false;
		}

		[MonoTODO ("Only supports one level of merging, cannot merge the same ToolStrip multiple times")]
		public static bool Merge (ToolStrip sourceToolStrip, string targetName)
		{
			if (string.IsNullOrEmpty (targetName))
				throw new ArgumentNullException ("targetName");
				
			return Merge (sourceToolStrip, FindToolStrip (targetName));
		}
		
		[MonoTODO ("Only supports one level of merging, cannot merge the same ToolStrip multiple times")]
		public static bool Merge (ToolStrip sourceToolStrip, ToolStrip targetToolStrip)
		{
			// Check for exceptions
			if (sourceToolStrip == null)
				throw new ArgumentNullException ("sourceToolStrip");
				
			if (targetToolStrip == null)
				throw new ArgumentNullException ("targetName");
				
			if (targetToolStrip == sourceToolStrip)
				throw new ArgumentException ("Source and target ToolStrip must be different.");
			
			// If the toolstrips don't allow merging, don't merge them
			if (!sourceToolStrip.AllowMerge || !targetToolStrip.AllowMerge)
				return false;
			
			// We currently can't support merging multiple times
			if (sourceToolStrip.IsCurrentlyMerged || targetToolStrip.IsCurrentlyMerged)
				return false;
				
			// What I wouldn't give to be able to modify a collection
			// while enumerating through it...

			List<ToolStripItem> items_to_move = new List<ToolStripItem> ();
			
			// Create a list of every ToolStripItem we plan on moving
			foreach (ToolStripItem tsi in sourceToolStrip.Items) {
				switch (tsi.MergeAction) {
					case MergeAction.Append:
					default:
						items_to_move.Add (tsi);
						break;
					case MergeAction.Insert:
						if (tsi.MergeIndex >= 0)
							items_to_move.Add (tsi);
						break;
					case MergeAction.Replace:
					case MergeAction.Remove:
					case MergeAction.MatchOnly:
						foreach (ToolStripItem target_tsi in targetToolStrip.Items)
							if (tsi.Text == target_tsi.Text) {
								items_to_move.Add (tsi);
								break;
							}
						break;
				}
			}

			// If there was nothing valid to merge, return false
			if (items_to_move.Count == 0)
				return false;
				
			// Set some state so we can unmerge later
			sourceToolStrip.BeginMerge ();
			targetToolStrip.BeginMerge ();			

			sourceToolStrip.SuspendLayout ();
			targetToolStrip.SuspendLayout ();
	
			while (items_to_move.Count > 0) {
				ToolStripItem tsi = items_to_move[0];
				items_to_move.Remove (tsi);
				
				switch (tsi.MergeAction) {
					case MergeAction.Append:
					default:
						// Just changing the parent will append it to the target
						// and remove it from the source
						ToolStrip.SetItemParent (tsi, targetToolStrip);
						
						break;
					case MergeAction.Insert:
						// Do the same work as Append, except Insert it into the
						// location specified by the MergeIndex
						RemoveItemFromParentToolStrip (tsi);

						if (tsi.MergeIndex == -1)
							continue;
						else if (tsi.MergeIndex >= targetToolStrip.Items.Count)
							targetToolStrip.Items.AddNoOwnerOrLayout (tsi);						
						else
							targetToolStrip.Items.InsertNoOwnerOrLayout (tsi.MergeIndex, tsi);

						tsi.Parent = targetToolStrip;
						
						break;
					case MergeAction.Replace:
						// Find a target ToolStripItem with the same Text, remove it
						// and replace it with the source one
						foreach (ToolStripItem target_tsi in targetToolStrip.Items)
							if (tsi.Text == target_tsi.Text) {
								RemoveItemFromParentToolStrip (tsi);

								// Insert where the old one is, then remove the old one
								targetToolStrip.Items.InsertNoOwnerOrLayout (targetToolStrip.Items.IndexOf (target_tsi), tsi);
								targetToolStrip.Items.RemoveNoOwnerOrLayout (target_tsi);

								// Store the replaced one so we can get it back in unmerge
								targetToolStrip.HiddenMergedItems.Add (target_tsi);
								break;
							}

						break;
					case MergeAction.Remove:
						// Find a target ToolStripItem with the same Text, and remove
						// it from the target, nothing else
						foreach (ToolStripItem target_tsi in targetToolStrip.Items)
							if (tsi.Text == target_tsi.Text) {
								targetToolStrip.Items.RemoveNoOwnerOrLayout (target_tsi);
								
								// Store the removed one so we can get it back in unmerge
								targetToolStrip.HiddenMergedItems.Add (target_tsi);
								break;
							}

						break;
					case MergeAction.MatchOnly:
						// Ugh, find the target ToolStripItem with the same Text, and take
						// all the subitems from the source one, and append it to the target one
						foreach (ToolStripItem target_tsi in targetToolStrip.Items)
							if (tsi.Text == target_tsi.Text) {
								if (target_tsi is ToolStripMenuItem && tsi is ToolStripMenuItem) {
									ToolStripMenuItem source = (ToolStripMenuItem)tsi;
									ToolStripMenuItem target = (ToolStripMenuItem)target_tsi;
									
									while (source.DropDownItems.Count > 0)
										ToolStrip.SetItemParent (source.DropDownItems[0], target.DropDown);
								}
								
								break;
							}
						
						break;
				}
			}
			
			sourceToolStrip.ResumeLayout ();
			targetToolStrip.ResumeLayout ();
			
			// Store who we merged with, so we can unmerge when only given the target toolstrip
			sourceToolStrip.CurrentlyMergedWith = targetToolStrip;
			targetToolStrip.CurrentlyMergedWith = sourceToolStrip;
			
			return true;
		}

		public static bool RevertMerge (string targetName)
		{
			return RevertMerge (FindToolStrip (targetName));
		}
		
		public static bool RevertMerge (ToolStrip targetToolStrip)
		{
			return RevertMerge (targetToolStrip, targetToolStrip.CurrentlyMergedWith);			
		}
		
		public static bool RevertMerge (ToolStrip targetToolStrip, ToolStrip sourceToolStrip)
		{
			if (sourceToolStrip == null)
				throw new ArgumentNullException ("sourceToolStrip");
				
			List<ToolStripItem> items_to_move = new List<ToolStripItem> ();
			
			// Find every ToolStripItem who's Owner is the source toolstrip
			// - If it's a TSMI, see if any of the subitems need to be moved back
			foreach (ToolStripItem tsi in targetToolStrip.Items) {
				if (tsi.Owner == sourceToolStrip)
					items_to_move.Add (tsi);
				else if (tsi is ToolStripMenuItem)
					foreach (ToolStripItem menuitem in (tsi as ToolStripMenuItem).DropDownItems)
						foreach (ToolStripMenuItem tsmi in sourceToolStrip.Items)
							if (menuitem.Owner == tsmi.DropDown)
								items_to_move.Add (menuitem);	
			}

			// If we didn't find anything, return false
			if (items_to_move.Count == 0 && targetToolStrip.HiddenMergedItems.Count == 0)
				return false;

			// Put back all the target's items removed in the merge
			while (targetToolStrip.HiddenMergedItems.Count > 0) {
				targetToolStrip.RevertMergeItem (targetToolStrip.HiddenMergedItems[0]);
				targetToolStrip.HiddenMergedItems.RemoveAt (0);
			}
				
			sourceToolStrip.SuspendLayout ();
			targetToolStrip.SuspendLayout ();
			
			// Revert everything
			while (items_to_move.Count > 0) {
				sourceToolStrip.RevertMergeItem (items_to_move[0]);
				items_to_move.Remove (items_to_move[0]);
			}

			sourceToolStrip.ResumeLayout ();
			targetToolStrip.ResumeLayout ();
			
			sourceToolStrip.IsCurrentlyMerged = false;
			targetToolStrip.IsCurrentlyMerged = false;

			sourceToolStrip.CurrentlyMergedWith = null;
			targetToolStrip.CurrentlyMergedWith = null;

			return true;
		}
		#endregion
		
		#region Public Events
		public static event EventHandler RendererChanged;
		#endregion

		#region Private/Internal Methods
		internal static void AddToolStrip (ToolStrip ts)
		{
			lock (toolstrips)
				toolstrips.Add (ts);
		}

		internal static bool ProcessCmdKey (ref Message m, Keys keyData)
		{
			lock (menu_items)
				foreach (ToolStripMenuItem tsmi in menu_items)
					if (tsmi.ProcessCmdKey (ref m, keyData) == true)
						return true;
			
			return false;
		}
		
		internal static void AddToolStripMenuItem (ToolStripMenuItem tsmi)
		{
			lock (menu_items)
				menu_items.Add (tsmi);
		}
		
		internal static void RemoveToolStrip (ToolStrip ts)
		{
			lock (toolstrips)
				toolstrips.Remove (ts);
		}

		internal static void RemoveToolStripMenuItem (ToolStripMenuItem tsmi)
		{
			lock (menu_items)
				menu_items.Remove (tsmi);
		}

		internal static void FireAppClicked ()
		{
			if (AppClicked != null) AppClicked (null, EventArgs.Empty);
		}

		internal static void FireAppFocusChanged (Form form)
		{
			if (AppFocusChange != null) AppFocusChange (form, EventArgs.Empty);
		}

		internal static void FireAppFocusChanged (object sender)
		{
			if (AppFocusChange != null) AppFocusChange (sender, EventArgs.Empty);
		}
		
		private static void OnRendererChanged (EventArgs e)
		{
			if (RendererChanged != null) RendererChanged (null, e);
		}

		private static void RemoveItemFromParentToolStrip (ToolStripItem tsi)
		{
			if (tsi.Parent != null) {
				tsi.Parent.Items.RemoveNoOwnerOrLayout (tsi);

				if (tsi.Parent is ToolStripOverflow)
					(tsi.Parent as ToolStripOverflow).ParentToolStrip.Items.RemoveNoOwnerOrLayout (tsi);
			}
		}
		
		internal static event EventHandler AppClicked;
		internal static event EventHandler AppFocusChange;
		#endregion
	}
}
#endif
