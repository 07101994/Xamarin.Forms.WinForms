﻿using System;
using System.ComponentModel;
using WDrawing = System.Drawing;
using WForms = System.Windows.Forms;

namespace Xamarin.Forms.Platform.WinForms
{
	public class WFormsCarouselPage : WForms.UserControl, INativeElement
	{
		WForms.Button _btnBack;
		WForms.Button _btnForward;
		WForms.Panel _content;

		int _selectedIndex = -1;

		public WFormsCarouselPage()
		{
			var size = ClientSize;
			int fw = (Font?.Height).GetValueOrDefault(1) * 2 + 4;

			_content = new WForms.Panel()
			{
				Parent = this,
				Left = fw,
				Top = 0,
				Width = size.Width - fw * 2,
				Height = size.Height,
				Anchor =
					WForms.AnchorStyles.Left |
					WForms.AnchorStyles.Right |
					WForms.AnchorStyles.Top |
					WForms.AnchorStyles.Bottom
			};

			_btnBack = new WForms.Button()
			{
				Parent = this,
				Left = 0,
				Top = 0,
				Width = fw,
				Height = size.Height,
				Text = "<",
				Anchor =
					WForms.AnchorStyles.Left |
					WForms.AnchorStyles.Top |
					WForms.AnchorStyles.Bottom
			};

			_btnForward = new WForms.Button()
			{
				Parent = this,
				Left = size.Width - fw,
				Top = 0,
				Width = fw,
				Height = size.Height,
				Text = ">",
				Anchor =
					WForms.AnchorStyles.Right |
					WForms.AnchorStyles.Top |
					WForms.AnchorStyles.Bottom
			};

			_btnBack.Click += OnBackButtonClicked;
			_btnForward.Click += OnForwardButtonClicked;
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_btnBack?.Dispose();
				_btnBack = null;
				_btnForward?.Dispose();
				_btnForward = null;
				_content?.Dispose();
				_content = null;
			}
			base.Dispose(disposing);
		}


		public WForms.Control ParentForChildren => _content;

		public ControlCollection Children => _content?.Controls;

		public WForms.Panel Content => _content;

		public int SelectedIndex
		{
			get => _selectedIndex;
			set
			{
				var children = Children;
				if (children != null)
				{
					int count = children.Count;
					int newIndex = count < 0 ? -1 : Math.Max(0, Math.Min(value, count - 1));
					if (newIndex != _selectedIndex)
					{
						for (int i = 0; i < count; i++)
						{
							children[i].Visible = i == newIndex;
						}
						_selectedIndex = newIndex;
					}
				}
			}
		}

		protected override void OnControlAdded(WForms.ControlEventArgs e)
		{
			base.OnControlAdded(e);
			UpdateContentChildren();
		}

		protected override void OnControlRemoved(WForms.ControlEventArgs e)
		{
			base.OnControlAdded(e);
			UpdateContentChildren();
		}

		void OnBackButtonClicked(object sender, EventArgs e)
		{
			SelectedIndex--;
		}

		void OnForwardButtonClicked(object sender, EventArgs e)
		{
			SelectedIndex++;
		}

		void UpdateContentChildren()
		{
			var children = Children;
			if (children != null)
			{
				if (children.Count < 0)
				{
					SelectedIndex = -1;
				}
				else if (_selectedIndex < 0)
				{
					SelectedIndex = 0;
				}
				else
				{
					SelectedIndex = _selectedIndex;
				}
			}
		}
	}
}
